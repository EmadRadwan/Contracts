namespace Application.Shipments
{
    public class CreateShipmentReceiptResult
    {
        /// <summary>
        /// The unique identifier for the created shipment receipt.
        /// </summary>
        public string ReceiptId { get; set; }

        /// <summary>
        /// Indicates whether accounting was affected by the shipment receipt.
        /// </summary>
        public bool AffectAccounting { get; set; }

        /// <summary>
        /// An error message if an error occurred during the creation process.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
    }
}
