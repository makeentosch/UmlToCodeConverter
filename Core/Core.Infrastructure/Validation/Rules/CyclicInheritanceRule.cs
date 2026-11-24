using Core.Domain.Enums;
using Core.Domain.Models;

// ReSharper disable InvertIf

namespace Core.Infrastructure.Validation.Rules;

public class CyclicInheritanceRule : IValidationRule
{
    public IEnumerable<string> Validate(CodeObjectModel model, string targetLanguage)
    {
        var graph = model.Relationships
            .Where(r => r.Type == RelationshipType.Inheritance)
            .GroupBy(r => r.FromClassName)
            .ToDictionary(g => g.Key, g => g.Select(r => r.ToClassName).ToList());

        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        foreach (var className in graph.Keys)
        {
            if (!visited.Contains(className))
            {
                var currentPath = new Stack<string>();

                if (HasCycle(className, graph, visited, recursionStack, currentPath))
                {
                    var pathList = currentPath.ToList();
                    pathList.Reverse();

                    var cycleStart = pathList.Last();
                    var cycleStartIndex = pathList.IndexOf(cycleStart);
                    var actualCycle = pathList.Skip(cycleStartIndex).ToList();

                    yield return $"Cyclic inheritance detected: {string.Join(" -> ", actualCycle)}.";
                }
            }
        }
    }

    private static bool HasCycle(string current, Dictionary<string, List<string>> graph,
        HashSet<string> visited, HashSet<string> recursionStack, Stack<string> path)
    {
        visited.Add(current);
        recursionStack.Add(current);
        path.Push(current);

        if (graph.TryGetValue(current, out var neighbors))
        {
            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    if (HasCycle(neighbor, graph, visited, recursionStack, path))
                        return true;
                }
                else if (recursionStack.Contains(neighbor))
                {
                    path.Push(neighbor);
                    return true;
                }
            }
        }

        recursionStack.Remove(current);
        path.Pop();

        return false;
    }
}