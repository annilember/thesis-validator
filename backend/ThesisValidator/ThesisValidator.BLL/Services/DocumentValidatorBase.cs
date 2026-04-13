using ThesisValidator.BLL.Interfaces;
using ThesisValidator.BLL.Models;
using ThesisValidator.Domain;
using ThesisValidator.Domain.Rules;

namespace ThesisValidator.BLL.Services;

public abstract class DocumentValidatorBase<TDocument> : IDocumentValidator
{
    protected readonly IRuleEvaluator RuleEvaluator;

    protected DocumentValidatorBase(IRuleEvaluator ruleEvaluator)
    {
        RuleEvaluator = ruleEvaluator;
    }

    public abstract bool CanValidate(string fileExtension);

    public abstract Task<ValidationResult> ValidateAsync(
        Stream document,
        IEnumerable<ValidationRule> rules);

    protected async Task<ValidationResult> ValidateRulesAsync(
        TDocument document,
        Stream rawStream,
        IEnumerable<ValidationRule> rules)
    {
        var issues = new List<ValidationIssue>();

        foreach (var rule in rules)
        {
            var issue = rule.Enabled
                ? await ValidateRuleAsync(document, rawStream, rule)
                : ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Reegel pole sisse lülitatud");

            issues.Add(issue);
        }

        return new ValidationResult { Issues = issues };
    }

    protected Task<ValidationIssue> ValidateRuleAsync(TDocument document, Stream rawStream, ValidationRule rule)
    {
        return rule switch
        {
            NumericRule numeric => ValidateNumericRuleAsync(document, numeric),
            BooleanRule boolean => ValidateBooleanRuleAsync(document, boolean),
            EnumRule enumRule => ValidateEnumRuleAsync(document, enumRule),
            RegexRule regex => ValidateRegexRuleAsync(document, regex),
            CountRule count => ValidateCountRuleAsync(document, rawStream, count),
            OrderRule order => ValidateOrderRuleAsync(document, order),
            CrossReferenceRule crossRef => ValidateCrossReferenceRuleAsync(document, crossRef),
            LanguageRule language => ValidateLanguageRuleAsync(document, language),
            _ => Task.FromResult(ValidationIssue.CreateSkipped(
                rule.RuleId,
                rule.Message,
                "Reegli kontrollmehhanism on puudu"))
        };
    }

    protected abstract Task<ValidationIssue> ValidateNumericRuleAsync(TDocument document, NumericRule rule);
    protected abstract Task<ValidationIssue> ValidateBooleanRuleAsync(TDocument document, BooleanRule rule);
    protected abstract Task<ValidationIssue> ValidateEnumRuleAsync(TDocument document, EnumRule rule);
    protected abstract Task<ValidationIssue> ValidateRegexRuleAsync(TDocument document, RegexRule rule);

    protected abstract Task<ValidationIssue> ValidateCountRuleAsync(TDocument document,
        Stream rawStream,
        CountRule rule);

    protected abstract Task<ValidationIssue> ValidateOrderRuleAsync(TDocument document, OrderRule rule);

    protected abstract Task<ValidationIssue> ValidateCrossReferenceRuleAsync(TDocument document,
        CrossReferenceRule rule);

    protected abstract Task<ValidationIssue> ValidateLanguageRuleAsync(TDocument document, LanguageRule rule);
}
