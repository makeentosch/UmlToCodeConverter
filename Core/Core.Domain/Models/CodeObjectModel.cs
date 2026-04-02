namespace Core.Domain.Models;

/// <summary>
/// Объектная модель, на основе которой будет происходить генерация кода на ЯП.
/// </summary>
public class CodeObjectModel
{
    public List<UmlClass> Classes { get; set; } = new();
    public List<UmlInterface> Interfaces { get; set; } = new();
    public List<UmlEnum> Enums { get; set; } = new();
    public List<UmlRelationship> Relationships { get; set; } = new();
}