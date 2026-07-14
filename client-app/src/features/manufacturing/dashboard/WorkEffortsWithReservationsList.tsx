import React, { useEffect, useState } from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper, Button, Dialog } from "@mui/material";
import { DataResult, State } from "@progress/kendo-data-query";
import { handleDatesArray } from "../../../app/util/utils";
import { useAppDispatch } from "../../../app/store/configureStore";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import FacilityMenu from "../../facilities/menu/FacilityMenu";
import IssueReservationsModal from "./IssueReservationsModal";
import { useFetchProductionRunReservationsQuery } from "../../../app/store/apis";

function WorkEffortsWithReservationsList() {
    const dispatch = useAppDispatch();
    const { getTranslatedLabel } = useTranslationHelper();
    const initialDataState = { take: 10, skip: 0 };
    const [dataState, setDataState] = useState<State>(initialDataState);
    const [workEfforts, setWorkEfforts] = useState<DataResult>({ data: [], total: 0 });
    const [selectedWorkEffortId, setSelectedWorkEffortId] = useState<string | null>(null);

    const { data, isFetching } = useFetchProductionRunReservationsQuery(dataState);

    // REFACTOR: Update workEfforts state with fetched data
    // Purpose: Handle date formatting and set grid data
    // Benefit: Ensures consistent data display
    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setWorkEfforts({ data: adjustedData, total: data.totalCount });
        }
    }, [data]);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    // REFACTOR: Handle WorkEffort cell click to open modal
    // Purpose: Open IssueReservationsModal with selected WorkEffortId
    // Benefit: Allows users to view and issue reservations
    const WorkEffortIdCell = (props: any) => {
        const value = props.dataItem[props.field || ''];
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{ color: 'blue' }}
                role="gridcell"
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button
                    variant="outlined"
                    color="primary"
                    onClick={() => setSelectedWorkEffortId(props.dataItem.reservationWorkEffortId)}
                >
                    {value}
                </Button>
            </td>
        );
    };

    return (
        <>
            <FacilityMenu selectedMenuItem={'reservations'} />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                style={{ height: '65vh', flex: 1 }}
                                data={workEfforts}
                                resizable
                                filterable
                                sortable
                                pageable
                                {...dataState}
                                onDataStateChange={dataStateChange}
                            >
                                <Column field="reservationWorkEffortId" title="Reservation ID" cell={WorkEffortIdCell} width={120} />
                                <Column field="productionRunWorkEffortId" title="Production Run ID" width={150} />
                                <Column field="productionRunName" title="Production Run Name" width={250} />
                                <Column field="facilityName" title="Facility" width={150} />
                                <Column field="currentStatusDescription" title="Status" width={150} />
                                <Column field="estimatedStartDate" title="Start Date" format="{0:dd/MM/yyyy}" width={130} />
                                <Column field="estimatedCompletionDate" title="Completion Date" format="{0:dd/MM/yyyy}" width={130} />
                                <Column field="description" title="Description" width={200} />
                            </KendoGrid>
                            {isFetching && (
                                <LoadingComponent
                                    message={getTranslatedLabel("manufacturing.reservations.loading", "Loading Reservations...")}
                                />
                            )}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
            {/* REFACTOR: Add Dialog to display IssueReservationsModal */}
            {/* Purpose: Show modal when WorkEffortId is clicked */}
            {/* Benefit: Provides a clean UI for viewing and issuing reservations */}
            <Dialog open={!!selectedWorkEffortId} onClose={() => setSelectedWorkEffortId(null)} maxWidth="md">
                {selectedWorkEffortId && (
                    <IssueReservationsModal
                        workEffortId={selectedWorkEffortId}
                        onClose={() => setSelectedWorkEffortId(null)}
                        language="en" // Adjust based on your app's language handling
                    />
                )}
            </Dialog>
        </>
    );
}

export default WorkEffortsWithReservationsList;