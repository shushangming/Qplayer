using KuGou.Net.Clients;
using Microsoft.AspNetCore.Mvc;

namespace KgWebApi.Net.Controllers;

[ApiController]
[Route("register")]
public class RegisterController(RegisterClient registerClient) : ControllerBase
{
    /// <summary>
    ///     初始化设备标识。
    /// </summary>
    /// <returns>设备初始化是否成功。</returns>
    [HttpGet("dev")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> UserDetail()
    {
        var result = await registerClient.InitDeviceAsync();
        return Ok(result);
    }
}
