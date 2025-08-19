import React, {useEffect, useState} from 'react';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GRID_COL_INDEX_ATTRIBUTE, GridToolbar
} from '@progress/kendo-react-grid';
import {useTableKeyboardNavigation} from '@progress/kendo-react-data-tools';
import {Grid, Paper, Button, Typography} from '@mui/material';
import {useParams} from 'react-router-dom';
import {toast} from 'react-toastify';
import {DataResult, State} from '@progress/kendo-data-query';
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {handleDatesArray} from "../../../app/util/utils";
import {useDeleteWorkEffortAssocMutation, useGetRoutingTaskAssocsQuery} from "../../../app/store/apis";
import EditRoutingTaskAssoc from "../form/EditRoutingTaskAssoc";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import RoutingMenu from "../menu/RoutingMenu";

interface RoutingTaskAssoc {
    workEffortIdFrom: string;
    workEffortIdTo: any;
    workEffortToName: string;
    workEffortAssocTypeId: string;
    sequenceNum: number | null;
    fromDate: string;
    thruDate: string | null;
    workEffortToSetup: number | null;
    workEffortToRun: number | null;
}

export default function ListRoutingTaskAssoc() {
    const {workEffortId} = useParams<{ workEffortId: string }>();
    const {getTranslatedLabel} = useTranslationHelper();

    const localizationKey = 'manufacturing.routingTaskAssoc.grid';
    const [editMode, setEditMode] = useState(0);
    const [selectedTaskAssoc, setSelectedTaskAssoc] = useState<RoutingTaskAssoc | undefined>(undefined);
    const [dataState, setDataState] = useState<State>({
        take: 6,
        skip: 0,
        sort: [
            {field: 'sequenceNum', dir: 'asc'},
        ],
    });
    const [routingTaskAssocs, setRoutingTaskAssocs] = useState<DataResult>({data: [], total: 0});
    const {data, isFetching, error} = useGetRoutingTaskAssocsQuery(workEffortId!);

    const [removeRoutingTaskAssoc, { isLoading: isRemoving }] = useDeleteWorkEffortAssocMutation();

    const handleSelectTaskAssoc = (taskAssoc?: RoutingTaskAssoc) => {
        if (taskAssoc) {
            setSelectedTaskAssoc(taskAssoc);
            setEditMode(2); // Edit mode
        } else {
            setSelectedTaskAssoc(null);
            setEditMode(1); // New mode
        }
    };

    const cancelEdit = () => {
        setEditMode(0);
        setSelectedTaskAssoc(null);
    };
    
    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data);
            setRoutingTaskAssocs(adjustedData);
        }
    }, [data]);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };




    const handleDeleteTaskAssoc = async (taskAssoc: RoutingTaskAssoc) => {
        try {
            // Business: Sends only the required composite key fields to identify the association
            // Technical: Removed unused workEffortId and fromDate, using workEffortAssocTypeId from data
            await removeRoutingTaskAssoc({
                workEffortIdFrom: taskAssoc.workEffortIdFrom,
                workEffortIdTo: taskAssoc.workEffortIdTo.workEffortIdTo,
                workEffortAssocTypeId: taskAssoc.workEffortAssocTypeId,
            }).unwrap();
            toast.success(getTranslatedLabel(`${localizationKey}.deleteSuccess`, 'Routing Task Association Deleted Successfully!'));
        } catch (e: any) {
            // Technical: Checks for e.data?.error to align with Result<Unit> error format from backend
            const errorMsg = e.data?.error || e.data?.errors?.join(', ') || 'Delete operation failed.';
            toast.error(errorMsg);
        }
    };
    
    const TaskNameCell = (props: any) => {
        const {workEffortIdTo, workEffortToName} = props.dataItem;
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: 'blue'}}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{[GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex}}
                {...navigationAttributes}
            >
                <Button onClick={() => handleSelectTaskAssoc(props.dataItem)}>
                    [{workEffortIdTo.workEffortIdTo}] {workEffortToName}
                </Button>
            </td>
        );
    };

    const DeleteCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button
                    color="error"
                    onClick={() => handleDeleteTaskAssoc(props.dataItem)}
                    disabled={isRemoving}
                >
                    {getTranslatedLabel('common.delete', 'Delete')}
                </Button>
            </td>
        );
    };
    
    return (
        <>
            {editMode ? (
                <EditRoutingTaskAssoc taskAssoc={selectedTaskAssoc} editMode={editMode} cancelEdit={cancelEdit} />
            ) : (
                <>
                    <ManufacturingMenu />
                    <Paper elevation={5} className="div-container-withBorderCurved">
                        <Grid container columnSpacing={1}>
                            <Grid container alignItems="center">
                                <Grid item xs={8}>
                                    <Typography sx={{ p: 2 }} variant="h4">
                                        {getTranslatedLabel(`${localizationKey}.title`, 'Routing Task Associations')}
                                    </Typography>
                                </Grid>
                            </Grid>
                            <RoutingMenu workEffortId={workEffortId!} selectedMenuItem="routingTaskAssoc" />
                            <Grid container p={2}>
                                <div className="div-container">
                                    <KendoGrid
                                        className="main-grid"
                                        data={routingTaskAssocs}
                                        resizable
                                        sortable
                                        pageable
                                        {...dataState}
                                        onDataStateChange={dataStateChange}
                                    >
                                        <GridToolbar>
                                            <Grid item xs={3}>
                                                <Button
                                                    color="secondary"
                                                    onClick={() => handleSelectTaskAssoc()}
                                                    variant="outlined"
                                                    disabled={isFetching}
                                                >
                                                    {getTranslatedLabel(`${localizationKey}.create`, 'Create Task Association')}
                                                </Button>
                                            </Grid>
                                        </GridToolbar>
                                        <Column
                                            field="sequenceNum"
                                            title={getTranslatedLabel('common.sequenceNum', 'Sequence Number')}
                                            width={220}
                                        />
                                        <Column
                                            field="workEffortIdTo.workEffortIdTo"
                                            title={getTranslatedLabel('manufacturing.taskName', 'Task Name')}
                                            cell={TaskNameCell}
                                            width={250}
                                        />
                                        <Column
                                            field="workEffortAssocTypeDescription"
                                            title={getTranslatedLabel('manufacturing.assocType', 'Association Type')}
                                            width={300}
                                        />
                                        <Column
                                            field="fromDate"
                                            title={getTranslatedLabel('common.fromDate', 'From Date')}
                                            format="{0:yyyy-MM-dd}"
                                            width={180}
                                        />
                                        <Column
                                            field="thruDate"
                                            title={getTranslatedLabel('common.thruDate', 'Thru Date')}
                                            format="{0:yyyy-MM-dd}"
                                            width={180}
                                        />
                                       
                                        <Column title=" " cell={DeleteCell} width={100} />
                                    </KendoGrid>
                                    {isFetching && (
                                        <LoadingComponent
                                            message={getTranslatedLabel(`${localizationKey}.loading`, 'Loading Routing Task Associations...')}
                                        />
                                    )}
                                </div>
                            </Grid>
                        </Grid>
                    </Paper>
                </>
            )}
        </>
    );
}