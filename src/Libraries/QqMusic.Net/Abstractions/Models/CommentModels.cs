using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     评论信息。
/// </summary>
public record QqComment : QqBaseModel
{
    [property: JsonPropertyName("commentid")]
    public string? CommentId { get; set; }

    [property: JsonPropertyName("rootcommentid")]
    public string? RootCommentId { get; set; }

    [property: JsonPropertyName("content")]
    public string? Content { get; set; }

    [property: JsonPropertyName("time")]
    public long? Time { get; set; }

    [property: JsonPropertyName("praise_num")]
    public int? PraiseNum { get; set; }

    [property: JsonPropertyName("reply_num")]
    public int? ReplyNum { get; set; }

    [property: JsonPropertyName("is_hot")]
    public int? IsHot { get; set; }

    [property: JsonPropertyName("is_praise")]
    public int? IsPraise { get; set; }

    [property: JsonPropertyName("uin")]
    public long? Uin { get; set; }

    [property: JsonPropertyName("encrypt_uin")]
    public string? EncryptUin { get; set; }

    [property: JsonPropertyName("nick")]
    public string? Nick { get; set; }

    [property: JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [property: JsonPropertyName("middlecomment_content")]
    public string? MiddleCommentContent { get; set; }
}

/// <summary>
///     评论列表结果。
/// </summary>
public record QqCommentList : QqBaseModel
{
    [property: JsonPropertyName("commentlist")]
    public List<QqComment>? Comments { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqComment>? List { get; set; }

    [property: JsonPropertyName("totalnum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }

    [property: JsonPropertyName("curnum")]
    public int? CurNum { get; set; }

    [property: JsonPropertyName("pagenum")]
    public int? PageNum { get; set; }

    [property: JsonPropertyName("id")]
    public string? Id { get; set; }

    [property: JsonPropertyName("biztype")]
    public int? BizType { get; set; }
}

/// <summary>
///     热门评论结果。
/// </summary>
public record QqHotCommentList : QqBaseModel
{
    [property: JsonPropertyName("commentlist")]
    public List<QqComment>? Comments { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqComment>? List { get; set; }

    [property: JsonPropertyName("totalnum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }

    [property: JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    ///     类型：1歌曲 / 2歌单 / 3专辑。
    /// </summary>
    [property: JsonPropertyName("type")]
    public int? Type { get; set; }
}
