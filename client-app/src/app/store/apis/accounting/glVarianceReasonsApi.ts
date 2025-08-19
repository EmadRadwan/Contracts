import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";

const glVarianceReasonsApi = createApi({
    reducerPath: "glVarianceReasons",
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
    endpoints (builder) {
        return {
            fetchVarianceReasons: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/varianceReasons`,
                        method: "GET",
                    };
                },
            }),
            fetchVarianceReasonGlAccounts: builder.query<any[], any>({
                query: (companyId) => {
                    return {
                        url: `/organizationGl/${companyId}/getVarianceReasonGlAccounts`,
                        method: "GET",
                    };
                },
            }),
        }
    }
})

export const {
    useFetchVarianceReasonsQuery,
    useFetchVarianceReasonGlAccountsQuery
} = glVarianceReasonsApi

export {glVarianceReasonsApi}