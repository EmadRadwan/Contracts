const salesRequestApi = createApi({
        reducerPath: "salesRequests",
        baseQuery: fetchBaseQuery({
                baseUrl: import.meta.env.VITE_API_URL,
                prepareHeaders: (headers, { getState }) => {
                        const token = (getState as () => { account: { user?: { token?: string } } })().account.user?.token;
                        const lang = (getState as () => { localization: { language?: string } })().localization.language;

                        if (token) headers.set("authorization", `Bearer ${token}`);
                        if (lang) headers.set("Accept-Language", lang);

                        return headers;
                },
        }),
        tagTypes: [
                "SalesRequest",           // For individual sales requests + list
                "SalesRequestInstallments", // Specific to installments per SR
                "ReserveRequest",         // For reserve requests
        ],
        endpoints: (builder) => ({
                // -----------------------------------------------------------------
                // LIST – Sales Requests
                // -----------------------------------------------------------------
                fetchSalesRequests: builder.query<ListResponse<SalesRequest>, State>({
                        query: (dataState) => {
                                const odata = toODataString(dataState);
                                return `/odata/salesRequestRecords?$count=true&${odata}`;
                        },
                        providesTags: (result) =>
                            result
                                ? [
                                        ...result.data.map(({ salesRequestId }) => ({
                                                type: "SalesRequest" as const,
                                                id: salesRequestId,
                                        })),
                                        { type: "SalesRequest", id: "LIST" },
                                ]
                                : [{ type: "SalesRequest", id: "LIST" }],
                        transformResponse: (response: any, meta) => {
                                const totalCountHeader = meta?.response?.headers.get("count");
                                const totalCount = totalCountHeader ? JSON.parse(totalCountHeader).totalCount : 0;
                                return { data: response.value || response, total: totalCount };
                        },
                }),

                // -----------------------------------------------------------------
                // LIST – Reserve Requests
                // -----------------------------------------------------------------
                fetchReserveRequests: builder.query<ListResponse<ReserveRequest>, State>({
                        query: (dataState) => {
                                const odata = toODataString(dataState);
                                return `/odata/reserveRequestRecords?$count=true&${odata}`;
                        },
                        providesTags: (result) =>
                            result
                                ? [
                                        ...result.data.map(({ reserveRequestId }) => ({
                                                type: "ReserveRequest" as const,
                                                id: reserveRequestId,
                                        })),
                                        { type: "ReserveRequest", id: "LIST" },
                                ]
                                : [{ type: "ReserveRequest", id: "LIST" }],
                        transformResponse: (response: any, meta) => {
                                const totalCountHeader = meta?.response?.headers.get("count");
                                const totalCount = totalCountHeader ? JSON.parse(totalCountHeader).totalCount : 0;
                                return { data: response.value || response, total: totalCount };
                        },
                }),

                // -----------------------------------------------------------------
                // CREATE
                // -----------------------------------------------------------------
                addSalesRequest: builder.mutation<string, any>({
                        query: (payload) => ({
                                url: "/salesRequests",
                                method: "POST",
                                body: payload,
                        }),
                        invalidatesTags: [{ type: "SalesRequest", id: "LIST" }],
                }),

                addReserveRequest: builder.mutation<string, any>({
                        query: (payload) => ({
                                url: "/reserveRequests/reserve",
                                method: "POST",
                                body: payload,
                        }),
                        invalidatesTags: [{ type: "ReserveRequest", id: "LIST" }],
                }),

                // -----------------------------------------------------------------
                // UPDATE
                // -----------------------------------------------------------------
                updateSalesRequest: builder.mutation<any, any>({
                        query: (payload) => ({
                                url: `/salesRequests/${payload.salesRequestDto.salesRequestId}`,
                                method: "PUT",
                                body: payload,
                        }),
                        invalidatesTags: (result, error, arg) => [
                                { type: "SalesRequest", id: arg.salesRequestDto.salesRequestId },
                                { type: "SalesRequest", id: "LIST" },
                                // Installments may have changed → invalidate cached installments
                                { type: "SalesRequestInstallments", id: arg.salesRequestDto.salesRequestId },
                        ],
                }),

                updateReserveRequest: builder.mutation<any, { reserveRequestDto: any }>({
                        query: (payload) => ({
                                url: "/reserveRequests",
                                method: "PUT",
                                body: payload,
                        }),
                        invalidatesTags: [{ type: "ReserveRequest", id: "LIST" }],
                }),

                // -----------------------------------------------------------------
                // APPROVE – Critical: invalidates both item and list + installments
                // -----------------------------------------------------------------
                approveSalesRequest: builder.mutation<CreateSalesRequest.SalesRequestResponseDto, string>({
                        query: (salesRequestId) => ({
                                url: `salesRequests/${salesRequestId}/approve`,
                                method: "POST",
                        }),
                        invalidatesTags: (result, error, salesRequestId) => [
                                { type: "SalesRequest", id: salesRequestId },
                                { type: "SalesRequest", id: "LIST" },
                                // Payments are created from installments → likely no longer editable/viewable same way
                                { type: "SalesRequestInstallments", id: salesRequestId },
                        ],
                }),

                // -----------------------------------------------------------------
                // DELETE
                // -----------------------------------------------------------------
                deleteSalesRequest: builder.mutation<void, string>({
                        query: (salesRequestId) => ({
                                url: `/salesRequests/${salesRequestId}`,
                                method: "DELETE",
                        }),
                        invalidatesTags: (result, error, salesRequestId) => [
                                { type: "SalesRequest", id: salesRequestId },
                                { type: "SalesRequest", id: "LIST" },
                        ],
                }),

                // -----------------------------------------------------------------
                // CALCULATOR
                // -----------------------------------------------------------------
                calculateInstallmentPrice: builder.mutation<CalculatorResponse, CalculateInstallmentPriceRequest>({
                        query: (body) => ({
                                url: "/salesRequests/calculate-meter-price",
                                method: "POST",
                                body,
                        }),
                        // No cache invalidation needed — pure calculation
                }),

                // -----------------------------------------------------------------
                // GET INSTALLMENTS FOR A SPECIFIC SALES REQUEST
                // -----------------------------------------------------------------
                getSalesRequestInstallments: builder.query<
                    Array<{ installmentNumber: number; dueDate: string; amount: number; isAdvance: boolean }>,
                    string
                    >({
                        query: (salesRequestId) => `salesRequests/${salesRequestId}/installments`,
                        providesTags: (result, error, salesRequestId) => [
                                { type: "SalesRequestInstallments", id: salesRequestId },
                        ],
                }),
        }),
});