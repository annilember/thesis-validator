using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Domain.Rules;

public class OrderRule : ValidationRule
{
    public EOrderType OrderType { get; set; }

    public List<string>? ExpectedOrder { get; set; }

    public int? StartValue { get; set; }
}
