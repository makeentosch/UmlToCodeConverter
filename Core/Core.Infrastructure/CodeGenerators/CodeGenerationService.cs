using Core.Infrastructure.DTOs;
using Core.Infrastructure.Validation;

namespace Core.Infrastructure.CodeGenerators;

public class CodeGenerationService
{
    private readonly CodeObjectModelValidator _validator = new();

    public GenerationResult Process(string inputText, string inputFormat, string targetLanguage)
    {
        try
        {
            var parser = ParserFactory.GetParser(inputFormat);
            var model = parser.Parse(inputText);

            var validation = _validator.Validate(model, targetLanguage);
            if (!validation.IsValid)
            {
                return new GenerationResult
                {
                    Content = "ARCHITECTURE VALIDATION ERROR:\n\n- " + string.Join("\n- ", validation.Errors),
                    IsSuccess = false,
                    StatusMessage = "Ready"
                };
            }

            var generator = GeneratorFactory.GetGenerator(targetLanguage);
            return new GenerationResult
            {
                Content = generator.Generate(model),
                IsSuccess = true,
                StatusMessage = "Code generated successfully."
            };
        }
        catch (Exception ex)
        {
            return new GenerationResult
            {
                Content = ex.Message,
                IsSuccess = false,
                StatusMessage = "Parsing error."
            };
        }
    }
}