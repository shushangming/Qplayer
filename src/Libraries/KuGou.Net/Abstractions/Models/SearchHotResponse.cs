using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     对应 data 节点的数据
/// </summary>
public record SearchHotResponse
{
    /// <summary>
    ///     热搜生成时间戳。
    /// </summary>
    [property: JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    ///     热搜分类列表。
    /// </summary>
    [property: JsonPropertyName("list")] public List<SearchHotCategory> Categories { get; set; } = new();
}

/// <summary>
///     榜单分类（如：热搜榜、飙升榜）
/// </summary>
public record SearchHotCategory
{
    /// <summary>
    ///     分类名称。
    /// </summary>
    [property: JsonPropertyName("name")] public string Name { get; set; } = "";

    /// <summary>
    ///     分类下的热搜关键词。
    /// </summary>
    [property: JsonPropertyName("keywords")]
    public List<SearchHotKeyword> Keywords { get; set; } = new();
}

/// <summary>
///     具体的热搜关键词项
/// </summary>
public record SearchHotKeyword
{
    /// <summary>
    ///     热搜关键词。
    /// </summary>
    [property: JsonPropertyName("keyword")]
    public string Keyword { get; set; } = "";

    /// <summary>
    ///     上榜原因。
    /// </summary>
    [property: JsonPropertyName("reason")] public string Reason { get; set; } = "";

    /// <summary>
    ///     跳转链接。
    /// </summary>
    [property: JsonPropertyName("jumpurl")]
    public string JumpUrl { get; set; } = "";

    /// <summary>
    ///     结构化结果链接。
    /// </summary>
    [property: JsonPropertyName("json_url")]
    public string JsonUrl { get; set; } = "";

    /// <summary>
    ///     是否为封面词。
    /// </summary>
    [property: JsonPropertyName("is_cover_word")]
    public int IsCoverWord { get; set; }

    /// <summary>
    ///     热搜类型。
    /// </summary>
    [property: JsonPropertyName("type")] public int Type { get; set; }

    /// <summary>
    ///     图标标记。
    /// </summary>
    [property: JsonPropertyName("icon")] public int Icon { get; set; }
}
