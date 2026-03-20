namespace MiniSocialNetwork.Wrappers
{
    public class ApiResponse<T>
    {
        public T? Result { get; set; }
        public bool Success { get; set; } = true;
        public string? ErrorCode { get; set; }
        public string? Error { get; set; }
        public object? Validates { get; set; }

        public ApiResponse() { }

        public ApiResponse(T result)
        {
            Result = result;
            Success = true;
        }

        public static ApiResponse<T> Fail(string error, string? errorCode = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = error,
                ErrorCode = errorCode
            };
        }

        public static ApiResponse<T> Ok(T result)
        {
            return new ApiResponse<T>(result);
        }
    }
}
