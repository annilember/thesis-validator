using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Domain.Rules;

public class NumericRule : ValidationRule
{
    public string Property { get; set; } = string.Empty;

    public double ExpectedValue { get; set; }

    public EUnit Unit { get; set; }

    public double Tolerance { get; set; }
}
