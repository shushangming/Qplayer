using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using KuGou.Net.Clients;
using KuGou.Net.Protocol.Session;
using Microsoft.Extensions.Logging;
using SukiUI.Toasts;
using QqUserClient = QqMusic.Net.Clients.UserClient;

namespace KugouAvaloniaPlayer.Services;

public interface ILoginInitializationService
{
    Task<LoginInitializationResult> InitializeLocalSessionAsync();
    Task<UserProfileLoadResult> LoadCurrentUserProfileAsync();
}

internal sealed class LoginInitializationService(
    KgSessionManager sessionManager,
    LoginClient loginClient,
    UserClient userClient,
    QqCookieService qqCookieService,
    QqUserClient qqUserClient,
    ISukiToastManager toastManager,
    ILogger<LoginInitializationService> logger) : ILoginInitializationService
{
    public async Task<LoginInitializationResult> InitializeLocalSessionAsync()
    {
        try
        {
            // QQ 音乐优先：客户端已保存 QQ cookie 时直接恢复 QQ 登录态（不依赖酷狗会话）。
            if (qqCookieService.IsLoggedIn)
            {
                var profileResult = await LoadCurrentUserProfileAsync();
                var uin = ParseUin(qqCookieService.Cookie);
                logger.LogInformation("已恢复 QQ 音乐登录态: uin={Uin}", uin);
                return new LoginInitializationResult(
                    true,
                    profileResult.Profile,
                    profileResult.Failed);
            }

            var session = sessionManager.Session;
            if (string.IsNullOrEmpty(session.Token))
            {
                logger.LogInformation("未登录，以游客身份运行。");
                loginClient.LogOutAsync();
                return LoginInitializationResult.GuestResult;
            }

            var kgProfileResult = await LoadKugouCurrentUserProfileAsync();
            logger.LogInformation("已加载本地用户: {UserId}", session.UserId);
            return new LoginInitializationResult(
                true,
                kgProfileResult.Profile,
                kgProfileResult.Failed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "登录初始化失败");
            await Dispatcher.UIThread.InvokeAsync(() =>
                toastManager.CreateToast()
                    .OfType(NotificationType.Error)
                    .WithTitle("登录初始化失败")
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .WithContent("登录初始化失败，请重新登录或检查网络连接")
                    .Queue());
            loginClient.LogOutAsync();
            return LoginInitializationResult.FailedResult;
        }
    }

    public async Task<UserProfileLoadResult> LoadCurrentUserProfileAsync()
    {
        try
        {
            // QQ 音乐优先：QQ cookie 存在时用 QQ 头像/昵称，避免酷狗接口报"加载用户信息失败"。
            if (qqCookieService.IsLoggedIn)
            {
                var uin = ParseUin(qqCookieService.Cookie);

                string? avatar = null;
                try
                {
                    var avatarResult = await qqUserClient.GetUserAvatarAsync(uin > 0 ? uin : null, size: 140);
                    avatar = string.IsNullOrWhiteSpace(avatarResult?.AvatarUrl) ? null : avatarResult.AvatarUrl;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "获取 QQ 头像失败，忽略。");
                }

                var name = uin > 0 ? $"QQ音乐用户 {uin}" : "QQ音乐用户";
                return new UserProfileLoadResult(
                    new UserProfileSnapshot(
                        name,
                        avatar,
                        uin > 0 ? uin.ToString() : "0"),
                    false);
            }

            return await LoadKugouCurrentUserProfileAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "加载用户信息失败");
            return UserProfileLoadResult.FailedResult;
        }
    }

    private async Task<UserProfileLoadResult> LoadKugouCurrentUserProfileAsync()
    {
        try
        {
            var userInfo = await userClient.GetUserInfoAsync();
            if (userInfo == null)
                return UserProfileLoadResult.EmptyResult;

            return new UserProfileLoadResult(
                new UserProfileSnapshot(
                    userInfo.Name,
                    string.IsNullOrWhiteSpace(userInfo.Pic) ? null : userInfo.Pic,
                    sessionManager.Session.UserId),
                false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "加载酷狗用户信息失败");
            return UserProfileLoadResult.FailedResult;
        }
    }

    private static long ParseUin(string? cookie)
    {
        if (string.IsNullOrWhiteSpace(cookie))
            return 0;

        foreach (System.Text.RegularExpressions.Match m in
                 System.Text.RegularExpressions.Regex.Matches(cookie, @"(?:^|[; ])uin=(\d+)"))
        {
            if (long.TryParse(m.Groups[1].Value, out var uin) && uin > 0)
                return uin;
        }

        return 0;
    }
}

public sealed record LoginInitializationResult(
    bool IsLoggedIn,
    UserProfileSnapshot? Profile,
    bool UserProfileLoadFailed)
{
    public static LoginInitializationResult GuestResult { get; } = new(false, null, false);
    public static LoginInitializationResult FailedResult { get; } = new(false, null, false);
}

public sealed record UserProfileLoadResult(
    UserProfileSnapshot? Profile,
    bool Failed)
{
    public static UserProfileLoadResult EmptyResult { get; } = new(null, false);
    public static UserProfileLoadResult FailedResult { get; } = new(null, true);
}

public sealed record UserProfileSnapshot(
    string UserName,
    string? UserAvatar,
    string UserId);
