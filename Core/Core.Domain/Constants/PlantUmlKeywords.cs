namespace Core.Domain.Constants;

public static class PlantUmlKeywords
{
    public const string StartUml = "@startuml";
    public const string EndUml = "@enduml";
    public const string ClassDeclaration = "class ";
    public const string BlockEnd = "}";

    public const string MethodIndicator = "()";
    public const char TypeSeparator = ':';

    public static class AccessModifiers
    {
        public const string Public = "+";
        public const string Private = "-";
        public const string Protected = "#";
    }
}