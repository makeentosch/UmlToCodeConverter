using Core.Domain.Enums;
using Core.Domain.Models;

namespace Core.Infrastructure.Validation.Rules;

public class CyclicInheritanceRule : IValidationRule
{
    public IEnumerable<string> Validate(CodeObjectModel model, string targetLanguage)
    {
        var inheritances = model.Relationships.Where(r => r.Type == RelationshipType.Inheritance).ToList();

        foreach (var rel1 in inheritances)
        {
            if (inheritances.Any(rel2 =>
                    rel2.FromClassName == rel1.ToClassName && rel2.ToClassName == rel1.FromClassName))
            {
                yield return
                    $"Cyclic inheritance detected between \"{rel1.FromClassName}\" and \"{rel1.ToClassName}\".";
            }
        }
    }
}