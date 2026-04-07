using Core.Application.Services;
using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Infrastructure.CodeGenerators;
using Core.Infrastructure.Constants;
using FluentAssertions;
using NUnit.Framework;

namespace E2E;

[TestFixture]
public class EndToEndCSharpGeneration
{
    private CSharpCodeGenerator _csharpCodeGenerator;

    [SetUp]
    public void SetUp()
    {
        _csharpCodeGenerator = new CSharpCodeGenerator();
    }

    [Test]
    public void Generate_ComplexClass_ShouldCorrectlyCombineInheritancePropertiesMethodsAndComposition()
    {
        // Arrange
        var codeObjectModel = new CodeObjectModel();

        var employeeClass = new UmlClass
        {
            Name = "Employee",
            Properties = new List<UmlProperty>
            {
                new()
                {
                    Name = "Id",
                    Type = "Guid",
                    AccessModifier = AccessModifier.Public
                },
                new()
                {
                    Name = "Salary",
                    Type = "decimal",
                    AccessModifier = AccessModifier.Private
                }
            },
            Methods = new List<UmlMethod>
            {
                new()
                {
                    Name = "CalculateBonus",
                    ReturnType = "decimal",
                    AccessModifier = AccessModifier.Public
                }
            }
        };
        codeObjectModel.Classes.Add(employeeClass);
        codeObjectModel.Relationships.Add(new UmlRelationship
        {
            FromClassName = "Employee",
            ToClassName = "Person",
            Type = RelationshipType.Inheritance
        });
        codeObjectModel.Relationships.Add(new UmlRelationship
        {
            FromClassName = "Employee",
            ToClassName = "IWorker",
            Type = RelationshipType.Realization
        });
        codeObjectModel.Relationships.Add(new UmlRelationship
        {
            FromClassName = "Employee",
            ToClassName = "Department",
            Type = RelationshipType.Composition
        });

        // Act
        var result = _csharpCodeGenerator.Generate(codeObjectModel);

        // Assert
        result.Should().Contain("public class Employee : Person, IWorker");

        result.Should().Contain("public Guid Id { get; set; }");
        result.Should().Contain("private decimal Salary { get; set; }");
        result.Should().Contain("public Department Department { get; set; }");

        result.Should().Contain("public decimal CalculateBonus()");
        result.Should().Contain(CSharpKeywords.NotImplementedMethodBody);
    }

    [Test]
    public void EndToEnd_PlantUmlToCSharp_ShouldGenerateValidArchitecture()
    {
        // Arrange
        const string plantUmlText = @"
        @startuml
        interface IRepository
        
        class BaseEntity {
            + Id: int
        }

        class UserRepository {
            - ConnectionString: string
            + GetUser(): User
        }

        UserRepository --|> BaseEntity
        UserRepository ..|> IRepository
        UserRepository *-- DatabaseContext
        @enduml";

        var parser = new PlantUmlParser();

        // Act
        var diagram = parser.Parse(plantUmlText);
        var csharpCode = _csharpCodeGenerator.Generate(diagram);

        // Assert
        csharpCode.Should().Contain("public interface IRepository");

        csharpCode.Should().Contain("public class BaseEntity");
        csharpCode.Should().Contain("public int Id { get; set; }");

        csharpCode.Should().Contain("public class UserRepository : BaseEntity, IRepository");
        csharpCode.Should().Contain("private string ConnectionString { get; set; }");
        csharpCode.Should().Contain("public DatabaseContext DatabaseContext { get; set; }");
        csharpCode.Should().Contain("public User GetUser()");
    }

    [Test]
    public void Generate_MultipleInheritance_ShouldGenerateCommaSeparatedBaseTypes()
    {
        // Arrange
        var diagram = new CodeObjectModel();
        diagram.Classes.Add(new UmlClass
        {
            Name = "Service"
        });
        diagram.Relationships.Add(new UmlRelationship
        {
            FromClassName = "Service",
            ToClassName = "BaseService",
            Type = RelationshipType.Inheritance
        });
        diagram.Relationships.Add(new UmlRelationship
        {
            FromClassName = "Service",
            ToClassName = "IDisposable",
            Type = RelationshipType.Realization
        });
        diagram.Relationships.Add(new UmlRelationship
        {
            FromClassName = "Service",
            ToClassName = "ILogger",
            Type = RelationshipType.Realization
        });

        // Act
        var result = _csharpCodeGenerator.Generate(diagram);

        // Assert
        result.Should().Contain("public class Service : BaseService, IDisposable, ILogger");
    }
}