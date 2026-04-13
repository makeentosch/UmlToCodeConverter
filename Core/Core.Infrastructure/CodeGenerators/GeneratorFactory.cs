using Core.Application.Interfaces;

namespace Core.Infrastructure.CodeGenerators;

public static class GeneratorFactory
{
    public static ICodeGenerator GetGenerator(string targetLanguage)
    {
        return targetLanguage.ToLower() switch
        {
            "c#" => new CSharpCodeGenerator(),
            "java" => new JavaCodeGenerator(),
            "go" => new GoCodeGenerator(),
            _ => throw new NotSupportedException($"Генерация для языка '{targetLanguage}' пока не поддерживается.")
        };
    }
}