using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace KuGou.Net.util;

internal static class RequestBodyJsonSerializer
{
    public static string Serialize(object body, JsonTypeInfo? bodyTypeInfo)
    {
        if (bodyTypeInfo != null) return JsonSerializer.Serialize(body, bodyTypeInfo);

        return body switch
        {
            JsonObject jsonObject => JsonSerializer.Serialize(jsonObject, AppJsonContext.Default.JsonObject),
            JsonNode jsonNode => JsonSerializer.Serialize(jsonNode, AppJsonContext.Default.JsonNode),
            _ => throw new InvalidOperationException(
                $"Body type '{body.GetType().FullName}' requires an explicit JsonTypeInfo for AOT-safe serialization.")
        };
    }
}
