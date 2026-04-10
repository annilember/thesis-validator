namespace ThesisValidator.Domain.Rules;

public class EnumRule : ValidationRule
{
    public string Property { get; set; } = string.Empty;

    public List<string> AllowedValues { get; set; } = [];
}
