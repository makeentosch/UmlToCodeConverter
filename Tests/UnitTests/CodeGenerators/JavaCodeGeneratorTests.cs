using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Infrastructure.CodeGenerators;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests.CodeGenerators;

[TestFixture]
public class JavaCodeGeneratorTests
{
    private JavaCodeGenerator _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new JavaCodeGenerator();
    }

    [Test]
    public void Generate_ClassWithInheritanceAndInterfaces_ShouldUseExtendsAndImplements()
    {
        // Arrange
        var model = new CodeObjectModel();

        var myClass = new UmlClass
        {
            Name = "UserService",
            Properties = new List<UmlProperty>(),
            Methods = new List<UmlMethod>()
        };

        model.Classes.Add(myClass);
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "UserService", ToClassName = "BaseService", Type = RelationshipType.Inheritance });
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "UserService", ToClassName = "IUserService", Type = RelationshipType.Realization });
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "UserService", ToClassName = "IDisposable", Type = RelationshipType.Realization });

        // Act
        var result = _sut.Generate(model);

        // Assert
        result.Should().Contain("public class UserService extends BaseService implements IUserService, IDisposable {");
    }

    [Test]
    public void Generate_TypesAndComposition_ShouldMapTypesCorrectlyAndCamelCaseProperties()
    {
        // Arrange
        var model = new CodeObjectModel();

        var entityClass = new UmlClass
        {
            Name = "User",
            Properties = new List<UmlProperty>
            {
                new() { Name = "Id", Type = "Guid", AccessModifier = AccessModifier.Private },
                new() { Name = "IsActive", Type = "bool", AccessModifier = AccessModifier.Public },
                new() { Name = "Name", Type = "string", AccessModifier = AccessModifier.Public }
            },
            Methods = new List<UmlMethod>()
        };

        model.Classes.Add(entityClass);
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "User", ToClassName = "UserProfile", Type = RelationshipType.Composition });

        // Act
        var result = _sut.Generate(model);

        // Assert
        result.Should().Contain("private java.util.UUID Id;");
        result.Should().Contain("public boolean IsActive;");
        result.Should().Contain("public String Name;");
        result.Should().Contain("public UserProfile userProfile;");
    }

    [Test]
    public void Generate_MethodsAndParameters_ShouldUseUnsupportedOperationException()
    {
        // Arrange
        var model = new CodeObjectModel();

        var processorClass = new UmlClass
        {
            Name = "Processor",
            Properties = new List<UmlProperty>(),
            Methods = new List<UmlMethod>
            {
                new()
                {
                    Name = "ProcessData",
                    ReturnType = "bool",
                    AccessModifier = AccessModifier.Public,
                    Parameters = new List<UmlParameter>
                    {
                        new() { Name = "data", Type = "string" },
                        new() { Name = "count", Type = "int" }
                    }
                }
            }
        };

        model.Classes.Add(processorClass);

        // Act
        var result = _sut.Generate(model);

        // Assert
        result.Should().Contain("public boolean ProcessData(String data, int count) {");
        result.Should().Contain("throw new UnsupportedOperationException();");
    }
}