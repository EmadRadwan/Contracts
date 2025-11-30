// src/features/salesRequest/dashboard/InstallmentPriceCalculatorModal.tsx
import React, { useState } from "react";
import {
    Button,
    Grid,
    Typography,
    Box,
    CircularProgress,
} from "@mui/material";
import {
    Form,
    FormElement,
    Field,
    FormRenderProps,
} from "@progress/kendo-react-form";
import {
    Grid as KendoGrid,
    GridColumn as Column,
} from "@progress/kendo-react-grid";
import { toast } from "react-toastify";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import { requiredValidator } from "../../../../../app/common/form/Validators";
import { useCalculateInstallmentPriceMutation } from "../../../../../app/store/apis/salesRequestApi";

// تم تصحيح الـ interface لتتوافق مع الـ Backend الصحيح
interface InstallmentCalcResult {
    period: number;
    dueDate: string;
    amount: number;
    presentValue: number;
}

interface CalculatorResponse {
    cashPricePerM2: number;
    installmentPricePerM2: number;           // ← سعر المتر الجديد الثابت (مثل 45415)
    installmentAmountPerM2: number;          // ← قيمة القسط الثابت لكل متر
    totalInstallments: number;
    downPaymentPercentage: number;
    annualDiscountRate: number;
    pvaf: number;
    increasePercentage: number;
    installmentsPerYear: number;             // ← تم إضافته لأن الـ Backend بيرجعه
    schedule: InstallmentCalcResult[];
}

const InstallmentPriceCalculatorModal: React.FC<{ onClose: () => void }> = ({ onClose }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const t = (key: string, fallback: string) =>
        getTranslatedLabel(`installmentCalculator.${key}`, fallback);

    const [calculate, { isLoading }] = useCalculateInstallmentPriceMutation();
    const [result, setResult] = useState<CalculatorResponse | null>(null);

    async function handleSubmit(data: any) {
        try {
            const payload = {
                cashPricePerM2: Number(data.values.cashPricePerM2),
                annualDiscountRate: Number(data.values.annualDiscountRate),
                downPaymentPercentage: Number(data.values.downPaymentPercentage),
                durationYears: Number(data.values.durationYears),
                installmentsPerYear: Number(data.values.installmentsPerYear),
            };

            const response = await calculate(payload).unwrap();
            setResult(response);
            toast.success(t("calculationSuccess", "تم الحساب بنجاح"));
        } catch (err: any) {
            console.error("Calculation failed:", err);
            toast.error(
                err?.data?.message ||
                err?.data?.error ||
                t("calculationError", "فشل في حساب سعر التقسيط")
            );
        }
    }

    // دالة لعرض نوع القسط بالعربي
    const getFrequencyLabel = () => {
        if (!result) return "";
        const freq = result.installmentsPerYear;
        if (freq === 12) return "الشهري";
        if (freq === 4) return "الربع سنوي";
        if (freq === 2) return "نصف سنوي";
        if (freq === 1) return "السنوي";
        if (freq === 6) return "كل شهرين";
        if (freq === 3) return "كل 4 أشهر";
        return `كل ${12 / freq} أشهر`;
    };

    return (
        <Grid container spacing={3} padding={3}>
            <Grid item xs={12}>
                <Typography variant="h5" gutterBottom>
                    {t("title", "حاسبة سعر المتر بالتقسيط (PV-Based)")}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                    {t(
                        "subtitle",
                        "احسب السعر الاسمي للمتر بالتقسيط بحيث تكون قيمته الحالية مساوية لسعر الكاش."
                    )}
                </Typography>
            </Grid>

            {/* =============== INPUT FORM =============== */}
            <Grid item xs={12}>
                <Form
                    initialValues={{
                        cashPricePerM2: 25000,
                        annualDiscountRate: 0.17,
                        downPaymentPercentage: 0.10,
                        durationYears: 9,
                        installmentsPerYear: 4,
                    }}
                    onSubmitClick={handleSubmit}
                    render={(formRenderProps: FormRenderProps) => (
                        <FormElement>
                            <fieldset className="k-form-fieldset">
                                <Grid container spacing={2}>
                                    <Grid item xs={12} md={6}>
                                        <Field
                                            id="cashPricePerM2"
                                            name="cashPricePerM2"
                                            label={t("cashPricePerM2", "سعر المتر كاش (ج.م)")}
                                            component={FormNumericTextBox}
                                            format="n0"
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                    <Grid item xs={12} md={6}>
                                        <Field
                                            id="annualDiscountRate"
                                            name="annualDiscountRate"
                                            label={t("annualDiscountRate", "معدل الخصم السنوي")}
                                            component={FormNumericTextBox}
                                            format="p2"
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                    <Grid item xs={12} md={6}>
                                        <Field
                                            id="downPaymentPercentage"
                                            name="downPaymentPercentage"
                                            label={t("downPaymentPercentage", "نسبة المقدم %")}
                                            component={FormNumericTextBox}
                                            format="p2"
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                    <Grid item xs={12} md={3}>
                                        <Field
                                            id="durationYears"
                                            name="durationYears"
                                            label={t("durationYears", "المدة (سنوات)")}
                                            component={FormNumericTextBox}
                                            min={1}
                                            format="n0"
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                    <Grid item xs={12} md={3}>
                                        <Field
                                            id="installmentsPerYear"
                                            name="installmentsPerYear"
                                            label={t("installmentsPerYear", "عدد الأقساط سنويًا")}
                                            component={FormNumericTextBox}
                                            min={1}
                                            max={12}
                                            format="n0"
                                            validator={requiredValidator}
                                        />
                                    </Grid>

                                    <Grid item xs={12}>
                                        <Box display="flex" gap={2} mt={2}>
                                            <Button
                                                variant="contained"
                                                color="primary"
                                                type="submit"
                                                disabled={isLoading || !formRenderProps.valid}
                                            >
                                                {isLoading ? <CircularProgress size={24} /> : t("calculate", "احسب")}
                                            </Button>
                                            <Button variant="outlined" onClick={onClose}>
                                                {t("close", "إغلاق")}
                                            </Button>
                                        </Box>
                                    </Grid>
                                </Grid>
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Grid>

            {/* ==================== RESULTS ==================== */}
            {result && (
                <>
                    <Grid item xs={12}>
                        <Typography variant="h6" gutterBottom>
                            {t("results.title", "نتائج الحساب")}
                        </Typography>
                    </Grid>

                    <Grid container spacing={3}>
                        {/* 1. سعر المتر الجديد */}
                        <Grid item xs={12} sm={6} md={3}>
                            <Box bgcolor="#e8f5e8" p={2} borderRadius={2}>
                                <Typography variant="caption" color="text.secondary">
                                    {t("results.installmentPricePerM2", "سعر المتر بالتقسيط (إجمالي)")}
                                </Typography>
                                <Typography variant="h5" color="primary" fontWeight="bold">
                                    {result} ج.م
                                </Typography>
                            </Box>
                        </Grid>
                    </Grid>
                </>
            )}
        </Grid>
    );
};

export default InstallmentPriceCalculatorModal;