using System.Xml;
using System.Xml.Linq;
using Core.Application.Exceptions;
using Core.Application.Interfaces;
using Core.Domain.Constants;
using Core.Domain.Enums;
using Core.Domain.Models;

namespace Core.Application.Services;

public class XmlUmlParser : IUmlParser
{
    public CodeObjectModel Parse(string xmlContent)
    {
        var result = new CodeObjectModel();

        if (string.IsNullOrWhiteSpace(xmlContent))
            return result;

        ValidateUml(xmlContent);

        var doc = XDocument.Parse(xmlContent);
        var root = doc.Element(XmlUmlKeywords.RootTag);

        if (root is null)
            return result;

        ParseClasses(root, result);
        ParseInterfaces(root, result);
        ParseEnums(root, result);
        ParseRelationships(root, result);

        return result;
    }

    private static void ValidateUml(string plantUmlContent)
    {
        if (string.IsNullOrWhiteSpace(plantUmlContent))
            throw new InvalidUmlException("UML is empty.");

        try
        {
            var doc = XDocument.Parse(plantUmlContent);
            if (doc.Element(XmlUmlKeywords.RootTag) is null)
                throw new InvalidUmlException($"Missing root element <{XmlUmlKeywords.RootTag}>.");
        }
        catch (XmlException innerException)
        {
            throw new InvalidUmlException(innerException.Message, innerException);
        }
    }

    private static void ParseClasses(XElement root, CodeObjectModel objectModel)
    {
        var classesNode = root.Element(XmlUmlKeywords.ClassesTag);

        if (classesNode is null)
            return;

        foreach (var node in classesNode.Elements(XmlUmlKeywords.ClassTag))
        {
            var name = node.Attribute(XmlUmlKeywords.NameAttribute)?.Value ?? string.Empty;
            var umlClass = new UmlClass
            {
                Name = name,
                Properties = new List<UmlProperty>(),
                Methods = new List<UmlMethod>()
            };

            ParseProperties(node, umlClass.Properties!);
            ParseMethods(node, umlClass.Methods!);

            objectModel.Classes.Add(umlClass);
        }
    }

    private static void ParseInterfaces(XElement root, CodeObjectModel objectModel)
    {
        var interfacesNode = root.Element(XmlUmlKeywords.InterfacesTag);

        if (interfacesNode == null)
            return;

        foreach (var node in interfacesNode.Elements(XmlUmlKeywords.InterfaceTag))
        {
            var name = node.Attribute(XmlUmlKeywords.NameAttribute)?.Value ?? string.Empty;
            var umlInterface = new UmlInterface
            {
                Name = name,
                Methods = new List<UmlMethod>()
            };

            ParseMethods(node, umlInterface.Methods);

            objectModel.Interfaces.Add(umlInterface);
        }
    }

    private static void ParseEnums(XElement root, CodeObjectModel objectModel)
    {
        var enumsNode = root.Element(XmlUmlKeywords.EnumsTag);

        if (enumsNode is null)
            return;

        foreach (var enumNode in enumsNode.Elements(XmlUmlKeywords.EnumTag))
        {
            var name = enumNode.Attribute(XmlUmlKeywords.NameAttribute)?.Value ?? string.Empty;

            var umlEnum = new UmlEnum
            {
                Name = name,
                Values = new List<string>()
            };

            var valuesNode = enumNode.Element(XmlUmlKeywords.ValuesTag);
            if (valuesNode is not null)
            {
                foreach (var valueNode in valuesNode.Elements(XmlUmlKeywords.ValueTag))
                    if (!string.IsNullOrWhiteSpace(valueNode.Value))
                        umlEnum.Values.Add(valueNode.Value.Trim());
            }

            objectModel.Enums.Add(umlEnum);
        }
    }

    private static void ParseRelationships(XElement root, CodeObjectModel objectModel)
    {
        var relationshipsNode = root.Element(XmlUmlKeywords.RelationshipsTag);

        if (relationshipsNode is null)
            return;

        foreach (var node in relationshipsNode.Elements(XmlUmlKeywords.RelationshipTag))
        {
            var typeString = node.Attribute(XmlUmlKeywords.TypeAttribute)?.Value;

            if (Enum.TryParse<RelationshipType>(typeString, out var relType))
            {
                objectModel.Relationships.Add(new UmlRelationship
                {
                    FromClassName = node.Attribute(XmlUmlKeywords.FromAttribute)?.Value ?? string.Empty,
                    ToClassName = node.Attribute(XmlUmlKeywords.ToAttribute)?.Value ?? string.Empty,
                    Type = relType
                });
            }
        }
    }

    private static void ParseProperties(XElement parentNode, List<UmlProperty> properties)
    {
        var propertiesNode = parentNode.Element(XmlUmlKeywords.PropertiesTag);

        if (propertiesNode is null)
            return;

        properties.AddRange(propertiesNode.Elements(XmlUmlKeywords.PropertyTag).Select(node => new UmlProperty
        {
            Name = node.Attribute(XmlUmlKeywords.NameAttribute)?.Value ?? string.Empty,
            Type = node.Attribute(XmlUmlKeywords.TypeAttribute)?.Value ?? PlantUmlKeywords.DefaultPropertyType,
            AccessModifier = ParseAccessModifier(node.Attribute(XmlUmlKeywords.AccessModifierAttribute)?.Value)
        }));
    }

    private static void ParseMethods(XElement parentNode, List<UmlMethod> methods)
    {
        var methodsNode = parentNode.Element(XmlUmlKeywords.MethodsTag);

        if (methodsNode is null)
            return;

        foreach (var methodNode in methodsNode.Elements(XmlUmlKeywords.MethodTag))
        {
            var method = new UmlMethod
            {
                Name = methodNode.Attribute(XmlUmlKeywords.NameAttribute)?.Value ?? string.Empty,
                ReturnType = methodNode
                    .Attribute(XmlUmlKeywords.ReturnTypeAttribute)?.Value ?? PlantUmlKeywords.DefaultReturnType,
                AccessModifier =
                    ParseAccessModifier(methodNode.Attribute(XmlUmlKeywords.AccessModifierAttribute)?.Value),
                Parameters = new List<UmlParameter>()
            };

            var paramsNode = methodNode.Element("Parameters");
            if (paramsNode is not null)
            {
                foreach (var paramNode in paramsNode.Elements("Parameter"))
                {
                    method.Parameters.Add(new UmlParameter
                    {
                        Name = paramNode.Attribute(XmlUmlKeywords.NameAttribute)?.Value ?? "param",
                        Type = paramNode.Attribute(XmlUmlKeywords.TypeAttribute)?.Value ?? "object"
                    });
                }
            }

            methods.Add(method);
        }
    }

    private static AccessModifier ParseAccessModifier(string? umlAccessModifier) =>
        umlAccessModifier switch
        {
            PlantUmlKeywords.AccessModifiers.Private => AccessModifier.Private,
            PlantUmlKeywords.AccessModifiers.Protected => AccessModifier.Protected,
            PlantUmlKeywords.AccessModifiers.Public => AccessModifier.Public,
            _ => AccessModifier.Public
        };
}