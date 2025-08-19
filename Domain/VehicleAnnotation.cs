namespace Domain;

public class VehicleAnnotation
{
    public string VehicleAnnotationId { get; set; }
    public string VehicleId { get; set; }
    public string AnnotationId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }

    public virtual Vehicle Vehicle { get; set; }
    public virtual Annotation Annotation { get; set; }
}