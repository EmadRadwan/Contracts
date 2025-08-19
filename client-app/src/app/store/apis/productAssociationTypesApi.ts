import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {ProductAssociationType} from "../../models/product/productAssociationType";
import {Product} from "../../models/product/product";

const productAssociationTypesApi = createApi({
    reducerPath: "productAssociationTypes",
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
            fetchProductAssociationTypes: builder.query<ProductAssociationType[],
                undefined>({
                query: () => {
                    return {
                        url: "/productAssociationTypes",
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    return response as ProductAssociationType[];
                },
            }),
        };
    },
});

export const {useFetchProductAssociationTypesQuery} =
    productAssociationTypesApi;
export {productAssociationTypesApi};


interface ProductAssociation {
    productId: string;
    productIdTo: string;
    productAssocTypeId: string;
    productAssocTypeDescription: string;
    fromDate: string | null;
    thruDate: string | null;
    reason: string | null;
    quantity: number | null;
    sequenceNum: number | null;
}
