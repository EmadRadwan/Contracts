// BillingAccountForm.tsx – FINAL VERSION (Perfect UX + Error Handling)
import React, { useEffect, useMemo, useState } from "react";
import {
    Box,
    Button,
    Grid,
    Paper,
    Typography,
    CircularProgress,
    Alert,
    Skeleton,
} from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { requiredValidator } from "../../../../app/common/form/Validators";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import { FormComboBoxVirtualProject } from "../../../../app/common/form/FormComboBoxVirtualProject";
import { FormComboBoxVirtualContractorsAndSuppliers } from "../../../../app/common/form/FormComboBoxVirtualContractorsAndSuppliers";
import {
    useCreateBillingAccountMutation,
    useLazyFetchBalancesForVendorAndProjectQuery,
} from "../../../../app/store/apis";
import { parseDate } from "../../../../app/util/utils";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import { BillingAccount } from "../../../../app/models/accounting/billingAccount";
import AccountingMenu from "../../invoice/menu/AccountingMenu";

interface Props {
    billingAccount?: BillingAccount;
    editMode: number; // 1 = create, 2 = edit
    cancelEdit: () => void;
    onBillingAccountCreated?: (created: BillingAccount) => void;
}

const BillingAccountForm: React.FC<Props> = ({
                                                 billingAccount,
                                                 editMode,
                                                 cancelEdit,
                                                 onBillingAccountCreated,
                                             }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [create, { isLoading: isCreating, error: createError }] = useCreateBillingAccountMutation();
    const [triggerBalance, { data: balanceData, isFetching: balanceLoading }] =
        useLazyFetchBalancesForVendorAndProjectQuery();

    const [currentPartyId, setCurrentPartyId] = useState<string | null>(null);
    const [currentProjectId, setCurrentProjectId] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [creationError, setCreationError] = useState<string | null>(null);

    // This will hold the freshly created account (used to show balance right after create)
    const [justCreatedAccount, setJustCreatedAccount] = useState<BillingAccount | null>(null);

    const initialValues = useMemo(() => {
        if (editMode === 1) {
            return {
                fromDate: new Date(),
                thruDate: null,
                accountLimit: null,
                partyId: null,
                projectId: null,
                description: "",
            };
        }

        const acc = billingAccount!;
        return {
            billingAccountId: acc.billingAccountId,
            accountLimit: acc.accountLimit,
            description: acc.description ?? "",
            fromDate: parseDate(acc.fromDate),
            thruDate: parseDate(acc.thruDate),
            createdDate: parseDate(acc.createdDate),
            partyId: { fromPartyId: acc.partyId, fromPartyName: acc.partyName },
            projectId: acc.projectId ? { projectId: acc.projectId, projectName: acc.projectName } : null,
        };
    }, [editMode, billingAccount]);

    // Trigger balance fetch only when we have both IDs and account exists
    useEffect(() => {
        const hasBothIds = currentPartyId && currentProjectId;
        const hasAccount = editMode === 2 || justCreatedAccount;

        if (hasBothIds && hasAccount) {
            triggerBalance({ partyId: currentPartyId, projectId: currentProjectId }, false);
        }
    }, [currentPartyId, currentProjectId, editMode, justCreatedAccount, triggerBalance]);

    const handleSubmit = async (data: any) => {
        setIsSubmitting(true);
        setCreationError(null); // Reset previous error

        try {
            const payload = {
                partyId: data.partyId?.fromPartyId ?? data.partyId,
                projectId: data.projectId?.projectId ?? data.projectId ?? null,
                accountLimit: data.accountLimit,
                fromDate: data.fromDate,
                thruDate: data.thruDate || null,
                description: data.description || "",
            };

            const result = await create(payload).unwrap(); // This does NOT throw on isSuccess: false

            // CRITICAL: Manually check isSuccess
            if (!result.isSuccess) {
                const errorMsg = result.error || "فشل إنشاء حساب الأجل";
                setCreationError(errorMsg);
                toast.error(errorMsg);
                return; // Stop here
            }

            // Success path
            toast.success("تم إنشاء حساب الأجل بنجاح");

            const createdAccount: BillingAccount = {
                ...result.value,
                fromDate: parseDate(result.value.fromDate),
                thruDate: parseDate(result.value.thruDate),
                createdDate: parseDate(result.value.createdDate),
                partyId: result.value.partyId,
                partyName: result.value.partyName,
                projectId: result.value.projectId,
                projectName: result.value.projectName,
            };

            setJustCreatedAccount(createdAccount);
            onBillingAccountCreated?.(createdAccount);
        } catch (err: any) {
            // Only for real network errors
            const msg = err?.data?.error || "حدث خطأ غير متوقع";
            setCreationError(msg);
            toast.error(msg);
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <>
            <AccountingMenu selectedMenuItem="/billingAccounts" onMenuSelect={(key) => {
                if (key === "billingAccounts") {
                    cancelEdit(); // ← Forces back to list view
                }
            }}/>
            <Paper elevation={5} className="div-container-withBorderCurved" sx={{ p: 3 }}>
                <Typography variant="h4" gutterBottom color={editMode === 1 ? "success.main" : "text.primary"}>
                    {editMode === 1 ? "حساب أجل جديد" : `حساب الأجل: ${billingAccount?.billingAccountId}`}
                </Typography>


                <Form
                    initialValues={initialValues}
                    onSubmit={handleSubmit}
                    render={(formRenderProps) => {
                        const { valueGetter, onChange } = formRenderProps;

                        const partyObj = valueGetter("partyId");
                        const projectObj = valueGetter("projectId");

                        const partyId = partyObj?.fromPartyId ?? partyObj?.partyId ?? null;
                        const projectId = projectObj?.projectId ?? null;

                        // Update tracking state
                        if (partyId !== currentPartyId) setCurrentPartyId(partyId);
                        if (projectId !== currentProjectId) setCurrentProjectId(projectId);

                        const showBalanceBox = partyId && projectId && (editMode === 2 || justCreatedAccount);

                        return (
                            <FormElement>
                                <fieldset className="k-form-fieldset">
                                    <Grid container spacing={3}>
                                        <Grid item xs={12} md={4}>
                                            <Field
                                                name="partyId"
                                                label="العميل / المورد"
                                                component={FormComboBoxVirtualContractorsAndSuppliers}
                                                validator={requiredValidator}
                                                disabled={editMode === 2}
                                            />
                                        </Grid>

                                        <Grid item xs={12} md={4}>
                                            <Field
                                                name="projectId"
                                                label="المشروع"
                                                component={FormComboBoxVirtualProject}
                                                validator={requiredValidator}
                                                disabled={editMode === 2}
                                            />
                                        </Grid>

                                        <Grid item xs={12} md={4}>
                                            <Field
                                                name="accountLimit"
                                                label="حد الحساب"
                                                component={FormNumericTextBox}
                                                format="n2"
                                                validator={requiredValidator}
                                                disabled={editMode === 2}
                                            />
                                        </Grid>

                                        <Grid item xs={12} md={4}>
                                            <Field
                                                name="fromDate"
                                                label="من تاريخ"
                                                component={FormDatePicker}
                                                validator={requiredValidator}
                                                disabled={editMode === 2}
                                            />
                                        </Grid>

                                        <Grid item xs={12} md={4}>
                                            <Field
                                                name="thruDate"
                                                label="إلى تاريخ"
                                                component={FormDatePicker}
                                                disabled={editMode === 2}
                                            />
                                        </Grid>

                                        <Grid item xs={12} md={8}>
                                            <Field
                                                name="description"
                                                label="الوصف"
                                                component={FormTextArea}
                                                rows={3}
                                            />
                                        </Grid>
                                    </Grid>

                                    {/* Balance Box – Only show when account exists */}
                                    {showBalanceBox && (
                                        <Grid item xs={12} sx={{ mt: 4 }}>
                                            <Box sx={{ p: 3, border: "1px solid #e0e0e0", borderRadius: 2, bgcolor: "#f9f9f9" }}>
                                                {balanceLoading ? (
                                                    <Skeleton variant="rounded" height={100} />
                                                ) : balanceData ? (
                                                    balanceData.initialBalance === 0 ? (
                                                        <Alert severity="info">
                                                            {balanceData.message || "لا يوجد سقف محدد حالياً"}
                                                        </Alert>
                                                    ) : (
                                                        <Grid container spacing={3}>
                                                            <Grid item xs={4}>
                                                                <Typography variant="body2" color="text.secondary">السقف المتاح</Typography>
                                                                <Typography variant="h6" color="success.main" fontWeight="bold">
                                                                    {Number(balanceData.initialBalance).toLocaleString("ar-EG")} ج.م
                                                                </Typography>
                                                            </Grid>
                                                            <Grid item xs={4}>
                                                                <Typography variant="body2" color="text.secondary">المستخدم</Typography>
                                                                <Typography variant="h6" color="warning.main">
                                                                    {Number(balanceData.usedBalance).toLocaleString("ar-EG")} ج.م
                                                                </Typography>
                                                            </Grid>
                                                            <Grid item xs={4}>
                                                                <Typography variant="body2" color="text.secondary">المتبقي</Typography>
                                                                <Typography
                                                                    variant="h6"
                                                                    color={balanceData.remainingBalance > 0 ? "primary" : "error"}
                                                                    fontWeight="bold"
                                                                >
                                                                    {Number(balanceData.remainingBalance).toLocaleString("ar-EG")} ج.م
                                                                </Typography>
                                                            </Grid>
                                                        </Grid>
                                                    )
                                                ) : null}
                                            </Box>
                                        </Grid>
                                    )}

                                    <Box sx={{ mt: 5 }} className="k-form-buttons">
                                        <Button
                                            variant="contained"
                                            color="success"
                                            type="submit"
                                            disabled={isSubmitting || isCreating || !formRenderProps.allowSubmit}
                                            startIcon={isCreating ? <CircularProgress size={20} /> : null}
                                        >
                                            {isCreating ? "جاري الحفظ..." : "حفظ"}
                                        </Button>

                                        <Button variant="contained" color="error" onClick={cancelEdit} sx={{ ml: 2 }}>
                                            رجوع
                                        </Button>
                                    </Box>
                                </fieldset>
                            </FormElement>
                        );
                    }}
                />
            </Paper>
        </>
        
    );
};

export default React.memo(BillingAccountForm);