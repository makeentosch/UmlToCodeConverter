using Core.Domain.Enums;

namespace Core.Domain.Models;

public class UmlProperty
{
    public string Name { get; set; }
    public string Type { get; set; }
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Private;
}