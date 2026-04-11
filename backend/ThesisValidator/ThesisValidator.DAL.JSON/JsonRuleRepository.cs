using System.Text.Json;
using System.Text.Json.Serialization;
using ThesisValidator.Domain;
using ThesisValidator.Domain.Rules;

namespace ThesisValidator.DAL.JSON;

public class JsonRuleRepository : IRuleRepository
{
    private readonly string _resourcesPath;

    private readonly JsonSerializerOptions _jsonOptions;

    public JsonRuleRepository()
    {
        _resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }

    public async Task<ValidationTemplate?> GetTemplateAsync(string templateId)
    {
        // TODO: look again if this can be implemented with existing serialization options, not raw switching.
        var filePath = Path.Combine(_resourcesPath, $"{templateId}.json");

        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var raw = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);

        var template = new ValidationTemplate
        {
            TemplateId = raw.GetProperty("templateId").GetString() ?? string.Empty,
            Name = raw.GetProperty("name").GetString() ?? string.Empty,
            Version = raw.GetProperty("version").GetString() ?? string.Empty,
            Rules = []
        };

        foreach (var ruleElement in raw.GetProperty("rules").EnumerateArray())
        {
            var type = ruleElement.GetProperty("type").GetString();
            var ruleJson = ruleElement.GetRawText();

            ValidationRule? rule = type?.ToLower() switch
            {
                "numeric" => JsonSerializer.Deserialize<NumericRule>(ruleJson, _jsonOptions),
                "boolean" => JsonSerializer.Deserialize<BooleanRule>(ruleJson, _jsonOptions),
                "enum" => JsonSerializer.Deserialize<EnumRule>(ruleJson, _jsonOptions),
                "regex" => JsonSerializer.Deserialize<RegexRule>(ruleJson, _jsonOptions),
                "count" => JsonSerializer.Deserialize<CountRule>(ruleJson, _jsonOptions),
                "order" => JsonSerializer.Deserialize<OrderRule>(ruleJson, _jsonOptions),
                "crossreference" => JsonSerializer.Deserialize<CrossReferenceRule>(ruleJson, _jsonOptions),
                _ => null
            };

            if (rule != null)
            {
                template.Rules.Add(rule);
            }
        }

        return template;
    }
}
