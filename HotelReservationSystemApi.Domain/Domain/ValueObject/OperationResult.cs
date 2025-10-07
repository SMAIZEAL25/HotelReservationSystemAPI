namespace HotelReservationSystemAPI.Domain.ValueObject;

// ===== NON-GENERIC RESULT (For operations without return data) =====
// Use this for operations like ConfirmEmail, UpdateProfile, etc.
// Named "OperationResult" to avoid ambiguity
public class OperationResult
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; } = string.Empty;

    private OperationResult() { }

    public static OperationResult Success(string message = "")
    {
        return new OperationResult
        {
            IsSuccess = true,
            Message = message
        };
    }

    public static OperationResult Failure(string message)
    {
        return new OperationResult
        {
            IsSuccess = false,
            Message = message
        };
    }
}


