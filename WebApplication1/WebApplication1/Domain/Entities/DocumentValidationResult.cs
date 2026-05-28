namespace DocumentValidator.Domain.Entities;

public class DocumentValidationResult
{
    public bool IsValid { get; set; }
    public int ConfidenceScore { get; set; }
    public List<string> Errors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}
