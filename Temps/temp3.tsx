// REFACTOR: Add lazy version of the daily payments query
// Purpose: Allow manual triggering + awaiting result in Excel export
// Improvement: Critical for reliable Excel generation without race conditions
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface PaymentsDailyResponse {
    data: PaymentRecordDto[];
    total: number;
}

export interface DailyPaymentsQueryArg {
    paymentType: 'incoming' | 'outgoing';
}

export const api = createApi({
    reducerPath: 'api',
    baseQuery: fetchBaseQuery({ baseUrl: '/api' }),
    endpoints: (builder) => ({
        // Your existing queries...

        // This is the regular auto-triggered one (you probably already have)
        fetchDailyPayments: builder.query<PaymentsDailyResponse, DailyPaymentsQueryArg>({
            query: (arg) => ({
                url: 'accounting/payments/daily',
                params: { paymentType: arg.paymentType },
            }),
        }),

        // ADD THIS: The lazy version (manual trigger)
        // REFACTOR: Export as lazy query for on-demand execution
        // Purpose: Used only by Excel export to guarantee fresh data
        fetchDailyPaymentsLazy: builder.query<PaymentsDailyResponse, DailyPaymentsQueryArg>({
            query: (arg) => ({
                url: 'accounting/payments/daily',
                params: { paymentType: arg.paymentType },
            }),
        }),
    }),
});

// Export both versions
export const {
    useFetchDailyPaymentsQuery,           // ← existing (auto-fetch)
    useLazyFetchDailyPaymentsLazyQuery,   // ← NEW: this is the one we want
} = api;