import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {Party} from "../../models/party/party";
import {State, toODataString} from "@progress/kendo-data-query";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const partiesApi = createApi({
    reducerPath: "parties",
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

    endpoints(builder) {
        return {
            fetchParties: builder.query<ListResponse<Party>, State>({
                query: (queryArgs) => {
                    const url = `/odata/partyRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {

                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            fetchCustomer: builder.query<Party, any>({
                query: (partyId) => {
                    return {
                        url: `/parties/${partyId}/getCustomer`,
                        params: partyId,
                        method: "GET",
                    };
                },
            }),
            fetchSupplier: builder.query<Party, any>({
                query: (partyId) => {
                    return {
                        url: `/parties/${partyId}/getSupplier`,
                        params: partyId,
                        method: "GET",
                    };
                },
            }),
            fetchContractor: builder.query<Party, any>({
                query: (partyId) => {
                    return {
                        url: `/parties/${partyId}/getContractor`,
                        params: partyId,
                        method: "GET",
                    };
                },
            }),
            fetchCompanies: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/parties/getCompanies`,
                        method: "GET",
                    };
                },
            }),
            getPartyFinancialHistory: builder.query<PartyFinancialHistoryDetails, string>({
                query: (partyId) => ({
                    url: `parties/${partyId}/getPartyFinancialHistory`,
                    method: 'GET',
                }),
            }),
        };
    },
});

export const {
    useFetchPartiesQuery,
    useFetchCustomerQuery,
    useFetchSupplierQuery,
    useFetchCompaniesQuery, useGetPartyFinancialHistoryQuery,
    useFetchContractorQuery,
} = partiesApi;
export {partiesApi};
