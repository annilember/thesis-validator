using Microsoft.AspNetCore.Mvc;
using ThesisValidator.BLL.Interfaces;

namespace ThesisValidator.Api.Controllers;

[ApiController]
[Route("api/templates")]
public class TemplateController : ControllerBase
{
    private readonly IValidationEngine _validationEngine;

    public TemplateController(IValidationEngine validationEngine)
    {
        _validationEngine = validationEngine;
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _validationEngine.GetAllTemplatesAsync();
        return Ok(templates);
    }
}
