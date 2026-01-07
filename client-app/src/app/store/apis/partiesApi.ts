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
    tagTypes: ["Parties"],
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
                providesTags: ["Parties"],
                transformResponse: (response: any, meta, arg) => {
                    console.log("Parties response meta:", meta);
                    console.log("Parties response arg:", arg);
                    console.log("Parties response data:", response);
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            updatePartyMainRole: builder.mutation<PartyDto, { partyId: string; mainRole: string }>({
                query: ({ partyId, mainRole }) => ({
                    url: `parties/updateMainRole/${partyId}`, // adjust to your actual route
                    method: 'PUT',
                    body: { mainRole },
                }),
                invalidatesTags: ['Parties'], // assuming you use tags; otherwise it will refetch via query invalidation
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
            fetchEmployee: builder.query<Party, string>({
                query: (partyId) => ({
                    url: `/parties/${partyId}/getEmployee`,
                    method: "GET",
                }),
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
    useFetchContractorQuery, useFetchEmployeeQuery, useUpdatePartyMainRoleMutation
} = partiesApi;
export {partiesApi};
