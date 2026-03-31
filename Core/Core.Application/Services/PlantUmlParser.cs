using Core.Application.Interfaces;
using Core.Domain.Constants;
using Core.Domain.Enums;
using Core.Domain.Models;

namespace Core.Application.Services;

public class PlantUmlParser : IUmlParser
{
    public UmlDiagram Parse(string plantUmlContent)
    {
        var result = new UmlDiagram();

        var umlStrings = plantUmlContent.Split(['\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        UmlClass? currentClass = null;

        foreach (var umlString in umlStrings)
        {
            if (umlString.StartsWith(PlantUmlKeywords.StartUml) || umlString.StartsWith(PlantUmlKeywords.EndUml))
                continue;

            if (umlString.StartsWith(PlantUmlKeywords.ClassDeclaration))
            {
                var parts = umlString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    currentClass = new UmlClass(name: parts[1].Replace('{', ' ').Trim());
                    result.Classes.Add(currentClass);
                }
            }

            else if (umlString == PlantUmlKeywords.BlockEnd)
                currentClass = null;
            else if (currentClass != null && umlString.Length > 0)
                ParseClassMembers(umlString, currentClass);
        }

        return result;
    }

    private void ParseClassMembers(string umlString, UmlClass targetClass)
    {
        var visibility = umlString switch
        {
            _ when umlString.StartsWith(PlantUmlKeywords.AccessModifiers.Private) => AccessModifier.Private,
            _ when umlString.StartsWith(PlantUmlKeywords.AccessModifiers.Protected) => AccessModifier.Protected,
            _ when umlString.StartsWith(PlantUmlKeywords.AccessModifiers.Public) => AccessModifier.Public,
            _ => AccessModifier.Public
        };

        var cleanLine = umlString.TrimStart('-', '+', '#', ' ');

        if (cleanLine.Contains(PlantUmlKeywords.MethodIndicator))
        {
            var method = new UmlMethod { AccessModifier = visibility };
            var parts = cleanLine.Split(':');

            method.Name = parts[0].Replace(PlantUmlKeywords.MethodIndicator, "").Trim();
            method.ReturnType = parts.Length > 1 ? parts[1].Trim() : "void";

            targetClass.Methods.Add(method);
        }
        else
        {
            var property = new UmlProperty { AccessModifier = visibility };
            var parts = cleanLine.Split(':');

            property.Name = parts[0].Trim();
            property.Type = parts.Length > 1 ? parts[1].Trim() : "object";

            targetClass.Properties.Add(property);
        }
    }

    public void ValidateUml(string plantUmlText)
    {
        throw new NotImplementedException();
    }
}