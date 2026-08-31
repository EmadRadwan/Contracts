import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {State, toODataString} from "@progress/kendo-data-query";
import {store} from "../../configureStore";
import {AuditActivity} from "../../../models/auditing/auditActivity";
import {EntityAuditLog} from "../../../models/auditing/entityAuditLog";

interface ListResponse<T> {
    data: T[];
    total: number;
}

// OData string literals are single-quoted; an embedded quote is escaped by doubling it.
const odataLiteral = (value: string) => `'${value.replace(/'/g, "''")}'`;

const auditApi = createApi({
    reducerPath: "audit",
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
    endpoints(builder) {
        return {
            // The Audit Trail grid: who ran which command, and how it went.
            fetchAuditActivities: builder.query<ListResponse<AuditActivity>, State>({
                query: (queryArgs) => ({
                    url: `/odata/auditActivityRecords?count=true&${toODataString(queryArgs)}`,
                    method: "GET",
                }),
                transformResponse: (response: any, meta) => {
                    const {totalCount} = JSON.parse(meta!.response!.headers.get("count")!);
                    return {data: response, total: totalCount};
                },
            }),

            // The field-level change log, unfiltered — the companion grid to the one above.
            fetchEntityAuditLogs: builder.query<ListResponse<EntityAuditLog>, State>({
                query: (queryArgs) => ({
                    url: `/odata/entityAuditLogRecords?count=true&${toODataString(queryArgs)}`,
                    method: "GET",
                }),
                transformResponse: (response: any, meta) => {
                    const {totalCount} = JSON.parse(meta!.response!.headers.get("count")!);
                    return {data: response, total: totalCount};
                },
            }),

            // Everything one action changed. Used to expand a row in the Audit Trail grid.
            fetchChangesByCorrelation: builder.query<EntityAuditLog[], string>({
                query: (correlationId) => ({
                    url: `/odata/entityAuditLogRecords?$filter=changedSessionInfo eq ${odataLiteral(correlationId)}`,
                    method: "GET",
                }),
            }),

            // The History tab: every recorded change to one business record.
            // Covered by the ENTITY_AUDIT_LOG_RECORD index (entity + pk + date).
            fetchRecordHistory: builder.query<EntityAuditLog[], { entityName: string; pkText: string }>({
                query: ({entityName, pkText}) => {
                    const filter =
                        `changedEntityName eq ${odataLiteral(entityName)}` +
                        ` and pkCombinedValueText eq ${odataLiteral(pkText)}`;
                    return {
                        url: `/odata/entityAuditLogRecords?$filter=${encodeURIComponent(filter)}&$orderby=changedDate desc`,
                        method: "GET",
                    };
                },
            }),
        };
    },
});

export const {
    useFetchAuditActivitiesQuery,
    useFetchEntityAuditLogsQuery,
    useFetchChangesByCorrelationQuery,
    useFetchRecordHistoryQuery,
} = auditApi;

export {auditApi};
