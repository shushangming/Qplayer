using KuGou.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using KuGou.Net.Abstractions.Models;

namespace KgWebApi.Net.Controllers;

[ApiController]
[Route("fm")]
public class FmController(FmClient fmClient) : ControllerBase
{
    /// <summary>
    ///     电台 - 推荐。
    /// </summary>
    /// <returns>推荐电台列表。</returns>
    [HttpGet("recommend")]
    [ProducesResponseType(typeof(FmRecommendResponse), 200)]
    public async Task<IActionResult> GetRecommend()
    {
        return Ok(await fmClient.GetRecommendAsync());
    }

    /// <summary>
    ///     电台 - 音乐列表。
    /// </summary>
    /// <param name="fmid">fmid。</param>
    /// <param name="type">fmtype。</param>
    /// <param name="offset">fmoffset。歌曲列表大小</param>
    /// <param name="size">fmsize。歌曲列表大小</param>
    /// <returns>电台音乐列表。</returns>
    [HttpGet("songs")]
    [ProducesResponseType(typeof(FmSongResponse), 200)]
    public async Task<IActionResult> GetSongs(
        [FromQuery][Required(AllowEmptyStrings = false)] string fmid,
        [FromQuery] int type = 2,
        [FromQuery] int offset = -1,
        [FromQuery] int size = 20)
    {
        return Ok(await fmClient.GetSongsAsync(fmid, type, offset, size));
    }

    /// <summary>
    ///     电台。（会返回所有电台数据，返回的json特别大，不建议使用）
    /// </summary>
    /// <returns>所有电台数据。</returns>
    [HttpGet("class")]
    public async Task<IActionResult> GetClassSong()
    {
        return Ok(await fmClient.GetClassSongAsync());
    }

    /// <summary>
    ///     电台 - 图片。
    /// </summary>
    /// <param name="fmid">fmid，可以传多个。</param>
    /// <returns>对应电台的图片。</returns>
    [HttpGet("image")]
    [ProducesResponseType(typeof(FmImageResponse), 200)]
    public async Task<IActionResult> GetImages([FromQuery][Required(AllowEmptyStrings = false)] string fmid)
    {
        return Ok(await fmClient.GetImagesAsync(fmid));
    }
}
