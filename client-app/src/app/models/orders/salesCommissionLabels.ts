export const SALE_TYPE_OPTIONS = [
    { saleTypeId: "COMM_SALE_DIRECT", description: "بيع مباشر" },
    { saleTypeId: "COMM_SALE_PERSONAL", description: "بيع شخصي" },
    { saleTypeId: "COMM_SALE_INDIRECT", description: "بيع غير مباشر" },
];

export const SALE_TYPE_LABELS: Record<string, string> = SALE_TYPE_OPTIONS.reduce(
    (acc, option) => ({ ...acc, [option.saleTypeId]: option.description }),
    {} as Record<string, string>
);

// MUI Chip `color` prop values, used in SalesCommissionsList
export const COMMISSION_STATUS_CHIP_COLORS: Record<string, "warning" | "success" | "info"> = {
    COMMISSION_PENDING: "warning",
    COMMISSION_APPROVED: "success",
    COMMISSION_PAID: "info",
};

// react-ribbons background colors, used in SalesCommissionForm
export const COMMISSION_STATUS_RIBBON_COLORS: Record<string, string> = {
    COMMISSION_PENDING: "#ff9800",
    COMMISSION_APPROVED: "#4caf50",
    COMMISSION_PAID: "#1976d2",
};
