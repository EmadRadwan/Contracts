import React from "react";
import {toast} from "react-toastify";
import {WorkEffort} from "../../../app/models/manufacturing/workEffort";
import {
    RootState,
    useAppDispatch,
    useChangeProductionRunStatusMutation,
    useCreateProductionRunMutation, useIssueProductionRunTaskMutation
} from "../../../app/store/configureStore";
import {
    setJobRunUnderProcessing,
    setProductionRunStatusDescription
} from "../slice/manufacturingSharedUiSlice";
import {ChangeProductionRunStatus} from "../../../app/models/manufacturing/ChangeProductionRunStatus";
import {
    useChangeProductionRunTaskStatusMutation, useQuickChangeProductionRunStatusMutation,
    useReserveProductionRunTaskMutation,
    useUpdateProductionRunMutation
} from "../../../app/store/apis";
import {ChangeProductionRunTaskStatus} from "../../../app/models/manufacturing/ChangeProductionRunTaskStatus";
import {useSelector} from "react-redux";
import {IssueProductionRunTaskParams} from "../../../app/models/manufacturing/IssueProductionRunTaskParams";
import {ReserveProductionRunTaskParams} from "../../../app/models/manufacturing/ReserveProductionRunTaskParams";

const ERROR_CODES = {
    WORK_EFFORT_NOT_FOUND: "WORK_EFFORT_NOT_FOUND",
    INVALID_WORK_EFFORT_STATUS: "INVALID_WORK_EFFORT_STATUS",
    INVALID_PRODUCTION_RUN_STATUS: "INVALID_PRODUCTION_RUN_STATUS",
    INSUFFICIENT_INVENTORY: "INSUFFICIENT_INVENTORY",
    DEFAULT: "DEFAULT",
};

const errorMessages: Record<string, Record<string, string>> = {
    en: {
        [ERROR_CODES.WORK_EFFORT_NOT_FOUND]: "The specified work effort could not be found.",
        [ERROR_CODES.INVALID_WORK_EFFORT_STATUS]: "Cannot issue inventory for a cancelled work effort.",
        [ERROR_CODES.INVALID_PRODUCTION_RUN_STATUS]: "The production run is cancelled or closed.",
        [ERROR_CODES.INSUFFICIENT_INVENTORY]: "Insufficient inventory for some items.",
        [ERROR_CODES.DEFAULT]: "An unexpected error occurred. Please try again.",
    },
    ar: {
        [ERROR_CODES.WORK_EFFORT_NOT_FOUND]: "جهد العمل المحدد غير موجود.",
        [ERROR_CODES.INVALID_WORK_EFFORT_STATUS]: "لا يمكن إصدار المخزون لجهد عمل ملغى.",
        [ERROR_CODES.INVALID_PRODUCTION_RUN_STATUS]: "عملية الإنتاج ملغاة أو مغلقة.",
        [ERROR_CODES.INSUFFICIENT_INVENTORY]: "المخزون غير كافٍ لبعض العناصر.",
        [ERROR_CODES.DEFAULT]: "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.",
    },
};

type UseProductionRunProps = {
    selectedMenuItem: string;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
};
const useProductionRun = ({
                              selectedMenuItem,
                              setIsLoading
                          }: UseProductionRunProps) => {

    const dispatch = useAppDispatch();
    const jobRunUnderProcessing = useSelector((state: RootState) => state.manufacturingSharedUi.jobRunUnderProcessing);
    const language = useSelector((state: RootState) => state.localization.language || "en");


    const [
        addProductionRun,
        {data: createProductionRunResults, error, isLoading},
    ] = useCreateProductionRunMutation();

    const [
        editProductionRun,
        {data: updateProductionRunResults},
    ] = useUpdateProductionRunMutation();

    const [
        changeProductionRunStatusTrigger,
        {data: changeProductionRunStatusResults, changeProductionRunStatusError, changeProductionRunStatusIsLoading},
    ] = useChangeProductionRunStatusMutation();

    const [
        quickChangeProductionRunStatusTrigger,
        {data: quickChangeProductionRunStatusResults, quickChangeProductionRunStatusError, quickChangeProductionRunStatusIsLoading},
    ] = useQuickChangeProductionRunStatusMutation();

    const [
        changeProductionRunTaskStatusTrigger
    ] = useChangeProductionRunTaskStatusMutation();

    const [
        issueProductionRunTaskTrigger
    ] = useIssueProductionRunTaskMutation();

    const [
    reserveProductionRunTaskTrigger
    ] = useReserveProductionRunTaskMutation();


    const handleApiError = (error: any, defaultMessage: string) => {
        const errorCode = error?.data?.errorCode || ERROR_CODES.DEFAULT;
        let errorMessage = error?.data?.errorMessage || defaultMessage;

        // REFACTOR: Customize error message for INSUFFICIENT_INVENTORY
        if (errorCode === ERROR_CODES.INSUFFICIENT_INVENTORY && error?.data?.value?.insufficientItems) {
            const items = error.data.value.insufficientItems;
            errorMessage = `${errorMessages[language][ERROR_CODES.INSUFFICIENT_INVENTORY]}: ` +
                items.map((item: { productName: string; quantityMissing: number }) => `${item.productName} (${item.quantityMissing})`).join(", ");
        } else {
            errorMessage = errorMessages[language][errorCode] || errorMessage || defaultMessage;
        }

        toast.error(errorMessage);
        console.error(error);
        setIsLoading(false);
    };
    
    async function createProductionRun(newProductionRun: WorkEffort) {
        try {
            let createdProductionRun: any;
            try {
                createdProductionRun = await addProductionRun(newProductionRun).unwrap();
                console.log('createdProductionRun', createdProductionRun);
            } catch (error) {
                toast.error("Failed to create productionRun");
            }
            if (createdProductionRun) {
                const workEffort = {
                    workEffortId: createdProductionRun.productionRunId,
                    workEffortName: newProductionRun.workEffortName,
                    productId: newProductionRun.productLov,
                    productName: newProductionRun.productLov.productName,
                    estimatedStartDate: createdProductionRun.estimatedStartDate,
                    currentStatusDescription: createdProductionRun.currentStatusDescription,
                    estimatedCompletionDate: createdProductionRun.estimatedCompletionDate,
                    facilityId: newProductionRun.facilityId,
                    facilityName: createdProductionRun.facilityName,
                    quantityToProduce: newProductionRun.quantityToProduce,
                };

                dispatch(setJobRunUnderProcessing(workEffort));
                dispatch(setProductionRunStatusDescription(createdProductionRun.currentStatusDescription));
                toast.success("Production Run Created Successfully");
            }

        } catch (error: any) {
            console.log(error);
        }
        setIsLoading(false);
    }

    async function updateProductionRun(existingProductionRun: WorkEffort) {
        try {
            let updatedProductionRun: any;
            try {
                updatedProductionRun = await editProductionRun(existingProductionRun).unwrap();
            } catch (error) {
                toast.error("Failed to update productionRun");
            }
            if (updatedProductionRun) {
                const workEffort = {
                    workEffortName: existingProductionRun.workEffortName,
                    estimatedStartDate: updatedProductionRun.estimatedStartDate,
                    estimatedCompletionDate: updatedProductionRun.estimatedCompletionDate,
                    facilityId: existingProductionRun.facilityId,
                    facilityName: updatedProductionRun.facilityName,
                    quantityToProduce: updatedProductionRun.quantity,
                };

                dispatch(setJobRunUnderProcessing(workEffort));
                toast.success("Production Run Updated Successfully");
            }

        } catch (error: any) {
            console.log(error);
        }
        setIsLoading(false);
    }

    async function changeProductionRunStatusToScheduled(newProductionRunStatus: ChangeProductionRunStatus) {
        try {
            let createdProductionRun: any;
            try {
                createdProductionRun = await changeProductionRunStatusTrigger(newProductionRunStatus).unwrap();
            } catch (error) {
                toast.error("Failed to schedule production Run");
            }
            if (createdProductionRun) {

                const workEffort = {
                    currentStatusDescription: "Scheduled"
                };

                dispatch(setJobRunUnderProcessing(workEffort));

                dispatch(setProductionRunStatusDescription('Scheduled'));
                toast.success("Production Run Scheduled");
            }

        } catch (error: any) {
            console.log(error);
        }
        setIsLoading(false);
    }

    async function changeProductionRunStatusToConfirmed(newProductionRunStatus: ChangeProductionRunStatus) {
        try {
            let createdProductionRun: any;
            try {
                createdProductionRun = await changeProductionRunStatusTrigger(newProductionRunStatus).unwrap();
            } catch (error) {
                toast.error("Failed to confirm production Run");
            }
            if (createdProductionRun) {

                const workEffort = {
                    currentStatusDescription: "Confirmed"
                };
                dispatch(setJobRunUnderProcessing(workEffort));

                dispatch(setProductionRunStatusDescription("Confirmed"));
                toast.success("Production Run Confirmed");
            }

        } catch (error: any) {
            console.log(error);
        }
        setIsLoading(false);
    }

    async function changeProductionRunTaskStatus(newProductionRunTaskStatus: ChangeProductionRunTaskStatus) {
        try {
            let createdProductionRun: any;
            try {
                createdProductionRun = await changeProductionRunTaskStatusTrigger(newProductionRunTaskStatus).unwrap();
            } catch (error) {
                toast.error("Failed to change production Run task status");
            }
            if (createdProductionRun) {
                console.log('createdProductionRun from Start', createdProductionRun);

                const workEffort = {
                    actualStartDate: createdProductionRun.mainProductionRunStartDate,
                    currentStatusDescription: createdProductionRun.mainProductionRunStatus,
                };
                dispatch(setJobRunUnderProcessing(workEffort));
                dispatch(setProductionRunStatusDescription(createdProductionRun.currentStatusDescription));
                toast.success("Production Run Task Status Changed Successfully");
            }

        } catch (error: any) {
            console.log(error);
        }
        setIsLoading(false);
    }

    async function issueProductionRunTask(issueProductionRunTaskParams: IssueProductionRunTaskParams) {
        try {
            const createdProductionRun = await issueProductionRunTaskTrigger(issueProductionRunTaskParams).unwrap();

            if (createdProductionRun) {

                const workEffort = {
                    actualStartDate: createdProductionRun.mainProductionRunStartDate,
                    currentStatusDescription: createdProductionRun.mainProductionRunStatus,
                };
                //dispatch(setJobRunUnderProcessing(workEffort));
                //dispatch(setProductionRunStatusDescription(createdProductionRun.currentStatusDescription));
                toast.success("Production Run Components Issued Successfully");
            }

        } catch (error: any) {
            handleApiError(error, "Failed to issue components");
        }
        setIsLoading(false);
    }
    
    async function reserveProductionRunTask(reserveProductionRunTaskParams: ReserveProductionRunTaskParams) {
        try {
            let createdProductionRun: any;
            try {
                createdProductionRun = await reserveProductionRunTaskTrigger(reserveProductionRunTaskParams).unwrap();
            } catch (error) {
                toast.error("Failed to issue components");
            }
            if (createdProductionRun) {

                const workEffort = {
                    actualStartDate: createdProductionRun.mainProductionRunStartDate,
                    currentStatusDescription: createdProductionRun.mainProductionRunStatus,
                };
                //dispatch(setJobRunUnderProcessing(workEffort));
                //dispatch(setProductionRunStatusDescription(createdProductionRun.currentStatusDescription));
                toast.success("Production Run Components Reserved Successfully");
            }

        } catch (error: any) {
            console.log(error);
        }
        setIsLoading(false);
    }


    async function handleStatusChangeStart(workEffort: WorkEffort): Promise<void> {
        try {
            await changeProductionRunTaskStatus({
                productionRunId: workEffort?.workEffortParentId,
                statusId: "PRUN_RUNNING",
                taskId: workEffort.workEffortId
            });
        } catch (error) {
            toast.error("Failed to start task");
            console.error(error);
        }
    }

    async function handleStatusChangeComplete(workEffort: WorkEffort): Promise<void> {
        try {
            await changeProductionRunTaskStatus({
                productionRunId: workEffort?.workEffortParentId,
                statusId: "PRUN_COMPLETED",
                taskId: workEffort.workEffortId
            });
        } catch (error) {
            toast.error("Failed to complete task");
            console.error(error);
        }
    }

    async function handleIssueTaskQoh(workEffort: WorkEffort): Promise<void> {
        try {
            await issueProductionRunTask({
                workEffortId: workEffort?.workEffortId,
                reserveOrderEnumId: null,
                failIfItemsAreNotAvailable: 'Y'
            });
        } catch (error) {
            toast.error("Failed to issue ATP task");
            console.error(error);
        }
    }
    
    async function handleQuickChangeProductionRunStatus(workEffort: WorkEffort): Promise<void> {
        let response: any;
        try {
            response = await quickChangeProductionRunStatusTrigger({
                productionRunId: workEffort?.workEffortId, 
                statusId: "PRUN_COMPLETED",
                startAllTasks: "Y"
            }).unwrap();
            
            if (response) {
                console.log('response from Quick', response);
                const workEffort = {
                    actualStartDate: response.actualStartDate,
                    actualCompletionDate: response.actualCompletionDate,
                };
                dispatch(setJobRunUnderProcessing(workEffort));
                dispatch(setProductionRunStatusDescription(response.currentStatusDescription));
                toast.success("Production Run completed Successfully");
            }

        } catch (error) {
            toast.error("Failed to Complete Production Run");
            console.error(error);
        }
        setIsLoading(false);
    }
    
    async function handleReserveTaskQoh(workEffort: WorkEffort): Promise<void> {
        try {
            await reserveProductionRunTask({
                workEffortId: workEffort?.workEffortId,
            });
        } catch (error) {
            toast.error("Failed to reserve inventory");
            console.error(error);
        }
    }

    async function handleCreate(data: any) {

        const newProductionRun: WorkEffort = {
            workEffortId:
                jobRunUnderProcessing !== undefined
                    ? jobRunUnderProcessing!.workEffortId
                    : "ProductionRun-DUMMY",
            workEffortName: data.values.workEffortName,
            productId: data.values.productId.productId,
            productLov: data.values.productId,
            productName: data.values.productId.productName,
            estimatedStartDate: data.values.estimatedStartDate,
            quantityToProduce: data.values.quantityToProduce,
            facilityId: data.values.facilityId,
            description: data.values.description,
            routingId: data.values.routingId,
        };

        


        if (selectedMenuItem === "Create Production Run") {
            await createProductionRun(newProductionRun);
        }

        if (selectedMenuItem === "Edit Production Run") {
            const editProductionRun: any = {
              productionRunId: jobRunUnderProcessing!.workEffortId,
              workEffortName: data.values.workEffortName,
              estimatedStartDate: data.values.estimatedStartDate,
              quantity: data.values.quantityToProduce,
              facilityId: data.values.facilityId,
              description: data.values.description,
            };
            await updateProductionRun(editProductionRun);
        }

        if (selectedMenuItem === "Schedule") {
            await changeProductionRunStatusToScheduled({
                productionRunId: newProductionRun?.workEffortId,
                statusId: "PRUN_SCHEDULED"
            });
        }

        if (selectedMenuItem === "Confirm") {
            await changeProductionRunStatusToConfirmed({
                productionRunId: newProductionRun?.workEffortId,
                statusId: "PRUN_DOC_PRINTED"
            });
        }
        

    }


    return {
        handleCreate,
        isLoading, handleStatusChangeStart, handleStatusChangeComplete, handleIssueTaskQoh, handleReserveTaskQoh, handleQuickChangeProductionRunStatus
    };
};
export default useProductionRun;

