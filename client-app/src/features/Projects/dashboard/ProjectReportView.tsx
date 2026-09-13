import React, { useEffect, useMemo, useRef, useState } from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridDataStateChangeEvent,
} from "@progress/kendo-react-grid";
import { process, State } from "@progress/kendo-data-query";
import {
    Box,
    Paper,
    Button,
    Typography,
    FormControlLabel,
    Checkbox,
    MenuItem,
    TextField,
    Divider,
} from "@mui/material";
import { DesktopDatePicker } from "@mui/x-date-pickers/DesktopDatePicker";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs, { Dayjs } from "dayjs";
import { saveAs } from "file-saver";
import { StyledTabs } from "../../../app/components/StyledTabs";
import { StyledTab } from "../../../app/components/StyledTab";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { handleDatesArray } from "../../../app/util/utils";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {
    useLazyFetchProjectReportQuery,
    useLazyFetchProjectReportPdfQuery,
    useFetchProjectBuildingsQuery,
    ProjectReportSummary,
} from "../../../app/store/apis/projectsApi";
import { buildProjectReportWorkbook } from "../report/buildProjectReportWorkbook";

interface Props {
    projectId: string;
    projectName: string;
    onExit: () => void;
}

const XLSX_MIME =
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
const MGMT_FEE_PERCENT_OPTIONS = [10, 11, 12, 12.5, 13, 14, 15];

// Same maintenance-deposit split the server / workbook use.
const isMaintenance = (r: any) =>
    r.paymentTypeId === "RECEIPT_MAINTENANCE_AMOUNT" ||
    r.revenueCategory === "Maintenance Deposit";

const money = (v: number | null | undefined) =>
    (v ?? 0).toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const count = (v: number | null | undefined) => (v ?? 0).toLocaleString("en-US");

type ColDef = {
    field: string;
    title: string;
    width?: number;
    format?: string;
    /** Custom display (fallbacks / conditional blanks) — sorting/filtering still use `field`. */
    cell?: (d: any) => React.ReactNode;
};

const MONEY = "{0:n2}";
const DATE = "{0:dd/MM/yyyy}";

const fmtDate = (d: any) => {
    if (!d) return "";
    const dt = new Date(d);
    return isNaN(dt.getTime()) ? "" : dt.toLocaleDateString("en-GB");
};

const textCell =
    (get: (d: any) => React.ReactNode) =>
    (props: any) =>
        (
            <td
                style={props.style}
                className={props.className}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
            >
                {get(props.dataItem)}
            </td>
        );

// ---- per-section column sets — mirror the Excel sheets (order, fields, formatting) ------
const expenseColumns: ColDef[] = [
    { field: "expenseDate", title: "التاريخ", width: 110, format: DATE },
    { field: "itemDescription", title: "وصف البند", width: 300 },
    { field: "partyName", title: "اسم الطرف", width: 220, cell: (d) => d.partyName || d.partyId || "" },
    { field: "netCertifiedAmount", title: "صافي المعتمد", width: 140, format: MONEY },
    { field: "paymentId", title: "رقم الدفعة", width: 140 },
    { field: "certificateNumber", title: "رقم الشهادة", width: 130 },
    { field: "productName", title: "المنتج/الخدمة", width: 200, cell: (d) => d.productName || d.productId || "" },
    { field: "certificateTypeArabic", title: "النوع", width: 190, cell: (d) => d.certificateTypeArabic || d.certificateType || "" },
    { field: "certificateDescription", title: "الوصف", width: 260 },
    { field: "quantity", title: "الكمية", width: 90, format: MONEY },
    { field: "unitRate", title: "السعر", width: 120, format: MONEY },
    { field: "grossAmount", title: "الإجمالي", width: 140, format: MONEY },
    { field: "discountAmount", title: "الخصم", width: 120, format: MONEY },
    { field: "deductionsAmount", title: "الاستقطاعات", width: 120, format: MONEY },
    { field: "insuranceAmount", title: "التأمين", width: 120, format: MONEY },
];

// Direct payments AND operating expenses use the same 13-column payment sheet.
const paymentColumns: ColDef[] = [
    { field: "paymentId", title: "رقم الدفعة", width: 150 },
    { field: "paymentTypeDescription", title: "النوع", width: 180 },
    { field: "partyIdFromName", title: "من طرف", width: 220 },
    { field: "partyIdToName", title: "إلى طرف", width: 220 },
    { field: "statusDescription", title: "الحالة", width: 150, cell: (d) => d.dueStatusArabic || d.statusDescription || "" },
    { field: "effectiveDate", title: "التاريخ", width: 110, format: DATE },
    { field: "amount", title: "المبلغ", width: 140, format: MONEY },
    { field: "paymentRefNum", title: "رقم المرجع", width: 150 },
    { field: "paymentMethodTypeDescription", title: "طريقة الدفع", width: 160 },
    { field: "chequeNumber", title: "الشيك", width: 120 },
    { field: "chequeDate", title: "تاريخ الشيك", width: 120, format: DATE },
    { field: "costCenterDescription", title: "مركز التكلفة", width: 190 },
    { field: "comments", title: "ملاحظات", width: 300 },
];

const transactionColumns: ColDef[] = [
    { field: "paymentId", title: "رقم القيد", width: 180 },
    { field: "paymentTypeDescription", title: "النوع", width: 180 },
    { field: "partyIdFromName", title: "من طرف", width: 210 },
    { field: "partyIdToName", title: "إلى طرف", width: 210 },
    { field: "statusDescription", title: "الحالة", width: 140, cell: (d) => d.dueStatusArabic || d.statusDescription || "" },
    { field: "effectiveDate", title: "التاريخ", width: 110, format: DATE },
    { field: "amount", title: "المبلغ", width: 140, format: MONEY },
    { field: "paymentRefNum", title: "رقم المرجع", width: 150 },
    { field: "comments", title: "ملاحظات", width: 340 },
];

const payrollColumns: ColDef[] = [
    { field: "paymentId", title: "رقم القيد", width: 200 },
    { field: "paymentTypeDescription", title: "النوع", width: 180 },
    { field: "partyIdFromName", title: "الموظف", width: 230 },
    { field: "partyIdToName", title: "المشروع", width: 210 },
    { field: "statusDescription", title: "الحالة", width: 140, cell: (d) => d.dueStatusArabic || d.statusDescription || "" },
    { field: "effectiveDate", title: "التاريخ", width: 110, format: DATE },
    { field: "amount", title: "المبلغ", width: 140, format: MONEY },
    { field: "comments", title: "ملاحظات", width: 300 },
];

// Agreed-revenue and maintenance-deposit sheets differ only in the scheduled-column header.
const revenueColumns = (scheduledTitle: string): ColDef[] => [
    { field: "paymentId", title: "رقم الدفعة", width: 150 },
    { field: "year", title: "السنة", width: 80 },
    { field: "quarter", title: "الربع", width: 80 },
    { field: "buildingNumber", title: "المبنى", width: 100 },
    { field: "apartmentId", title: "الوحدة", width: 120 },
    { field: "customerName", title: "العميل", width: 220 },
    { field: "revenueCategory", title: "الفئة", width: 160 },
    { field: "scheduledAmount", title: scheduledTitle, width: 160, format: MONEY },
    { field: "collectedAmount", title: "المحصل", width: 140, format: MONEY },
    { field: "outstandingAmount", title: "المتبقي", width: 140, format: MONEY },
    { field: "paymentStatus", title: "الحالة", width: 130 },
    { field: "overdueBucket", title: "شريحة التأخير", width: 200 },
    { field: "dueStatusArabic", title: "حالة الاستحقاق", width: 200 },
    { field: "dueDate", title: "تاريخ الاستحقاق", width: 130, format: DATE },
    { field: "deservedToday", title: "مستحق اليوم", width: 130 },
    { field: "deservedWithinWeek", title: "مستحق خلال أسبوع", width: 150 },
    { field: "deservedWithinMonth", title: "مستحق خلال شهر", width: 150 },
    { field: "lateDue", title: "متأخر", width: 110 },
];

const salesColumns: ColDef[] = [
    { field: "salesRequestId", title: "رقم الطلب", width: 130 },
    { field: "apartmentName", title: "الوحدة", width: 170 },
    { field: "buildingNumber", title: "المبنى", width: 100 },
    { field: "floorNumber", title: "الطابق", width: 110 },
    { field: "fromPartyName", title: "العميل", width: 210 },
    { field: "employeeName", title: "الموظف", width: 190 },
    { field: "statusDescription", title: "الحالة", width: 140 },
    { field: "apartmentStatusDescription", title: "حالة الوحدة", width: 140 },
    { field: "saleDate", title: "تاريخ البيع", width: 120, cell: (d) => (d.isSold ? fmtDate(d.saleDate) : "") },
    { field: "totalPrice", title: "الإجمالي", width: 140, cell: (d) => (d.isSold ? money(d.totalPrice) : "") },
    { field: "advancePayment", title: "المقدم", width: 130, cell: (d) => (d.isSold ? money(d.advancePayment) : "") },
    { field: "maintenanceDeposit", title: "وديعة الصيانة", width: 140, cell: (d) => (d.isSold ? money(d.maintenanceDeposit) : "") },
    { field: "apartmentSpaceM2", title: "مساحة الوحدة", width: 130, format: MONEY },
    { field: "gardenSpaceM2", title: "مساحة الحديقة", width: 130, format: MONEY },
    { field: "apartmentPricePerM2", title: "سعر المتر", width: 130, format: MONEY },
    { field: "projectName", title: "المشروع", width: 200 },
    { field: "comments", title: "ملاحظات", width: 340 },
];

const commissionColumns: ColDef[] = [
    { field: "salesCommissionId", title: "رقم العمولة", width: 150 },
    { field: "salesRequestId", title: "رقم طلب البيع", width: 150 },
    { field: "apartmentName", title: "الوحدة", width: 200 },
    { field: "buildingNumber", title: "المبنى", width: 110 },
    { field: "payeeName", title: "المستفيد", width: 240, cell: (d) => d.payeeName || d.payeePartyId || "" },
    { field: "amount", title: "المبلغ", width: 150, format: MONEY },
    { field: "paymentStatusArabic", title: "حالة الدفع", width: 150 },
    { field: "commissionStatusArabic", title: "حالة العمولة", width: 160 },
    { field: "paymentMethodTypeArabic", title: "طريقة الدفع", width: 160 },
    { field: "effectiveDate", title: "التاريخ", width: 120, format: DATE },
    { field: "chequeNumber", title: "رقم الشيك", width: 140 },
    { field: "comments", title: "ملاحظات", width: 360 },
];

// ---- one grid per section (client-side sort / filter / page) --------------------
function SectionGrid({
    rows,
    columns,
    emptyText,
}: {
    rows: any[] | undefined;
    columns: ColDef[];
    emptyText: string;
}) {
    const [state, setState] = useState<State>({ skip: 0, take: 20 });
    const adjusted = useMemo(() => handleDatesArray(rows || []), [rows]);
    const data = useMemo(() => process(adjusted, state), [adjusted, state]);

    if (!rows || rows.length === 0) {
        return (
            <Typography sx={{ p: 3, color: "text.secondary" }}>{emptyText}</Typography>
        );
    }

    return (
        <KendoGrid
            style={{ height: "58vh" }}
            scrollable="scrollable"
            resizable
            sortable
            filterable
            pageable={{ pageSizes: [20, 50, 100] }}
            {...state}
            data={data}
            onDataStateChange={(e: GridDataStateChangeEvent) => setState(e.dataState)}
        >
            {columns.map((c) => (
                <Column
                    key={c.field}
                    field={c.field}
                    title={c.title}
                    width={c.width}
                    format={c.format}
                    cells={c.cell ? { data: textCell(c.cell) } : undefined}
                />
            ))}
        </KendoGrid>
    );
}

// ---- one period filter (all-data toggle + from/to dates) ----------------------
function PeriodFilter({
    label,
    all,
    setAll,
    start,
    setStart,
    end,
    setEnd,
}: {
    label: string;
    all: boolean;
    setAll: (v: boolean) => void;
    start: Dayjs | null;
    setStart: (v: Dayjs | null) => void;
    end: Dayjs | null;
    setEnd: (v: Dayjs | null) => void;
}) {
    return (
        <Box>
            <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>{label}</Typography>
            <FormControlLabel
                control={<Checkbox size="small" checked={all} onChange={(e) => setAll(e.target.checked)} />}
                label="كل البيانات"
            />
            <Box sx={{ display: "flex", gap: 1, mt: 0.5 }}>
                <DesktopDatePicker
                    label="من"
                    value={start}
                    onChange={setStart}
                    disabled={all}
                    slotProps={{ textField: { size: "small" } }}
                />
                <DesktopDatePicker
                    label="إلى"
                    value={end}
                    onChange={setEnd}
                    disabled={all}
                    slotProps={{ textField: { size: "small" } }}
                />
            </Box>
        </Box>
    );
}

// ---- summary groups (shared by the on-screen ribbon and the PDF) -------------
type SummaryLine = [label: string, value: string, bold?: boolean];
function summaryGroups(s: ProjectReportSummary): { title: string; lines: SummaryLine[] }[] {
    const excluded = s.mgmtExcludedBuildings.length
        ? ` (عدا ${s.mgmtExcludedBuildings.join("، ")})`
        : "";
    return [
        {
            title: "المصاريف",
            lines: [
                ["المستخلصات", money(s.certificateExpenses)],
                ["الدفعات المباشرة", money(s.directPayments)],
                ["قيود محاسبية", money(s.accountingTransactions)],
                ["رواتب المشروع", money(s.projectPayroll)],
                ["المصاريف التشغيلية", money(s.operatingExpenses)],
                ["إجمالي مصاريف المشروع", money(s.totalProjectExpenses), true],
            ],
        },
        {
            title: "الإيرادات",
            lines: [
                ["الإيراد المتفق عليه", money(s.revenueScheduled)],
                ["المحصل", money(s.revenueCollected)],
                ["المتبقي", money(s.revenueOutstanding)],
            ],
        },
        {
            title: "وديعة الصيانة",
            lines: [
                ["الإجمالي", money(s.maintenanceScheduled)],
                ["المحصل", money(s.maintenanceCollected)],
                ["المتبقي", money(s.maintenanceOutstanding)],
            ],
        },
        {
            title: "مبيعات الوحدات",
            lines: [
                ["عدد الوحدات المباعة", count(s.unitsSold)],
                ["إجمالي القيمة", money(s.unitsSoldValue)],
                ["إجمالي المقدمات المحصلة", money(s.unitsAdvanceCollected)],
                ["عدد الوحدات المتاحة", count(s.unitsAvailable)],
            ],
        },
        {
            title: "العمولات",
            lines: [
                ["عدد الدفعات", count(s.commissionPaymentCount)],
                ["المدفوعة", money(s.commissionsPaid)],
                ["المستحقة", money(s.commissionsPending)],
                ["الإجمالي", money(s.commissionsPaid + s.commissionsPending), true],
            ],
        },
        {
            title: "مبلغ الإدارة",
            lines: [
                [`الأساس${excluded}`, money(s.mgmtFeeBase)],
                [`النسبة (${s.mgmtFeePercent}%)`, money(s.mgmtFee)],
                ["يُخصم: المصاريف التشغيلية", money(-s.operatingExpenses)],
                ["الصافي المتبقي", money(s.mgmtFeeNet), true],
            ],
        },
        {
            title: "الصافي",
            lines: [
                ["المحصل − مصاريف المشروع", money(s.netAfterExpenses), true],
                ["بعد خصم العمولات المدفوعة", money(s.netAfterPaidCommissions), true],
            ],
        },
    ];
}

// ---- summary ribbon (screen) ------------------------------------------------
function SummaryRibbon({ s }: { s: ProjectReportSummary }) {
    const groups = summaryGroups(s);

    return (
        <Box
            sx={{
                display: "flex",
                flexWrap: "wrap",
                gap: 1.5,
                mb: 2,
            }}
            dir="rtl"
        >
            {groups.map((g) => (
                <Paper key={g.title} elevation={2} sx={{ p: 1.5, minWidth: 240, flex: "1 1 240px" }}>
                    <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 0.5, color: "primary.main" }}>
                        {g.title}
                    </Typography>
                    {g.lines.map(([label, value, bold]) => (
                        <Box
                            key={label}
                            sx={{
                                display: "flex",
                                justifyContent: "space-between",
                                gap: 2,
                                fontWeight: bold ? 700 : 400,
                                borderTop: bold ? "1px solid" : "none",
                                borderColor: "divider",
                                pt: bold ? 0.5 : 0,
                                mt: bold ? 0.5 : 0,
                            }}
                        >
                            <Typography variant="body2" sx={{ fontWeight: "inherit" }}>{label}</Typography>
                            <Typography variant="body2" sx={{ fontWeight: "inherit", fontVariantNumeric: "tabular-nums" }}>
                                {value}
                            </Typography>
                        </Box>
                    ))}
                </Paper>
            ))}
        </Box>
    );
}

// ---- the screen -------------------------------------------------------------
export default function ProjectReportView({ projectId, projectName, onExit }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();

    const [expensesStartDate, setExpensesStartDate] = useState<Dayjs | null>(dayjs().startOf("year"));
    const [expensesEndDate, setExpensesEndDate] = useState<Dayjs | null>(dayjs());
    const [expensesAllData, setExpensesAllData] = useState(false);
    const [revenuesStartDate, setRevenuesStartDate] = useState<Dayjs | null>(dayjs().startOf("year"));
    const [revenuesEndDate, setRevenuesEndDate] = useState<Dayjs | null>(dayjs());
    const [revenuesAllData, setRevenuesAllData] = useState(false);
    const [salesStartDate, setSalesStartDate] = useState<Dayjs | null>(dayjs().startOf("year"));
    const [salesEndDate, setSalesEndDate] = useState<Dayjs | null>(dayjs());
    const [salesAllData, setSalesAllData] = useState(false);
    const [mgmtFeePercent, setMgmtFeePercent] = useState<number>(12);
    const [excludedBuildings, setExcludedBuildings] = useState<string[]>([]);

    const [tab, setTab] = useState(0);
    const [exporting, setExporting] = useState(false);
    const [exportingPdf, setExportingPdf] = useState(false);

    const { data: projectBuildings = [] } = useFetchProjectBuildingsQuery(projectId);
    const buildingsInitialised = useRef(false);
    useEffect(() => {
        if (buildingsInitialised.current || projectBuildings.length === 0) return;
        buildingsInitialised.current = true;
        setExcludedBuildings(projectBuildings.filter((b) => b === "A1" || b === "A2"));
    }, [projectBuildings]);

    const [trigger, { data: report, isFetching, isError }] = useLazyFetchProjectReportQuery();
    const [triggerPdf] = useLazyFetchProjectReportPdfQuery();

    const buildArgs = () => ({
        projectId,
        expensesStartDate: expensesAllData ? undefined : expensesStartDate?.format("YYYY-MM-DD"),
        expensesEndDate: expensesAllData ? undefined : expensesEndDate?.format("YYYY-MM-DD"),
        expensesAllData,
        revenuesStartDate: revenuesAllData ? undefined : revenuesStartDate?.format("YYYY-MM-DD"),
        revenuesEndDate: revenuesAllData ? undefined : revenuesEndDate?.format("YYYY-MM-DD"),
        revenuesAllData,
        salesStartDate: salesAllData ? undefined : salesStartDate?.format("YYYY-MM-DD"),
        salesEndDate: salesAllData ? undefined : salesEndDate?.format("YYYY-MM-DD"),
        salesAllData,
        mgmtFeePercent,
        excludedBuildings: excludedBuildings.length ? excludedBuildings.join(",") : undefined,
    });

    // Initial load.
    useEffect(() => {
        trigger(buildArgs());
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const period = (all: boolean, start: Dayjs | null, end: Dayjs | null) =>
        all ? "All_Data" : `${start?.format("YYYY-MM-DD")}_to_${end?.format("YYYY-MM-DD")}`;

    const handleExportExcel = async () => {
        if (!report) return;
        setExporting(true);
        try {
            const buffer = await buildProjectReportWorkbook(report, {
                projectName,
                expensesPeriod: period(expensesAllData, expensesStartDate, expensesEndDate),
                revenuesPeriod: period(revenuesAllData, revenuesStartDate, revenuesEndDate),
                salesPeriod: period(salesAllData, salesStartDate, salesEndDate),
            });
            const safe = projectName.replace(/[^a-zA-Z0-9\u0600-\u06FF\s-]/g, "_").trim();
            saveAs(
                new Blob([buffer], { type: XLSX_MIME }),
                `Project_Report_${safe}_${expensesAllData ? "All" : expensesStartDate?.format("YYYYMMDD")}.xlsx`
            );
        } catch (e) {
            console.error("Excel export failed:", e);
            alert("فشل تصدير Excel.");
        } finally {
            setExporting(false);
        }
    };

    // Server-rendered (Telerik Reporting) — KendoReact Grid PDFExport could not shape/reorder
    // Arabic text at all, so PDF is generated on the server via the same pipeline already proven
    // correct for the payment voucher, instead of client-side.
    const handleExportPdf = async () => {
        if (!report) return;
        setExportingPdf(true);
        try {
            const buffer = await triggerPdf({ ...buildArgs(), projectName }).unwrap();
            const safe = projectName.replace(/[^a-zA-Z0-9\u0600-\u06FF\s-]/g, "_").trim();
            saveAs(
                new Blob([buffer], { type: "application/pdf" }),
                `Project_Report_${safe}.pdf`
            );
        } catch (e) {
            console.error("PDF export failed:", e);
            alert("فشل تصدير PDF.");
        } finally {
            setExportingPdf(false);
        }
    };

    const agreedRevenues = useMemo(
        () => (report?.revenues || []).filter((r: any) => !isMaintenance(r)),
        [report]
    );
    const maintenanceRevenues = useMemo(
        () => (report?.revenues || []).filter((r: any) => isMaintenance(r)),
        [report]
    );

    const sections = useMemo(
        () => [
            { label: "المستخلصات", rows: report?.expenses, columns: expenseColumns },
            { label: "الدفعات المباشرة", rows: report?.directPayments, columns: paymentColumns },
            { label: "قيود محاسبية", rows: report?.accountingTransactions, columns: transactionColumns },
            { label: "رواتب المشروع", rows: report?.payroll, columns: payrollColumns },
            { label: "المصاريف التشغيلية", rows: report?.operatingExpenses, columns: paymentColumns },
            { label: "الإيرادات", rows: agreedRevenues, columns: revenueColumns("الإيراد المتفق عليه") },
            { label: "وديعة الصيانة", rows: maintenanceRevenues, columns: revenueColumns("المجدول") },
            { label: "مبيعات الوحدات", rows: report?.apartmentSales, columns: salesColumns },
            { label: "العمولات المدفوعة", rows: report?.paidCommissions, columns: commissionColumns },
        ],
        [report, agreedRevenues, maintenanceRevenues]
    );

    return (
        <Paper elevation={5} className="div-container-withBorderCurved" sx={{ p: 2 }}>
            <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", mb: 1 }}>
                <Typography variant="h5">
                    {getTranslatedLabel("project.projects.report", "تقرير المشروع")} — {projectName}
                    <Typography component="span" variant="h6" color="grey" sx={{ ml: 1 }}>
                        ({projectId})
                    </Typography>
                </Typography>
                <Button color="error" variant="outlined" onClick={onExit}>
                    {getTranslatedLabel("general.back", "رجوع")}
                </Button>
            </Box>

            {/* ---------- filter bar ---------- */}
            <Paper elevation={1} sx={{ p: 1.5, mb: 2 }}>
                <LocalizationProvider dateAdapter={AdapterDayjs}>
                    <Box sx={{ display: "flex", flexWrap: "wrap", gap: 3, alignItems: "flex-start" }}>
                        <PeriodFilter
                            label="المصاريف"
                            all={expensesAllData} setAll={setExpensesAllData}
                            start={expensesStartDate} setStart={setExpensesStartDate}
                            end={expensesEndDate} setEnd={setExpensesEndDate}
                        />
                        <PeriodFilter
                            label="الإيرادات"
                            all={revenuesAllData} setAll={setRevenuesAllData}
                            start={revenuesStartDate} setStart={setRevenuesStartDate}
                            end={revenuesEndDate} setEnd={setRevenuesEndDate}
                        />
                        <PeriodFilter
                            label="المبيعات"
                            all={salesAllData} setAll={setSalesAllData}
                            start={salesStartDate} setStart={setSalesStartDate}
                            end={salesEndDate} setEnd={setSalesEndDate}
                        />

                        <Box>
                            <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>مبلغ الإدارة</Typography>
                            <TextField
                                select
                                size="small"
                                label="النسبة %"
                                value={mgmtFeePercent}
                                onChange={(e) => setMgmtFeePercent(Number(e.target.value))}
                                sx={{ width: 120, mt: 0.5 }}
                            >
                                {MGMT_FEE_PERCENT_OPTIONS.map((p) => (
                                    <MenuItem key={p} value={p}>{p}%</MenuItem>
                                ))}
                            </TextField>
                            {projectBuildings.length > 0 && (
                                <Box sx={{ mt: 0.5, maxWidth: 260 }}>
                                    <Typography variant="caption" color="text.secondary">المباني المستبعدة</Typography>
                                    <Box sx={{ display: "flex", flexWrap: "wrap" }}>
                                        {projectBuildings.map((b) => (
                                            <FormControlLabel
                                                key={b}
                                                control={
                                                    <Checkbox
                                                        size="small"
                                                        checked={excludedBuildings.includes(b)}
                                                        onChange={(e) =>
                                                            setExcludedBuildings((prev) =>
                                                                e.target.checked ? [...prev, b] : prev.filter((x) => x !== b)
                                                            )
                                                        }
                                                    />
                                                }
                                                label={b}
                                            />
                                        ))}
                                    </Box>
                                </Box>
                            )}
                        </Box>
                    </Box>
                </LocalizationProvider>

                <Divider sx={{ my: 1 }} />
                <Box sx={{ display: "flex", gap: 1 }}>
                    <Button variant="contained" onClick={() => trigger(buildArgs())} disabled={isFetching}>
                        {isFetching ? "جاري التحميل..." : "تحديث"}
                    </Button>
                    <Button
                        variant="outlined"
                        onClick={handleExportExcel}
                        disabled={!report || isFetching || exporting}
                    >
                        {exporting ? "جاري التصدير..." : "تصدير Excel"}
                    </Button>
                    <Button
                        variant="outlined"
                        onClick={handleExportPdf}
                        disabled={!report || isFetching || exportingPdf}
                    >
                        {exportingPdf ? "جاري التصدير..." : "تصدير PDF"}
                    </Button>
                </Box>
            </Paper>

            {isError && (
                <Typography color="error" sx={{ mb: 2 }}>
                    تعذّر تحميل التقرير. حاول مرة أخرى.
                </Typography>
            )}

            {isFetching && !report && (
                <LoadingComponent message={getTranslatedLabel("project.projects.report.loading", "جاري تحميل التقرير...")} />
            )}

            {report?.summary && <SummaryRibbon s={report.summary} />}

            {report && (
                <>
                    <StyledTabs
                        value={tab}
                        onChange={(_e: React.SyntheticEvent, v: number) => setTab(v)}
                        variant="scrollable"
                        scrollButtons="auto"
                    >
                        {sections.map((sec, i) => (
                            <StyledTab
                                key={sec.label}
                                label={`${sec.label}${sec.rows ? ` (${sec.rows.length})` : ""}`}
                                value={i}
                            />
                        ))}
                    </StyledTabs>
                    {sections.map((sec, i) =>
                        tab === i ? (
                            <SectionGrid
                                key={sec.label}
                                rows={sec.rows}
                                columns={sec.columns}
                                emptyText="لا توجد بيانات للفترة المحددة."
                            />
                        ) : null
                    )}
                </>
            )}
        </Paper>
    );
}
