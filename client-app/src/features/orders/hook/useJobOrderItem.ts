import {useEffect, useState} from "react";

import {toast} from "react-toastify";
import {useSelector} from "react-redux";
import {
    useAppDispatch,
    useAppSelector,
    useFetchAvailableProductPromotionsQuery,
    useFetchCustomerTaxStatusQuery,
} from "../../../app/store/configureStore";
import {
    deletePromoOrderAdjustments,
    deletePromoOrderItem,
    jobOrderAdjustmentsSelector,
    jobOrderItemsSelector,
    setRelatedRecords,
    setUiJobOrderAdjustments,
    setUiJobOrderItems,
} from "../slice/jobOrderUiSlice";

import {OrderItem} from "../../../app/models/order/orderItem";
import {OrderAdjustment} from "../../../app/models/order/orderAdjustment";
import {
    productsEndpoints,
    salesOrderPromoProductDiscountEndpoints,
    salesOrderTaxAdjustmentsEndpoints,
} from "../../../app/store/apis";

import {Order} from "../../../app/models/order/order";

type UseJobOrderItem = {
    skip: boolean;
    orderItem: any;
    editMode: number;
};
export default function useJobOrderItem({
                                            skip,
                                            orderItem,
                                            editMode,
                                        }: UseJobOrderItem) {
    const productId = useAppSelector(
        (state) => state.jobOrderUi.selectedProductId,
    );
    const [oItem, setOItem] = useState(orderItem);

    const jobOrderItemsFromUi: any = useSelector(jobOrderItemsSelector);
    const jobOrderAdjustmentsFromUi: any = useSelector(
        jobOrderAdjustmentsSelector,
    );
    const isNewServiceRateAndSpecificationAdded = useAppSelector(
        (state) => state.jobOrderUi.isNewServiceRateAndSpecificationAdded,
    );

    const customerId = useAppSelector(
        (state) => state.jobOrderUi.selectedCustomerId,
    );
    const vehicleId = useAppSelector(
        (state) => state.jobOrderUi.selectedVehicleId,
    );
    const {data: customerTaxStatus} = useFetchCustomerTaxStatusQuery(
        customerId,
        {skip: customerId === undefined},
    );

    const dispatch = useAppDispatch();

    const {data: productPromotions} = useFetchAvailableProductPromotionsQuery(
        productId,
        {
            skip: productId === undefined,
        },
    );

    const emptyPromotion = {productPromoId: "", promoText: ""};
    const productPromotionsWithEmpty = productPromotions
        ? [emptyPromotion, ...productPromotions]
        : [emptyPromotion];

    useEffect(() => {
        // new orderItem already created, no need to fetch product
        if (orderItem !== undefined && orderItem.orderId === "ORDER-DUMMY") {
            setOItem({...orderItem, productId: orderItem.productLov});
        }
    }, [orderItem]);

    useEffect(() => {
        // existing orderItem, need to fetch product
        if (orderItem !== undefined && orderItem.orderId !== "ORDER-DUMMY") {
            setOItem({...orderItem});
        }
    }, [orderItem]);

    useEffect(() => {
        if (orderItem === undefined) {
            setOItem(undefined);
        }
    }, [oItem, orderItem]);

    async function handleSubmitData(data: any) {
        let allOrderItems: any[] = []; // Array to store both promo and original order items
        let allOrderAdjustments: any[] = []; // Similarly for order adjustments

        try {
            const newOrderItem = await createOrUpdateOrderItem(data);
            // update allOrderItems with newOrderItem
            allOrderItems.push(newOrderItem);

            // if there's a promotion associated, call the promo API
            if (
                newOrderItem.productPromoId !== undefined &&
                newOrderItem.productPromoId !== "" &&
                newOrderItem.productPromoId !== null
            ) {
                const promoResult = await applyProductPromotions(newOrderItem);
                // if promoResult is not null, update allOrderItems and allOrderAdjustments with promoResult
                if (promoResult.promoResult === "Success") {
                    allOrderItems = [...allOrderItems, ...promoResult.promoOrderItems];
                    allOrderAdjustments = [
                        ...allOrderAdjustments,
                        ...promoResult.promoOrderAdjustments,
                    ];
                } else {
                    return;
                }
            }

            if (customerTaxStatus?.isExempt !== "Y") {
                const taxAdjustments = await fetchAndSetTaxAdjustments({
                    ...newOrderItem,
                    productId: data.productId,
                });
                allOrderAdjustments = [...allOrderAdjustments, ...taxAdjustments];
            }

            // update OrderItems and OrderAdjustments in the ui
            dispatch(setUiJobOrderAdjustments(allOrderAdjustments));
            dispatch(setUiJobOrderItems(allOrderItems));

            toggleOItemState();
        } catch (error) {
            handleError(error);
        }
    }

    const fetchAndSetTaxAdjustments = async (
        orderItem: OrderItem,
    ): Promise<OrderAdjustment[]> => {
        const currentOrderItemAdjustments = jobOrderAdjustmentsFromUi?.filter(
            (orderAdjustment: OrderAdjustment) =>
                orderAdjustment.orderItemSeqId === orderItem.orderItemSeqId,
        );

        const result = await dispatch(
            salesOrderTaxAdjustmentsEndpoints.fetchSalesOrderTaxAdjustments.initiate({
                orderItems: [
                    {...orderItem, productId: orderItem.productId.productId},
                ],
                orderAdjustments: currentOrderItemAdjustments || [],
            }),
        );

        if (result.status === "fulfilled") {
            return result.data || [];
        }

        return [];
    };

    async function createOrUpdateOrderItem(data: any): Promise<OrderItem> {
        // logic to create or update order item
        let newOrderItem: OrderItem;
        if (editMode === 2) {
            newOrderItem = {...data, quantity: data.quantity};
        } else {
            const orderItemSeqId = jobOrderItemsFromUi?.length
                ? jobOrderItemsFromUi?.length + 1
                : 1;

            let servicePrice = 0;
            if (isNewServiceRateAndSpecificationAdded) {
                const itemAndVehicle = {
                    productId: data.productId.productId,
                    vehicleId: vehicleId,
                };
                const servicePriceResult = await dispatch(
                    productsEndpoints.getServiceProductPrice.initiate(itemAndVehicle),
                );
                servicePrice = servicePriceResult.data
                    ? parseFloat(servicePriceResult.data)
                    : 0;
                console.log("servicePriceResult", servicePriceResult);
            }

            newOrderItem = {
                inventoryItemId: data.productId.inventoryItem,
                productTypeId: data.productId.productTypeId,
                isProductDeleted: false,
                orderItemSeqId: orderItemSeqId.toString().padStart(2, "0"),
                productId: data.productId.productId,
                productLov: data.productId,
                productName: data.productId.productName,
                quantity: data.quantity,
                unitListPrice: isNewServiceRateAndSpecificationAdded
                    ? servicePrice.toFixed(2)
                    : +data.productId.price.toFixed(2), // Limit to two decimal places
                productPromoId: data.productPromoId,
            };
            if (orderItem) {
                newOrderItem.orderId = orderItem.orderId;
            } else {
                newOrderItem.orderId = "ORDER-DUMMY";
            }

            if (data.productId.productTypeId === "MARKETING_PKG") {
                dispatch(setRelatedRecords(data.productId.relatedRecords));
            }
        }
        return newOrderItem;
    }

    // This function handles the logic when there's a promotion associated.
    // It will return the order items and adjustments from the promo API.
    async function applyProductPromotions(newOrderItem: OrderItem): Promise<{
        promoOrderItems: Order[];
        promoOrderAdjustments: OrderAdjustment[];
        promoResult: string;
    }> {
        const promoOrderItems: any[] = [];
        const promoOrderAdjustments: any[] = [];
        let result: any | undefined = undefined;
        let promoResult = "";

        if (
            newOrderItem.productPromoId !== undefined &&
            newOrderItem.productPromoId !== "" &&
            newOrderItem.productPromoId !== null
        ) {
            // using newOrderItem.productPromoId, get promoActionEnumId from productPromotions
            const promoActionEnumId = productPromotions?.find(
                (p: any) => p.productPromoId === newOrderItem.productPromoId,
            )?.productPromoActionEnumId;
            // switch case for promoActionEnumId
            switch (promoActionEnumId) {
                case "PROMO_PROD_DISC":
                    result = await dispatch(
                        salesOrderPromoProductDiscountEndpoints.fetchSalesOrderPromoProductDiscount.initiate(
                            newOrderItem,
                        ),
                    );
                    break;
            }
            if (result !== undefined && result !== null) {
                console.log("result", result);
                if (result.data?.resultMessage === "Success") {
                    promoResult = result.data?.resultMessage;
                    // based on editMode, look for the order items in the promo
                    // and delete any similar order items and order item adjustments
                    // then add the new order item and order item adjustments
                    // then update the order items and order item adjustments in the ui
                    if (editMode === 2) {
                        // get from the result the productPromoId from the orderAdjustments
                        // as orderAdjustment.productPromoId should be the same as newOrderItem.productPromoId
                        // and based on that, get the promo orderItem that was added
                        // and delete it

                        const existingOrderAdjustments = jobOrderAdjustmentsFromUi.filter(
                            (uiOa: any) =>
                                uiOa.productPromoId === newOrderItem.productPromoId,
                        );
                        // get existing order items from the ui that are related to existingOrderAdjustments
                        const existingOrderItems = jobOrderItemsFromUi.filter((oi: any) =>
                            existingOrderAdjustments.find(
                                (uiOa: any) =>
                                    uiOa.orderId === oi.orderId &&
                                    uiOa.orderItemSeqId === oi.orderItemSeqId,
                            ),
                        );

                        if (existingOrderItems !== undefined) {
                            // before deleting, delete the order item adjustments

                            dispatch(deletePromoOrderAdjustments(existingOrderAdjustments));

                            dispatch(deletePromoOrderItem(existingOrderItems));
                        }
                    }
                    // get orderItemSeq from the newOrderItem
                    let orderItemSeqId = parseInt(newOrderItem.orderItemSeqId) + 1;
                    // before adding the new order item and order item adjustments
                    // loop through the order items in the promo - result.data.orderItems
                    // and results.data.orderItemAdjustments
                    // and assign the new orderItemSeqId to the order items and order item adjustments
                    // then add the new order item and order item adjustments
                    // then update the order items and order item adjustments in the ui
                    const orderItems = result.data?.orderItems;
                    const orderItemAdjustments = result.data?.orderItemAdjustments;
                    // loop through orderItems
                    orderItems?.forEach((oi: any) => {
                        // assign the new orderItemSeqId to the order items
                        const newPromoOrderItem = {
                            ...oi,
                            orderItemSeqId: orderItemSeqId.toString().padStart(2, "0"),
                        };
                        // link newPromotionOrderItem to the newOrderItem
                        newPromoOrderItem.parentOrderItemSeqId =
                            newOrderItem.orderItemSeqId;
                        // add the new order item
                        promoOrderItems.push(newPromoOrderItem);
                        // loop through orderItemAdjustments
                        orderItemAdjustments?.forEach((oia: any) => {
                            const newPromoOrderItemAdjustment = {
                                ...oia,
                                orderItemSeqId: orderItemSeqId.toString().padStart(2, "0"),
                            };
                            // add the new order item adjustments
                            promoOrderAdjustments.push(newPromoOrderItemAdjustment);
                        });
                        // increment the orderItemSeqId
                        orderItemSeqId++;
                    });
                } else {
                    promoResult = "Failed";
                    toast.error(result!.data?.resultMessage);
                }
            }
        }

        return {promoOrderItems, promoOrderAdjustments, promoResult};
    }

    function toggleOItemState() {
        if (oItem === undefined) {
            setOItem({});
        } else {
            setOItem(undefined);
        }
    }

    function handleError(error: any) {
        console.log(error);
        // Additional error handling logic
    }

    return {
        //orderItemProduct,
        productPromotionsWithEmpty,
        oItem,
        setOItem,
        handleSubmitData,
        productPromotions,
    };
}
