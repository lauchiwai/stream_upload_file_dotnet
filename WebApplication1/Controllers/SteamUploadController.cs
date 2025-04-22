using Microsoft.AspNetCore.Mvc;
using WebApplication1.Filters;
using WebApplication1.Services.Interfaces;
namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SteamUploadController : ControllerBase
{
    private readonly ILocalUploadService _steamUploadServices;
    public SteamUploadController(ILocalUploadService steamUploadServices)
    {
        _steamUploadServices = steamUploadServices;
    }

    [HttpPost]
    [Route("UploadFile")]
    [DisableFormValueModelBindingFilter]
    public async Task<IActionResult> UploadFile()
    {
        var result = await _steamUploadServices.UploadFileAsync(Request);
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
        }
    }
}
