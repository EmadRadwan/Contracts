import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {PartyAcctgPreference} from "../../../models/accounting/partyAcctgPreference";

const orgGlSettingsApi = createApi({
    reducerPath: "orgGlSettings",
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
            fetchOrgGlSettings: builder.query<PartyAcctgPreference, any>({
                query: (companyId) => {
                    // console.log("queryArgs order item", orderId)
                    return {
                        url: `/organizationGl/${companyId}/getPartyAccountingPreferences`,
                        params: companyId,
                        method: "GET",
                    };
                },
            }),
            fetchInventoryValuationReport: builder.query<any, {
                organizationPartyId?: string,
                productId?: string,
                facilityId?: string,
                dateThru?: string
            }>({
                query: ({ organizationPartyId, productId, facilityId, dateThru }) => ({
                    url: `/organizationGlReports/getInventoryValuationReport`,
                    params: {
                        ...(organizationPartyId && { organizationPartyId }),
                        ...(productId && { productId }),
                        ...(facilityId && { facilityId }),
                        ...(dateThru && { thruDate: dateThru })
                    },
                    method: "GET"
                })
            })
        };
    },
});

export const {
    useFetchOrgGlSettingsQuery,
    useFetchInventoryValuationReportQuery

} = orgGlSettingsApi;
export {orgGlSettingsApi};
