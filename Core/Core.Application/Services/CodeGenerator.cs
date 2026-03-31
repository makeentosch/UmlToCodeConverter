using Core.Domain.Models;

namespace Core.Application.Services;

public interface ICodeGenerator
{
    /// <summary>
    /// Название языка.
    /// </summary>
    string LanguageName { get; }

    /// <summary>
    /// Расширение файла.
    /// </summary>
    string FileExtension { get; }

    string Generate(UmlDiagram diagram);
}

public class CodeGenerator : ICodeGenerator
{
    public string LanguageName { get; }
    public string FileExtension { get; }

    public string Generate(UmlDiagram diagram)
    {
        throw new NotImplementedException();
    }
}