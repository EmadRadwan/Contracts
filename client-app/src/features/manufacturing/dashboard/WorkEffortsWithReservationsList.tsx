import React, { useEffect, useState } from "react";
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
import { handleDatesArray } from "../../../app/util/utils";
import { useAppDispatch } from "../../../app/store/configureStore";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import {useFetchProductionRunReservationsQuery} from "../../../app/store/apis";
import FacilityMenu from "../../facilities/menu/FacilityMenu";

function WorkEffortsWithReservationsList() {
    const dispatch = useAppDispatch();
    const { getTranslatedLabel } = useTranslationHelper();

    const initialDataState = { take: 10, skip: 0 };
    const [dataState, setDataState] = useState<State>(initialDataState);
    const [workEfforts, setWorkEfforts] = useState<DataResult>({ data: [], total: 0 });

    const { data, isFetching } = useFetchProductionRunReservationsQuery(dataState);

    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setWorkEfforts({ data: adjustedData, total: data.totalCount });
        }
    }, [data]);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

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
                    onClick={() => console.log("Selected WorkEffort:", props.dataItem)}
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
                                    message={getTranslatedLabel(
                                        "manufacturing.reservations.loading",
                                        "Loading Reservations..."
                                    )}
                                />
                            )}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}

export default WorkEffortsWithReservationsList;
