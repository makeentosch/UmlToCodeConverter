namespace Core.Domain.Models;

public abstract class UmlElement
{
    public string Name { get; set; } = string.Empty;
    public string? Stereotype { get; set; }
    public bool IsAbstract { get; set; }
}