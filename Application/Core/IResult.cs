namespace Application.Core;

public interface IResult
{
    bool IsSuccess { get; }
    string? ErrorMessage { get; }
}
