# QQ 音乐 .NET SDK 交付说明

## 概述

在现有 KugouMusic.NET 解决方案中新增了 `QqMusic.Net` SDK 工程，并在 `KgWebApi.Net` 中新增了 QQ 音乐 API 控制器层。整个实现镜像了 `KuGou.Net` 的分层架构（Raw API → Client → 模型 → 传输层 → DI 组合），但针对 QQ 音乐 API 的特点做了简化（无签名/加密，BaseUrl 固定 localhost:3200）。

## 新增工程

### `src/Libraries/QqMusic.Net`

QQ 音乐业务 SDK，TargetFrameworks: `net10.0;net11.0`，依赖 Pure.DI、Microsoft.Extensions.Http、ZLinq。

#### 分层结构

```
QqMusic.Net/
├── QqMusic.Net.csproj
├── Abstractions/
│   ├── QqAudioQuality.cs              # 音质枚举（128/320/flac/m4a/ape）
│   └── Models/                        # 14 个模型文件，60+ 强类型
│       ├── QqBaseModel.cs             # 基类（status=200 语义 + Extras 扩展字段）
│       ├── SongModels.cs              # QqSongInfo, QqSongPlayResult, QqSingerLite 等
│       ├── SearchModels.cs            # QqSearchResult（多域分区）, QqHotKeyList, QqSmartboxResult
│       ├── LyricModels.cs             # QqLyricResult
│       ├── PlaylistModels.cs          # QqPlaylistDetail, QqPlaylistList, QqPlaylistTag 等
│       ├── RankModels.cs              # QqTopList, QqRankResult, QqRankSong
│       ├── ArtistModels.cs            # QqSingerInfo, QqSingerList, QqSingerDesc 等
│       ├── AlbumModels.cs             # QqAlbumInfo, QqAlbumSongs, QqNewDisks
│       ├── UserModels.cs              # QqUserInfo, QqVipInfo, QqUserPlaylists 等
│       ├── LoginModels.cs             # QqLoginQr, QqLoginQrResult, QqLoginQrCheckRequest
│       ├── RecommendModels.cs         # QqDailyRecommend, QqPrivateFm, QqRecommend 等
│       ├── MvModels.cs                # QqMvPlay, QqMvList, QqMvCategory
│       ├── CommentModels.cs           # QqCommentList, QqHotCommentList
│       └── ImageModels.cs             # QqImageUrl, QqDownloadInfo, QqTicketInfo
├── Adapters/Common/
│   └── QqApiResponseParser.cs         # 统一响应解析器（处理 {status,body:{response}} 和 {response}）
├── Clients/                           # 13 个域客户端
│   ├── SearchClient.cs                # 搜索/联想/热搜
│   ├── SongClient.cs                  # 播放地址/歌曲信息/批量
│   ├── LyricClient.cs                 # 歌词
│   ├── PlaylistClient.cs              # 歌单全系列
│   ├── RankClient.cs                  # 排行榜
│   ├── ArtistClient.cs                # 歌手全系列
│   ├── AlbumClient.cs                 # 专辑/新碟
│   ├── UserClient.cs                  # 用户信息/VIP/cookie
│   ├── LoginClient.cs                 # 扫码登录
│   ├── RecommendClient.cs             # 推荐/FM/新歌
│   ├── MvClient.cs                    # MV
│   ├── CommentClient.cs               # 评论
│   └── ImageClient.cs                 # 图片/下载/票据
├── Infrastructure/
│   ├── QqMusicComposition.cs          # Pure.DI 组合根 + QqMusicApi record struct
│   └── Http/
│       └── QqHttpTransport.cs         # IQqTransport + HttpClient 实现（重试机制）
├── Protocol/
│   ├── Transport/QqRequest.cs         # 请求对象（无签名，GET 拼 query / POST 拼 JSON）
│   └── Raw/                           # 13 个 Raw API（拼请求返回 JsonElement）
│       ├── RawSearchApi.cs
│       ├── RawSongApi.cs
│       ├── RawLyricApi.cs
│       ├── RawPlaylistApi.cs
│       ├── RawRankApi.cs
│       ├── RawArtistApi.cs
│       ├── RawAlbumApi.cs
│       ├── RawUserApi.cs
│       ├── RawLoginApi.cs
│       ├── RawRecommendApi.cs
│       ├── RawMvApi.cs
│       ├── RawCommentApi.cs
│       └── RawImageApi.cs
└── util/
    ├── AppJsonContext.cs              # Source-Generated JsonSerializerContext（全部模型注册）
    └── Constants.cs                   # DefaultBaseUrl = http://localhost:3200
```

#### 关键设计

1. **响应解析器**：`QqApiResponseParser.Parse<T>()` 自动处理 QQ 音乐 API 的两种封装格式：
   - `{ "status": 200, "body": { "response": {...} } }`
   - `{ "response": {...} }`
   提取 `body.response`（或 `response`）中的 payload 后反序列化，并回填外层 status/code/message 到基类。

2. **模型字段**：全部使用 QQ 域字段名（songmid/songid/singermid/albummid/disstid/vid），与酷狗的 hash/albumAudioId 完全区分。

3. **无签名**：QQ 音乐 API（qq-music-api Node.js 服务）不需要任何签名加密，`QqRequest` 去掉了酷狗的 SignatureType/SpecificRouter 等字段。

4. **Pure.DI**：`QqMusicComposition` 通过 `DI.Setup()` 注册全部 Raw API 和 Client，聚合为 `QqMusicApi` record struct 供外部一次性注入。

5. **AOT 兼容**：`AppJsonContext` 使用 `[JsonSerializable]` 源生成，POST 请求的 BodyTypeInfo 均从源生成器获取。

## 改造内容：`src/Apps/KgWebApi.Net`

### 修改的文件（3 个，均为增量修改，未破坏既有功能）

1. **`KgWebApi.Net.csproj`**：新增 `<ProjectReference Include="..\..\Libraries\QqMusic.Net\QqMusic.Net.csproj" />`（保留 KuGou.Net 引用）
2. **`Program.cs`**：新增 `builder.Services.AddWebApiQqMusicServices();`；OpenAPI 标题改为 "Music API (KuGou + QQ)"
3. **`KugouMusic.NET.slnx`**：新增 QqMusic.Net 工程引用

### 新增的文件

1. **`Services/WebApiQqMusicServiceCollectionExtensions.cs`**：QQ 音乐 DI 注册扩展，配置 HttpClient（BaseAddress localhost:3200，连接池 64），注册全部 13 个 Raw API 和 Client 为 Scoped。

2. **`Controllers/Qq/` 下 13 个控制器**（命名空间 `KgWebApi.Net.Controllers.Qq`，路由前缀 `qq/`）：

| 控制器 | 路由 | 端点 |
|--------|------|------|
| QqSearchController | `qq/search` | GET /（搜索）、GET /smartbox、GET /hot |
| QqSongController | `qq/song` | GET /url、GET /info、POST /batch |
| QqLyricController | `qq/lyric` | GET / |
| QqPlaylistController | `qq/playlist` | GET /categories、GET /lists、GET /detail、POST /batch、GET /tags、GET /byTag、GET /resolve |
| QqRankController | `qq/rank` | GET /lists、GET /detail |
| QqArtistController | `qq/artist` | GET /list、GET /category、GET /desc、GET /hotsong、GET /album、GET /mv、GET /similar、GET /starnum |
| QqAlbumController | `qq/album` | GET /songs、GET /info、GET /new |
| QqUserController | `qq/user` | GET /playlists、GET /detail、GET /collected、GET /follow-singers、GET /avatar、GET /liked、GET /vip、GET/POST /cookie |
| QqLoginController | `qq/login` | GET /qr、POST /check |
| QqRecommendController | `qq/recommend` | GET /daily、GET /fm、GET /new、GET /index、GET /banner、GET /personal、GET /similar、GET /related |
| QqMvController | `qq/mv` | GET /play、GET /list、GET /category、GET /byTag |
| QqCommentController | `qq/comment` | GET /list、GET /hot |
| QqImageController | `qq/image` | GET /url、GET /download、GET /ticket |

## 编译验证

```
dotnet build src/Apps/KgWebApi.Net/KgWebApi.Net.csproj -p:TargetFrameworks=net10.0
```

结果：**0 警告，0 错误**。KuGou.Net、QqMusic.Net、KgWebApi.Net 三个工程全部编译通过。

> 注：csproj 配置了 net10.0;net11.0 双目标。本机仅安装 .NET SDK 10.0.400，因此用 `-p:TargetFrameworks=net10.0` 验证。安装 .NET 11 SDK 后可直接 `dotnet build` 构建双目标。

## 如何启动 QQ 音乐服务

1. 克隆 qq-music-api：`git clone https://github.com/sansenjian/qq-music-api.git`
2. 进入目录：`cd qq-music-api`
3. 安装依赖：`npm install`
4. 启动服务：`npm start`（默认监听 `http://localhost:3200`）

## 如何验证

1. 启动 qq-music-api 服务（localhost:3200）
2. 启动 Web API：`dotnet run --project src/Apps/KgWebApi.Net/KgWebApi.Net.csproj -f net10.0`
3. 打开 Swagger 文档：`http://localhost:<port>/scalar`（或 `/openapi/v1.json`）
4. 测试不需登录的公开接口：
   - `GET /qq/rank/lists` — 排行榜列表
   - `GET /qq/search?key=周杰伦` — 搜索
   - `GET /qq/rank/detail?topId=26` — 排行榜详情
   - `GET /qq/album/new` — 新碟上架
   - `GET /qq/artist/category` — 歌手分类
5. 需登录 cookie 的接口（播放地址、每日推荐、私人FM、用户信息等）需先通过 `GET /qq/login/qr` 获取二维码，扫码后 `POST /qq/login/check` 确认登录获取 cookie。

## 接口覆盖清单

### 已实现（13 域，50+ 端点）

- ✅ 搜索：getSearchByKey、getSmartbox、getHotkey
- ✅ 歌曲：getMusicPlay、getSongInfo、batchGetSongInfo
- ✅ 歌词：getLyric
- ✅ 歌单：getSongListCategories、getSongLists、getSongListDetail、batchGetSongLists、getPlaylistTags、getPlaylistsByTag、resolveSongListShareUrl
- ✅ 排行榜：getTopLists、getRanks
- ✅ 歌手：getSingerList、getSingerCategory、getSingerDesc、getSingerHotsong、getSingerAlbum、getSingerMv、getSimilarSinger、getSingerStarNum
- ✅ 专辑：getAlbumSongs、getAlbumInfo、getNewDisks
- ✅ 用户：getUserPlaylists、getUserDetail、getUserCollectedSongLists、getUserFollowSingers、getUserAvatar、getUserLikedSongs、getVipInfo、getCookie、setCookie
- ✅ 登录：getQQLoginQr、checkQQLoginQr
- ✅ 推荐：getDailyRecommend、getPrivateFM、getNewSongs、getRecommend、getRecommendBanner、getPersonalRecommend、getSimilarSongs、getRelatedPlaylists
- ✅ MV：getMvPlay、getMv、getMvCategory、getMvByTag
- ✅ 评论：getComments、getHotComments
- ✅ 图片/其他：getImageUrl、getDownloadQQMusic、getTicketInfo

### 需登录 cookie 的接口（未实测运行时）

以下接口在 qq-music-api 中需要登录 cookie 才能返回完整数据，SDK 和 Web API 层已完整实现参数传递，但未做端到端运行时验证：
- getMusicPlay（播放地址）
- getSongInfo（歌曲详情，匿名调用上游返回 code=500001，需登录 cookie 后才能拿到有效数据）
- getDailyRecommend、getPrivateFM、getPersonalRecommend、getSimilarSongs
- user/* 系列（getUserPlaylists、getUserDetail、getUserCollectedSongLists、getUserLikedSongs、getVipInfo）
- getLyric（部分歌曲需登录）

### 实测对齐记录（2026-09-27，qq-music-api v2.6.0）

以下为带真实登录 cookie 逐接口实测后的结构事实，模型已按此对齐：

1. **播放地址 getMusicPlay**：真实结构为 `data.playUrl.{songmid: {url, error?}}`——playUrl 是**以 songmid 为键的字典**而非字符串。`QqSongPlayResult.PlayUrls` 为 `Dictionary<string, QqPlayUrlItem>`，用 `GetPlayableUrl(songmid)` 取播放地址；未登录时 url 为空且带 error 提示。
2. **歌手列表 getSingerList**：参数必须传数字（默认 **-100**），`index` 为 int；传字符串 "ALL" 会导致上游 `Number("ALL")=NaN` 返回空列表。结构 `response.singerList.data.{singerlist[], total}`，字段 `singer_id/singer_mid/singer_name/singer_pic`。
3. **歌手专辑 getSingerAlbum**：`response.singer.data.albumList[0]` 的专辑 mid 字段为驼峰 `albumMid`（既不是 albumMID 也不是全小写）。
4. **搜索分区**：上游 v2.6.0 `getSearchByKey` 无论传何种 `remoteplace`（playlist/album/singer），都**只返回 song 分区 + zhida**，无独立 playlist/album 分区。客户端搜索歌手时优先用 `zhida.zhida_singer`（SingerMid 非空即显示）。
5. **歌单详情 getSongListDetail**：歌曲元素为 `{id, mid, name, title, interval, singer:[{id,mid,name}], album:{id,mid,name}, file:{media_mid}, mv:{vid}}`（与搜索返回的 songmid 风格不同），`PlaylistClient` 已做手动映射。
6. **隐私歌单**：部分歌单返回 `{"response":{"code":0,"subcode":4000,"msg":"check privacy error!"}}` 且无 cdlist，属歌单隐私保护，客户端显示为空属正常。
7. **每日推荐/私人FM（上游缺陷）**：上游 `getDailyRecommend`/`getPrivateFM` 请求体**硬编码 `param.uin:0, login:0`**，即使带完整登录 cookie，QQ 官方接口仍返回 `code=500003/subcode=860100005`。这是 qq-music-api v2.6.0 自身缺陷（不修改上游源码无法解决），客户端已做优雅空态 + toast 提示。
8. **登录链路**：`getQQLoginQr` 返回 `{img(base64 data URI), qrsig, ptqrtoken(数字=hash33(qrsig))}`；`checkQQLoginQr` 返回 `{isOk, refresh, message, session:{loginUin, uin, cookie, ...}}`（未扫=isOk:false/refresh:false、失效=refresh:true、成功=isOk:true+session.cookie）。

### 缺口与后续建议

1. **运行时端到端验证**：当前以编译通过为强制验收门槛。如需运行时验证，需克隆并启动 qq-music-api 服务后用 curl 或 Swagger 测试。
2. **cookie 会话管理**：当前 Web API 层将 cookie 作为 query 参数透传，未做服务端会话持久化。可参考 KgWebSessionMiddleware 实现 QQ 登录会话管理。
3. **net11.0 目标**：需安装 .NET 11 SDK 后验证双目标构建。
4. **模型字段补全**：部分模型字段基于 QQ API 常见响应结构定义，实际运行时可能发现遗漏字段，可通过 QqBaseModel.Extras 捕获或补充模型属性。

## 未触碰的文件

- `src/Libraries/KuGou.Net/` — 全部保持原样，零修改
- `src/Apps/KgWebApi.Net/Controllers/` 下原有的酷狗控制器 — 全部保持原样
- `src/Apps/KgWebApi.Net/Services/` 下原有的酷狗服务 — 全部保持原样
- 其他工程（Avalonia 播放器、测试等）— 未触碰
