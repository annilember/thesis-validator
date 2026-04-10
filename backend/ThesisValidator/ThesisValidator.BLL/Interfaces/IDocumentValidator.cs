using ThesisValidator.BLL.Models;
using ThesisValidator.Domain;
using ThesisValidator.Domain.Enums;

namespace ThesisValidator.BLL.Interfaces;

public interface IDocumentValidator
{
    bool CanValidate(string fileExtension);
    Task<ValidationResult> ValidateAsync(Stream document, ValidationTemplate template, ESupportedLanguage language);
}
