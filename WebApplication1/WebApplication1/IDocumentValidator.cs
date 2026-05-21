using DocumentValidator.Domain.Enums;
namespace DocumentValidator.Aplication.Interfaces
{
    public interface IDocumentValidator
    {
        DocumentValidatorResult Validate(string documentNumber, DocumentType documentType);
    }
}