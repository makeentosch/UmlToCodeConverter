using Core.Domain.Models;
using Core.Infrastructure.Constants;

namespace Core.Infrastructure.Validation.Rules;

public class MethodOverloadingRule : IValidationRule
{
    public IEnumerable<string> Validate(CodeObjectModel model, string targetLanguage)
    {
        if (targetLanguage.ToLower() != Common.Languages.Golang)
            yield break;

        foreach (var umlClass in model.Classes.Where(c => c.Methods != null))
        {
            var overloadedMethods = umlClass.Methods!
                .GroupBy(m => m.Name)
                .Where(g => g.Count() > 1);

            foreach (var group in overloadedMethods)
                yield return $"Golang does not support method overloading. Multiple methods named \"{group.Key}\" found in class \"{umlClass.Name}\".";
        }
    }
}