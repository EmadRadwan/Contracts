import {QuoteItem} from "../../../app/models/order/quoteItem";
import {Quote} from "../../../app/models/order/quote";
import {QuoteAdjustment} from "../../../app/models/order/quoteAdjustment";


export function getDiscountAdjustments(quoteAdjustments: any, quoteItemSeqId: string) {
    return quoteAdjustments.filter(
        (quoteAdjustment: any) =>
            quoteAdjustment.quoteItemSeqId === quoteItemSeqId &&
            quoteAdjustment.quoteAdjustmentTypeId === 'DISCOUNT_ADJUSTMENT'
    );
}

export function generateNewPromoQuoteItem(
    oi: any,
    quoteItemSeqId: number,
    newQuoteItem: QuoteItem
): Quote {
    return {
        ...oi,
        quoteItemSeqId: quoteItemSeqId.toString().padStart(2, "0"),
        parentQuoteItemSeqId: newQuoteItem.quoteItemSeqId,
    };
}

export function generateNewPromoQuoteItemAdjustment(
    oia: any,
    quoteItemSeqId: number
): QuoteAdjustment {
    return {
        ...oia,
        quoteItemSeqId: quoteItemSeqId.toString().padStart(2, "0"),
    };
}

export const getExistingPromoQuoteItems = (quoteItems: any, newQuoteItem: QuoteItem) => {
    return quoteItems
        .filter((oi: any) => oi.productPromoId === newQuoteItem.productPromoId &&
            oi.quoteId === newQuoteItem.quoteId &&
            oi.quoteItemSeqId === newQuoteItem.quoteItemSeqId)
        .map((oi: any) => ({...oi, isProductDeleted: true}));
};

export const getExistingPromoQuoteAdjustments = (quoteAdjustments: any, newQuoteItem: QuoteItem) => {
    return quoteAdjustments
        .filter(
            (uiOa: any) => uiOa.productPromoId === newQuoteItem.productPromoId &&
                uiOa.quoteItemSeqId === newQuoteItem.quoteItemSeqId &&
                uiOa.quoteId === newQuoteItem.quoteId
        )
        .map((uiOa: any) => ({...uiOa, isAdjustmentDeleted: true}));
};

