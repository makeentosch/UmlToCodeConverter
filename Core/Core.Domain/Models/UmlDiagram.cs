namespace Core.Domain.Models;

public class UmlDiagram
{
    public List<UmlClass> Classes { get; set; } = new();
    public List<UmlInterface> Interfaces { get; set; } = new();
    public List<UmlEnum> Enums { get; set; } = new();
    public List<UmlRelationship> Relationships { get; set; } = new();

    public UmlClass? FindClass(string name) => 
        Classes.FirstOrDefault(c => c.Name == name);
}