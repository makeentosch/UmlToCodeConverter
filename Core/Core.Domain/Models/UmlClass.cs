namespace Core.Domain.Models;

public class UmlClass : UmlElement
{
    public string Name { get; set; }
    public List<UmlProperty>? Properties { get; set; }
    public List<UmlMethod>? Methods { get; set; }
}