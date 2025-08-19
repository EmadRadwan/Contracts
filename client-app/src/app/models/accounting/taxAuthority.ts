export interface TaxAuthority {
    taxAuthGeoId?: string;
    taxAuthGeoDescription?: string;
    taxAuthPartyId?: string;
    taxAuthPartyName?: string;
    requireTaxIdForExemption?: boolean;
    taxIdFormatPattern?: string;
    includeTaxInPrice?: boolean;
}
