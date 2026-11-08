using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     当前登录用户详情。
/// </summary>
public record UserDetailModel : KgBaseModel
{
    /// <summary>
    ///     用户昵称。
    /// </summary>
    [property: JsonPropertyName("nickname")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     用户头像地址。
    /// </summary>
    [property: JsonPropertyName("pic")] public string Pic { get; set; } = string.Empty;
}

/// <summary>
///     领取一天 VIP 的结果。
/// </summary>
public record OneDayVipModel : KgBaseModel
{
    /// <summary>
    ///     获得的 VIP 天数。
    /// </summary>
    [property: JsonPropertyName("ad_vip_num")]
    public int AdVipNum { get; set; }

    /// <summary>
    ///     VIP 到期时间。
    /// </summary>
    [property: JsonPropertyName("ad_vip_end_time")]
    public int AdVipEndTime { get; set; }
}

/// <summary>
///     VIP 升级奖励领取结果。
/// </summary>
public record UpgradeVipModel : KgBaseModel
{
    /// <summary>
    ///     获得的充值时长。
    /// </summary>
    [property: JsonPropertyName("recharge_hours")]
    public int RechargeHours { get; set; }
}
