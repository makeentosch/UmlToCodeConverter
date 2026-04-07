using Core.Application.Exceptions;
using Core.Application.Services;
using Core.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests.UmlParsers;

public class PlantUmlParserTests
{
    private PlantUmlParser _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new PlantUmlParser();
    }

    [Test]
    public void Parse_ValidClass_ReturnsCorrectUmlClass()
    {
        // Arrange
        var uml = @"
        @startuml
        class User {
            -Id: int
            +Name: string
            +Login(): bool
        }
        @enduml";

        // Act
        var result = _sut.Parse(uml);

        // Assert
        result.Classes.Should().HaveCount(1);
        result.Classes.Single().Methods.Should().HaveCount(1);
        result.Classes.Single().Properties.Should().HaveCount(2);

        var userClass = result.Classes.Single();
        var singleMethod = userClass.Methods!.Single();

        userClass.Name.Should().Be("User");

        userClass.Properties!.First().Name.Should().Be("Id");
        userClass.Properties!.First().Type.Should().Be("int");
        userClass.Properties!.First().AccessModifier.Should().Be(AccessModifier.Private);

        singleMethod.Name.Should().Be("Login");
        singleMethod.ReturnType.Should().Be("bool");
        singleMethod.AccessModifier.Should().Be(AccessModifier.Public);
    }

    [Test]
    public void Parse_ValidInterfaceAndEnum_ReturnsCorrectModels()
    {
        // Arrange
        var uml = @"
        @startuml
        interface IService {
            +Execute(): void
        }
        enum Role {
            Admin
            User
            Guest
        }
        @enduml";

        // Act
        var result = _sut.Parse(uml);

        // Assert
        result.Interfaces.Should().HaveCount(1);
        result.Interfaces.Single().Name.Should().Be("IService");
        result.Interfaces.Single().Methods.Should().HaveCount(1);

        Assert.That(result.Enums, Has.Count.EqualTo(1));
        Assert.That(result.Enums[0].Name, Is.EqualTo("Role"));
        Assert.That(result.Enums[0].Values, Has.Count.EqualTo(3));
        Assert.That(result.Enums[0].Values, Does.Contain("Admin"));
    }

    [Test]
    public void ValidateUml_MissingStartTag_ThrowsFormatException()
    {
        // Arrange
        var uml = @"
        class Test {
        }
        @enduml";

        // Act & Assert
        Assert.Throws<InvalidUmlException>(() => _sut.Parse(uml));
    }

    [Test]
    public void ValidateUml_MismatchedBraces_ThrowsFormatException()
    {
        // Arrange
        var uml = @"
        @startuml
        class Test {
            +Prop: int
    
        @enduml";

        // Act & Assert
        Assert.Throws<InvalidUmlException>(() => _sut.Parse(uml));
    }

    [Test]
    public void Parse_EmptyString_ReturnsEmptyDiagramWithoutThrowing()
    {
        // Arrange
        var uml = string.Empty;

        // Act
        var result = _sut.Parse(uml);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Classes, Is.Empty);
        Assert.That(result.Interfaces, Is.Empty);
        Assert.That(result.Enums, Is.Empty);
    }
}