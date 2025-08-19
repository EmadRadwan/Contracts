import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {Product, ServiceProductPriceParams,} from "../../models/product/product";
import {State, toODataString} from "@progress/kendo-data-query";
import {Quantity} from "../../models/common/quantity";
import {Currency} from "../../models/common/currency";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const productsApi = createApi({
    reducerPath: "products",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            const lang = store.getState().localization.language ;
            if (lang) {
                headers.set("Accept-Language", `${lang}`);
            }
            return headers;
        },
    }),
    tagTypes: ["Products", "ProductSuppliers", "Currencies", "Quantities"],
    endpoints(builder) {
        return {
            fetchProducts: builder.query<ListResponse<Product>,
                State>({
                query: (queryArgs) => {
                    const url = `/odata/productRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                providesTags: ["Products"],
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    console.log("response", response);
                    console.log("totalCount", totalCount);
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            fetchProductUOMs: builder.query<string, any>({
                query: () => {
                    return {
                        url: "/uoms/quantity",
                        method: "GET",
                    };
                },
            }),
            getServiceProductPrice: builder.query<string, ServiceProductPriceParams>({
                query: (queryArgs) => {
                    return {
                        url: "/products/getServiceProductPrice",
                        params: queryArgs,
                        method: "GET",
                    };
                },
            }),
            addProduct: builder.mutation({
                query: (product) => {
                    return {
                        url: "/products/createProduct",
                        method: "POST",
                        body: {...product},
                    };
                },
                invalidatesTags: ["Products"]
            }),
            updateProduct: builder.mutation({
                query: (product) => {
                    return {
                        url: "/products/updateProduct",
                        method: "PUT",
                        body: {...product},
                    };
                },
                invalidatesTags: ["Products"]
            }),
            fetchProductStoreFacilities: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/productStores",
                        method: "GET",
                    };
                },
            }),
            fetchProductQuantityUom: builder.query<any, { productId: string }>({
                query: ({ productId }) => `/products/${productId}/getProductQuantityUom`,
                transformResponse: (response: { description: string }) => response.description,
            }),
            fetchFinishedProductsForWIP: builder.query<any, any>({
                query: (productId) => {
                    return {
                        url: `/products/${productId}/getFinishedProductsForWIP`,
                        method: "GET",
                    };
                },
                transformResponse: (response: any[]) => response.map(item => ({
                    productId: item.productId,
                    productName: item.productName,
                    wipPerUnit: item.quantity // Map QUANTITY
                })),
            }),
            getProductPrice: builder.query<ProductPriceDto, string>({
                query: (productId) => `/products/getProductPrice/${productId}`,
            }),
            getProductDetails: builder.query<ProductDetails, string>({
                query: (productId) => `products/getProductDetails/${productId}`,
            }),
            getProductSuppliers: builder.query<any[], string>({
                // REFACTOR: Define query for fetching product suppliers by productId
                // Maps to the GET endpoint and transforms the Result wrapper to extract the Value
                query: (productId) => `productSuppliers/${productId}`,
                providesTags: ["ProductSuppliers"]
                    
            }),
            createProductSupplier: builder.mutation<SupplierProductDto, SupplierProductCreateDto>({
                query: (dto) => ({
                    url: 'productSuppliers',
                    method: 'POST',
                    body: dto,
                }),
                invalidatesTags: ["ProductSuppliers"]
            }),
            updateProductSupplier: builder.mutation<SupplierProductDto, SupplierProductUpdateDto>({
                // REFACTOR: Fixed request body to match backend expectation
                // Sends DTO wrapped in { SupplierProduct: dto } for update endpoint
                query: (dto) => ({
                    url: 'productSuppliers/updateProductSupplier',
                    method: 'PUT',
                    body: dto,
                }),
                invalidatesTags: ["ProductSuppliers"]
            }),
            getCurrencies: builder.query<Currency[], string>({
                // REFACTOR: Updated to match /api/uoms/currency endpoint with language parameter
                // Fetches currency data for dropdown, respecting user language
                query: () => `uoms/currency`,
            }),
            getQuantities: builder.query<Quantity[], string>({
                // REFACTOR: Updated to match /api/uoms/quantity endpoint with language parameter
                // Fetches quantity UOM data for dropdown, respecting user language
                query: () => `uoms/quantity`,
            }),
            fetchInventoryItemColors: builder.query<InventoryItemColor[], string>({
                query: (productId) => `products/${productId}/getInventoryItemColors`,
                transformResponse: (response: any) => {
                    // Assuming response is an array of { colorId, colorName } from inventoryItemFeatures
                    return response.map((item: any) => ({
                        colorId: item.colorId,
                        colorName: item.colorName,
                    }));
                },
            }),
        };
    },
});

export const {
    useFetchProductsQuery,
    useFetchProductUOMsQuery,
    useAddProductMutation,
    useUpdateProductMutation,
    useFetchProductStoreFacilitiesQuery,
    useGetServiceProductPriceQuery, useFetchFinishedProductsForWIPQuery,
    endpoints: productsEndpoints, useFetchProductQuantityUomQuery,
    useGetProductPriceQuery, useGetProductDetailsQuery, 
    useGetProductSuppliersQuery, useUpdateProductSupplierMutation,
    useCreateProductSupplierMutation, useGetCurrenciesQuery, useGetQuantitiesQuery, useFetchInventoryItemColorsQuery
} = productsApi;
export {productsApi};

interface SupplierProductDto {
    FromPartyId: OrderPartyDto;
    CurrencyUomDescription: string;
    QuantityUomDescription: string | null;
    PartyName: string;
    AvailableFromDate: string;
    AvailableThruDate: string | null;
    LastPrice: number | null;
}

interface SupplierProductCreateDto {
    ProductId: string;
    PartyId: string;
    CurrencyUomId: string;
    MinimumOrderQuantity: number;
    AvailableFromDate: string;
    AvailableThruDate: string | null;
    LastPrice: number | null;
    QuantityUomId: string | null;
}

interface SupplierProductUpdateDto {
    ProductId: string;
    PartyId: string;
    CurrencyUomId: string;
    MinimumOrderQuantity: number;
    AvailableFromDate: string;
    AvailableThruDate: string | null;
    LastPrice: number | null;
    QuantityUomId: string | null;
}
