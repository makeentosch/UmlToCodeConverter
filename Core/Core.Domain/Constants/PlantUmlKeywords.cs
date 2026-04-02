namespace Core.Domain.Constants;

public static class PlantUmlKeywords
{
    public const string StartUml = "@startuml";
    public const string EndUml = "@enduml";

    public const string OpenBrace = "{";
    public const string CloseBrace = "}";

    public const string MethodIndicator = "()";
    public const char TypeSeparator = ':';
    public const char Space = ' ';

    public const string DefaultReturnType = "void";
    public const string DefaultPropertyType = "object";

    public static class Declarations
    {
        public const string Class = "class";
        public const string Interface = "interface";
        public const string Enum = "enum";
    }

    public static class Relationships
    {
        public const string Inheritance = "--|>";
        public const string Realization = "..|>";
        public const string Association = "-->";
        public const string Aggregation = "o--"; 
        public const string Composition = "*--";
        public const string Dependency = "..>";
    }

    public static class AccessModifiers
    {
        public const string Public = "+";
        public const string Private = "-";
        public const string Protected = "#";
    }
}