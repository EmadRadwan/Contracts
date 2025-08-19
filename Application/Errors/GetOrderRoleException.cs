namespace Application.Errors;

public class GetOrderRoleException : Exception
{
    public GetOrderRoleException(string message) : base(message)
    {
    }

    public GetOrderRoleException(string message, Exception innerException) : base(message, innerException)
    {
    }
}