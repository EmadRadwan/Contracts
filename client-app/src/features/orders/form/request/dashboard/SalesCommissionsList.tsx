import React, { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper, Button, Chip } from "@mui/material";
import { DataResult, State } from "@progress/kendo-data-query";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import {
    useFetchSalesCommissionsQuery,
    useApproveSalesCommissionMutation,
} from "../../../../../app/store/apis/salesCommissionsApi";
import { SalesCommission } from "../../../../../app/models/orders/salesCommission";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import SalesRequestMenu from "../menu/SalesRequestMenu";
import SalesCommissionForm from "../form/SalesCommissionForm";
import { toast } from "react-toastify";

const SALE_TYPE_LABELS: Record<string, string> = {
    COMM_SALE_DIRECT: "بيع مباشر",
    COMM_SALE_PERSONAL: "بيع شخصي",
    COMM_SALE_INDIRECT: "بيع غير مباشر",
};

const STATUS_COLORS: Record<string, "warning" | "success" | "info"> = {
    COMMISSION_PENDING: "warning",
    COMMISSION_APPROVED: "success",
    COMMISSION_PAID: "info",
};

export default function SalesCommissionsList() {
    const location = useLocation();
    const initialSalesRequestId: string | undefined = (location.state as any)?.salesRequestId;

    const [editMode, setEditMode] = useState<0 | 1 | 2>(initialSalesRequestId ? 1 : 0);
    const [selectedCommission, setSelectedCommission] = useState<SalesCommission | undefined>(undefined);
    const [openSalesRequestId, setOpenSalesRequestId] = useState<string | undefined>(initialSalesRequestId);
    const [commissions, setCommissions] = useState<DataResult>({ data: [], total: 0 });
    const [dataState, setDataState] = useState<State>({ take: 10, skip: 0 });

    const { data, isFetching, refetch } = useFetchSalesCommissionsQuery({ ...dataState });
    const [approveCommission, { isLoading: isApproving }] = useApproveSalesCommissionMutation();
    const { getTranslatedLabel } = useTranslationHelper();

    useEffect(() => {
        if (data) {
            const total = typeof data.total === "object" ? (data.total as any).total : data.total;
            setCommissions({ data: data.data, total: Number(total) });
        }
    }, [data]);

    function handleSelectCommission(commissionId: string) {
        const commission = commissions.data.find((c: SalesCommission) => c.salesCommissionId === commissionId);
        setSelectedCommission(commission);
        setOpenSalesRequestId(undefined);
        setEditMode(2);
    }

    function cancelEdit() {
        setEditMode(0);
        setSelectedCommission(undefined);
        setOpenSalesRequestId(undefined);
    }

    async function handleApprove(commissionId: string, e: React.MouseEvent) {
        e.stopPropagation();
        try {
            await approveCommission(commissionId).unwrap();
            toast.success(getTranslatedLabel("salesCommission.form.approveSuccess", "تم اعتماد العمولة بنجاح"));
            refetch();
        } catch {
            toast.error(getTranslatedLabel("salesCommission.form.error", "فشل في حفظ العمولة"));
        }
    }

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const CommissionIdCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{ ...props.style, color: "blue" }}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button size="small" onClick={() => handleSelectCommission(props.dataItem.salesCommissionId)}>
                    {props.dataItem.salesCommissionId}
                </Button>
            </td>
        );
    };

    const SaleTypeCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        const label = SALE_TYPE_LABELS[props.dataItem.saleTypeId] ?? props.dataItem.saleTypeId;
        return (
            <td className={props.className} style={props.style} colSpan={props.colSpan} role="gridcell"
                aria-colindex={props.ariaColumnIndex} aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }} {...navigationAttributes}>
                {label}
            </td>
        );
    };

    const StatusCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        const statusId = props.dataItem.statusId;
        const color = STATUS_COLORS[statusId] ?? "default";
        const statusLabels: Record<string, string> = {
            COMMISSION_PENDING: getTranslatedLabel("salesCommission.list.statusPending", "قيد الانتظار"),
            COMMISSION_APPROVED: getTranslatedLabel("salesCommission.list.statusApproved", "معتمدة"),
            COMMISSION_PAID: getTranslatedLabel("salesCommission.list.statusPaid", "مدفوعة"),
        };
        return (
            <td className={props.className} style={props.style} colSpan={props.colSpan} role="gridcell"
                aria-colindex={props.ariaColumnIndex} aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }} {...navigationAttributes}>
                <Chip label={statusLabels[statusId] ?? statusId} color={color} size="small" />
            </td>
        );
    };

    const ApproveCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        const isPending = props.dataItem.statusId === "COMMISSION_PENDING";
        return (
            <td className={props.className} style={props.style} colSpan={props.colSpan} role="gridcell"
                aria-colindex={props.ariaColumnIndex} aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }} {...navigationAttributes}>
                {isPending && (
                    <Button
                        size="small"
                        variant="outlined"
                        color="success"
                        disabled={isApproving}
                        onClick={(e) => handleApprove(props.dataItem.salesCommissionId, e)}
                    >
                        {getTranslatedLabel("salesCommission.list.approve", "اعتماد")}
                    </Button>
                )}
            </td>
        );
    };

    if (editMode > 0) {
        return (
            <SalesCommissionForm
                commission={editMode === 2 ? selectedCommission : undefined}
                salesRequestId={openSalesRequestId}
                editMode={editMode}
                cancelEdit={cancelEdit}
            />
        );
    }

    return (
        <>
            <SalesRequestMenu selectedMenuItem="sales-commissions" />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <KendoGrid
                    style={{ height: "75vh", width: "94vw", flex: 1 }}
                    resizable
                    filterable
                    sortable
                    pageable
                    {...dataState}
                    data={commissions}
                    onDataStateChange={dataStateChange}
                >
                    <GridToolbar>
                        <Grid container>
                            <Grid item xs={5}>
                                <Button
                                    color="secondary"
                                    onClick={() => { setOpenSalesRequestId(undefined); setEditMode(1); }}
                                    variant="outlined"
                                >
                                    {getTranslatedLabel("salesCommission.list.create", "إنشاء عمولة جديدة")}
                                </Button>
                            </Grid>
                        </Grid>
                    </GridToolbar>
                    <Column
                        field="salesCommissionId"
                        title={getTranslatedLabel("salesCommission.list.id", "رقم العمولة")}
                        cell={CommissionIdCell}
                        width={160}
                        locked
                    />
                    <Column
                        field="salesRequestId"
                        title={getTranslatedLabel("salesCommission.list.salesRequest", "طلب البيع")}
                        width={150}
                    />
                    <Column
                        field="apartmentName"
                        title={getTranslatedLabel("salesCommission.list.apartment", "الشقة")}
                        width={200}
                    />
                    <Column
                        field="projectName"
                        title={getTranslatedLabel("salesCommission.list.project", "المشروع")}
                        width={200}
                    />
                    <Column
                        field="saleTypeId"
                        title={getTranslatedLabel("salesCommission.list.saleType", "نوع البيع")}
                        cell={SaleTypeCell}
                        width={180}
                    />
                    <Column
                        field="statusId"
                        title={getTranslatedLabel("salesCommission.list.status", "الحالة")}
                        cell={StatusCell}
                        width={140}
                    />
                    <Column
                        field="salesRepName"
                        title={getTranslatedLabel("salesCommission.list.salesRep", "المندوب")}
                        width={180}
                    />
                    <Column
                        field="managerName"
                        title={getTranslatedLabel("salesCommission.list.manager", "المدير")}
                        width={180}
                    />
                    <Column
                        title={getTranslatedLabel("salesCommission.list.approve", "اعتماد")}
                        cell={ApproveCell}
                        width={120}
                        filterable={false}
                        sortable={false}
                    />
                </KendoGrid>
                {isFetching && (
                    <LoadingComponent message={getTranslatedLabel("salesCommission.list.loading", "جاري تحميل عمولات المبيعات...")} />
                )}
            </Paper>
        </>
    );
}
