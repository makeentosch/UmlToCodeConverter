using Core.Domain.Enums;

namespace Core.Domain.Models;

public class UmlMethod
{
    public string Name { get; set; }
    public string ReturnType { get; set; }
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public List<UmlParameter> Parameters { get; set; } = new();
}