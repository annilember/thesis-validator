using ThesisValidator.BLL.Models;
using ThesisValidator.Domain;

namespace ThesisValidator.BLL.Interfaces;

public interface IDocumentValidator
{
    bool CanValidate(string fileExtension);
    Task<ValidationResult> ValidateAsync(Stream document, IEnumerable<ValidationRule> rules);
}
