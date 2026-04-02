using Core.Application.Interfaces;
using Core.Domain.Models;

namespace Core.Infrastructure;

public class CodeGenerator : ICodeGenerator
{
    public string LanguageName { get; }

    public string Generate(CodeObjectModel objectModel)
    {
        throw new NotImplementedException();
    }
}