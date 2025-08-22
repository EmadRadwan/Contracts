namespace Domain;

public class Annotation
{
    public string AnnotationId { get; set; }
    public float XCoordinate { get; set; }
    public float YCoordinate { get; set; }
    public string Note { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }

    //public virtual ICollection<VehicleAnnotation> VehicleAnnotations { get; set; } = new List<VehicleAnnotation>();
}