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