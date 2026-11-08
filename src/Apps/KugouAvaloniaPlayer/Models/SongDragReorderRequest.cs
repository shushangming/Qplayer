using System;
using KugouAvaloniaPlayer.ViewModels;

namespace KugouAvaloniaPlayer.Models;

/// <summary>
///     歌单内拖拽排序请求：把 <see cref="Song"/> 移动到锚点歌曲 <see cref="Anchor"/> 的
///     上方或下方。用锚点歌曲而非行索引传递位置，搜索过滤/自动排序时视图顺序
///     与底层顺序不一致也能正确落位。
/// </summary>
public sealed record SongDragReorderRequest(
    SongItem Song,
    SongItem Anchor,
    bool InsertAfter)
{
    public bool IsValid => Song is not null && Anchor is not null && !ReferenceEquals(Song, Anchor);
}
