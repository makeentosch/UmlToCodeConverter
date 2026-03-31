using Core.Domain.Models;

namespace Core.Application.Services;

public interface IPlantUmlParser
{
    UmlDiagram Parse(string plantUmlText);

    bool IsValidPlantUml(string plantUmlText);
}

public class PlantUmlParser : IPlantUmlParser
{
    public UmlDiagram Parse(string plantUmlText)
    {
        throw new NotImplementedException();
    }

    public bool IsValidPlantUml(string plantUmlText)
    {
        throw new NotImplementedException();
    }
}