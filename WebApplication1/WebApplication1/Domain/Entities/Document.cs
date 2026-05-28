namespace DocumentValidator.Domain.Entities;

public class Document
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentValue { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public int ConfidenceScore { get; set; }
    public string Country { get; set; } = string.Empty;
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    public int? ValidatedByEmployeeId { get; set; }
    public Employee? ValidatedByEmployee { get; set; }
}
