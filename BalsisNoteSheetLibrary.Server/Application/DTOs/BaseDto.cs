namespace BalsisNoteSheetLibrary.Server.Application.DTOs;

public class BaseResponseDto<T>(T? data, bool success = true, string? message = null)
{
    public T? Data { get; set; } = data;
    public bool Success { get; set; } = success;
    public string? Message { get; set; } = message;
}

public class BaseResponseDto(string? message = null, bool success = true)
{
    public bool Success { get; set; } = success;
    public string? Message { get; set; } = message;
}
