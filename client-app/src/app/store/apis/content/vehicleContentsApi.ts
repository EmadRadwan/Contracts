import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {VehicleContent} from "../../../models/content/vehicleContent";

const vehicleContentsApi = createApi({
    reducerPath: "vehicleContents",
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

    tagTypes: ["VehicleContents"],
    endpoints(builder) {
        return {
            fetchVehicleContents: builder.query<VehicleContent[], any>({
                query: (vehicleId) => {
                    return {
                        url: `/Contents/${vehicleId}/getVehicleContents`,
                        params: vehicleId,
                        method: "GET",
                    };
                },
            }),
            createVehicleContent: builder.mutation({
                query: (vehicleContent) => {
                    return {
                        url: "/Contents/createVehicleContent",
                        method: "POST",
                        body: {...vehicleContent},
                    };
                },
                invalidatesTags: ["VehicleContents"],
            }),
        };
    },
});

export const {useFetchVehicleContentsQuery, useCreateVehicleContentMutation} =
    vehicleContentsApi;
export {vehicleContentsApi};
