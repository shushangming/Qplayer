using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using KuGou.Net.Infrastructure.Http;
using KuGou.Net.Protocol.Transport;
using KuGou.Net.util;

namespace KuGou.Net.Protocol.Raw;

public class RawAlbumApi(IKgTransport transport)
{
    public Task<JsonElement> GetAlbumShopAsync()
    {
        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Get,
            Path = "/zhuanjidata/v3/album_shop_v2/get_classify_data",
            SignatureType = SignatureType.Default
        });
    }

    public async Task<JsonElement> GetAlbumAsync(string albumIds, string? fields = null)
    {
        var clientTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var body = new AlbumLookupRequestBody(
            "3116",
            clientTime,
            "11440",
            albumIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => new AlbumLookupRequestItem(id, string.Empty, string.Empty))
                .ToList(),
            "-",
            fields ?? string.Empty,
            KgSigner.CalcLoginKey(clientTime),
            "-");

        var request = new KgRequest
        {
            Method = HttpMethod.Post,
            BaseUrl = "http://kmr.service.kugou.com",
            Path = "/v1/album",
            Body = body,
            BodyTypeInfo = RawAlbumApiJsonContext.Default.AlbumLookupRequestBody,
            SpecificRouter = "kmr.service.kugou.com",
            SignatureType = SignatureType.Default
        };

        return await transport.SendAsync(request);
    }

    public async Task<JsonElement> GetAlbumInfoAsync(string albumId)
    {
        var body = new AlbumInfoRequestBody(
            [new AlbumIdRequestItem(albumId)],
            0,
            "album_id,album_name,publish_date,sizable_cover,intro,language,is_publish,heat,type,quality,authors,exclusive,author_name,trans_param");

        var request = new KgRequest
        {
            Method = HttpMethod.Post,
            Path = "/kmr/v2/albums",
            Body = body,
            BodyTypeInfo = RawAlbumApiJsonContext.Default.AlbumInfoRequestBody,
            SpecificRouter = "openapi.kugou.com",
            SignatureType = SignatureType.Default,
            CustomHeaders = new Dictionary<string, string>
            {
                { "kg-tid", "255" }
            }
        };

        return await transport.SendAsync(request);
    }

    public async Task<JsonElement> GetAlbumSongAsync(string albumId, int page, int pageSize)
    {
        var body = new JsonObject
        {
            ["album_id"] = albumId,
            ["is_buy"] = 0,
            ["page"] = page,
            ["pagesize"] = pageSize
        };

        var request = new KgRequest
        {
            Method = HttpMethod.Post,
            Path = "/v1/album_audio/lite",
            Body = body,
            SpecificRouter = "openapi.kugou.com",
            SignatureType = SignatureType.Default,
            CustomHeaders = new Dictionary<string, string>
            {
                { "kg-tid", "255" }
            }
        };

        return await transport.SendAsync(request);
    }
}

internal sealed record AlbumLookupRequestItem(
    [property: JsonPropertyName("album_id")] string AlbumId,
    [property: JsonPropertyName("album_name")] string AlbumName,
    [property: JsonPropertyName("author_name")] string AuthorName);

internal sealed record AlbumLookupRequestBody(
    [property: JsonPropertyName("appid")] string AppId,
    [property: JsonPropertyName("clienttime")] long ClientTime,
    [property: JsonPropertyName("clientver")] string ClientVer,
    [property: JsonPropertyName("data")] List<AlbumLookupRequestItem> Data,
    [property: JsonPropertyName("dfid")] string Dfid,
    [property: JsonPropertyName("fields")] string Fields,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("mid")] string Mid);

internal sealed record AlbumIdRequestItem(
    [property: JsonPropertyName("album_id")] string AlbumId);

internal sealed record AlbumInfoRequestBody(
    [property: JsonPropertyName("data")] List<AlbumIdRequestItem> Data,
    [property: JsonPropertyName("is_buy")] int IsBuy,
    [property: JsonPropertyName("fields")] string Fields);

[JsonSerializable(typeof(AlbumLookupRequestBody))]
[JsonSerializable(typeof(AlbumInfoRequestBody))]
internal partial class RawAlbumApiJsonContext : JsonSerializerContext
{
}
