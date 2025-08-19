using Domain;

namespace Application.Manufacturing;

public class RoutingResult
{ 
    public WorkEffort Routing { get; set; }

    public List<WorkEffortAssoc> Tasks { get; set; }
}