using System.Text;
using Core.Application.Interfaces;
using Core.Domain.Enums;
using Core.Domain.Models;

namespace Core.Infrastructure.CodeGenerators;

public class GoCodeGenerator : ICodeGenerator
{
    public string LanguageName => "Go";

    public string Generate(CodeObjectModel objectModel)
    {
        var sb = new StringBuilder();

        sb.AppendLine("package generated");
        sb.AppendLine();

        if (RequiresTimePackage(objectModel))
        {
            sb.AppendLine("import \"time\"");
            sb.AppendLine();
        }

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
            GenerateStruct(umlClass, sb, objectModel);
            sb.AppendLine();
        }

        foreach (var umlClass in objectModel.Classes)
        {
            if (umlClass.Methods != null && umlClass.Methods.Any())
            {
                GenerateMethods(umlClass, sb);
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static bool RequiresTimePackage(CodeObjectModel model)
    {
        return model.Classes.Any(c => c.Properties != null &&
                                      c.Properties.Any(p =>
                                          p.Type.Equals("datetime", StringComparison.OrdinalIgnoreCase)));
    }

    private static void GenerateEnum(UmlEnum umlEnum, StringBuilder sb)
    {
        sb.AppendLine($"type {umlEnum.Name} int");
        sb.AppendLine();

        if (umlEnum.Values.Count == 0)
            return;

        sb.AppendLine("const (");
        for (var i = 0; i < umlEnum.Values.Count; i++)
        {
            var value = umlEnum.Values[i];
            var suffix = i == 0 ? $" {umlEnum.Name} = iota" : string.Empty;
            sb.AppendLine($"\t{umlEnum.Name}_{value}{suffix}");
        }

        sb.AppendLine(")");
    }

    private static void GenerateInterface(UmlInterface umlInterface, StringBuilder sb)
    {
        sb.AppendLine($"type {umlInterface.Name} interface {{");

        foreach (var method in umlInterface.Methods)
        {
            var parameters = GenerateParameters(method.Parameters);
            var returnType = MapToGoType(method.ReturnType);
            var returnString = string.IsNullOrWhiteSpace(returnType) ? string.Empty : $" {returnType}";

            sb.AppendLine($"\t{FormatIdentifier(method.Name, method.AccessModifier)}({parameters}){returnString}");
        }

        sb.AppendLine("}");
    }

    private static void GenerateStruct(UmlClass umlClass, StringBuilder sb, CodeObjectModel model)
    {
        sb.AppendLine($"type {umlClass.Name} struct {{");

        var inheritances = model.Relationships
            .Where(r => r.FromClassName == umlClass.Name && r.Type == RelationshipType.Inheritance)
            .Select(r => r.ToClassName)
            .ToList();

        foreach (var baseStruct in inheritances)
        {
            sb.AppendLine($"\t{baseStruct}");
        }

        if (umlClass.Properties != null)
        {
            foreach (var prop in umlClass.Properties)
            {
                var formattedName = FormatIdentifier(prop.Name, prop.AccessModifier);
                var goType = MapToGoType(prop.Type);
                sb.AppendLine($"\t{formattedName} {goType}");
            }
        }

        var relationalProperties = model.Relationships
            .Where(r => r.FromClassName == umlClass.Name &&
                        (r.Type == RelationshipType.Composition ||
                         r.Type == RelationshipType.Aggregation ||
                         r.Type == RelationshipType.Association))
            .ToList();

        foreach (var relProp in relationalProperties)
        {
            var goType = MapToGoType(relProp.ToClassName);
            var isPointer = relProp.Type == RelationshipType.Aggregation ||
                            relProp.Type == RelationshipType.Association;
            var typePrefix = isPointer ? "*" : string.Empty;

            sb.AppendLine($"\t{relProp.ToClassName} {typePrefix}{goType}");
        }

        sb.AppendLine("}");
    }

    private static void GenerateMethods(UmlClass umlClass, StringBuilder sb)
    {
        var receiverName = umlClass.Name.Substring(0, 1).ToLower();

        foreach (var method in umlClass.Methods!)
        {
            var formattedName = FormatIdentifier(method.Name, method.AccessModifier);
            var parameters = GenerateParameters(method.Parameters);
            var returnType = MapToGoType(method.ReturnType);
            var returnString = string.IsNullOrWhiteSpace(returnType) || returnType == "void"
                ? string.Empty
                : $" {returnType}";

            sb.AppendLine($"func ({receiverName} *{umlClass.Name}) {formattedName}({parameters}){returnString} {{");
            sb.AppendLine("\tpanic(\"implement me\")");
            sb.AppendLine("}");
            sb.AppendLine();
        }
    }

    private static string GenerateParameters(List<UmlParameter>? parameters)
    {
        if (parameters == null || !parameters.Any())
            return string.Empty;

        var paramStrings =
            parameters.Select(p => $"{FormatIdentifier(p.Name, AccessModifier.Private)} {MapToGoType(p.Type)}");
        return string.Join(", ", paramStrings);
    }

    private static string FormatIdentifier(string name, AccessModifier modifier)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        if (modifier == AccessModifier.Public)
        {
            return char.ToUpper(name[0]) + name.Substring(1);
        }

        return char.ToLower(name[0]) + name.Substring(1);
    }

    private static string MapToGoType(string type)
    {
        var lowerType = type.ToLower();
        return lowerType switch
        {
            "string" => "string",
            "bool" => "bool",
            "int" => "int",
            "decimal" => "float64",
            "double" => "float64",
            "float" => "float32",
            "object" => "any",
            "datetime" => "time.Time",
            "guid" => "string",
            "void" => "",
            _ => type
        };
    }
}