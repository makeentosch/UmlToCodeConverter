using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Domain.Constants;
using Core.Domain.Enums;
using Core.Domain.Models;

namespace Core.Application.Services;

public class PlantUmlParser : IUmlParser
{
    public CodeObjectModel Parse(string plantUmlContent)
    {
        var result = new CodeObjectModel();

        if (string.IsNullOrWhiteSpace(plantUmlContent))
            return result;

        ValidateUml(plantUmlContent);

        var umlStrings = plantUmlContent.Split(['\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        UmlElement? currentElement = null;

        foreach (var umlString in umlStrings)
        {
            if (IsStartOrEndTag(umlString))
                continue;

            if (umlString == PlantUmlKeywords.CloseBrace)
            {
                currentElement = null;
                continue;
            }

            if (currentElement is null && TryParseRelationship(umlString, out var relationship))
            {
                result.Relationships.Add(relationship!);
                continue;
            }

            if (TryParseElementDeclaration(umlString, out var newElement))
            {
                currentElement = newElement;
                AddElementToDiagram(result, currentElement!);
                continue;
            }

            if (currentElement is not null && umlString.Length > 0)
                ParseMember(umlString, currentElement);
        }

        return result;
    }

    public void ValidateUml(string plantUmlContent)
    {
        if (string.IsNullOrWhiteSpace(plantUmlContent))
            throw new InvalidUmlException("UML is empty.");

        var umlStrings = plantUmlContent.Split(['\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (!umlStrings.Any(x => x.StartsWith(PlantUmlKeywords.StartUml)))
            throw new InvalidUmlException($"Missing {PlantUmlKeywords.StartUml} tag.");

        if (!umlStrings.Any(x => x.StartsWith(PlantUmlKeywords.EndUml)))
            throw new InvalidUmlException($"Missing {PlantUmlKeywords.EndUml} tag.");

        var openBracesCount = umlStrings.Count(x => x.Contains(PlantUmlKeywords.OpenBrace));
        var closeBracesCount = umlStrings.Count(x => x.Contains(PlantUmlKeywords.CloseBrace));

        if (openBracesCount != closeBracesCount)
            throw new InvalidUmlException(
                $"Mismatched brackets. Found {openBracesCount} open and {closeBracesCount} close.");
    }

    private static bool TryParseRelationship(string line, out UmlRelationship? relationship)
    {
        relationship = null;

        var relationships = new Dictionary<string, RelationshipType>
        {
            { PlantUmlKeywords.Relationships.Inheritance, RelationshipType.Inheritance },
            { PlantUmlKeywords.Relationships.Realization, RelationshipType.Realization },
            { PlantUmlKeywords.Relationships.Association, RelationshipType.Association },
            { PlantUmlKeywords.Relationships.Aggregation, RelationshipType.Aggregation },
            { PlantUmlKeywords.Relationships.Composition, RelationshipType.Composition },
            { PlantUmlKeywords.Relationships.Dependency, RelationshipType.Dependency }
        };

        foreach (var (symbol, type) in relationships)
        {
            if (!line.Contains(symbol))
                continue;

            var parts = line.Split(symbol);

            if (parts.Length != 2)
                continue;

            relationship = new UmlRelationship
            {
                FromClassName = parts[0].Trim(),
                ToClassName = parts[1].Trim(),
                Type = type
            };
            return true;
        }

        return false;
    }

    private static bool TryParseElementDeclaration(string umlString, out UmlElement? element)
    {
        element = null;
        var parts = umlString.Split(PlantUmlKeywords.Space, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            return false;

        var name = parts[1].Replace(PlantUmlKeywords.OpenBrace, string.Empty).Trim();

        if (umlString.StartsWith(PlantUmlKeywords.Declarations.Class))
        {
            element = new UmlClass
            {
                Name = name,
                Properties = new List<UmlProperty>(),
                Methods = new List<UmlMethod>()
            };
            return true;
        }

        if (umlString.StartsWith(PlantUmlKeywords.Declarations.Interface))
        {
            element = new UmlInterface
            {
                Name = name,
                Methods = new List<UmlMethod>()
            };
            return true;
        }

        if (umlString.StartsWith(PlantUmlKeywords.Declarations.Enum))
        {
            element = new UmlEnum
            {
                Name = name,
                Values = new List<string>()
            };
            return true;
        }

        return false;
    }

    private static void AddElementToDiagram(CodeObjectModel objectModel, UmlElement element)
    {
        switch (element)
        {
            case UmlClass classElement:
                objectModel.Classes.Add(classElement);
                break;
            case UmlInterface interfaceElement:
                objectModel.Interfaces.Add(interfaceElement);
                break;
            case UmlEnum enumElement:
                objectModel.Enums.Add(enumElement);
                break;
        }
    }

    private static void ParseMember(string umlString, UmlElement targetElement)
    {
        if (umlString == PlantUmlKeywords.OpenBrace) return;

        switch (targetElement)
        {
            case UmlEnum enumElement:
                ParseEnumValue(umlString, enumElement);
                break;
            case UmlClass classElement:
                ParseMember(umlString, classElement.Properties, classElement.Methods);
                break;
            case UmlInterface interfaceElement:
                ParseMember(umlString, null, interfaceElement.Methods);
                break;
        }
    }

    private static void ParseEnumValue(string umlString, UmlEnum targetEnum)
    {
        var value = umlString.Trim();

        if (!string.IsNullOrEmpty(value))
            targetEnum.Values.Add(value);
    }

    private static void ParseMember(string umlString, List<UmlProperty>? properties, List<UmlMethod>? methods)
    {
        var visibility = umlString switch
        {
            _ when umlString.StartsWith(PlantUmlKeywords.AccessModifiers.Private) => AccessModifier.Private,
            _ when umlString.StartsWith(PlantUmlKeywords.AccessModifiers.Protected) => AccessModifier.Protected,
            _ when umlString.StartsWith(PlantUmlKeywords.AccessModifiers.Public) => AccessModifier.Public,
            _ => AccessModifier.Public
        };

        var cleanLine = umlString.TrimStart(
            PlantUmlKeywords.AccessModifiers.Private[0],
            PlantUmlKeywords.AccessModifiers.Public[0],
            PlantUmlKeywords.AccessModifiers.Protected[0],
            PlantUmlKeywords.Space,
            PlantUmlKeywords.OpenBrace[0]);

        if (cleanLine.Contains(PlantUmlKeywords.MethodIndicator))
        {
            if (methods == null) return;

            var method = new UmlMethod
            {
                AccessModifier = visibility,
                Parameters = new List<UmlParameter>()
            };

            var parts = cleanLine.Split(PlantUmlKeywords.TypeSeparator);

            method.Name = parts[0].Replace(PlantUmlKeywords.MethodIndicator, string.Empty).Trim();
            method.ReturnType = parts.Length > 1 ? parts[1].Trim() : PlantUmlKeywords.DefaultReturnType;

            methods.Add(method);
        }
        else
        {
            if (properties is null)
                return;

            var property = new UmlProperty { AccessModifier = visibility };
            var parts = cleanLine.Split(PlantUmlKeywords.TypeSeparator);

            property.Name = parts[0].Trim();
            property.Type = parts.Length > 1 ? parts[1].Trim() : PlantUmlKeywords.DefaultPropertyType;

            properties.Add(property);
        }
    }

    private static bool IsStartOrEndTag(string line) =>
        line.StartsWith(PlantUmlKeywords.StartUml) || line.StartsWith(PlantUmlKeywords.EndUml);
}