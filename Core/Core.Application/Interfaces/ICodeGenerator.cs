using Core.Domain.Models;

namespace Core.Application.Interfaces;

public interface ICodeGenerator
{
    public string LanguageName { get; }
    string Generate(UmlDiagram diagram);
}