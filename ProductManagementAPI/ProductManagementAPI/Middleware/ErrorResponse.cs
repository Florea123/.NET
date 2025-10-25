namespace UserManagement.Middleware;

public class ErrorResponse()
{
    public ErrorResponse(string errorCode,string messsage) : this()
    {
        ErrorCode = errorCode;
        Message = messsage;
    }

    public ErrorResponse(string errorCode, string message, List<string> details) : this(errorCode, message)
    {
        
    }
    public string Message { get; set; }

    public string ErrorCode { get; set; }
    public string TraceId { get; set; }
}