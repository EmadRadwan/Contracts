export interface Agreement {
    agreementId: string;
    partyIdFrom?: string | null;
    partyIdFromName?: string | null;
    fromPartyId?: {fromPartyId?: string | null, fromPartyName?: string | null} | null
    partyIdTo?: string | null;
    partyIdToName?: string | null;
    toPartyId?: {toPartyId?: string | null, toPartyName?: string | null} | null
    roleTypeIdFrom?: string | null;
    roleTypeIdFromDescription?: string | null;
    roleTypeIdTo?: string | null;
    roleTypeIdToDescription?: string | null;
    agreementTypeId?: string | null;
    agreementTypeIdDescription?: string | null;
    agreementDate?: Date | null;
    fromDate?: Date | null;
    thruDate?: Date | null;
    description?: string | null;
    textData?: string | null;
    statusId?: string | null;
}
