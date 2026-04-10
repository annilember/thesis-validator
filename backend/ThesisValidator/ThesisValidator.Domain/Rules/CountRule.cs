namespace ThesisValidator.Domain.Rules;

public class CountRule : ValidationRule
{
    public string Property { get; set; } = string.Empty;

    public int MinValue { get; set; }

    public int? MaxValue { get; set; }
}
