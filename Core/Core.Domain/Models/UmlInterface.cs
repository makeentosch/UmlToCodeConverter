namespace Core.Domain.Models;

public class UmlInterface : UmlElement
{
    public List<UmlProperty> Properties { get; set; } = new();
    public List<UmlMethod> Methods { get; set; } = new();
}