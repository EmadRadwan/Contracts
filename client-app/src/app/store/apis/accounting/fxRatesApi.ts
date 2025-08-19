import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";

const fxRatesApi = createApi({
    reducerPath: "fxRates",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (lang) {
                headers.set("Accept-Language", lang)
            }
            return headers;
        },
    }),

    endpoints (builder) {
        return {
            fetchFXRates: builder.query<any[], any>({
                query: () => {
                    return {
                        url: "/uoms/conversionDated",
                        method: "GET"
                    }
                }
            }),
            fetchCurrencies: builder.query<any[], any>({
                query: () => {
                    return {
                        url: "/uoms/currency",
                        method: "GET"
                    }
                }
            }),
            fetchPurchaseCostCurrencies: builder.query<any[], any>({
                query: () => {
                    return {
                        url: "/uoms/currency",
                        method: "GET"
                    }
                },
                transformResponse: (response: any, meta, arg) => {
                    return response.map((currency: any) => ({
                        purchaseCostUomId: currency.currencyUomId,
                        description: currency.description,
                    }));
                },
            }),
            fetchCompanyBaseCurrency: builder.query<any, any>({
                query: () => {
                    return {
                        url: "/organizationGl/getBaseCurrencyId",
                        method: "GET"
                    }
                }
            })
        }
    }
})

export const {useFetchFXRatesQuery, useFetchCurrenciesQuery, useFetchPurchaseCostCurrenciesQuery, useFetchCompanyBaseCurrencyQuery} = fxRatesApi
export {fxRatesApi}