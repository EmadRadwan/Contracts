Summary of Database Tables in OFBiz-like Production Run Creation
This document summarizes the database tables used or created during the creation of a production run in an OFBiz-like system, as implemented in the provided code. Each table's purpose, the types of records it contains (configuration or dynamic), and what these records represent are described. The tables play various roles in defining routings, tasks, material requirements, and tracking production runs.
1. WorkEffort
   Purpose: A versatile table that stores both configuration and dynamic records for various work efforts, such as manufacturing routings, routing tasks, and production runs.
   Records Used/Created:

Configuration Records:
Routings: Represent the overall manufacturing process for a product.
WORK_EFFORT_TYPE_ID = 'ROUTING': Defines the routing (e.g., "Aspirin Manufacturing Process").
CURRENT_STATUS_ID = 'ROU_ACTIVE': Indicates the routing is active.
WORK_EFFORT_NAME: Name of the routing (e.g., "ROUTING_ASPIRIN").
DESCRIPTION: Details the routing process.


Routing Tasks: Individual tasks within a routing.
WORK_EFFORT_TYPE_ID = 'ROU_TASK': Identifies a task in the routing.
WORK_EFFORT_PURPOSE_TYPE_ID = 'ROU_MANUFACTURING': Specifies the task is for manufacturing.
FIXED_ASSET_ID: Links to equipment or assets used in the task.
EstimatedSetupMillis, EstimatedMilliSeconds: Define setup and task duration estimates.




Dynamic Records:
Production Run Header: Represents the production run instance.
WORK_EFFORT_TYPE_ID = 'PROD_ORDER_HEADER': Marks the record as a production run.
WORK_EFFORT_PURPOSE_TYPE_ID = 'WEPT_PRODUCTION_RUN': Indicates the purpose is a production run.
CURRENT_STATUS_ID = 'PRUN_CREATED': Initial status of the production run.
FacilityId, EstimatedStartDate, QuantityToProduce: Specify the facility, start date, and quantity to produce.


Production Run Tasks: Tasks executed as part of the production run.
WORK_EFFORT_TYPE_ID = 'PROD_ORDER_TASK': Marks the record as a task in the production run.
WORK_EFFORT_PURPOSE_TYPE_ID = 'WEPT_PRODUCTION_RUN': Links to the production run purpose.
WorkEffortParentId: References the production run header (PROD_ORDER_HEADER).
EstimatedStartDate, EstimatedCompletionDate: Define task timing.
Priority, ReservPersons: Specify task sequence and resource requirements.





Main Role: Stores both the static configuration of manufacturing processes (routings and tasks) and dynamic instances of production runs and their tasks. It serves as the core entity for tracking work efforts across planning and execution.
2. WorkEffortAssoc
   Purpose: Defines relationships between work efforts, such as linking routing tasks to a routing or production run tasks to a production run header.
   Records Used/Created:

Configuration Records:
Routing Task Associations: Link tasks to a routing.
WORK_EFFORT_ID_FROM: References the routing (ROUTING WorkEffort).
WORK_EFFORT_ID_TO: References the task (ROU_TASK WorkEffort).
WORK_EFFORT_ASSOC_TYPE_ID = 'ROUTING_COMPONENT': Indicates the task is a component of the routing.
SEQUENCE_NUM: Specifies the order of tasks in the routing.




Dynamic Records:
Production Run Task Associations: Link production run tasks to the production run header.
WORK_EFFORT_ID_FROM: References the production run header (PROD_ORDER_HEADER WorkEffort).
WORK_EFFORT_ID_TO: References the production run task (PROD_ORDER_TASK WorkEffort).
WORK_EFFORT_ASSOC_TYPE_ID = 'WORK_EFF_TEMPLATE': Indicates the task is derived from a routing task template.
SEQUENCE_NUM: Maintains the task order from the routing.
FromDate, CreatedStamp, LastUpdatedStamp: Track the validity and creation/update timestamps.





Main Role: Manages hierarchical relationships between work efforts, enabling the structuring of complex manufacturing processes by linking tasks to their parent routings or production runs.
3. WorkEffortGoodStandard
   Purpose: Associates products with work efforts, defining which products are produced or consumed during manufacturing processes.
   Records Used/Created:

Configuration Records:
Routing Product Template: Links a product to a routing.
WORK_EFFORT_ID: References the routing (ROUTING WorkEffort).
PRODUCT_ID: The product being manufactured.
WORK_EFFORT_GOOD_STD_TYPE_ID = 'ROU_PROD_TEMPLATE': Indicates the product is the output of the routing.
FromDate, ThruDate: Define the validity period for the association.




Dynamic Records:
Production Run Output: Links the finished product to the production run.
WORK_EFFORT_ID: References the production run header (PROD_ORDER_HEADER WorkEffort).
PRODUCT_ID: The finished product being produced.
WORK_EFFORT_GOOD_STD_TYPE_ID = 'PRUN_PROD_DELIV': Indicates the product is the delivered output.
STATUS_ID = 'WEGS_CREATED': Initial status of the record.
EstimatedQuantity: Quantity of the product to be produced.


Production Run Input: Links component materials to production run tasks.
WORK_EFFORT_ID: References the production run task (PROD_ORDER_TASK WorkEffort).
PRODUCT_ID: The component material required.
WORK_EFFORT_GOOD_STD_TYPE_ID = 'PRUNT_PROD_NEEDED': Indicates the product is a required input.
STATUS_ID = 'WEGS_CREATED': Initial status.
EstimatedQuantity: Quantity of the component needed, calculated based on the production quantity.





Main Role: Tracks the products involved in manufacturing, distinguishing between outputs (finished goods) and inputs (materials) for both configuration (routing templates) and execution (production runs).
4. ProductAssoc
   Purpose: Defines relationships between products, such as bill of materials (BOM) components or product variants.
   Records Used/Created:

Configuration Records:
Bill of Materials (BOM): Specifies components required to manufacture a product.
PRODUCT_ID: The finished product.
PRODUCT_ID_TO: The component product.
PRODUCT_ASSOC_TYPE_ID = 'MANUF_COMPONENT': Indicates a manufacturing component.
Quantity: Amount of the component needed per unit of the finished product.
RoutingWorkEffortId: Optional; specifies the routing task associated with the component issuance (if null, components are issued with the first task).
FromDate, ThruDate: Define the validity period.


Product Variants: Links a product to its virtual product (if applicable).
PRODUCT_ID_TO: The product being manufactured.
PRODUCT_ASSOC_TYPE_ID = 'PRODUCT_VARIANT': Indicates a variant relationship.
FromDate, ThruDate: Define the validity period.





Main Role: Manages product relationships, particularly the BOM for manufacturing components and variant associations for routing lookups, controlling material issuance in production runs.
5. WorkEffortCostCalc
   Purpose: Stores cost calculations associated with work efforts, such as tasks in a routing or production run.
   Records Used/Created:

Configuration Records:
Routing Task Cost Calculations: Define cost estimates for routing tasks.
WORK_EFFORT_ID: References the routing task (ROU_TASK WorkEffort).
COST_COMPONENT_TYPE_ID: Type of cost (e.g., labor, material).
COST_COMPONENT_CALC_ID: References a cost calculation method or formula.
FromDate, ThruDate: Validity period for the cost calculation.




Dynamic Records:
Production Run Task Cost Calculations: Cloned from routing task cost calculations for production run tasks.
WORK_EFFORT_ID: References the production run task (PROD_ORDER_TASK WorkEffort).
COST_COMPONENT_TYPE_ID, COST_COMPONENT_CALC_ID: Copied from the routing task.
FromDate, CreatedStamp, LastUpdatedStamp: Track validity and creation/update timestamps.





Main Role: Tracks cost estimates for tasks, enabling cost analysis for both planning (routing tasks) and execution (production run tasks).
6. WorkEffortStatus
   Purpose: Maintains the status history of work efforts, particularly for production runs and tasks.
   Records Created:

Dynamic Records:
Production Run Status: Tracks status changes for the production run header.
WORK_EFFORT_ID: References the production run header (PROD_ORDER_HEADER WorkEffort).
STATUS_ID: The new status (e.g., PRUN_CREATED).
STATUS_DATETIME: Timestamp of the status change.
REASON: Optional reason for the status change.
CreatedStamp, LastUpdatedStamp: Creation and update timestamps.





Main Role: Provides an audit trail of status changes for production runs, ensuring traceability of the production process.
7. CustomMethods
   Purpose: Stores custom methods for calculating task times or other parameters, used in task time estimation.
   Records Used:

Configuration Records:
CUSTOM_METHOD_ID: Unique identifier for the custom method.
CUSTOM_METHOD_NAME: Name of the method (used to invoke it programmatically).
Linked to tasks via WorkEffort.EstimateCalcMethod.



Main Role: Allows flexible, custom logic for task time calculations, overriding default estimates when specified.
8. StatusValidChanges
   Purpose: Defines valid status transitions for work efforts to ensure proper state changes.
   Records Used:

Configuration Records:
STATUS_ID: Current status of a work effort.
STATUS_ID_TO: Allowed next status.
Used to validate status changes in UpdateWorkEffort.



Main Role: Ensures that status changes for production runs and tasks follow predefined rules, maintaining data integrity.
9. Products
   Purpose: Stores product information, including finished goods and components.
   Records Used:

Configuration Records:
PRODUCT_ID: Unique identifier for the product.
PRODUCT_NAME: Name of the product (used for display or default naming).
Referenced in WorkEffortGoodStandard and ProductAssoc to identify products in manufacturing.



Main Role: Provides product details for routing, BOM, and production run associations.
10. Facilities
    Purpose: Stores information about facilities where production occurs.
    Records Used:

Configuration Records:
FACILITY_ID: Unique identifier for the facility.
FACILITY_NAME: Name of the facility (used in response for display).
Referenced in WorkEffort for production runs and tasks to specify the location.



Main Role: Identifies the physical location of manufacturing activities.

Summary of Key Processes

Routing Configuration: Uses WorkEffort (for routings and tasks), WorkEffortAssoc (to link tasks to routings), WorkEffortGoodStandard (to associate products with routings), and ProductAssoc (for BOM and variants) to define the manufacturing process template.
Production Run Creation:
Creates a WorkEffort for the production run header (PROD_ORDER_HEADER) and tasks (PROD_ORDER_TASK).
Creates WorkEffortAssoc to link tasks to the production run.
Creates WorkEffortGoodStandard for the finished product (PRUN_PROD_DELIV) and components (PRUNT_PROD_NEEDED).
Clones cost calculations into WorkEffortCostCalc for production run tasks.
Records status changes in WorkEffortStatus.


Material Issuance: Controlled by ProductAssoc.RoutingWorkEffortId, issuing components with specific tasks or the first task if unspecified.
Task Timing: Uses CustomMethods and WorkEffort fields (EstimatedMilliSeconds, EstimatedSetupMillis) to calculate task durations, with calendar services for scheduling.

This structure allows the system to manage both the planning (configuration) and execution (dynamic) aspects of manufacturing, with tables like WorkEffort and WorkEffortAssoc serving multiple roles to support flexibility and scalability.