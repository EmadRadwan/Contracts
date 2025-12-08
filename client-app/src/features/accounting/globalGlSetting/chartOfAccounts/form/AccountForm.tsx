
import React, { useMemo, useState } from "react";
import {
    Form,
    FormElement,
    Field,
    FormRenderProps,
} from "@progress/kendo-react-form";
import {
    Paper,
    Grid,
    Button,
    Typography,
    Box,
    CircularProgress,
    Alert,
} from "@mui/material";
import { toast } from "react-toastify";

import { requiredValidator } from "../../../../../app/common/form/Validators";
import FormInput from "../../../../../app/common/form/FormInput";
import { FormDropDownTreeGlAccount2 } from "../../../../../app/common/form/FormDropDownTreeGlAccount2";



import { GlAccount } from "../../../../../app/models/accounting/globalGlSettings";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import {FormDropDownList} from "../../../../../app/common/form/MemoizedFormDropDownList2";
import { useAppSelector } from "../../../../../app/store/configureStore";
import { useFetchGlAccountOrganizationHierarchyLovQuery } from "../../../../../app/store/apis";
import { useFetchTopLevelGlobalGlAccountsQuery, useCreateGlAccountMutation } from "../../../../../app/store/apis/accounting/globalGlSettingsApi";
import { FormDropDownTreeGlAccountWithChildren } from "../../../../../app/common/form/FormDropDownTreeGlAccountWithChildren";

interface Props {
    account?: GlAccount;
    editMode: 1 | 2; // 1 = create, 2 = edit
    cancelEdit: () => void;
    onAccountCreated?: (created: GlAccount) => void;
    onAccountUpdated?: (updated: GlAccount) => void;
}

/* ------------------------------------------------------------------
   Static lookup data – no extra API calls needed
   ------------------------------------------------------------------ */
const glAccountTypes = [
    "_NA_",
    "ACCOUNTS_PAYABLE",
    "ACCOUNTS_RECEIVABLE",
    "CURRENT_ASSET",
    "CURRENT_LIABILITY",
    "INCOME",
    "EXPENSE",
    "OWNERS_EQUITY",
    "INVENTORY_ACCOUNT",
    "COGS_ACCOUNT",
    "SALES_ACCOUNT",
    "BANK_STLMNT_ACCOUNT",
    "UNDEPOSITED_RECEIPTS",
    // … add the rest from your list if you want them visible
].map((id) => ({ glAccountTypeId: id, text: id === "_NA_" ? id : id.split("_").map((w) => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase()).join(" ") }));

const glAccountClasses = [
    "ASSET",
    "LIABILITY",
    "EQUITY",
    "REVENUE",
    "EXPENSE",
    "CASH_EQUIVALENT",
    "BANK_ACCOUNT",
    "INVENTORY",
    "ACCUMULATED_DEPRECIATION",
].map((id) => ({ glAccountClassId: id, text: id.split("_").map((w) => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase()).join(" ") }));

const glResourceTypes = ["MONEY", "INVENTORY_ITEM", "FIXED_ASSET", "SERVICE"].map(
    (id) => ({ glResourceTypeId: id, text: id.split("_").map((w) => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase()).join(" ") })
);

const AccountForm: React.FC<Props> = ({
                                          account,
                                          editMode,
                                          cancelEdit,
                                          onAccountCreated,
                                          onAccountUpdated,
                                      }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";

    const { data: glAccounts, isLoading: isLoadingGlAccounts } = useFetchTopLevelGlobalGlAccountsQuery(undefined);
    const [createGlAccount, { isLoading: isCreating }] = useCreateGlAccountMutation();

    const [isSubmitting, setIsSubmitting] = useState(false);
    const [creationError, setCreationError] = useState<string | null>(null);
    const [justCreatedAccount, setJustCreatedAccount] = useState<GlAccount | null>(null);
    /* ------------------------------------------------------------------
       Initial values – empty on create, populated on edit
       ------------------------------------------------------------------ */
    const initialValues = useMemo(() => {
        if (editMode === 1) return {}; // create → empty form

        return {
            glAccountId: account?.glAccountId ?? "",
            accountCode: account?.accountCode ?? "",
            accountName: account?.accountName ?? "",
            accountNameArabic: account?.accountNameArabic ?? "",
            description: account?.description ?? null,

            glAccountTypeId: account?.glAccountTypeId ?? null,
            glAccountClassId: account?.glAccountClassId ?? null,
            glResourceTypeId: account?.glResourceTypeId ?? null,

            // For the tree dropdown we need an object with glAccountId + text
            parentGlAccountId: account?.parentGlAccountId
                ? {
                    glAccountId: account.parentGlAccountId,
                    text:
                        account.parentAccountName ||
                        account.parentGlAccountId ||
                        account.parentGlAccountId,
                }
                : null,
        };
    }, [account, editMode]);

    /* ------------------------------------------------------------------
       Submit handler
       ------------------------------------------------------------------ */
    const handleSubmit = async (data: any) => {
        setIsSubmitting(true);
        setCreationError(null);

        try {
            const payload = {
                accountName: data.accountName,
                glAccountTypeId: data.glAccountTypeId,
                glAccountClassId: data.glAccountClassId,
                glResourceTypeId: data.glResourceTypeId,
                parentGlAccountId: data.parentGlAccountId?.glAccountId ?? null,
                description: data.description,
            };

            if (editMode === 1) {
                const result = await createGlAccount(payload).unwrap();

                toast.success("GL Account created successfully");

                const createdAccount: GlAccount = {
                    ...result,
                    glAccountId: result.glAccountId,
                    accountCode: result.accountCode,
                    accountName: result.accountName,
                    description: result.description,
                    glAccountTypeId: result.glAccountTypeId,
                    glAccountClassId: result.glAccountClassId,
                    glResourceTypeId: result.glResourceTypeId,
                    parentGlAccountId: result.parentGlAccountId,
                };

                setJustCreatedAccount(createdAccount);
                onAccountCreated?.(createdAccount);
            } else {
                // edit - TODO: implement update mutation
                toast.info("Update functionality not yet implemented");
            }
        } catch (err: any) {
            const msg =
                err?.data?.error ||
                err?.data?.message ||
                err?.data?.title ||
                "Failed to save GL Account";
            setCreationError(msg);
            toast.error(msg);
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <Paper elevation={6} sx={{ p: 4, mt: 2, mx: 2 }}>
            <Typography variant="h5" gutterBottom>
                {editMode === 1
                    ? getTranslatedLabel("glAccount.form.new", "New GL Account")
                    : `${getTranslatedLabel("glAccount.form.edit", "Edit GL Account")} – ${
                        account?.glAccountId
                    }`}
            </Typography>

            <Form
                initialValues={initialValues}
                onSubmit={handleSubmit}
                render={(formRenderProps: FormRenderProps) => (
                    <FormElement>
                        <Grid container spacing={3}>
                            {/* Row 1 */}
                            {/* <Grid item xs={12} sm={6}>
                                <Field
                                    name="glAccountId"
                                    label={getTranslatedLabel("glAccount.id", "GL Account ID *")}
                                    component={FormInput}
                                    validator={requiredValidator}
                                    disabled={editMode === 2} // cannot change ID on edit
                                />
                            </Grid>

                            <Grid item xs={12} sm={6}>
                                <Field
                                    name="accountCode"
                                    label={getTranslatedLabel("glAccount Code *")}
                                    component={FormInput}
                                    validator={requiredValidator}
                                />
                            </Grid> */}

                            {/* Row 2 */}
                            <Grid item xs={12}>
                                <Field
                                    name="accountName"
                                    label={getTranslatedLabel("glAccount.name", "Account Name *")}
                                    component={FormInput}
                                    validator={requiredValidator}
                                />
                            </Grid>

                            {/* <Grid item xs={12} sm={6}>
                                <Field
                                    name="accountNameArabic"
                                    label={getTranslatedLabel("glAccount.nameArabic", "Account Name (AR)")}
                                    component={FormInput}
                                />
                            </Grid> */}

                            {/* Row 3 – Dropdowns */}
                            <Grid item xs={12} sm={4}>
                                <Field
                                    name="glAccountTypeId"
                                    label={getTranslatedLabel("glAccount.type", "Account Type *")}
                                    component={FormDropDownList}
                                    data={glAccountTypes}
                                    textField="text"
                                    dataItemKey="glAccountTypeId"
                                    // validator={requiredValidator}
                                    disabled={editMode > 1}
                                />
                            </Grid>

                            <Grid item xs={12} sm={4}>
                                <Field
                                    name="glAccountClassId"
                                    label={getTranslatedLabel("glAccount.class", "Account Class")}
                                    component={FormDropDownList}
                                    data={glAccountClasses}
                                    textField="text"
                                    dataItemKey="glAccountClassId"
                                    // validator={requiredValidator}
                                    disabled={editMode > 1}
                                />
                            </Grid>

                            <Grid item xs={12} sm={4}>
                                <Field
                                    name="glResourceTypeId"
                                    label={getTranslatedLabel("glAccount.resourceType", "Resource Type")}
                                    component={FormDropDownList}
                                    data={glResourceTypes}
                                    textField="text"
                                    dataItemKey="glResourceTypeId"
                                    disabled={editMode > 1}
                                />
                            </Grid>

                            {/* Parent Account – Tree dropdown */}
                            <Grid item xs={12}>
                                <Field
                                    id="parentGlAccountId"
                                    name="parentGlAccountId"
                                    label={getTranslatedLabel("glAccount.parent", "Parent Account")}
                                    component={FormDropDownTreeGlAccountWithChildren}
                                    dataItemKey="glAccountId"
                                    textField="accountName"
                                    data={glAccounts || []}
                                    selectField="selected"
                                    expandField="expanded"
                                    loading={isLoadingGlAccounts}
                                    validator={requiredValidator}
                                    // you can pass filter={item => item.glAccountId !== data.glAccountId} to prevent self-parent if needed
                                />
                            </Grid>

                            {/* Description */}
                            <Grid item xs={12}>
                                <Field
                                    name="description"
                                    label={getTranslatedLabel("glAccount.description", "Description")}
                                    component={FormInput}
                                    multiline
                                    rows={3}
                                />
                            </Grid>

                            {/* Error Alert */}
                            {creationError && (
                                <Grid item xs={12}>
                                    <Alert severity="error" onClose={() => setCreationError(null)}>
                                        {creationError}
                                    </Alert>
                                </Grid>
                            )}

                            {/* Buttons */}
                            <Grid item xs={12}>
                                <Box sx={{ mt: 2 }}>
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        type="submit"
                                        disabled={isSubmitting || isCreating || !formRenderProps.allowSubmit}
                                        startIcon={isCreating ? <CircularProgress size={20} /> : null}
                                    >
                                        {isCreating ? "Creating..." : editMode === 1 ? "Create" : "Update"} Account
                                    </Button>

                                    <Button
                                        variant="outlined"
                                        onClick={cancelEdit}
                                        sx={{ ml: 2 }}
                                    >
                                        Cancel
                                    </Button>
                                </Box>
                            </Grid>
                        </Grid>
                    </FormElement>
                )}
            />
        </Paper>
    );
};

export default AccountForm;