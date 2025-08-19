import {ReturnItem} from "./returnItem";

export interface Return {
    returnId: string;
    returnHeaderTypeId?: string;
    statusId?: string;
    createdBy?: string;
    fromPartyId?: any;
    fromPartyName?: string;
    toPartyId?: any;
    toPartyName?: string;
    paymentMethodId?: string;
    finAccountId?: string;
    billingAccountId?: string;
    entryDate?: Date;
    originContactMechId?: string;
    destinationFacilityId?: string;
    needsInventoryReceive?: string;
    currencyUomId?: string;
    supplierRmaId?: string;
    lastUpdatedStamp?: Date;
    lastUpdatedTxStamp?: Date;
    createdStamp?: Date;
    createdTxStamp?: Date;

    returnItems?: ReturnItem[];
}
