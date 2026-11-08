# KuGou.Net 听歌识曲使用说明

本文说明 `KuGou.Net` 里已经接好的听歌识曲能力，包括：

- SDK 内部如何直接调用 `SongClient`
- `KgWebApi.Net` 暴露出的 HTTP 接口怎么测
- 当前接口对音频格式的要求

## 入口概览

当前听歌识曲能力分成两层：

- SDK 层：`KuGou.Net.Clients.SongClient.GetAudioMatchAsync(byte[] pcmData)`
- WebApi 层：`POST /audio/match`

SDK 层返回强类型 `AudioMatchResponse`，主要模型位于：

- `Abstractions/Models/AudioMatchResponse.cs`

WebApi 层当前支持两种请求体：

- `application/octet-stream`
- `multipart/form-data`

两种方式最终都会落到同一个 `SongClient.GetAudioMatchAsync`。

## SDK 调用

推荐直接通过 DI 获取 `SongClient`：

```csharp
using KuGou.Net.Clients;
using KuGou.Net.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging();
services.AddKuGouSdk();

await using var provider = services.BuildServiceProvider();

var songClient = provider.GetRequiredService<SongClient>();
var pcmBytes = await File.ReadAllBytesAsync("sample.pcm");

var result = await songClient.GetAudioMatchAsync(pcmBytes);

if (result?.Status == 1 && result.Data.Count > 0)
{
    var first = result.Data[0];
    Console.WriteLine($"{first.SongName} - {first.SingerName}");
    Console.WriteLine(first.UnionCover);
}
```

如果你不走 DI，也可以在手动创建 `RawSongApi` 后再创建 `SongClient`，方式和 `README.md` 里的其他 Client 一样。

## 返回类型

`AudioMatchResponse` 当前已经按真实返回建了常用字段，适合直接在业务里用：

顶层字段：

- `Status`
- `ErrorCode`
- `ServerTime`
- `PcmSecond`
- `Process`
- `Data`

单条结果常用字段：

- `SongId`
- `MixSongId`
- `SongName`
- `SongNameSuffix`
- `SingerName`
- `UnionCover`
- `Hash128`
- `Hash320`
- `Distance`
- `DistanceNoMelody`
- `TimeOffset`
- `TimeLength128`
- `Album`
- `Authors`

接口还有不少长尾字段没有完全强类型化，这些字段仍然会保留在 `Extras` 里。

## 音频格式要求

当前这条识曲链路按“原始 PCM 直传”接入，推荐输入格式：

- 单声道 `mono`
- `16-bit PCM`
- `8000 Hz`

也就是常见的 `pcm_s16le`。

如果你手上是 `flac/mp3/wav`，建议先转成 PCM 再送给 SDK 或 WebApi。

示例 `ffmpeg`：

```bash
ffmpeg -i input.flac -ac 1 -ar 8000 -f s16le output.pcm
```

## WebApi 调用

`KgWebApi.Net` 暴露的是：

```text
POST /audio/match
```

### 方式一：`application/octet-stream`

适合直接发送原始 PCM 字节：

```bash
curl.exe -s -X POST "http://localhost:5058/audio/match" ^
  -H "Content-Type: application/octet-stream" ^
  --data-binary "@sample.pcm"
```

### 方式二：`multipart/form-data`

适合浏览器表单、Swagger 或 Postman 上传文件：

```bash
curl.exe -s -X POST "http://localhost:5058/audio/match" ^
  -F "file=@sample.pcm;type=application/octet-stream"
```

表单方式里，文件字段名必须是：

```text
file
```

## 当前实现说明

这条接口不是简单复用仓库里默认的 lite 参数，而是单独按识曲接口的实际请求形状处理：

- 使用 official Android 风格参数
- 按二进制原始字节参与签名
- 直接发送 `application/octet-stream`

因此如果你后面要继续改识曲相关逻辑，优先看这些位置：

- `Protocol/Raw/RawSongApi.cs`
- `Clients/SongClient.cs`
- `Abstractions/Models/AudioMatchResponse.cs`
- `util/KGSigner.cs`
- `Infrastructure/Http/Handlers/SignatureHandler.cs`

## 注意事项

- 当前输入默认按 PCM 处理，不会自动帮你解码 `wav/mp3/flac`。
- `album` 字段在真实返回里可能是数组，也可能是空对象；当前模型已经做了兼容转换。
- 识曲结果通常会返回多条候选，业务层不要只依赖第一条以外的字段完全稳定。
- 如果你只是调 WebApi，本地浏览器页面更推荐用 `multipart/form-data`；如果是 Flutter/Android 直传 PCM，继续用 `application/octet-stream` 更直接。

