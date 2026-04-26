using ThesisValidator.Domain;

namespace ThesisValidator.DAL.Interfaces;

public interface IRuleRepository
{
    Task<IEnumerable<ValidationTemplate>> GetAllTemplatesAsync();

    Task<ValidationTemplate?> GetTemplateAsync(string templateId);
}
