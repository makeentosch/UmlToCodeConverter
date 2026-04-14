using Core.Application.Interfaces;
using Core.Application.Services;

namespace Core.Infrastructure;

public class ParserFactory
{
    public static IUmlParser GetParser(string inputFormat)
    {
        return inputFormat.ToLower() switch
        {
            "plantuml" => new PlantUmlParser(),
            "xml" => new XmlUmlParser(),
            _ => throw new NotSupportedException($"Input format \"{inputFormat}\" is not supported.")
        };
    }
}