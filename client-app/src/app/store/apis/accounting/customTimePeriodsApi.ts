import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {State, toODataString} from "@progress/kendo-data-query";
import {CustomTimePeriod} from "../../../models/accounting/customTimePeriod";
import { PeriodType } from "../../../models/accounting/periodType";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const customTimePeriodsApi = createApi({
    reducerPath: "customTimePeriods",
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
    tagTypes: ["TimePeriods", "TimePeriodsLov"],
    endpoints(builder) {
        return {
            fetchCustomTimePeriods: builder.query<ListResponse<CustomTimePeriod>, State>({
                query: (queryArgs) => {
                    const url = `/odata/customTimePeriodRecords?count=true&${toODataString(queryArgs)}`
                    return {url, method: "GET"}
                },
                providesTags: ["TimePeriods"],
                transformResponse: (response: any, meta: any) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                }
            }),
            fetchCustomTimePeriodsLov: builder.query<CustomTimePeriod[], any>({
                query: () => {
                    return {
                        url: `/customTimePeriods/listCustomTimePeriodsLov`, 
                        method: "GET"
                    }
                },
                providesTags: ["TimePeriodsLov"],
                transformResponse: (res: any, meta: any, arg: any) => {
                    return res.map((r: CustomTimePeriod) => {
                        return {
                            ...r,
                            description: `[${r.customTimePeriodId}] - ${new Date(r.fromDate).toLocaleDateString('en-GB')} - ${new Date(r.thruDate).toLocaleDateString('en-GB')}`
                        }
                    })
                }
            }),
            fetchTimePeriodTypesLov: builder.query<PeriodType[], any>({
                query: () => {
                    return {
                        url: `/customTimePeriods/listCustomTimePeriodTypesLov`, 
                        method: "GET"
                    }
                },
            }),
            closeTimePeriod: builder.mutation({
                query: (customTimePeriodId) => {
                    return {
                        url: `/customTimePeriods/${customTimePeriodId}/closeTimePeriod`,
                        method: "POST"
                    }
                },
                invalidatesTags: ["TimePeriods", "TimePeriodsLov"]
            })
        }
    }
})

export const {useFetchCustomTimePeriodsQuery, useFetchCustomTimePeriodsLovQuery, useFetchTimePeriodTypesLovQuery, useCloseTimePeriodMutation} = customTimePeriodsApi
export {customTimePeriodsApi}