using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using KuGou.Net.Infrastructure.Http;
using KuGou.Net.Protocol.Session;
using KuGou.Net.Protocol.Transport;
using KuGou.Net.util;

namespace KuGou.Net.Protocol.Raw;

public class RawFmApi(IKgTransport transport, KgSessionManager sessionManager)
{
    public Task<JsonElement> GetRecommendAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Post,
            Path = "/v1/rcmd_list",
            Body = SignedBody(now, new JsonObject
            {
                ["rcmdsongcount"] = 1,
                ["level"] = 0,
                ["area_code"] = 1,
                ["get_tracker"] = 1,
                ["uid"] = 0
            }),
            SpecificRouter = "fm.service.kugou.com",
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetSongsAsync(string fmIds, int type = 2, int offset = -1, int size = 20)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var body = new FmSongsRequestBody(
            1,
            fmIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => new FmSongRequestItem(id, type, offset, size, string.Empty))
                .ToList(),
            1,
            sessionManager.Session.UserId,
            KuGouConfig.AppId,
            now,
            KuGouConfig.ClientVer,
            KgSigner.CalcLoginKey(now),
            KgUtils.CalcNewMid(sessionManager.Session.Dfid));

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Post,
            Path = "/v1/app_song_list_offset",
            Body = body,
            BodyTypeInfo = RawFmApiJsonContext.Default.FmSongsRequestBody,
            SpecificRouter = "fm.service.kugou.com",
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetClassSongAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var userId = sessionManager.Session.UserId;

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Post,
            Path = "/v1/class_fm_song",
            Body = SignedBody(now, new JsonObject
            {
                ["kguid"] = userId,
                ["platform"] = "android",
                ["uid"] = userId,
                ["get_tracker"] = 1
            }),
            SpecificRouter = "fm.service.kugou.com",
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetImagesAsync(string fmIds)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var body = new FmImagesRequestBody(
            fmIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => new FmImageRequestItem("imgUrl100,imgUrl50", id, 2))
                .ToList(),
            sessionManager.Session.Dfid,
            KuGouConfig.AppId,
            now,
            KuGouConfig.ClientVer,
            KgSigner.CalcLoginKey(now),
            KgUtils.CalcNewMid(sessionManager.Session.Dfid));

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Post,
            Path = "/v1/fm_info",
            Body = body,
            BodyTypeInfo = RawFmApiJsonContext.Default.FmImagesRequestBody,
            SpecificRouter = "fm.service.kugou.com",
            SignatureType = SignatureType.Default
        });
    }

    private JsonObject SignedBody(long now, JsonObject body)
    {
        body["appid"] = KuGouConfig.AppId;
        body["clienttime"] = now;
        body["clientver"] = KuGouConfig.ClientVer;
        body["key"] = KgSigner.CalcLoginKey(now);
        body["mid"] = KgUtils.CalcNewMid(sessionManager.Session.Dfid);
        return body;
    }
}

internal sealed record FmSongRequestItem(
    [property: JsonPropertyName("fmid")] string FmId,
    [property: JsonPropertyName("fmtype")] int FmType,
    [property: JsonPropertyName("offset")] int Offset,
    [property: JsonPropertyName("size")] int Size,
    [property: JsonPropertyName("singername")] string SingerName);

internal sealed record FmSongsRequestBody(
    [property: JsonPropertyName("area_code")] int AreaCode,
    [property: JsonPropertyName("data")] List<FmSongRequestItem> Data,
    [property: JsonPropertyName("get_tracker")] int GetTracker,
    [property: JsonPropertyName("uid")] string? UserId,
    [property: JsonPropertyName("appid")] string AppId,
    [property: JsonPropertyName("clienttime")] long ClientTime,
    [property: JsonPropertyName("clientver")] string ClientVer,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("mid")] string Mid);

internal sealed record FmImageRequestItem(
    [property: JsonPropertyName("fields")] string Fields,
    [property: JsonPropertyName("fmid")] string FmId,
    [property: JsonPropertyName("fmtype")] int FmType);

internal sealed record FmImagesRequestBody(
    [property: JsonPropertyName("data")] List<FmImageRequestItem> Data,
    [property: JsonPropertyName("dfid")] string Dfid,
    [property: JsonPropertyName("appid")] string AppId,
    [property: JsonPropertyName("clienttime")] long ClientTime,
    [property: JsonPropertyName("clientver")] string ClientVer,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("mid")] string Mid);

[JsonSerializable(typeof(FmSongsRequestBody))]
[JsonSerializable(typeof(FmImagesRequestBody))]
internal partial class RawFmApiJsonContext : JsonSerializerContext
{
}
