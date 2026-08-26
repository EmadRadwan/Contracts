import { ComponentType, useEffect, useMemo, useState } from "react";
import {
    Alert,
    Box,
    Button,
    Checkbox,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControlLabel,
    Grid,
    IconButton,
    Paper,
    Tooltip,
    Typography,
} from "@mui/material";
import InfoOutlinedIcon from "@mui/icons-material/InfoOutlined";
import AddCircleOutlineIcon from "@mui/icons-material/AddCircleOutline";
import { Ribbon, RibbonContainer } from "react-ribbons";
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
    useGetSalesCommissionDefaultsQuery,
    useGetSalesCommissionBySalesRequestQuery,
} from "../../../../../app/store/apis/salesCommissionsApi";
import { SalesCommissionActionsMenu } from "../menu/SalesCommissionActionsMenu";
import { FormComboBoxVirtualPartySalesRep } from "../../../../../app/common/form/FormComboBoxVirtualPartySalesRep";
import { FormComboBoxVirtualPartySalesManager } from "../../../../../app/common/form/FormComboBoxVirtualPartySalesManager";
import { FormComboBoxVirtualPartyBroker } from "../../../../../app/common/form/FormComboBoxVirtualPartyBroker";
import { FormComboBoxVirtualPartyExternalSalesRep } from "../../../../../app/common/form/FormComboBoxVirtualPartyExternalSalesRep";
import { FormComboBoxVirtualPartyExternalSalesManager } from "../../../../../app/common/form/FormComboBoxVirtualPartyExternalSalesManager";
import QuickCreatePartyDialog, { PartyRole } from "../../../../../app/common/form/QuickCreatePartyDialog";
import SalesRequestMenu from "../menu/SalesRequestMenu";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import { SALE_TYPE_OPTIONS, COMMISSION_STATUS_RIBBON_COLORS } from "../../../../../app/models/orders/salesCommissionLabels";

function formatEGP(value: number | null | undefined) {
    return Math.round(value ?? 0).toLocaleString("ar-SA");
}

// Collection ratio no longer blocks commission creation, and no longer affects what gets paid —
// approval always pays each party's full percentage-based amount. This factor only drives the
// "المستحق الآن" preview on a still-pending commission.
function getThresholdStatus(ratio: number, saleTypeId: string | null) {
    const lower = saleTypeId === "COMM_SALE_INDIRECT" ? 0.075 : 0.05;
    if (ratio < lower) return { status: "zero" as const, factor: 0, lower };
    if (ratio < 0.10) return { status: "partial" as const, factor: 0.5, lower };
    return { status: "full" as const, factor: 1, lower };
}

// Builds a validator capping `fieldValue + otherFieldValue` at maxPct, used for the
// sales rep 1/2 and manager 1/2 percent fields which share a combined project cap.
function makePercentCapValidator(
    maxPct: number | undefined,
    getOtherValue: () => number,
    label: string
) {
    if (maxPct === undefined) return undefined;
    return (v: any) => {
        const other = getOtherValue();
        return (v ?? 0) + other > maxPct
            ? `إجمالي نسبة ${label} يتجاوز الحد الأقصى ${maxPct.toFixed(4)}%`
            : undefined;
    };
}

// Mirror of the backend's ValidatePartyPercentPairing: a slot may be left entirely empty, but a party
// that IS named must carry a percentage — otherwise approval would create no payment for someone the
// user deliberately assigned. The reverse case (percentage, no party) is not an error here: the
// percentage is zeroed on submit and flagged inline, so the user can save a half-known commission.
function makePercentRequiredValidator(partySelected: boolean, label: string) {
    return (v: any) =>
        partySelected && (v ?? 0) <= 0
            ? `أدخل نسبة ${label} أو احذف الطرف`
            : undefined;
}

// External broker net calc — mirrors SalesCommissionCalculator.CalculateAmountsCore's external
// company VAT/WHT math on the backend. The two exemptions are independent: a VAT-exempt broker is
// still subject to WHT, and only hasWithholdingTaxExemption removes it. VAT, when it applies, is
// embedded in the gross amount, so WHT comes off the VAT-exclusive base; with no VAT embedded the
// gross IS the base.
interface TaxBreakdown {
    gross: number;
    // VAT embedded in the gross — 0 when the party is VAT-exempt or not VAT-liable. Deducted from the
    // net along with `wht`, so gross − vat − wht === net and the on-screen lines add up.
    vat: number;
    wht: number;   // withholding tax actually deducted — 0 when exempt or the rate is 0
    net: number;
}

function calcExternalCompanyAmounts(
    salePrice: number, percent: number, factor: number,
    hasVatExemption: boolean, vatPercent: number,
    hasWithholdingTaxExemption: boolean, withholdingTaxPercent: number
): TaxBreakdown {
    const gross = salePrice * percent / 100 * factor;
    const vatRate = vatPercent > 0 ? vatPercent : 14;
    const baseAmount = hasVatExemption ? gross : gross * 100 / (100 + vatRate);
    const vat = gross - baseAmount;
    const wht = (!hasWithholdingTaxExemption && withholdingTaxPercent > 0)
        ? baseAmount * (withholdingTaxPercent / 100)
        : 0;
    return { gross, vat, wht, net: gross - vat - wht };
}

// External broker's rep/manager net calc — WHT-only, mirrors the backend's ExternalSalesRep/ExternalManager math.
// These are natural persons, so no VAT is embedded and the gross is the withholding base.
function calcWhtOnlyAmounts(salePrice: number, percent: number, factor: number, hasExemption: boolean, withholdingTaxPercent: number): TaxBreakdown {
    const gross = salePrice * percent / 100 * factor;
    const wht = (!hasExemption && withholdingTaxPercent > 0) ? gross * (withholdingTaxPercent / 100) : 0;
    return { gross, vat: 0, wht, net: gross - wht };
}

interface Props {
    commission?: SalesCommission;
    salesRequestId?: string;
    editMode: 0 | 1 | 2;
    cancelEdit: () => void;
}

interface CommissionPartyRowProps {
    partyFieldName: string;
    percentFieldName: string;
    partyLabel: string;
    percentLabel: string;
    partyComponent: ComponentType<any>;
    percentValidators?: Array<(value: any) => string | undefined>;
    disabled: boolean;
    partySelected: boolean;
    showQuickCreate: boolean;
    quickCreateTooltip: string;
    onQuickCreate: () => void;
    amountRoleLabel: string;
    percent: number;
    salePrice: number;
    factor: number;
    finalAmount?: number | null;
    secondToggle?: {
        checked: boolean;
        onChange: (checked: boolean) => void;
        label: string;
    };
}

// Shared row shape for the internal party sections (sales rep 1/2, manager 1/2):
// party combobox + optional quick-create icon + percent field, an optional "add second
// party" toggle, and an info line showing percent, full (factor-independent) value, and
// the amount currently deserved based on collection — collection no longer blocks
// creation, so this is always shown live, not just after the commission is saved.
// Once approved, `finalAmount` (the persisted, already-paid amount) is shown instead.
function CommissionPartyRow({
    partyFieldName, percentFieldName, partyLabel, percentLabel, partyComponent,
    percentValidators, disabled, partySelected, showQuickCreate, quickCreateTooltip, onQuickCreate,
    amountRoleLabel, percent, salePrice, factor, finalAmount, secondToggle,
}: CommissionPartyRowProps) {
    const fullAmount = salePrice * percent / 100;
    const nowAmount = fullAmount * factor;

    return (
        <Grid container spacing={2} alignItems="center" sx={{ mb: 1 }}>
            <Grid item xs={12} md={4}>
                <Grid container alignItems="center">
                    <Grid item xs={11}>
                        <Field
                            id={partyFieldName}
                            name={partyFieldName}
                            label={partyLabel}
                            component={partyComponent}
                            disabled={disabled}
                        />
                    </Grid>
                    <Grid item xs={1}>
                        {showQuickCreate && (
                            <Tooltip title={quickCreateTooltip}>
                                <IconButton size="small" onClick={onQuickCreate}>
                                    <AddCircleOutlineIcon fontSize="small" />
                                </IconButton>
                            </Tooltip>
                        )}
                    </Grid>
                </Grid>
            </Grid>
            <Grid item xs={6} md={2}>
                <Field
                    id={percentFieldName}
                    name={percentFieldName}
                    label={percentLabel}
                    component={FormNumericTextBox}
                    min={0} max={100} format="n4"
                    disabled={disabled}
                    validator={percentValidators?.length ? percentValidators : undefined}
                />
            </Grid>
            {disabled && finalAmount ? (
                <Grid item xs={12} md={4}>
                    <Typography variant="body2" sx={{ mt: 3 }}>
                        {amountRoleLabel}: {formatEGP(finalAmount)}
                    </Typography>
                </Grid>
            ) : percent > 0 && !partySelected ? (
                <Grid item xs={12} md={4}>
                    <Typography variant="body2" color="warning.main" sx={{ mt: 3 }}>
                        {`${amountRoleLabel}: لم يتم تحديد الطرف — لن تُحتسب عمولة وستُحفظ النسبة صفراً`}
                    </Typography>
                </Grid>
            ) : percent > 0 ? (
                <Grid item xs={12} md={4}>
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 3 }}>
                        {`${amountRoleLabel}: ${percent}% ← القيمة الكاملة: ${formatEGP(fullAmount)} ← المستحق الآن: ${formatEGP(nowAmount)}`}
                    </Typography>
                </Grid>
            ) : null}
            {secondToggle && !disabled && (
                <Grid item xs={12} md={2}>
                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={secondToggle.checked}
                                onChange={(e) => secondToggle.onChange(e.target.checked)}
                                size="small"
                            />
                        }
                        label={secondToggle.label}
                    />
                </Grid>
            )}
        </Grid>
    );
}

interface ExternalAmountInfoProps {
    label: string;
    percent: number;
    salePrice: number;
    factor: number;
    calcAmounts: (salePrice: number, percent: number, factor: number) => TaxBreakdown;
    finalGross?: number | null;
    finalNet?: number | null;
    disabled: boolean;
    partySelected: boolean;
}

// Info line for the external broker rows (company/sales rep/manager): percent, gross, every tax that
// is actually taken off it, then the net and the net currently deserved based on collection. A tax
// line only appears when it is really applied — a VAT-exempt broker shows no VAT line but still shows
// its withholding line, since the two exemptions are independent. Once approved, shows the persisted
// gross/net with the deduction derived from them.
function ExternalAmountInfo({ label, percent, salePrice, factor, calcAmounts, finalGross, finalNet, disabled, partySelected }: ExternalAmountInfoProps) {
    if (disabled && finalGross) {
        const deducted = finalNet != null ? finalGross - finalNet : 0;
        return (
            <Grid item xs={12} md={5}>
                <Box sx={{ mt: 3 }}>
                    <Typography variant="body2">
                        {label} — إجمالي: {formatEGP(finalGross)}
                    </Typography>
                    {deducted > 0 && (
                        <Typography variant="caption" color="error.main" display="block">
                            {`إجمالي المخصوم (ض.ق.م + ض.استقطاع): −${formatEGP(deducted)}`}
                        </Typography>
                    )}
                    {finalNet != null && (
                        <Typography variant="body2">صافي: {formatEGP(finalNet)}</Typography>
                    )}
                </Box>
            </Grid>
        );
    }
    if (percent <= 0) return null;
    if (!partySelected) {
        return (
            <Grid item xs={12} md={5}>
                <Typography variant="body2" color="warning.main" sx={{ mt: 3 }}>
                    {`${label}: لم يتم تحديد الطرف — لن تُحتسب عمولة وستُحفظ النسبة صفراً`}
                </Typography>
            </Grid>
        );
    }
    const full = calcAmounts(salePrice, percent, 1);
    const now = calcAmounts(salePrice, percent, factor);
    return (
        <Grid item xs={12} md={5}>
            <Box sx={{ mt: 3 }}>
                <Typography variant="body2" color="text.secondary">
                    {`${label}: ${percent}% ← إجمالي القيمة الكاملة: ${formatEGP(full.gross)}`}
                </Typography>
                {full.vat > 0 && (
                    <Typography variant="caption" color="error.main" display="block">
                        {`ض.ق.م المخصومة: −${formatEGP(full.vat)}`}
                    </Typography>
                )}
                {full.wht > 0 && (
                    <Typography variant="caption" color="error.main" display="block">
                        {`ض.الاستقطاع المخصومة: −${formatEGP(full.wht)}`}
                    </Typography>
                )}
                <Typography variant="body2" color="text.secondary">
                    {`صافي القيمة الكاملة: ${formatEGP(full.net)} ← المستحق الآن: ${formatEGP(now.net)}`}
                </Typography>
            </Box>
        </Grid>
    );
}

interface QuickCreateState {
    open: boolean;
    role: PartyRole;
    fieldName: string;
}

export default function SalesCommissionForm({ commission, salesRequestId, editMode, cancelEdit }: Props) {
    const [createCommission] = useCreateSalesCommissionMutation();
    const [updateCommission] = useUpdateSalesCommissionMutation();
    const [buttonFlag, setButtonFlag] = useState(false);
    const [hasTwoSalesReps, setHasTwoSalesReps] = useState(false);
    const [hasTwoManagers, setHasTwoManagers] = useState(false);
    const [hasVatExemption, setHasVatExemption] = useState(false);
    const [hasWithholdingTaxExemption, setHasWithholdingTaxExemption] = useState(false);
    const [hasExternalSalesRepWhtExemption, setHasExternalSalesRepWhtExemption] = useState(false);
    const [hasExternalManagerWhtExemption, setHasExternalManagerWhtExemption] = useState(false);
    const [quickCreate, setQuickCreate] = useState<QuickCreateState>({ open: false, role: "SALES_REP", fieldName: "" });
    const [showExistingDialog, setShowExistingDialog] = useState(false);
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

    const { data: existingCommission } = useGetSalesCommissionBySalesRequestQuery(effectiveSrId!, {
        skip: !effectiveSrId || editMode === 2,
    });

    useEffect(() => {
        if (defaults?.existingCommissionId && editMode === 1) {
            setShowExistingDialog(true);
        }
    }, [defaults?.existingCommissionId, editMode]);

    const activeCommission = editMode === 2 ? commission : existingCommission ?? undefined;

    // These six toggles live outside the Kendo Form, so remounting the Form (its `key` changes with the
    // sales request) does not reset them. In create mode the user can point the sales-request combobox
    // at a request that already has a commission and then at one that does not — activeCommission goes
    // defined → undefined while this component stays mounted. Without the else branch the previous
    // record's tax exemptions would silently carry into the new commission.
    useEffect(() => {
        if (activeCommission) {
            setHasTwoSalesReps(!!activeCommission.salesRep2PartyId);
            setHasTwoManagers(!!activeCommission.manager2PartyId);
            setHasVatExemption(activeCommission.hasVatExemption ?? false);
            setHasWithholdingTaxExemption(activeCommission.hasWithholdingTaxExemption ?? false);
            setHasExternalSalesRepWhtExemption(activeCommission.hasExternalSalesRepWithholdingTaxExemption ?? false);
            setHasExternalManagerWhtExemption(activeCommission.hasExternalManagerWithholdingTaxExemption ?? false);
        } else {
            setHasTwoSalesReps(false);
            setHasTwoManagers(false);
            setHasVatExemption(false);
            setHasWithholdingTaxExemption(false);
            setHasExternalSalesRepWhtExemption(false);
            setHasExternalManagerWhtExemption(false);
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
        const srId = effectiveSrId;
        // Prefer the commission's own snapshot; fall back to the defaults lookup, which is
        // always fetched as soon as an SR is known — covers the case of a brand-new commission
        // navigated in with a pre-set salesRequestId (no activeCommission exists yet).
        const apartmentName = activeCommission?.apartmentName ?? defaults?.apartmentName ?? "";
        const projectName = activeCommission?.projectName ?? defaults?.projectName ?? "";
        const srIdItem = srId ? {
            salesRequestId: srId,
            customerName: "",
            apartmentName,
            projectName,
            label: [apartmentName, projectName].filter(Boolean).join(" - ") || srId,
        } : null;
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
                externalSalesRepNationalId: activeCommission.externalSalesRepNationalId ?? "",
                externalManagerParty: activeCommission.externalManagerPartyId
                    ? { fromPartyId: activeCommission.externalManagerPartyId, fromPartyName: activeCommission.externalManagerName ?? "" }
                    : null,
                externalManagerPercent: activeCommission.externalManagerPercent ?? null,
                externalManagerNationalId: activeCommission.externalManagerNationalId ?? "",
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
            externalSalesRepNationalId: "",
            externalManagerParty: null,
            externalManagerPercent: null,
            externalManagerNationalId: "",
            vatPercent: 14,
            withholdingTaxPercent: 5,
            notes: "",
        };
    }, [activeCommission, defaults, effectiveSrId]);

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        const isIndirect = data.saleTypeId === "COMM_SALE_INDIRECT";
        const srId = data.salesRequestId?.salesRequestId ?? null;

        // A party slot may be left unassigned — the user often does not know the manager or the broker
        // when the commission is first recorded. Its percentage is dropped so no amount is ever stored
        // against a party that does not exist; the backend enforces the same pairing rule.
        const partyId = (party: any) => party?.fromPartyId ?? null;
        const pct = (party: any, value: number | null | undefined) =>
            partyId(party) ? (value ?? null) : null;

        const payload: Partial<SalesCommission> = {
            salesRequestId: srId,
            saleTypeId: data.saleTypeId,
            salesRepPartyId: partyId(data.salesRepParty),
            salesRepPercent: pct(data.salesRepParty, data.salesRepPercent) ?? 0,
            managerPartyId: partyId(data.managerParty),
            managerPercent: pct(data.managerParty, data.managerPercent) ?? 0,
            salesRep2PartyId: hasTwoSalesReps ? partyId(data.salesRep2Party) : null,
            salesRep2Percent: hasTwoSalesReps ? pct(data.salesRep2Party, data.salesRep2Percent) : null,
            manager2PartyId: hasTwoManagers ? partyId(data.manager2Party) : null,
            manager2Percent: hasTwoManagers ? pct(data.manager2Party, data.manager2Percent) : null,
            externalCompanyPartyId: isIndirect ? partyId(data.externalCompanyParty) : null,
            externalCompanyPercent: isIndirect ? pct(data.externalCompanyParty, data.externalCompanyPercent) : null,
            externalSalesRepPartyId: isIndirect ? partyId(data.externalSalesRepParty) : null,
            externalSalesRepPercent: isIndirect ? pct(data.externalSalesRepParty, data.externalSalesRepPercent) : null,
            hasExternalSalesRepWithholdingTaxExemption: isIndirect ? hasExternalSalesRepWhtExemption : false,
            externalSalesRepNationalId: isIndirect ? (data.externalSalesRepNationalId ?? null) : null,
            externalManagerPartyId: isIndirect ? partyId(data.externalManagerParty) : null,
            externalManagerPercent: isIndirect ? pct(data.externalManagerParty, data.externalManagerPercent) : null,
            hasExternalManagerWithholdingTaxExemption: isIndirect ? hasExternalManagerWhtExemption : false,
            externalManagerNationalId: isIndirect ? (data.externalManagerNationalId ?? null) : null,
            hasVatExemption: isIndirect ? hasVatExemption : false,
            hasWithholdingTaxExemption: isIndirect ? hasWithholdingTaxExemption : false,
            // Not zeroed by hasVatExemption: that flag is the single source of truth for the exemption,
            // and zeroing the rate made the reloaded field read 0% while the calc fell back to 14%.
            vatPercent: isIndirect ? (data.vatPercent ?? 14) : 0,
            // Not zeroed by hasWithholdingTaxExemption: this rate also drives the external
            // sales rep / manager net calc, gated independently by their own exemption flags.
            withholdingTaxPercent: isIndirect ? (data.withholdingTaxPercent ?? 5) : 0,
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

    function openQuickCreate(role: PartyRole, fieldName: string) {
        setQuickCreate({ open: true, role, fieldName });
    }

    function handlePartyCreated(party: { fromPartyId: string; fromPartyName: string }) {
        if (formApi) {
            formApi.onChange(quickCreate.fieldName, { value: party });
        }
        setQuickCreate(q => ({ ...q, open: false }));
    }

    // Unassigned slots on the saved record, for the approve confirmation. Only slots that always
    // exist are listed — a second rep/manager simply does not exist when its party is empty.
    const unassignedSavedParties = useMemo(() => {
        if (!activeCommission) return [];
        const savedIsIndirect = activeCommission.saleTypeId === "COMM_SALE_INDIRECT";
        return [
            { id: activeCommission.salesRepPartyId, label: "المندوب الأول", shown: true },
            { id: activeCommission.managerPartyId, label: "المدير الأول", shown: true },
            { id: activeCommission.externalCompanyPartyId, label: "شركة الوسيط", shown: savedIsIndirect },
            { id: activeCommission.externalSalesRepPartyId, label: "مندوب الوسيط", shown: savedIsIndirect },
            { id: activeCommission.externalManagerPartyId, label: "مدير الوسيط", shown: savedIsIndirect },
        ].filter(x => x.shown && !x.id).map(x => x.label);
    }, [activeCommission]);

    const isApproved = activeCommission?.statusId === "COMMISSION_APPROVED";

    const ribbonBg = COMMISSION_STATUS_RIBBON_COLORS;
    const ribbonLabels: Record<string, string> = {
        COMMISSION_PENDING: getTranslatedLabel("salesCommission.status.pending", "قيد الانتظار"),
        COMMISSION_APPROVED: getTranslatedLabel("salesCommission.status.approved", "معتمدة"),
        COMMISSION_PAID: getTranslatedLabel("salesCommission.status.paid", "مدفوعة"),
    };
    const statusId = activeCommission?.statusId ?? "";

    return (
        <>
            <SalesRequestMenu
                selectedMenuItem="sales-commissions"
                onMenuSelect={(key) => {
                    if (key === "salesRequest.menu.salesCommissions") {
                        cancelEdit();
                    }
                }}
            />
            <Paper elevation={5} className="div-container-withBorderCurved" style={{ padding: "16px" }}>
                <Grid container alignItems="center" sx={{ mb: 2 }}>
                    <Grid item xs={editMode === 2 && activeCommission?.salesCommissionId ? 11 : 12}>
                        <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
                            <Typography variant="h4" color={editMode === 1 ? "green" : "black"}>
                                {editMode === 1
                                    ? getTranslatedLabel("salesCommission.form.new", "عمولة مبيعات جديدة")
                                    : getTranslatedLabel("salesCommission.form.edit", "تعديل العمولة")}
                                {activeCommission?.salesCommissionId && (
                                    <Typography component="span" variant="h5" color="grey" sx={{ ml: 1 }}>
                                        ({activeCommission.salesCommissionId})
                                    </Typography>
                                )}
                            </Typography>
                            {editMode === 2 && activeCommission?.salesCommissionId && (
                                <SalesCommissionActionsMenu
                                    salesCommissionId={activeCommission.salesCommissionId}
                                    currentStatusId={activeCommission.statusId}
                                    disabled={false}
                                    unassignedParties={unassignedSavedParties}
                                    onCommissionApproved={cancelEdit}
                                    onCommissionReset={cancelEdit}
                                    onCommissionDeleted={cancelEdit}
                                />
                            )}
                        </Box>
                    </Grid>
                    {editMode === 2 && activeCommission?.salesCommissionId && (
                        <Grid item xs={1}>
                            <RibbonContainer>
                                <Ribbon
                                    side="left"
                                    type="corner"
                                    size="large"
                                    backgroundColor={ribbonBg[statusId] ?? "#757575"}
                                    color="#ffffff"
                                    fontFamily="sans-serif"
                                >
                                    {ribbonLabels[statusId] ?? statusId}
                                </Ribbon>
                            </RibbonContainer>
                        </Grid>
                    )}
                </Grid>

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

                        const maxRepPct = activeRate?.salesRepPercent;
                        const maxMgrPct = activeRate?.managerPercent;
                        const maxExtPct = activeRate?.externalCompanyPercent ?? undefined;

                        const repValidator = makePercentCapValidator(
                            maxRepPct,
                            () => hasTwoSalesReps ? (formRenderProps.valueGetter("salesRep2Percent") ?? 0) : 0,
                            "المندوبين"
                        );
                        const rep2Validator = makePercentCapValidator(
                            maxRepPct,
                            () => formRenderProps.valueGetter("salesRepPercent") ?? 0,
                            "المندوبين"
                        );
                        const mgrValidator = makePercentCapValidator(
                            maxMgrPct,
                            () => hasTwoManagers ? (formRenderProps.valueGetter("manager2Percent") ?? 0) : 0,
                            "المديرين"
                        );
                        const mgr2Validator = makePercentCapValidator(
                            maxMgrPct,
                            () => formRenderProps.valueGetter("managerPercent") ?? 0,
                            "المديرين"
                        );
                        const extValidator = maxExtPct !== undefined
                            ? (v: any) => (v ?? 0) > maxExtPct ? `الحد الأقصى ${maxExtPct.toFixed(4)}%` : undefined
                            : undefined;

                        // Every party slot is optional — the user may not know the manager or the broker yet.
                        const isPartySelected = (fieldName: string) =>
                            !!formRenderProps.valueGetter(fieldName)?.fromPartyId;
                        const validators = (...vs: Array<((v: any) => string | undefined) | undefined>) =>
                            vs.filter(Boolean) as Array<(v: any) => string | undefined>;

                        const repSelected = isPartySelected("salesRepParty");
                        const rep2Selected = isPartySelected("salesRep2Party");
                        const mgrSelected = isPartySelected("managerParty");
                        const mgr2Selected = isPartySelected("manager2Party");
                        const extCompanySelected = isPartySelected("externalCompanyParty");
                        const extRepSelected = isPartySelected("externalSalesRepParty");
                        const extMgrSelected = isPartySelected("externalManagerParty");

                        // Slots the user can currently see but has left empty — these produce no payment
                        // on approval, so they are called out before saving rather than after.
                        const unassignedParties = [
                            { selected: repSelected, label: "المندوب الأول", shown: true },
                            { selected: mgrSelected, label: "المدير الأول", shown: true },
                            { selected: rep2Selected, label: "المندوب الثاني", shown: hasTwoSalesReps },
                            { selected: mgr2Selected, label: "المدير الثاني", shown: hasTwoManagers },
                            { selected: extCompanySelected, label: "شركة الوسيط", shown: isIndirect },
                            { selected: extRepSelected, label: "مندوب الوسيط", shown: isIndirect },
                            { selected: extMgrSelected, label: "مدير الوسيط", shown: isIndirect },
                        ].filter(x => x.shown && !x.selected).map(x => x.label);

                        
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
                                                disabled={!!resolvedSalesRequestId || !!activeCommission || isApproved}
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
                                                data={SALE_TYPE_OPTIONS}
                                                validator={requiredValidator}
                                                disabled={isApproved}
                                                onAfterChange={setSelectedSaleTypeId}
                                            />
                                        </Grid>
                                    </Grid>

                                    {/* Sale Price / Collected Amount — read-only, derived from SR and payments */}
                                    {defaults && (() => {
                                        const salePrice = defaults.salePrice;
                                        const collected = defaults.collectedAmount;
                                        const ratio = salePrice > 0 ? collected / salePrice : 0;
                                        const ratioColor = ratio >= 0.10 ? "green" : ratio >= 0.05 ? "orange" : "red";
                                        return (
                                            <Grid container spacing={2} sx={{ mb: 2, p: 1, background: "#f9f9f9", borderRadius: 1 }}>
                                                <Grid item xs={6} md={3}>
                                                    <Typography variant="caption" color="text.secondary" display="block">
                                                        {getTranslatedLabel("salesCommission.form.salePrice", "سعر البيع")}
                                                    </Typography>
                                                    <Typography variant="body1" fontWeight="bold">
                                                        {formatEGP(salePrice)}
                                                    </Typography>
                                                </Grid>
                                                <Grid item xs={6} md={3}>
                                                    <Typography variant="caption" color="text.secondary" display="block">
                                                        {getTranslatedLabel("salesCommission.form.collectedAmount", "المبلغ المحصل")}
                                                    </Typography>
                                                    <Typography variant="body1" fontWeight="bold">
                                                        {formatEGP(collected)}
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
                                                    {` ← ${formatEGP(salePrice * activeRate.salesRepPercent / 100 * factor)}`}
                                                </Typography>
                                                <Typography variant="caption" sx={{ whiteSpace: "nowrap" }}>
                                                    {`المدير: ${activeRate.managerPercent}%`}
                                                    {hasTwoManagers && ` (${(activeRate.managerPercent / 2).toFixed(4)}% × 2)`}
                                                    {` ← ${formatEGP(salePrice * activeRate.managerPercent / 100 * factor)}`}
                                                </Typography>
                                                {isIndirect && activeRate.externalCompanyPercent != null && (
                                                    <Typography variant="caption" sx={{ whiteSpace: "nowrap" }}>
                                                        {`الوسيط: ${activeRate.externalCompanyPercent}%`}
                                                        {` ← ${formatEGP(salePrice * activeRate.externalCompanyPercent / 100 * factor)}`}
                                                    </Typography>
                                                )}
                                                {isIndirect && activeRate.externalSalesRepPercent != null && (
                                                    <Typography variant="caption" sx={{ whiteSpace: "nowrap" }}>
                                                        {`مندوب الوسيط: ${activeRate.externalSalesRepPercent}%`}
                                                        {` ← ${formatEGP(salePrice * activeRate.externalSalesRepPercent / 100 * factor)}`}
                                                    </Typography>
                                                )}
                                                {isIndirect && activeRate.externalManagerPercent != null && (
                                                    <Typography variant="caption" sx={{ whiteSpace: "nowrap" }}>
                                                        {`مدير الوسيط: ${activeRate.externalManagerPercent}%`}
                                                        {` ← ${formatEGP(salePrice * activeRate.externalManagerPercent / 100 * factor)}`}
                                                    </Typography>
                                                )}
                                            </Box>
                                        </Alert>
                                    )}

                                    {/* Threshold status alert — shown once an SR is chosen */}
                                    {effectiveSrId && defaults && (
                                        <Alert
                                            severity={
                                                threshold.status === "zero" ? "error"
                                                : threshold.status === "partial" ? "warning"
                                                : "success"
                                            }
                                            sx={{ mb: 2 }}
                                        >
                                            {threshold.status === "zero" && (
                                                <>
                                                    {`نسبة التحصيل الحالية (${((defaults?.collectedRatio ?? 0) * 100).toFixed(2)}%) أقل من الحد الأدنى المطلوب (${(threshold.lower * 100).toFixed(1)}%)`}
                                                </>
                                            )}
                                            {threshold.status === "partial" && (
                                                <>
                                                    {`نسبة التحصيل الحالية ${((defaults?.collectedRatio ?? 0) * 100).toFixed(2)}% (بين ${(threshold.lower * 100).toFixed(1)}% و10%)`}
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
                                    <CommissionPartyRow
                                        partyFieldName="salesRepParty"
                                        percentFieldName="salesRepPercent"
                                        partyLabel={getTranslatedLabel("salesCommission.form.salesRep1", "المندوب الأول")}
                                        percentLabel={getTranslatedLabel("salesCommission.form.salesRep1Percent", "نسبة المندوب الأول %")}
                                        partyComponent={FormComboBoxVirtualPartySalesRep}
                                        percentValidators={validators(repValidator, makePercentRequiredValidator(repSelected, "المندوب الأول"))}
                                        partySelected={repSelected}
                                        disabled={isApproved}
                                        showQuickCreate={!isApproved && isIndirect}
                                        quickCreateTooltip="إنشاء مندوب جديد"
                                        onQuickCreate={() => openQuickCreate("SALES_REP", "salesRepParty")}
                                        amountRoleLabel={getTranslatedLabel("salesCommission.form.salesRep1Amount", "مبلغ المندوب الأول")}
                                        percent={formRenderProps.valueGetter("salesRepPercent") ?? 0}
                                        salePrice={salePrice}
                                        factor={factor}
                                        finalAmount={activeCommission?.salesRepAmount}
                                        secondToggle={{
                                            checked: hasTwoSalesReps,
                                            onChange: setHasTwoSalesReps,
                                            label: "مندوب ثاني",
                                        }}
                                    />

                                    {/* Sales Rep 2 */}
                                    {hasTwoSalesReps && (
                                        <CommissionPartyRow
                                            partyFieldName="salesRep2Party"
                                            percentFieldName="salesRep2Percent"
                                            partyLabel={getTranslatedLabel("salesCommission.form.salesRep2", "المندوب الثاني")}
                                            percentLabel={getTranslatedLabel("salesCommission.form.salesRep2Percent", "نسبة المندوب الثاني %")}
                                            partyComponent={FormComboBoxVirtualPartySalesRep}
                                            percentValidators={validators(rep2Validator, makePercentRequiredValidator(rep2Selected, "المندوب الثاني"))}
                                            partySelected={rep2Selected}
                                            disabled={isApproved}
                                            showQuickCreate={!isApproved && isIndirect}
                                            quickCreateTooltip="إنشاء مندوب جديد"
                                            onQuickCreate={() => openQuickCreate("SALES_REP", "salesRep2Party")}
                                            amountRoleLabel={getTranslatedLabel("salesCommission.form.salesRep2Amount", "مبلغ المندوب الثاني")}
                                            percent={formRenderProps.valueGetter("salesRep2Percent") ?? 0}
                                            salePrice={salePrice}
                                            factor={factor}
                                            finalAmount={activeCommission?.salesRep2Amount}
                                        />
                                    )}

                                    {/* Manager 1 */}
                                    <CommissionPartyRow
                                        partyFieldName="managerParty"
                                        percentFieldName="managerPercent"
                                        partyLabel={getTranslatedLabel("salesCommission.form.manager1", "المدير الأول")}
                                        percentLabel={getTranslatedLabel("salesCommission.form.manager1Percent", "نسبة المدير الأول %")}
                                        partyComponent={FormComboBoxVirtualPartySalesManager}
                                        percentValidators={validators(mgrValidator, makePercentRequiredValidator(mgrSelected, "المدير الأول"))}
                                        partySelected={mgrSelected}
                                        disabled={isApproved}
                                        showQuickCreate={!isApproved && isIndirect}
                                        quickCreateTooltip="إنشاء مدير جديد"
                                        onQuickCreate={() => openQuickCreate("SALES_MANAGER", "managerParty")}
                                        amountRoleLabel={getTranslatedLabel("salesCommission.form.manager1Amount", "مبلغ المدير الأول")}
                                        percent={formRenderProps.valueGetter("managerPercent") ?? 0}
                                        salePrice={salePrice}
                                        factor={factor}
                                        finalAmount={activeCommission?.managerAmount}
                                        secondToggle={{
                                            checked: hasTwoManagers,
                                            onChange: setHasTwoManagers,
                                            label: "مدير ثاني",
                                        }}
                                    />

                                    {/* Manager 2 */}
                                    {hasTwoManagers && (
                                        <CommissionPartyRow
                                            partyFieldName="manager2Party"
                                            percentFieldName="manager2Percent"
                                            partyLabel={getTranslatedLabel("salesCommission.form.manager2", "المدير الثاني")}
                                            percentLabel={getTranslatedLabel("salesCommission.form.manager2Percent", "نسبة المدير الثاني %")}
                                            partyComponent={FormComboBoxVirtualPartySalesManager}
                                            percentValidators={validators(mgr2Validator, makePercentRequiredValidator(mgr2Selected, "المدير الثاني"))}
                                            partySelected={mgr2Selected}
                                            disabled={isApproved}
                                            showQuickCreate={!isApproved && isIndirect}
                                            quickCreateTooltip="إنشاء مدير جديد"
                                            onQuickCreate={() => openQuickCreate("SALES_MANAGER", "manager2Party")}
                                            amountRoleLabel={getTranslatedLabel("salesCommission.form.manager2Amount", "مبلغ المدير الثاني")}
                                            percent={formRenderProps.valueGetter("manager2Percent") ?? 0}
                                            salePrice={salePrice}
                                            factor={factor}
                                            finalAmount={activeCommission?.manager2Amount}
                                        />
                                    )}

                                    {/* External Section — INDIRECT only */}
                                    {isIndirect && (
                                        <>
                                            <Typography variant="h6" sx={{ mt: 2, mb: 1, fontWeight: "bold" }}>
                                                {getTranslatedLabel("salesCommission.form.externalSection", "العمولات الخارجية (الوسيط)")}
                                            </Typography>

                                            {/* Tax flags */}
                                            <Grid container spacing={2} alignItems="flex-end" sx={{ mb: 1 }}>
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
                                                        validator={validators(extValidator, makePercentRequiredValidator(extCompanySelected, "الوسيط"))}
                                                    />
                                                </Grid>
                                                <ExternalAmountInfo
                                                    label={getTranslatedLabel("salesCommission.form.externalCompanyNet", "صافي الوسيط")}
                                                    percent={formRenderProps.valueGetter("externalCompanyPercent") ?? 0}
                                                    salePrice={salePrice}
                                                    factor={factor}
                                                    calcAmounts={(sp, pct, f) => calcExternalCompanyAmounts(
                                                        sp, pct, f,
                                                        hasVatExemption, formRenderProps.valueGetter("vatPercent") ?? 14,
                                                        hasWithholdingTaxExemption, formRenderProps.valueGetter("withholdingTaxPercent") ?? 5
                                                    )}
                                                    partySelected={extCompanySelected}
                                                    finalGross={activeCommission?.externalCompanyGrossAmount}
                                                    finalNet={activeCommission?.externalCompanyNetAmount}
                                                    disabled={isApproved}
                                                />
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
                                                                component={FormComboBoxVirtualPartyExternalSalesRep}
                                                                disabled={isApproved}
                                                            />
                                                        </Grid>
                                                        <Grid item xs={1}>
                                                            {!isApproved && (
                                                                <Tooltip title="إنشاء مندوب وسيط جديد">
                                                                    <IconButton size="small" onClick={() => openQuickCreate("EXTERNAL_SALES_REP", "externalSalesRepParty")}>
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
                                                        validator={validators(makePercentRequiredValidator(extRepSelected, "مندوب الوسيط"))}
                                                    />
                                                </Grid>
                                                <Grid item xs={6} md={2}>
                                                    <FormControlLabel
                                                        control={
                                                            <Checkbox
                                                                checked={hasExternalSalesRepWhtExemption}
                                                                onChange={(e) => setHasExternalSalesRepWhtExemption(e.target.checked)}
                                                                disabled={isApproved}
                                                                size="small"
                                                            />
                                                        }
                                                        label={getTranslatedLabel("salesCommission.form.hasExternalSalesRepWhtExemption", "إعفاء ض.استقطاع")}
                                                    />
                                                </Grid>
                                                <Grid item xs={6} md={2}>
                                                    <Field
                                                        id="externalSalesRepNationalId"
                                                        name="externalSalesRepNationalId"
                                                        label={getTranslatedLabel("salesCommission.form.externalSalesRepNationalId", "الرقم القومي (مندوب الوسيط)")}
                                                        component={FormInput}
                                                        disabled={isApproved}
                                                    />
                                                </Grid>
                                                <ExternalAmountInfo
                                                    label={getTranslatedLabel("salesCommission.form.externalSalesRepNet", "صافي مندوب الوسيط")}
                                                    percent={formRenderProps.valueGetter("externalSalesRepPercent") ?? 0}
                                                    salePrice={salePrice}
                                                    factor={factor}
                                                    calcAmounts={(sp, pct, f) => calcWhtOnlyAmounts(
                                                        sp, pct, f,
                                                        hasExternalSalesRepWhtExemption, formRenderProps.valueGetter("withholdingTaxPercent") ?? 5
                                                    )}
                                                    partySelected={extRepSelected}
                                                    finalGross={activeCommission?.externalSalesRepAmount}
                                                    finalNet={activeCommission?.externalSalesRepNetAmount}
                                                    disabled={isApproved}
                                                />
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
                                                                component={FormComboBoxVirtualPartyExternalSalesManager}
                                                                disabled={isApproved}
                                                            />
                                                        </Grid>
                                                        <Grid item xs={1}>
                                                            {!isApproved && (
                                                                <Tooltip title="إنشاء مدير وسيط جديد">
                                                                    <IconButton size="small" onClick={() => openQuickCreate("EXTERNAL_SALES_MANAGER", "externalManagerParty")}>
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
                                                        validator={validators(makePercentRequiredValidator(extMgrSelected, "مدير الوسيط"))}
                                                    />
                                                </Grid>
                                                <Grid item xs={6} md={2}>
                                                    <FormControlLabel
                                                        control={
                                                            <Checkbox
                                                                checked={hasExternalManagerWhtExemption}
                                                                onChange={(e) => setHasExternalManagerWhtExemption(e.target.checked)}
                                                                disabled={isApproved}
                                                                size="small"
                                                            />
                                                        }
                                                        label={getTranslatedLabel("salesCommission.form.hasExternalManagerWhtExemption", "إعفاء ض.استقطاع")}
                                                    />
                                                </Grid>
                                                <Grid item xs={6} md={2}>
                                                    <Field
                                                        id="externalManagerNationalId"
                                                        name="externalManagerNationalId"
                                                        label={getTranslatedLabel("salesCommission.form.externalManagerNationalId", "الرقم القومي (مدير الوسيط)")}
                                                        component={FormInput}
                                                        disabled={isApproved}
                                                    />
                                                </Grid>
                                                <ExternalAmountInfo
                                                    label={getTranslatedLabel("salesCommission.form.externalManagerNet", "صافي مدير الوسيط")}
                                                    percent={formRenderProps.valueGetter("externalManagerPercent") ?? 0}
                                                    salePrice={salePrice}
                                                    factor={factor}
                                                    calcAmounts={(sp, pct, f) => calcWhtOnlyAmounts(
                                                        sp, pct, f,
                                                        hasExternalManagerWhtExemption, formRenderProps.valueGetter("withholdingTaxPercent") ?? 5
                                                    )}
                                                    partySelected={extMgrSelected}
                                                    finalGross={activeCommission?.externalManagerAmount}
                                                    finalNet={activeCommission?.externalManagerNetAmount}
                                                    disabled={isApproved}
                                                />
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

                                    {/* Unassigned parties — savable, but they earn nothing until named */}
                                    {!isApproved && unassignedParties.length > 0 && (
                                        <Alert severity="warning" sx={{ mt: 2 }}>
                                            {getTranslatedLabel(
                                                "salesCommission.form.unassignedParties",
                                                "لم يتم تحديد الأطراف التالية"
                                            )}
                                            {`: ${unassignedParties.join("، ")} — `}
                                            {getTranslatedLabel(
                                                "salesCommission.form.unassignedPartiesHint",
                                                "يمكنك الحفظ الآن وتحديدها لاحقاً، ولن يتم إنشاء دفعات لها عند الاعتماد."
                                            )}
                                        </Alert>
                                    )}

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
                                                            (!!effectiveSrId && !!defaults && !!currentSaleTypeId && !activeRate)
                                                        }
                                                    >
                                                        {getTranslatedLabel("salesCommission.form.submit", "حفظ")}
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

            <Dialog open={showExistingDialog} onClose={() => setShowExistingDialog(false)} maxWidth="sm" fullWidth>
                <DialogTitle sx={{ display: "flex", alignItems: "center", gap: 1, color: "info.main" }}>
                    <InfoOutlinedIcon color="info" />
                    سجل عمولة موجود
                </DialogTitle>
                <DialogContent>
                    <Typography variant="body1" sx={{ mb: 1 }}>
                        يوجد سجل عمولة مبيعات مرتبط بهذا الطلب
                        {activeCommission?.salesCommissionId && (
                            <Typography component="span" variant="body1" fontWeight="bold" sx={{ mx: 0.5 }}>
                                ({activeCommission.salesCommissionId})
                            </Typography>
                        )}.
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                        تم تحميل البيانات الموجودة تلقائياً — يمكنك مراجعتها أو تعديلها.
                    </Typography>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setShowExistingDialog(false)} variant="contained" color="info">
                        حسناً
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
}
