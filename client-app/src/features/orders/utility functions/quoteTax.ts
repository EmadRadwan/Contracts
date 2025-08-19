import {QuoteAdjustment} from "../../../app/models/quote/quoteAdjustment";

const SALES_TAX = "SALES_TAX";
const VAT_TAX = "VAT_TAX";

export function getExistingTaxAdjustments(quoteItems: any, quoteAdjustments: any): QuoteAdjustment[] {

    if (!quoteItems || quoteItems.length === 0) {
        return [];
    }


    const taxAdjustments: QuoteAdjustment[] = [];

    quoteItems.forEach((quoteItem) => {
        const existingTaxAdjustmentsForQuoteItem = quoteAdjustments.filter(
            (quoteAdjustment: QuoteAdjustment) =>
                quoteAdjustment.quoteItemSeqId === quoteItem.quoteItemSeqId &&
                (quoteAdjustment.quoteAdjustmentTypeId === SALES_TAX ||
                    quoteAdjustment.quoteAdjustmentTypeId === VAT_TAX)
        );

        if (existingTaxAdjustmentsForQuoteItem.length > 0) {
            // mark the tax adjustments as deleted
            taxAdjustments.push(
                ...existingTaxAdjustmentsForQuoteItem.map((taxAdjustment) => ({
                    ...taxAdjustment,
                    isAdjustmentDeleted: true,
                }))
            );
        }
    });

    return taxAdjustments;
}
