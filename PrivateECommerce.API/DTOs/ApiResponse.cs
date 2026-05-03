namespace PrivateECommerce.API.DTOs
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public static ApiResponse Ok(string message, object? data = null) =>
            new() { Success = true, Message = message, Data = data };

        public static ApiResponse Fail(string message, object? data = null) =>
            new() { Success = false, Message = message, Data = data };
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(string message, T? data = default) =>
            new() { Success = true, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message, T? data = default) =>
            new() { Success = false, Message = message, Data = data };
    }
}
