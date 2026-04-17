using ThesisValidator.Domain.Enums;

namespace ThesisValidator.BLL.Models;

public class ValidationIssue
{
    public string RuleId { get; set; } = string.Empty;

    public bool Passed { get; set; }

    public bool Skipped { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Details { get; set; }

    public ERuleSeverity? Severity { get; set; }

    public static ValidationIssue CreateSkipped(string ruleId, string message, string reason) => new()
    {
        RuleId = ruleId,
        Skipped = true,
        Message = message,
        Details = reason
    };

    public static ValidationIssue CreatePassed(string ruleId, string description) => new()
    {
        RuleId = ruleId,
        Passed = true,
        Message = description
    };

    public static ValidationIssue CreateFailed(
        string ruleId,
        string message,
        ERuleSeverity severity,
        string? details = null) => new()
    {
        RuleId = ruleId,
        Passed = false,
        Severity = severity,
        Message = message,
        Details = details
    };
}
