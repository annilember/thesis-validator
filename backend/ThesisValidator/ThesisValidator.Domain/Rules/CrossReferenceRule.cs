using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Domain.Rules;

public class CrossReferenceRule : ValidationRule
{
    public EReferenceTarget ReferenceTarget { get; set; }
}
