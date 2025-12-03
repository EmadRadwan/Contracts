import React, { useMemo, useRef, useState } from "react";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Box, Button, Grid, Paper, Typography, CircularProgress } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { BillingAccount } from "../../../../app/models/accounting/billingAccount";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import {
    useCreateBillingAccountMutation,
    useFetchBillingAccountsBalanceQuery,
} from "../../../../app/store/apis";
import {formatCurrency, handleDatesObject, parseDate} from "../../../../app/util/utils";
import { FormComboBoxVirtualProject } from "../../../../app/common/form/FormComboBoxVirtualProject";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {
    FormComboBoxVirtualContractorsAndSuppliers,
} from "../../../../app/common/form/FormComboBoxVirtualContractorsAndSuppliers";
import { toast } from "react-toastify";

interface Props {
    editMode: number;
    selectedBillingAccount?: BillingAccount;
    onClose: () => void;
    setEditMode?: (mode: number) => void;
    onBillingAccountCreated?: (account: BillingAccount) => void; // ← New callback
}

const BillingAccountForm = ({
                                editMode,
                                onClose,
                                selectedBillingAccount,
                                setEditMode,
                                onBillingAccountCreated,
                            }: Props) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [createBillingAccount, { isLoading: isCreating }] = useCreateBillingAccountMutation();

    // REFACTOR: Fetch balance only when viewing/editing existing account
    const { data: billingAccountBalance } = useFetchBillingAccountsBalanceQuery(
        selectedBillingAccount?.billingAccountId!,
        { skip: !selectedBillingAccount?.billingAccountId }
    );

    const [buttonFlag, setButtonFlag] = useState(false);

    // REFACTOR: Compute initial values cleanly using useMemo (same pattern as ProductForm)
    const formInitialValues = useMemo(() => {
        if (editMode === 1) {
            return {
                fromDate: new Date(), // today
                thruDate: null,
            };
        }

        if (!selectedBillingAccount) return {};

        return {
            ...selectedBillingAccount,

            // Convert string dates → real Date objects
            fromDate: parseDate(selectedBillingAccount.fromDate),
            thruDate: parseDate(selectedBillingAccount.thruDate),
            createdDate: parseDate(selectedBillingAccount.createdDate),

            // Ensure ComboBox fields are objects (safe fallback)
            partyId: typeof selectedBillingAccount.partyId === "string" || !selectedBillingAccount.partyId
                ? {
                    fromPartyId: selectedBillingAccount.partyId,
                    fromPartyName: selectedBillingAccount.partyName,
                }
                : selectedBillingAccount.partyId,

            projectId: typeof selectedBillingAccount.projectId === "string" || !selectedBillingAccount.projectId
                ? selectedBillingAccount.projectId
                    ? {
                        projectId: selectedBillingAccount.projectId,
                        ProjectName: selectedBillingAccount.projectName ?? null,
                    }
                    : null
                : selectedBillingAccount.projectId,
        };
    }, [editMode, selectedBillingAccount]);
    
    const availableBalance = selectedBillingAccount?.billingAccountId
        ? billingAccountBalance?.billingAccountBalance ?? selectedBillingAccount.availableBalance ?? 0
        : 0;

    const handleSubmit = async (data: any) => {
        setButtonFlag(true);

        try {
            const payload = {
                partyId: data.partyId?.fromPartyId ?? data.partyId,
                projectId: data.projectId?.projectId ?? data.projectId,
                accountLimit: data.accountLimit,
                fromDate: data.fromDate,
                thruDate: data.thruDate || null,
                description: data.description || "",
            };

            const result = await createBillingAccount(payload).unwrap();

            // ─────────────────────────────────────────────────────
            // CRITICAL: Check the actual result from your API
            // ─────────────────────────────────────────────────────
            if (!result?.isSuccess) {
                // Backend explicitly told us it failed
                const errorMsg = result?.error || result?.message || "فشل إنشاء حساب الأجل";
                throw new Error(errorMsg); // go to catch block
            }

            // Only now we know it REALLY succeeded
            toast.success("تم إنشاء حساب الأجل بنجاح");

            if (onBillingAccountCreated) {
                // Pass the whole successful response (contains the new account)
                onBillingAccountCreated(result);
            }
            if (setEditMode) setEditMode(2);

        } catch (error: any) {
            // This now catches BOTH network errors AND explicit { isSuccess: false } cases
            const errorMessage =
                error?.message ||                    // from the throw above
                error?.data?.error ||
                error?.data?.message ||
                "حدث خطأ أثناء إنشاء حساب الأجل";

            toast.error(errorMessage);

            // Form stays exactly as-is (editMode stays 1 → no remount → values preserved)
        } finally {
            setButtonFlag(false);
        }
    };
    
    console.log('selectedBillingAccount', selectedBillingAccount)

    return (
        <>
            <AccountingMenu selectedMenuItem="/billingAccounts" />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2}>
                    <Grid item xs={12}>
                        <Box display="flex" justifyContent="space-between" alignItems="center">
                            <Typography
                                sx={{ p: 2 }}
                                color={editMode > 1 ? "black" : "green"}
                                variant="h4"
                            >
                                {editMode === 1
                                    ? getTranslatedLabel("accounting.billingAccounts.form.new", "حساب أجل جديد")
                                    : `${getTranslatedLabel("accounting.billingAccounts.form.title", "حساب الأجل")}: ${selectedBillingAccount?.billingAccountId}`}
                            </Typography>
                        </Box>
                    </Grid>
                </Grid>

                <Form
                    key={editMode === 2 ? selectedBillingAccount?.billingAccountId : "new-billing-account"}
                    initialValues={formInitialValues}
                    onSubmit={handleSubmit}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className="k-form-fieldset">
                                <Grid container spacing={2}>
                                    <Grid item xs={12} md={4}>
                                        <Field
                                            name="accountLimit"
                                            label={getTranslatedLabel("accounting.billingAccounts.form.accountLimit", "حد الحساب")}
                                            component={FormNumericTextBox}
                                            validator={requiredValidator}
                                        />
                                    </Grid>

                                    <Grid item xs={12} md={4}>
                                        <Field
                                            name="partyId"
                                            label={getTranslatedLabel("accounting.billingAccounts.form.party", "العميل / الطرف")}
                                            component={FormComboBoxVirtualContractorsAndSuppliers}
                                            disabled={editMode > 1}
                                            validator={editMode === 1 ? requiredValidator : undefined}
                                        />
                                    </Grid>

                                    <Grid item xs={12} md={4}>
                                        <Field
                                            name="projectId"
                                            component={FormComboBoxVirtualProject}
                                            label={getTranslatedLabel("projects.certificate.form.project", "المشروع")}
                                            dataItemKey="projectId"
                                            textField="ProjectName"
                                            validator={requiredValidator}
                                            disabled={editMode > 3}
                                        />
                                    </Grid>
                                </Grid>

                                <Grid container spacing={2} sx={{ mt: 2 }}>
                                    <Grid item xs={12} md={4}>
                                        <Field
                                            name="fromDate"
                                            label={getTranslatedLabel("accounting.billingAccounts.form.fromDate", "تاريخ البداية")}
                                            component={FormDatePicker}
                                            validator={requiredValidator}
                                            disabled={editMode > 1}
                                        />
                                    </Grid>

                                    <Grid item xs={12} md={4}>
                                        <Field
                                            name="thruDate"
                                            label={getTranslatedLabel("accounting.billingAccounts.form.thruDate", "تاريخ النهاية")}
                                            component={FormDatePicker}
                                        />
                                    </Grid>

                                    {editMode > 1 && (
                                        <Grid item xs={12}>
                                            <Typography variant="body1" sx={{ fontWeight: "bold", mt: 2 }}>
                                                {getTranslatedLabel("accounting.billingAccounts.form.availableBalance", "الرصيد المتاح")}
                                            </Typography>
                                            <Typography variant="h5" sx={{ color: "red", fontWeight: "bold" }}>
                                                {formatCurrency(availableBalance)}
                                            </Typography>
                                        </Grid>
                                    )}

                                    <Grid item xs={12} md={8}>
                                        <Field
                                            name="description"
                                            label={getTranslatedLabel("accounting.billingAccounts.form.description", "الوصف")}
                                            component={FormTextArea}
                                        />
                                    </Grid>
                                </Grid>
                            </fieldset>

                            <div className="k-form-buttons" style={{ marginTop: 24 }}>
                                <Button
                                    variant="contained"
                                    color="success"
                                    type="submit"
                                    disabled={!formRenderProps.allowSubmit || buttonFlag || isCreating}
                                    startIcon={isCreating ? <CircularProgress size={20} /> : null}
                                >
                                    {isCreating
                                        ? getTranslatedLabel("general.saving", "جاري الحفظ...")
                                        : getTranslatedLabel("general.save", "حفظ")}
                                </Button>

                                <Button variant="contained" color="error" onClick={onClose} sx={{ ml: 2 }}>
                                    {getTranslatedLabel("general.back", "رجوع")}
                                </Button>
                            </div>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
};

export default React.memo(BillingAccountForm);