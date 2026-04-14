using Core.Domain.Models;
using Core.Infrastructure.Validation.Rules;

namespace Core.Infrastructure.Validation;

public class CodeObjectModelValidator
{
    private readonly List<IValidationRule> _rules;

    public CodeObjectModelValidator()
    {
        _rules = new List<IValidationRule>
        {
            new MultipleInheritanceRule(),
            new SelfInheritanceRule(),
            new CyclicInheritanceRule(),
            new InterfacePropertiesRule(),
            new MethodOverloadingRule()
        };
    }

    public ValidationResult Validate(CodeObjectModel model, string targetLanguage)
    {
        var result = new ValidationResult();

        foreach (var rule in _rules)
        {
            var errors = rule.Validate(model, targetLanguage);
            foreach (var error in errors)
                result.AddError(error);
        }

        return result;
    }
}