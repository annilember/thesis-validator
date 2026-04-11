using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Domain.Rules;

public class BooleanRule : ValidationRule
{
    public ERuleProperty Property { get; set; }

    public bool ExpectedValue { get; set; }
}
