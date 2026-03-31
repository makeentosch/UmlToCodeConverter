namespace Core.Domain.Models;

public class UmlClass : UmlElement
{
    public List<UmlProperty> Properties { get; set; } = new();
    public List<UmlMethod> Methods { get; set; } = new();
    public List<string> Interfaces { get; set; } = new();
}