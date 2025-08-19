import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {FacilityType} from "../../../models/facility/facilityType";

const facilityTypesApi = createApi({
    reducerPath: "facilityTypes",
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
            fetchFacilityTypes: builder.query<FacilityType[], undefined>({
                query: () => {
                    return {
                        url: "/facilityTypes",
                        method: "GET",
                    };
                },
                transformResponse: (response: any, meta) => {
                    return response as FacilityType[];
                },
            }),
        };
    },
});

export const {useFetchFacilityTypesQuery} = facilityTypesApi;
export {facilityTypesApi};
