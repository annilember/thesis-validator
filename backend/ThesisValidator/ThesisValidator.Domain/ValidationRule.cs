using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Domain;

public abstract class ValidationRule
{
    public string RuleId { get; set; } = string.Empty;

    public string RuleName { get; set; } = string.Empty;

    public ERuleType Type { get; set; }

    public bool Enabled { get; set; }

    public ERuleTarget Target { get; set; }

    public string? SectionTitle { get; set; }

    public string? StartSectionTitle { get; set; }

    public string? EndSectionTitle { get; set; }

    public string? AfterSectionTitle { get; set; }

    public string? BeforeSectionTitle { get; set; }

    public List<string>? StyleFilters { get; set; }

    public List<string>? FontFilters { get; set; }

    public List<string>? ExcludeFontFilters { get; set; }

    public ERuleSeverity Severity { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public EThesisType? ThesisType { get; set; }

    public ESupportedLanguage? Language { get; set; }

    public ESupportedLanguage? CurriculumLanguage { get; set; }
}
