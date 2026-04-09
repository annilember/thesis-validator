using Microsoft.AspNetCore.Mvc;

namespace ThesisValidator.Api.Controllers;

[ApiController]
[Route("api/validate")]
public class ValidationController : ControllerBase
{
    [HttpPost]
    public IActionResult Validate(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file received" });

        return Ok(new { status = "received", fileName = file.FileName });
    }
}