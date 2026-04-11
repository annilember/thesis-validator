using ThesisValidator.Domain.Enums;

namespace ThesisValidator.BLL.Models;

public class ValidationResult
{
    public string TemplateId { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;

    public List<ValidationIssue> Issues { get; set; } = [];

    public bool IsValid => Issues
        .Where(i => i.Severity == ERuleSeverity.Error)
        .All(i => i.Passed);
}
