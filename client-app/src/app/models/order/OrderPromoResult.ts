import {OrderItem} from "./orderItem";
import {OrderAdjustment} from "./orderAdjustment";

export interface OrderPromoResult {
    resultMessage: string;
    orderItems?: OrderItem[];
    orderItemAdjustments?: OrderAdjustment[];
}