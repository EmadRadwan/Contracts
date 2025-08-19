namespace Application.Facilities;

public class PackBulkLine
{
    public bool IsSelected { get; set; }            // Instead of selInfo in Groovy
    public string OrderItemSeqId { get; set; }      // Instead of ite_[rowKey]
    public string ProductId { get; set; }           // Instead of prd_[rowKey]
    public string PackageStr { get; set; }          // Instead of pkg_[rowKey]
    public string QuantityStr { get; set; }         // Instead of qty_[rowKey]
    public string WeightStr { get; set; }           // Instead of wgt_[rowKey]
    public string NumPackagesStr { get; set; }      // Instead of numPackages_[rowKey]
}
