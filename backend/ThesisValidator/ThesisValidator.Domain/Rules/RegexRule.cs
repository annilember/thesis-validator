using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Domain.Rules;

public class RegexRule : ValidationRule
{
    public ERuleProperty Property { get; set; }

    public string Pattern { get; set; } = string.Empty;
}
