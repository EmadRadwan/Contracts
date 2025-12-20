// Inside the Accounting module's children array

// Payments sub-module (Incoming + Outgoing)
{
    element: <RequireRole allowedRoles="Accounting_Payments_View" />,
        children: [
    { path: "payments/incoming", element: <PaymentsList paymentType="incoming" /> },
    { path: "payments/outgoing", element: <PaymentsList paymentType="outgoing" /> },
],
},

// Due Payments — separately protected
{
    element: <RequireRole allowedRoles="Accounting_Payments_Due_View" />,
        children: [
    // REFACTOR: Wrapped duePayments route in its own RequireRole wrapper.
    // Purpose: Allow fine-grained access — users can have permission for regular payments
    // but not for due/overdue payments view (or vice versa).
    // Improves: Maximum flexibility in role assignments without affecting other payment routes.
    { path: "duePayments", element: <PaymentsWithDueAmountsList /> },
],
},