using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
/// 电台 (FM) 推荐响应数据
/// </summary>
public record FmRecommendResponse : KgBaseModel
{
    [JsonPropertyName("data")]
    public List<FmRecommendCategory> Data { get; set; } = new();
}

/// <summary>
/// 单个电台频道信息
/// </summary>
public record FmRecommendCategory
{
    /// <summary>
    /// 电台 ID
    /// </summary>
    [JsonPropertyName("fmid")]
    public string FmId { get; set; } = "";

    /// <summary>
    /// 电台名称 (例如: 抖音热门歌)
    /// </summary>
    [JsonPropertyName("fmname")]
    public string FmName { get; set; } = "";

    /// <summary>
    /// 分类 ID
    /// </summary>
    [JsonPropertyName("classid")]
    public string ClassId { get; set; } = "";

    /// <summary>
    /// 分类名称 (例如: 主题, 语言, 风格)
    /// </summary>
    [JsonPropertyName("classname")]
    public string ClassName { get; set; } = "";

    /// <summary>
    /// 电台描述
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// 顶部 Banner 封面图
    /// </summary>
    [JsonPropertyName("banner")]
    public string Banner { get; set; } = "";

    /// <summary>
    /// 电台 Logo 封面图
    /// </summary>
    [JsonPropertyName("imgurl")]
    public string ImgUrl { get; set; } = "";

    /// <summary>
    /// 推荐歌曲列表 (通常会返回 1 首作为试听/封面辅助)
    /// </summary>
    [JsonPropertyName("rcmdlist")]
    public List<FmRecommendSong> RecommendSongs { get; set; } = new();
}

/// <summary>
/// 电台单曲信息
/// </summary>
public record FmRecommendSong
{
    /// <summary>
    /// 歌曲名称 (注意：该接口下通常是 "歌手 - 歌名" 的组合形式)
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// 歌曲 Hash
    /// </summary>
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = "";

    /// <summary>
    /// 音频 ID
    /// </summary>
    [JsonPropertyName("audio_id")]
    public long AudioId { get; set; }

    /// <summary>
    /// 专辑音频 ID
    /// </summary>
    [JsonPropertyName("album_audio_id")]
    public long AlbumAudioId { get; set; }

    /// <summary>
    /// 专辑 ID
    /// </summary>
    [JsonPropertyName("album_id")]
    public string AlbumId { get; set; } = "";

    /// <summary>
    /// 播放时长 (毫秒)
    /// </summary>
    [JsonPropertyName("time")]
    public int DurationMs { get; set; }

    /// <summary>
    /// 权限位 (8/10为VIP, 0为免费等)
    /// </summary>
    [JsonPropertyName("privilege")]
    public int Privilege { get; set; }

    /// <summary>
    /// 额外转换参数，复用项目中已有的 TransParam 类 (用于获取 union_cover 高清封面)
    /// </summary>
    [JsonPropertyName("trans_param")]
    public TransParam? TransParam { get; set; }
}