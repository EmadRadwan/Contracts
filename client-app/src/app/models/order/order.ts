import {OrderItem} from "./orderItem";
import {OrderAdjustment} from "./orderAdjustment";
import {Payment} from "../accounting/payment";
import { OrderTerm } from "./orderTerm";

export interface Order {
    orderId: string;
    orderTypeId?: any;
    orderTypeDescription?: any;
    fromPartyId?: any;
    fromPartyName?: string;
    modificationType?: string;
    orderDate?: Date;
    statusId?: any;
    statusDescription?: string;
    agreementId?: string
    currencyUomId?: any;
    paymentMethodTypeId?: string;
    paymentMethodId?: string;
    productStoreId?: any;
    salesChannelEnumId?: any;
    grandTotal?: any;
    allowSubmit?: string;
    totalAdjustments?: number;
    vehicleId?: any;
    chassisNumber?: string;
    customerRemarks?: any;
    internalRemarks?: any;
    currentMileage?: any;
    billingAccountId?: string;
    useUpToFromBillingAccount?: number;
    invoiceId?: string | null
    paymentId?: string | null
    orderItems?: OrderItem[];
    orderAdjustments?: OrderAdjustment[];
    orderPayments?: Payment[];
    orderTerms?: OrderTerm[]
}

export interface OrderParams {
    orderBy: string;
    customerName?: string;
    customerPhone?: string;
    pageNumber?: number;
    pageSize?: number;
    orderTypes?: string[];
    filter?: any;
    group?: any;
    sort?: any;
    skip?: number;
    take?: number;
}
