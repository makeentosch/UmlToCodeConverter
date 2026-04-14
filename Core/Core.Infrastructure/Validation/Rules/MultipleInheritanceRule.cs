using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Infrastructure.Constants;

namespace Core.Infrastructure.Validation.Rules;

public class MultipleInheritanceRule : IValidationRule
{
    private readonly string[] _unsupportedLanguages =
    {
        Common.Languages.Java, 
        Common.Languages.CSharp, 
        Common.Languages.Golang
    };

    public IEnumerable<string> Validate(CodeObjectModel model, string targetLanguage)
    {
        if (!_unsupportedLanguages.Contains(targetLanguage.ToLower()))
            yield break;

        var multipleInheritances = model.Relationships
            .Where(r => r.Type == RelationshipType.Inheritance)
            .GroupBy(r => r.FromClassName)
            .Where(g => g.Count() > 1);

        foreach (var group in multipleInheritances)
            yield return
                $"Language {targetLanguage} does not support multiple class inheritance. Class \"{group.Key}\" attempts to inherit from multiple classes.";
    }
}