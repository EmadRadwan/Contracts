using Application.Core;

public class Result<T> : IResult
{
    public bool IsSuccess { get; set; }
    public T Value { get; set; }
    public string Error { get; set; }
    public int? Count { get; set; } // Make Count nullable

    string? IResult.ErrorMessage => Error;

    public static Result<T> Success(T value, int? count = null)
    {
        return new Result<T> { IsSuccess = true, Value = value, Count = count };
    }

    public static Result<T> Failure(string error)
    {
        return new Result<T> { IsSuccess = false, Error = error };
    }
}