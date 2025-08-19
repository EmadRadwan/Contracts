//this function filters an array of QuoteAdjustment objects based on specific criteria,
// including the quote item sequence ID and a list of adjustment types.
import {QuoteAdjustment} from "../../../app/models/order/quoteAdjustment";
import {QuoteItem} from "../../../app/models/order/quoteItem";

function filterAdjustmentsByTypes(
    adjustments: QuoteAdjustment[],
    quoteItemSeqId: string,
    adjustmentTypes: string[]
): QuoteAdjustment[] {
    return adjustments.filter(
        (adjustment) =>
            !adjustment.isAdjustmentDeleted &&
            adjustment.quoteItemSeqId === quoteItemSeqId &&
            adjustmentTypes.includes(adjustment.quoteAdjustmentTypeId!)
    );
}

//This function calculates the total adjustment amount for a given array of QuoteAdjustment objects and an QuoteItem
// . The calculation includes adjustment amounts and, optionally,
// the product of unit price and quantity of the quote item.
function calculateAdjustments(
    adjustments: QuoteAdjustment[],
    quoteItem: QuoteItem,
    includeQuantity: boolean
): number {
    return adjustments.reduce((sum: number, record: QuoteAdjustment) => {
        const adjustmentAmount = record.amount || 0;
        const itemAmount = includeQuantity
            ? quoteItem.unitPrice * quoteItem.quantity
            : 0;
        return sum + adjustmentAmount + itemAmount;
    }, 0);
}

//This function takes an array of QuoteItem objects and an array of QuoteAdjustment objects,
// applies various adjustment filters and calculations, and returns an updated array of QuoteItem objects
// with additional properties such as discount and promotion adjustments, other adjustments (taxes), and subtotal.
export function CalculateItem(items: QuoteItem[], adjustments: QuoteAdjustment[]): QuoteItem[] {
    const PROMOTION_ADJUSTMENT = "PROMOTION_ADJUSTMENT";
    const DISCOUNT_ADJUSTMENT = "DISCOUNT_ADJUSTMENT";
    const SALES_TAX = "SALES_TAX";
    const VAT_TAX = "VAT_TAX";

    //if (!adjustments || adjustments.length === 0) return items;

    return (
        items?.map((quoteItem: QuoteItem) => {
            const promotionAdjustments = filterAdjustmentsByTypes(
                adjustments,
                quoteItem.quoteItemSeqId,
                [PROMOTION_ADJUSTMENT]
            );

            const totalPromotions = calculateAdjustments(
                promotionAdjustments,
                quoteItem,
                false
            );

            const discountAdjustments = filterAdjustmentsByTypes(
                adjustments,
                quoteItem.quoteItemSeqId,
                [DISCOUNT_ADJUSTMENT]
            );

            const totalDisCounts = calculateAdjustments(
                discountAdjustments,
                quoteItem,
                true
            );

            const taxAdjustments = filterAdjustmentsByTypes(
                adjustments,
                quoteItem.quoteItemSeqId,
                [SALES_TAX, VAT_TAX]
            );

            const totalTaxs = calculateAdjustments(
                taxAdjustments,
                quoteItem,
                false
            );

            const subTotal =
                quoteItem.unitPrice * quoteItem.quantity +
                totalPromotions +
                totalDisCounts;

            return {
                ...quoteItem,
                discountAndPromotionAdjustments: totalPromotions + totalDisCounts,
                otherAdjustments: totalTaxs,
                subTotal,
            };
        }) || []
    );
}
