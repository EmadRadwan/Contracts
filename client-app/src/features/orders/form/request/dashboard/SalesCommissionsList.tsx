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
} from "../../../../../app/store/apis/salesCommissionsApi";
import { SalesCommissionActionsMenu } from "../menu/SalesCommissionActionsMenu";
import { SalesCommission } from "../../../../../app/models/orders/salesCommission";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import SalesRequestMenu from "../menu/SalesRequestMenu";
import SalesCommissionForm from "../form/SalesCommissionForm";
import SalesCommissionsDateRangeExcel from "../report/SalesCommissionsDateRangeExcel";

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

    const BooleanCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td className={props.className} style={props.style} colSpan={props.colSpan} role="gridcell"
                aria-colindex={props.ariaColumnIndex} aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }} {...navigationAttributes}>
                {props.dataItem[props.field] ? "نعم" : "لا"}
            </td>
        );
    };

    const ActionsCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td className={props.className} style={props.style} colSpan={props.colSpan} role="gridcell"
                aria-colindex={props.ariaColumnIndex} aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }} {...navigationAttributes}>
                <SalesCommissionActionsMenu
                    salesCommissionId={props.dataItem.salesCommissionId}
                    currentStatusId={props.dataItem.statusId}
                    disabled={false}
                    onCommissionApproved={refetch}
                    onCommissionReset={refetch}
                    onCommissionDeleted={refetch}
                />
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
                        <Grid container alignItems="center">
                            <Grid item xs={5}>
                                <Button
                                    color="secondary"
                                    onClick={() => { setOpenSalesRequestId(undefined); setEditMode(1); }}
                                    variant="outlined"
                                >
                                    {getTranslatedLabel("salesCommission.list.create", "إنشاء عمولة جديدة")}
                                </Button>
                            </Grid>
                            <Grid item>
                                <SalesCommissionsDateRangeExcel dataState={dataState} />
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
                        field="commissionDate"
                        title={getTranslatedLabel("salesCommission.list.commissionDate", "تاريخ العمولة")}
                        width={160}
                        filter="date"
                        format="{0:dd/MM/yyyy}"
                    />
                    <Column
                        field="salePrice"
                        title={getTranslatedLabel("salesCommission.list.salePrice", "سعر البيع")}
                        width={150}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="collectedAmount"
                        title={getTranslatedLabel("salesCommission.list.collectedAmount", "المبلغ المحصل")}
                        width={150}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="salesRepPercent"
                        title={getTranslatedLabel("salesCommission.list.salesRepPercent", "نسبة المندوب %")}
                        width={140}
                        filter="numeric"
                        format="{0:n4}"
                    />
                    <Column
                        field="salesRepAmount"
                        title={getTranslatedLabel("salesCommission.list.salesRepAmount", "مبلغ المندوب")}
                        width={150}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="salesRepNetAmount"
                        title={getTranslatedLabel("salesCommission.list.salesRepNetAmount", "صافي المندوب")}
                        width={150}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="salesRep2Name"
                        title={getTranslatedLabel("salesCommission.list.salesRep2", "المندوب الثاني")}
                        width={180}
                    />
                    <Column
                        field="salesRep2Percent"
                        title={getTranslatedLabel("salesCommission.list.salesRep2Percent", "نسبة المندوب الثاني %")}
                        width={160}
                        filter="numeric"
                        format="{0:n4}"
                    />
                    <Column
                        field="salesRep2Amount"
                        title={getTranslatedLabel("salesCommission.list.salesRep2Amount", "مبلغ المندوب الثاني")}
                        width={170}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="salesRep2NetAmount"
                        title={getTranslatedLabel("salesCommission.list.salesRep2NetAmount", "صافي المندوب الثاني")}
                        width={170}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="managerPercent"
                        title={getTranslatedLabel("salesCommission.list.managerPercent", "نسبة المدير %")}
                        width={140}
                        filter="numeric"
                        format="{0:n4}"
                    />
                    <Column
                        field="managerAmount"
                        title={getTranslatedLabel("salesCommission.list.managerAmount", "مبلغ المدير")}
                        width={150}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="managerNetAmount"
                        title={getTranslatedLabel("salesCommission.list.managerNetAmount", "صافي المدير")}
                        width={150}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="manager2Name"
                        title={getTranslatedLabel("salesCommission.list.manager2", "المدير الثاني")}
                        width={180}
                    />
                    <Column
                        field="manager2Percent"
                        title={getTranslatedLabel("salesCommission.list.manager2Percent", "نسبة المدير الثاني %")}
                        width={160}
                        filter="numeric"
                        format="{0:n4}"
                    />
                    <Column
                        field="manager2Amount"
                        title={getTranslatedLabel("salesCommission.list.manager2Amount", "مبلغ المدير الثاني")}
                        width={170}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="manager2NetAmount"
                        title={getTranslatedLabel("salesCommission.list.manager2NetAmount", "صافي المدير الثاني")}
                        width={170}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="externalCompanyName"
                        title={getTranslatedLabel("salesCommission.list.externalCompany", "شركة الوسيط")}
                        width={200}
                    />
                    <Column
                        field="externalCompanyPercent"
                        title={getTranslatedLabel("salesCommission.list.externalCompanyPercent", "نسبة الوسيط %")}
                        width={140}
                        filter="numeric"
                        format="{0:n4}"
                    />
                    <Column
                        field="externalCompanyGrossAmount"
                        title={getTranslatedLabel("salesCommission.list.externalCompanyGross", "إجمالي الوسيط")}
                        width={160}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="externalCompanyNetAmount"
                        title={getTranslatedLabel("salesCommission.list.externalCompanyNet", "صافي الوسيط")}
                        width={150}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="externalSalesRepName"
                        title={getTranslatedLabel("salesCommission.list.externalSalesRep", "مندوب الوسيط")}
                        width={180}
                    />
                    <Column
                        field="externalSalesRepPercent"
                        title={getTranslatedLabel("salesCommission.list.externalSalesRepPercent", "نسبة مندوب الوسيط %")}
                        width={180}
                        filter="numeric"
                        format="{0:n4}"
                    />
                    <Column
                        field="externalSalesRepAmount"
                        title={getTranslatedLabel("salesCommission.list.externalSalesRepAmount", "مبلغ مندوب الوسيط")}
                        width={180}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="externalSalesRepNetAmount"
                        title={getTranslatedLabel("salesCommission.list.externalSalesRepNetAmount", "صافي مندوب الوسيط")}
                        width={180}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="hasExternalSalesRepWithholdingTaxExemption"
                        title={getTranslatedLabel("salesCommission.list.hasExternalSalesRepWhtExemption", "إعفاء ض.استقطاع مندوب الوسيط")}
                        cell={BooleanCell}
                        width={170}
                        filterable={false}
                        sortable={false}
                    />
                    <Column
                        field="externalSalesRepNationalId"
                        title={getTranslatedLabel("salesCommission.list.externalSalesRepNationalId", "الرقم القومي (مندوب الوسيط)")}
                        width={180}
                    />
                    <Column
                        field="externalManagerName"
                        title={getTranslatedLabel("salesCommission.list.externalManager", "مدير الوسيط")}
                        width={180}
                    />
                    <Column
                        field="externalManagerPercent"
                        title={getTranslatedLabel("salesCommission.list.externalManagerPercent", "نسبة مدير الوسيط %")}
                        width={180}
                        filter="numeric"
                        format="{0:n4}"
                    />
                    <Column
                        field="externalManagerAmount"
                        title={getTranslatedLabel("salesCommission.list.externalManagerAmount", "مبلغ مدير الوسيط")}
                        width={180}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="externalManagerNetAmount"
                        title={getTranslatedLabel("salesCommission.list.externalManagerNetAmount", "صافي مدير الوسيط")}
                        width={180}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="hasExternalManagerWithholdingTaxExemption"
                        title={getTranslatedLabel("salesCommission.list.hasExternalManagerWhtExemption", "إعفاء ض.استقطاع مدير الوسيط")}
                        cell={BooleanCell}
                        width={170}
                        filterable={false}
                        sortable={false}
                    />
                    <Column
                        field="externalManagerNationalId"
                        title={getTranslatedLabel("salesCommission.list.externalManagerNationalId", "الرقم القومي (مدير الوسيط)")}
                        width={180}
                    />
                    <Column
                        field="hasVatExemption"
                        title={getTranslatedLabel("salesCommission.list.hasVatExemption", "إعفاء ض.ق.م")}
                        cell={BooleanCell}
                        width={130}
                        filterable={false}
                        sortable={false}
                    />
                    <Column
                        field="hasWithholdingTaxExemption"
                        title={getTranslatedLabel("salesCommission.list.hasWithholdingTaxExemption", "إعفاء ض.استقطاع")}
                        cell={BooleanCell}
                        width={150}
                        filterable={false}
                        sortable={false}
                    />
                    <Column
                        field="vatPercent"
                        title={getTranslatedLabel("salesCommission.list.vatPercent", "نسبة ض.ق.م %")}
                        width={130}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="withholdingTaxPercent"
                        title={getTranslatedLabel("salesCommission.list.withholdingTaxPercent", "نسبة ض.استقطاع %")}
                        width={160}
                        filter="numeric"
                        format="{0:n2}"
                    />
                    <Column
                        field="notes"
                        title={getTranslatedLabel("salesCommission.list.notes", "ملاحظات")}
                        width={200}
                    />
                    <Column
                        title={getTranslatedLabel("salesCommission.list.actions", "إجراءات")}
                        cell={ActionsCell}
                        width={140}
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
