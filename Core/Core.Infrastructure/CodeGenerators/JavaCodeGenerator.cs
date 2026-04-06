namespace Core.Infrastructure.CodeGenerators;

using System.Text;
using Core.Application.Interfaces;
using Core.Domain.Enums;
using Core.Domain.Models;

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
            GenerateClass(umlClass, sb);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static void GenerateEnum(UmlEnum umlEnum, StringBuilder sb)
    {
        sb.AppendLine($"public enum {umlEnum.Name} {{");

        if (umlEnum.Values.Count > 0)
        {
            sb.AppendLine($"    {string.Join(", ", umlEnum.Values)}");
        }

        sb.AppendLine("}");
    }

    private static void GenerateInterface(UmlInterface umlInterface, StringBuilder sb)
    {
        sb.AppendLine($"public interface {umlInterface.Name} {{");

        foreach (var method in umlInterface.Methods)
        {
            var parameters = GenerateParameters(method.Parameters);
            var returnType = MapToJavaType(method.ReturnType);
            sb.AppendLine($"    {returnType} {method.Name}({parameters});");
        }

        sb.AppendLine("}");
    }

    private static void GenerateClass(UmlClass umlClass, StringBuilder sb)
    {
        sb.AppendLine($"public class {umlClass.Name} {{");

        foreach (var property in umlClass.Properties)
        {
            var accessModifier = GetAccessModifierString(property.AccessModifier);
            var javaType = MapToJavaType(property.Type);
            sb.AppendLine($"    {accessModifier} {javaType} {property.Name};");
        }

        if (umlClass.Properties.Count > 0 && umlClass.Methods.Count > 0)
        {
            sb.AppendLine();
        }

        foreach (var method in umlClass.Methods)
        {
            var accessModifier = GetAccessModifierString(method.AccessModifier);
            var returnType = MapToJavaType(method.ReturnType);
            var parameters = GenerateParameters(method.Parameters);

            sb.AppendLine($"    {accessModifier} {returnType} {method.Name}({parameters}) {{");
            sb.AppendLine("        throw new UnsupportedOperationException();");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
    }

    private static string GenerateParameters(List<UmlParameter> parameters)
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
        return type.ToLower() switch
        {
            "string" => "String",
            "bool" => "boolean",
            "int" => "int",
            "double" => "double",
            "float" => "float",
            "void" => "void",
            _ => type
        };
    }
}