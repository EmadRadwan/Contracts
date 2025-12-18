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
import { DataResult, State } from "@progress/kendo-data-query";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import { ReserveRequest } from "../../../../../app/models/order/ReserveRequest";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import { handleDatesArray } from "../../../../../app/util/utils";
import { useFetchReserveRequestsQuery } from "../../../../../app/store/apis/salesRequestApi";
import SalesRequestMenu from "../menu/SalesRequestMenu";
import ReserveRequestForm from "../form/ReserveRequestForm";

function ReserveRequestsList() {
    const [editMode, setEditMode] = useState(0); // 0=list, 1=create, 2=edit
    const [selectedRR, setSelectedRR] = useState<ReserveRequest | undefined>(undefined);
    const [rRequests, setRRequests] = React.useState<DataResult>({
        data: [],
        total: 0,
    });
    const [viewMode, setViewMode] = useState<"list" | "form">("list");

    const { getTranslatedLabel } = useTranslationHelper();

    const [dataState, setDataState] = useState<State>({ take: 9, skip: 0 });
    const { data, isFetching } = useFetchReserveRequestsQuery(dataState);

    React.useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setRRequests({ data: adjustedData, total: data.total });
        }
    }, [data]);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    // -----------------------------------------------------------------
    // Unified handlers – SAME PATTERN as SalesRequestsList
    // -----------------------------------------------------------------
    const startEdit = (rr?: ReserveRequest) => {
        if (!rr) {
            setSelectedRR(undefined);
            setEditMode(1);
            setViewMode("form");
            return;
        }

        setSelectedRR(rr);
        setEditMode(2);
        setViewMode("form");
    };

    const cancelEdit = () => {
        setSelectedRR(undefined);
        setEditMode(0);
        setViewMode("list");
    };

    const handleReserveRequestCreated = (createdRequest: ReserveRequest) => {
        // Keep form open, switch to edit mode with fresh server data
        setEditMode(2);
        setSelectedRR(createdRequest);
        setViewMode("form");
    };

    // -----------------------------------------------------------------
    // Form rendering
    // -----------------------------------------------------------------
    if (viewMode === "form" && editMode !== 0) {
        return (
            <ReserveRequestForm
                reserveRequest={editMode === 1 ? undefined : selectedRR}
                editMode={editMode}
                cancelEdit={cancelEdit}
                onReserveRequestCreated={handleReserveRequestCreated}
                onReserveRequestUpdated={(updated) => {
                    setSelectedRR(updated);
                    // Future: switch to read-only mode if you add approval status
                }}
            />
        );
    }

    const handleMenuSelect = (key: string) => {
        if (key === "salesRequest.menu.reserveRequests") {
            setViewMode("list");
            setEditMode(0);
            setSelectedRR(undefined);
        }
    };

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
                    {props.dataItem.reserveRequestId}
                </Button>
            </td>
        );
    };

    return (
        <>
            <SalesRequestMenu
                selectedMenuItem="/reserve-requests"
                on_kmMenuSelect={handleMenuSelect}
            />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                style={{ height: "65vh", flex: 1 }}
                                data={rRequests || { data: [], total: 0 }}
                                resizable
                                filterable
                                sortable
                                pageable
                                {...dataState}
                                onDataStateChange={dataStateChange}
                            >
                                <GridToolbar>
                                    <Grid container>
                                        <Grid item xs={3}>
                                            <Button
                                                color="secondary"
                                                onClick={() => startEdit()}
                                                variant="outlined"
                                            >
                                                {getTranslatedLabel("reserveRequest.list.create", "Create Reserve Request")}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </GridToolbar>

                                <Column field="reserveRequestId" title={getTranslatedLabel("reserveRequest.list.id", "Request ID")} cell={RequestIdCell} width={130} />
                                <Column field="apartmentName" title={getTranslatedLabel("reserveRequest.list.apartment", "Apartment")} width={250} />
                                <Column field="fromPartyName" title={getTranslatedLabel("reserveRequest.list.customer", "Customer")} width={200} />
                                <Column field="employeeName" title={getTranslatedLabel("reserveRequest.list.employee", "Employee")} width={200} />
                                <Column field="reserveDate" title={getTranslatedLabel("reserveRequest.list.reserveDate", "Reserve Date")} format="{0:dd/MM/yyyy}" filter="date" width={140} />
                                <Column field="reserveAmount" title={getTranslatedLabel("reserveRequest.list.amount", "Reserve Amount")} format="{0:n2}" filter="numeric" width={140} />
                                <Column field="payMethod" title={getTranslatedLabel("reserveRequest.list.payMethod", "Payment Method")} width={150} />
                                <Column field="comments" title={getTranslatedLabel("reserveRequest.list.comments", "Comments")} />
                            </KendoGrid>

                            {isFetching && <LoadingComponent message={getTranslatedLabel("general.loading", "Loading Reserve Requests...")} />}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}

export default ReserveRequestsList;