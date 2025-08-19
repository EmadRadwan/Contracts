import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../../configureStore";
import {Vehicle} from "../../../models/service/vehicle";
import {State, toODataString} from "@progress/kendo-data-query";


interface ListResponse<T> {
    data: T[];
    total: number;
}

const vehiclesApi = createApi({
    reducerPath: "vehicles",
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

    tagTypes: [
        "Vehicles",
        "ServiceRates",
        "ServiceSpecifications",
        "VehicleMakes",
        "VehicleModels",
    ],
    endpoints(builder) {
        return {
            fetchVehicles: builder.query<ListResponse<Vehicle>, State>({
                query: (queryArgs) => {
                    const url = `/odata/vehicleRecords?$count=true&${toODataString(queryArgs)}`;
                    return {url, method: "GET"};
                },
                providesTags: ["Vehicles"],
                transformResponse: (response: any, meta, arg) => {
                    const {totalCount} = JSON.parse(
                        meta!.response!.headers.get("count")!,
                    );
                    return {
                        data: response,
                        total: totalCount,
                    };
                },
            }),
            fetchVehicleMakes: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/vehicleProperties",
                        method: "GET",
                    };
                },
                providesTags: ["VehicleMakes"],
            }),
            fetchVehicleModels: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/vehicleProperties/getVehicleModels",
                        method: "GET",
                    };
                },
                providesTags: ["VehicleModels"],
            }),
            fetchVehicleModelsByMakeId: builder.query<any[], any>({
                query: (makeId) => {
                    return {
                        url: `/vehicleProperties/${makeId}/getVehicleModelsByMakeId`,
                        params: makeId,
                        method: "GET",
                    };
                },
            }),
            fetchVehicleAnnotations: builder.query<any[], any>({
                query: (vehicleId) => {
                    return {
                        url: `/vehicleAnnotations/${vehicleId}/getVehicleAnnotations`,
                        params: vehicleId,
                        method: "GET",
                    };
                },
            }),
            fetchVehicleInteriorColors: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/vehicleProperties/getVehicleInteriorColors",
                        method: "GET",
                    };
                },
            }),
            fetchVehicleExteriorColors: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/vehicleProperties/getVehicleExteriorColors",
                        method: "GET",
                    };
                },
            }),
            fetchVehicleTypes: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/vehicleProperties/getVehicleTypes",
                        method: "GET",
                    };
                },
            }),
            fetchVehicleTransmissionTypes: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/vehicleProperties/getVehicleTransmissionTypes",
                        method: "GET",
                    };
                },
            }),
            fetchServiceRates: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/vehicleProperties/getServiceRates",
                        method: "GET",
                    };
                },
            }),
            fetchServiceSpecification: builder.query<any[], undefined>({
                query: () => {
                    return {
                        url: "/vehicleProperties/getServiceSpecifications",
                        method: "GET",
                    };
                },
            }),
            createVehicle: builder.mutation({
                query: (vehicle) => {
                    return {
                        url: "/vehicles/createVehicle",
                        method: "POST",
                        body: {...vehicle},
                    };
                },
                invalidatesTags: ["Vehicles"],
            }),
            updateVehicle: builder.mutation({
                query: (vehicle) => {
                    return {
                        url: "/vehicles/updateVehicle",
                        method: "PUT",
                        body: {...vehicle},
                    };
                },
                invalidatesTags: ["Vehicles"],
            }),
            createServiceRate: builder.mutation({
                query: (serviceRate) => {
                    return {
                        url: "/vehicleProperties/createServiceRate",
                        method: "POST",
                        body: {...serviceRate},
                    };
                },
                invalidatesTags: ["ServiceRates"],
            }),
            updateServiceRate: builder.mutation({
                query: (serviceRate) => {
                    return {
                        url: "/vehicleProperties/updateServiceRate",
                        method: "PUT",
                        body: {...serviceRate},
                    };
                },
                invalidatesTags: ["ServiceRates"],
            }),
            createServiceSpecification: builder.mutation({
                query: (serviceSpecification) => {
                    return {
                        url: "/vehicleProperties/createServiceSpecification",
                        method: "POST",
                        body: {...serviceSpecification},
                    };
                },
                invalidatesTags: ["ServiceSpecifications"],
            }),
            createVehicleMake: builder.mutation({
                query: (vehicleMake) => {
                    return {
                        url: "/vehicleProperties/createVehicleMake",
                        method: "POST",
                        body: {...vehicleMake},
                    };
                },
                invalidatesTags: ["VehicleMakes"],
            }),
            createVehicleModel: builder.mutation({
                query: (vehicleModel) => {
                    return {
                        url: "/vehicleProperties/createVehicleModel",
                        method: "POST",
                        body: {...vehicleModel},
                    };
                },
                invalidatesTags: ["VehicleModels"],
            }),
            updateServiceSpecification: builder.mutation({
                query: (serviceSpecification) => {
                    return {
                        url: "/vehicleProperties/updateServiceSpecification",
                        method: "PUT",
                        body: {...serviceSpecification},
                    };
                },
                invalidatesTags: ["ServiceSpecifications"],
            }),
            updateVehicleMake: builder.mutation({
                query: (vehicleMake) => {
                    return {
                        url: "/vehicleProperties/updateVehicleMake",
                        method: "PUT",
                        body: {...vehicleMake},
                    };
                },
                invalidatesTags: ["VehicleMakes"],
            }),
            updateVehicleModel: builder.mutation({
                query: (vehicleModel) => {
                    return {
                        url: "/vehicleProperties/updateVehicleModel",
                        method: "PUT",
                        body: {...vehicleModel},
                    };
                },
                invalidatesTags: ["VehicleModels"],
            }),
        };
    },
});

export const {
    useFetchVehiclesQuery,
    useFetchVehicleMakesQuery,
    useFetchVehicleModelsQuery,
    useFetchVehicleModelsByMakeIdQuery,
    useFetchVehicleInteriorColorsQuery,
    useFetchVehicleExteriorColorsQuery,
    useFetchVehicleTypesQuery,
    useFetchVehicleTransmissionTypesQuery,
    useCreateVehicleMutation,
    useUpdateVehicleMutation,
    useFetchServiceRatesQuery,
    useFetchServiceSpecificationQuery,
    useCreateServiceRateMutation,
    useUpdateServiceRateMutation,
    useCreateServiceSpecificationMutation,
    useUpdateServiceSpecificationMutation,
    useCreateVehicleMakeMutation,
    useUpdateVehicleMakeMutation,
    useCreateVehicleModelMutation,
    useUpdateVehicleModelMutation,
    useFetchVehicleAnnotationsQuery,
} = vehiclesApi;
export {vehiclesApi};
