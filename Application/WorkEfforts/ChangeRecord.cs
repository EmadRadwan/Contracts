public class ChangeRecord
{
    public string TableName { get; set; }
    public string PKValues { get; set; }
    public string Operation { get; set; }

    public override string ToString()
    {
        return $"TableName: {TableName} | PKValues: {PKValues} | Operation: {Operation}";
    }
}