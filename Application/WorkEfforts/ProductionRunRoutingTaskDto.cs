using Application.Catalog.Products;
using Domain;

namespace Application.WorkEfforts;

    public class ProductionRunRoutingTaskDto
    {
        public string WorkEffortId { get; set; }
        public string WorkEffortParentId { get; set; }
        public string WorkEffortName { get; set; }
        public string Description { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public int? Priority { get; set; }

        public decimal? QuantityToProduce { get; set; }
        public string CurrentStatusId { get; set; }
        public string CurrentStatusDescription { get; set; }
        public DateTime? EstimatedStartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public double? EstimatedSetupMillis { get; set; }
        public double? EstimatedMilliSeconds { get; set; }
        public string FixedAssetId { get; set; }
        public string FixedAssetName { get; set; }
        public int? SequenceNum { get; set; }
        public bool CanStartTask { get; set; }
        public bool CanCompleteTask { get; set; }
        public bool CanDeclareTask { get; set; }
        public bool IsFinalTask { get; set; }
        public decimal? QuantityProduced { get; set; }
        public decimal? QuantityRejected { get; set; }
        public string CanDeclareAndProduce { get; set; }
        public bool BomReservationInProgress { get; set; }

        public string CanProduce { get; set; }
        public string LastLotId { get; set; }
        public double? ActualMilliSeconds { get; set; }
        public double? ActualSetupMillis { get; set; }
        public bool AreComponentsIssued { get; set; } // New property to track issuance status

        public List<ProductDto> DelivProducts { get; set; } = new List<ProductDto>();
        public List<WorkEffortInventoryProduced> PrunInventoryProduced { get; set; } = new List<WorkEffortInventoryProduced>();
    }

