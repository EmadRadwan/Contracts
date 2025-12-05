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
    useUpdateBillingAccountMutation,
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
    const [updateBillingAccount, { isLoading: isUpdating }] = useUpdateBillingAccountMutation();

    // REFACTOR: Fetch balance only when viewing/editing existing account
    const { data: billingAccountBalance } = useFetchBillingAccountsBalanceQuery(
        selectedBillingAccount?.billingAccountId!,
        { skip: !selectedBillingAccount?.billingAccountId }
    );

    const [buttonFlag, setButtonFlag] = useState(false);
    const [formKey, setFormKey] = useState(0); // Used to force form re-render on update success

    if (!selectedBillingAccount && editMode !== 1) onClose();

    // REFACTOR: Compute initial values cleanly using useMemo (same pattern as ProductForm)
    const computeInitialValues = (account?: BillingAccount) => {
        if (editMode === 1 && !account) {
            return {
                fromDate: new Date(), // today
                thruDate: null,
            };
        }

        const source = account || selectedBillingAccount;
        if (!source) return {};

        return {
            ...source,

            // Convert string dates → real Date objects
            fromDate: parseDate(source.fromDate),
            thruDate: parseDate(source.thruDate),
            createdDate: parseDate(source.createdDate),

            // Ensure ComboBox fields are objects (safe fallback)
            partyId: typeof source.partyId === "string" || !source.partyId
                ? {
                    fromPartyId: source.partyId,
                    fromPartyName: source.partyName,
                }
                : source.partyId,

            projectId: typeof source.projectId === "string" || !source.projectId
                ? source.projectId
                    ? {
                        projectId: source.projectId,
                        projectName: source.projectName ?? null,
                    }
                    : null
                : source.projectId,
        };
    };

    const [formInitialValues, setFormInitialValues] = useState(() => computeInitialValues());
    
    const availableBalance = selectedBillingAccount?.billingAccountId
        ? billingAccountBalance?.billingAccountBalance ?? selectedBillingAccount.availableBalance ?? 0
        : 0;

    const handleSubmit = async (data: any) => {
        setButtonFlag(true);

        try {
            if (editMode === 1) {
                // CREATE MODE
                const payload = {
                    partyId: data.partyId?.fromPartyId ?? data.partyId,
                    projectId: data.projectId?.projectId ?? data.projectId,
                    accountLimit: data.accountLimit,
                    fromDate: data.fromDate,
                    thruDate: data.thruDate || null,
                    description: data.description || "",
                };

                const result = await createBillingAccount(payload).unwrap();

                if (!result?.isSuccess) {
                    const errorMsg = result?.error || result?.message || "فشل إنشاء حساب الأجل";
                    throw new Error(errorMsg);
                }

                toast.success("تم إنشاء حساب الأجل بنجاح");

                if (onBillingAccountCreated) {
                    onBillingAccountCreated(result);
                }
                if (setEditMode) setEditMode(2);

            } else if (editMode === 2) {
                // UPDATE MODE
                const payload = {
                    billingAccountId: selectedBillingAccount?.billingAccountId,
                    fromDate: data.fromDate,
                    thruDate: data.thruDate || null,
                    description: data.description || "",
                };

                const result = await updateBillingAccount(payload).unwrap();

                if (!result?.isSuccess) {
                    const errorMsg = result?.error || result?.message || "فشل تحديث حساب الأجل";
                    throw new Error(errorMsg);
                }

                toast.success("تم تحديث حساب الأجل بنجاح");

                // Update form initial values with the response data
                const updatedAccount = result.value;
                setFormInitialValues(computeInitialValues(updatedAccount));
                setFormKey((prev) => prev + 1); // Force form re-render
            }

        } catch (error: any) {
            const errorMessage =
                error?.message ||
                error?.data?.error ||
                error?.data?.message ||
                (editMode === 1 ? "حدث خطأ أثناء إنشاء حساب الأجل" : "حدث خطأ أثناء تحديث حساب الأجل");

            toast.error(errorMessage);
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
                    key={editMode === 2 ? `${selectedBillingAccount?.billingAccountId}-${formKey}` : "new-billing-account"}
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
                                            disabled={editMode > 1}
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
                                            disabled={editMode > 1}
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
                                    disabled={!formRenderProps.allowSubmit || buttonFlag || isCreating || isUpdating}
                                    startIcon={(isCreating || isUpdating) ? <CircularProgress size={20} /> : null}
                                >
                                    {(isCreating || isUpdating)
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