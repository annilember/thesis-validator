namespace ThesisValidator.Domain.Rules;

public class RegexRule : ValidationRule
{
    public string Property { get; set; } = string.Empty;

    public string Pattern { get; set; } = string.Empty;
}
