using Core.Domain.Models;

// ReSharper disable InvertIf

namespace Core.Infrastructure.Validation.Rules;

public class UniqueNameRule : IValidationRule
{
    public IEnumerable<string> Validate(CodeObjectModel? model, string targetLanguage)
    {
        if (model is null)
            yield break;

        var nameRegistry = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var cls in model.Classes)
        {
            if (string.IsNullOrWhiteSpace(cls.Name))
                continue;

            if (!nameRegistry.TryGetValue(cls.Name, out var types))
            {
                types = new List<string>();
                nameRegistry[cls.Name] = types;
            }

            types.Add("class");
        }

        foreach (var e in model.Interfaces)
        {
            if (string.IsNullOrWhiteSpace(e.Name))
                continue;

            if (!nameRegistry.TryGetValue(e.Name, out var types))
            {
                types = new List<string>();
                nameRegistry[e.Name] = types;
            }

            types.Add("interface");
        }

        foreach (var entry in nameRegistry)
        {
            var name = entry.Key;
            var types = entry.Value;

            var grouped = types.GroupBy(t => t)
                .ToDictionary(g => g.Key, g => g.Count());

            if (grouped.TryGetValue("class", out var classCount) && classCount > 1)
                yield return $"Duplicate class name \"{name}\" found.";

            if (grouped.TryGetValue("interface", out var interfaceCount) && interfaceCount > 1)
                yield return $"Duplicate interface name \"{name}\" found.";

            if (grouped.ContainsKey("class") && grouped.ContainsKey("interface"))
                yield return $"Name conflict: \"{name}\" is used for both a class and an interface. " +
                             $"In {targetLanguage}, classes and interfaces must have distinct names.";
        }
    }
}