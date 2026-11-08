using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using KugouAvaloniaPlayer.Models;
using KugouAvaloniaPlayer.ViewModels;

namespace KugouAvaloniaPlayer.Services;

/// <summary>
///     本地歌单实体（持久化）。
/// </summary>
public sealed record QqLocalPlaylistEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Cover { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

/// <summary>
///     本地歌单歌曲实体（持久化）。
/// </summary>
public sealed record QqLocalPlaylistSongEntity
{
    public string SongMid { get; set; } = "";
    public string Name { get; set; } = "";
    public string Singer { get; set; } = "";
    public string AlbumMid { get; set; } = "";
    public string AlbumName { get; set; } = "";
    public double DurationSeconds { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(List<QqLocalPlaylistEntity>))]
[JsonSerializable(typeof(List<QqLocalPlaylistSongEntity>))]
[JsonSerializable(typeof(List<string>))]
internal partial class QqLocalPlaylistJsonContext : JsonSerializerContext
{
}

/// <summary>
///     QQ 客户端本地歌单服务（AppSqliteStore 持久化）。
///     上游 qq-music-api 无创建/写入歌单接口，因此"创建歌单/加入歌曲/移除/删除"全部落在本地存储，
///     歌曲用 songmid 等元数据保存，播放时走 QQ 播放地址解析。
/// </summary>
public sealed class QqLocalPlaylistService
{
    private const string StoreScope = "qq_local_playlists";
    private const string PlaylistsKey = "playlists";
    private const string SongsKeyPrefix = "songs_";
    private const string HiddenPlaylistsKey = "hidden_playlists";
    private const string OnlineRemovedSongsKeyPrefix = "online_removed_";
    private const string OnlineSongOrderKeyPrefix = "online_order_";

    /// <summary>
    ///     全部本地歌单。
    /// </summary>
    public Task<List<QqLocalPlaylistEntity>> GetAllAsync()
    {
        var json = AppSqliteStore.LoadValue(StoreScope, PlaylistsKey);
        if (string.IsNullOrWhiteSpace(json))
            return Task.FromResult(new List<QqLocalPlaylistEntity>());

        try
        {
            var list = JsonSerializer.Deserialize(json, QqLocalPlaylistJsonContext.Default.ListQqLocalPlaylistEntity) ?? [];
            list.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
            return Task.FromResult(list);
        }
        catch
        {
            return Task.FromResult(new List<QqLocalPlaylistEntity>());
        }
    }

    /// <summary>
    ///     创建本地歌单，返回新实体。
    /// </summary>
    public Task<QqLocalPlaylistEntity> CreateAsync(string name)
    {
        var cleanName = string.IsNullOrWhiteSpace(name) ? "新建歌单" : name.Trim();
        var all = GetAllAsync().GetAwaiter().GetResult();
        var newId = 1L;
        foreach (var p in all)
            if (p.Id >= newId)
                newId = p.Id + 1;

        var entity = new QqLocalPlaylistEntity
        {
            Id = newId,
            Name = cleanName,
            Cover = "",
            CreatedAt = DateTime.Now
        };
        all.Add(entity);
        SavePlaylists(all);
        return Task.FromResult(entity);
    }

    /// <summary>
    ///     重命名本地歌单。
    /// </summary>
    public Task RenameAsync(long playlistId, string name)
    {
        var all = GetAllAsync().GetAwaiter().GetResult();
        var target = all.Find(x => x.Id == playlistId);
        if (target == null || string.IsNullOrWhiteSpace(name))
            return Task.CompletedTask;

        target.Name = name.Trim();
        SavePlaylists(all);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     删除本地歌单（含歌曲）。
    /// </summary>
    public Task DeleteAsync(long playlistId)
    {
        var all = GetAllAsync().GetAwaiter().GetResult();
        var removed = all.RemoveAll(x => x.Id == playlistId) > 0;
        SavePlaylists(all);
        if (removed)
            AppSqliteStore.DeleteValue(StoreScope, SongsKey(playlistId));
        return Task.CompletedTask;
    }

    /// <summary>
    ///     本机隐藏的在线歌单、收藏专辑和“我喜欢的音乐”标识。
    /// </summary>
    public Task<HashSet<string>> GetHiddenPlaylistKeysAsync()
    {
        var json = AppSqliteStore.LoadValue(StoreScope, HiddenPlaylistsKey);
        if (string.IsNullOrWhiteSpace(json))
            return Task.FromResult(new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        try
        {
            var keys = JsonSerializer.Deserialize(json, QqLocalPlaylistJsonContext.Default.ListString) ?? [];
            return Task.FromResult(new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase));
        }
        catch
        {
            return Task.FromResult(new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }
    }

    public async Task HidePlaylistAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        var keys = await GetHiddenPlaylistKeysAsync();
        if (keys.Add(key))
            SaveHiddenPlaylistKeys(keys);
    }

    public async Task UnhidePlaylistAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        var keys = await GetHiddenPlaylistKeysAsync();
        if (keys.Remove(key))
            SaveHiddenPlaylistKeys(keys);
    }

    public static string GetPlaylistHideKey(PlaylistItem item)
    {
        if (item.IsLikePlaylist)
            return "qq:liked";

        return item.Type switch
        {
            PlaylistType.Album => $"album:{item.Id}",
            PlaylistType.Online => $"online:{item.Id}",
            _ => $"local:{item.ListId}"
        };
    }

    /// <summary>
    ///     歌单内歌曲。
    /// </summary>
    public Task<List<QqLocalPlaylistSongEntity>> GetSongsAsync(long playlistId)
    {
        var json = AppSqliteStore.LoadValue(StoreScope, SongsKey(playlistId));
        if (string.IsNullOrWhiteSpace(json))
            return Task.FromResult(new List<QqLocalPlaylistSongEntity>());

        try
        {
            return Task.FromResult(JsonSerializer.Deserialize(json, QqLocalPlaylistJsonContext.Default.ListQqLocalPlaylistSongEntity) ?? []);
        }
        catch
        {
            return Task.FromResult(new List<QqLocalPlaylistSongEntity>());
        }
    }

    /// <summary>
    ///     向本地歌单添加歌曲（去重：同 songmid 不重复添加）。
    ///     返回是否新增（false 表示已存在）。
    /// </summary>
    public Task<bool> AddSongAsync(long playlistId, SongItem song)
    {
        if (playlistId <= 0 || song == null || string.IsNullOrWhiteSpace(song.Hash))
            return Task.FromResult(false);

        var songs = GetSongsAsync(playlistId).GetAwaiter().GetResult();
        if (songs.Exists(x => x.SongMid == song.Hash))
            return Task.FromResult(false);

        songs.Add(new QqLocalPlaylistSongEntity
        {
            SongMid = song.Hash,
            Name = song.Name ?? "",
            Singer = song.Singer ?? "",
            AlbumMid = song.AlbumId ?? "",
            AlbumName = song.AlbumName ?? "",
            DurationSeconds = song.DurationSeconds
        });
        SaveSongs(playlistId, songs);
        return Task.FromResult(true);
    }

    /// <summary>
    ///     从本地歌单移除歌曲。
    /// </summary>
    public Task RemoveSongAsync(long playlistId, string songMid)
    {
        var songs = GetSongsAsync(playlistId).GetAwaiter().GetResult();
        songs.RemoveAll(x => x.SongMid == songMid);
        SaveSongs(playlistId, songs);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     按给定 songmid 顺序重排本地歌单歌曲（手动排序）。
    /// </summary>
    public Task ReorderSongsAsync(long playlistId, IReadOnlyList<string> songMids)
    {
        if (songMids.Count == 0)
            return Task.CompletedTask;

        var songs = GetSongsAsync(playlistId).GetAwaiter().GetResult();
        var orderIndex = new Dictionary<string, int>(songMids.Count, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < songMids.Count; i++)
            orderIndex.TryAdd(songMids[i], i);

        songs.Sort((a, b) =>
        {
            var ia = orderIndex.TryGetValue(a.SongMid, out var va) ? va : int.MaxValue;
            var ib = orderIndex.TryGetValue(b.SongMid, out var vb) ? vb : int.MaxValue;
            return ia.CompareTo(ib);
        });
        SaveSongs(playlistId, songs);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     向本地歌单批量添加歌曲（去重：同 songmid 不重复添加）。
    ///     返回实际新增数量。
    /// </summary>
    public Task<int> AddSongsAsync(long playlistId, IEnumerable<SongItem> songs)
    {
        if (playlistId <= 0 || songs == null)
            return Task.FromResult(0);

        var existing = GetSongsAsync(playlistId).GetAwaiter().GetResult();
        var seen = new HashSet<string>(existing.Select(x => x.SongMid), StringComparer.OrdinalIgnoreCase);
        var added = 0;
        foreach (var song in songs)
        {
            if (song == null || string.IsNullOrWhiteSpace(song.Hash) || !seen.Add(song.Hash))
                continue;

            existing.Add(new QqLocalPlaylistSongEntity
            {
                SongMid = song.Hash,
                Name = song.Name ?? "",
                Singer = song.Singer ?? "",
                AlbumMid = song.AlbumId ?? "",
                AlbumName = song.AlbumName ?? "",
                DurationSeconds = song.DurationSeconds
            });
            added++;
        }

        if (added > 0)
            SaveSongs(playlistId, existing);
        return Task.FromResult(added);
    }

    /// <summary>
    ///     从本地歌单批量移除歌曲。
    /// </summary>
    public Task<int> RemoveSongsAsync(long playlistId, IEnumerable<string> songMids)
    {
        var mids = new HashSet<string>(songMids ?? [], StringComparer.OrdinalIgnoreCase);
        if (mids.Count == 0)
            return Task.FromResult(0);

        var songs = GetSongsAsync(playlistId).GetAwaiter().GetResult();
        var before = songs.Count;
        songs.RemoveAll(x => mids.Contains(x.SongMid));
        if (songs.Count != before)
            SaveSongs(playlistId, songs);
        return Task.FromResult(before - songs.Count);
    }

    /// <summary>
    ///     记录在本机移除的在线歌单歌曲（批量）。
    /// </summary>
    public async Task<int> RemoveOnlineSongsAsync(string playlistKey, IEnumerable<string> songMids)
    {
        if (string.IsNullOrWhiteSpace(playlistKey))
            return 0;

        var removed = await GetOnlineRemovedSongsAsync(playlistKey);
        var count = 0;
        foreach (var mid in songMids ?? [])
        {
            if (!string.IsNullOrWhiteSpace(mid) && removed.Add(mid))
                count++;
        }

        if (count > 0)
        {
            AppSqliteStore.SaveValue(
                StoreScope,
                OnlineRemovedSongsKey(playlistKey),
                JsonSerializer.Serialize(removed.ToList(), QqLocalPlaylistJsonContext.Default.ListString));

            // 手动顺序里的残留项也一并清理。
            var order = await GetOnlineSongOrderAsync(playlistKey);
            var orderChanged = order.RemoveAll(x => removed.Contains(x)) > 0;
            if (orderChanged)
                SaveOnlineSongOrder(playlistKey, order);
        }

        return count;
    }

    /// <summary>
    ///     把本地歌单歌曲还原为播放器 SongItem（QqCatalog 播放源）。
    /// </summary>
    public static SongItem ToSongItem(QqLocalPlaylistSongEntity s)
    {
        var cover = string.IsNullOrWhiteSpace(s.AlbumMid)
            ? "avares://QqMusicPlayer/Assets/default_song.png"
            : $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{s.AlbumMid}.jpg";
        return new SongItem
        {
            Name = s.Name,
            Singer = s.Singer,
            Hash = s.SongMid,
            AlbumId = s.AlbumMid,
            AlbumName = s.AlbumName,
            Cover = cover,
            DurationSeconds = s.DurationSeconds,
            PlaybackSource = SongPlaybackSource.QqCatalog
        };
    }

    private static string SongsKey(long playlistId) => SongsKeyPrefix + playlistId;

    // ---- 在线歌单本地覆盖层 ----
    // 上游 qq-music-api 只读（无移除歌曲/排序接口），因此对在线歌单的
    // "从歌单移除"与"手动排序"仅保存在本机，展示时应用覆盖层，云端数据不变。

    /// <summary>
    ///     在线歌单中被本机移除的歌曲 songmid 集合。
    /// </summary>
    public Task<HashSet<string>> GetOnlineRemovedSongsAsync(string playlistKey)
    {
        var json = AppSqliteStore.LoadValue(StoreScope, OnlineRemovedSongsKey(playlistKey));
        if (string.IsNullOrWhiteSpace(json))
            return Task.FromResult(new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        try
        {
            var list = JsonSerializer.Deserialize(json, QqLocalPlaylistJsonContext.Default.ListString) ?? [];
            return Task.FromResult(new HashSet<string>(list, StringComparer.OrdinalIgnoreCase));
        }
        catch
        {
            return Task.FromResult(new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    ///     记录在本机移除的在线歌单歌曲。
    /// </summary>
    public async Task RemoveOnlineSongAsync(string playlistKey, string songMid)
    {
        if (string.IsNullOrWhiteSpace(playlistKey) || string.IsNullOrWhiteSpace(songMid))
            return;

        var removed = await GetOnlineRemovedSongsAsync(playlistKey);
        if (removed.Add(songMid))
        {
            AppSqliteStore.SaveValue(
                StoreScope,
                OnlineRemovedSongsKey(playlistKey),
                JsonSerializer.Serialize(removed.ToList(), QqLocalPlaylistJsonContext.Default.ListString));
        }

        // 手动顺序里的残留项也一并清理。
        var order = await GetOnlineSongOrderAsync(playlistKey);
        if (order.Remove(songMid))
            SaveOnlineSongOrder(playlistKey, order);
    }

    /// <summary>
    ///     在线歌单手动排序（songmid 顺序表，未包含的歌曲按云端原始顺序排在后面）。
    /// </summary>
    public Task<List<string>> GetOnlineSongOrderAsync(string playlistKey)
    {
        var json = AppSqliteStore.LoadValue(StoreScope, OnlineSongOrderKey(playlistKey));
        if (string.IsNullOrWhiteSpace(json))
            return Task.FromResult(new List<string>());

        try
        {
            return Task.FromResult(
                JsonSerializer.Deserialize(json, QqLocalPlaylistJsonContext.Default.ListString) ?? []);
        }
        catch
        {
            return Task.FromResult(new List<string>());
        }
    }

    public Task SaveOnlineSongOrderAsync(string playlistKey, IReadOnlyList<string> songMids)
    {
        SaveOnlineSongOrder(playlistKey, new List<string>(songMids));
        return Task.CompletedTask;
    }

    private void SaveOnlineSongOrder(string playlistKey, List<string> songMids)
    {
        AppSqliteStore.SaveValue(
            StoreScope,
            OnlineSongOrderKey(playlistKey),
            JsonSerializer.Serialize(songMids, QqLocalPlaylistJsonContext.Default.ListString));
    }

    private static string OnlineRemovedSongsKey(string playlistKey) => OnlineRemovedSongsKeyPrefix + playlistKey;

    private static string OnlineSongOrderKey(string playlistKey) => OnlineSongOrderKeyPrefix + playlistKey;

    private static void SaveHiddenPlaylistKeys(IEnumerable<string> keys)
    {
        AppSqliteStore.SaveValue(
            StoreScope,
            HiddenPlaylistsKey,
            JsonSerializer.Serialize(keys.ToList(), QqLocalPlaylistJsonContext.Default.ListString));
    }

    private static void SavePlaylists(List<QqLocalPlaylistEntity> list)
    {
        AppSqliteStore.SaveValue(StoreScope, PlaylistsKey, JsonSerializer.Serialize(list, QqLocalPlaylistJsonContext.Default.ListQqLocalPlaylistEntity));
    }

    private static void SaveSongs(long playlistId, List<QqLocalPlaylistSongEntity> songs)
    {
        AppSqliteStore.SaveValue(StoreScope, SongsKey(playlistId), JsonSerializer.Serialize(songs, QqLocalPlaylistJsonContext.Default.ListQqLocalPlaylistSongEntity));
    }
}
