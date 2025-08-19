import {Order} from "../../../app/models/order/order";
import { paymentsApi, useAddSalesOrderMutation, useQuickShipSalesOrderMutation} from "../../../app/store/apis";
import React, {useState} from "react";
import {useSelector} from "react-redux";

import {toast} from "react-toastify";
import {useUpdateSalesOrderMutation} from "../../../app/store/apis/ordersApi";
import {invoicesApi, useAppDispatch, useAppSelector, useFetchInvoicesQuery} from "../../../app/store/configureStore";
import {
    allItemsAreDeletedOrNone, orderAdjustmentsSelector,
    orderLevelAdjustmentsTotal,
    orderLevelTaxTotal,
    orderSubTotal,
    selectAdjustedOrderItemsWithMarkedForDeletionItems
} from "../slice/orderSelectors";
import {orderAdjustmentsEntities} from "../slice/orderAdjustmentsUiSlice";
import {orderItemsEntities} from "../slice/orderItemsUiSlice";
import {orderPaymentsEntities} from "../slice/orderPaymentsUiSlice";
import {setOrderFormEditMode} from "../slice/ordersUiSlice";
import { orderTermsEntities } from "../slice/orderTermsUiSlice";
import {OrderAdjustment} from "../../../app/models/order/orderAdjustment";

type UseSalesOrderProps = {
    formRef2: any;
    editMode: number;
    selectedOrder: Order;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
};
const useSalesOrder = ({
                           formRef2,
                           editMode,
                           selectedOrder,
                           setIsLoading,
                       }: UseSalesOrderProps) => {

    

    const [order, setOrder] = useState<Order | undefined>(selectedOrder);

    const [
        addSalesOrder,
        {data: orderResults, error, isLoading: isAddSalesOrderLoading},
    ] = useAddSalesOrderMutation();
    const dispatch = useAppDispatch();
    const { refetch: refetchInvoices } = useFetchInvoicesQuery({ take: 6, skip: 0 });

    const [updateSalesOrder, {isLoading: isUpdateSalesOrderLoading}] =
        useUpdateSalesOrderMutation();

    const [quickShipSalesOrderTrigger, {isLoading}] = useQuickShipSalesOrderMutation();

    const [formEditMode, setFormEditMode] = useState(editMode);
    const orderAdjustmentsFromUi: any = useSelector(orderAdjustmentsEntities);
    const orderItemsFromUi: any = useSelector(orderItemsEntities);
    const orderPaymentsFromUi: any = useSelector(orderPaymentsEntities);
    const orderTermsFromUi: any = useSelector(orderTermsEntities)
    const sTotal: any = useSelector(orderSubTotal);
    const taxTotal: any = useAppSelector(orderLevelTaxTotal);
    const aTotal: any = useSelector(orderLevelAdjustmentsTotal);
    const uiOrderAdjustments: any = useSelector(orderAdjustmentsSelector);

    const discounts = uiOrderAdjustments.filter(
        (a: OrderAdjustment) => a.orderAdjustmentTypeId === "DISCOUNT_ADJUSTMENT"
    );
    const discountTotal = -discounts.reduce(
        (a: number, b: OrderAdjustment) => a + Math.abs(b.amount!),
        0
    );
    const adjustedOrderItemsWithMarkedForDeletionItems = useSelector(
        selectAdjustedOrderItemsWithMarkedForDeletionItems,
    );

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
   


    async function createOrder(newOrder: Order, customer: any) {
        setIsLoading(true); // Start loading spinner or progress indicator
        
        try {
            // Prepare the order data
            newOrder.orderItems = orderItemFlat;
            newOrder.orderAdjustments = orderAdjustmentsFromUi;
            newOrder.orderPayments = orderPaymentsFromUi;
            newOrder.orderTerms = orderTermsFromUi

            if (
                (!newOrder.billingAccountId || newOrder.billingAccountId.trim() === '') &&
                // (!newOrder.paymentMethodId || newOrder.paymentMethodId.trim() === '') &&
                (!newOrder.paymentMethodTypeId || newOrder.paymentMethodTypeId.trim() === '')
            ) {
                toast.error("Must select either a billing account or a payment method type.");
                return;
            }




            // Make the API call to add a sales order using Redux mutation hook and unwrap the result
            const createdOrder = await addSalesOrder(newOrder).unwrap();


            // Handle success: Update the order state, show success message, etc.
            setOrder({
                orderId: createdOrder.orderId,
                fromPartyId: customer,
                statusDescription: createdOrder.statusDescription,
                internalRemarks: createdOrder.internalRemarks,
                customerRemarks: createdOrder.customerRemarks,
                billingAccountId: createdOrder.billingAccountId,
                paymentMethodTypeId: createdOrder.paymentMethodTypeId,
                currencyUomId: createdOrder.currencyUomId,
                agreementId: createdOrder.agreementId
            });

            setFormEditMode(createdOrder.statusDescription === "Approved" ? 3 : 2); // Transition to "view mode" or another step in the form process
            dispatch(setOrderFormEditMode(createdOrder.statusDescription === "Approved" ? 3 : 2)); // Update form mode state if needed
            formRef2.current = !formRef2.current; // Trigger any form state updates

            toast.success("Order Created Successfully");
        } catch (error: any) {
            if (error?.message) {
                toast.error(error.message)
            } else if (error?.data?.errorMessage) {
                // Catch the error and display the message returned from the backend
                // If the backend provided a specific error message
                toast.error(error.data.errorMessage);
            } else if (error?.data?.title) {
                // Handle error with a title (like "Insufficient inventory")
                toast.error(error.data.title);
            } else {
                // Fallback generic error
                toast.error("Failed to create order. Please try again.");
            }

            console.error("Error during order creation:", error);
        } finally {
            setIsLoading(false); // Stop loading spinner or progress indicator
        }
    }


    async function updateOrApproveOrder(
        newOrder: Order,
        customer: any,
        modificationType: string,
    ) {
        // formEditMode === 2, edit order
        try {
            // update modification type flag in newOrder to 'Update'
            let updatedOrder: any;
            try {
                newOrder = {...newOrder, modificationType};
                newOrder.orderItems = updatedOrderItemFlat;
                newOrder.orderAdjustments = orderAdjustmentsFromUi;
                newOrder.orderPayments = orderPaymentsFromUi;
                updatedOrder = await updateSalesOrder(newOrder).unwrap();
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
                statusDescription: updatedOrder.statusDescription,
                internalRemarks: updatedOrder.internalRemarks,
                customerRemarks: updatedOrder.customerRemarks,
            });
            formRef2.current = !formRef2.current;
            setFormEditMode(modificationType === "UPDATE" ? 2 : 3);
            dispatch(setOrderFormEditMode(modificationType === "UPDATE" ? 2 : 3));

            toast.success(
                `Order  ${
                    modificationType === "UPDATE" ? "updated" : "approved"
                } Successfully`,
            );
            setIsLoading(false);

        } catch (error: any) {
            toast.error(
                `Failed to ${
                    modificationType === "UPDATE" ? "update" : "approve"
                } order`,
            );
        }
    }

    async function quickShipSalesOrder(newOrder: Order) {
        try {
            
            setIsLoading(true);
            let completedOrder: Order;
            try {
                newOrder = {...newOrder};
                completedOrder = await quickShipSalesOrderTrigger(newOrder).unwrap();
                setOrder({...order, orderId: completedOrder.orderId, statusDescription: completedOrder.statusDescription!, invoiceId: completedOrder.invoiceId, paymentId: completedOrder?.paymentId})
                setIsLoading(false);
                toast.success("Order Shipped Successfully");
                setFormEditMode(4);
                dispatch(setOrderFormEditMode(4));
                dispatch(invoicesApi.util.invalidateTags(["invoices"]))
                dispatch(paymentsApi.util.invalidateTags(["Payments"]))
                refetchInvoices()
            } catch (error) {
                setIsLoading(false);
                toast.error("Failed to ship sales order");
            }
        } catch (error: any) {
            setIsLoading(false);
            toast.error("Failed to ship order");
        }
    }
    
   
    async function handleCreate(data: any) {
        const customer = data.values.fromPartyId;
        const selectedMenuItem = data.selectedMenuItem; // Use the passed value
        
        const newOrder: Order = {
            orderId: formEditMode > 1 ? data.values.orderId : "ORDER-DUMMY",
            fromPartyId: data.values.fromPartyId.fromPartyId,
            fromPartyName: data.values.fromPartyId.fromPartyName,
            paymentMethodTypeId: data.values.paymentMethodTypeId,
            internalRemarks: data.values.internalRemarks,
            customerRemarks: data.values.customerRemarks,
            grandTotal: sTotal + aTotal + taxTotal + discountTotal
        };

        if (data.values.billingAccountId && typeof data.values.billingAccountId === "string") {
            newOrder.billingAccountId = data.values.billingAccountId
        }

       
        if (data.values.currencyUomId) {
            newOrder.currencyUomId = data.values.currencyUomId
        }
        if (data.values.agreementId) {
            newOrder.agreementId = data.values.agreementId
        }

        // pre submit validation
        // check if order items are all deleted, that's also considered as empty order
        if (allItemsAreDeletedOrNot) {
            toast.error('Order Items cannot be empty');
            setIsLoading(false);
            return;
        }

        if (selectedMenuItem === "Create Order") {
            await createOrder(newOrder, customer);
        }

        if (selectedMenuItem === "Update Order") {
            await updateOrApproveOrder(newOrder, customer, "UPDATE");
        }

        if (selectedMenuItem === "Approve Order") {
            await updateOrApproveOrder(newOrder, customer, "APPROVE");
        }

        if (selectedMenuItem === "Quick Ship Order") {
            await quickShipSalesOrder(newOrder);
        }
    }

    return {
        orderResults,
        error,
        isAddSalesOrderLoading,
        isUpdateSalesOrderLoading,
        formEditMode,
        setFormEditMode,
        order,
        setOrder,
        handleCreate,
        isLoading
    };
};
export default useSalesOrder;
