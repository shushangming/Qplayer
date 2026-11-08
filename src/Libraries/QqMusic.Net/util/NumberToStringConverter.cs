using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QqMusic.Net.util;

/// <summary>
///     将 JSON 数字或字符串统一转换为 string 的转换器。
///     QQ 上游接口字段类型不统一（如 dissid 在公开歌单接口是字符串、在用户歌单接口是数字），
///     应用此转换器的 string 属性可同时兼容两种形态。
/// </summary>
public sealed class NumberToStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetDecimal().ToString(CultureInfo.InvariantCulture),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token {reader.TokenType} when reading string.")
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}
