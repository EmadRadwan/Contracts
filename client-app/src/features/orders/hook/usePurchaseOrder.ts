import {Order} from "../../../app/models/order/order";
import {
    useAddPurchaseOrderMutation,
    useUpdatePurchaseOrderMutation
} from "../../../app/store/apis";
import React, {useState} from "react";
import {useSelector} from "react-redux";

import {toast} from "react-toastify";
import {
    allItemsAreDeletedOrNone,
    orderLevelAdjustmentsTotal,
    orderSubTotal,
    selectAdjustedOrderItemsWithMarkedForDeletionItems
} from "../slice/orderSelectors";
import {orderItemsEntities} from "../slice/orderItemsUiSlice";
import {orderAdjustmentsEntities} from "../slice/orderAdjustmentsUiSlice";
import {setOrderFormEditMode} from "../slice/ordersUiSlice";
import {useAppDispatch} from "../../../app/store/configureStore";
import { orderTermsEntities } from "../slice/orderTermsUiSlice";

type UsePurchaseOrderProps = {
    selectedMenuItem: string;
    formRef2: any;
    editMode: number;
    selectedOrder: Order;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;

};
const usePurchaseOrder = ({
                              selectedMenuItem,
                              formRef2,
                              editMode,
                              selectedOrder,
                              setIsLoading
                          }: UsePurchaseOrderProps) => {

    const [addPurchaseOrder, {
        data: orderResults,
        error,
        isLoading: isAddPurchaseOrderLoading
    }] = useAddPurchaseOrderMutation();

    const dispatch = useAppDispatch();


    const [updatePurchaseOrder, {isLoading: isUpdatePurchaseOrderLoading}] = useUpdatePurchaseOrderMutation();

    const [formEditMode, setFormEditMode] = useState(editMode)
    const [order, setOrder] = useState<Order | undefined>(selectedOrder);
    const orderItemsFromUi: any = useSelector(orderItemsEntities)
    const orderAdjustmentsFromUi: any = useSelector(orderAdjustmentsEntities)
    const orderTermsFromUi: any = useSelector(orderTermsEntities)
    const adjustedOrderItemsWithMarkedForDeletionItems = useSelector(
        selectAdjustedOrderItemsWithMarkedForDeletionItems
    )
    const sTotal: any = useSelector(orderSubTotal)
    const aTotal: any = useSelector(orderLevelAdjustmentsTotal)
    const allItemsAreDeletedOrNot: any = useSelector(allItemsAreDeletedOrNone);
    const orderItemFlat = orderItemsFromUi.map((item: any) => {
        if (typeof item.productId === "object") {
            return {...item, productId: item.productId.productId}
        } else {
            return item
        }
    });

    const updatedOrderItemFlat = adjustedOrderItemsWithMarkedForDeletionItems.map((item: any) => {
        if (typeof item.productId === "object") {
            return {...item, productId: item.productId.productId}
        } else {
            return item
        }
    });


    async function createOrder(newOrder: Order, vendor: any) {
        // Purpose: Simplify error handling and ensure setIsLoading is only called once in the finally block
        try {
            newOrder.orderItems = orderItemFlat;
            newOrder.orderAdjustments = orderAdjustmentsFromUi;
            newOrder.orderTerms = orderTermsFromUi;
            const createdOrder = await addPurchaseOrder(newOrder).unwrap();

            setOrder({
                orderId: createdOrder.orderId,
                fromPartyId: vendor,
                statusDescription: createdOrder.statusDescription,
                currencyUomId: createdOrder.currencyUomId,
                agreementId: createdOrder.agreementId
            });
            setFormEditMode(3);
            dispatch(setOrderFormEditMode(3));
            formRef2.current = !formRef2.current;
            toast.success("Order Created Successfully");
            return { orderId: createdOrder.orderId };
        } catch (error: any) {
            console.error('Failed to create order:', error); 
            // Purpose: Improve error logging for debugging
            toast.error('Failed to create order');
            throw error; 
            // Purpose: Allow handleCreate to handle errors if necessary
        } finally {
            setIsLoading(false); 
            // Purpose: Guarantee LoadingComponent is hidden after operation completes or fails
        }
    }

    async function updateOrApproveOrder(newOrder: Order, vendor: any, modificationType: string) {
        // Purpose: Simplify error handling and ensure consistent loading state management
        try {
            const updatedItems = modificationType === "APPROVE"
                ? updatedOrderItemFlat.map((item: any) => ({
                    ...item,
                    orderId: newOrder.orderId
                }))
                : updatedOrderItemFlat;
            
            newOrder = { ...newOrder, orderItems: updatedItems, modificationType };
            console.log('updatedOrderItemFlat', updatedOrderItemFlat)
            const updatedOrder = await updatePurchaseOrder(newOrder).unwrap();
            setOrder({
                orderId: updatedOrder.orderId,
                fromPartyId: vendor,
                statusDescription: updatedOrder.statusDescription,
                agreementId: updatedOrder.agreementId,
                currencyUomId: updatedOrder.currencyUomId
            });
            const newEditMode = modificationType === "UPDATE" ? 2 : 3;
            setFormEditMode(newEditMode);
            dispatch(setOrderFormEditMode(newEditMode));
            formRef2.current = !formRef2.current;
            toast.success(
                `Order ${modificationType === "UPDATE" ? "updated" : "approved"} Successfully`
            );
            return { orderId: updatedOrder.orderId };
        } catch (error: any) {
            console.error(`Failed to ${modificationType === "UPDATE" ? "update" : "approve"} order:`, error);
            // Purpose: Improve error logging for debugging
            toast.error(`Failed to ${modificationType === "UPDATE" ? "update" : "approve"} order`);
            throw error; 
            // Purpose: Allow handleCreate to handle errors if necessary
        } finally {
            setIsLoading(false); 
            // Purpose: Guarantee LoadingComponent is hidden after operation completes or fails
        }
    }

    async function handleCreate(data: any) {
        setIsLoading(true);
        const vendor = data.values.fromPartyId;
        const actionType = data.selectedMenuItem;

        const newOrder: Order = {
            orderId: data.values.orderId || (formEditMode > 1 ? order?.orderId : "ORDER-DUMMY"),
            fromPartyId: vendor.fromPartyId,
            fromPartyName: vendor.fromPartyName,
            grandTotal: sTotal + aTotal,
            currencyUomId: data.values.currencyUomId,
            agreementId: data.values.agreementId
        };

        if (allItemsAreDeletedOrNot) {
            toast.error('Order Items cannot be empty');
            setIsLoading(false);
            return;
        }

        if (actionType === "Create Order") {
            return await createOrder(newOrder, vendor);
        } else if (actionType === "Update Order") {
            return await updateOrApproveOrder(newOrder, vendor, "UPDATE");
        } else if (actionType === "Approve Order") {
            return await updateOrApproveOrder(newOrder, vendor, "APPROVE");
        } else {
            toast.error("Invalid action type");
            setIsLoading(false);
            return;
        }
    }
    
    return {
        orderResults,
        error,
        isAddPurchaseOrderLoading,
        isUpdatePurchaseOrderLoading,
        formEditMode,
        setFormEditMode,
        order,
        setOrder,
        handleCreate
    };
};
export default usePurchaseOrder;