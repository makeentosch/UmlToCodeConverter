namespace Core.Domain.Models;

public class UmlMethod
{
    public Visibility Visibility { get; set; } = Visibility.Public;
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = "void";
    public List<UmlParameter> Parameters { get; set; } = new();
    public bool IsAbstract { get; set; }
    public bool IsStatic { get; set; }
}