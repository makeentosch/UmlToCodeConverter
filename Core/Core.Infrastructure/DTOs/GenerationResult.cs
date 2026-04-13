namespace Core.Infrastructure.DTOs;

public class GenerationResult
{
    public string Content { get; set; } = string.Empty;
    public string StatusMessage { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}