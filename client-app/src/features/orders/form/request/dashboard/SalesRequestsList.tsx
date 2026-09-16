import React, { useState } from "react";
import { useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridFilterCellProps,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { DatePicker, DatePickerChangeEvent } from "@progress/kendo-react-dateinputs";
import { DropDownList, DropDownListChangeEvent } from "@progress/kendo-react-dropdowns";
import { Button as KendoButton } from "@progress/kendo-react-buttons";
import { filterIcon, filterClearIcon } from "@progress/kendo-svg-icons";
import { Grid, Paper } from "@mui/material";

import Button from "@mui/material/Button";
import {DataResult, State} from "@progress/kendo-data-query";
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";
import {useFetchSalesRequestsQuery} from "../../../../../app/store/apis/salesRequestApi";
import {SalesRequest} from "../../../../../app/models/order/SalesRequest";
import SalesRequestForm from "../form/SalesRequestForm";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import SalesRequestMenu from "../menu/SalesRequestMenu";
import { Can } from "../../../../account/Can";
import { SALES_COMMISSION_ENTRY_ROLES } from "../../../../../app/models/orders/salesCommissionRoles";
import {handleDatesArray} from "../../../../../app/util/utils";
import InstallmentPriceCalculatorModal from "./InstallmentPriceCalculatorModal";
import ModalContainer from "../../../../../app/common/modals/ModalContainer";
import {SalesRequestsAndApartmentsDateRangeExcel} from "../report/SalesRequestsAndApartmentsDateRangeExcel";
import {useFetchProjectsLovQuery} from "../../../../../app/store/apis/projectsApi";
import {useFetchPartiesEmployeesLovQuery} from "../../../../../app/store/apis/partiesApi";
import {TextFilterCell, NumericFilterCell, createSelectFilterCell} from "../../../../../app/common/grid";
import "../../../../../app/common/grid/grid.styles.css";

function SalesRequestsList() {
    const navigate = useNavigate();
    // -----------------------------------------------------------------
    // State: editMode + selected full object
    // -----------------------------------------------------------------
    const [editMode, setEditMode] = useState(0); // 0=list, 1=create, 2=edit
    const [selectedSR, setSelectedSR] = useState<SalesRequest | undefined>(undefined);
    const [sRequests, setSRequests] = React.useState<DataResult>({
        data: [],
        total: 0,
    });
    const [viewMode, setViewMode] = useState<"list" | "form">("list"); // NEW
    const [showCalculator, setShowCalculator] = useState(false);

    const { getTranslatedLabel } = useTranslationHelper();

    // REFACTOR: ViewSalesRequest grants read-only access; only CreateSalesRequest allows create/edit
    const roles = useSelector((state: any) => state.account.user?.roles || []);
    const canCreate = roles.includes("CreateSalesRequest");

    // REFACTOR: the status column filters/sorts on statusId (a real SalesRequest column),
    // not statusDescription — statusDescription is a server-side projection built from a
    // Dictionary lookup in ListSalesRequestsQuery.cs (SALES_REQUEST_STATUS StatusItems),
    // and EF Core can't translate a filter/orderby against it into SQL (500s). statusId is
    // stable across languages; only the dropdown's display text needs localizing.
    const language = useSelector((state: any) => state.localization.language);
    const statusFilterOptions = language === "ar"
        ? [
            { text: "تم الإنشاء", value: "SALES_REQUEST_CREATED" },
            { text: "تم الاعتماد", value: "SALES_REQUEST_APPROVED" },
        ]
        : [
            { text: "Created", value: "SALES_REQUEST_CREATED" },
            { text: "Approved", value: "SALES_REQUEST_APPROVED" },
        ];

    // Same statusId/statusDescription split, applied to apartment status
    // (APARTMENT_STATUS StatusItems: 3 values today).
    const apartmentStatusFilterOptions = language === "ar"
        ? [
            { text: "متاح", value: "APARTMENT_AVAILABLE" },
            { text: "محجوز", value: "APARTMENT_RESERVED" },
            { text: "مباع", value: "APARTMENT_SOLD" },
        ]
        : [
            { text: "Available", value: "APARTMENT_AVAILABLE" },
            { text: "Reserved", value: "APARTMENT_RESERVED" },
            { text: "Sold", value: "APARTMENT_SOLD" },
        ];

    // Same split, applied to floor. floorMap (ListSalesRequestsQuery.cs) only ever produces
    // Arabic labels regardless of language — that's a separate pre-existing gap, left as-is —
    // so these option labels intentionally match what the column actually displays in both languages.
    const floorFilterOptions = [
        { text: "الطابق الأرضي", value: "0" },
        { text: "الطابق الأول", value: "1" },
        { text: "الطابق الثاني", value: "2" },
        { text: "الطابق الثالث", value: "3" },
        { text: "الطابق الرابع", value: "4" },
        { text: "الطابق الخامس", value: "5" },
        { text: "الطابق السادس", value: "6" },
    ];

    // Same split, applied to project. Unlike status/apartment-status/floor (small fixed
    // constants), the project list is real business data (WorkEffort rows), so it's fetched
    // via the same LOV endpoint other project pickers in the app already use, rather than
    // hardcoded — a project added later shows up in the dropdown without a code change.
    const { data: projectsLovData } = useFetchProjectsLovQuery();
    const projectFilterOptions = (projectsLovData?.projects ?? [])
        .map((p) => ({ text: p.projectName, value: p.workEffortId }))
        .sort((a, b) => a.text.localeCompare(b.text));

    // Same split, applied to sales employee. Filters on employeePartyId (already a real,
    // directly-exposed SalesRequest column, no dictionary lookup involved) while displaying
    // employeeName. Options come from the same "EMPLOYEE" role LOV the create/edit form's
    // employee picker (FormComboBoxVirtualPartyEmployee) already uses.
    const { data: employeesLovData } = useFetchPartiesEmployeesLovQuery();
    const employeeFilterOptions = (employeesLovData?.parties ?? [])
        .map((p) => ({ text: p.fromPartyName, value: p.fromPartyId }))
        .sort((a, b) => a.text.localeCompare(b.text));

    // -----------------------------------------------------------------
    // Grid data
    // -----------------------------------------------------------------
    const [dataState, setDataState] = React.useState<State>({
        sort: [
            {
                field: "createdStamp",
                dir: "desc",
            },
        ],
        skip: 0,
        take: 9,
    });
    const { data, isFetching } = useFetchSalesRequestsQuery({...dataState});

    React.useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setSRequests({ data: adjustedData, total: data!.totalCount});
        }
    }, [data]);
    
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    // -----------------------------------------------------------------
    // Unified handlers – only two
    // -----------------------------------------------------------------
    const startEdit = (sr?: SalesRequest) => {
        if (!sr) {
            // Create mode — unreachable for read-only viewers since the Create button is hidden
            setSelectedSR(undefined);
            setEditMode(1);
            setViewMode("form");
            return;
        }

        // Edit or View mode — decide based on status, but read-only viewers always get the
        // read-only form (editMode 3) regardless of status, since they lack CreateSalesRequest
        const isApproved = sr.statusId === "SALES_REQUEST_APPROVED";

        setSelectedSR(sr);
        setEditMode(!canCreate || isApproved ? 3 : 2);  // ← THIS IS THE KEY LINE
        setViewMode("form");
    };

    const cancelEdit = () => {
        setSelectedSR(undefined);
        setEditMode(0);
        setViewMode("list");
    };

    const handleSalesRequestCreated = async (createdRequest: SalesRequest) => {
        // 1. Keep the form open
        // 2. Switch to edit mode
        setEditMode(2);

        setSelectedSR(createdRequest);
        setViewMode("form");
    };

    // -----------------------------------------------------------------
    // Form rendering
    // -----------------------------------------------------------------
    if (viewMode === "form" && editMode !== 0) {
        return (
            <SalesRequestForm
                salesRequest={editMode === 1 ? undefined : selectedSR}   // create → undefined, edit/view → selectedSR
                editMode={editMode}                                      // 1 = create, 2 = edit, 3 = approved (read-only)
                cancelEdit={cancelEdit}
                onSalesRequestCreated={handleSalesRequestCreated}
                onSalesRequestUpdated={(updated) => {
                    setSelectedSR(updated);   // Update the object with fresh data (new status, timestamps, etc.)
                    // Decide edit mode based on status
                    const isApproved = updated.statusId === "SALES_REQUEST_APPROVED";
                    setEditMode(isApproved ? 3 : 2);
                }}
            />
        );
    }

    const handleMenuSelect = (key: string) => {
        if (key === "salesRequest.menu.salesRequests") {
            // REFACTOR: Menu click → force grid-only view, exit any edit mode
            // Purpose: Unmounts the form instantly when the user clicks the menu item
            // Context: Works even when the route does not change
            setViewMode("list");
            setEditMode(0);
            setSelectedSR(undefined);
        }
    };

    // -----------------------------------------------------------------
    // Grid list
    // -----------------------------------------------------------------
    const RequestIdCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{ ...props.style, color: "blue" }}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button onClick={() => startEdit(props.dataItem)}>
                    {props.dataItem.salesRequestId}
                </Button>
            </td>
        );
    };

    const CommissionCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        const isApproved = props.dataItem.statusId === "SALES_REQUEST_APPROVED";
        return (
            <td className={props.className} style={props.style} colSpan={props.colSpan} role="gridcell"
                aria-colindex={props.ariaColumnIndex} aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }} {...navigationAttributes}>
                {isApproved && (
                    <Can perform={SALES_COMMISSION_ENTRY_ROLES}>
                        <Button
                            size="small"
                            variant="outlined"
                            color="warning"
                            onClick={() => navigate("/sales-commissions", { state: { salesRequestId: props.dataItem.salesRequestId } })}
                        >
                            عمولة
                        </Button>
                    </Can>
                )}
            </td>
        );
    };

    // Displays the localized statusDescription while the column itself filters/sorts on
    // statusId (see the REFACTOR note above statusFilterOptions).
    const StatusDescriptionCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={props.style}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                {props.dataItem.statusDescription}
            </td>
        );
    };

    // Same statusId/statusDescription split, for apartment status (filters on apartmentStatusId).
    const ApartmentStatusDescriptionCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={props.style}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                {props.dataItem.apartmentStatusDescription}
            </td>
        );
    };

    // Same split, for floor (filters on floorNumberId, the raw "0".."6" code).
    const FloorDescriptionCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={props.style}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                {props.dataItem.floorNumber}
            </td>
        );
    };

    // Same split, for project (filters on projectId, the raw WorkEffortId).
    const ProjectNameCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={props.style}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                {props.dataItem.projectName}
            </td>
        );
    };

    // Same split, for sales employee (filters on employeePartyId).
    const EmployeeNameCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={props.style}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                {props.dataItem.employeeName}
            </td>
        );
    };

    const IsChequesDeliveredCell = (props: any) => {
        const value = props.dataItem.isBankTransfer === 1 || props.dataItem.isChequesDelivered === true;

        return (
            <td
                style={{
                    ...props.style,
                    textAlign: "center",
                    fontSize: "18px",
                    fontWeight: "bold",
                }}
            >
                {value ? (
                    <span style={{ color: "#2e7d32" }}>✓</span>   // Green check
                ) : (
                    <span style={{ color: "#c62828" }}>✕</span>   // Red cross
                )}
            </td>
        );
    };

    // REFACTOR: Kendo's built-in date FilterCell has no explicit format/locale configured
    // app-wide, so it falls back to a US-style (M/d/y) date input that also misparses
    // dd/MM/yyyy input, silently sending the wrong date to the OData filter.
    // This custom cell mirrors Kendo's default GridFilterCell markup/behavior (operator
    // dropdown + clear button) but pins format/formatPlaceholder to dd/MM/yyyy.
    const SaleDateFilterCell = (props: GridFilterCellProps & { tdProps?: React.TdHTMLAttributes<HTMLTableCellElement> }) => {
        const hasValue = props.value !== null && props.value !== undefined && props.value !== "";
        const defaultOperator = props.operators[0]?.operator as string;

        const handleDateChange = (event: DatePickerChangeEvent) => {
            const value = event.value;
            let operator = props.operator as string;
            if (!operator || operator === "isnull" || operator === "isnotnull") {
                operator = defaultOperator;
            }
            if (value === null && operator === defaultOperator) {
                operator = "";
            }
            props.onChange({ value, operator, syntheticEvent: event.syntheticEvent as React.SyntheticEvent });
        };

        const handleOperatorChange = (event: DropDownListChangeEvent) => {
            if (!event.target.opened) {
                return;
            }
            const item = event.target.value as { text: string; operator: string };
            let value = props.value;
            if (item.operator === "isnull" || item.operator === "isnotnull") {
                value = null;
            } else if (props.value === null) {
                value = undefined;
            }
            props.onChange({ value, operator: item.operator, syntheticEvent: event.syntheticEvent as React.SyntheticEvent });
        };

        const clear = (event: React.SyntheticEvent) => {
            event.preventDefault();
            props.onChange({ value: null, operator: "", syntheticEvent: event });
        };

        const selectedOperator = props.operators.find((op) => op.operator === props.operator) || null;

        return (
            <td {...props.tdProps}>
                <div className="k-filtercell">
                    <div className="k-filtercell-wrapper">
                        <DatePicker
                            value={props.value ?? null}
                            format="dd/MM/yyyy"
                            formatPlaceholder={{ year: "yyyy", month: "mm", day: "dd" }}
                            onChange={handleDateChange}
                            title={props.title}
                            ariaLabel={props.ariaLabel}
                        />
                        <div className="k-filtercell-operator">
                            <DropDownList
                                data={props.operators}
                                textField="text"
                                value={selectedOperator}
                                onChange={handleOperatorChange}
                                iconClassName="k-i-filter k-icon"
                                svgIcon={filterIcon}
                                className="k-dropdown-operator"
                                popupSettings={{ width: "" }}
                            />
                            &nbsp;
                            <KendoButton
                                icon="filter-clear"
                                svgIcon={filterClearIcon}
                                type="button"
                                title={getTranslatedLabel("general.clear", "Clear")}
                                onClick={clear}
                                disabled={!(hasValue || props.operator)}
                            />
                        </div>
                    </div>
                </div>
            </td>
        );
    };

    const StatusFilterCell = createSelectFilterCell(statusFilterOptions, getTranslatedLabel("general.all", "All"));
    const ApartmentStatusFilterCell = createSelectFilterCell(apartmentStatusFilterOptions, getTranslatedLabel("general.all", "All"));
    const FloorFilterCell = createSelectFilterCell(floorFilterOptions, getTranslatedLabel("general.all", "All"));
    const ProjectFilterCell = createSelectFilterCell(projectFilterOptions, getTranslatedLabel("general.all", "All"));
    const EmployeeFilterCell = createSelectFilterCell(employeeFilterOptions, getTranslatedLabel("general.all", "All"));

    return (
        <>
            <SalesRequestMenu
                selectedMenuItem="/sales-requests"
                on_kmMenuSelect={handleMenuSelect}
            />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                className="kendo-grid-styled"
                                style={{ height: "65vh", flex: 1 }}
                                data={sRequests ? sRequests : { data: [], total: 0 }}
                                resizable
                                filterable
                                sortable
                                pageable
                                {...dataState}
                                onDataStateChange={dataStateChange}
                            >
                                <GridToolbar>
                                    <Grid container>
                                        {canCreate && (
                                            <Grid item xs={2}>
                                                <Button
                                                    color="secondary"
                                                    onClick={() => startEdit()}
                                                    variant="outlined"
                                                >
                                                    {getTranslatedLabel("salesRequest.list.create", "Create Sales Request")}
                                                </Button>
                                            </Grid>
                                        )}
                                        <Grid item xs={2}>
                                            <Button
                                                color="primary"
                                                variant="contained"
                                                onClick={() => setShowCalculator(true)}
                                            >
                                                {getTranslatedLabel("installmentCalculator.open", "حاسبة سعر المتر بالتقسيط")}
                                            </Button>
                                        </Grid>
                                        <Grid item xs={2}>
                                            <SalesRequestsAndApartmentsDateRangeExcel getTranslatedLabel={getTranslatedLabel} />
                                        </Grid>
                                    </Grid>

                                    
                                </GridToolbar>

                                <Column
                                    field="salesRequestId"
                                    title={getTranslatedLabel("salesRequest.list.id", "Request ID")}
                                    cells={{ data: RequestIdCell }}
                                    width={130}
                                />
                                <Column
                                    field="apartmentName"
                                    title={getTranslatedLabel("salesRequest.list.apartment", "Apartment")}
                                    filter="text"
                                    cells={{ filterCell: TextFilterCell }}
                                    width={250}
                                />
                                <Column
                                    field="buildingNumber"
                                    title={getTranslatedLabel("salesRequest.list.buildingNumber", "Building Number")}
                                    filter="text"
                                    cells={{ filterCell: TextFilterCell }}
                                    width={100}
                                />
                                {/* Displays floorNumber (localized display text) but filters/sorts on
                                    floorNumberId, the raw "0".."6" code — see FloorDescriptionCell. */}
                                <Column
                                    field="floorNumberId"
                                    title={getTranslatedLabel("salesRequest.list.floorNumber", "Floor")}
                                    filter="text"
                                    cells={{ data: FloorDescriptionCell, filterCell: FloorFilterCell }}
                                    width={110}
                                />
                                <Column
                                    field="fromPartyName"
                                    title={getTranslatedLabel("salesRequest.list.customer", "Customer")}
                                    filter="text"
                                    cells={{ filterCell: TextFilterCell }}
                                    width={200}
                                />
                                <Column
                                    field="employeePartyId"
                                    title={getTranslatedLabel("salesRequest.list.employee", "Employee")}
                                    filter="text"
                                    cells={{ data: EmployeeNameCell, filterCell: EmployeeFilterCell }}
                                    width={200}
                                />
                                <Column
                                    field="statusId"
                                    title={getTranslatedLabel("salesRequest.list.status", "Status")}
                                    filter="text"
                                    cells={{ data: StatusDescriptionCell, filterCell: StatusFilterCell }}
                                    width={200}
                                />
                                {/* Displays apartmentStatusDescription but filters/sorts on apartmentStatusId
                                    — see ApartmentStatusDescriptionCell. */}
                                <Column
                                    field="apartmentStatusId"
                                    title={getTranslatedLabel("salesRequest.list.apartmentStatus", "Apartment Status")}
                                    filter="text"
                                    cells={{ data: ApartmentStatusDescriptionCell, filterCell: ApartmentStatusFilterCell }}
                                    width={140}
                                />
                                <Column
                                    field="saleDate"
                                    title={getTranslatedLabel("salesRequest.list.saleDate", "Sale Date")}
                                    format="{0:dd/MM/yyyy}"
                                    filter="date"
                                    cells={{ filterCell: SaleDateFilterCell }}
                                    width={140}
                                />
                                <Column
                                    field="totalPrice"
                                    title={getTranslatedLabel("salesRequest.list.total", "Total")}
                                    format="{0:n2}"
                                    filter="numeric"
                                    cells={{ filterCell: NumericFilterCell }}
                                    width={130}
                                />
                                <Column
                                    field="isChequesDelivered"
                                    title={getTranslatedLabel("salesRequest.list.isChequesDelivered", "Total")}
                                    filter="boolean"
                                    width={120}
                                    cells={{ data: IsChequesDeliveredCell }}
                                />
                                <Column
                                    field="advancePayment"
                                    title={getTranslatedLabel("salesRequest.list.advance", "Advance")}
                                    format="{0:n2}"
                                    filter="numeric"
                                    cells={{ filterCell: NumericFilterCell }}
                                    width={130}
                                />
                                <Column
                                    field="maintenanceDeposit"
                                    title={getTranslatedLabel("salesRequest.list.maintenanceDeposit", "Maintenance")}
                                    format="{0:n2}"
                                    filter="numeric"
                                    cells={{ filterCell: NumericFilterCell }}
                                    width={130}
                                />
                                <Column
                                    field="apartmentSpaceM2"
                                    title={getTranslatedLabel("salesRequest.list.apartmentSpace", "Apartment Space (m²)")}
                                    format="{0:n2}"
                                    filter="numeric"
                                    cells={{ filterCell: NumericFilterCell }}
                                    width={150}
                                />
                                <Column
                                    field="gardenSpaceM2"
                                    title={getTranslatedLabel("salesRequest.list.gardenSpace", "Garden Space (m²)")}
                                    format="{0:n2}"
                                    filter="numeric"
                                    cells={{ filterCell: NumericFilterCell }}
                                    width={150}
                                />
                                <Column
                                    field="apartmentPricePerM2"
                                    title={getTranslatedLabel("salesRequest.list.pricePerM2", "Price / m²")}
                                    format="{0:n2}"
                                    filter="numeric"
                                    cells={{ filterCell: NumericFilterCell }}
                                    width={130}
                                />
                                <Column
                                    field="projectId"
                                    title={getTranslatedLabel("salesRequest.list.project", "Project")}
                                    filter="text"
                                    cells={{ data: ProjectNameCell, filterCell: ProjectFilterCell }}
                                    width={150}
                                />
                                <Column
                                    field="comments"
                                    title={getTranslatedLabel("salesRequest.list.comments", "Comments")}
                                    filter="text"
                                    cells={{ filterCell: TextFilterCell }}
                                />
                                <Column
                                    title="عمولة"
                                    cells={{ data: CommissionCell }}
                                    width={100}
                                    filterable={false}
                                    sortable={false}
                                />
                            </KendoGrid>

                            {isFetching && <LoadingComponent message={getTranslatedLabel("general.loading", "Loading Sales Requests...")} />}
                        </div>
                    </Grid>
                </Grid>

                {showCalculator && (
                    <ModalContainer show={showCalculator} onClose={() => setShowCalculator(false)} width={850}>
                        <InstallmentPriceCalculatorModal
                            onClose={() => setShowCalculator(false)}
                        />
                    </ModalContainer>
                )}
                
            </Paper>
        </>
    );
}

export default SalesRequestsList;