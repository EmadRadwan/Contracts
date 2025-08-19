import {OrderAdjustment} from "./orderAdjustment";

export interface OrderTaxResult {
    resultMessage: string;
    orderItemAdjustments?: OrderAdjustment[];
}