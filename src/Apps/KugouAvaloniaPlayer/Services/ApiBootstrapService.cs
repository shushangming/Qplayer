using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KugouAvaloniaPlayer.Services;

/// <summary>
/// 内嵌 qq-music-api 服务（Node）的生命周期管理：
/// - 客户端启动时自动探测 localhost:3200；未运行则用随包分发的 node.exe 拉起 dist/app.js；
/// - 登录成功后把 QQ cookie 同步写入 API 的 config/user-info.json（托管实例会自动重启生效）；
/// - 客户端退出时只关闭由本程序拉起的 API 实例，不影响用户手动启动的服务。
/// </summary>
public sealed class ApiBootstrapService
{
    public const string ApiBaseUrl = "http://localhost:3200";
    private const string BundledApiDirectoryName = "api";
    private const string ExternalApiDirectoryEnvironmentVariable = "QQ_MUSIC_API_HOME";
    private const string NodeExeName = "node.exe";
    private const string ApiEntry = "dist\\app.js";

    private readonly ILogger<ApiBootstrapService> _logger;
    private readonly HttpClient _probeClient;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Lazy<string?> _resolvedApiDirectory;

    private Process? _managedProcess;
    private bool _managed;
    private bool _ready;

    public ApiBootstrapService(ILogger<ApiBootstrapService> logger)
    {
        _logger = logger;
        _probeClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        _resolvedApiDirectory = new Lazy<string?>(
            ResolveApiDirectory,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>API 是否由本程序拉起（而非用户手动启动）。</summary>
    public bool IsManaged => _managed;

    /// <summary>API 是否已就绪。</summary>
    public bool IsReady => _ready;

    /// <summary>实际使用的 api 目录；优先随包分发目录，也支持本地开发目录。</summary>
    public string ApiDir =>
        _resolvedApiDirectory.Value ?? Path.Combine(AppContext.BaseDirectory, BundledApiDirectoryName);

    /// <summary>
    /// 确保 API 可用：优先复用已在运行的实例，否则拉起内嵌实例并等待就绪。
    /// 不阻塞 UI 线程；由调用方以 fire-and-forget 方式触发。
    /// </summary>
    public async Task EnsureApiRunningAsync(CancellationToken ct = default)
    {
        if (_ready)
            return;

        await _gate.WaitAsync(ct);
        try
        {
            if (_ready)
                return;

            if (await IsApiReachableAsync())
            {
                _ready = true;
                _managed = false;
                _logger.LogInformation("检测到 qq-music-api 已在运行（{Url}），直接复用", ApiBaseUrl);
                return;
            }

            var apiDir = _resolvedApiDirectory.Value;
            if (apiDir is null)
            {
                _logger.LogWarning(
                    "未找到 qq-music-api。可将完整服务放在 {BundledDir}，或通过 {EnvironmentVariable} 指定目录。",
                    Path.Combine(AppContext.BaseDirectory, BundledApiDirectoryName),
                    ExternalApiDirectoryEnvironmentVariable);
                return;
            }

            _managed = StartApiProcess(apiDir);
            if (_managed)
            {
                _ready = await WaitForApiReadyAsync(TimeSpan.FromSeconds(25), ct);
                _logger.LogInformation(
                    _ready ? "qq-music-api 已由本程序拉起并就绪" : "qq-music-api 拉起后未在预期时间内就绪，请查看 {Dir}\\logs\\api.log", ApiDir);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<bool> IsApiReachableAsync()
    {
        try
        {
            using var resp = await _probeClient.GetAsync(ApiBaseUrl + "/", HttpCompletionOption.ResponseHeadersRead);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private bool StartApiProcess(string apiDir)
    {
        try
        {
            var bundledNodePath = Path.Combine(apiDir, NodeExeName);
            var nodePath = File.Exists(bundledNodePath) ? bundledNodePath : "node";

            var logDir = AppStoragePaths.GetLogDirectory("qq-music-api");
            var logPath = Path.Combine(logDir, "api.log");

            var psi = new ProcessStartInfo
            {
                FileName = nodePath,
                Arguments = ApiEntry,
                WorkingDirectory = apiDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var proc = Process.Start(psi);
            if (proc is null)
                return false;

            proc.OutputDataReceived += (_, e) => AppendLog(logPath, e.Data);
            proc.ErrorDataReceived += (_, e) => AppendLog(logPath, e.Data);
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            _managedProcess = proc;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动内嵌 qq-music-api 失败");
            return false;
        }
    }

    private static void AppendLog(string path, string? line)
    {
        if (string.IsNullOrEmpty(line))
            return;
        try
        {
            File.AppendAllText(path, $"[{DateTime.Now:HH:mm:ss}] {line}{Environment.NewLine}");
        }
        catch
        {
            // 日志写入失败不影响服务运行
        }
    }

    private async Task<bool> WaitForApiReadyAsync(TimeSpan timeout, CancellationToken ct)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            if (await IsApiReachableAsync())
                return true;
            await Task.Delay(500, ct);
        }
        return false;
    }

    /// <summary>
    /// 登录成功后把 cookie 写入 API 的 config/user-info.json；
    /// 若 API 由本程序托管则重启进程使其立即生效。
    /// </summary>
    public async Task ApplyCookieToApiAsync(string? cookie)
    {
        if (string.IsNullOrWhiteSpace(cookie))
            return;

        try
        {
            var apiDir = _resolvedApiDirectory.Value;
            if (apiDir is null)
                return;

            var cfgDir = Path.Combine(apiDir, "config");
            Directory.CreateDirectory(cfgDir);
            var loginUin = ExtractUin(cookie);
            var payload = $"{{\"loginUin\":\"{JsonEscape(loginUin)}\",\"cookie\":\"{JsonEscape(cookie)}\"}}";
            File.WriteAllText(Path.Combine(cfgDir, "user-info.json"), payload, Encoding.UTF8);

            if (_managed && _managedProcess is { HasExited: false })
            {
                _logger.LogInformation("QQ cookie 已写入内嵌 API 配置，重启 API 使其生效");
                StopManagedProcess();
                _ready = false;
                _managed = StartApiProcess(apiDir);
                if (_managed)
                    _ready = await WaitForApiReadyAsync(TimeSpan.FromSeconds(15), CancellationToken.None);
            }

            _logger.LogInformation("QQ cookie 已同步到 qq-music-api");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "同步 QQ cookie 到 API 失败");
        }
    }

    private static string JsonEscape(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string? ResolveApiDirectory()
    {
        var configuredDirectory = Environment.GetEnvironmentVariable(ExternalApiDirectoryEnvironmentVariable);
        var storageRoot = AppStoragePaths.Root;

        var candidates = new[]
        {
            configuredDirectory,
            Path.Combine(AppContext.BaseDirectory, BundledApiDirectoryName),
            Path.Combine(storageRoot, "qq-music-api"),
            Path.Combine(storageRoot, BundledApiDirectoryName)
        };

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            try
            {
                var fullPath = Path.GetFullPath(candidate.Trim());
                if (Directory.Exists(fullPath) &&
                    File.Exists(Path.Combine(fullPath, "dist", "app.js")))
                {
                    return fullPath;
                }
            }
            catch
            {
                // Ignore malformed environment values and continue probing known locations.
            }
        }

        return null;
    }

    private static string ExtractUin(string cookie)
    {
        foreach (var part in cookie.Split(';'))
        {
            var seg = part.Trim();
            if (seg.StartsWith("uin=", StringComparison.OrdinalIgnoreCase))
                return seg[4..].Trim();
        }
        return "";
    }

    private void StopManagedProcess()
    {
        var proc = _managedProcess;
        _managedProcess = null;
        if (proc is null)
            return;

        try
        {
            if (!proc.HasExited)
                proc.Kill(entireProcessTree: true);
            proc.WaitForExit(3000);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止内嵌 qq-music-api 失败");
        }

        proc.Dispose();
    }

    /// <summary>
    /// 客户端退出时调用：仅关闭由本程序拉起的 API 实例。
    /// </summary>
    public void Shutdown()
    {
        if (_managed)
        {
            _logger.LogInformation("客户端退出，关闭由本程序拉起的 qq-music-api");
            StopManagedProcess();
            _managed = false;
        }
        _ready = false;
    }
}
