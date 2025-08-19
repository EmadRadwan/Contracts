import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { Grid as KendoGrid, GridColumn as Column, GridPageChangeEvent, GridSortChangeEvent, GRID_COL_INDEX_ATTRIBUTE, GridToolbar } from '@progress/kendo-react-grid';
import { useTableKeyboardNavigation } from '@progress/kendo-react-data-tools';
import { Grid, Paper, Button, Typography } from '@mui/material';
import { useParams, useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { DataResult, SortDescriptor, State } from '@progress/kendo-data-query';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import { handleDatesArray } from '../../../app/util/utils';
import EditWorkEffortCostCalc from '../form/EditWorkEffortCostCalc';
import ManufacturingMenu from '../menu/ManufacturingMenu';
import { useGetWorkEffortCostCalcsQuery, useGetWorkEffortQuery, useDeleteWorkEffortCostCalcMutation } from '../../../app/store/apis';

// REFACTOR: Define interface for WorkEffortCostCalc, aligning with OFBiz grid fields
interface WorkEffortCostCalc {
    workEffortId: string;
    costComponentCalcId: string;
    costComponentTypeId: string;
    costComponentTypeDescription: string;
    costComponentCalcDescription: string;
    fromDate: string;
    thruDate: string | null;
}

export default function ListRoutingTaskCosts() {
    const { workEffortId } = useParams<{ workEffortId: string }>();
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'manufacturing.routingTaskCosts.grid';
    const navigate = useNavigate();

    // REFACTOR: Fix initialSort to use a valid field
    // Purpose: Prevents sorting errors by aligning with WorkEffortCostCalc interface
    const initialSort: SortDescriptor[] = [{ field: 'costComponentCalcId', dir: 'asc' }];
    const initialDataState: State = { skip: 0, take: 4 };
    const [sort, setSort] = useState<SortDescriptor[]>(initialSort);
    const [page, setPage] = useState<State>(initialDataState);
    const [editMode, setEditMode] = useState(0);
    const [selectedCostCalc, setSelectedCostCalc] = useState<WorkEffortCostCalc | undefined>(undefined);
    const [costCalcs, setCostCalcs] = useState<DataResult>({ data: [], total: 0 });

    // REFACTOR: Fetch WorkEffortCostCalc and WorkEffort data using RTK Query
    const { data: costCalcsData, isFetching: isCostCalcsFetching, error: costCalcsError } = useGetWorkEffortCostCalcsQuery(workEffortId!);
    const { data: workEffortData, isFetching: isWorkEffortFetching, error: workEffortError } = useGetWorkEffortQuery(workEffortId!);
    const [deleteWorkEffortCostCalc, { isLoading: isRemoving }] = useDeleteWorkEffortCostCalcMutation();

    // REFACTOR: Memoize getTranslatedLabel to stabilize reference
    // Purpose: Prevents unnecessary re-renders due to new function instances
    const stableGetTranslatedLabel = useMemo(() => getTranslatedLabel, [getTranslatedLabel]);

    // REFACTOR: Compute all static labels in a memoized object
    // Purpose: Stabilizes translations, prevents re-computation on every render, and avoids dependency issues in effects and memos
    const labels = useMemo(() => ({
        title: stableGetTranslatedLabel(`${localizationKey}.title`, `Routing Task Costs for ${workEffortId} - ${workEffortData?.workEffortName || 'Loading...'}`),
        create: stableGetTranslatedLabel(`${localizationKey}.create`, 'Create Task Cost'),
        costComponentCalc: stableGetTranslatedLabel('manufacturing.costComponentCalc', 'Cost Component Calc'),
        costComponentType: stableGetTranslatedLabel('manufacturing.costComponentType', 'Cost Component Type'),
        fromDate: stableGetTranslatedLabel('common.fromDate', 'From Date'),
        thruDate: stableGetTranslatedLabel('common.thruDate', 'Thru Date'),
        delete: stableGetTranslatedLabel('common.delete', 'Delete'),
        back: stableGetTranslatedLabel('common.back', 'Back'),
        loading: stableGetTranslatedLabel(`${localizationKey}.loading`, 'Loading Routing Task Costs...'),
        error: stableGetTranslatedLabel(`${localizationKey}.error`, ''), // Placeholder for dynamic error msg
        deleteSuccess: stableGetTranslatedLabel(`${localizationKey}.deleteSuccess`, 'Routing Task Cost Deleted Successfully!'),
    }), [stableGetTranslatedLabel, workEffortId, workEffortData?.workEffortName, localizationKey]);

    // REFACTOR: Memoize handleSelectCostCalc to ensure stable function reference
    const handleSelectCostCalc = useCallback((costCalc?: WorkEffortCostCalc) => {
        if (costCalc) {
            setSelectedCostCalc(costCalc);
            setEditMode(2); // Edit mode
        } else {
            setSelectedCostCalc(undefined);
            setEditMode(1); // Create mode
        }
    }, []);

    // REFACTOR: Reset edit state on cancel
    const cancelEdit = useCallback(() => {
        setEditMode(0);
        setSelectedCostCalc(undefined);
    }, []);

    // REFACTOR: Split useEffect for data handling
    // Purpose: Separates data update from error handling to avoid dependency on translations, preventing infinite loops
    useEffect(() => {
        if (costCalcsData) {
            const adjustedData = handleDatesArray(costCalcsData);
            setCostCalcs(prev => {
                // REFACTOR: Add deep comparison to prevent unnecessary state updates
                // Purpose: Reduces re-renders by only updating if data has changed, avoiding potential loops
                if (JSON.stringify(prev.data) !== JSON.stringify(adjustedData)) {
                    return { data: adjustedData, total: adjustedData.length };
                }
                return prev;
            });
        }
    }, [costCalcsData]);

    // REFACTOR: Separate useEffect for costCalcsError
    // Purpose: Isolates error handling to prevent translation dependencies from affecting data updates
    useEffect(() => {
        if (costCalcsError) {
            const errorMsg = (costCalcsError as any).data?.error || 'Failed to load routing task costs';
            toast.error(labels.error + errorMsg); // Use pre-computed label
        }
    }, [costCalcsError, labels.error]);

    // REFACTOR: Separate useEffect for workEffortError
    // Purpose: Isolates error handling for workEffort
    useEffect(() => {
        if (workEffortError) {
            const errorMsg = (workEffortError as any).data?.error || 'Failed to load routing task details';
            toast.error(labels.error + errorMsg); // Use pre-computed label
        }
    }, [workEffortError, labels.error]);

    // REFACTOR: Handle deletion with stable function reference
    const handleDeleteCostCalc = useCallback(async (costCalc: WorkEffortCostCalc) => {
        try {
            await deleteWorkEffortCostCalc({
                workEffortId: costCalc.workEffortId,
                costComponentCalcId: costCalc.costComponentCalcId,
                costComponentTypeId: costCalc.costComponentTypeId,
                fromDate: costCalc.fromDate,
            }).unwrap();
            toast.success(labels.deleteSuccess);
        } catch (e: any) {
            const errorMsg = e.data?.error || e.data?.errors?.join(', ') || 'Delete operation failed.';
            toast.error(errorMsg);
        }
    }, [deleteWorkEffortCostCalc, labels.deleteSuccess]);

    // REFACTOR: Memoize CostCalcNameCell to prevent re-renders
    const CostCalcNameCell = useMemo(() => {
        return function CostCalcNameCell(props: any) {
            const { costComponentCalcId, costComponentCalcDescription } = props.dataItem;
            const navigationAttributes = useTableKeyboardNavigation(props.id);
            return (
                <td
                    className={props.className}
                    style={{ ...props.style, color: 'blue' }}
                    colSpan={props.colSpan}
                    role="gridcell"
                    aria-colindex={props.ariaColumnIndex}
                    aria-selected={props.isSelected}
                    {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                    {...navigationAttributes}
                >
                    <Button onClick={() => handleSelectCostCalc(props.dataItem)}>
                        {costComponentCalcDescription}
                    </Button>
                </td>
            );
        };
    }, [handleSelectCostCalc]);

    // REFACTOR: Memoize DeleteCell to prevent re-renders
    // Purpose: Stabilizes dependencies to avoid infinite render loops
    const DeleteCell = useMemo(() => {
        return function DeleteCell(props: any) {
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
                        onClick={() => handleDeleteCostCalc(props.dataItem)}
                        disabled={isRemoving}
                    >
                        {labels.delete}
                    </Button>
                </td>
            );
        };
    }, [handleDeleteCostCalc, isRemoving, labels.delete]); // REFACTOR: Removed stableGetTranslatedLabel dependency by pre-computing label outside

    const handleBack = useCallback(() => {
        navigate(-1);
    }, [navigate]);

    const pageChange = useCallback((event: GridPageChangeEvent) => {
        setPage(event.page);
    }, []);

    const sortChange = useCallback((e: GridSortChangeEvent) => {
        setSort(e.sort);
    }, []);

    if (editMode) {
        return (
            <EditWorkEffortCostCalc
                costCalc={selectedCostCalc}
                editMode={editMode}
                cancelEdit={cancelEdit}
                workEffortId={workEffortId!}
                workEffortName={workEffortData?.workEffortName || ''}
            />
        );
    }

    return (
        <>
            <ManufacturingMenu />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1}>
                    <Grid container alignItems="center">
                        <Grid item xs={8}>
                            <Typography sx={{ p: 2 }} variant="h4">
                                {labels.title}
                            </Typography>
                        </Grid>
                    </Grid>
                    <Grid container p={2}>
                        <div className="div-container">
                            <KendoGrid
                                className="main-grid basic-table hover-bar"
                                data={costCalcs}
                                resizable
                                sortable
                                sort={sort}
                                onSortChange={sortChange}
                                skip={page.skip}
                                take={page.take}
                                total={costCalcs.total}
                                pageable
                                onPageChange={pageChange}
                            >
                                <GridToolbar>
                                    <Grid item xs={3}>
                                        <Button
                                            color="secondary"
                                            onClick={() => handleSelectCostCalc()}
                                            variant="outlined"
                                            disabled={isCostCalcsFetching}
                                        >
                                            {labels.create}
                                        </Button>
                                    </Grid>
                                </GridToolbar>
                                <Column
                                    field="costComponentCalcId"
                                    title={labels.costComponentCalc}
                                    cell={CostCalcNameCell}
                                    width={400}
                                />
                                <Column
                                    field="costComponentTypeDescription"
                                    title={labels.costComponentType}
                                    width={250}
                                />
                                <Column
                                    field="fromDate"
                                    title={labels.fromDate}
                                    format="{0:yyyy-MM-dd}"
                                    width={180}
                                />
                                <Column
                                    field="thruDate"
                                    title={labels.thruDate}
                                    format="{0:yyyy-MM-dd}"
                                    width={180}
                                />
                                <Column title=" " cell={DeleteCell} width={100} />
                            </KendoGrid>
                            {(isCostCalcsFetching || isWorkEffortFetching) && (
                                <LoadingComponent message={labels.loading} />
                            )}
                            <Grid container justifyContent="flex-start" sx={{ mt: 2 }}>
                                <Grid item>
                                    <Button color="primary" variant="outlined" onClick={handleBack}>
                                        {labels.back}
                                    </Button>
                                </Grid>
                            </Grid>
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}