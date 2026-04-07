using System.Text;
using Core.Application.Interfaces;
using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Infrastructure.Constants;

namespace Core.Infrastructure.CodeGenerators;

public class CSharpCodeGenerator : ICodeGenerator
{
    public string LanguageName => "C#";

    public string Generate(CodeObjectModel objectModel)
    {
        var sb = new StringBuilder();

        foreach (var umlEnum in objectModel.Enums)
        {
            GenerateEnum(umlEnum, sb);
            sb.AppendLine();
        }

        foreach (var umlInterface in objectModel.Interfaces)
        {
            GenerateInterface(umlInterface, sb);
            sb.AppendLine();
        }

        foreach (var umlClass in objectModel.Classes)
        {
            GenerateClass(umlClass, sb, objectModel);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static void GenerateEnum(UmlEnum umlEnum, StringBuilder sb)
    {
        sb.AppendLine($"{CSharpKeywords.Public} {CSharpKeywords.EnumDeclaration} {umlEnum.Name}");
        sb.AppendLine(CSharpKeywords.OpenBrace);

        for (int i = 0; i < umlEnum.Values.Count; i++)
        {
            var isLast = i == umlEnum.Values.Count - 1;
            var comma = isLast ? string.Empty : ",";
            sb.AppendLine($"{CSharpKeywords.Indent}{umlEnum.Values[i]}{comma}");
        }

        sb.AppendLine(CSharpKeywords.CloseBrace);
    }

    private static void GenerateInterface(UmlInterface umlInterface, StringBuilder sb)
    {
        sb.AppendLine($"{CSharpKeywords.Public} {CSharpKeywords.InterfaceDeclaration} {umlInterface.Name}");
        sb.AppendLine(CSharpKeywords.OpenBrace);

        foreach (var method in umlInterface.Methods)
        {
            sb.AppendLine($"{CSharpKeywords.Indent}{method.ReturnType} {method.Name}();");
        }

        sb.AppendLine(CSharpKeywords.CloseBrace);
    }

    private static void GenerateClass(UmlClass umlClass, StringBuilder sb, CodeObjectModel objectModel)
    {
        var baseTypes = objectModel.Relationships
            .Where(r => r.FromClassName == umlClass.Name &&
                        (r.Type == RelationshipType.Inheritance || r.Type == RelationshipType.Realization))
            .Select(r => r.ToClassName)
            .ToList();

        var inheritanceString = baseTypes.Any() ? $" : {string.Join(", ", baseTypes)}" : string.Empty;

        sb.AppendLine($"{CSharpKeywords.Public} {CSharpKeywords.ClassDeclaration} {umlClass.Name}{inheritanceString}");
        sb.AppendLine(CSharpKeywords.OpenBrace);

        var relationalProperties = objectModel.Relationships
            .Where(r => r.FromClassName == umlClass.Name &&
                        (r.Type == RelationshipType.Composition || r.Type == RelationshipType.Aggregation ||
                         r.Type == RelationshipType.Association))
            .ToList();

        var hasProperties = (umlClass.Properties != null && umlClass.Properties.Any()) || relationalProperties.Any();
        var hasMethods = umlClass.Methods != null && umlClass.Methods.Any();

        if (hasProperties)
        {
            if (umlClass.Properties != null)
            {
                foreach (var prop in umlClass.Properties)
                {
                    var accessModifier = GetAccessModifierString(prop.AccessModifier);
                    sb.AppendLine(
                        $"{CSharpKeywords.Indent}{accessModifier} {prop.Type} {prop.Name} {CSharpKeywords.AutoProperty}");
                }
            }

            foreach (var relProp in relationalProperties)
            {
                sb.AppendLine(
                    $"{CSharpKeywords.Indent}{CSharpKeywords.Public} {relProp.ToClassName} {relProp.ToClassName} {CSharpKeywords.AutoProperty}");
            }

            if (hasMethods) sb.AppendLine();
        }

        if (hasMethods)
        {
            for (var i = 0; i < umlClass.Methods!.Count; i++)
            {
                var method = umlClass.Methods[i];
                var accessModifier = GetAccessModifierString(method.AccessModifier);

                var parametersString = method.Parameters.Count != 0
                    ? string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"))
                    : string.Empty;

                sb.AppendLine(
                    $"{CSharpKeywords.Indent}{accessModifier} {method.ReturnType} {method.Name}({parametersString})");
                sb.AppendLine($"{CSharpKeywords.Indent}{CSharpKeywords.OpenBrace}");
                sb.AppendLine(
                    $"{CSharpKeywords.Indent}{CSharpKeywords.Indent}{CSharpKeywords.NotImplementedMethodBody}");
                sb.AppendLine($"{CSharpKeywords.Indent}{CSharpKeywords.CloseBrace}");

                if (i < umlClass.Methods.Count - 1)
                    sb.AppendLine();
            }
        }

        sb.AppendLine(CSharpKeywords.CloseBrace);
    }

    private static string GetAccessModifierString(AccessModifier modifier)
    {
        return modifier switch
        {
            AccessModifier.Public => CSharpKeywords.Public,
            AccessModifier.Private => CSharpKeywords.Private,
            AccessModifier.Protected => CSharpKeywords.Protected,
            _ => CSharpKeywords.Private
        };
    }
}