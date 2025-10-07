namespace HotelReservationSystemAPI.Domain.ValueObject;

// Generic result class
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public T? Data { get; private set; }

    private Result() { }

    public static Result<T> Success(T data, string message = "")
    {
        return new Result<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    public static Result<T> Failure(string message)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Data = default
        };
    }
}


