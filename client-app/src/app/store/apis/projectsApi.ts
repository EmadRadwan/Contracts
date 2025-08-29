import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
import {Party} from "../../models/party/party";
import {State, toODataString} from "@progress/kendo-data-query";
import {WorkEffort} from "../../models/manufacturing/workEffort";

interface ListResponse<T> {
    data: T[];
    total: number;
}

const projectsApi = createApi({
    reducerPath: "projects",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            const lang = store.getState().localization.language;

            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            if (lang) {
                headers.set("Accept-Language", lang);
            }
            return headers;
        },
    }),

    endpoints(builder) {
        return {
            fetchProjects: builder.query<ListResponse<WorkEffort>, State>({
                query: (queryArgs) => {
                    const url = `/odata/projectRecords?$count=true&${toODataString(queryArgs)}`;
                    return { url, method: "GET" };
                },
                transformResponse: (response: any, meta, arg) => {
                    const totalCount = JSON.parse(meta!.response!.headers.get("count")!);
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
                providesTags: ["WorkEffort"],
            }),
            addProject: builder.mutation<WorkEffort, Partial<WorkEffort>>({
                query: (project) => ({
                    url: "/project/createProject",
                    method: "POST",
                    body: { ...project },
                }),
                invalidatesTags: ["WorkEffort"],
            }),
            updateProject: builder.mutation<WorkEffort, Partial<WorkEffort>>({
                query: (project) => ({
                    url: `/project/${project.WorkEffortId}`,
                    method: "PUT",
                    body: project,
                }),
                invalidatesTags: ["WorkEffort"],
            }),
            fetchProjectCertificates: builder.query<ListResponse<WorkEffort>, State>({
                query: (queryArgs) => {
                    const url = `/odata/ProjectCertificateRecords?$count=true&${toODataString(queryArgs)}`;
                    return {
                        url,
                        method: "GET",
                    };
                },
                providesTags: ["ProjectCertificates"],
                transformResponse: (response: any, meta, arg) => {
                    const { totalCount } = JSON.parse(meta!.response!.headers.get("count")!);
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            addProjectCertificate: builder.mutation<ProjectCertificateRecord, Partial<ProjectCertificateRecord>>({
                query: (certificate) => ({
                    url: "/project/createProjectCertificate",
                    method: "POST",
                    body: { ...certificate },
                }),
                invalidatesTags: ["WorkEffort"],
            }),
        };
    },
});

export const {
    useFetchProjectsQuery,
    useAddProjectMutation,
    useUpdateProjectMutation,
    useFetchProjectCertificatesQuery, useAddProjectCertificateMutation
} = projectsApi;
export {projectsApi};
