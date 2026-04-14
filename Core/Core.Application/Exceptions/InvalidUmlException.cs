namespace Core.Application.Exceptions;

public class InvalidUmlException(string message, Exception? innerException = null)
    : FormatException($"Given UML is invalid. Reason: {message}.", innerException)
{
}