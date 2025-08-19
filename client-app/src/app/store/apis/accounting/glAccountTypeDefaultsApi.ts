import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";

const glAccountTypeDefaultsApi = createApi({
    reducerPath: "glAccountTypeDefaults",
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
            fetchGlAccountTypes: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/organizationGl/getGlAccountTypes`,
                        method: "GET",
                    };
                },
            }),
            fetchGlAccountOrganizationAndClass: builder.query<any[], any>({
                query: (companyId) => {
                    return {
                        url: `/organizationGl/${companyId}/getGlAccountOrganizationAndClass`,
                        method: "GET",
                    };
                },
            }),
            fetchGlAccountTypeDefaults: builder.query<any[], any>({
                query: (companyId) => {
                    return {
                        url: `/organizationGl/${companyId}/getGlAccountTypeDefaults`,
                        method: "GET",
                    };
                },
            }),
        };
    },
});

export const {
    useFetchGlAccountTypesQuery,
    useFetchGlAccountOrganizationAndClassQuery,
    useFetchGlAccountTypeDefaultsQuery
} = glAccountTypeDefaultsApi;
export {glAccountTypeDefaultsApi};
