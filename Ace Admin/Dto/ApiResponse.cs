using System.Net;

namespace Ace_Admin.Dto
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }

        public ApiResponse(bool success, string message, HttpStatusCode statusCode, T? data = default)
        {
            Success = success;
            Message = message;
            StatusCode = (int)statusCode; // convert enum to int
            Data = data;
        }

        // ✅ Optional: helper factory methods
        public static ApiResponse<T> Ok(T data, string message = "Success")
            => new ApiResponse<T>(true, message, HttpStatusCode.OK, data);

        public static ApiResponse<T> BadRequest(string message = "Bad Request")
            => new ApiResponse<T>(false, message, HttpStatusCode.BadRequest);

        public static ApiResponse<T> Unauthorized(string message = "Unauthorized")
            => new ApiResponse<T>(false, message, HttpStatusCode.Unauthorized);

        public static ApiResponse<T> NotFound(string message = "Not Found")
            => new ApiResponse<T>(false, message, HttpStatusCode.NotFound);

        public static ApiResponse<T> InternalServerError(string message = "Internal Server Error")
            => new ApiResponse<T>(false, message, HttpStatusCode.InternalServerError);

    }
}
