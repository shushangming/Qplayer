using System.Text;
using System.Text.Json;
using QqMusic.Net.Protocol.Transport;
using QqMusic.Net.util;

namespace QqMusic.Net.Infrastructure.Http;

/// <summary>
///     QQ 音乐 API 传输层接口。
/// </summary>
public interface IQqTransport
{
    /// <summary>
    ///     发送请求并返回 JsonElement。
    /// </summary>
    Task<JsonElement> SendAsync(QqRequest request);
}

/// <summary>
///     基于 HttpClient 的 QQ 音乐 API 传输层。
///     BaseUrl 固定为 localhost:3200（可配置覆盖），GET 拼 query、POST 拼 JSON body，无签名。
/// </summary>
public class QqHttpTransport(HttpClient client) : IQqTransport
{
    private const int MaxGetAttempts = 3;

    /// <inheritdoc />
    public async Task<JsonElement> SendAsync(QqRequest request)
    {
        var requestUrl = BuildRequestUrl(request);
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await SendOnceAsync(request, requestUrl);
            }
            catch (HttpRequestException ex) when (
                request.Method == HttpMethod.Get &&
                attempt < MaxGetAttempts &&
                IsTransient(ex))
            {
                await Task.Delay(GetRetryDelay(attempt));
            }
        }
    }

    private async Task<JsonElement> SendOnceAsync(QqRequest request, string requestUrl)
    {
        using var msg = CreateRequestMessage(request, requestUrl);
        using var response = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength == 0)
            return JsonElement.Parse("{}");

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync(responseStream, AppJsonContext.Default.JsonElement);
    }

    private static string BuildRequestUrl(QqRequest request)
    {
        var baseUrl = request.BaseUrl ?? Constants.DefaultBaseUrl;
        var urlBuilder = new StringBuilder($"{baseUrl.TrimEnd('/')}/{request.Path.TrimStart('/')}");

        if (request.Params.Count > 0)
        {
            urlBuilder.Append('?');
            var queryString = string.Join("&", request.Params.Select(kv =>
                $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
            urlBuilder.Append(queryString);
        }

        return urlBuilder.ToString();
    }

    private static HttpRequestMessage CreateRequestMessage(QqRequest request, string requestUrl)
    {
        var msg = new HttpRequestMessage(request.Method, requestUrl);

        if (request.CustomHeaders != null)
            foreach (var kv in request.CustomHeaders)
                msg.Headers.TryAddWithoutValidation(kv.Key, kv.Value);

        if (request.Method != HttpMethod.Post)
            return msg;

        if (request.Body != null)
        {
            var jsonBody = request.BodyTypeInfo != null
                ? JsonSerializer.Serialize(request.Body, request.BodyTypeInfo)
                : JsonSerializer.Serialize(request.Body);
            msg.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        return msg;
    }

    private static bool IsTransient(HttpRequestException exception)
    {
        if (exception.StatusCode is null)
            return true;

        var statusCode = (int)exception.StatusCode.Value;
        return statusCode is 408 or 429 || statusCode >= 500;
    }

    private static TimeSpan GetRetryDelay(int failedAttempt)
    {
        return failedAttempt == 1
            ? TimeSpan.FromMilliseconds(250)
            : TimeSpan.FromMilliseconds(750);
    }
}
