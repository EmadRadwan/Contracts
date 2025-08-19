export interface Payment {
  paymentId: string;
  paymentTypeId?: string;
  paymentTypeDescription?: string;
  paymentMethodTypeId?: string;
  paymentMethodId?: string;
  paymentMethodTypeDescription?: string;
  fromPartyId?: any;
  fromPartyName?: string;
  partyIdFromName: string;
  organizationPartyId?: any;
  organizationPartyName?: string;
  partyIdFrom?: any;
  partyIdTo?: any;
  partyIdToName?: string;
  isDepositWithDrawPayment?: string;
  roleTypeIdTo?: any;
  statusId?: string;
  statusDescription?: string;
  effectiveDate?: any;
  paymentRefNum?: any;
  amount?: any;
  currencyUomId?: string;
  comments?: any;
  finAccountTransId?: any;
  finAcctTransTypeId?: any;
  overrideGlAccountId?: any;
  actualCurrencyAmount?: any;
  actualCurrencyUomId?: any;
  total?: any;
  allowSubmit?: boolean;
  amountToPay?: any;
  inEdit?: boolean | string;
  isPaymentDeleted?: boolean;
  isDisbursement?: boolean;
  paymentApplications?: any[];
}


