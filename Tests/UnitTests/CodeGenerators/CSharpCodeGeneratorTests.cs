using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Infrastructure.CodeGenerators;
using Core.Infrastructure.Constants;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests.CodeGenerators;

[TestFixture]
public class CSharpCodeGeneratorTests
{
    private CSharpCodeGenerator _generator;

    [SetUp]
    public void SetUp()
    {
        _generator = new CSharpCodeGenerator();
    }

    [Test]
    public void LanguageName_ShouldBeCSharp()
    {
        // Act
        var result = _generator.LanguageName;

        // Assert
        result.Should().Be("C#");
    }

    [Test]
    public void Generate_WithValidEnum_ShouldGenerateCorrectEnumCode()
    {
        // Arrange
        var diagram = new CodeObjectModel();
        diagram.Enums.Add(new UmlEnum
        {
            Name = "Status",
            Values = new List<string> { "Active", "Inactive", "Deleted" }
        });

        // Act
        var result = _generator.Generate(diagram);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("public enum Status");
        result.Should().Contain("Active,");
        result.Should().Contain("Inactive,");
        result.Should().Contain("Deleted");
        result.Should().NotContain("Deleted,");
    }

    [Test]
    public void Generate_WithValidInterface_ShouldGenerateCorrectInterfaceCode()
    {
        // Arrange
        var diagram = new CodeObjectModel();
        diagram.Interfaces.Add(new UmlInterface
        {
            Name = "IUserService",
            Methods = new List<UmlMethod>
            {
                new() { Name = "GetUser", ReturnType = "User" },
                new() { Name = "DeleteUser", ReturnType = "void" }
            }
        });

        // Act
        var result = _generator.Generate(diagram);

        // Assert
        result.Should().Contain("public interface IUserService");
        result.Should().Contain("User GetUser();");
        result.Should().Contain("void DeleteUser();");
        result.Should().NotContain("public User GetUser();");
    }

    [Test]
    public void Generate_WithValidClass_ShouldGeneratePropertiesAndNotImplementedMethods()
    {
        // Arrange
        var diagram = new CodeObjectModel();
        diagram.Classes.Add(new UmlClass
        {
            Name = "Customer",
            Properties = new List<UmlProperty>
            {
                new() { Name = "Id", Type = "int", AccessModifier = AccessModifier.Public },
                new() { Name = "PasswordHash", Type = "string", AccessModifier = AccessModifier.Private }
            },
            Methods = new List<UmlMethod>
            {
                new()
                {
                    Name = "Validate", ReturnType = "bool", AccessModifier = AccessModifier.Public,
                    Parameters = new List<UmlParameter>()
                }
            }
        });

        // Act
        var result = _generator.Generate(diagram);

        // Assert
        result.Should().Contain($"{CSharpKeywords.Public} {CSharpKeywords.ClassDeclaration} Customer");

        result.Should().Contain($"{CSharpKeywords.Public} int Id {CSharpKeywords.AutoProperty}");
        result.Should().Contain($"{CSharpKeywords.Private} string PasswordHash {CSharpKeywords.AutoProperty}");

        result.Should().Contain($"{CSharpKeywords.Public} bool Validate()");
        result.Should().Contain(CSharpKeywords.NotImplementedMethodBody);
    }

    [Test]
    public void Generate_EmptyDiagram_ShouldReturnEmptyString()
    {
        // Arrange
        var diagram = new CodeObjectModel();

        // Act
        var result = _generator.Generate(diagram);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Generate_ClassWithInheritanceAndRealization_ShouldGenerateBaseTypes()
    {
        // Arrange
        var diagram = new CodeObjectModel();
        diagram.Classes.Add(new UmlClass
        {
            Name = "Repository",
            Properties = new List<UmlProperty>(),
            Methods = new List<UmlMethod>()
        });
        diagram.Relationships.Add(new UmlRelationship
        {
            FromClassName = "Repository",
            ToClassName = "BaseRepository",
            Type = RelationshipType.Inheritance
        });
        diagram.Relationships.Add(new UmlRelationship
        {
            FromClassName = "Repository",
            ToClassName = "IRepository",
            Type = RelationshipType.Realization
        });

        // Act
        var result = _generator.Generate(diagram);

        // Assert
        result.Should().Contain("public class Repository : BaseRepository, IRepository");
    }

    [Test]
    public void Generate_ClassWithComposition_ShouldGenerateProperty()
    {
        // Arrange
        var diagram = new CodeObjectModel();
        diagram.Classes.Add(new UmlClass
        {
            Name = "Order",
            Properties = new List<UmlProperty>(),
            Methods = new List<UmlMethod>()
        });
        diagram.Relationships.Add(new UmlRelationship
        {
            FromClassName = "Order",
            ToClassName = "Customer",
            Type = RelationshipType.Association
        });

        // Act
        var result = _generator.Generate(diagram);

        // Assert
        result.Should().Contain("public class Order");
        result.Should().Contain("public Customer Customer { get; set; }");
    }
}