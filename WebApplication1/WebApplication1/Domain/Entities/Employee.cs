namespace DocumentValidator.Domain.Entities;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = "Analyst";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Document> ValidatedDocuments { get; set; } = [];
}
