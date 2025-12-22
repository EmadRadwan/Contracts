// === Sales / Orders Module ===
{
    element: <RequireRole allowedRoles="Sales_View"/>,
        children: [
    {path: "ordersDashboard", element: <OrderDashboard/>},
    {path: "orders/sales", element: <OrdersList orderType="SALES_ORDER"/>},
    {path: "orders/purchase", element: <OrdersList orderType="PURCHASE_ORDER"/>},
    {path: "returns", element: <ReturnsList/>},
    {path: "returns/:returnId", element: <EditReturn/>},
    {path: "returns/:returnId/items", element: <OrderReturnItems/>},
    {path: "quotes", element: <QuotesList/>},

    // REFACTOR: Added dedicated RequireRole wrappers for the two sales request routes
    // Purpose: Protect access to Sales Requests and Reserve Requests with granular roles
    // Improves: Prevents unauthorized users from accessing these pages via direct URL
    // Context: Matches the pattern used elsewhere (e.g., Accounting_Payments_Due_View)
    {
        element: <RequireRole allowedRoles="CreateSalesRequest" />,
        children: [
            {path: "sales-requests", element: <SalesRequestsList/>},
        ],
    },
    {
        element: <RequireRole allowedRoles="CreateReserveRequest" />,
        children: [
            {path: "reserve-requests", element: <ReserveRequestsList/>},
        ],
    },
],
},