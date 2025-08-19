import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {State, toODataString} from '@progress/kendo-data-query';
import { store } from "../../configureStore";

const paymentGroupsApi = createApi({
    reducerPath: "paymentGroups",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            const lang = store.getState().localization.language ;
            if (lang) {
                headers.set("Accept-Language", `${lang}`);
            }
            return headers;
        },
    }),
    tagTypes: ["PaymentGroups", "PaymentGroupMembers"],
    endpoints(builder) {
        return {
            fetchPaymentGroups: builder.query<ListResponse<any>, State>({
                providesTags: ["PaymentGroups"],
                query: (queryArgs) => {
                    const url = `/odata/paymentGroupRecords?count=true&${toODataString(queryArgs)}`
                    return {
                        url,
                        method: "GET",
                    };
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
            fetchPaymentGroupMembers: builder.query<any[], any>({
                providesTags: ["PaymentGroupMembers"],
                query: (paymentGroupId) => {
                    return {
                        url: `/paymentGroups/${paymentGroupId}/getPaymentGroupMembers`,
                        method: "GET",
                    };
                }
            }),
            addPaymentGroup: builder.mutation({
                query: (paymentGroup) => {
                    return {
                        url: "/paymentGroups/createPaymentGroup",
                        method: "POST",
                        body: {...paymentGroup},
                    };
                },
                invalidatesTags: ["PaymentGroups"],
            }),
            addPaymentGroupMember: builder.mutation({
                query: (paymentGroupMember) => {
                    return {
                        url: "/paymentGroups/createPaymentGroupPayment",
                        method: "POST",
                        body: {...paymentGroupMember},
                    };
                },
                invalidatesTags: ["PaymentGroupMembers", "PaymentGroups"],
            }),
            updatePaymentGroupMember: builder.mutation({
                query: (paymentGroupMember) => {
                    return {
                        url: "/paymentGroups/updatePaymentGroupMember",
                        method: "PUT",
                        body: {...paymentGroupMember},
                    };
                },
                invalidatesTags: ["PaymentGroupMembers", "PaymentGroups"],
            }),
            cancelCheckRun: builder.mutation({
                query: (paymentGroupId: string) => {
                    return {
                        url: `/paymentGroups/${paymentGroupId}/cancelCheckRun`,
                        method: "POST",
                    };
                },
                invalidatesTags: ["PaymentGroupMembers", "PaymentGroups"],
            }),
            expirePaymentGroupMember: builder.mutation({
                query: (paymentGroupMember: any) => {
                    return {
                        url: `/paymentGroups/expirePaymentGroupMember`,
                        method: "POST",
                        body: {...paymentGroupMember},
                    };
                },
                invalidatesTags: ["PaymentGroupMembers", "PaymentGroups"],
            }),
        }
    }
})

export const {
    useFetchPaymentGroupsQuery,
    useFetchPaymentGroupMembersQuery,
    useLazyFetchPaymentGroupMembersQuery,
    useAddPaymentGroupMutation,
    useAddPaymentGroupMemberMutation,
    useUpdatePaymentGroupMemberMutation,
    useCancelCheckRunMutation,
    useExpirePaymentGroupMemberMutation,
} = paymentGroupsApi
export {paymentGroupsApi}