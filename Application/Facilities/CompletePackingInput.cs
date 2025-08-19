using Application.Shipments;

namespace Application.Facilities
{
    public class CompletePackingInput
    {
        /// <summary>
        /// If true, forces the completion process even if there are reservation mismatches.
        /// </summary>
        public bool ForceComplete { get; set; }

        /// <summary>
        /// List of packing line details.
        /// </summary>
        public List<PackingLineDto> Lines { get; set; } = new();

        /// <summary>
        /// The highest package sequence number currently used.
        /// </summary>
        public int PackageSeq { get; set; }

        /// <summary>
        /// The facility identifier where packing is taking place.
        /// </summary>
        public string FacilityId { get; set; }

        /// <summary>
        /// The primary order identifier for the packing session.
        /// </summary>
        public string PrimaryOrderId { get; set; }

        /// <summary>
        /// The primary ship group identifier.
        /// </summary>
        public string PrimaryShipGroupId { get; set; }

        /// <summary>
        /// The picklist bin identifier, if applicable.
        /// </summary>
        public string PicklistBinId { get; set; }

        /// <summary>
        /// Handling instructions for the shipment.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// The identifier for the picker party.
        /// </summary>
        public string PickerPartyId { get; set; }

        /// <summary>
        /// Any additional shipping charge to be applied.
        /// </summary>
        public decimal? AdditionalShippingCharge { get; set; }

        /// <summary>
        /// The weight unit of measure identifier.
        /// </summary>
        public string WeightUomId { get; set; }
        public string HandlingInstructions { get; set; }

        /// <summary>
        /// A dictionary mapping package sequence keys (as strings) to their weights.
        /// </summary>
        public IDictionary<string, string> PackageWeights { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// A dictionary mapping package sequence keys (as strings) to their shipment box types.
        /// </summary>
        public IDictionary<string, string> BoxTypes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Optionally, an existing packing session instance can be provided.
        /// </summary>
        public PackingSession? PackingSession { get; set; }
    }
}
