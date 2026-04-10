namespace ThesisValidator.Domain.Rules;

public class CrossReferenceRule : ValidationRule
{
    public string ReferenceTarget { get; set; } = string.Empty;

    public string? SectionTitle { get; set; }
}
