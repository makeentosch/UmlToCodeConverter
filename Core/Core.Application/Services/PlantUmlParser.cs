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

        var lines = plantUmlContent
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        UmlElement? currentElement = null;

        foreach (var line in lines)
        {
            if (line.StartsWith(PlantUmlKeywords.StartUml) || line.StartsWith(PlantUmlKeywords.EndUml))
                continue;

            if (line is PlantUmlKeywords.CloseBrace)
            {
                currentElement = null;
                continue;
            }

            if (currentElement is null && TryParseRelationship(line, out var relationship))
            {
                result.Relationships.Add(relationship!);
                continue;
            }

            if (currentElement is null && TryParseElementDeclaration(line, out var newElement))
            {
                switch (newElement)
                {
                    case UmlClass c:
                        result.Classes.Add(c);
                        break;
                    case UmlInterface i:
                        result.Interfaces.Add(i);
                        break;
                    case UmlEnum e:
                        result.Enums.Add(e);
                        break;
                }

                if (line.Contains(PlantUmlKeywords.OpenBrace))
                    currentElement = newElement;

                continue;
            }

            if (currentElement is null)
                continue;

            if (currentElement is UmlClass currentClass)
            {
                if (TryParseMethod(line, out var method))
                    currentClass.Methods!.Add(method!);
                else if (TryParseProperty(line, out var property))
                    currentClass.Properties!.Add(property!);
            }
            else if (currentElement is UmlInterface currentInterface)
            {
                if (TryParseMethod(line, out var method))
                    currentInterface.Methods.Add(method!);
            }
            else if (currentElement is UmlEnum currentEnum)
                currentEnum.Values.Add(line);
        }

        return result;
    }

    private static void ValidateUml(string plantUmlContent)
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

    private static bool TryParseElementDeclaration(string line, out UmlElement? element)
    {
        element = null;
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            return false;

        var keyword = parts.First().ToLower();
        var name = parts[1].Trim('{');

        switch (keyword)
        {
            case PlantUmlKeywords.Declarations.Class:
                element = new UmlClass
                    { Name = name, Properties = new List<UmlProperty>(), Methods = new List<UmlMethod>() };
                return true;
            case PlantUmlKeywords.Declarations.Interface:
                element = new UmlInterface { Name = name, Methods = new List<UmlMethod>() };
                return true;
            case PlantUmlKeywords.Declarations.Enum:
                element = new UmlEnum { Name = name, Values = new List<string>() };
                return true;
            default:
                return false;
        }
    }

    private static bool TryParseRelationship(string line, out UmlRelationship? relationship)
    {
        relationship = null;

        var operators = new Dictionary<string, RelationshipType>
        {
            { "--|>", RelationshipType.Inheritance },
            { "..|>", RelationshipType.Realization },
            { "-->", RelationshipType.Association },
            { "o--", RelationshipType.Aggregation },
            { "*--", RelationshipType.Composition },
            { "..>", RelationshipType.Dependency }
        };

        foreach (var operation in operators)
        {
            if (!line.Contains(operation.Key))
                continue;

            var parts = line.Split(new[] { operation.Key }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                relationship = new UmlRelationship
                {
                    FromClassName = parts[0].Trim(),
                    ToClassName = parts[1].Trim(),
                    Type = operation.Value
                };
                return true;
            }
        }

        return false;
    }

    private static bool TryParseProperty(string line, out UmlProperty? property)
    {
        property = null;
        if (line.Contains('(')) return false;

        var parts = line.Split(':');
        if (parts.Length < 2) return false;

        property = new UmlProperty
        {
            AccessModifier = ParseAccessModifier(line.First().ToString()),
            Name = parts.First().TrimStart('+', '-', '#', ' ').Trim(),
            Type = parts[1].Trim()
        };

        return true;
    }

    private static bool TryParseMethod(string line, out UmlMethod? method)
    {
        method = null;

        var openParenIndex = line.IndexOf('(');
        var closeParenIndex = line.IndexOf(')');

        if (openParenIndex == -1 || closeParenIndex == -1)
            return false;

        method = new UmlMethod
        {
            Parameters = new List<UmlParameter>(),
            AccessModifier = ParseAccessModifier(line)
        };

        var nameStartIndex = line.StartsWith("+") || line.StartsWith("-") || line.StartsWith("#") ? 1 : 0;
        method.Name = line.Substring(nameStartIndex, openParenIndex - nameStartIndex).Trim();

        var colonIndex = line.LastIndexOf(':');
        method.ReturnType = colonIndex > closeParenIndex
            ? line.Substring(colonIndex + 1).Trim()
            : PlantUmlKeywords.DefaultReturnType;

        var paramsString = line.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);

        if (string.IsNullOrWhiteSpace(paramsString))
            return true;

        var paramTokens = paramsString.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in paramTokens)
        {
            var parts = token.Split(':');
            method.Parameters.Add(new UmlParameter
            {
                Name = parts[0].Trim(),
                Type = parts.Length > 1 ? parts[1].Trim() : PlantUmlKeywords.DefaultPropertyType
            });
        }

        return true;
    }

    private static AccessModifier ParseAccessModifier(string umlAccessModifier) =>
        umlAccessModifier switch
        {
            PlantUmlKeywords.AccessModifiers.Private => AccessModifier.Private,
            PlantUmlKeywords.AccessModifiers.Protected => AccessModifier.Protected,
            PlantUmlKeywords.AccessModifiers.Public => AccessModifier.Public,
            _ => AccessModifier.Public
        };
}