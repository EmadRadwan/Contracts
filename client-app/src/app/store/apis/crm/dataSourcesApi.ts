import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";

export interface DataSource {
    dataSourceId: string;
    description: string;
}

/**
 * RTK Query API for Data Sources (Lead Sources).
 */
const dataSourcesApi = createApi({
    reducerPath: "dataSources",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers) => {
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),
    tagTypes: ["DataSource"],

    endpoints(builder) {
        return {
            // Fetch all data sources
            fetchDataSources: builder.query<DataSource[], void>({
                query: () => ({
                    url: `/datasources`,
                    method: "GET",
                }),
                providesTags: [{ type: "DataSource", id: "LIST" }],
            }),
        };
    },
});

export const { useFetchDataSourcesQuery } = dataSourcesApi;

export { dataSourcesApi };
