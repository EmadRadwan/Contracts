import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../../configureStore";
import {State, toODataString} from "@progress/kendo-data-query";

const facilityLocationsApi = createApi({
    reducerPath: "facilityLocations",
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

    endpoints(builder) {
        return {
            fetchFacilityLocations: builder.query<any, State>({
                query: (queryArgs) => {
                    const url = `/odata/facilityLocationRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                }
            }),
            fetchFacilityLocationsLov: builder.query<any, any>({
                query: () => {
                    return {
                        url: "/facilityLocations",
                        method: "GET"
                    }
                },
                transformResponse: (response: any, meta, arg) => {
                    return response.map((r: any) => {
                        return {
                            ...r,
                            description: `Location Type: ${r.locationTypeEnumDescription} | Area: ${r.areaId} | Aisle: ${r.aisleId} | Section: ${r.sectionId} | Level: ${r.levelId} | Position: ${r.positionId}`
                        }
                    }) 
                }
            })
        }
    }
})

export const {useFetchFacilityLocationsQuery, useFetchFacilityLocationsLovQuery} = facilityLocationsApi
export {facilityLocationsApi}