// REFACTOR: Import required components for column menu filtering
// This replaces the default filter row with a more controlled menu-based approach,
// preventing immediate filter application and invalid queries when no value is entered.
import {
    Grid,
    GridColumn as Column,
    // ... your other imports
    GridColumnMenuFilter,      // Built-in filter component for the menu
    GridColumnMenuCheckboxFilter, // Optional: for distinct values checkbox list if needed
} from "@progress/kendo-react-grid";

// REFACTOR: Add a reusable column menu component
// This ensures consistent filtering behavior across columns and avoids immediate triggers.
const DefaultColumnMenu = (props: any) => {
    return (
        <div>
            <GridColumnMenuFilter {...props} expanded={true} />
        </div>
    );
};

// Inside your KendoGrid component:
<KendoGrid
    style={{ height: "65vh", flex: 1 }}
    data={payments || { data: [], total: 0 }}
    resizable={true}
    filterable={true}              // Keep this to enable filtering
    sortable={true}
    pageable={true}
    {...dataState}
    onDataStateChange={dataStateChange}
>
    {/* REFACTOR: Apply columnMenu to each column for menu-based filtering */}
    {/* This changes filtering to popup menu with Apply button, fixing the 'undefined' error */}
    <Column
        field="paymentId"
        title={getTranslatedLabel(`${localizationKey}.paymentId`, "Payment Number")}
        cell={PaymentDescriptionCell}
        width={150}
        columnMenu={DefaultColumnMenu}   // Menu mode
    />
    {/* Repeat columnMenu={DefaultColumnMenu} for other columns as needed */}
    {/* Especially important for daysUntilDue */}
    <Column
        field="daysUntilDue"
        title={getTranslatedLabel(`${localizationKey}.dueStatus`, "Due Status")}
        width={220}
        cell={DueStatusCell}
        filter="numeric"                 // Keep numeric type
        columnMenu={DefaultColumnMenu}   // Critical: uses menu with explicit Apply
    />
    {/* ... other columns with columnMenu={DefaultColumnMenu} */}
</KendoGrid>