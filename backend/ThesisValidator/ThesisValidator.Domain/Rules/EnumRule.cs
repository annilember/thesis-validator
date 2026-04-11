using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Domain.Rules;

public class EnumRule : ValidationRule
{
    public ERuleProperty Property { get; set; }

    public List<string> AllowedValues { get; set; } = [];
}
