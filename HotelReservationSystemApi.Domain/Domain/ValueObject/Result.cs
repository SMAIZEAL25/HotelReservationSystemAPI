namespace HotelReservationSystemAPI.Domain.ValueObject;

// Generic result class
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }

    protected Result() { }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}

// OperationResult.cs (non-generic for behaviors)
public class OperationResult
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }

    protected OperationResult() { }

    public static OperationResult Success(string message = "Success") => new() { IsSuccess = true };
    public static OperationResult Failure(string error) => new() { IsSuccess = false, Error = error };
}

// ValueObject base
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode() => GetEqualityComponents().Aggregate(0, (a, v) => HashCode.Combine(a, v?.GetHashCode() ?? 0));

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);
    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}



