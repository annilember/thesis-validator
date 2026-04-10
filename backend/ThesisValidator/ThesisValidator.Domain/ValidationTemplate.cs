namespace ThesisValidator.Domain;

public class ValidationTemplate
{
    public string TemplateId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public List<ValidationRule> Rules { get; set; } = [];
}
