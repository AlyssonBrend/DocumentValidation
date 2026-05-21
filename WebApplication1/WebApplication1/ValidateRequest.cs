using DocumentValidator.Domain.Enums;

namespace DocumentValidator.Api.Models;

public class ValidateRequest
{
    public DocumentType Type { get; set; }

    public string Value { get; set; } = string.Empty;
}