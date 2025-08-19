namespace Application.Facilities;

public class PackOrderResult
{
    public bool Success { get; private set; }
    public string Message { get; private set; }
    public object Data { get; private set; }

    public PackOrderResult SetSuccess(bool success)
    {
        Success = success;
        return this;
    }

    public PackOrderResult SetMessage(string message)
    {
        Message = message;
        return this;
    }

    public PackOrderResult SetData(object data)
    {
        Data = data;
        return this;
    }
}

