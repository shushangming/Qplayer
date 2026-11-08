using System;
using System.Collections.Generic;
using System.IO;

namespace KugouAvaloniaPlayer.Services;

public static class AppStoragePaths
{
    public const string StorageRootEnvironmentVariable = "KUGOU_STORAGE_ROOT";

    private const string WindowsDefaultRoot = @"D:\QQ_music_api";

    // 热数据（数据库/日志/设置/会话）必须落在 C 盘 SSD 系统信任区：
    // 落在 D:\QQ_music_api 时（目录含大量 exe/dll/zip，会被实时扫描/IO 更慢），
    // 高频小写入会拖住 UI 线程，表现为切换/hover 动画整体掉帧。
    // 冷数据（图片缓存/下载）仍保留在项目目录 D:\QQ_music_api 下。
    private static readonly string HotDataRoot =
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) is { Length: > 0 } localAppData
            ? Path.Combine(localAppData, "QCMusic")
            : WindowsDefaultRoot;

    private static readonly object MigrationLock = new();
    private static readonly string[] MigratedDataFiles =
    [
        "AvaloniaPlayerSettings.json",
        "session.json",
        "qq_cookie.txt"
    ];

    // 该方法被 AppDbContext 在每次打开连接时调用；迁移只需真正执行一次。
    // 没有一次性标志时每次开连接都做锁+目录枚举+文件检查，高频开关连接
    // 会造成持续 IO 与锁竞争，拖慢 UI（表现为侧栏切换/hover 动画掉帧）。
    private static bool _migrationCompleted;

    public static string Root { get; } = ResolveStorageRoot();
    public static string DataDirectory { get; } = EnsureDirectory(Path.Combine(HotDataRoot, "data"));
    public static string LogsDirectory { get; } = EnsureDirectory(Path.Combine(HotDataRoot, "logs"));
    // 冷数据：图片缓存/下载仍放项目目录（用户要求），低频大文件不受扫描影响。
    public static string CacheDirectory { get; } = EnsureDirectory(Path.Combine(Root, "cache"));
    public static string DownloadsDirectory { get; } = EnsureDirectory(Path.Combine(Root, "downloads"));
    public static string DatabasePath { get; } = Path.Combine(DataDirectory, "kugou-player.db");
    public static string SettingsPath { get; } = Path.Combine(DataDirectory, "AvaloniaPlayerSettings.json");
    public static string? LegacyKugouDirectory { get; } = ResolveLegacyKugouDirectory();

    static AppStoragePaths()
    {
        Environment.SetEnvironmentVariable(StorageRootEnvironmentVariable, Root);
        MigrateLegacyDataIfNeeded();
    }

    public static string GetDataFilePath(string fileName) => Path.Combine(DataDirectory, fileName);

    public static string GetCacheDirectory(params string[] segments)
    {
        var path = CacheDirectory;
        foreach (var segment in segments)
        {
            if (!string.IsNullOrWhiteSpace(segment))
                path = Path.Combine(path, segment);
        }

        return EnsureDirectory(path);
    }

    public static string GetLogDirectory(params string[] segments)
    {
        var path = LogsDirectory;
        foreach (var segment in segments)
        {
            if (!string.IsNullOrWhiteSpace(segment))
                path = Path.Combine(path, segment);
        }

        return EnsureDirectory(path);
    }

    public static string? GetLegacyKugouFilePath(string fileName)
    {
        var legacyDirectory = LegacyKugouDirectory;
        return string.IsNullOrWhiteSpace(legacyDirectory)
            ? null
            : Path.Combine(legacyDirectory, fileName);
    }

    private static string ResolveStorageRoot()
    {
        var configuredRoot = Environment.GetEnvironmentVariable(StorageRootEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            var configuredPath = TryPrepareDirectory(configuredRoot.Trim());
            if (configuredPath is not null)
                return configuredPath;
        }

        if (OperatingSystem.IsWindows())
        {
            var preferredPath = TryPrepareDirectory(WindowsDefaultRoot);
            if (preferredPath is not null)
                return preferredPath;
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(localAppData))
        {
            var fallbackPath = TryPrepareDirectory(Path.Combine(localAppData, "QCMusic"));
            if (fallbackPath is not null)
                return fallbackPath;
        }

        return EnsureDirectory(Path.Combine(Path.GetTempPath(), "QCMusic"));
    }

    private static string? ResolveLegacyKugouDirectory()
    {
        var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return string.IsNullOrWhiteSpace(applicationData)
            ? null
            : Path.Combine(applicationData, "kugou");
    }

    private static string EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }

    private static string? TryPrepareDirectory(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
            Directory.CreateDirectory(fullPath);

            var probePath = Path.Combine(fullPath, $".write-test-{Environment.ProcessId}-{Guid.NewGuid():N}");
            using (File.Create(probePath, 1, FileOptions.DeleteOnClose))
            {
            }

            return fullPath;
        }
        catch
        {
            return null;
        }
    }

    public static void MigrateLegacyDataIfNeeded()
    {
        if (_migrationCompleted)
            return;

        lock (MigrationLock)
        {
            if (_migrationCompleted)
                return;

            var legacyDirectory = LegacyKugouDirectory;
            if (string.IsNullOrWhiteSpace(legacyDirectory) ||
                !Directory.Exists(legacyDirectory) ||
                PathsEqual(legacyDirectory, DataDirectory))
            {
                _migrationCompleted = true;
                return;
            }

            MigrateDatabase(legacyDirectory);

            foreach (var fileName in MigratedDataFiles)
                CopyFileIfMissing(Path.Combine(legacyDirectory, fileName), GetDataFilePath(fileName));

            CopyMatchingFiles(legacyDirectory, "favorite_like_cache*.json");
            CopyMatchingFiles(legacyDirectory, "playback_history_*.json");

            _migrationCompleted = true;
        }
    }

    private static void MigrateDatabase(string legacyDirectory)
    {
        var legacyDatabase = Path.Combine(legacyDirectory, "kugou-player.db");
        if (!File.Exists(legacyDatabase) || File.Exists(DatabasePath))
            return;

        if (!CopyFileAtomically(legacyDatabase, DatabasePath))
            return;

        foreach (var suffix in new[] { "-wal", "-shm" })
            CopyFileIfMissing(legacyDatabase + suffix, DatabasePath + suffix);
    }

    private static void CopyMatchingFiles(string legacyDirectory, string pattern)
    {
        try
        {
            foreach (var sourcePath in Directory.EnumerateFiles(legacyDirectory, pattern, SearchOption.TopDirectoryOnly))
                CopyFileIfMissing(sourcePath, GetDataFilePath(Path.GetFileName(sourcePath)));
        }
        catch
        {
            // Migration is best effort. The database remains the primary store.
        }
    }

    private static void CopyFileIfMissing(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath) || File.Exists(destinationPath))
            return;

        CopyFileAtomically(sourcePath, destinationPath);
    }

    private static bool CopyFileAtomically(string sourcePath, string destinationPath)
    {
        var temporaryPath = destinationPath + $".migrating-{Guid.NewGuid():N}";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, temporaryPath, overwrite: false);
            File.Move(temporaryPath, destinationPath, overwrite: false);
            return true;
        }
        catch
        {
            TryDelete(temporaryPath);
            return false;
        }
    }

    private static bool PathsEqual(string left, string right)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Best effort cleanup.
        }
    }
}
