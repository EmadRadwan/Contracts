using Domain;

namespace Application.Manufacturing;

public class ManufacturingComponentsResult
{
    public string WorkEffortId { get; set; }
    public List<ProductAssoc> Components { get; set; }
    public List<ComponentMap> ComponentsMap { get; set; }
    public List<WorkEffortAssoc> RoutingTasks { get; set; }
}