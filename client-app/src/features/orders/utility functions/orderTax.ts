import {OrderAdjustment} from "../../../app/models/order/orderAdjustment";

const SALES_TAX = "SALES_TAX";
const VAT_TAX = "VAT_TAX";

export function getExistingTaxAdjustments(orderItems: any, orderAdjustments: any): OrderAdjustment[] {

    if (!orderItems || orderItems.length === 0) {
        return [];
    }


    const taxAdjustments: OrderAdjustment[] = [];

    orderItems.forEach((orderItem) => {
        const existingTaxAdjustmentsForOrderItem = orderAdjustments.filter(
            (orderAdjustment: OrderAdjustment) =>
                orderAdjustment.orderItemSeqId === orderItem.orderItemSeqId &&
                (orderAdjustment.orderAdjustmentTypeId === SALES_TAX ||
                    orderAdjustment.orderAdjustmentTypeId === VAT_TAX)
        );

        if (existingTaxAdjustmentsForOrderItem.length > 0) {
            // mark the tax adjustments as deleted
            taxAdjustments.push(
                ...existingTaxAdjustmentsForOrderItem.map((taxAdjustment) => ({
                    ...taxAdjustment,
                    isAdjustmentDeleted: true,
                }))
            );
        }
    });

    return taxAdjustments;
}
