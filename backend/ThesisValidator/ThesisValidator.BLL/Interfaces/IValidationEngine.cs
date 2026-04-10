using ThesisValidator.BLL.Models;
using ThesisValidator.Domain.Enums;

namespace ThesisValidator.BLL.Interfaces;

public interface IValidationEngine
{
    Task<ValidationResult> ValidateAsync(
        Stream document,
        string fileName,
        ESupportedLanguage language,
        string templateId);
}
