using Core.Domain.Models;

namespace Core.Application.Interfaces;

public interface IUmlParser
{
    UmlDiagram Parse(string plantUmlContent);

    void ValidateUml(string plantUmlText);
}