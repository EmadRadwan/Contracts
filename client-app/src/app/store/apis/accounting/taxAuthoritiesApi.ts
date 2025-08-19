import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {State, toODataString} from "@progress/kendo-data-query";
import {TaxAuthority} from "../../../models/accounting/taxAuthority";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const taxAuthoritiesApi = createApi({
    reducerPath: "taxAuthorities",
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
            fetchTaxAuthorities: builder.query<ListResponse<TaxAuthority>, State>({
                query: (queryArgs) => {
                    const url = `/odata/taxAuthorityRecords?$count=true&${toODataString(queryArgs)}`;
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
            fetchTaxAuthoritiesList: builder.query<any[], any>({
                query: () => {
                    return {
                        url: "/taxAuthorities/getTaxAuthorities",
                        method: "GET"
                    }
                }
            }),
            fetchTaxAuthorityProductsList: builder.query<any[], any>({
                query: ({taxAuthGeoId, taxAuthPartyId}) => {
                    return {
                        url: `/taxes/${taxAuthGeoId}/${taxAuthPartyId}/getTaxAuthorityRateProducts`,
                        method: "GET"
                    }
                }
            }),
            fetchTaxAuthorityCategoriesList: builder.query<any[], any>({
                query: ({taxAuthGeoId, taxAuthPartyId}) => {
                    return {
                        url: `/taxes/${taxAuthGeoId}/${taxAuthPartyId}/getTaxAuthorityCategories`,
                        method: "GET"
                    }
                }
            })
        };
    },
});

export const {useFetchTaxAuthoritiesQuery, useFetchTaxAuthoritiesListQuery, useFetchTaxAuthorityCategoriesListQuery, useFetchTaxAuthorityProductsListQuery} = taxAuthoritiesApi;
export {taxAuthoritiesApi};
