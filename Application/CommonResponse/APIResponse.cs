using System.Net;

namespace HotelReservationSystemAPI.Application.CommonResponse
{
    public class APIResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }

        public string message { get; set; }

        public T? Data { get; set; }

        public static APIResponse<T> Success(T data, string message = "Success")
            => new APIResponse<T>
            {
                StatusCode = HttpStatusCode.OK,
                message = message,
                Data = default
            };
        public static APIResponse<T> Fail(HttpStatusCode statusCode, string message)
            => new APIResponse<T>
            {
                StatusCode = statusCode,
                message = message,
                Data = default
            };
    }
}
