using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DeveloperStore.Sales.Domain.Dtos.Response;

[ExcludeFromCodeCoverage]
public class ApiResponseDto<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    public static ApiResponseDto<T> AsSuccess(string message = "")
    {
        return new ApiResponseDto<T>
        {
            IsSuccess = true,
            StatusCode = 200,
            Message = string.IsNullOrEmpty(message) ? "Operation successful" : message
        };
    }

    public static ApiResponseDto<T> AsSuccess(T data, string message = "")
    {
        return new ApiResponseDto<T>
        {
            IsSuccess = true,
            StatusCode = 200,
            Data = data,
            Message = string.IsNullOrEmpty(message) ? "Operation successful" : message
        };
    }

    public static ApiResponseDto<T> AsError(string message = "")
    {
        return new ApiResponseDto<T>
        {
            IsSuccess = false,
            StatusCode = 500,
            Message = string.IsNullOrEmpty(message) ? "Internal server error" : message
        };
    }

    public static ApiResponseDto<T> AsInternalServerError(string message = "")
    {
        return new ApiResponseDto<T>
        {
            IsSuccess = false,
            StatusCode = 500,
            Message = string.IsNullOrEmpty(message) ? "Internal Server Error" : message
        };
    }

    public static ApiResponseDto<T> AsBadRequest(string message = "")
    {
        return new ApiResponseDto<T>
        {
            IsSuccess = false,
            StatusCode = 400,
            Message = string.IsNullOrEmpty(message) ? "Bad request" : message
        };
    }

    public static ApiResponseDto<T> AsNotFound(string message = "")
    {
        return new ApiResponseDto<T>
        {
            IsSuccess = false,
            StatusCode = 404,
            Message = string.IsNullOrEmpty(message) ? "Resource not found" : message
        };
    }

    public static ApiResponseDto<T> AsCreated(T data, string message = "")
    {
        return new ApiResponseDto<T>
        {
            IsSuccess = true,
            StatusCode = 201,
            Data = data,
            Message = string.IsNullOrEmpty(message) ? "Resource created successfully" : message
        };
    }

    public static ApiResponseDto<T> AsUnauthorized(string message = "")
    {
        return new ApiResponseDto<T>
        {
            IsSuccess = false,
            StatusCode = 401,
            Message = string.IsNullOrEmpty(message) ? "Unauthorized" : message
        };
    }
}
