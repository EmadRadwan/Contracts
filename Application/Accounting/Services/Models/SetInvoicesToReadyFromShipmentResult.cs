namespace Application.Accounting.Services.Models;

public class SetInvoicesToReadyFromShipmentResult
{
    // Indicates if the result represents an error.
    public bool IsError { get; set; }
    // An optional message that can describe the error or provide success information.
    public string Message { get; set; }
    // An object that can hold additional data (if needed).
    public object AdditionalData { get; set; }

    // Returns a successful result.
    public static SetInvoicesToReadyFromShipmentResult ReturnSuccess()
    {
        return new SetInvoicesToReadyFromShipmentResult { IsError = false };
    }

    // Returns an error result with a given message.
    public static SetInvoicesToReadyFromShipmentResult ReturnError(string message)
    {
        return new SetInvoicesToReadyFromShipmentResult { IsError = true, Message = message };
    }

    // Overload for ReturnError that accepts additional parameters.
    public static SetInvoicesToReadyFromShipmentResult ReturnError(string message, object param1 = null, object param2 = null, object additional = null)
    {
        return new SetInvoicesToReadyFromShipmentResult { IsError = true, Message = message, AdditionalData = additional };
    }
}
