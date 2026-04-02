using Core.Domain.Models;

namespace Core.Application.Interfaces;

public interface IUmlParser
{
    CodeObjectModel Parse(string plantUmlContent);

    void ValidateUml(string plantUmlContent);
}