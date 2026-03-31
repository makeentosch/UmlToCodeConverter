namespace Core.Domain.Models;

public class UmlRelationship
{
    public string FromClassName { get; set; } = string.Empty;
    public string ToClassName { get; set; } = string.Empty;
    public RelationshipType Type { get; set; }
    public string? Multiplicity { get; set; }
    public string? RoleName { get; set; }
}