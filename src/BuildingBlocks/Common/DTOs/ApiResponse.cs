namespace Common.DTOs;

/// <summary>
/// Standardized API Response Wrapper
/// 
/// This generic wrapper provides consistent response structure across all microservices.
/// 
/// Benefits of Standardized Responses:
/// ===================================
/// 1. Predictability: Clients always know the response format
/// 2. Error Handling: Consistent error structure simplifies client-side error handling
/// 3. Metadata: Can include pagination, timing, version info
/// 4. Success Indication: Clear success/failure status
/// 5. Multiple Errors: Can return multiple validation errors at once
/// 
/// Response Structure:
/// {
///   "success": true/false,
///   "message": "Human-readable message",
///   "data": { ... actual response data ... },
///   "errors": [ ... list of error messages ... ]
/// }
/// 
/// Example Success Response:
/// {
///   "success": true,
///   "message": "Product retrieved successfully",
///   "data": { "id": "123", "name": "Laptop", ... },
///   "errors": []
/// }
/// 
/// Example Error Response:
/// {
///   "success": false,
///   "message": "Validation failed",
///   "data": null,
///   "errors": ["Price must be greater than 0", "Name is required"]
/// }
/// 
/// This pattern is also known as the "Result Pattern" or "Operation Result Pattern"
/// </summary>
public class ApiResponse<T>
{
    /// <summary>Indicates if the operation was successful</summary>
    public bool Success { get; set; }
    
    /// <summary>Human-readable message describing the result</summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>The actual data payload (null if error)</summary>
    public T? Data { get; set; }
    
    /// <summary>List of error messages (empty if successful)</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Creates a successful response with data
    /// Used when operations complete successfully
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates an error response with optional error list
    /// Used when validation fails or operations encounter errors
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
