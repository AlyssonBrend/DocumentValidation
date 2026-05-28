using DocumentValidator.Application.Services;
using DocumentValidator.Domain.Entities;
using DocumentValidator.Domain.Enums;
using DocumentValidator.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocumentValidator.Api.Controllers;

[ApiController]
[Route("api/validation")]
public class ValidationController : ControllerBase
{
    private readonly ValidationService _service;
    private readonly AppDbContext _db;

    public ValidationController(ValidationService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Validate([FromBody] ValidateRequest request)
    {
        var result = _service.Validate(request.Type, request.Value);

        // Persist the validation record
        int? employeeId = null;
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (idClaim is not null && int.TryParse(idClaim, out int eid))
            employeeId = eid;

        _db.Documents.Add(new Document
        {
            DocumentType = request.Type.ToString(),
            DocumentValue = request.Value,
            IsValid = result.IsValid,
            ConfidenceScore = result.ConfidenceScore,
            Country = request.Country ?? string.Empty,
            ValidatedByEmployeeId = employeeId
        });

        await _db.SaveChangesAsync();

        return Ok(result);
    }
}

public class ValidateRequest
{
    public DocumentType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? Country { get; set; }
}
