namespace Core.Application.Services;

public class DiagramGeneratorService
{
    private readonly IPlantUmlParser _parser;
    private readonly IEnumerable<ICodeGenerator> _generators;

    public DiagramGeneratorService(
        IPlantUmlParser parser,
        IEnumerable<ICodeGenerator> generators)
    {
        _parser = parser;
        _generators = generators;
    }

    public Dictionary<string, string> GenerateCode(
        string plantUmlText, 
        IEnumerable<string> selectedLanguages)
    {
        var diagram = _parser.Parse(plantUmlText);

        var result = new Dictionary<string, string>();

        foreach (var generator in _generators)
        {
            if (selectedLanguages.Contains(generator.LanguageName))
            {
                var code = generator.Generate(diagram);
                result[generator.LanguageName] = code;
            }
        }

        return result;
    }

    public bool ValidateInput(string plantUmlText)
    {
        return _parser.IsValidPlantUml(plantUmlText);
    }
}