namespace Core.Domain.Models;

public class UmlProperty
{
    public Visibility Visibility { get; set; } = Visibility.Public;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "object";
    public bool IsStatic { get; set; }
    public bool IsReadOnly { get; set; }
}