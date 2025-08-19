import {Order} from "../../../app/models/order/order";
import React, {useState} from "react";
import {useSelector} from "react-redux";
import {
    jobOrderLevelAdjustmentsTotal,
    jobOrderPaymentsTotal,
    jobOrderSubTotal,
    orderAdjustmentsEntities,
    orderItemsEntities,
    orderPaymentsEntities,
    selectAdjustedOrderItemsWithMarkedForDeletionItems,
} from "../slice/jobOrderUiSlice";
import {toast} from "react-toastify";
import {useCompleteJobOrderMutation, useUpdateOrApproveJobOrderMutation,} from "../../../app/store/apis";

type UseJobOrderProps = {
    selectedMenuItem: string;
    formRef2: any;
    editMode: number;
    selectedOrder: Order;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
};
const useJobOrder = ({
                         selectedMenuItem,
                         formRef2,
                         editMode,
                         selectedOrder,
                         setIsLoading,
                     }: UseJobOrderProps) => {
    const [
        updateJobOrderTrigger,
        {data: jobOrderResults, error, isLoading: isUpdateJobOrderLoading},
    ] = useUpdateOrApproveJobOrderMutation();

    const [
        completeJobOrderTrigger,
        {
            data: completeJobOrderResults,
            error: completeJobOrderError,
            isLoading: isCompleteJobOrderLoading,
        },
    ] = useCompleteJobOrderMutation();

    const [formEditMode, setFormEditMode] = useState(editMode);
    console.log("formEditMode from hook", formEditMode);
    const [order, setOrder] = useState<Order | undefined>(selectedOrder);
    const orderAdjustmentsFromUi: any = useSelector(orderAdjustmentsEntities);
    const orderItemsFromUi: any = useSelector(orderItemsEntities);
    const orderPaymentsFromUi: any = useSelector(orderPaymentsEntities);
    const sTotal: any = useSelector(jobOrderSubTotal);
    const aTotal: any = useSelector(jobOrderLevelAdjustmentsTotal);
    const paidAmount: any = useSelector(jobOrderPaymentsTotal);

    const adjustedOrderItemsWithMarkedForDeletionItems = useSelector(
        selectAdjustedOrderItemsWithMarkedForDeletionItems,
    );

    async function updateOrApproveOrder(
        newOrder: Order,
        customer: any,
        vehicle: any,
        modificationType: string,
    ) {
        try {
            const allItemsAreDeleted = orderItemsFromUi.every(
                (item: any) => item.isProductDeleted === true,
            );

            if (
                !orderItemsFromUi ||
                orderItemsFromUi.length === 0 ||
                allItemsAreDeleted
            ) {
                toast.error("Order Items cannot be empty");
                return;
            } else {
                let updatedOrder: any;
                try {
                    newOrder = {...newOrder, modificationType: modificationType};
                    newOrder.orderItems = adjustedOrderItemsWithMarkedForDeletionItems;
                    newOrder.orderAdjustments = orderAdjustmentsFromUi;
                    newOrder.orderPayments = orderPaymentsFromUi;

                    newOrder.orderItems =
                        adjustedOrderItemsWithMarkedForDeletionItems.map((item: any) => {
                            if (typeof item.productId === "object") {
                                return {...item, productId: item.productId.productId};
                            } else {
                                return item;
                            }
                        });
                    updatedOrder = await updateJobOrderTrigger(newOrder).unwrap();
                    console.log("updatedOrder", updatedOrder);
                } catch (error) {
                    toast.error(
                        `Failed to ${
                            modificationType === "UPDATE" ? "update" : "approve"
                        } order`,
                    );
                }

                setOrder({
                    orderId: updatedOrder.orderId,
                    fromPartyId: customer,
                    vehicleId: vehicle,
                    customerRemarks: updatedOrder.customerRemarks,
                    internalRemarks: updatedOrder.internalRemarks,
                    statusDescription: updatedOrder.statusDescription,
                    currentMileage: updatedOrder.currentMileage,
                });
                setFormEditMode(modificationType === "UPDATE" ? 2 : 3);
                formRef2.current = !formRef2.current;
                toast.success(
                    `Order  ${
                        modificationType === "UPDATE" ? "updated" : "approved"
                    } Successfully`,
                );

                setIsLoading(false);
            }
        } catch (error: any) {
            console.log(error);
            toast.error(
                `Failed to ${
                    modificationType === "UPDATE" ? "update" : "approve"
                } order`,
            );
        }
    }

    async function completeJobOrder(
        newOrder: Order,
        customer: any,
        vehicle: any,
    ) {
        try {
            // check if payment total is equal to order total
            if (paidAmount !== sTotal + aTotal) {
                toast.error("Order total and payment total do not match");
                return false;
            }
            let completedOrder: any;
            try {
                newOrder = {...newOrder};
                newOrder.orderPayments = orderPaymentsFromUi;
                completedOrder = await completeJobOrderResults(newOrder).unwrap();
            } catch (error) {
                toast.error("Failed to complete job order");
            }

            setOrder({
                orderId: completedOrder.orderId,
                fromPartyId: customer,
                vehicleId: vehicle,
                customerRemarks: completedOrder.customerRemarks,
                internalRemarks: completedOrder.internalRemarks,
                statusDescription: completedOrder.statusDescription,
                currentMileage: completedOrder.currentMileage,
            });
            setFormEditMode(4);
            formRef2.current = !formRef2.current;
            toast.success("Order  Completed Successfully");

            setIsLoading(false);
        } catch (error: any) {
            setIsLoading(false);
            console.log(error);
            toast.error("Failed to complete order");
        }
    }

    async function handleCreate(data: any) {
        const customer = data.values.fromPartyId;

        const newOrder: Order = {
            orderId: formEditMode > 1 ? data.values.orderId : "ORDER-DUMMY",
            fromPartyId: data.values.fromPartyId.fromPartyId,
            fromPartyName: data.values.fromPartyId.fromPartyName,
            customerRemarks: data.values.customerRemarks,
            internalRemarks: data.values.internalRemarks,
            currentMileage: data.values.currentMileage,
            vehicleId: data.values.vehicleId.vehicleId,
            grandTotal: sTotal + aTotal,
        };

        // pre submit validation
        // check if order items are all deleted, that's also considered as empty order
        const allItemsAreDeleted = orderItemsFromUi.every(
            (item: any) => item.isProductDeleted === true,
        );
        if (
            !orderItemsFromUi ||
            orderItemsFromUi.length === 0 ||
            allItemsAreDeleted
        ) {
            // add toasts here
            toast.error("Order Items cannot be empty");
            return false;
        }

        if (paidAmount > sTotal + aTotal) {
            toast.error("Paid amount cannot be greater than order total");
            return false;
        }

        if (selectedMenuItem === "Update Job Order") {
            await updateOrApproveOrder(
                newOrder,
                customer,
                data.values.vehicleId,
                "UPDATE",
            );
        }

        if (selectedMenuItem === "Approve Job Order") {
            await updateOrApproveOrder(
                newOrder,
                customer,
                data.values.vehicleId,
                "APPROVE",
            );
        }

        if (selectedMenuItem === "Complete Job Order") {
            // complete order

            await completeJobOrder(newOrder, customer, data.values.vehicleId);
        }
    }

    return {
        jobOrderResults,
        error,
        formEditMode,
        setFormEditMode,
        order,
        setOrder,
        handleCreate,
        isUpdateJobOrderLoading,
    };
};
export default useJobOrder;
