import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {RoleType} from "../../models/common/roleType";

const roleTypesApi = createApi({
    reducerPath: "roleTypes",
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
            fetchRoleTypes: builder.query<RoleType[], undefined>({
                query: () => {
                    return {
                        url: "/roleTypes",
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    return response as RoleType[];
                },
            }),
        };
    },
});

export const {useFetchRoleTypesQuery} = roleTypesApi;
export {roleTypesApi};
