import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";

export interface UserListDto {
    id: string;
    userName: string;
    displayName: string;
    email: string;
    organizationPartyId: string;
}

export interface RoleDto {
    id: string;
    name: string;
}

export interface AssignRoleDto {
    userId: string;
    role: string;
}

export interface CreateRoleDto {
    roleName: string;
}

export interface CreateUserDto {
    email: string;
    userName: string;
    displayName: string;
    organizationPartyId: string;
    roles?: string[];
}

export interface UpdateUserDto {
    userId: string;
    userName: string;
    displayName: string;
    email: string;
    organizationPartyId: string;
    roles: string[];
}

export interface UpdateUserResponse {
    message: string;
    rolesAdded: string[];
    rolesRemoved: string[];
    currentRoles: string[];
}

const usersApi = createApi({
    reducerPath: "users",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers) => {
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            const lang = store.getState().localization.language;
            if (lang) {
                headers.set("Accept-Language", `${lang}`);
            }
            return headers;
        },
    }),
    tagTypes: ["Users", "UserRoles", "Roles"],
    endpoints(builder) {
        return {
            fetchUsers: builder.query<UserListDto[], void>({
                query: () => ({
                    url: `/account/listUsers`,
                    method: "GET",
                }),
                providesTags: ["Users"],
            }),

            fetchAllRoles: builder.query<RoleDto[], void>({
                query: () => ({
                    url: `/account/listRoles`,
                    method: "GET",
                }),
                providesTags: ["Roles"],
            }),

            fetchUserRoles: builder.query<string[], string>({
                query: (userId) => ({
                    url: `/account/userRoles/${userId}`,
                    method: "GET",
                }),
                providesTags: (_result, _error, userId) => [{ type: "UserRoles", id: userId }],
            }),

            assignRole: builder.mutation<string, AssignRoleDto>({
                query: (dto) => ({
                    url: `/account/assignRole`,
                    method: "POST",
                    body: dto,
                }),
                invalidatesTags: (_result, _error, dto) => [
                    { type: "UserRoles", id: dto.userId },
                    "Users",
                ],
            }),

            removeRole: builder.mutation<string, AssignRoleDto>({
                query: (dto) => ({
                    url: `/account/removeRole`,
                    method: "POST",
                    body: dto,
                }),
                invalidatesTags: (_result, _error, dto) => [
                    { type: "UserRoles", id: dto.userId },
                    "Users",
                ],
            }),

            createRole: builder.mutation<string, CreateRoleDto>({
                query: (dto) => ({
                    url: `/account/createRole`,
                    method: "POST",
                    body: dto,
                    responseHandler: (response) => response.text(),
                }),
                invalidatesTags: ["Roles"],
            }),

            createUser: builder.mutation<UserListDto, CreateUserDto>({
                query: (dto) => ({
                    url: `/account/createUser`,
                    method: "POST",
                    body: dto,
                }),
                invalidatesTags: ["Users"],
            }),

            updateUser: builder.mutation<UpdateUserResponse, UpdateUserDto>({
                query: (dto) => ({
                    url: `/account/updateUser`,
                    method: "POST",
                    body: dto,
                }),
                invalidatesTags: (_result, _error, dto) => [
                    { type: "UserRoles", id: dto.userId },
                    "Users",
                ],
            }),

            fetchUsersByRole: builder.query<UserListDto[], string>({
                query: (roleName) => ({
                    url: `/account/usersByRole/${roleName}`,
                    method: "GET",
                }),
                providesTags: (_result, _error, roleName) => [{ type: "UserRoles", id: roleName }],
            }),
        };
    },
});

export const {
    useFetchUsersQuery,
    useFetchAllRolesQuery,
    useFetchUserRolesQuery,
    useFetchUsersByRoleQuery,
    useAssignRoleMutation,
    useRemoveRoleMutation,
    useCreateRoleMutation,
    useCreateUserMutation,
    useUpdateUserMutation,
} = usersApi;

export {usersApi};
