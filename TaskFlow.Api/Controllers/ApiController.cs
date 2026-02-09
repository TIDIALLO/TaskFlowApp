using Microsoft.AspNetCore.Mvc;
using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    /// <summary>
    /// Convertit un Result en IActionResult
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return HandleError(result.Error);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok();

        return HandleError(result.Error);
    }

    private IActionResult HandleError(Error error)
    {
        return error.Code switch
        {
            "User.NotFound" => NotFound(new { error.Code, error.Message }),
            _ when error.Code.Contains("Empty") => BadRequest(new { error.Code, error.Message }),
            _ when error.Code.Contains("Invalid") => BadRequest(new { error.Code, error.Message }),
            _ when error.Code.Contains("Exists") => Conflict(new { error.Code, error.Message }),
            _ => BadRequest(new { error.Code, error.Message })
        };
    }
}