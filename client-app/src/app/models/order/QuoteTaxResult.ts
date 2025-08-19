import {QuoteAdjustment} from "./quoteAdjustment";

export interface QuoteTaxResult {
    resultMessage: string;
    quoteItemAdjustments?: QuoteAdjustment[];
}