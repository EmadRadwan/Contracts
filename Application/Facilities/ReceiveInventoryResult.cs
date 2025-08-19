namespace Application.Facilities;

public class ReceiveInventoryResult
{
    public bool Success { get; private set; }
    public string Message { get; private set; }
    public object Data { get; private set; }

    public ReceiveInventoryResult SetSuccess(bool success)
    {
        Success = success;
        return this;
    }

    public ReceiveInventoryResult SetMessage(string message)
    {
        Message = message;
        return this;
    }

    public ReceiveInventoryResult SetData(object data)
    {
        Data = data;
        return this;
    }
}

