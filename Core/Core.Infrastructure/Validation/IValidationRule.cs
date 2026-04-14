using Core.Domain.Models;

namespace Core.Infrastructure.Validation;

public interface IValidationRule
{
    IEnumerable<string> Validate(CodeObjectModel model, string targetLanguage);
}