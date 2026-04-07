using Core.Application.Services;
using Core.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests.UmlParsers;

[TestFixture]
public class UmlRelationshipsTests
{
    [Test]
    public void PlantUmlParser_ShouldParseAllRelationships()
    {
        // Arrange
        const string uml = @"
        @startuml
        Child --|> Parent
        Service ..|> IService
        Order *-- LineItem
        @enduml";

        var parser = new PlantUmlParser();

        // Act
        var result = parser.Parse(uml);

        // Assert
        result.Relationships.Should().HaveCount(3);

        var inheritance = result.Relationships.First(r => r.Type == RelationshipType.Inheritance);
        inheritance.FromClassName.Should().Be("Child");
        inheritance.ToClassName.Should().Be("Parent");

        var composition = result.Relationships.First(r => r.Type == RelationshipType.Composition);
        composition.FromClassName.Should().Be("Order");
        composition.ToClassName.Should().Be("LineItem");
    }

    [Test]
    public void XmlUmlParser_ShouldParseRelationships()
    {
        // Arrange 
        const string xml = @"
        <UmlDiagram>
            <Relationships>
                <Relationship From=""Car"" To=""Engine"" Type=""Composition"" />
                <Relationship From=""Worker"" To=""IWorker"" Type=""Realization"" />
            </Relationships>
        </UmlDiagram>";

        var parser = new XmlUmlParser();

        // Act
        var result = parser.Parse(xml);

        // Assert
        result.Relationships.Should().HaveCount(2);
        result.Relationships.Should().Contain(r => r.FromClassName == "Car" && r.Type == RelationshipType.Composition);
        result.Relationships.Should()
            .Contain(r => r.ToClassName == "IWorker" && r.Type == RelationshipType.Realization);
    }
}