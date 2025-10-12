using System.Net;

namespace HotelReservationSystemAPI.Application.CommonResponse
{
    public class APIResponse<T>
    {
        public bool IsSuccess { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        private APIResponse(bool isSuccess, HttpStatusCode statusCode, T? data, string message)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Data = data;
            Message = message;
        }

        public static APIResponse<T> Success(T data, string message = "Operation successful")
            => new(true, HttpStatusCode.OK, data, message);

        public static APIResponse<T> Fail(HttpStatusCode statusCode, string message)
            => new(false, statusCode, default, message);
    }
}
