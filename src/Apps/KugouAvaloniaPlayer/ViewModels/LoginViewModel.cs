using System;
using System.Collections.ObjectModel;
using ZLinq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KugouAvaloniaPlayer.Models;
using KugouAvaloniaPlayer.Services;
using Microsoft.Extensions.Logging;
using QqLoginClient = QqMusic.Net.Clients.LoginClient;

namespace KugouAvaloniaPlayer.ViewModels;

public partial class LoginViewModel(
    QqLoginClient authClient,
    QqCookieService qqCookieService,
    ApiBootstrapService apiBootstrapService,
    IMessenger messenger,
    ILogger<LoginViewModel> logger)
    : ObservableObject
{
    private static readonly HttpClient SharedHttp = new();

    [ObservableProperty] public partial string Code { get; set; } = "";

    [ObservableProperty] public partial int Countdown { get; set; }

    [ObservableProperty] public partial bool IsLoggingIn { get; set; }

    [ObservableProperty] public partial bool IsQrExpired { get; set; }

    [ObservableProperty] public partial bool IsQrLoginSelected { get; set; } = true;

    [ObservableProperty] public partial bool IsSendingCode { get; set; }

    [ObservableProperty] public partial bool HasLoginAccountChoices { get; set; }

    [ObservableProperty] public partial string Mobile { get; set; } = "";

    [ObservableProperty] public partial string? QrCodeImageUrl { get; set; }

    private string? _qrSig;
    private long? _ptQrToken;
    private CancellationTokenSource? _qrPollingCts;
    private int _qrRequestVersion;
    [ObservableProperty] public partial string QrStatusMessage { get; set; } = "请使用QQ音乐App扫码";

    [ObservableProperty] public partial string StatusMessage { get; set; } = "";

    public ObservableCollection<LoginAccountOptionViewModel> LoginAccountChoices { get; } = [];

    partial void OnCodeChanged(string value)
    {
        ClearLoginAccountChoices();
    }

    partial void OnIsQrLoginSelectedChanged(bool value)
    {
        if (value)
            _ = RefreshQrCode();
        else
            StopQrPolling();
    }

    partial void OnMobileChanged(string value)
    {
        ClearLoginAccountChoices();
    }

    /// <summary>
    ///     供登录页挂载时调用：二维码为空或已过期时自动刷新一次。
    /// </summary>
    public Task RefreshQrCodeAsync()
    {
        if (string.IsNullOrEmpty(QrCodeImageUrl) || IsQrExpired)
            return RefreshQrCodeCommand.ExecuteAsync(null);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task RefreshQrCode()
    {
        StopQrPolling();
        var requestVersion = _qrRequestVersion;
        QrStatusMessage = "正在获取二维码...";
        QrCodeImageUrl = null;
        IsQrExpired = false;

        try
        {
            var qr = await authClient.GetQQLoginQrAsync();
            if (requestVersion != _qrRequestVersion)
                return;

            if (qr == null ||
                string.IsNullOrEmpty(qr.QrSig) ||
                qr.PtQrToken == null)
            {
                QrStatusMessage = "获取二维码失败，请重试";
                IsQrExpired = true;
                return;
            }

            var rawImage = qr.Img;
            var base64Image = await NormalizeQrImageAsync(rawImage);
            if (requestVersion != _qrRequestVersion)
                return;

            if (string.IsNullOrEmpty(base64Image))
            {
                QrStatusMessage = "二维码内容获取失败，请重试";
                IsQrExpired = true;
                return;
            }

            _qrSig = qr.QrSig;
            _ptQrToken = qr.PtQrToken;
            QrCodeImageUrl = base64Image;
            QrStatusMessage = "请使用QQ音乐App扫码";
            StartQrPolling();
        }
        catch (Exception ex)
        {
            if (requestVersion != _qrRequestVersion)
                return;

            logger.LogWarning(ex, "获取QQ登录二维码失败");
            QrStatusMessage = $"获取二维码出错: {ex.Message}";
            IsQrExpired = true;
        }
    }

    private static async Task<string?> NormalizeQrImageAsync(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        // 后端可能直接返回 http(s) 图片地址，也可能直接返回 base64 / data URI。
        // XAML 侧的 Base64ImageConverter 只认 base64，http 地址需要先下载再转 base64。
        if (raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var bytes = await SharedHttp.GetByteArrayAsync(raw);
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return null;
            }
        }

        return raw;
    }

    private void StartQrPolling()
    {
        _qrPollingCts = new CancellationTokenSource();
        var token = _qrPollingCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(2000, token);
                    if (token.IsCancellationRequested) break;

                    if (string.IsNullOrEmpty(_qrSig) || _ptQrToken == null) continue;

                    try
                    {
                        var status = await authClient.CheckQQLoginQrAsync(_qrSig, _ptQrToken.Value);

                        if (status == null)
                        {
                            await Task.Delay(2000, token);
                            continue;
                        }

                        if (status.IsWaiting)
                        {
                            Dispatcher.UIThread.Post(() => QrStatusMessage = "请使用QQ音乐App扫码");
                        }
                        else if (status.IsExpired)
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                QrStatusMessage = "二维码已过期，请点击刷新";
                                IsQrExpired = true;
                            });
                            StopQrPolling();
                            break;
                        }
                        else if (status.IsConfirmed)
                        {
                            var cookie = status.Session?.Cookie;
                            Dispatcher.UIThread.Post(() =>
                            {
                                if (!string.IsNullOrWhiteSpace(cookie))
                                {
                                    try
                                    {
                                        qqCookieService.Save(cookie);
                                        _ = apiBootstrapService.ApplyCookieToApiAsync(cookie);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.LogError(ex, "保存QQ登录 cookie 失败");
                                    }

                                    QrStatusMessage = "登录成功";
                                    messenger.Send(new AuthStateChangedEvent(true));
                                }
                                else
                                {
                                    QrStatusMessage = "登录成功但未获取到凭证，请重试";
                                }

                                StopQrPolling();
                            });
                            break;
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogError(ex, "轮询QQ二维码登录状态出错");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "二维码登录轮询任务异常退出");
            }
        }, token);
    }

    public void StopQrPolling()
    {
        _qrRequestVersion++;
        if (_qrPollingCts != null)
        {
            _qrPollingCts.Cancel();
            _qrPollingCts.Dispose();
            _qrPollingCts = null;
        }
    }

    [RelayCommand]
    private void StopQrPollingOnUnload()
    {
        StopQrPolling();
        if (IsQrLoginSelected)
        {
            IsQrExpired = true;
            QrStatusMessage = "请刷新二维码后重新扫码";
        }
    }

    [RelayCommand]
    private Task SendCode()
    {
        // QQ 音乐 SDK 仅提供扫码登录，不支持手机号验证码登录。
        StatusMessage = "QQ音乐仅支持扫码登录，请切换到扫码登录";
        IsSendingCode = false;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task Login()
    {
        StatusMessage = "QQ音乐仅支持扫码登录，请切换到扫码登录";
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task SelectLoginAccount(LoginAccountOptionViewModel account)
    {
        StatusMessage = "QQ音乐仅支持扫码登录，请切换到扫码登录";
        return Task.CompletedTask;
    }

    private void ClearLoginAccountChoices()
    {
        if (LoginAccountChoices.Count == 0 && !HasLoginAccountChoices)
            return;

        LoginAccountChoices.Clear();
        HasLoginAccountChoices = false;
    }
}

public sealed record LoginAccountOptionViewModel(
    long UserId,
    string DisplayName,
    string? Avatar,
    string? Username);
