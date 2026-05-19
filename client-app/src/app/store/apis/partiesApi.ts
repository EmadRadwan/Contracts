import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {Party} from "../../models/party/party";
import {State, toODataString} from "@progress/kendo-data-query";
import {EmployeeAdvance, EmployeeAdvanceDetail} from "../../models/humanResources/employeeAdvance";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const partiesApi = createApi({
    reducerPath: "parties",
    tagTypes: ["Parties", "EmplPositionTypes", "Employee", "Party", "Parties", "Supplier", "Contractor", "Customer", "EmployeeAdvance", "SalesRep", "Broker"],
    refetchOnMountOrArgChange: true,
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
    endpoints(builder) {
        return {
            fetchParties: builder.query<ListResponse<Party>, State>({
                query: (queryArgs) => {
                    const url = `/odata/partyRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                providesTags: ["Parties"],
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    console.log("Total Parties:", totalCount);
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            updatePartyMainRole: builder.mutation<PartyDto, { partyId: string; mainRole: string }>({
                query: ({ partyId, mainRole }) => ({
                    url: `parties/updateMainRole/${partyId}`, // adjust to your actual route
                    method: 'PUT',
                    body: { mainRole },
                }),
                invalidatesTags: ['Parties'], // assuming you use tags; otherwise it will refetch via query invalidation
            }),
            fetchCustomer: builder.query<Party, any>({
                query: (partyId) => {
                    return {
                        url: `/parties/${partyId}/getCustomer`,
                        params: partyId,
                        method: "GET",
                    };
                },
                providesTags: ['Customer']
            }),
            fetchEmployee: builder.query<Party, string>({
                query: (partyId) => ({
                    url: `/parties/${partyId}/getEmployee`,
                    method: "GET",
                }),
                providesTags: ['Employee']
                
            }),
            fetchSupplier: builder.query<Party, any>({
                query: (partyId) => {
                    return {
                        url: `/parties/${partyId}/getSupplier`,
                        params: partyId,
                        method: "GET",
                    };
                },
                providesTags: ['Supplier']
            }),
            fetchContractor: builder.query<Party, any>({
                query: (partyId) => {
                    return {
                        url: `/parties/${partyId}/getContractor`,
                        params: partyId,
                        method: "GET",
                    };
                },
                providesTags: ['Contractor']
            }),
            fetchSalesRep: builder.query<Party, any>({
                query: (partyId) => {
                    return {
                        url: `/parties/${partyId}/getSalesRep`,
                        params: partyId,
                        method: "GET",
                    };
                },
                providesTags: ['SalesRep']
            }),
            fetchBroker: builder.query<Party, any>({
                query: (partyId) => {
                    return {
                        url: `/parties/${partyId}/getBroker`,
                        params: partyId,
                        method: "GET",
                    };
                },
                providesTags: ['Broker']
            }),
            fetchCompanies: builder.query<any[], any>({
                query: () => {
                    return {
                        url: `/parties/getCompanies`,
                        method: "GET",
                    };
                },
            }),
            getPartyFinancialHistory: builder.query<PartyFinancialHistoryDetails, string>({
                query: (partyId) => ({
                    url: `parties/${partyId}/getPartyFinancialHistory`,
                    method: 'GET',
                }),
            }),
            getPartySubLedger: builder.query<PartySubLedgerResponse, string>({
                query: (partyId) => ({
                    url: `parties/subledger/${partyId}`,
                    method: 'GET',
                }),
            }),
            getEmplPositionTypes: builder.query<EmplPositionType[], void>({
                query: () => ({
                    url: '/EmplPositionTypes',
                    method: 'GET',
                }),
                providesTags: ['EmplPositionTypes'], // useful if you later invalidate on create/update
            }),
            createEmployee: builder.mutation<
                EmployeeMutationResponse,     // result type
                any                           // input type — you can tighten later (e.g. EmployeeFormValues)
                >({
                query: (employeeData) => ({
                    url: '/parties/createEmployee',
                    method: 'POST',
                    body: employeeData,
                }),

                invalidatesTags: ['Employee', 'Party', 'Parties'],
            }),

            updateEmployee: builder.mutation<
                EmployeeMutationResponse,
                any
                >({
                query: (employeeData) => ({
                    url: '/parties/updateEmployee',
                    method: 'PUT',
                    body: employeeData,
                }),

                // More precise invalidation if you know the partyId
                invalidatesTags: ['Employee', 'Party', 'Parties'],
            }),
            createCustomer: builder.mutation<any, any>({
                query: (customerData) => ({
                    url: '/parties/createCustomer',
                    method: 'POST',
                    body: customerData,
                }),
                invalidatesTags: ['Customer', 'Party', 'Parties'],
            }),
            updateCustomer: builder.mutation<any, any>({
                query: (customerData) => ({
                    url: '/parties/updateCustomer',
                    method: 'PUT',
                    body: customerData,
                }),
                invalidatesTags: ['Customer', 'Party', 'Parties'],
            }),
            createSupplier: builder.mutation<any, any>({
                query: (supplierData) => ({
                    url: '/parties/createSupplier',
                    method: 'POST',
                    body: supplierData,
                }),
                invalidatesTags: ['Supplier', 'Party', 'Parties'],
            }),
            updateSupplier: builder.mutation<any, any>({
                query: (supplierData) => ({
                    url: '/parties/updateSupplier',
                    method: 'PUT',
                    body: supplierData,
                }),
                invalidatesTags: ['Supplier', 'Party', 'Parties'],
            }),
            createContractor: builder.mutation<any, any>({
                query: (contractorData) => ({
                    url: '/parties/createContractor',
                    method: 'POST',
                    body: contractorData,
                }),
                invalidatesTags: ['Contractor', 'Party', 'Parties'],
            }),
            updateContractor: builder.mutation<any, any>({
                query: (contractorData) => ({
                    url: '/parties/updateContractor',
                    method: 'PUT',
                    body: contractorData,
                }),
                invalidatesTags: ['Contractor', 'Party', 'Parties'],
            }),
            createSalesRep: builder.mutation<any, any>({
                query: (salesRepData) => ({
                    url: '/parties/createSalesRep',
                    method: 'POST',
                    body: salesRepData,
                }),
                invalidatesTags: ['SalesRep', 'Party', 'Parties'],
            }),
            updateSalesRep: builder.mutation<any, any>({
                query: (salesRepData) => ({
                    url: '/parties/updateSalesRep',
                    method: 'PUT',
                    body: salesRepData,
                }),
                invalidatesTags: ['SalesRep', 'Party', 'Parties'],
            }),
            createBroker: builder.mutation<any, any>({
                query: (brokerData) => ({
                    url: '/parties/createBroker',
                    method: 'POST',
                    body: brokerData,
                }),
                invalidatesTags: ['Broker', 'Party', 'Parties'],
            }),
            updateBroker: builder.mutation<any, any>({
                query: (brokerData) => ({
                    url: '/parties/updateBroker',
                    method: 'PUT',
                    body: brokerData,
                }),
                invalidatesTags: ['Broker', 'Party', 'Parties'],
            }),
            fetchEmployeeAdvances: builder.query<ListResponse<EmployeeAdvance>, State>({
                query: (queryArgs) => {
                    const url = `/odata/employeeAdvanceRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                providesTags: ['EmployeeAdvance'],
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    console.log("Total Employee Advances:", totalCount);
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            createEmployeeAdvance: builder.mutation<EmployeeAdvance, Partial<EmployeeAdvance>>({
                query: (advance) => ({
                    url: '/humanResources/createEmployeeAdvance',
                    method: 'POST',
                    body: advance,
                }),
                invalidatesTags: ['EmployeeAdvance'],
            }),
            updateEmployeeAdvance: builder.mutation<EmployeeAdvance, Partial<EmployeeAdvance> & { advanceId: string | number }>({
                query: (advance) => ({
                    url: '/humanResources/updateEmployeeAdvance',
                    method: 'PUT',
                    body: advance,
                }),
                invalidatesTags: ['EmployeeAdvance'],
            }),
            deleteEmployeeAdvance: builder.mutation<void, string>({
                query: (advanceId) => ({
                    url: `/humanResources/${advanceId}`,
                    method: 'DELETE',
                }),
                invalidatesTags: ['EmployeeAdvance'],
            }),
            getEmployeeAdvanceDetail: builder.query<EmployeeAdvanceDetail, string>({
                query: (advanceId) => `humanResources/${advanceId}`,
            }),
            fetchEmployeeAdvancesByDateRange: builder.query<EmployeeAdvancesResponse, { fromDate: string, toDate: string }>({
                query: ({ fromDate, toDate }) => `/humanResources/getEmployeeAdvancesByDateRange?fromDate=${fromDate}&toDate=${toDate}`,
            }),
            fetchRolesTypes: builder.query<any[], { searchTerm?: string }>({
                query: (queryArgs) => ({
                    url: '/parties/listRoleTypes',
                    method: 'GET',
                    params: queryArgs,
                }),
                providesTags: ['RoleType'],
            }),
            fetchPartyRoles: builder.query<any[], string>({
                query: (partyId) => `/parties/${partyId}/listPartyRoles`,
                providesTags: ['PartyRole'],
            }),
            addPartyRole: builder.mutation<void, { partyId: string, roleTypeId: string }>({
                query: (data) => ({
                    url: '/parties/addPartyRole',
                    method: 'POST',
                    body: data,
                }),
                invalidatesTags: ['PartyRole'],
            }),
            deletePartyRole: builder.mutation<void, { partyId: string, roleTypeId: string }>({
                query: ({ partyId, roleTypeId }) => ({
                    url: `/parties/deletePartyRole?partyId=${partyId}&roleTypeId=${roleTypeId}`,
                    method: 'DELETE',
                }),
                invalidatesTags: ['PartyRole'],
            }),
            deleteParty: builder.mutation<void, string>({
                query: (partyId) => ({
                    url: `/parties/deleteParty/${partyId}`,
                    method: 'DELETE',
                }),
                invalidatesTags: ['Parties'],
            }),
            fetchAllPartiesForReport: builder.query<any[], void>({
                query: () => ({
                    url: '/parties/listAllParties',
                    method: 'GET',
                }),
            }),
            fetchEmployeesWithSalary: builder.query<ListResponse<any>, void>({
                query: () => ({
                    url: '/parties/listEmployeesWithSalary',
                    method: 'GET',
                }),
                transformResponse: (response: any) => {
                    return {
                        data: response,
                        total: response.length,
                    };
                },
            }),
            fetchDepartmentsLov: builder.query<any, any>({
                query: (params) => ({
                    url: '/parties/getDepartmentsLov',
                    method: 'GET',
                    params: params,
                }),
            }),
            fetchPayrollAdvances: builder.query<EmployeeAdvancesResponse, { invoiceDate: string; organizationPartyId: string }>({
                query: ({ invoiceDate, organizationPartyId }) => `/humanResources/listPayrollAdvances?invoiceDate=${invoiceDate}&organizationPartyId=${organizationPartyId}`,
                providesTags: ['EmployeeAdvance'],
            }),
        };
    },
});

export interface EmployeeAdvancesResponse {
    data: EmployeeAdvance[];
    total: number;
}

export const {
    useFetchPartiesQuery,
    useFetchCustomerQuery,
    useFetchSupplierQuery,
    useFetchCompaniesQuery, useGetPartyFinancialHistoryQuery,
    useFetchContractorQuery, useFetchEmployeeQuery,
    useUpdatePartyMainRoleMutation,
    useGetEmplPositionTypesQuery, useCreateEmployeeMutation,
    useUpdateEmployeeMutation, useCreateCustomerMutation, useUpdateCustomerMutation,
    useCreateSupplierMutation, useUpdateSupplierMutation, useCreateContractorMutation,
    useUpdateContractorMutation, useGetPartySubLedgerQuery, useFetchEmployeeAdvancesQuery,
    useCreateSalesRepMutation, useUpdateSalesRepMutation, useFetchSalesRepQuery,
    useCreateBrokerMutation, useUpdateBrokerMutation, useFetchBrokerQuery,
    useCreateEmployeeAdvanceMutation,
    useUpdateEmployeeAdvanceMutation, useDeleteEmployeeAdvanceMutation, useLazyGetEmployeeAdvanceDetailQuery,
    useLazyFetchEmployeeAdvancesByDateRangeQuery,
    useFetchRolesTypesQuery, useFetchPartyRolesQuery, useAddPartyRoleMutation, useDeletePartyRoleMutation,
    useDeletePartyMutation,
    useLazyFetchAllPartiesForReportQuery,
    useFetchEmployeesWithSalaryQuery,
    useFetchPayrollAdvancesQuery,
    useFetchDepartmentsLovQuery,
} = partiesApi;
export {partiesApi};
