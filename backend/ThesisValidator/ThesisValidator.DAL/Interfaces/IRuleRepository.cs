using ThesisValidator.Domain;

namespace ThesisValidator.DAL.Interfaces;

public interface IRuleRepository
{
    Task<ValidationTemplate?> GetTemplateAsync(string templateId);
}
