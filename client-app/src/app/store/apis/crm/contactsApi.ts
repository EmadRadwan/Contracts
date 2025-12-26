import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../../configureStore";
import { Contact, ContactLov, ContactQueryParams } from "../../../../features/CRM/models/contact";

/**
 * RTK Query API for CRM Contacts (People).
 */
const contactsApi = createApi({
    reducerPath: "contacts",
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
    tagTypes: ["Contact"],

    endpoints(builder) {
        return {
            // Fetch all contacts with optional filtering
            fetchContacts: builder.query<Contact[], ContactQueryParams | void>({
                query: (params) => {
                    const searchParams = new URLSearchParams();
                    if (params?.search) searchParams.append('search', params.search);
                    if (params?.dataSourceId) searchParams.append('dataSourceId', params.dataSourceId);
                    if (params?.sortBy) searchParams.append('sortBy', params.sortBy);
                    if (params?.sortDesc !== undefined) searchParams.append('sortDesc', String(params.sortDesc));

                    return {
                        url: `/contacts?${searchParams.toString()}`,
                        method: "GET",
                    };
                },
                providesTags: (result) =>
                    result
                        ? [
                            ...result.map(({ partyId }) => ({
                                type: "Contact" as const,
                                id: partyId,
                            })),
                            { type: "Contact", id: "LIST" },
                        ]
                        : [{ type: "Contact", id: "LIST" }],
            }),

            // Fetch contacts for LOV/picker (lightweight)
            fetchContactsLov: builder.query<ContactLov[], { search?: string; take?: number } | void>({
                query: (params) => {
                    const searchParams = new URLSearchParams();
                    if (params?.search) searchParams.append('search', params.search);
                    if (params?.take) searchParams.append('take', String(params.take));

                    return {
                        url: `/contacts/lov?${searchParams.toString()}`,
                        method: "GET",
                    };
                },
                providesTags: [{ type: "Contact", id: "LOV" }],
            }),

            // Create a new contact
            createContact: builder.mutation<Contact, Contact>({
                query: (contact) => ({
                    url: `/contacts`,
                    method: "POST",
                    body: contact,
                }),
                invalidatesTags: [{ type: "Contact", id: "LIST" }, { type: "Contact", id: "LOV" }],
            }),

            // Update an existing contact
            updateContact: builder.mutation<Contact, { id: string; contact: Contact }>({
                query: ({ id, contact }) => ({
                    url: `/contacts/${id}`,
                    method: "PUT",
                    body: contact,
                }),
                invalidatesTags: (result, error, { id }) => [
                    { type: "Contact", id },
                    { type: "Contact", id: "LIST" },
                    { type: "Contact", id: "LOV" },
                ],
            }),
        };
    },
});

export const {
    useFetchContactsQuery,
    useFetchContactsLovQuery,
    useCreateContactMutation,
    useUpdateContactMutation,
} = contactsApi;

export { contactsApi };
