using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using KuGou.Net.Abstractions;
using KuGou.Net.Infrastructure.Http;
using KuGou.Net.Protocol.Session;
using KuGou.Net.Protocol.Transport;
using KuGou.Net.util;
using ZLinq;

namespace KuGou.Net.Protocol.Raw;

public class RawSongApi(IKgTransport transport, KgSessionManager sessionManager)
{
    public Task<JsonElement> GetAudioAsync(string hash)
    {
        var session = sessionManager.Session;
        var clientTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var body = new AudioLookupRequestBody(
            KuGouConfig.AppId,
            clientTime,
            KuGouConfig.ClientVer,
            SplitComma(hash, s => new AudioLookupRequestItem(s, 0)),
            string.IsNullOrWhiteSpace(session.Dfid) ? "-" : session.Dfid,
            KgSigner.CalcLoginKey(clientTime),
            string.IsNullOrWhiteSpace(session.Mid) ? KgUtils.CalcNewMid(session.Dfid) : session.Mid,
            string.IsNullOrWhiteSpace(session.Token) ? null : session.Token,
            string.IsNullOrWhiteSpace(session.UserId) || session.UserId == "0" ? null : session.UserId);

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Post,
            BaseUrl = "http://kmr.service.kugou.com",
            Path = "/v1/audio/audio",
            Body = body,
            BodyTypeInfo = RawSongApiJsonContext.Default.AudioLookupRequestBody,
            SpecificRouter = "kmr.service.kugou.com",
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetAudioRelatedAsync(
        long albumAudioId,
        int page = 1,
        int pageSize = 30,
        string sort = "all",
        int type = 0,
        int showType = 0,
        bool showDetail = true)
    {
        var parameters = new Dictionary<string, string>
        {
            ["album_audio_id"] = albumAudioId.ToString(),
            ["appid"] = "1005",
            ["area_code"] = "1",
            ["clientver"] = "12329"
        };

        if (!showDetail)
        {
            parameters["page"] = page.ToString();
            parameters["pagesize"] = pageSize.ToString();
            parameters["show_input"] = "1";
            parameters["show_type"] = showType.ToString();
            parameters["sort"] = (sort switch
            {
                "hot" => 2,
                "new" => 3,
                _ => 1
            }).ToString();
            parameters["type"] = type.ToString();
        }

        parameters["version"] = "1";
        parameters["signature"] = CalcJoinedSignature(parameters, "OIlwieks28dk2k092lksi2UIkp", string.Empty);

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Get,
            BaseUrl = "https://listkmrp3cdnretry.kugou.com",
            Path = showDetail ? "/v2/audio_related/total" : "/v3/album_audio/related",
            Params = parameters,
            ClearDefaultParams = true,
            NotSignature = true,
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetAudioAccompanyMatchingAsync(
        string hash,
        long mixId = 0,
        string? fileName = null)
    {
        var parameters = new Dictionary<string, string>
        {
            ["isteen"] = "0",
            ["mixId"] = mixId.ToString(),
            ["usemkv"] = "1",
            ["platform"] = "2",
            ["fileName"] = fileName ?? string.Empty,
            ["hash"] = hash,
            ["version"] = "12375",
            ["appid"] = KuGouConfig.AppId
        };
        parameters["sign"] = KgUtils.Md5($"{BuildSortedParamString(parameters, "&")}*s&iN#G70*")[8..24];

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Get,
            BaseUrl = "https://nsongacsing.kugou.com",
            Path = "/sing7/accompanywan/json/v2/cdn/optimal_matching_accompany_2_listen.do",
            Params = parameters,
            ClearDefaultParams = true,
            NotSignature = true,
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetAudioMatchAsync(byte[] pcmData)
    {
        var session = sessionManager.Session;
        var dfid = string.IsNullOrWhiteSpace(session.Dfid) ? "-" : session.Dfid;
        var mid = KgUtils.CalcNewMid(dfid);
        var clientTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var parameters = new Dictionary<string, string>
        {
            ["appid"] = KuGouConfig.OfficialAppId,
            ["clientver"] = KuGouConfig.OfficialClientVer,
            ["dfid"] = dfid,
            ["mid"] = mid,
            ["uuid"] = KgUtils.Md5(dfid + mid),
            ["clienttime"] = clientTime,
            ["fpid"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            ["area_code"] = "1",
            ["include_unpublish"] = "1",
            ["useid"] = string.IsNullOrWhiteSpace(session.UserId) ? "0" : session.UserId,
            ["multi_result"] = "1"
        };

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Post,
            Path = "/fingerprint.service/v1/music_trackid_mulit",
            Params = parameters,
            ClearDefaultParams = true,
            BinaryBody = pcmData,
            ContentType = "application/octet-stream",
            CustomHeaders = new Dictionary<string, string>
            {
                ["user-agent"] = "KuGou/11490 (Android)"
            },
            SignatureType = SignatureType.OfficialAndroid
        });
    }

    public Task<JsonElement> GetAudioKtvTotalAsync(long songId, string songHash, string singerName)
    {
        var parameters = new Dictionary<string, string>
        {
            ["isteen"] = "0",
            ["songId"] = songId.ToString(),
            ["usemkv"] = "1",
            ["platform"] = "2",
            ["singerName"] = singerName,
            ["songHash"] = songHash,
            ["version"] = "12375",
            ["appid"] = KuGouConfig.AppId
        };
        parameters["sign"] = KgUtils.Md5($"{BuildSortedParamString(parameters, "&")}*s&iN#G70*")[8..24];

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Get,
            BaseUrl = "https://acsing.service.kugou.com",
            Path = "/sing7/listenguide/json/v2/cdn/listenguide/get_total_opus_num_v02.do",
            Params = parameters,
            ClearDefaultParams = true,
            NotSignature = true,
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetKmrAudioMvAsync(string albumAudioIds, string? fields = null)
    {
        var body = new KmrAudioMvRequestBody(
            SplitComma(albumAudioIds, s => new AlbumAudioIdRequestItem(s)),
            fields ?? string.Empty);

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Post,
            Path = "/kmr/v1/audio/mv",
            Body = body,
            BodyTypeInfo = RawSongApiJsonContext.Default.KmrAudioMvRequestBody,
            SpecificRouter = "openapi.kugou.com",
            CustomHeaders = new Dictionary<string, string> { ["KG-TID"] = "38" },
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetKmrAudioAsync(string albumAudioIds, string? fields = "base")
    {
        var body = new KmrAudioRequestBody(
            SplitComma(albumAudioIds, s => new EntityIdRequestItem(long.TryParse(s, out var id) ? id : 0)),
            fields ?? "base");

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Post,
            Path = "/kmr/v2/audio",
            Body = body,
            BodyTypeInfo = RawSongApiJsonContext.Default.KmrAudioRequestBody,
            SpecificRouter = "openapi.kugou.com",
            CustomHeaders = new Dictionary<string, string> { ["KG-TID"] = "238" },
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetSongClimaxAsync(string hash)
    {
        var data = JsonSerializer.Serialize(
            SplitComma(hash, s => new HashOnlyRequestItem(s)),
            RawSongApiJsonContext.Default.ListHashOnlyRequestItem);

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Get,
            BaseUrl = "https://expendablekmrcdn.kugou.com",
            Path = "/v1/audio_climax/audio",
            Params = new Dictionary<string, string>
            {
                ["data"] = data
            },
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetSongRankingAsync(string albumAudioId)
    {
        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Get,
            Path = "/grow/v1/song_ranking/play_page/ranking_info",
            Params = new Dictionary<string, string> { ["album_audio_id"] = albumAudioId },
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetSongRankingFilterAsync(string albumAudioId, int page = 1, int pageSize = 30)
    {
        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Get,
            Path = "/grow/v1/song_ranking/unlock/v2/ranking_filter",
            Params = new Dictionary<string, string>
            {
                ["album_audio_id"] = albumAudioId,
                ["page"] = page.ToString(),
                ["pagesize"] = pageSize.ToString()
            },
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetPrivilegeLiteAsync(string hash, string? albumIds = null)
    {
        var hashes = hash.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var albums = (albumIds ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var resource = new List<PrivilegeLiteResourceItem>(hashes.Length);

        for (var i = 0; i < hashes.Length; i++)
        {
            resource.Add(new PrivilegeLiteResourceItem(
                "audio",
                0,
                hashes[i],
                i < albums.Length ? albums[i] : "0"));
        }

        var body = new PrivilegeLiteRequestBody(
            KuGouConfig.AppId,
            1,
            "play",
            KuGouConfig.ClientVer,
            1,
            1,
            1,
            resource,
            [AudioQuality.Standard, AudioQuality.High, AudioQuality.Lossless,
                AudioQuality.HiRes, "viper_atmos", "viper_tape", "viper_clear", "super", "multitrack"]);

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Post,
            Path = "/v2/get_res_privilege/lite",
            Body = body,
            BodyTypeInfo = RawSongApiJsonContext.Default.PrivilegeLiteRequestBody,
            SpecificRouter = "media.store.kugou.com",
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetImagesAsync(string hash, string? albumIds = null, string? albumAudioIds = null,
        int count = 5)
    {
        var data = JsonSerializer.Serialize(
            BuildImageResource(hash, albumIds, albumAudioIds),
            RawSongApiJsonContext.Default.ListImageResourceRequestItem);
        var parameters = new Dictionary<string, string>
        {
            ["album_image_type"] = "-3",
            ["appid"] = KuGouConfig.AppId,
            ["clientver"] = KuGouConfig.ClientVer,
            ["author_image_type"] = "3,4,5",
            ["count"] = count.ToString(),
            ["data"] = data,
            ["isCdn"] = "1",
            ["publish_time"] = "1"
        };
        parameters["signature"] = CalcAndroidRawSignature(parameters);

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Get,
            BaseUrl = "https://expendablekmr.kugou.com",
            Path = "/container/v2/image",
            Params = parameters,
            ClearDefaultParams = true,
            NotSignature = true,
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetAudioImagesAsync(string hash, string? audioIds = null, string? albumAudioIds = null,
        string? fileNames = null, int count = 5)
    {
        var data = JsonSerializer.Serialize(
            BuildAudioImageResource(hash, audioIds, albumAudioIds, fileNames),
            RawSongApiJsonContext.Default.ListAudioImageResourceRequestItem);
        var parameters = new Dictionary<string, string>
        {
            ["appid"] = KuGouConfig.AppId,
            ["clientver"] = KuGouConfig.ClientVer,
            ["count"] = count.ToString(),
            ["data"] = data,
            ["isCdn"] = "1",
            ["publish_time"] = "1",
            ["show_authors"] = "1"
        };
        parameters["signature"] = CalcAndroidRawSignature(parameters);

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Get,
            BaseUrl = "https://expendablekmr.kugou.com",
            Path = "/v2/author_image/audio",
            Params = parameters,
            ClearDefaultParams = true,
            NotSignature = true,
            SignatureType = SignatureType.Default
        });
    }

    public Task<JsonElement> GetUrlNewAsync(string hash, string? albumAudioId = null, bool freePart = false)
    {
        var session = sessionManager.Session;
        var clientTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var dfid = string.IsNullOrWhiteSpace(session.Dfid) || session.Dfid == "-" ? KgUtils.RandomString(24) : session.Dfid;
        var mid = KgUtils.CalcNewMid(dfid);
        var userId = string.IsNullOrWhiteSpace(session.UserId) ? "0" : session.UserId;

        var body = new JsonObject
        {
            ["area_code"] = "1",
            ["behavior"] = "play",
            ["qualities"] = new JsonArray(AudioQuality.Standard, AudioQuality.High, AudioQuality.Lossless,
                AudioQuality.HiRes, "multitrack", "viper_atmos", "viper_tape",
                "viper_clear", "super"),
            ["resource"] = new JsonObject
            {
                ["album_audio_id"] = albumAudioId ?? "",
                ["collect_list_id"] = "3",
                ["collect_time"] = clientTimeMs,
                ["hash"] = hash,
                ["id"] = 0,
                ["page_id"] = 1,
                ["type"] = "audio"
            },
            ["token"] = session.Token,
            ["tracker_param"] = new JsonObject
            {
                ["all_m"] = 1,
                ["auth"] = "",
                ["is_free_part"] = freePart ? 1 : 0,
                ["key"] = KgUtils.Md5($"{hash}{KuGouConfig.V5KeySalt}{KuGouConfig.AppId}{mid}{userId}"),
                ["module_id"] = 0,
                ["need_climax"] = 1,
                ["need_xcdn"] = 1,
                ["open_time"] = "",
                ["pid"] = "411",
                ["pidversion"] = "3001",
                ["priv_vip_type"] = "6",
                ["viptoken"] = session.VipToken
            },
            ["userid"] = userId,
            ["vip"] = session.VipType
        };

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Post,
            BaseUrl = "http://tracker.kugou.com",
            Path = "/v6/priv_url",
            Body = body,
            SignatureType = SignatureType.Default,
            SessionOverrides = new Dictionary<string, string> { ["dfid"] = dfid }
        });
    }

    public Task<JsonElement> GetUrlAsync(string hash, string? quality = AudioQuality.Default, string? albumId = null,
        string? albumAudioId = null, bool freePart = false, string? ppageId = null)
    {
        var session = sessionManager.Session;
        var dfid = string.IsNullOrWhiteSpace(session.Dfid) || session.Dfid == "-" ? KgUtils.RandomString(24) : session.Dfid;
        var normalizedQuality = quality is "piano" or "acappella" or "subwoofer" or "ancient" or "dj" or "surnay"
            ? $"magic_{quality}"
            : quality ?? AudioQuality.Default;

        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Get,
            Path = "/v5/url",
            Params = new Dictionary<string, string>
            {
                ["album_id"] = albumId ?? "0",
                ["area_code"] = "1",
                ["hash"] = hash.ToLowerInvariant(),
                ["ssa_flag"] = "is_fromtrack",
                ["version"] = "11430",
                ["page_id"] = "967177915",
                ["quality"] = normalizedQuality,
                ["album_audio_id"] = albumAudioId ?? "0",
                ["behavior"] = "play",
                ["pid"] = "411",
                ["cmd"] = "26",
                ["pidversion"] = "3001",
                ["IsFreePart"] = freePart ? "1" : "0",
                ["ppage_id"] = string.IsNullOrWhiteSpace(ppageId) ? "356753938,823673182,967485191" : ppageId,
                ["cdnBackup"] = "1",
                ["module"] = "",
                ["clientver"] = "11430"
            },
            SpecificRouter = "trackercdn.kugou.com",
            SignatureType = SignatureType.V5,
            SpecificDfid = dfid
        });
    }

    public Task<JsonElement> GetVideoUrlAsync(string hash)
    {
        return transport.SendAsync(new KgRequest
        {
            Method = HttpMethod.Get,
            Path = "/v2/interface/index",
            Params = new Dictionary<string, string>
            {
                ["backupdomain"] = "1",
                ["cmd"] = "123",
                ["ext"] = "mp4",
                ["ismp3"] = "0",
                ["hash"] = hash,
                ["pid"] = "1",
                ["type"] = "1"
            },
            SpecificRouter = "trackermv.kugou.com",
            SignatureType = SignatureType.V5
        });
    }

    private static List<T> SplitComma<T>(string value, Func<string, T> factory)
    {
        var items = new List<T>();
        foreach (var item in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            items.Add(factory(item));
        return items;
    }

    private static string CalcJoinedSignature(Dictionary<string, string> parameters, string salt, string separator)
    {
        return KgUtils.Md5($"{salt}{BuildSortedParamString(parameters, separator)}{salt}");
    }

    private static string BuildSortedParamString(Dictionary<string, string> parameters, string separator)
    {
        return string.Join(separator, parameters
            .AsValueEnumerable().OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{x.Key}={x.Value}").ToArray());
    }

    private static string CalcAndroidRawSignature(Dictionary<string, string> parameters)
    {
        return KgUtils.Md5($"{KuGouConfig.LiteSalt}{BuildSortedParamString(parameters, string.Empty)}{KuGouConfig.LiteSalt}");
    }

    private static List<ImageResourceRequestItem> BuildImageResource(string hash, string? albumIds, string? albumAudioIds)
    {
        var hashes = SplitValues(hash);
        var albums = SplitValues(albumIds);
        var albumAudios = SplitValues(albumAudioIds);
        var data = new List<ImageResourceRequestItem>(hashes.Length);

        for (var i = 0; i < hashes.Length; i++)
            data.Add(new ImageResourceRequestItem(
                i < albums.Length ? albums[i] : "0",
                hashes[i],
                i < albumAudios.Length ? albumAudios[i] : "0"));

        return data;
    }

    private static List<AudioImageResourceRequestItem> BuildAudioImageResource(string hash, string? audioIds, string? albumAudioIds,
        string? fileNames)
    {
        var hashes = SplitValues(hash);
        var audios = SplitValues(audioIds);
        var albumAudios = SplitValues(albumAudioIds);
        var names = SplitValues(fileNames);
        var data = new List<AudioImageResourceRequestItem>(hashes.Length);

        for (var i = 0; i < hashes.Length; i++)
            data.Add(new AudioImageResourceRequestItem(
                i < audios.Length ? audios[i] : "0",
                hashes[i],
                i < albumAudios.Length ? albumAudios[i] : "0",
                i < names.Length ? names[i] : string.Empty));

        return data;
    }

    private static string[] SplitValues(string? value)
    {
        return (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}

internal sealed record AudioLookupRequestItem(
    [property: JsonPropertyName("hash")] string Hash,
    [property: JsonPropertyName("audio_id")] int AudioId);

internal sealed record AudioLookupRequestBody(
    [property: JsonPropertyName("appid")] string AppId,
    [property: JsonPropertyName("clienttime")] long ClientTime,
    [property: JsonPropertyName("clientver")] string ClientVer,
    [property: JsonPropertyName("data")] List<AudioLookupRequestItem> Data,
    [property: JsonPropertyName("dfid")] string Dfid,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("mid")] string Mid,
    [property: JsonPropertyName("token")] string? Token,
    [property: JsonPropertyName("userid")] string? UserId);

internal sealed record AlbumAudioIdRequestItem(
    [property: JsonPropertyName("album_audio_id")] string AlbumAudioId);

internal sealed record KmrAudioMvRequestBody(
    [property: JsonPropertyName("data")] List<AlbumAudioIdRequestItem> Data,
    [property: JsonPropertyName("fields")] string Fields);

internal sealed record EntityIdRequestItem(
    [property: JsonPropertyName("entity_id")] long EntityId);

internal sealed record KmrAudioRequestBody(
    [property: JsonPropertyName("data")] List<EntityIdRequestItem> Data,
    [property: JsonPropertyName("fields")] string Fields);

internal sealed record HashOnlyRequestItem(
    [property: JsonPropertyName("hash")] string Hash);

internal sealed record PrivilegeLiteResourceItem(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("page_id")] int PageId,
    [property: JsonPropertyName("hash")] string Hash,
    [property: JsonPropertyName("album_id")] string AlbumId);

internal sealed record PrivilegeLiteRequestBody(
    [property: JsonPropertyName("appid")] string AppId,
    [property: JsonPropertyName("area_code")] int AreaCode,
    [property: JsonPropertyName("behavior")] string Behavior,
    [property: JsonPropertyName("clientver")] string ClientVer,
    [property: JsonPropertyName("need_hash_offset")] int NeedHashOffset,
    [property: JsonPropertyName("relate")] int Relate,
    [property: JsonPropertyName("support_verify")] int SupportVerify,
    [property: JsonPropertyName("resource")] List<PrivilegeLiteResourceItem> Resource,
    [property: JsonPropertyName("qualities")] List<string> Qualities);

internal sealed record ImageResourceRequestItem(
    [property: JsonPropertyName("album_id")] string AlbumId,
    [property: JsonPropertyName("hash")] string Hash,
    [property: JsonPropertyName("album_audio_id")] string AlbumAudioId);

internal sealed record AudioImageResourceRequestItem(
    [property: JsonPropertyName("audio_id")] string AudioId,
    [property: JsonPropertyName("hash")] string Hash,
    [property: JsonPropertyName("album_audio_id")] string AlbumAudioId,
    [property: JsonPropertyName("filename")] string FileName);

[JsonSerializable(typeof(AudioLookupRequestBody))]
[JsonSerializable(typeof(KmrAudioMvRequestBody))]
[JsonSerializable(typeof(KmrAudioRequestBody))]
[JsonSerializable(typeof(PrivilegeLiteRequestBody))]
[JsonSerializable(typeof(List<HashOnlyRequestItem>))]
[JsonSerializable(typeof(List<ImageResourceRequestItem>))]
[JsonSerializable(typeof(List<AudioImageResourceRequestItem>))]
internal partial class RawSongApiJsonContext : JsonSerializerContext
{
}
