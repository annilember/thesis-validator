using ThesisValidator.Domain;

namespace ThesisValidator.DAL;

public interface IRuleRepository
{
    Task<ValidationTemplate?> GetTemplateAsync(string templateId);
}
