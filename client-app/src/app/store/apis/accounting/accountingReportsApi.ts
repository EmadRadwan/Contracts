import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";

export interface TransactionEntryDto {
    acctgTransId: string;
    acctgTransEntrySeqId: string;
    transactionDate: string;
    acctgTransTypeId: string;
    acctgTransTypeDescription: string;
    glFiscalTypeId: string;
    invoiceId?: string;
    paymentId?: string;
    workEffortId?: string;
    shipmentId?: string;
    partyId?: string;
    partyName?: string;
    productId?: string;
    productName?: string;
    isPosted: string;
    description?: string;
    postedDate?: string;
    debitCreditFlag: string;
    currencyUomId?: string;
    amount: number;
    certificateNumber?: string;
    projectName?: string;
}

export interface GlAccountTransactionDetails {
    openingBalance: number;
    postedDebits: number;
    postedCredits: number;
    endingBalance: number;
    glAccountId: string;
    accountCode: string;
    accountName: string;
    glAccountClassId?: string;
    transactions: TransactionEntryDto[];
}

const accountingReportsApi = createApi({
    reducerPath: "accountingReports",
    keepUnusedDataFor: 0,
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),
    refetchOnMountOrArgChange: true,
    endpoints(builder) {
        return {
            fetchTrialBalanceReport: builder.query<
                any,
                { customTimePeriodId: string; organizationPartyId: string }
                >({
                query: ({ customTimePeriodId, organizationPartyId }) => {
                    return {
                        url: `/trialBalance/${organizationPartyId}/${customTimePeriodId}/getTrialBalanceReport`,
                        method: "GET",
                    };
                },
                // REFACTOR: Disable caching for this query to ensure fresh data on every call
                // Why: Prevents stale data and forces network request each time
                // Context: Use when report must reflect real-time backend state
                keepUnusedDataFor: 0,
            }),
            fetchTransactionTotalsReport: builder.query<any,
                {
                    organizationPartyId: string;
                    glFiscalTypeId: string;
                    fromDate?: string;
                    thruDate?: string;
                    selectedMonth?: number;
                }>({
                query: ({
                            organizationPartyId,
                            glFiscalTypeId,
                            fromDate,
                            thruDate,
                            selectedMonth,
                        }) => {
                    return {
                        url: `/organizationGlReports/${organizationPartyId}/getTransactionTotalsReport`,
                        method: "GET",
                        params: {glFiscalTypeId, fromDate, thruDate, selectedMonth},
                    };
                },
            }),
            fetchIncomeStatementReport: builder.query<any,
                {
                    organizationPartyId: string;
                    glFiscalTypeId: string;
                    fromDate?: string;
                    thruDate?: string;
                    selectedMonth?: number;
                }>({
                query: ({
                            organizationPartyId,
                            glFiscalTypeId,
                            fromDate,
                            thruDate,
                            selectedMonth,
                        }) => {
                    return {
                        url: `/organizationGlReports/${organizationPartyId}/getIncomeStatementReport`,
                        method: "GET",
                        params: {glFiscalTypeId, fromDate, thruDate, selectedMonth},
                    };
                },
            }),
            fetchCashFlowStatementReport: builder.query<any,
                {
                    organizationPartyId: string;
                    glFiscalTypeId: string;
                    fromDate?: string;
                    thruDate?: string;
                    selectedMonth?: number;
                }>({
                query: ({
                            organizationPartyId,
                            glFiscalTypeId,
                            fromDate,
                            thruDate,
                            selectedMonth,
                        }) => {
                    return {
                        url: `/organizationGlReports/${organizationPartyId}/getCashFlowStatementReport`,
                        method: "GET",
                        params: {glFiscalTypeId, fromDate, thruDate, selectedMonth},
                    };
                },
            }),
            fetchGlAccountTrialBalanceReport: builder.query<any,
                {
                    organizationPartyId: string;
                    glAccountId: string;
                    timePeriodId?: string;
                    isPosted?: string;
                }>({
                query: ({
                            organizationPartyId,
                            glAccountId,
                            isPosted,
                            timePeriodId,
                        }) => {
                    return {
                        url: `/organizationGlReports/${organizationPartyId}/getGlAccountTrialBalanceReport`,
                        method: "GET",
                        params: {glAccountId, timePeriodId, isPosted},
                    };
                },
            }),
            fetchBalanceSheetReport: builder.query<any,
                {
                    organizationPartyId: string;
                    glFiscalTypeId: string;
                    thruDate?: string;
                }>({
                query: ({organizationPartyId, glFiscalTypeId, thruDate}) => {
                    return {
                        url: `/organizationGlReports/${organizationPartyId}/getBalanceSheetReport`,
                        method: "GET",
                        params: {glFiscalTypeId, thruDate},
                    };
                },
            }),
            fetchComparativeBalanceSheetReport: builder.query<any,
                {
                    organizationPartyId: string;
                    period1GlFiscalTypeId: string;
                    period2GlFiscalTypeId: string;
                    period1ThruDate?: string;
                    period2ThruDate?: string;
                }>({
                query: ({
                            organizationPartyId,
                            period1GlFiscalTypeId,
                            period2GlFiscalTypeId,
                            period1ThruDate,
                            period2ThruDate,
                        }) => {
                    return {
                        url: `/organizationGlReports/${organizationPartyId}/generateComparativeBalanceSheet`,
                        method: "GET",
                        params: {
                            period1GlFiscalTypeId,
                            period2GlFiscalTypeId,
                            period1ThruDate,
                            period2ThruDate,
                        },
                    };
                },
            }),
          // New endpoint for transaction details
          fetchGlAccountTransactionDetails: builder.query<GlAccountTransactionDetails, { organizationPartyId: string; customTimePeriodId: string; glAccountId: string; includePrePeriodTransactions: boolean }>({
            query: ({ organizationPartyId, customTimePeriodId, glAccountId, includePrePeriodTransactions }) => ({
              url: `/trialBalance/${organizationPartyId}/${customTimePeriodId}/${glAccountId}/getGlAccountTransactionDetails`,
              method: 'GET',
              params: { includePrePeriodTransactions },
            }),
          }),
          fetchBalanceSheetGlAccountTransactionDetails: builder.query<GlAccountTransactionDetails, { organizationPartyId: string; thruDate: string; glFiscalTypeId: string; glAccountId: string; includePrePeriodTransactions: boolean }>({
            query: ({ organizationPartyId, thruDate, glFiscalTypeId, glAccountId, includePrePeriodTransactions }) => ({
              url: `/organizationGlReports/${organizationPartyId}/getBalanceSheetGlAccountTransactionDetails`,
              method: 'GET',
              params: { thruDate, glFiscalTypeId, glAccountId, includePrePeriodTransactions },
            }),
          }),
          fetchIncomeStatementGlAccountTransactionDetails: builder.query<GlAccountTransactionDetails, { organizationPartyId: string; fromDate?: string; thruDate?: string; selectedMonth?: number; glFiscalTypeId: string; glAccountId: string; includePrePeriodTransactions: boolean }>({
            query: ({ organizationPartyId, fromDate, thruDate, selectedMonth, glFiscalTypeId, glAccountId, includePrePeriodTransactions }) => ({
                url: `/organizationGlReports/${organizationPartyId}/getIncomeStatementGlAccountTransactionDetails`,
                method: 'GET',
                params: { fromDate, thruDate, selectedMonth, glFiscalTypeId, glAccountId, includePrePeriodTransactions },
            }),
          }),
        };
    },
});

export const {
    useLazyFetchTrialBalanceReportQuery,
    useFetchTransactionTotalsReportQuery,
    useFetchIncomeStatementReportQuery,
    useLazyFetchIncomeStatementReportQuery,
    useFetchCashFlowStatementReportQuery,
    useFetchGlAccountTrialBalanceReportQuery,
    useLazyFetchBalanceSheetReportQuery,
    useFetchComparativeBalanceSheetReportQuery, 
    useFetchGlAccountTransactionDetailsQuery,
    useFetchBalanceSheetGlAccountTransactionDetailsQuery,
    useFetchIncomeStatementGlAccountTransactionDetailsQuery
} = accountingReportsApi;
export {accountingReportsApi};
