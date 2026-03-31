namespace Core.Domain.Models;

public class UmlClass : UmlElement
{
    public string Name { get; set; }
    public List<UmlProperty>? Properties { get; set; }
    public List<UmlMethod>? Methods { get; set; }

    public UmlClass(string name, List<UmlProperty>? properties = null, List<UmlMethod>? methods = null)
    {
        Name = name;
        Properties = properties;
        Methods = methods;
    }
}