using Core.Domain.Models;
using Core.Infrastructure.Constants;

namespace Core.Infrastructure.Validation.Rules;

public class InterfacePropertiesRule : IValidationRule
{
    private readonly string[] _unsupportedLanguages =
    {
        Common.Languages.Java, 
        Common.Languages.Golang
    };

    public IEnumerable<string> Validate(CodeObjectModel model, string targetLanguage)
    {
        if (!_unsupportedLanguages.Contains(targetLanguage.ToLower()))
            yield break;

        var interfacesWithProps = model.Interfaces
            .Where(i => i.Properties is not null && i.Properties.Count != 0);

        foreach (var i in interfacesWithProps)
        {
            yield return $"In {targetLanguage}, interface \"{i.Name}\" cannot contain fields or properties.";
        }
    }
}