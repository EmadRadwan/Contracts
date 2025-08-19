import React, { useEffect, useState } from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import OrderMenu from "../../menu/OrderMenu";
import { Grid, Paper } from "@mui/material";
import { ExcelExport } from "@progress/kendo-react-excel-export";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { handleDatesArray } from "../../../../app/util/utils";
import { useAppDispatch, useFetchReturnsQuery } from "../../../../app/store/configureStore";
import { DataResult, State } from "@progress/kendo-data-query";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import EditReturn from "../../form/return/EditReturn";
import { Return } from "../../../../app/models/order/return";

export default function ReturnsList() {
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();

    // REFACTOR: State aligned with OrdersList, using selectedReturnHeader to avoid conflict with returnUiSlice
    const [returns, setReturns] = useState<DataResult>({ data: [], total: 0 });
    const [dataState, setDataState] = useState<State>({ take: 10, skip: 0 });
    const [editMode, setEditMode] = useState(0);
    const [selectedReturnHeader, setSelectedReturnHeader] = useState<Return | undefined>(undefined);

    const { data, isFetching, error } = useFetchReturnsQuery(dataState, { skip: !dataState });

    // REFACTOR: Update returns state with fetched data, unchanged from previous version
    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setReturns({ data: adjustedData, total: data.total });
        }
        if (error) {
            toast.error("Failed to load returns. Please try again.");
        }
    }, [data, error]);

    // REFACTOR: No setSelectedReturn dispatch, per user request

    // REFACTOR: Unchanged dataStateChange, aligns with OrdersList
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    // REFACTOR: Updated companyId mapping to use a single string value instead of an object.
    // For CUSTOMER_RETURN, companyId = toPartyId.fromPartyId or toPartyId; for VENDOR_RETURN, companyId = fromPartyId.fromPartyId or fromPartyId.
    // Renamed selected to selectedReturnHeader for consistency.
    const handleSelectReturn = (returnId: string, statusId: string) => {
        const statusToEditMode: { [key: string]: number } = {
            RETURN_REQUESTED: 5,
            RETURN_ACCEPTED: 3,
            RETURN_RECEIVED: 6,
            RETURN_COMPLETED: 4,
            RETURN_MAN_REFUND: 7,
            RETURN_CANCELLED: 8,
        };

        const selected = returns.data.find((r: Return) => r.returnId === returnId);
        if (!selected) {
            toast.error("Return not found");
            return;
        }

        // Map companyId as a string based on return type, matching useReturn logic
        const transformedReturn: Return = {
            ...selected,
            companyId:
                selected.returnHeaderTypeId === "CUSTOMER_RETURN"
                    ? (typeof selected.toPartyId === "object" && selected.toPartyId?.fromPartyId) || selected.toPartyId
                    : (typeof selected.fromPartyId === "object" && selected.fromPartyId?.fromPartyId) || selected.fromPartyId,
        };

        setSelectedReturnHeader(transformedReturn);
        setEditMode(statusToEditMode[statusId] || 2);
    };

    // REFACTOR: Updated to use selectedReturnHeader, no setSelectedReturn dispatch
    const cancelEdit = () => {
        setEditMode(0);
        setSelectedReturnHeader(undefined);
    };

    // REFACTOR: Updated to use selectedReturnHeader, no setSelectedReturn dispatch
    const handleNewReturn = () => {
        setEditMode(1);
        setSelectedReturnHeader(undefined);
    };

    const ReturnDescriptionCell = (props: any) => {
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
                <Button
                    variant="text"
                    color="primary"
                    onClick={() => handleSelectReturn(props.dataItem.returnId, props.dataItem.statusId)}
                    aria-label={`View return ${props.dataItem.returnId}`}
                >
                    {props.dataItem.returnId}
                </Button>
            </td>
        );
    };

    // REFACTOR: Unchanged dataToExport, uses returns.data
    const dataToExport = returns.data.length ? handleDatesArray(returns.data) : [];

    const handleCreateReturn = () => {
        handleNewReturn();
    };

    // REFACTOR: Updated to pass selectedReturnHeader with string companyId to EditReturn
    if (editMode > 0) {
        return (
            <EditReturn
                selectedReturn={selectedReturnHeader}
                cancelEdit={cancelEdit}
                editMode={editMode}
                handleNewReturn={handleNewReturn}
            />
        );
    }

    return (
        <>
            <OrderMenu />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <Grid container>
                            <div className="div-container">
                                <ExcelExport data={dataToExport}>
                                    <KendoGrid
                                        style={{ height: "518px", width: "93vw" }}
                                        resizable={true}
                                        filterable={true}
                                        sortable={true}
                                        pageable={true}
                                        {...dataState}
                                        data={returns}
                                        onDataStateChange={dataStateChange}
                                    >
                                        <GridToolbar>
                                            <Grid container>
                                                <Grid item xs={2}>
                                                    <Button
                                                        color="secondary"
                                                        onClick={handleCreateReturn}
                                                        variant="outlined"
                                                        sx={{ marginLeft: "auto" }}
                                                    >
                                                        Create Return
                                                    </Button>
                                                </Grid>
                                            </Grid>
                                        </GridToolbar>
                                        <Column
                                            field="returnId"
                                            title="Return Number"
                                            cell={ReturnDescriptionCell}
                                            width={150}
                                            locked={true}
                                        />
                                        <Column field="returnHeaderTypeDescription" title="Return Type" width={200} />
                                        <Column field="fromPartyName" title="From Party" width={200} />
                                        <Column field="toPartyName" title="To Party" width={200} />
                                        <Column field="entryDate" title="Return Date" width={150} format="{0: dd/MM/yyyy}" />
                                        <Column field="statusDescription" title="Status" width={100} />
                                    </KendoGrid>
                                </ExcelExport>
                                {isFetching && <LoadingComponent message="Loading Returns..." />}
                            </div>
                        </Grid>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}