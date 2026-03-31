namespace Core.Domain.Models;

public enum RelationshipType
{
    Inheritance,      // <|-- 
    Realization,      // ..|>
    Association,      // -->
    Aggregation,      // o--
    Composition,      // *--
    Dependency        // ..>
}