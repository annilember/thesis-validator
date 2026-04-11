using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Domain;

public abstract class ValidationRule
{
    public string RuleId { get; set; } = string.Empty;

    public ERuleType Type { get; set; }

    public bool Enabled { get; set; }

    public ERuleTarget Target { get; set; }

    public List<string>? StyleFilters { get; set; }

    public ERuleSeverity Severity { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public EThesisType? ThesisType { get; set; }

    public ESupportedLanguage? Language { get; set; }

    public ESupportedLanguage? CurriculumLanguage { get; set; }
}
