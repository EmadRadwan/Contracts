import {OrderAdjustment} from "../../../app/models/order/orderAdjustment";
import {OrderItem} from "../../../app/models/order/orderItem";

//this function filters an array of OrderAdjustment objects based on specific criteria,
// including the order item sequence ID and a list of adjustment types.
function filterAdjustmentsByTypes(
    adjustments: OrderAdjustment[],
    orderItemSeqId: string,
    adjustmentTypes: string[]
): OrderAdjustment[] {
    return adjustments.filter(
        (adjustment) =>
            !adjustment.isAdjustmentDeleted &&
            adjustment.orderItemSeqId === orderItemSeqId &&
            adjustmentTypes.includes(adjustment.orderAdjustmentTypeId!)
    );
}

//This function calculates the total adjustment amount for a given array of OrderAdjustment objects and an OrderItem
// . The calculation includes adjustment amounts and, optionally,
// the product of unit price and quantity of the order item.
function calculateAdjustments(
    adjustments: OrderAdjustment[],
    orderItem: OrderItem,
    includeQuantity: boolean
): number {
    return adjustments.reduce((sum: number, record: OrderAdjustment) => {
        const adjustmentAmount = record.amount || 0;
        const itemAmount = includeQuantity
            ? orderItem.unitPrice * orderItem.quantity
            : 0;
        return sum + adjustmentAmount + itemAmount;
    }, 0);
}

//This function takes an array of OrderItem objects and an array of OrderAdjustment objects,
// applies various adjustment filters and calculations, and returns an updated array of OrderItem objects
// with additional properties such as discount and promotion adjustments, other adjustments (taxes), and subtotal.
export function CalculateItem(items: OrderItem[], adjustments: OrderAdjustment[]): OrderItem[] {
    const PROMOTION_ADJUSTMENT = "PROMOTION_ADJUSTMENT";
    const DISCOUNT_ADJUSTMENT = "DISCOUNT_ADJUSTMENT";
    const SALES_TAX = "SALES_TAX";
    const VAT_TAX = "VAT_TAX";

    //if (!adjustments || adjustments.length === 0) return items;

    return (
        items?.map((orderItem: OrderItem) => {
            const promotionAdjustments = filterAdjustmentsByTypes(
                adjustments,
                orderItem.orderItemSeqId,
                [PROMOTION_ADJUSTMENT]
            );

            const totalPromotions = calculateAdjustments(
                promotionAdjustments,
                orderItem,
                false
            );

            const discountAdjustments = filterAdjustmentsByTypes(
                adjustments,
                orderItem.orderItemSeqId,
                [DISCOUNT_ADJUSTMENT]
            );

            const totalDisCounts = calculateAdjustments(
                discountAdjustments,
                orderItem,
                false
            );

            const taxAdjustments = filterAdjustmentsByTypes(
                adjustments,
                orderItem.orderItemSeqId,
                [SALES_TAX, VAT_TAX]
            );

            const totalTaxs = calculateAdjustments(
                taxAdjustments,
                orderItem,
                false
            );

            const subTotal =
                orderItem.unitPrice * orderItem.quantity 

            return {
                ...orderItem,
                discountAndPromotionAdjustments: totalPromotions + totalDisCounts,
                otherAdjustments: totalTaxs,
                subTotal,
            };
        }) || []
    );
}
