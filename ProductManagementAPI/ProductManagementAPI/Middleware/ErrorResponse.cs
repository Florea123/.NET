using System.Collections.Generic;

namespace ProductManagementAPI.Middleware;

public class ErrorResponse
{
    public ErrorResponse()
    {
        Details = new List<string>();
    }

    public ErrorResponse(string errorCode, string message) : this()
    {
        ErrorCode = errorCode;
        Message = message;
    }

    public ErrorResponse(string errorCode, string message, List<string> details) : this(errorCode, message)
    {
        Details = details ?? new List<string>();
    }

    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public List<string> Details { get; set; }
}