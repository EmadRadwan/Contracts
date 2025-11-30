import React, { useState } from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper } from "@mui/material";

import Button from "@mui/material/Button";
import {DataResult, State} from "@progress/kendo-data-query";
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";
import {useFetchSalesRequestsQuery} from "../../../../../app/store/apis/salesRequestApi";
import {SalesRequest} from "../../../../../app/models/order/SalesRequest";
import SalesRequestForm from "../form/SalesRequestForm";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import SalesRequestMenu from "../menu/SalesRequestMenu";
import {handleDatesArray} from "../../../../../app/util/utils";
import InstallmentPriceCalculatorModal from "./InstallmentPriceCalculatorModal";
import ModalContainer from "../../../../../app/common/modals/ModalContainer";
import PaymentPlanModal from "./PaymentPlanModal";

function SalesRequestsList() {
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

    // -----------------------------------------------------------------
    // Grid data
    // -----------------------------------------------------------------
    const [dataState, setDataState] = useState<State>({ take: 9, skip: 0 });
    const { data, isFetching } = useFetchSalesRequestsQuery(dataState);

    React.useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setSRequests({ data: adjustedData, total: data.total });
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
            // Create mode
            setSelectedSR(undefined);
            setEditMode(1);
            setViewMode("form");
            return;
        }

        // Edit or View mode — decide based on status
        const isApproved = sr.statusId === "SALES_REQUEST_APPROVED";

        setSelectedSR(sr);
        setEditMode(isApproved ? 3 : 2);  // ← THIS IS THE KEY LINE
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
                    setEditMode(3);           // Switch to read-only "Approved" mode
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
                                        <Grid item xs={2}>
                                            <Button
                                                color="secondary"
                                                onClick={() => startEdit()}
                                                variant="outlined"
                                            >
                                                {getTranslatedLabel("salesRequest.list.create", "Create Sales Request")}
                                            </Button>
                                        </Grid>
                                    </Grid>

                                    <Grid item>
                                        <Button
                                            color="primary"
                                            variant="contained"
                                            onClick={() => setShowCalculator(true)}
                                        >
                                            {getTranslatedLabel("installmentCalculator.open", "حاسبة سعر المتر بالتقسيط")}
                                        </Button>
                                    </Grid>
                                </GridToolbar>

                                <Column
                                    field="salesRequestId"
                                    title={getTranslatedLabel("salesRequest.list.id", "Request ID")}
                                    cell={RequestIdCell}
                                    width={130}
                                />
                                <Column
                                    field="apartmentName"
                                    title={getTranslatedLabel("salesRequest.list.apartment", "Apartment")}
                                    width={250}
                                />
                                <Column
                                    field="fromPartyName"
                                    title={getTranslatedLabel("salesRequest.list.customer", "Customer")}
                                    width={200}
                                />
                                <Column
                                    field="statusDescription"
                                    title={getTranslatedLabel("salesRequest.list.status", "Status")}
                                    width={200}
                                />
                                <Column
                                    field="saleDate"
                                    title={getTranslatedLabel("salesRequest.list.saleDate", "Sale Date")}
                                    format="{0:dd/MM/yyyy}"
                                    filter="date"
                                    width={140}
                                />
                                <Column
                                    field="totalPrice"
                                    title={getTranslatedLabel("salesRequest.list.total", "Total")}
                                    format="{0:n2}"
                                    filter="numeric"
                                    width={130}
                                />
                                <Column
                                    field="advancePayment"
                                    title={getTranslatedLabel("salesRequest.list.advance", "Advance")}
                                    format="{0:n2}"
                                    filter="numeric"
                                    width={130}
                                />
                                <Column
                                    field="comments"
                                    title={getTranslatedLabel("salesRequest.list.comments", "Comments")}
                                />
                            </KendoGrid>

                            {isFetching && <LoadingComponent message={getTranslatedLabel("general.loading", "Loading Sales Requests...")} />}
                        </div>
                    </Grid>
                </Grid>
                {showCalculator && (
                    <InstallmentPriceCalculatorModal onClose={() => setShowCalculator(false)} />
                )}

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