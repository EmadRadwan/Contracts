import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";

const finAccountStatusApi = createApi({
    reducerPath: "finAccountStatus",
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
            fetchFinAccountStatuses: builder.query<any[], any>({
                query: () => {
                    return {
                        url: "/finAccountStatus",
                        method: "GET"
                    }
                }
            })
        }
    }
})

export const { useFetchFinAccountStatusesQuery } = finAccountStatusApi
export {finAccountStatusApi}