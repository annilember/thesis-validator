using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Domain.Rules;

public class CountRule : ValidationRule
{
    public ERuleProperty Property { get; set; }

    public int MinValue { get; set; }

    public int? MaxValue { get; set; }

    public EUnit? Unit { get; set; }
}
