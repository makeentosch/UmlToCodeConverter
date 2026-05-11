using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Infrastructure.Validation.Rules;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests;

[TestFixture]
public class ValidationRulesTests
{
    [Test]
    public void MultipleInheritanceRule_WhenJavaAndMultipleInheritance_ShouldReturnError()
    {
        var model = new CodeObjectModel();
        model.Relationships.Add(new UmlRelationship { FromClassName = "Child", ToClassName = "Base1", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship { FromClassName = "Child", ToClassName = "Base2", Type = RelationshipType.Inheritance });

        var rule = new MultipleInheritanceRule();
        var errors = rule.Validate(model, "Java").ToList();

        errors.Should().ContainSingle();
        errors.First().Should().Contain("does not support multiple class inheritance");
    }

    [Test]
    public void MultipleInheritanceRule_WhenCppAndMultipleInheritance_ShouldNotReturnError()
    {
        var model = new CodeObjectModel();
        model.Relationships.Add(new UmlRelationship { FromClassName = "Child", ToClassName = "Base1", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship { FromClassName = "Child", ToClassName = "Base2", Type = RelationshipType.Inheritance });

        var rule = new MultipleInheritanceRule();
        var errors = rule.Validate(model, "C++").ToList();

        errors.Should().BeEmpty();
    }

    [Test]
    public void CyclicInheritanceRule_WhenCycleExists_ShouldReturnError()
    {
        var model = new CodeObjectModel();
        model.Relationships.Add(new UmlRelationship { FromClassName = "A", ToClassName = "B", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship { FromClassName = "B", ToClassName = "A", Type = RelationshipType.Inheritance });

        var rule = new CyclicInheritanceRule();
        var errors = rule.Validate(model, "C#").ToList();

        errors.Should().NotBeEmpty();
        errors.First().Should().Contain("Cyclic inheritance");
    }

    [Test]
    public void CyclicInheritanceRule_WhenCycleOfLength3Exists_ShouldReturnError()
    {
        // Arrange: цикл A → B → C → A
        var model = new CodeObjectModel();
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "A", ToClassName = "B", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "B", ToClassName = "C", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "C", ToClassName = "A", Type = RelationshipType.Inheritance });

        var rule = new CyclicInheritanceRule();

        // Act
        var errors = rule.Validate(model, "C#").ToList();

        // Assert
        errors.Should().NotBeEmpty();
        errors.First().Should().Contain("Cyclic inheritance");
    }

    [Test]
    public void CyclicInheritanceRule_WhenCycleOfLength4Exists_ShouldReturnError()
    {
        // Arrange
        // Цикл A -> B -> C -> D -> A
        var model = new CodeObjectModel();
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "A", ToClassName = "B", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "B", ToClassName = "C", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "C", ToClassName = "D", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "D", ToClassName = "A", Type = RelationshipType.Inheritance });

        var rule = new CyclicInheritanceRule();

        // Act
        var errors = rule.Validate(model, "Java").ToList();

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().ContainSingle();
    }

    [Test]
    public void CyclicInheritanceRule_WhenMultipleIndependentCycles_ShouldReturnMultipleErrors()
    {
        // Arrange
        var model = new CodeObjectModel();

        // Цикл 1
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "X", ToClassName = "Y", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "Y", ToClassName = "X", Type = RelationshipType.Inheritance });

        // Цикл 2
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "P", ToClassName = "Q", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "Q", ToClassName = "R", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "R", ToClassName = "P", Type = RelationshipType.Inheritance });

        var rule = new CyclicInheritanceRule();

        // Act
        var errors = rule.Validate(model, "C#").ToList();

        // Assert
        errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Test]
    public void CyclicInheritanceRule_WhenNoCycle_ShouldNotReturnError()
    {
        // Arrange
        var model = new CodeObjectModel();
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "Child", ToClassName = "Parent", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "Parent", ToClassName = "GrandParent", Type = RelationshipType.Inheritance });

        var rule = new CyclicInheritanceRule();

        // Act
        var errors = rule.Validate(model, "C#").ToList();

        // Assert
        errors.Should().BeEmpty();
    }

    [Test]
    public void CyclicInheritanceRule_WhenSelfInheritance_ShouldReturnError()
    {
        // Arrange
        var model = new CodeObjectModel();
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "Self", ToClassName = "Self", Type = RelationshipType.Inheritance });

        var rule = new CyclicInheritanceRule();

        // Act
        var errors = rule.Validate(model, "C#").ToList();

        // Assert
        errors.Should().NotBeEmpty();
        errors.First().Should().Contain("Self");
    }

    [Test]
    public void MethodOverloadingRule_WhenGoAndOverloadedMethods_ShouldReturnError()
    {
        var model = new CodeObjectModel();
        var umlClass = new UmlClass
        {
            Name = "Logger",
            Methods = new List<UmlMethod>
            {
                new() { Name = "Log" },
                new() { Name = "Log" }
            }
        };
        model.Classes.Add(umlClass);

        var rule = new MethodOverloadingRule();
        var errors = rule.Validate(model, "Go").ToList();

        errors.Should().ContainSingle();
        errors.First().Should().Contain("does not support method overloading");
    }

    [Test]
    public void InterfacePropertiesRule_WhenCsharpAndHasProperties_ShouldNotReturnError()
    {
        var model = new CodeObjectModel();
        var umlInterface = new UmlInterface
        {
            Name = "IConfig",
            Properties = new List<UmlProperty> { new() { Name = "Timeout" } }
        };
        model.Interfaces.Add(umlInterface);

        var rule = new InterfacePropertiesRule();
        var errors = rule.Validate(model, "C#").ToList();

        errors.Should().BeEmpty();
    }
}