import {QuoteItem} from "./quoteItem";
import {QuoteAdjustment} from "./quoteAdjustment";

export interface QuotePromoResult {
    resultMessage: string;
    quoteItems?: QuoteItem[];
    quoteItemAdjustments?: QuoteAdjustment[];
}