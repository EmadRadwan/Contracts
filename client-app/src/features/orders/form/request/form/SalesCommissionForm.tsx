import { useEffect, useMemo, useState } from "react";
import {
    Alert,
    Box,
    Button,
    Checkbox,
    FormControlLabel,
    Grid,
    IconButton,
    Paper,
    Tooltip,
    Typography,
} from "@mui/material";
import AddCircleOutlineIcon from "@mui/icons-material/AddCircleOutline";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import FormInput from "../../../../../app/common/form/FormInput";
import { MemoizedFormDropDownList } from "../../../../../app/common/form/MemoizedFormDropDownList";
import { FormComboBoxVirtualSalesRequest } from "../../../../../app/common/form/FormComboBoxVirtualSalesRequest";
import { requiredValidator } from "../../../../../app/common/form/Validators";
import { toast } from "react-toastify";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import { SalesCommission, CommissionRateDefaults } from "../../../../../app/models/orders/salesCommission";
import {
    useCreateSalesCommissionMutation,
    useUpdateSalesCommissionMutation,
    useApproveSalesCommissionMutation,
    useGetSalesCommissionDefaultsQuery,
    useGetSalesCommissionBySalesRequestQuery,
} from "../../../../../app/store/apis/salesCommissionsApi";
import { FormComboBoxVirtualPartySalesRep } from "../../../../../app/common/form/FormComboBoxVirtualPartySalesRep";
import { FormComboBoxVirtualPartySalesManager } from "../../../../../app/common/form/FormComboBoxVirtualPartySalesManager";
import { FormComboBoxVirtualPartyBroker } from "../../../../../app/common/form/FormComboBoxVirtualPartyBroker";
import QuickCreatePartyDialog, { PartyRole } from "../../../../../app/common/form/QuickCreatePartyDialog";
import SalesRequestMenu from "../menu/SalesRequestMenu";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";

function getThresholdStatus(ratio: number, saleTypeId: string | null) {
    const lower = saleTypeId === "COMM_SALE_INDIRECT" ? 0.075 : 0.05;
    if (ratio < lower) return { status: "blocked" as const, factor: 0, lower };
    if (ratio < 0.10) return { status: "partial" as const, factor: 0.5, lower };
    return { status: "full" as const, factor: 1, lower };
}

const saleTypeOptions = [
    { saleTypeId: "COMM_SALE_DIRECT", description: "بيع مباشر" },
    { saleTypeId: "COMM_SALE_PERSONAL", description: "بيع شخصي" },
    { saleTypeId: "COMM_SALE_INDIRECT", description: "بيع غير مباشر" },
];

interface Props {
    commission?: SalesCommission;
    salesRequestId?: string;
    editMode: 0 | 1 | 2;
    cancelEdit: () => void;
}

interface QuickCreateState {
    open: boolean;
    role: PartyRole;
    fieldName: string;
}

export default function SalesCommissionForm({ commission, salesRequestId, editMode, cancelEdit }: Props) {
    const [createCommission] = useCreateSalesCommissionMutation();
    const [updateCommission] = useUpdateSalesCommissionMutation();
    const [approveCommission, { isLoading: isApproving }] = useApproveSalesCommissionMutation();
    const [buttonFlag, setButtonFlag] = useState(false);
    const [hasTwoSalesReps, setHasTwoSalesReps] = useState(false);
    const [hasTwoManagers, setHasTwoManagers] = useState(false);
    const [hasVatExemption, setHasVatExemption] = useState(false);
    const [hasWithholdingTaxExemption, setHasWithholdingTaxExemption] = useState(false);
    const [quickCreate, setQuickCreate] = useState<QuickCreateState>({ open: false, role: "SALES_REP", fieldName: "" });
    const [formApi, setFormApi] = useState<any>(null);
    const [selectedSrId, setSelectedSrId] = useState<string | null>(null);
    const [selectedSaleTypeId, setSelectedSaleTypeId] = useState<string | null>(null);

    const { getTranslatedLabel } = useTranslationHelper();

    const resolvedSalesRequestId = editMode === 2 ? commission?.salesRequestId : salesRequestId;

    // Effective SR ID: pre-set prop takes priority, then user's combobox selection
    const effectiveSrId = resolvedSalesRequestId ?? selectedSrId ?? undefined;

    // In edit mode use the commission's saved saleTypeId until the user changes the dropdown
    const effectiveSaleTypeId = selectedSaleTypeId ?? commission?.saleTypeId ?? null;

    const { data: defaults } = useGetSalesCommissionDefaultsQuery(
        { salesRequestId: effectiveSrId!, saleTypeId: effectiveSaleTypeId },
        { skip: !effectiveSrId }
    );

    const { data: existingCommission } = useGetSalesCommissionBySalesRequestQuery(resolvedSalesRequestId!, {
        skip: !resolvedSalesRequestId || editMode === 2,
    });

    useEffect(() => {
        if (existingCommission && editMode === 1) {
            toast.info("يوجد سجل عمولة لهذا الطلب - تم تحميله للتعديل");
        }
    }, [existingCommission, editMode]);

    const activeCommission = editMode === 2 ? commission : existingCommission ?? undefined;

    useEffect(() => {
        if (activeCommission) {
            setHasTwoSalesReps(!!activeCommission.salesRep2PartyId);
            setHasTwoManagers(!!activeCommission.manager2PartyId);
            setHasVatExemption(activeCommission.hasVatExemption ?? false);
            setHasWithholdingTaxExemption(activeCommission.hasWithholdingTaxExemption ?? false);
        }
    }, [activeCommission]);

    // Auto-populate percent fields when the user explicitly picks an SR or changes the sale type
    useEffect(() => {
        if (!formApi || !defaults || !defaults.rates || selectedSaleTypeId === null) return;
        const rate = defaults.rates[selectedSaleTypeId];
        if (!rate) return;

        const repPercent = hasTwoSalesReps
            ? Math.round(rate.salesRepPercent / 2 * 10000) / 10000
            : rate.salesRepPercent;
        const mgrPercent = hasTwoManagers
            ? Math.round(rate.managerPercent / 2 * 10000) / 10000
            : rate.managerPercent;

        formApi.onChange("salesRepPercent", { value: repPercent });
        if (hasTwoSalesReps) formApi.onChange("salesRep2Percent", { value: repPercent });
        formApi.onChange("managerPercent", { value: mgrPercent });
        if (hasTwoManagers) formApi.onChange("manager2Percent", { value: mgrPercent });
        if (rate.externalCompanyPercent != null)
            formApi.onChange("externalCompanyPercent", { value: rate.externalCompanyPercent });
        if (rate.externalSalesRepPercent != null)
            formApi.onChange("externalSalesRepPercent", { value: rate.externalSalesRepPercent });
        if (rate.externalManagerPercent != null)
            formApi.onChange("externalManagerPercent", { value: rate.externalManagerPercent });
    }, [defaults, selectedSaleTypeId, formApi, hasTwoSalesReps, hasTwoManagers]);

    const initialValues = useMemo(() => {
        const srId = resolvedSalesRequestId;
        const srIdItem = srId ? { salesRequestId: srId, label: srId } : null;
        if (activeCommission) {
            return {
                salesRequestId: srIdItem,
                saleTypeId: activeCommission.saleTypeId,
                salesRepParty: activeCommission.salesRepPartyId
                    ? { fromPartyId: activeCommission.salesRepPartyId, fromPartyName: activeCommission.salesRepName ?? "" }
                    : null,
                salesRepPercent: activeCommission.salesRepPercent,
                salesRep2Party: activeCommission.salesRep2PartyId
                    ? { fromPartyId: activeCommission.salesRep2PartyId, fromPartyName: activeCommission.salesRep2Name ?? "" }
                    : null,
                salesRep2Percent: activeCommission.salesRep2Percent ?? null,
                managerParty: activeCommission.managerPartyId
                    ? { fromPartyId: activeCommission.managerPartyId, fromPartyName: activeCommission.managerName ?? "" }
                    : null,
                managerPercent: activeCommission.managerPercent,
                manager2Party: activeCommission.manager2PartyId
                    ? { fromPartyId: activeCommission.manager2PartyId, fromPartyName: activeCommission.manager2Name ?? "" }
                    : null,
                manager2Percent: activeCommission.manager2Percent ?? null,
                externalCompanyParty: activeCommission.externalCompanyPartyId
                    ? { fromPartyId: activeCommission.externalCompanyPartyId, fromPartyName: activeCommission.externalCompanyName ?? "" }
                    : null,
                externalCompanyPercent: activeCommission.externalCompanyPercent ?? null,
                externalSalesRepParty: activeCommission.externalSalesRepPartyId
                    ? { fromPartyId: activeCommission.externalSalesRepPartyId, fromPartyName: activeCommission.externalSalesRepName ?? "" }
                    : null,
                externalSalesRepPercent: activeCommission.externalSalesRepPercent ?? null,
                externalManagerParty: activeCommission.externalManagerPartyId
                    ? { fromPartyId: activeCommission.externalManagerPartyId, fromPartyName: activeCommission.externalManagerName ?? "" }
                    : null,
                externalManagerPercent: activeCommission.externalManagerPercent ?? null,
                vatPercent: activeCommission.vatPercent,
                withholdingTaxPercent: activeCommission.withholdingTaxPercent,
                notes: activeCommission.notes ?? "",
            };
        }
        return {
            salesRequestId: srIdItem,
            saleTypeId: null,
            salesRepParty: null,
            salesRepPercent: 0,
            salesRep2Party: null,
            salesRep2Percent: null,
            managerParty: null,
            managerPercent: 0,
            manager2Party: null,
            manager2Percent: null,
            externalCompanyParty: null,
            externalCompanyPercent: null,
            externalSalesRepParty: null,
            externalSalesRepPercent: null,
            externalManagerParty: null,
            externalManagerPercent: null,
            vatPercent: 14,
            withholdingTaxPercent: 5,
            notes: "",
        };
    }, [activeCommission, defaults, resolvedSalesRequestId]);

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        const isIndirect = data.saleTypeId === "COMM_SALE_INDIRECT";
        const srId = data.salesRequestId?.salesRequestId ?? null;

        const payload: Partial<SalesCommission> = {
            salesRequestId: srId,
            saleTypeId: data.saleTypeId,
            salesRepPartyId: data.salesRepParty?.fromPartyId ?? null,
            salesRepPercent: data.salesRepPercent ?? 0,
            managerPartyId: data.managerParty?.fromPartyId ?? null,
            managerPercent: data.managerPercent ?? 0,
            salesRep2PartyId: hasTwoSalesReps ? (data.salesRep2Party?.fromPartyId ?? null) : null,
            salesRep2Percent: hasTwoSalesReps ? (data.salesRep2Percent ?? null) : null,
            manager2PartyId: hasTwoManagers ? (data.manager2Party?.fromPartyId ?? null) : null,
            manager2Percent: hasTwoManagers ? (data.manager2Percent ?? null) : null,
            externalCompanyPartyId: isIndirect ? (data.externalCompanyParty?.fromPartyId ?? null) : null,
            externalCompanyPercent: isIndirect ? (data.externalCompanyPercent ?? null) : null,
            externalSalesRepPartyId: isIndirect ? (data.externalSalesRepParty?.fromPartyId ?? null) : null,
            externalSalesRepPercent: isIndirect ? (data.externalSalesRepPercent ?? null) : null,
            externalManagerPartyId: isIndirect ? (data.externalManagerParty?.fromPartyId ?? null) : null,
            externalManagerPercent: isIndirect ? (data.externalManagerPercent ?? null) : null,
            hasVatExemption,
            hasWithholdingTaxExemption,
            vatPercent: hasVatExemption ? 0 : (data.vatPercent ?? 14),
            withholdingTaxPercent: hasWithholdingTaxExemption ? 0 : (data.withholdingTaxPercent ?? 5),
            notes: data.notes ?? null,
        };

        try {
            if (activeCommission?.salesCommissionId) {
                await updateCommission({ id: activeCommission.salesCommissionId, dto: payload }).unwrap();
                toast.success(getTranslatedLabel("salesCommission.form.updateSuccess", "تم تحديث العمولة بنجاح"));
            } else {
                await createCommission(payload).unwrap();
                toast.success(getTranslatedLabel("salesCommission.form.createSuccess", "تم إنشاء العمولة بنجاح"));
            }
            cancelEdit();
        } catch {
            toast.error(getTranslatedLabel("salesCommission.form.error", "فشل في حفظ العمولة"));
        } finally {
            setButtonFlag(false);
        }
    }

    async function handleApprove() {
        if (!activeCommission?.salesCommissionId) return;
        try {
            await approveCommission(activeCommission.salesCommissionId).unwrap();
            toast.success(getTranslatedLabel("salesCommission.form.approveSuccess", "تم اعتماد العمولة بنجاح"));
            cancelEdit();
        } catch {
            toast.error(getTranslatedLabel("salesCommission.form.error", "فشل في حفظ العمولة"));
        }
    }

    function openQuickCreate(role: PartyRole, fieldName: string) {
        setQuickCreate({ open: true, role, fieldName });
    }

    function handlePartyCreated(party: { fromPartyId: string; fromPartyName: string }) {
        if (formApi) {
            formApi.onChange(quickCreate.fieldName, { value: party });
        }
        setQuickCreate(q => ({ ...q, open: false }));
    }

    const isApproved = activeCommission?.statusId === "COMMISSION_APPROVED";
    const isPending = activeCommission?.statusId === "COMMISSION_PENDING";

    return (
        <>
            <SalesRequestMenu selectedMenuItem="sales-commissions" />
            <Paper elevation={5} className="div-container-withBorderCurved" style={{ padding: "16px" }}>
                <Typography variant="h4" color={editMode === 1 ? "green" : "black"} sx={{ mb: 2 }}>
                    {editMode === 1
                        ? getTranslatedLabel("salesCommission.form.new", "عمولة مبيعات جديدة")
                        : getTranslatedLabel("salesCommission.form.edit", "تعديل العمولة")}
                    {activeCommission?.salesCommissionId && (
                        <Typography component="span" variant="h5" color="grey" sx={{ ml: 1 }}>
                            ({activeCommission.salesCommissionId})
                        </Typography>
                    )}
                </Typography>

                <Form
                    key={activeCommission?.salesCommissionId ?? resolvedSalesRequestId ?? "new"}
                    initialValues={initialValues}
                    onSubmit={handleSubmitData}
                    render={(formRenderProps) => {
                        if (!formApi) setFormApi(formRenderProps);
                        const currentSaleTypeId: string | null = formRenderProps.valueGetter("saleTypeId") ?? activeCommission?.saleTypeId ?? null;
                        const isIndirect = currentSaleTypeId === "COMM_SALE_INDIRECT";
                        const threshold = getThresholdStatus(defaults?.collectedRatio ?? 0, currentSaleTypeId);
                        const activeRate: CommissionRateDefaults | null = currentSaleTypeId ? (defaults?.rates?.[currentSaleTypeId] ?? null) : null;
                        const salePrice = defaults?.salePrice ?? activeCommission?.salePrice ?? 0;
                        const factor = threshold.status === "full" ? 1 : threshold.status === "partial" ? 0.5 : 0;

                        const maxRepPct = activeRate
                            ? (hasTwoSalesReps ? activeRate.salesRepPercent / 2 : activeRate.salesRepPercent)
                            : undefined;
                        const maxMgrPct = activeRate
                            ? (hasTwoManagers ? activeRate.managerPercent / 2 : activeRate.managerPercent)
                            : undefined;
                        const maxExtPct = activeRate?.externalCompanyPercent ?? undefined;

                        const repValidator = maxRepPct !== undefined
                            ? (v: any) => (v ?? 0) > maxRepPct ? `الحد الأقصى ${maxRepPct.toFixed(4)}%` : undefined
                            : undefined;
                        const mgrValidator = maxMgrPct !== undefined
                            ? (v: any) => (v ?? 0) > maxMgrPct ? `الحد الأقصى ${maxMgrPct.toFixed(4)}%` : undefined
                            : undefined;
                        const extValidator = maxExtPct !== undefined
                            ? (v: any) => (v ?? 0) > maxExtPct ? `الحد الأقصى ${maxExtPct.toFixed(4)}%` : undefined
                            : undefined;

                        return (
                            <FormElement>
                                <fieldset className="k-form-fieldset">
                                    {/* Sales Request */}
                                    <Grid container spacing={2} sx={{ mb: 2 }}>
                                        <Grid item xs={12} md={5}>
                                            <Field
                                                id="salesRequestId"
                                                name="salesRequestId"
                                                label={getTranslatedLabel("salesCommission.form.salesRequest", "طلب البيع")}
                                                component={FormComboBoxVirtualSalesRequest}
                                                validator={requiredValidator}
                                                disabled={!!resolvedSalesRequestId || isApproved}
                                                onSalesRequestIdChange={setSelectedSrId}
                                            />
                                        </Grid>
                                        <Grid item xs={6} md={3}>
                                            <Field
                                                id="saleTypeId"
                                                name="saleTypeId"
                                                label={getTranslatedLabel("salesCommission.form.saleType", "نوع البيع")}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey="saleTypeId"
                                                textField="description"
                                                data={saleTypeOptions}
                                                validator={requiredValidator}
                                                disabled={isApproved}
                                                onAfterChange={setSelectedSaleTypeId}
                                            />
                                        </Grid>
                                    </Grid>

                                    {/* Sale Price / Collected Amount — read-only, derived from SR and payments */}
                                    {(defaults || activeCommission) && (() => {
                                        const salePrice = defaults?.salePrice ?? activeCommission?.salePrice ?? 0;
                                        const collected = defaults?.collectedAmount ?? activeCommission?.collectedAmount ?? 0;
                                        const ratio = salePrice > 0 ? collected / salePrice : 0;
                                        const ratioColor = ratio >= 0.10 ? "green" : ratio >= 0.05 ? "orange" : "red";
                                        return (
                                            <Grid container spacing={2} sx={{ mb: 2, p: 1, background: "#f9f9f9", borderRadius: 1 }}>
                                                <Grid item xs={6} md={3}>
                                                    <Typography variant="caption" color="text.secondary" display="block">
                                                        {getTranslatedLabel("salesCommission.form.salePrice", "سعر البيع")}
                                                    </Typography>
                                                    <Typography variant="body1" fontWeight="bold">
                                                        {salePrice.toLocaleString("ar-SA", { minimumFractionDigits: 2 })}
                                                    </Typography>
                                                </Grid>
                                                <Grid item xs={6} md={3}>
                                                    <Typography variant="caption" color="text.secondary" display="block">
                                                        {getTranslatedLabel("salesCommission.form.collectedAmount", "المبلغ المحصل")}
                                                    </Typography>
                                                    <Typography variant="body1" fontWeight="bold">
                                                        {collected.toLocaleString("ar-SA", { minimumFractionDigits: 2 })}
                                                    </Typography>
                                                </Grid>
                                                <Grid item xs={6} md={2}>
                                                    <Typography variant="caption" color="text.secondary" display="block">
                                                        {getTranslatedLabel("salesCommission.form.collectedRatio", "نسبة التحصيل")}
                                                    </Typography>
                                                    <Typography variant="body1" fontWeight="bold" color={ratioColor}>
                                                        {(ratio * 100).toLocaleString("ar-SA", { minimumFractionDigits: 2 })}%
                                                    </Typography>
                                                </Grid>
                                            </Grid>
                                        );
                                    })()}

                                    {/* Configured project commission rates — shown as info Alert once SR + sale type are chosen */}
                                    {effectiveSrId && defaults && currentSaleTypeId && !activeRate && (
                                        <Alert severity="error" sx={{ mb: 2 }}>
                                            {getTranslatedLabel(
                                                "salesCommission.form.noRateConfigured",
                                                "لا توجد أسعار عمولة مُعدَّة للمشروع لنوع البيع المحدد — لا يمكن إنشاء العمولة"
                                            )}
                                        </Alert>
                                    )}

                                    {activeRate && (
                                        <Alert severity="info" sx={{ mb: 2, py: 0.5 }}>
                                            <Box sx={{ display: "flex", flexWrap: "wrap", alignItems: "center", gap: "12px 24px" }}>
                                                <Typography variant="caption" fontWeight="bold" sx={{ whiteSpace: "nowrap" }}>
                                                    {getTranslatedLabel("salesCommission.form.configuredRates", "الحدود المقررة للمشروع:")}
                                                </Typography>
                                                <Typography variant="caption" sx={{ whiteSpace: "nowrap" }}>
                                                    {`المندوب: ${activeRate.salesRepPercent}%`}
                                                    {hasTwoSalesReps && ` (${(activeRate.salesRepPercent / 2).toFixed(4)}% × 2)`}
                                                    {` ← ${(salePrice * activeRate.salesRepPercent / 100 * factor).toLocaleString("ar-SA", { maximumFractionDigits: 2 })}`}
                                                </Typography>
                                                <Typography variant="caption" sx={{ whiteSpace: "nowrap" }}>
                                                    {`المدير: ${activeRate.managerPercent}%`}
                                                    {hasTwoManagers && ` (${(activeRate.managerPercent / 2).toFixed(4)}% × 2)`}
                                                    {` ← ${(salePrice * activeRate.managerPercent / 100 * factor).toLocaleString("ar-SA", { maximumFractionDigits: 2 })}`}
                                                </Typography>
                                                {isIndirect && activeRate.externalCompanyPercent != null && (
                                                    <Typography variant="caption" sx={{ whiteSpace: "nowrap" }}>
                                                        {`الوسيط: ${activeRate.externalCompanyPercent}%`}
                                                        {` ← ${(salePrice * activeRate.externalCompanyPercent / 100 * factor).toLocaleString("ar-SA", { maximumFractionDigits: 2 })}`}
                                                    </Typography>
                                                )}
                                            </Box>
                                        </Alert>
                                    )}

                                    {/* Threshold status alert — shown once an SR is chosen */}
                                    {effectiveSrId && defaults && (
                                        <Alert
                                            severity={
                                                threshold.status === "blocked" ? "error"
                                                : threshold.status === "partial" ? "warning"
                                                : "success"
                                            }
                                            sx={{ mb: 2 }}
                                        >
                                            {threshold.status === "blocked" && (
                                                <>
                                                    {`لا يمكن إنشاء العمولة — نسبة التحصيل (${((defaults?.collectedRatio ?? 0) * 100).toFixed(2)}%) أقل من الحد الأدنى المطلوب (${(threshold.lower * 100).toFixed(1)}%)`}
                                                </>
                                            )}
                                            {threshold.status === "partial" && (
                                                <>
                                                    {`يُسمح بنصف العمولة فقط (50%) — نسبة التحصيل الحالية ${((defaults?.collectedRatio ?? 0) * 100).toFixed(2)}% (بين ${(threshold.lower * 100).toFixed(1)}% و10%)`}
                                                </>
                                            )}
                                            {threshold.status === "full" && (
                                                <>
                                                    {getTranslatedLabel("salesCommission.form.thresholdFull", "نسبة التحصيل كافية — يُسمح بالعمولة الكاملة (100%)")}
                                                </>
                                            )}
                                        </Alert>
                                    )}

                                    {/* Internal Parties Section */}
                                    <Typography variant="h6" sx={{ mt: 2, mb: 1, fontWeight: "bold" }}>
                                        {getTranslatedLabel("salesCommission.form.internalSection", "العمولات الداخلية")}
                                    </Typography>

                                    {/* Sales Rep 1 */}
                                    <Grid container spacing={2} alignItems="center" sx={{ mb: 1 }}>
                                        <Grid item xs={12} md={4}>
                                            <Grid container alignItems="center">
                                                <Grid item xs={11}>
                                                    <Field
                                                        id="salesRepParty"
                                                        name="salesRepParty"
                                                        label={getTranslatedLabel("salesCommission.form.salesRep1", "المندوب الأول")}
                                                        component={FormComboBoxVirtualPartySalesRep}
                                                        disabled={isApproved}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>
                                                <Grid item xs={1}>
                                                    {!isApproved && isIndirect && (
                                                        <Tooltip title="إنشاء مندوب جديد">
                                                            <IconButton size="small" onClick={() => openQuickCreate("SALES_REP", "salesRepParty")}>
                                                                <AddCircleOutlineIcon fontSize="small" />
                                                            </IconButton>
                                                        </Tooltip>
                                                    )}
                                                </Grid>
                                            </Grid>
                                        </Grid>
                                        <Grid item xs={6} md={2}>
                                            <Field
                                                id="salesRepPercent"
                                                name="salesRepPercent"
                                                label={getTranslatedLabel("salesCommission.form.salesRep1Percent", "نسبة المندوب الأول %")}
                                                component={FormNumericTextBox}
                                                min={0} max={100} format="n4"
                                                disabled={isApproved}
                                                validator={repValidator ? [repValidator] : undefined}
                                            />
                                        </Grid>
                                        {activeCommission?.salesRepAmount && (
                                            <Grid item xs={6} md={2}>
                                                <Typography variant="body2" sx={{ mt: 3 }}>
                                                    {getTranslatedLabel("salesCommission.form.salesRep1Amount", "مبلغ المندوب الأول")}: {activeCommission.salesRepAmount?.toLocaleString("ar-SA", { minimumFractionDigits: 2 })}
                                                </Typography>
                                            </Grid>
                                        )}
                                        <Grid item xs={12} md={2}>
                                            {!isApproved && (
                                                <FormControlLabel
                                                    control={
                                                        <Checkbox
                                                            checked={hasTwoSalesReps}
                                                            onChange={(e) => setHasTwoSalesReps(e.target.checked)}
                                                            size="small"
                                                        />
                                                    }
                                                    label="مندوب ثاني"
                                                />
                                            )}
                                        </Grid>
                                    </Grid>

                                    {/* Sales Rep 2 */}
                                    {hasTwoSalesReps && (
                                        <Grid container spacing={2} alignItems="center" sx={{ mb: 1 }}>
                                            <Grid item xs={12} md={4}>
                                                <Grid container alignItems="center">
                                                    <Grid item xs={11}>
                                                        <Field
                                                            id="salesRep2Party"
                                                            name="salesRep2Party"
                                                            label={getTranslatedLabel("salesCommission.form.salesRep2", "المندوب الثاني")}
                                                            component={FormComboBoxVirtualPartySalesRep}
                                                            disabled={isApproved}
                                                            validator={requiredValidator}
                                                        />
                                                    </Grid>
                                                    <Grid item xs={1}>
                                                        {!isApproved && isIndirect && (
                                                            <Tooltip title="إنشاء مندوب جديد">
                                                                <IconButton size="small" onClick={() => openQuickCreate("SALES_REP", "salesRep2Party")}>
                                                                    <AddCircleOutlineIcon fontSize="small" />
                                                                </IconButton>
                                                            </Tooltip>
                                                        )}
                                                    </Grid>
                                                </Grid>
                                            </Grid>
                                            <Grid item xs={6} md={2}>
                                                <Field
                                                    id="salesRep2Percent"
                                                    name="salesRep2Percent"
                                                    label={getTranslatedLabel("salesCommission.form.salesRep2Percent", "نسبة المندوب الثاني %")}
                                                    component={FormNumericTextBox}
                                                    min={0} max={100} format="n4"
                                                    disabled={isApproved}
                                                    validator={repValidator ? [repValidator] : undefined}
                                                />
                                            </Grid>
                                        </Grid>
                                    )}

                                    {/* Manager 1 */}
                                    <Grid container spacing={2} alignItems="center" sx={{ mb: 1 }}>
                                        <Grid item xs={12} md={4}>
                                            <Grid container alignItems="center">
                                                <Grid item xs={11}>
                                                    <Field
                                                        id="managerParty"
                                                        name="managerParty"
                                                        label={getTranslatedLabel("salesCommission.form.manager1", "المدير الأول")}
                                                        component={FormComboBoxVirtualPartySalesManager}
                                                        disabled={isApproved}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>
                                                <Grid item xs={1}>
                                                    {!isApproved && isIndirect && (
                                                        <Tooltip title="إنشاء مدير جديد">
                                                            <IconButton size="small" onClick={() => openQuickCreate("SALES_MANAGER", "managerParty")}>
                                                                <AddCircleOutlineIcon fontSize="small" />
                                                            </IconButton>
                                                        </Tooltip>
                                                    )}
                                                </Grid>
                                            </Grid>
                                        </Grid>
                                        <Grid item xs={6} md={2}>
                                            <Field
                                                id="managerPercent"
                                                name="managerPercent"
                                                label={getTranslatedLabel("salesCommission.form.manager1Percent", "نسبة المدير الأول %")}
                                                component={FormNumericTextBox}
                                                min={0} max={100} format="n4"
                                                disabled={isApproved}
                                                validator={mgrValidator ? [mgrValidator] : undefined}
                                            />
                                        </Grid>
                                        {activeCommission?.managerAmount && (
                                            <Grid item xs={6} md={2}>
                                                <Typography variant="body2" sx={{ mt: 3 }}>
                                                    {getTranslatedLabel("salesCommission.form.manager1Amount", "مبلغ المدير الأول")}: {activeCommission.managerAmount?.toLocaleString("ar-SA", { minimumFractionDigits: 2 })}
                                                </Typography>
                                            </Grid>
                                        )}
                                        <Grid item xs={12} md={2}>
                                            {!isApproved && (
                                                <FormControlLabel
                                                    control={
                                                        <Checkbox
                                                            checked={hasTwoManagers}
                                                            onChange={(e) => setHasTwoManagers(e.target.checked)}
                                                            size="small"
                                                        />
                                                    }
                                                    label="مدير ثاني"
                                                />
                                            )}
                                        </Grid>
                                    </Grid>

                                    {/* Manager 2 */}
                                    {hasTwoManagers && (
                                        <Grid container spacing={2} alignItems="center" sx={{ mb: 1 }}>
                                            <Grid item xs={12} md={4}>
                                                <Grid container alignItems="center">
                                                    <Grid item xs={11}>
                                                        <Field
                                                            id="manager2Party"
                                                            name="manager2Party"
                                                            label={getTranslatedLabel("salesCommission.form.manager2", "المدير الثاني")}
                                                            component={FormComboBoxVirtualPartySalesManager}
                                                            disabled={isApproved}
                                                            validator={requiredValidator}
                                                        />
                                                    </Grid>
                                                    <Grid item xs={1}>
                                                        {!isApproved && isIndirect && (
                                                            <Tooltip title="إنشاء مدير جديد">
                                                                <IconButton size="small" onClick={() => openQuickCreate("SALES_MANAGER", "manager2Party")}>
                                                                    <AddCircleOutlineIcon fontSize="small" />
                                                                </IconButton>
                                                            </Tooltip>
                                                        )}
                                                    </Grid>
                                                </Grid>
                                            </Grid>
                                            <Grid item xs={6} md={2}>
                                                <Field
                                                    id="manager2Percent"
                                                    name="manager2Percent"
                                                    label={getTranslatedLabel("salesCommission.form.manager2Percent", "نسبة المدير الثاني %")}
                                                    component={FormNumericTextBox}
                                                    min={0} max={100} format="n4"
                                                    disabled={isApproved}
                                                    validator={mgrValidator ? [mgrValidator] : undefined}
                                                />
                                            </Grid>
                                        </Grid>
                                    )}

                                    {/* External Section — INDIRECT only */}
                                    {isIndirect && (
                                        <>
                                            <Typography variant="h6" sx={{ mt: 2, mb: 1, fontWeight: "bold" }}>
                                                {getTranslatedLabel("salesCommission.form.externalSection", "العمولات الخارجية (الوسيط)")}
                                            </Typography>

                                            {/* Tax flags */}
                                            <Grid container spacing={2} sx={{ mb: 1 }}>
                                                <Grid item>
                                                    <FormControlLabel
                                                        control={
                                                            <Checkbox
                                                                checked={hasVatExemption}
                                                                onChange={(e) => setHasVatExemption(e.target.checked)}
                                                                disabled={isApproved}
                                                            />
                                                        }
                                                        label={getTranslatedLabel("salesCommission.form.hasVatExemption", "إعفاء من ضريبة القيمة المضافة")}
                                                    />
                                                </Grid>
                                                <Grid item>
                                                    <FormControlLabel
                                                        control={
                                                            <Checkbox
                                                                checked={hasWithholdingTaxExemption}
                                                                onChange={(e) => setHasWithholdingTaxExemption(e.target.checked)}
                                                                disabled={isApproved}
                                                            />
                                                        }
                                                        label={getTranslatedLabel("salesCommission.form.hasWithholdingTaxExemption", "إعفاء من ضريبة الاستقطاع")}
                                                    />
                                                </Grid>
                                                {!hasVatExemption && (
                                                    <Grid item xs={6} md={2}>
                                                        <Field
                                                            id="vatPercent"
                                                            name="vatPercent"
                                                            label={getTranslatedLabel("salesCommission.form.vatPercent", "نسبة ضريبة القيمة المضافة %")}
                                                            component={FormNumericTextBox}
                                                            min={0} max={100} format="n2"
                                                            disabled={isApproved}
                                                        />
                                                    </Grid>
                                                )}
                                                {!hasWithholdingTaxExemption && (
                                                    <Grid item xs={6} md={2}>
                                                        <Field
                                                            id="withholdingTaxPercent"
                                                            name="withholdingTaxPercent"
                                                            label={getTranslatedLabel("salesCommission.form.withholdingTaxPercent", "نسبة ضريبة الاستقطاع %")}
                                                            component={FormNumericTextBox}
                                                            min={0} max={100} format="n2"
                                                            disabled={isApproved}
                                                        />
                                                    </Grid>
                                                )}
                                            </Grid>

                                            {/* External Company */}
                                            <Grid container spacing={2} alignItems="center" sx={{ mb: 1 }}>
                                                <Grid item xs={12} md={4}>
                                                    <Grid container alignItems="center">
                                                        <Grid item xs={11}>
                                                            <Field
                                                                id="externalCompanyParty"
                                                                name="externalCompanyParty"
                                                                label={getTranslatedLabel("salesCommission.form.externalCompany", "شركة الوسيط")}
                                                                component={FormComboBoxVirtualPartyBroker}
                                                                disabled={isApproved}
                                                            />
                                                        </Grid>
                                                        <Grid item xs={1}>
                                                            {!isApproved && (
                                                                <Tooltip title="إنشاء وسيط جديد">
                                                                    <IconButton size="small" onClick={() => openQuickCreate("BROKER", "externalCompanyParty")}>
                                                                        <AddCircleOutlineIcon fontSize="small" />
                                                                    </IconButton>
                                                                </Tooltip>
                                                            )}
                                                        </Grid>
                                                    </Grid>
                                                </Grid>
                                                <Grid item xs={6} md={2}>
                                                    <Field
                                                        id="externalCompanyPercent"
                                                        name="externalCompanyPercent"
                                                        label={getTranslatedLabel("salesCommission.form.externalCompanyPercent", "نسبة الوسيط %")}
                                                        component={FormNumericTextBox}
                                                        min={0} max={100} format="n4"
                                                        disabled={isApproved}
                                                        validator={extValidator ? [extValidator] : undefined}
                                                    />
                                                </Grid>
                                                {activeCommission?.externalCompanyGrossAmount && (
                                                    <Grid item xs={6} md={3}>
                                                        <Typography variant="body2" sx={{ mt: 3 }}>
                                                            {getTranslatedLabel("salesCommission.form.externalCompanyGross", "إجمالي الوسيط")}: {activeCommission.externalCompanyGrossAmount?.toLocaleString("ar-SA", { minimumFractionDigits: 2 })}
                                                            {" / "}
                                                            {getTranslatedLabel("salesCommission.form.externalCompanyNet", "صافي الوسيط")}: {activeCommission.externalCompanyNetAmount?.toLocaleString("ar-SA", { minimumFractionDigits: 2 })}
                                                        </Typography>
                                                    </Grid>
                                                )}
                                            </Grid>

                                            {/* External Sales Rep */}
                                            <Grid container spacing={2} alignItems="center" sx={{ mb: 1 }}>
                                                <Grid item xs={12} md={4}>
                                                    <Grid container alignItems="center">
                                                        <Grid item xs={11}>
                                                            <Field
                                                                id="externalSalesRepParty"
                                                                name="externalSalesRepParty"
                                                                label={getTranslatedLabel("salesCommission.form.externalSalesRep", "مندوب الوسيط")}
                                                                component={FormComboBoxVirtualPartySalesRep}
                                                                disabled={isApproved}
                                                            />
                                                        </Grid>
                                                        <Grid item xs={1}>
                                                            {!isApproved && (
                                                                <Tooltip title="إنشاء مندوب جديد">
                                                                    <IconButton size="small" onClick={() => openQuickCreate("SALES_REP", "externalSalesRepParty")}>
                                                                        <AddCircleOutlineIcon fontSize="small" />
                                                                    </IconButton>
                                                                </Tooltip>
                                                            )}
                                                        </Grid>
                                                    </Grid>
                                                </Grid>
                                                <Grid item xs={6} md={2}>
                                                    <Field
                                                        id="externalSalesRepPercent"
                                                        name="externalSalesRepPercent"
                                                        label={getTranslatedLabel("salesCommission.form.externalSalesRepPercent", "نسبة مندوب الوسيط %")}
                                                        component={FormNumericTextBox}
                                                        min={0} max={100} format="n4"
                                                        disabled={isApproved}
                                                    />
                                                </Grid>
                                            </Grid>

                                            {/* External Manager */}
                                            <Grid container spacing={2} alignItems="center" sx={{ mb: 1 }}>
                                                <Grid item xs={12} md={4}>
                                                    <Grid container alignItems="center">
                                                        <Grid item xs={11}>
                                                            <Field
                                                                id="externalManagerParty"
                                                                name="externalManagerParty"
                                                                label={getTranslatedLabel("salesCommission.form.externalManager", "مدير الوسيط")}
                                                                component={FormComboBoxVirtualPartySalesManager}
                                                                disabled={isApproved}
                                                            />
                                                        </Grid>
                                                        <Grid item xs={1}>
                                                            {!isApproved && (
                                                                <Tooltip title="إنشاء مدير جديد">
                                                                    <IconButton size="small" onClick={() => openQuickCreate("SALES_MANAGER", "externalManagerParty")}>
                                                                        <AddCircleOutlineIcon fontSize="small" />
                                                                    </IconButton>
                                                                </Tooltip>
                                                            )}
                                                        </Grid>
                                                    </Grid>
                                                </Grid>
                                                <Grid item xs={6} md={2}>
                                                    <Field
                                                        id="externalManagerPercent"
                                                        name="externalManagerPercent"
                                                        label={getTranslatedLabel("salesCommission.form.externalManagerPercent", "نسبة مدير الوسيط %")}
                                                        component={FormNumericTextBox}
                                                        min={0} max={100} format="n4"
                                                        disabled={isApproved}
                                                    />
                                                </Grid>
                                            </Grid>
                                        </>
                                    )}

                                    {/* Notes */}
                                    <Grid container spacing={2} sx={{ mt: 1 }}>
                                        <Grid item xs={12} md={6}>
                                            <Field
                                                id="notes"
                                                name="notes"
                                                label={getTranslatedLabel("salesCommission.form.notes", "ملاحظات")}
                                                component={FormInput}
                                                disabled={isApproved}
                                            />
                                        </Grid>
                                    </Grid>

                                    {/* Action Buttons */}
                                    <div className="k-form-buttons" style={{ marginTop: "20px" }}>
                                        <Grid container spacing={1}>
                                            {!isApproved && (
                                                <Grid item>
                                                    <Button
                                                        type="submit"
                                                        color="success"
                                                        variant="contained"
                                                        disabled={
                                                            !formRenderProps.allowSubmit ||
                                                            buttonFlag ||
                                                            threshold.status === "blocked" ||
                                                            (!!effectiveSrId && !!defaults && !!currentSaleTypeId && !activeRate)
                                                        }
                                                    >
                                                        {getTranslatedLabel("salesCommission.form.submit", "حفظ")}
                                                    </Button>
                                                </Grid>
                                            )}
                                            {isPending && activeCommission?.salesCommissionId && (
                                                <Grid item>
                                                    <Button
                                                        color="primary"
                                                        variant="contained"
                                                        disabled={isApproving}
                                                        onClick={handleApprove}
                                                    >
                                                        {getTranslatedLabel("salesCommission.form.approve", "اعتماد")}
                                                    </Button>
                                                </Grid>
                                            )}
                                            <Grid item>
                                                <Button onClick={cancelEdit} variant="contained" color="error">
                                                    {getTranslatedLabel("salesCommission.form.cancel", "إلغاء")}
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </div>
                                    {buttonFlag && (
                                        <Grid container sx={{ mt: 1 }}>
                                            <Grid item xs={4}>
                                                <LoadingComponent
                                                    message={getTranslatedLabel("salesCommission.form.processing", "جاري المعالجة...")}
                                                />
                                            </Grid>
                                        </Grid>
                                    )}
                                </fieldset>
                            </FormElement>
                        );
                    }}
                />
            </Paper>

            <QuickCreatePartyDialog
                open={quickCreate.open}
                role={quickCreate.role}
                onClose={() => setQuickCreate(q => ({ ...q, open: false }))}
                onCreated={handlePartyCreated}
            />
        </>
    );
}
