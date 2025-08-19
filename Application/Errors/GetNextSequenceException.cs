namespace Application.Errors;

public class GetNextSequenceException : Exception
{
    public GetNextSequenceException(string message) : base(message)
    {
    }

    public GetNextSequenceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}