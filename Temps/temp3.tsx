import { Fragment, useCallback, useEffect, useState } from "react";
import { Button, CircularProgress, Typography } from "@mui/material";
import { Grid, GridColumn as Column, GridPageChangeEvent, GridSortChangeEvent, GridToolbar } from "@progress/kendo-react-grid";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import { useFetchProductionRunTasksQuery } from "../../../app/store/apis";
import { handleDatesArray } from "../../../app/util/utils";
import { ProductionRunRoutingTask } from "../../../app/models/manufacturing/productionRunRoutingTask";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import EditProductionRunDeclRoutingTask from "../form/EditProductionRunDeclRoutingTask";
import { WorkEffort } from "../../../app/models/manufacturing/workEffort";
import ProductionRunDeclareAndProduceTopDiscreate from "../form/ProductionRunDeclareAndProduceTopDiscreate";
import ReserveBomModal from "./ReserveBomModal";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useAppSelector } from "../../../app/store/configureStore";

interface Props {
    productionRunId?: string | undefined;
    productId?: string | undefined;
    startTask?: (dataItem: ProductionRunRoutingTask) => Promise<void>;
    completeTask?: (dataItem: ProductionRunRoutingTask) => Promise<void>;
    declareTask?: (dataItem: ProductionRunRoutingTask) => Promise<void>;
    issueTaskQoh?: (dataItem: ProductionRunRoutingTask) => Promise<void>;
    reserveTaskQoh?: (dataItem: ProductionRunRoutingTask) => Promise<void>;
}

const millisToMinutes = (millis: number) => (millis / 60000).toFixed(2);

export default function ProductionRunTasksList({
                                                   productionRunId,
                                                   productId,
                                                   startTask,
                                                   completeTask,
                                                   declareTask,
                                                   issueTaskQoh,
                                                   reserveTaskQoh,
                                               }: Props) {
    const initialSort: Array<SortDescriptor> = [{ field: "sequenceNum", dir: "asc" }];
    const [sort, setSort] = useState(initialSort);
    const initialDataState: State = { skip: 0, take: 10 };
    const [page, setPage] = useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => setPage(event.page);
    const { getTranslatedLabel } = useTranslationHelper();
    const { language } = useAppSelector(state => state.localization);
    const [productionRunTasks, setProductionRunTasks] = useState<ProductionRunRoutingTask[]>([]);
    const [loadingStates, setLoadingStates] = useState<{ [key: string]: boolean }>({});
    const [loadingText, setLoadingText] = useState<{ [key: string]: string }>({});
    const [showProduceForm, setShowProduceForm] = useState(false);
    const [showDeclareForm, setShowDeclareForm] = useState(false);
    const [showReserveModal, setShowReserveModal] = useState(false);
    const [selectedWorkEffort, setSelectedWorkEffort] = useState<WorkEffort | undefined>(undefined);
    const [firstColumnWidth, setFirstColumnWidth] = useState(150);
    const [reservedBOM, setReservedBOM] = useState<string | null>(null);
    const [showActionColumn, setShowActionColumn] = useState(true);

    const { data: productionRunDetailsData, isLoading } = useFetchProductionRunTasksQuery(productionRunId, {
        skip: !productionRunId,
    });

    useEffect(() => {
        if (productionRunDetailsData) {
            const adjustedData = handleDatesArray(productionRunDetailsData.productionRunRoutingTasks);
            setProductionRunTasks(adjustedData);
        }
    }, [productionRunDetailsData]);

    useEffect(() => {
        let columnWidth = 150;
        let anyButtonVisible = false;
        productionRunTasks.forEach((task, index) => {
            const isFirstTask = index === 0;
            const canShowIssueBOM = isFirstTask && !task.canStartTask && !task.canCompleteTask && task.currentStatusId !== "PRUN_RUNNING" && task.currentStatusId !== "PRUN_COMPLETED";
            const canShowReserveBOM = canShowIssueBOM && reserveTaskQoh;
            const canShowComplete = task.canCompleteTask;
            const canShowDeclare = task.canDeclareTask;
            const shouldRenderButtons = task.canStartTask || canShowComplete || canShowDeclare || canShowIssueBOM || canShowReserveBOM;
            if (shouldRenderButtons) {
                anyButtonVisible = true;
                columnWidth = 300;
            }
            if (reservedBOM === task.workEffortId) {
                columnWidth = 300;
            }
        });
        setFirstColumnWidth(columnWidth);
        setShowActionColumn(anyButtonVisible);
    }, [productionRunTasks, reserveTaskQoh, reservedBOM]);

    const handleButtonClick = async (
        action: (dataItem: ProductionRunRoutingTask) => Promise<void>,
        dataItem: ProductionRunRoutingTask,
        key: string,
        fallbackText: string,
        buttonType: string
    ) => {
        const workEffortId = dataItem.workEffortId;
        if (!workEffortId) return;

        if (buttonType === "declare") {
            setSelectedWorkEffort(dataItem as WorkEffort);
            setShowDeclareForm(true);
            return;
        }

        if (buttonType === "reserve") {
            setSelectedWorkEffort(dataItem as WorkEffort);
            setShowReserveModal(true);
            return;
        }

        const stateKey = `${workEffortId}_${buttonType}`;
        setLoadingStates(prev => ({ ...prev, [stateKey]: true }));
        setLoadingText(prev => ({ ...prev, [stateKey]: `${getTranslatedLabel(`manufacturing.jobshop.prodruntasks.list.${key}`, fallbackText)}` }));
        try {
            await action(dataItem);
            if (buttonType === "issue" && reservedBOM === workEffortId) {
                setReservedBOM(null);
            }
        } catch (error) {
            console.error(error);
        } finally {
            setLoadingStates(prev => ({ ...prev, [stateKey]: false }));
            setLoadingText(prev => ({ ...prev, [stateKey]: "" }));
        }
    };

    const renderActionButtons = (props: any) => {
        const task: ProductionRunRoutingTask = props.dataItem;
        const index = productionRunTasks.indexOf(task);
        const isFirstTask = index === 0;

        if (task.bomReservationInProgress) {
            return (
                <td>
          <span style={{ color: "orange" }}>
            {getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.waitingBOM", "Waiting BOM...")}
          </span>
                </td>
            );
        }

        const shouldShowIssueBomButton = isFirstTask && !task.canStartTask && !task.canCompleteTask && task.currentStatusId !== "PRUN_RUNNING" && task.currentStatusId !== "PRUN_COMPLETED";
        const shouldShowReserveBomButton = shouldShowIssueBomButton && reserveTaskQoh;
        const shouldRenderButtons = task.canStartTask || task.canCompleteTask || task.canDeclareTask || shouldShowIssueBomButton || shouldShowReserveBomButton;
        const userHasPendingReservation = reservedBOM && reservedBOM === task.workEffortId;

        return (
            <td>
                {shouldRenderButtons && (
                    <Fragment>
                        {shouldShowIssueBomButton && (
                            <Button
                                key={`${task.workEffortId}-issue`}
                                variant="contained"
                                color="primary"
                                onClick={() => handleButtonClick(issueTaskQoh!, task, "issuingBOM", "Issuing BOM", "issue")}
                                disabled={loadingStates?.[`${task.workEffortId}_issue`]}
                                style={language === "ar" ? { marginLeft: "4px" } : { marginRight: "4px" }}
                            >
                                {loadingStates?.[`${task.workEffortId}_issue`] ? (
                                    <>
                                        <CircularProgress size={20} />
                                        <span>{loadingText?.[`${task.workEffortId}_issue`] || getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.issuingBOM", "Issuing BOM")}</span>
                                    </>
                                ) : (
                                    getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.issueBOM", "Issue BOM")
                                )}
                            </Button>
                        )}
                        {shouldShowReserveBomButton && (
                            <Button
                                key={`${task.workEffortId}-reserve`}
                                variant="contained"
                                color="secondary"
                                onClick={() => handleButtonClick(reserveTaskQoh!, task, "reservingBOM", "Reserving BOM", "reserve")}
                                disabled={loadingStates?.[`${task.workEffortId}_reserve`]}
                                style={language === "ar" ? { marginLeft: "4px" } : { marginRight: "4px" }}
                            >
                                {loadingStates?.[`${task.workEffortId}_reserve`] ? (
                                    <>
                                        <CircularProgress size={20} />
                                        <span>{loadingText?.[`${task.workEffortId}_reserve`] || getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.reservingBOM", "Reserving BOM")}</span>
                                    </>
                                ) : (
                                    getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.reserveBOM", "Reserve BOM")
                                )}
                            </Button>
                        )}
                        {task.canStartTask && task.currentStatusId !== "PRUN_RUNNING" && !userHasPendingReservation && (
                            <Button
                                key={`${task.workEffortId}-start`}
                                variant="contained"
                                color="primary"
                                onClick={() => handleButtonClick(startTask!, task, "starting", "Starting", "start")}
                                disabled={loadingStates?.[`${task.workEffortId}_start`]}
                            >
                                {loadingStates?.[`${task.workEffortId}_start`] ? (
                                    <>
                                        <CircularProgress size={20} />
                                        <span>{loadingText?.[`${task.workEffortId}_start`] || getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.starting", "Starting")}</span>
                                    </>
                                ) : (
                                    getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.start", "Start")
                                )}
                            </Button>
                        )}
                        {task.canCompleteTask && (
                            <Button
                                key={`${task.workEffortId}-complete`}
                                variant="contained"
                                color="primary"
                                onClick={() => handleButtonClick(completeTask!, task, "completing", "Completing", "complete")}
                                style={language === "ar" ? { marginLeft: "8px" } : { marginRight: "8px" }}
                                disabled={loadingStates?.[`${task.workEffortId}_complete`]}
                            >
                                {loadingStates?.[`${task.workEffortId}_complete`] ? (
                                    <>
                                        <CircularProgress size={20} />
                                        <span>{loadingText?.[`${task.workEffortId}_complete`] || getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.completing", "Completing")}</span>
                                    </>
                                ) : (
                                    getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.complete", "Complete")
                                )}
                            </Button>
                        )}
                        {task.canDeclareTask && (
                            <Button
                                key={`${task.workEffortId}-declare`}
                                variant="contained"
                                color="primary"
                                onClick={() => handleButtonClick(declareTask!, task, "declaring", "Declaring", "declare")}
                                disabled={loadingStates?.[`${task.workEffortId}_declare`]}
                            >
                                {loadingStates?.[`${task.workEffortId}_declare`] ? (
                                    <>
                                        <CircularProgress size={20} />
                                        <span>{loadingText?.[`${task.workEffortId}_declare`] || getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.declaring", "Declaring")}</span>
                                    </>
                                ) : (
                                    getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.declare", "Declare")
                                )}
                            </Button>
                        )}
                    </Fragment>
                )}
            </td>
        );
    };

    const TimeDisplayCell = (props: any) => (
        <td>
            <span>{millisToMinutes(props.dataItem[props.field])} {getTranslatedLabel("general.mins", "mins")}</span>
        </td>
    );

    const memoizedOnClose = useCallback(() => setShowDeclareForm(false), []);
    const memoizedOnClose2 = useCallback(() => setShowProduceForm(false), []);
    const memoizedOnCloseReserve = useCallback(() => setShowReserveModal(false), []);

    return (
        <Fragment>
            {isLoading ? (
                <LoadingComponent message={getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.loading", "Loading Tasks...")} />
            ) : (
                <Grid
                    data={orderBy(productionRunTasks ?? [], sort).slice(page.skip, page.take + page.skip)}
                    onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                    skip={page.skip}
                    take={page.take}
                    total={productionRunTasks ? productionRunTasks.length : 0}
                    pageable={true}
                    onPageChange={pageChange}
                    resizable={true}
                >
                    <GridToolbar sx={{ display: "flex", justifyContent: "space-between" }}>
                        <Typography color="primary" sx={{ fontSize: "18px", fontWeight: "bold" }} variant="h6">
                            {getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.title", "Tasks")}
                        </Typography>
                        {productionRunDetailsData?.canDeclareAndProduce === "Y" && (
                            <Button
                                variant="contained"
                                sx={{ backgroundColor: "blue", color: "white", marginLeft: "auto" }}
                                onClick={() => setShowProduceForm(true)}
                            >
                                {getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.declareButton", "Declare and Produce")}
                            </Button>
                        )}
                    </GridToolbar>
                    {showActionColumn && (
                        <Column
                            title={getTranslatedLabel("general.actions", "Actions")}
                            cell={renderActionButtons}
                            width={firstColumnWidth}
                        />
                    )}
                    <Column
                        field="sequenceNum"
                        title={getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.sequenceNum", "Seq. Number")}
                        width={100}
                    />
                    <Column
                        field="workEffortName"
                        title={getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.workEffortName", "Routing Task Name")}
                        width={250}
                    />
                    <Column
                        field="currentStatusDescription"
                        title={getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.currentStatusDescription", "Status")}
                        width={100}
                    />
                    <Column
                        field="fixedAssetName"
                        title={getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.fixedAssetName", "Fixed Asset")}
                        width={200}
                    />
                    <Column
                        field="estimatedCompletionDate"
                        title={getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.estimatedCompletionDate", "Estimated Completion Date")}
                        width={200}
                        format="{0: dd/MM/yyyy}"
                    />
                    <Column
                        field="actualSetupMillis"
                        title={getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.actualSetupMillis", "Actual Setup Time")}
                        width={200}
                        cell={TimeDisplayCell}
                    />
                    <Column
                        field="actualMilliSeconds"
                        title={getTranslatedLabel("manufacturing.jobshop.prodruntasks.list.actualMilliSeconds", "Actual Run Time")}
                        width={200}
                        cell={TimeDisplayCell}
                    />
                </Grid>
            )}
            {showDeclareForm && (
                <ModalContainer show={showDeclareForm} onClose={memoizedOnClose} width={1250}>
                    <EditProductionRunDeclRoutingTask
                        workEffort={selectedWorkEffort}
                        closeModal={memoizedOnClose}
                        productRunId={productionRunId!}
                    />
                </ModalContainer>
            )}
            {showProduceForm && (
                <ModalContainer show={showProduceForm} onClose={memoizedOnClose2} width={400}>
                    <ProductionRunDeclareAndProduceTopDiscreate
                        productId={productId!}
                        mainProductionRunId={productionRunId!}
                        closeModal={memoizedOnClose2}
                    />
                </ModalContainer>
            )}
            {showReserveModal && selectedWorkEffort && (
                <ModalContainer show={showReserveModal} onClose={memoizedOnCloseReserve} width={600}>
                    <ReserveBomModal
                        // REFACTOR: Use selectedWorkEffort.workEffortId instead of productionRunId.
                        // Purpose: Passes the correct WorkEffortId for the selected task to fetch BOM components.
                        // Benefit: Ensures GetBomInventoryItemsQuery uses the correct WorkEffortId (e.g., "10030").
                        workEffortId={selectedWorkEffort.workEffortId}
                        onClose={memoizedOnCloseReserve}
                        language={language}
                        reserveTaskQoh={reserveTaskQoh!}
                    />
                </ModalContainer>
            )}
        </Fragment>
    );
}