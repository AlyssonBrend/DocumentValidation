using DocumentValidator.Api.Models;
using DocumentValidator.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentValidator.Api.Controllers;

[ApiController]
[Route("api/validation")]
public class ValidationController : ControllerBase
{
    private readonly ValidationService _service;

    public ValidationController(ValidationService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Validate([FromBody] ValidateRequest request)
    {
        var result = _service.Validate(request.Type, request.Value);

        return Ok(result);
    }
}