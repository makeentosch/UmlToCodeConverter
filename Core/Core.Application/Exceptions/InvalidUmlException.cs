namespace Core.Application.Exceptions;

public class InvalidUmlException(string message) 
    : FormatException($"Given UML is invalid. Reason: {message}.")
{
}