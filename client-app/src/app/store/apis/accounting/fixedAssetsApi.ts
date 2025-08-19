import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {State, toODataString} from "@progress/kendo-data-query";
import {FixedAsset} from "../../../models/accounting/fixedAsset";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const fixedAssetsApi = createApi({
    reducerPath: "fixedAssets",
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
            fetchFixedAssets: builder.query<ListResponse<FixedAsset>, State>({
                query: (queryArgs) => {
                    const url = `/odata/fixedAssetRecords?$count=true&${toODataString(queryArgs)}`;
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
            fetchFixedAssetTypes: builder.query({
                query: () => {
                    return {
                        url: "/fixedAssetTypes",
                        method: "GET"
                    }
                },
                transformResponse: (response: any, meta, arg) => {
                    return response.map((type: any) => ({
                       ... type,
                       cleanName: type.fixedAssetTypeId.toLowerCase().split("_").map((part: string) => {
                        return `${part[0].toUpperCase()}${part.slice(1)}`
                       }).join(" ")
                    }))
                }
            }),
            fetchFixedAssetStandardCosts: builder.query({
                query: (fixedAssetId) => {
                    return {
                        url: `/fixedAssetStdCosts/${fixedAssetId}/listFixedAssetStdCosts`,
                        params: fixedAssetId,
                        method: "GET"
                    }
                },
                transformResponse: (response: any, meta, arg) => {
                    return response.map((cost: any) => ({
                       ... cost,
                       cleanAssetName: cost.fixedAssetId.toLowerCase().split("_").map((part: string) => {
                        return `${part[0].toUpperCase()}${part.slice(1)}`
                       }).join(" "),
                       cleanCostName: cost.fixedAssetStdCostTypeId.toLowerCase().split("_").map((part: string) => {
                        return `${part[0].toUpperCase()}${part.slice(1)}`
                       }).join(" ") 
                    }))
                    // return response
                }
            }),
            fetchFixedAssetStandardCostTypes: builder.query<any[], any>({
                query: () => {
                    return {
                        url: "/fixedAssetStdCostTypes",
                        method: "GET"
                    }
                }
            })
        };
    },
});

export const {
    useFetchFixedAssetsQuery,
    useFetchFixedAssetTypesQuery,
    useFetchFixedAssetStandardCostsQuery,
    useFetchFixedAssetStandardCostTypesQuery
} = fixedAssetsApi;
export {fixedAssetsApi};
