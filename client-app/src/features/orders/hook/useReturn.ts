import { useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import { toast } from "react-toastify";
import {
    useAddReturnMutation,
    useFetchProductStoreFacilitiesQuery,
    useFetchSalesOrderItemsQuery,
    useFetchOrderAdjustmentsQuery,
} from "../../../app/store/apis";
import { Return } from "../../../app/models/order/return";
import { ReturnItem } from "../../../app/models/order/returnItem";
import { ReturnAdjustment } from "../../../app/models/order/returnAdjustment";
import { ReturnItemAndAdjustment } from "../../../app/models/order/returnItemAndAdjustment";
import {
    setSelectedReturn,
    setUiReturnItems,
    setUiReturnAdjustments,
    setUiReturnItemsAndAdjustments,
} from "../slice/returnUiSlice";

type UseReturnHeaderProps = {
    selectedMenuItem: string;
    formRef2: any;
    editMode: number;
    selectedReturn: Return | undefined;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
};



const useReturn = ({
                       selectedMenuItem,
                       formRef2,
                       editMode,
                       selectedReturn,
                       setIsLoading,
                   }: UseReturnHeaderProps) => {
    const dispatch = useAppDispatch();
    const orderId = useAppSelector((state) => state.returnUi.selectedOrderId);

    const [addReturnHeader, { isLoading: isAddReturnHeaderLoading }] = useAddReturnMutation();
    const {
        data: productStoreFacilities,
        error: errorGettingProductStoreFacilities,
        isFetching: isFetchingFacilities,
    } = useFetchProductStoreFacilitiesQuery(undefined);
    
    const { data: orderItemsData, error: orderItemsError } = useFetchSalesOrderItemsQuery(orderId, {
        skip: !orderId,
    });
    
    const { data: orderAdjustmentsData, error: orderAdjustmentsError } = useFetchOrderAdjustmentsQuery(orderId, {
        skip: !orderId,
    });

    const [formEditMode, setFormEditMode] = useState(editMode);
    
    const [returnHeader, setReturnHeader] = useState<Return | undefined>(
        selectedReturn || {
            returnId: undefined,
            companyId: null,
            currencyUomId: undefined,
            returnHeaderTypeId: "CUSTOMER_RETURN",
            entryDate: new Date(),
            statusId: "RETURN_REQUESTED",
            needsInventoryReceive: "Y",
        }
    );
    const [returnItems, setReturnItems] = useState<ReturnItem[]>([]);
    const [returnAdjustments, setReturnAdjustments] = useState<ReturnAdjustment[]>([]);

    // Sync returnHeader with selectedReturn for existing returns
    useEffect(() => {
        if (selectedReturn) {
            let transformedReturn: ReturnHeader = { ...selectedReturn };
            if (selectedReturn.returnHeaderTypeId === 'CUSTOMER_RETURN') {
                transformedReturn = {
                    ...selectedReturn,
                    /*fromPartyId: selectedReturn.fromPartyId
                        ? {
                            fromPartyId: selectedReturn.fromPartyId,
                            fromPartyName: selectedReturn.fromPartyName,
                        }
                        : null,
                    toPartyId: null,*/
                    companyId: selectedReturn.toPartyId
                        ? selectedReturn.toPartyId
                        : null,
                };
            } else if (selectedReturn.returnHeaderTypeId === 'VENDOR_RETURN') {
                transformedReturn = {
                    ...selectedReturn,
                    fromPartyId: null,
                   /* toPartyId: selectedReturn.toPartyId
                        ? {
                            fromPartyId: selectedReturn.toPartyId,
                            fromPartyName: selectedReturn.toPartyName,
                        }
                        : null,*/
                    companyId: selectedReturn.fromPartyId
                        ? selectedReturn.fromPartyId
                        : null,
                };
            }

            console.log('Transformed returnHeader:', transformedReturn); // Debug log
            setReturnHeader(transformedReturn);
            setFormEditMode(editMode);
        } else if (editMode === 1) {
            // New return
            setReturnHeader({
                returnId: undefined,
                returnHeaderTypeId: 'CUSTOMER_RETURN',
                statusId: 'RETURN_REQUESTED',
                entryDate: new Date().toISOString(),
                needsInventoryReceive: 'Y',
                fromPartyId: null,
                toPartyId: null,
                companyId: null,
            });
            setFormEditMode(1);
        }
    }, [selectedReturn, editMode]);


    // Data preparation for FormComboBoxVirtualCustomer and FormComboBoxVirtualSupplier
    const customerData = returnHeader?.fromPartyId ? [returnHeader.fromPartyId] : [];
    const supplierData = returnHeader?.toPartyId ? [returnHeader.toPartyId] : [];


    async function createReturn(newReturn: Return) {
        try {
            const createdReturn = await addReturnHeader({
                ...newReturn,
                needsInventoryReceive: newReturn.needsInventoryReceive || (newReturn.returnHeaderTypeId === "VENDOR_RETURN" ? "N" : "Y"),
            }).unwrap();
            toast.success(`Return ${createdReturn.returnId} created successfully`);
            setReturnHeader({
                ...createdReturn,
                fromPartyId: {
                    partyId: createdReturn.fromPartyId,
                    partyName: createdReturn.fromPartyName || createdReturn.fromPartyId,
                },
                toPartyId: {
                    partyId: createdReturn.toPartyId,
                    partyName: createdReturn.toPartyName || createdReturn.toPartyId,
                },
                needsInventoryReceive: createdReturn.needsInventoryReceive || (createdReturn.returnHeaderTypeId === "VENDOR_RETURN" ? "N" : "Y"),
            });
            setFormEditMode(2);
            dispatch(setSelectedReturn(createdReturn));
        } catch (e) {
            toast.error("Failed to create return");
            console.error(e);
        } finally {
            setIsLoading(false);
        }
    }

    async function updateReturn(newReturn: Return) {
        toast.info("Update return functionality not yet implemented");
        setIsLoading(false);
    }


    const handleCreate = async (data: any, action: string) => {
        try {
            setIsLoading(true);
            const submitData = { ...data };

            if (data.returnHeaderTypeId === "CUSTOMER_RETURN") {
                submitData.fromPartyId = data.fromPartyId?.fromPartyId || data.fromPartyId;
                submitData.toPartyId = data.companyId;
            } else {
                submitData.fromPartyId = data.companyId;
                submitData.toPartyId = data.toPartyId?.fromPartyId || data.toPartyId;
            }

            console.log("Submitting return:", submitData); // Debug log
            if (action === "Create Return" || formEditMode === 1) {
                await createReturn(submitData);
            } else if (action === "Update Return") {
                await updateReturn(submitData);
            } else if (action === "Approve Return") {
                submitData.statusId = "RETURN_ACCEPTED";
                await updateReturn(submitData);
            } else if (action === "Complete Return") {
                submitData.statusId = "RETURN_COMPLETED";
                await updateReturn(submitData);
            }
        } catch (error) {
            console.error("Error processing return:", error);
            toast.error(`Failed to ${action.toLowerCase()}`);
        } finally {
            setIsLoading(false);
        }
    };



    return {
        returnHeader,
        setReturnHeader,
        returnItems,
        setReturnItems,
        returnAdjustments,
        setReturnAdjustments,
        formEditMode,
        setFormEditMode,
        handleCreate,
        productStoreFacilities,
        customerData,
        supplierData,
        isLoading: isAddReturnHeaderLoading || isFetchingFacilities,
    };
};

export default useReturn;