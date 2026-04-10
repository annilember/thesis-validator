using ThesisValidator.Domain.Enums;

namespace ThesisValidator.BLL.Models;

public class ValidationResult
{
    // TODO: remove string.Empty where not needed explicitly.
    public string TemplateId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public List<ValidationIssue> Issues { get; set; } = [];

    public bool IsValid => Issues
        .Where(i => i.Severity == ERuleSeverity.Error)
        .All(i => i.Passed);
}
