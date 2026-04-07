using System.Text;
using Core.Application.Interfaces;
using Core.Domain.Enums;
using Core.Domain.Models;

namespace Core.Infrastructure.CodeGenerators;

public class JavaCodeGenerator : ICodeGenerator
{
    public string LanguageName => "Java";

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
        sb.AppendLine($"public enum {umlEnum.Name} {{");

        if (umlEnum.Values.Count != 0)
            sb.AppendLine($"    {string.Join(", ", umlEnum.Values)}");

        sb.AppendLine("}");
    }

    private static void GenerateInterface(UmlInterface umlInterface, StringBuilder sb)
    {
        sb.AppendLine($"public interface {umlInterface.Name} {{");

        foreach (var method in umlInterface.Methods)
        {
            var parameters = GenerateParameters(method.Parameters);
            sb.AppendLine($"    {MapToJavaType(method.ReturnType)} {method.Name}({parameters});");
        }

        sb.AppendLine("}");
    }

    private static void GenerateClass(UmlClass umlClass, StringBuilder sb, CodeObjectModel model)
    {
        var inheritances = model.Relationships
            .Where(r => r.FromClassName == umlClass.Name && r.Type == RelationshipType.Inheritance)
            .Select(r => r.ToClassName)
            .ToList();

        var realizations = model.Relationships
            .Where(r => r.FromClassName == umlClass.Name && r.Type == RelationshipType.Realization)
            .Select(r => r.ToClassName)
            .ToList();

        var classSignature = new StringBuilder($"public class {umlClass.Name}");

        if (inheritances.Count != 0)
            classSignature.Append($" extends {inheritances.First()}");
        
        if (realizations.Count != 0)
            classSignature.Append($" implements {string.Join(", ", realizations)}");

        sb.AppendLine(classSignature + " {");

        var relationalProperties = model.Relationships
            .Where(r => r.FromClassName == umlClass.Name &&
                        (r.Type == RelationshipType.Composition ||
                         r.Type == RelationshipType.Aggregation ||
                         r.Type == RelationshipType.Association))
            .ToList();

        var hasProperties = (umlClass.Properties is not null
                             && umlClass.Properties.Count != 0)
                            || relationalProperties.Count != 0;

        if (hasProperties)
        {
            if (umlClass.Properties != null)
            {
                foreach (var prop in umlClass.Properties)
                {
                    var accessModifier = GetAccessModifierString(prop.AccessModifier);
                    var javaType = MapToJavaType(prop.Type);
                    sb.AppendLine($"    {accessModifier} {javaType} {prop.Name};");
                }
            }

            foreach (var relProp in relationalProperties)
            {
                var javaType = MapToJavaType(relProp.ToClassName);
                var propName = char.ToLower(relProp.ToClassName[0]) + relProp.ToClassName.Substring(1);
                sb.AppendLine($"    public {javaType} {propName};");
            }

            sb.AppendLine();
        }

        if (umlClass.Methods != null)
        {
            foreach (var method in umlClass.Methods)
            {
                var accessModifier = GetAccessModifierString(method.AccessModifier);
                var returnType = MapToJavaType(method.ReturnType);
                var parameters = GenerateParameters(method.Parameters);

                sb.AppendLine($"    {accessModifier} {returnType} {method.Name}({parameters}) {{");
                sb.AppendLine("        throw new UnsupportedOperationException();");
                sb.AppendLine("    }");
            }
        }

        sb.AppendLine("}");
    }

    private static string GenerateParameters(List<UmlParameter>? parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return string.Empty;

        var paramStrings = parameters.Select(p => $"{MapToJavaType(p.Type)} {p.Name}");
        return string.Join(", ", paramStrings);
    }

    private static string GetAccessModifierString(AccessModifier modifier)
    {
        return modifier switch
        {
            AccessModifier.Private => "private",
            AccessModifier.Protected => "protected",
            AccessModifier.Public => "public",
            _ => "public"
        };
    }

    private static string MapToJavaType(string type)
    {
        var lowerType = type.ToLower();
        return lowerType switch
        {
            "string" => "String",
            "bool" => "boolean",
            "object" => "Object",
            "datetime" => "java.util.Date",
            "guid" => "java.util.UUID",
            _ => type
        };
    }
}