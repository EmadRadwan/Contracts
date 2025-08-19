// import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
// import {store} from "../configureStore";
// import { salesOrdersApi } from "./salesOrdersApi";

// interface ListResponse<T> {
//     currentPage: number;
//     pageSize: number;
//     totalCount: number;
//     totalPages: number;
//     data: T[];
// }

// interface ListOrder<T> {
//     data: T[];
// }

// const purchaseOrdersApi = createApi({
//     reducerPath: "purchaseOrders",
//     baseQuery: fetchBaseQuery({
//         baseUrl: import.meta.env.VITE_API_URL,
//         prepareHeaders: (headers, {getState}) => {
//             // By default, if we have a token in the store, let's use that for authenticated requests
//             const token = store.getState().account.user?.token;
//             if (token) {
//                 headers.set("authorization", `Bearer ${token}`);
//             }
//             return headers;
//         },
//     }),
//     tagTypes: ['PurchaseOrders'],
//     endpoints(builder) {
        
//     },
// });

// export const {
    
// } = purchaseOrdersApi;
// export {purchaseOrdersApi};
