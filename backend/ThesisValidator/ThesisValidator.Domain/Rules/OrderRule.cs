namespace ThesisValidator.Domain.Rules;

public class OrderRule : ValidationRule
{
    public string OrderType { get; set; } = string.Empty;

    public List<string>? ExpectedOrder { get; set; }

    public int? StartValue { get; set; }
}
