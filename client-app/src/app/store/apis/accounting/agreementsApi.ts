import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";
import { State, toODataString } from "@progress/kendo-data-query";
import { Agreement } from "../../../models/accounting/agreement";
import { AgreementItem } from "../../../models/accounting/agreementItem";
import { AgreementTerm } from "../../../models/accounting/agreementTerm";
import { AgreementLov } from "../../../models/accounting/agreementLov";

interface ListResponse<T> {
  data: T[];
  total: number;
}

const agreementsApi = createApi({
  reducerPath: "agreements",
  baseQuery: fetchBaseQuery({
    baseUrl: import.meta.env.VITE_API_URL,
    prepareHeaders: (headers, { getState }) => {
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
  tagTypes: ["language"],
  endpoints(builder) {
    return {
      fetchAgreements: builder.query<ListResponse<Agreement>, State>({
        query: (queryArgs) => {
          const url = `/odata/agreementRecords?$count=true&${toODataString(
            queryArgs
          )}`;
          return { url, method: "GET" };
        },
        transformResponse: (response: any, meta, arg) => {
          const { totalCount } = JSON.parse(
            meta!.response!.headers.get("count")!
          );
          return {
            data: response,
            total: totalCount,
          };
        },
        providesTags: ["language"]
      }),
      fetchAgreementTypes: builder.query<any, any>({
        query: () => {
          return {
            url: "/agreementTypes",
            method: "GET",
          };
        },
        providesTags: ["language"]
      }),
      fetchAgreementItems: builder.query<AgreementItem[], string>({
        query: (agreementId) => {
          return {
            url: `/agreements/${agreementId}/getAgreementItems`,
            method: "GET",
          };
        },
      }),
      fetchAgreementTerms: builder.query<AgreementTerm[], string>({
        query: (agreementId) => {
          return {
            url: `/agreements/${agreementId}/getAgreementTerms`,
            method: "GET",
          };
        },
      }),
      fetchAgreementsByPartyId: builder.query<
        AgreementLov[],
        { partyId: string; orderType: string }
      >({
        query: ({ partyId, orderType }) => {
          return {
            url: `/agreements/${partyId}/${orderType}/getAgreementsByPartyId`,
            method: "GET",
          };
        },
      }),
    };
  },
});

export const {
  useFetchAgreementsQuery,
  useFetchAgreementTypesQuery,
  useFetchAgreementItemsQuery,
  useFetchAgreementTermsQuery,
  useFetchAgreementsByPartyIdQuery
} = agreementsApi;
export { agreementsApi };
