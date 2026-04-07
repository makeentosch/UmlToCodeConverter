using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Infrastructure.CodeGenerators;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests.CodeGenerators;

[TestFixture]
public class GoCodeGeneratorTests
{
    private GoCodeGenerator _generator;

    [SetUp]
    public void SetUp()
    {
        _generator = new GoCodeGenerator();
    }

    [Test]
    public void Generate_ClassWithInheritance_ShouldUseStructEmbedding()
    {
        var model = new CodeObjectModel();
        var employeeClass = new UmlClass
        {
            Name = "Employee",
            Properties = new List<UmlProperty>(),
            Methods = new List<UmlMethod>()
        };

        model.Classes.Add(employeeClass);
        model.Relationships.Add(new UmlRelationship
            { FromClassName = "Employee", ToClassName = "Person", Type = RelationshipType.Inheritance });

        var result = _generator.Generate(model);

        result.Should().Contain("type Employee struct {");
        result.Should().Contain("\tPerson");
    }

    [Test]
    public void Generate_PropertiesWithAccessModifiers_ShouldUsePascalAndCamelCase()
    {
        var model = new CodeObjectModel();
        var configClass = new UmlClass
        {
            Name = "Config",
            Properties = new List<UmlProperty>
            {
                new() { Name = "MaxConnections", Type = "int", AccessModifier = AccessModifier.Public },
                new() { Name = "secretKey", Type = "string", AccessModifier = AccessModifier.Private }
            },
            Methods = new List<UmlMethod>()
        };

        model.Classes.Add(configClass);

        var result = _generator.Generate(model);

        result.Should().Contain("\tMaxConnections int");
        result.Should().Contain("\tsecretKey string");
    }

    [Test]
    public void Generate_ClassWithMethods_ShouldGenerateReceiverFunctions()
    {
        var model = new CodeObjectModel();
        var workerClass = new UmlClass
        {
            Name = "Worker",
            Properties = new List<UmlProperty>(),
            Methods = new List<UmlMethod>
            {
                new()
                {
                    Name = "ProcessTask",
                    ReturnType = "bool",
                    AccessModifier = AccessModifier.Public,
                    Parameters = new List<UmlParameter>
                    {
                        new() { Name = "taskId", Type = "string" }
                    }
                }
            }
        };

        model.Classes.Add(workerClass);

        var result = _generator.Generate(model);

        result.Should().Contain("func (w *Worker) ProcessTask(taskId string) bool {");
        result.Should().Contain("\tpanic(\"implement me\")");
    }

    [Test]
    public void Generate_Enum_ShouldGenerateIotaConstants()
    {
        var model = new CodeObjectModel();
        var statusEnum = new UmlEnum
        {
            Name = "Status",
            Values = new List<string> { "Active", "Inactive", "Deleted" }
        };

        model.Enums.Add(statusEnum);

        var result = _generator.Generate(model);

        result.Should().Contain("type Status int");
        result.Should().Contain("Status_Active Status = iota");
        result.Should().Contain("Status_Inactive");
        result.Should().Contain("Status_Deleted");
    }
}