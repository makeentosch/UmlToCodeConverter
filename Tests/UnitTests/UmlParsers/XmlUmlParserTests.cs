using Core.Application.Exceptions;
using Core.Application.Services;
using Core.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTests.UmlParsers;

[TestFixture]
public class XmlUmlParserTests
{
    private XmlUmlParser _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new XmlUmlParser();
    }

    [Test]
    public void Parse_ValidXmlWithClass_ShouldReturnCorrectUmlClass()
    {
        // Arrange
        const string xml = @"
        <UmlDiagram>
            <Classes>
                <Class Name=""User"">
                    <Properties>
                        <Property Name=""Id"" Type=""int"" AccessModifier=""-"" />
                        <Property Name=""Name"" Type=""string"" AccessModifier=""+"" />
                    </Properties>
                    <Methods>
                        <Method Name=""Login"" ReturnType=""bool"" AccessModifier=""+"" />
                    </Methods>
                </Class>
            </Classes>
        </UmlDiagram>";

        // Act
        var result = _sut.Parse(xml);

        // Assert
        result.Classes.Should().HaveCount(1);
        var userClass = result.Classes.First();
        userClass.Name.Should().Be("User");

        userClass.Properties.Should().HaveCount(2);
        userClass.Properties.First().Name.Should().Be("Id");
        userClass.Properties.First().Type.Should().Be("int");
        userClass.Properties.First().AccessModifier.Should().Be(AccessModifier.Private);

        userClass.Methods.Should().HaveCount(1);
        userClass.Methods.Single().Name.Should().Be("Login");
        userClass.Methods.Single().ReturnType.Should().Be("bool");
        userClass.Methods.Single().AccessModifier.Should().Be(AccessModifier.Public);
    }

    [Test]
    public void Parse_ValidXmlWithInterfaceAndEnum_ShouldReturnCorrectModels()
    {
        // Arrange
        const string xml = @"
        <UmlDiagram>
            <Interfaces>
                <Interface Name=""IService"">
                    <Methods>
                        <Method Name=""Execute"" ReturnType=""void"" />
                    </Methods>
                </Interface>
            </Interfaces>
            <Enums>
                <Enum Name=""Role"">
                    <Values>
                        <Value>Admin</Value>
                        <Value>User</Value>
                    </Values>
                </Enum>
            </Enums>
        </UmlDiagram>";

        // Act
        var result = _sut.Parse(xml);

        // Assert
        result.Interfaces.Should().HaveCount(1);
        result.Interfaces.Single().Name.Should().Be("IService");
        result.Interfaces.Single().Methods.Should().HaveCount(1);

        result.Enums.Should().HaveCount(1);
        result.Enums.Single().Name.Should().Be("Role");
        result.Enums.Single().Values.Should().ContainInOrder("Admin", "User");
    }

    [Test]
    public void ValidateUml_InvalidXmlFormat_ShouldThrowFormatException()
    {
        // Arrange & Act
        var act = () => _sut.Parse("<UmlDiagram><Classes><Class Name=\"Test\">");

        // Assert
        act.Should().Throw<InvalidUmlException>();
    }

    [Test]
    public void ValidateUml_MissingRootTag_ShouldThrowFormatException()
    {
        // Arrange & Act
        var act = () => _sut.Parse("<NotUmlDiagram></NotUmlDiagram>");

        // Assert
        act.Should().Throw<InvalidUmlException>();
    }

    [Test]
    public void Parse_EmptyString_ShouldReturnEmptyDiagram()
    {
        // Arrange & Act
        var result = _sut.Parse(string.Empty);

        // Assert
        result.Should().NotBeNull();
        result.Classes.Should().BeEmpty();
        result.Interfaces.Should().BeEmpty();
        result.Enums.Should().BeEmpty();
    }
}