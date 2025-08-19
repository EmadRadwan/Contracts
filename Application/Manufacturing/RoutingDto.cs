using Domain;

namespace Application.Manufacturing;

public class RoutingDto
{
    // The routing work effort details
    public WorkEffort Routing { get; set; }
    
    // The list of tasks associated with the routing
    public List<WorkEffortAssoc> Tasks { get; set; }
}