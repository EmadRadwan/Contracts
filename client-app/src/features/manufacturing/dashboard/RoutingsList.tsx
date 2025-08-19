import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {Grid, Paper} from "@mui/material";
import Button from "@mui/material/Button";
import {useLocation} from "react-router-dom";
import {DataResult, State} from "@progress/kendo-data-query";
import {useFetchRoutingsQuery} from "../../../app/store/configureStore";
import {handleDatesArray} from "../../../app/util/utils";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import EditRouting from "../form/EditRouting";
import {WorkEffort} from "../../../app/models/manufacturing/workEffort";

function RoutingsList() {
    const [editMode, setEditMode] = useState(0);
    const location = useLocation();

    const initialDataState = {take: 6, skip: 0};
    const [dataState, setDataState] = React.useState<State>(initialDataState);
    const {getTranslatedLabel} = useTranslationHelper();

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        console.log("dataStateChange", e.dataState);
        setDataState(e.dataState);
    };

    const {data, error, isFetching} = useFetchRoutingsQuery({...dataState});
    const [routings, setRoutings] = useState<DataResult>({data: [], total: 0});
    const [selectedRouting, setSelectedRouting] = useState<WorkEffort | undefined>();
    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setRoutings({data: adjustedData, total: data.totalCount});
        }
    }, [data]);

    const handleSelectRouting = (selectedWorkEffort: WorkEffort) => {
        const updatedRouting = {
            ...selectedWorkEffort,
            workEffortId: selectedWorkEffort.workEffortId,
            workEffortTypeId: selectedWorkEffort.workEffortTypeId || 'ROUTING',
            workEffortName: selectedWorkEffort.workEffortName || '',
            description: selectedWorkEffort.description || null,
            quantityToProduce: selectedWorkEffort.quantityToProduce || null,
            currentStatusId: selectedWorkEffort.currentStatusId || null,
        };
        setSelectedRouting(updatedRouting);
        setEditMode(2);
    };


    const handleCreateRouting = () => {
        setSelectedRouting(undefined);
        setEditMode(1);
    };

    const cancelEdit = () => {
        setEditMode(0);
        setSelectedRouting(undefined);
    };

    const RoutingIdCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: "blue"}}
                colSpan={props.colSpan}
                role={"gridcell"}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{
                    [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
                }}
                {...navigationAttributes}
            >
                <Button onClick={() => handleSelectRouting(props.dataItem)}>
                    {props.dataItem.workEffortId}
                </Button>
            </td>
        );
    };

    if (editMode > 0) {
        return (
            <EditRouting
                selectedRouting={selectedRouting}
                editMode={editMode}
                cancelEdit={cancelEdit}
            />
        );
    }

    return (
        <>
            <ManufacturingMenu selectedMenuItem={"routings"}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                style={{height: "65vh", flex: 1}}
                                data={routings ? routings : {data: [], total: 0}}
                                resizable={true}
                                filterable={true}
                                sortable={true}
                                pageable={true}
                                {...dataState}
                                onDataStateChange={dataStateChange}
                            >
                                <GridToolbar>
                                    <Grid container>
                                        <Grid item xs={4}>
                                            <Button
                                                color={"secondary"}
                                                onClick={handleCreateRouting}
                                                variant="outlined"
                                            >
                                                {getTranslatedLabel(
                                                    "manufacturing.routings.list.createRouting",
                                                    "Create Routing"
                                                )}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </GridToolbar>

                                <Column
                                    field="workEffortId"
                                    title={getTranslatedLabel(
                                        "manufacturing.routings.list.routingId",
                                        "Routing Id"
                                    )}
                                    cell={RoutingIdCell}
                                    width={200}
                                    locked={true}
                                />
                                <Column
                                    field="workEffortName"
                                    title={getTranslatedLabel(
                                        "manufacturing.routings.list.routingName",
                                        "Routing Name"
                                    )}
                                    width={400}
                                />
                                <Column
                                    field="description"
                                    title={getTranslatedLabel(
                                        "manufacturing.routings.list.description",
                                        "Description"
                                    )}
                                    width={350}
                                />
                                <Column
                                    field="quantityToProduce"
                                    title={getTranslatedLabel(
                                        "manufacturing.routings.list.quantityToProduce",
                                        "Quantity To Produce"
                                    )}
                                    //   width={250}
                                />
                                <Column
                                    field="currentStatusId"
                                    title={getTranslatedLabel(
                                        "manufacturing.routings.list.currentStatusId",
                                        "Current Status Id"
                                    )}
                                    //   width={250}
                                />
                            </KendoGrid>
                            {isFetching && (
                                <LoadingComponent
                                    message={getTranslatedLabel(
                                        "manufacturing.routings.list.loading",
                                        "Loading Routings..."
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

export default RoutingsList;
