using Core.Application.Services;
using Core.Domain.Enums;
using Core.Domain.Models;
using Core.Infrastructure.CodeGenerators;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests;

[TestFixture]
public class MethodParamsTests
{
    private CSharpCodeGenerator _generator;

    [SetUp]
    public void SetUp()
    {
        _generator = new CSharpCodeGenerator();
    }

    [Test]
    public void PlantUmlParser_ShouldParseMethodWithMultipleParameters()
    {
        // Arrange
        var plantUmlText = @"
        @startuml
        class AuthenticationService {
            + Login(username: string, passwordHash: byte[]): bool
        }
        @enduml";
        var parser = new PlantUmlParser();

        // Act
        var model = parser.Parse(plantUmlText);

        // Assert
        var methods = model.Classes.First().Methods;
        methods.Should().HaveCount(1);

        var method = methods.First();
        method.Name.Should().Be("Login");
        method.ReturnType.Should().Be("bool");

        method.Parameters.Should().HaveCount(2);
        method.Parameters.First().Name.Should().Be("username");
        method.Parameters.First().Type.Should().Be("string");
        method.Parameters[1].Name.Should().Be("passwordHash");
        method.Parameters[1].Type.Should().Be("byte[]");
    }

    [Test]
    public void Generator_ShouldGenerateMethodWithParametersCorrectly()
    {
        // Arrange
        var codeObjectModel = new CodeObjectModel();

        var testClass = new UmlClass
        {
            Name = "AuthenticationService",
            Properties = new List<UmlProperty>(),
            Methods = new List<UmlMethod>
            {
                new()
                {
                    Name = "Login",
                    ReturnType = "bool",
                    AccessModifier = AccessModifier.Public,
                    Parameters = new List<UmlParameter>
                    {
                        new() { Name = "username", Type = "string" },
                        new() { Name = "passwordHash", Type = "byte[]" }
                    }
                }
            }
        };

        codeObjectModel.Classes.Add(testClass);

        // Act
        var result = _generator.Generate(codeObjectModel);

        // Assert
        result.Should().Contain("public bool Login(string username, byte[] passwordHash)");
    }

    [Test]
    public void XmlParser_ShouldParseMethodWithParameters()
    {
        // Arrange
        const string xml = @"
        <UmlDiagram>
            <Classes>
                <Class Name=""Calculator"">
                    <Methods>
                        <Method Name=""Add"" ReturnType=""int"" AccessModifier=""+"">
                            <Parameters>
                                <Parameter Name=""a"" Type=""int"" />
                                <Parameter Name=""b"" Type=""int"" />
                            </Parameters>
                        </Method>
                    </Methods>
                </Class>
            </Classes>
        </UmlDiagram>";
        var parser = new XmlUmlParser();

        // Act
        var model = parser.Parse(xml);

        // Assert
        var parameters = model.Classes.First().Methods!.First().Parameters;

        parameters.Should().HaveCount(2);
        parameters.First().Name.Should().Be("a");
        parameters.First().Type.Should().Be("int");
    }
}