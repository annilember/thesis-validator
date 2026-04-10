using ThesisValidator.Domain.Enums;

namespace ThesisValidator.DAL;

public interface IDocumentParsingService
{
    Task<double?> GetPageMarginLeftAsync(Stream document, EUnit ruleUnit);
}
