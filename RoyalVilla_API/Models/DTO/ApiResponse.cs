using System.Net;

namespace RoyalVilla_API.Models.DTO;

public class ApiResponse<TData>
{
    public bool Success { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; } = string.Empty; 
    public TData? Data { get; set; }
    public object? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; 

    public static ApiResponse<TData> Create(
        bool success, HttpStatusCode statusCode, string message, TData? data=default, object? errors=null
    ) => new() {
            Success = success,
            StatusCode = statusCode,
            Message = message,
            Data = data,
            Errors = errors
        };
    
    // Success responses
    public static ApiResponse<TData> Ok(TData data, string message) =>
        Create(true, HttpStatusCode.OK, message, data);

    public static ApiResponse<TData> CreatedAt(TData data, string message) =>
        Create(true, HttpStatusCode.Created, message, data);

    public static ApiResponse<TData> NoContent(string message="Operation completed successfully") => 
        Create(true, HttpStatusCode.NoContent, message);


    // Failure responses
    public static ApiResponse<TData> NotFound(string message="Resource not found") => 
        Create(false, HttpStatusCode.NotFound, message); 

    public static ApiResponse<TData> BadRequest(string message, object? errors=null) =>
        Create(false, HttpStatusCode.BadRequest, message, errors: errors);

    public static ApiResponse<TData> Conflict(string message) =>
        Create(false, HttpStatusCode.Conflict, message);

    public static ApiResponse<TData> Error(HttpStatusCode statusCode, string message, object? errors=null) =>
        Create(false, statusCode, message, errors: errors);
}