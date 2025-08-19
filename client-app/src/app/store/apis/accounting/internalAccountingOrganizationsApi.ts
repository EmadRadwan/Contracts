import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {State, toODataString} from "@progress/kendo-data-query";
import {InternalAccountingOrganization} from "../../../models/accounting/internalAccountingOrganization";

interface ListResponse<T> {
    data: T[];
    total: number;
}

interface GlAccountDiagramResult {
    diagram: string;
}

const internalAccountingOrganizationsApi = createApi({
    reducerPath: "internalAccountingOrganizations",
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
            fetchInternalAccountingOrganizations: builder.query<ListResponse<InternalAccountingOrganization>, State>({
                query: (queryArgs) => {
                    const url = `/odata/internalAccountingOrganizationRecords?$count=true&${toODataString(queryArgs)}`;
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
            fetchInternalAccountingOrganizationsLov: builder.query({
                query: () => {
                    return {
                        url: "/internalAccountingOrganizations",
                        method: "GET"
                    }
                }
            }),
            getGlAccountDiagram: builder.query<GlAccountDiagramResult, string>({
                query: (acctgTransId) => ({
                    url: `/internalAccountingOrganizations/diagram/${acctgTransId}`,
                    method: "GET",
                }),
            }),
        };
    },
});

export const {useFetchInternalAccountingOrganizationsQuery, useFetchInternalAccountingOrganizationsLovQuery, useGetGlAccountDiagramQuery} = internalAccountingOrganizationsApi;
export {internalAccountingOrganizationsApi};
