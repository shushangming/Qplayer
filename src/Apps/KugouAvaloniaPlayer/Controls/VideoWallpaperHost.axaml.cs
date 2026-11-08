using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using KugouAvaloniaPlayer.Services;
using LibVLCSharp.Shared;
using Serilog;

namespace KugouAvaloniaPlayer.Controls;

/// <summary>
/// 视频壁纸渲染宿主：LibVLC 软解码 → BGRA 帧缓冲 → WriteableBitmap → Image 显示。
/// 纯 Avalonia 渲染，不依赖原生子窗口，透明窗口下与图片壁纸同样可靠。
/// 未启用视频壁纸时不初始化 VLC（启动零开销）。
/// </summary>
public partial class VideoWallpaperHost : UserControl
{
    private const int FrameTimerIntervalMs = 16; // ≈60fps 刷新
    private const uint MaxDecodeWidth = 3840;    // 解码输出上限（4K 及以下原样输出，超过才等比缩放）
    private const uint MaxDecodeHeight = 2160;

    private LibVLC? _libVlc;
    private MediaPlayer? _mediaPlayer;
    private bool _initialized;
    private bool _initializing;

    private string? _pendingPath;
    private double _pendingOpacity;
    private bool _hasPendingPlay;

    // 帧渲染状态（format 回调在 libvlc 线程，UI timer 在主线程，均经 Interlocked 保护）
    private readonly object _bufferLock = new();
    private IntPtr _bufferA;
    private IntPtr _bufferB;
    private int _frameWidth;
    private int _frameHeight;
    private int _frameRowBytes;
    private int _frameBytes;
    private int _writeIndex;                 // 下一帧写入缓冲号 (0/1)
    private int _readyIndex = -1;            // 最新完成帧所在缓冲号
    private int _shownIndex = -1;            // 已拷贝到界面的帧缓冲号
    private WriteableBitmap? _writeableBitmap;
    private DispatcherTimer? _frameTimer;

    public VideoWallpaperHost()
    {
        InitializeComponent();
        IsHitTestVisible = false;
        IsVisible = false;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        VideoWallpaperService.Attach(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        VideoWallpaperService.Detach(this);
        base.OnDetachedFromVisualTree(e);
    }

    public void Play(string path, double opacity)
    {
        _pendingPath = path;
        _pendingOpacity = opacity;
        _hasPendingPlay = true;

        if (_initialized)
        {
            PlayCore(path, opacity);
            return;
        }

        // 懒初始化：仅在首次需要播放视频时后台初始化，避免拖慢启动
        _ = InitializeAndPlayAsync();
    }

    public void Stop()
    {
        _hasPendingPlay = false;
        StopCore();
        IsVisible = false;
        _frameTimer?.Stop();
    }

    private async Task InitializeAndPlayAsync()
    {
        if (_initializing || _initialized)
            return;

        _initializing = true;
        try
        {
            await Task.Run(() =>
            {
                Core.Initialize();
                _libVlc = new LibVLC("--no-audio", "--quiet", "--no-video-title-show");
                _mediaPlayer = new MediaPlayer(_libVlc);
                // 注册视频帧回调（必须在播放前设置）
                _mediaPlayer.SetVideoCallbacks(OnVideoLock, OnVideoUnlock, OnVideoDisplay);
                _mediaPlayer.SetVideoFormatCallbacks(OnVideoFormatSetup, OnVideoCleanup);
            });

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _initialized = true;

                _frameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(FrameTimerIntervalMs) };
                _frameTimer.Tick += OnFrameTimerTick;

                if (_hasPendingPlay && _pendingPath is not null)
                    PlayCore(_pendingPath, _pendingOpacity);
            });
        }
        catch (Exception ex)
        {
            _initialized = false;
            Log.Error(ex, "初始化 LibVLC 失败，视频壁纸不可用");
            IsVisible = false;
        }
        finally
        {
            _initializing = false;
        }
    }

    private void PlayCore(string path, double opacity)
    {
        if (!_initialized || _libVlc is null || _mediaPlayer is null)
        {
            IsVisible = false;
            return;
        }

        StopCore();

        try
        {
            // 直接以本地路径字符串作为 MRL
            var media = new Media(_libVlc, path);
            // 无限循环 + 静音（作为壁纸不应出声）
            media.AddOption(":input-repeat=65535");
            media.AddOption(":no-audio");

            _mediaPlayer.Volume = 0;
            Opacity = Math.Clamp(opacity, 0.0, 1.0);
            IsVisible = true;
            _frameTimer?.Start();
            _mediaPlayer.Play(media);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "播放视频壁纸失败: {Path}", path);
            IsVisible = false;
            _frameTimer?.Stop();
        }
    }

    // ---------- libvlc 视频回调（libvlc 线程） ----------

    private IntPtr OnVideoLock(IntPtr opaque, IntPtr planes)
    {
        var buf = _writeIndex == 0 ? _bufferA : _bufferB;
        // libvlc 以 planes[0] 作为像素缓冲指针，必须写入；返回值作为补充
        try
        {
            Marshal.WriteIntPtr(planes, 0, buf);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "写入 planes 失败");
        }
        return buf;
    }

    private void OnVideoUnlock(IntPtr opaque, IntPtr picture, IntPtr planes)
    {
        // 无需额外处理
    }

    private void OnVideoDisplay(IntPtr opaque, IntPtr picture)
    {
        var done = _writeIndex;
        _writeIndex = 1 - _writeIndex;
        Interlocked.Exchange(ref _readyIndex, done);
    }

    private uint OnVideoFormatSetup(ref IntPtr opaque, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines)
    {
        try
        {
            // 固定输出 BGRA（与 Avalonia Bgra8888 匹配）
            unsafe
            {
                var p = (byte*)chroma;
                p[0] = (byte)'B'; p[1] = (byte)'G'; p[2] = (byte)'R'; p[3] = (byte)'A';
            }

            // 等比缩放到解码上限，避免 4K 视频每帧 30MB+ 的拷贝开销
            if (width > MaxDecodeWidth || height > MaxDecodeHeight)
            {
                var scale = Math.Min((double)MaxDecodeWidth / width, (double)MaxDecodeHeight / height);
                width = (uint)Math.Max(1, (uint)(width * scale));
                height = (uint)Math.Max(1, (uint)(height * scale));
            }

            _frameWidth = (int)width;
            _frameHeight = (int)height;
            _frameRowBytes = _frameWidth * 4;
            _frameBytes = _frameRowBytes * _frameHeight;
            pitches = (uint)_frameRowBytes;
            lines = (uint)_frameHeight;

            AllocateBuffers();
            EnsureWriteableBitmap();
            return 1;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "视频壁纸格式协商失败");
            return 0;
        }
    }

    private void OnVideoCleanup(ref IntPtr opaque)
    {
        // 缓冲由 Shutdown 统一释放；这里仅作标记，避免与 UI 线程竞争
        Interlocked.Exchange(ref _readyIndex, -1);
    }

    // ---------- 缓冲与位图 ----------

    private void AllocateBuffers()
    {
        // 预留对齐/越界安全边距
        var needed = Math.Max(_frameBytes, 1) + 4096;
        lock (_bufferLock)
        {
            if (_bufferA != IntPtr.Zero) Marshal.FreeHGlobal(_bufferA);
            if (_bufferB != IntPtr.Zero) Marshal.FreeHGlobal(_bufferB);
            _bufferA = Marshal.AllocHGlobal(needed);
            _bufferB = Marshal.AllocHGlobal(needed);
            _writeIndex = 0;
            Interlocked.Exchange(ref _readyIndex, -1);
        }
    }

    private void EnsureWriteableBitmap()
    {
        if (_writeableBitmap is not null &&
            _writeableBitmap.PixelSize.Width == _frameWidth &&
            _writeableBitmap.PixelSize.Height == _frameHeight)
        {
            return;
        }

        var size = new PixelSize(Math.Max(_frameWidth, 1), Math.Max(_frameHeight, 1));
        var bitmap = new WriteableBitmap(size, new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);

        Dispatcher.UIThread.Post(() =>
        {
            _writeableBitmap?.Dispose();
            _writeableBitmap = bitmap;
            FrameImage.Source = bitmap;
        });
    }

    private void OnFrameTimerTick(object? sender, EventArgs e)
    {
        if (!IsVisible || _writeableBitmap is null)
            return;

        // 格式协商期间位图可能尚未重建，尺寸不一致时跳过本轮拷贝
        if (_writeableBitmap.PixelSize.Width != _frameWidth ||
            _writeableBitmap.PixelSize.Height != _frameHeight)
            return;

        var ready = Volatile.Read(ref _readyIndex);
        if (ready < 0 || ready == _shownIndex)
            return;

        lock (_bufferLock)
        {
            // 二次确认（等待换源分配完成）
            ready = Volatile.Read(ref _readyIndex);
            if (ready < 0 || ready == _shownIndex)
                return;

            var src = ready == 0 ? _bufferA : _bufferB;
            try
            {
                using var fb = _writeableBitmap.Lock();
                var dst = fb.Address;
                var dstRowBytes = fb.RowBytes;

                unsafe
                {
                    for (var y = 0; y < _frameHeight; y++)
                    {
                        var srcRow = (byte*)src + y * _frameRowBytes;
                        var dstRow = (byte*)dst + y * dstRowBytes;
                        Buffer.MemoryCopy(srcRow, dstRow, _frameRowBytes, _frameRowBytes);
                    }
                }

                _shownIndex = ready;
                FrameImage.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "视频壁纸帧拷贝失败");
            }
        }
    }

    private void StopCore()
    {
        if (_mediaPlayer is null)
            return;

        try
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Media = null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "停止视频壁纸失败");
        }
    }

    public void Shutdown()
    {
        StopCore();
        _frameTimer?.Stop();
        _frameTimer = null;
        _mediaPlayer?.Dispose();
        _mediaPlayer = null;
        _libVlc?.Dispose();
        _libVlc = null;
        _writeableBitmap?.Dispose();
        _writeableBitmap = null;
        FrameImage.Source = null;
        lock (_bufferLock)
        {
            if (_bufferA != IntPtr.Zero) Marshal.FreeHGlobal(_bufferA);
            if (_bufferB != IntPtr.Zero) Marshal.FreeHGlobal(_bufferB);
            _bufferA = IntPtr.Zero;
            _bufferB = IntPtr.Zero;
        }
        _initialized = false;
    }
}
