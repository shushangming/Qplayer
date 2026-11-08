using System;

using KugouAvaloniaPlayer.Controls;

namespace KugouAvaloniaPlayer.Services;

/// <summary>
/// 视频壁纸协调器：桥接 MainWindowViewModel（设置来源）与 VideoWallpaperHost（渲染宿主）。
/// 宿主控件挂载到可视树后注册自身；设置侧调用 Play/Stop 即可，宿主未就绪时请求会被暂存。
/// </summary>
public static class VideoWallpaperService
{
    private static readonly object Gate = new();
    private static VideoWallpaperHost? _host;
    private static string? _pendingPath;
    private static double _pendingOpacity;
    private static bool _pendingPlay;

    public static void Attach(VideoWallpaperHost host)
    {
        lock (Gate)
        {
            _host = host;
            if (_pendingPlay && _pendingPath is not null)
            {
                host.Play(_pendingPath, _pendingOpacity);
                _pendingPlay = false;
                _pendingPath = null;
            }
        }
    }

    public static void Detach(VideoWallpaperHost host)
    {
        lock (Gate)
        {
            if (ReferenceEquals(_host, host))
                _host = null;
        }
    }

    public static void Play(string path, double opacity)
    {
        lock (Gate)
        {
            if (_host is not null)
            {
                _host.Play(path, opacity);
            }
            else
            {
                _pendingPath = path;
                _pendingOpacity = opacity;
                _pendingPlay = true;
            }
        }
    }

    public static void Stop()
    {
        lock (Gate)
        {
            _pendingPlay = false;
            _pendingPath = null;
            _host?.Stop();
        }
    }

    public static bool IsVideoWallpaperFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var ext = System.IO.Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
        return ext is "mp4" or "mkv" or "webm" or "mov" or "avi" or "wmv" or "flv" or "m4v" or "mpg" or "mpeg" or "ts" or "3gp";
    }

    public const long MaxVideoWallpaperBytes = 100L * 1024 * 1024;
}
