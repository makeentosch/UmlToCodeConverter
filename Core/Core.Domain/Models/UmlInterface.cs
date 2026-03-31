namespace Core.Domain.Models;

public class UmlInterface : UmlElement
{
    public List<UmlMethod> Methods { get; set; } = new();
}