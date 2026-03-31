namespace Core.Domain.Enums;

public enum RelationshipType
{
    Inheritance,      // <|-- 
    Realization,      // ..|>
    Association,      // -->
    Aggregation,      // o--
    Composition,      // *--
    Dependency        // ..>
}