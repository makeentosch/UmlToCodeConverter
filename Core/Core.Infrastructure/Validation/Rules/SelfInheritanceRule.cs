using Core.Domain.Enums;
using Core.Domain.Models;

namespace Core.Infrastructure.Validation.Rules;

public class SelfInheritanceRule : IValidationRule
{
    public IEnumerable<string> Validate(CodeObjectModel model, string targetLanguage)
    {
        var selfInheritances = model.Relationships
            .Where(r => r.Type == RelationshipType.Inheritance && r.FromClassName == r.ToClassName);

        foreach (var rel in selfInheritances)
            yield return $"Class \"{rel.FromClassName}\" cannot inherit from itself.";
    }
}