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
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import {useCalculateInstallmentPriceMutation} from "../../../../../app/store/apis/salesRequestApi";

interface InstallmentCalcResult {
    period: number;
    dueDate: string;
    amount: number;
    presentValue: number;
}

interface CalculatorResponse {
    cashPricePerM2: number;
    installmentPricePerM2: number;
    totalInstallments: number;
    downPaymentPercentage: number;
    annualDiscountRate: number;
    pvaf: number;
    increasePercentage: number;
    schedule: InstallmentCalcResult[];
}

const InstallmentPriceCalculatorModal: React.FC<{ onClose: () => void }> = ({
                                                                                onClose,
                                                                            }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const t = (key: string, fallback: string) =>
        getTranslatedLabel(`installmentCalculator.${key}`, fallback);

    // This mutation is only triggered on submit → perfect!
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
                                            {console.log("VALID:", formRenderProps.valid, "VALUES:", formRenderProps.values)}

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

                        {/* ---- Summary Cards ---- */}
                        <Grid container spacing={3}>
                            {/* Total Installment Price */}
                            <Grid item xs={12} sm={6} md={3}>
                                <Box bgcolor="#e8f5e8" p={2} borderRadius={2}>
                                    <Typography variant="caption" color="text.secondary">
                                        {t("results.installmentPricePerM2", "سعر المتر بالتقسيط (إجمالي)")}
                                    </Typography>
                                    <Typography variant="h5" color="primary">
                                        {result.installmentPricePerM2.toLocaleString("en-US")} ج.م
                                    </Typography>
                                </Box>
                            </Grid>

                            {/* Quarterly Payment Per m² */}
                            <Grid item xs={12} sm={6} md={3}>
                                <Box bgcolor="#e3f2fd" p={2} borderRadius={2}>
                                    <Typography variant="caption" color="text.secondary">
                                        قيمة القسط {result.installmentsPerYear === 12 ? "الشهري" :
                                        result.installmentsPerYear === 4 ? "الربع سنوي" :
                                            result.installmentsPerYear === 2 ? "نصف السنوي" : "السنوي"} للمتر
                                    </Typography>
                                    <Typography variant="h5" color="info.main">
                                        {result.quarterlyInstallmentPerM2?.toLocaleString() ?? "—"} ج.م
                                    </Typography>
                                </Box>
                            </Grid>

                            {/* Increase % */}
                            <Grid item xs={12} sm={6} md={3}>
                                <Box bgcolor="#fff3e0" p={2} borderRadius={2}>
                                    <Typography variant="caption" color="text.secondary">
                                        نسبة الزيادة عن الكاش
                                    </Typography>
                                    <Typography
                                        variant="h5"
                                        color={result.increasePercentage > 0 ? "error" : result.increasePercentage < 0 ? "success" : "text.primary"}
                                    >
                                        {result.increasePercentage >= 0 ? "+" : ""}
                                        {result.increasePercentage.toFixed(2)}%
                                    </Typography>
                                </Box>
                            </Grid>

                            {/* PVAF */}
                            <Grid item xs={12} sm={6} md={3}>
                                <Box bgcolor="#f5f5f5" p={2} borderRadius={2}>
                                    <Typography variant="caption" color="text.secondary">
                                        PVAF
                                    </Typography>
                                    <Typography variant="h6">{result.pvaf.toFixed(6)}</Typography>
                                </Box>
                            </Grid>
                        </Grid>

                        {/* ---- Installment Schedule Grid ---- */}
                        <Grid item xs={12} mt={4}>
                            <Typography variant="subtitle1" gutterBottom>
                                {t("schedule.title", "جدول الأقساط")}
                            </Typography>

                            <div style={{ height: 100, width: "100%" }}>
                                <KendoGrid
                                    data={result.schedule}
                                    sortable
                                    resizable
                                    style={{ height: "100%" }}
                                >
                                    <Column field="period" title="#" width={80} />
                                    <Column
                                        field="dueDate"
                                        title={t("schedule.dueDate", "تاريخ الاستحقاق")}
                                        format="{0:dd/MM/yyyy}"
                                        width={150}
                                    />
                                    <Column
                                        field="amount"
                                        title={t("schedule.amount", "قيمة القسط (ج.م)")}
                                        format="{0:n0}"
                                        width={140}
                                    />
                                    <Column
                                        field="presentValue"
                                        title={t("schedule.pv", "القيمة الحالية (ج.م)")}
                                        format="{0:n0}"
                                        width={160}
                                    />
                                </KendoGrid>
                            </div>
                        </Grid>
                    </>
                )}
            </Grid>
    );
};

export default InstallmentPriceCalculatorModal;