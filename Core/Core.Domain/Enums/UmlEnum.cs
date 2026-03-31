namespace Core.Domain.Models;

public class UmlEnum : UmlElement
{
    public List<string> Values { get; set; } = new();
}