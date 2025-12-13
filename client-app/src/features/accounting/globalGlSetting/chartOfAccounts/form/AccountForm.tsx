
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
import { FormDropDownList } from "../../../../../app/common/form/MemoizedFormDropDownList2";
import { useAppSelector } from "../../../../../app/store/configureStore";
import { useAssignGlAccountToOrganizationMutation, useFetchGlAccountOrganizationHierarchyLovQuery } from "../../../../../app/store/apis";
import { useFetchTopLevelGlobalGlAccountsQuery, useCreateGlAccountMutation, useUpdateGlAccountMutation } from "../../../../../app/store/apis/accounting/globalGlSettingsApi";
import { FormDropDownTreeGlAccountWithChildren } from "../../../../../app/common/form/FormDropDownTreeGlAccountWithChildren";

interface Props {
    account?: GlAccount;
    editMode: 1 | 2; // 1 = create, 2 = edit
    cancelEdit: () => void;
    onAccountCreated?: (created: GlAccount) => void;
    onAccountUpdated?: (updated: GlAccount) => void;
}

const messages: Record<string, Record<string, string>> = {
    en: {
        // Success messages
        GL_ACCOUNT_CREATED: "GL Account created successfully.",
        GL_ACCOUNT_UPDATED: "GL Account updated successfully.",
        // Error messages
        USER_NOT_FOUND: "Unauthorized: User not found.",
        GL_ACCOUNT_ID_REQUIRED: "GL Account ID is required.",
        GL_ACCOUNT_NOT_FOUND: "The specified GL Account could not be found.",
        GL_ACCOUNT_SAVE_FAILED: "Failed to save the GL Account.",
        GL_ACCOUNT_UPDATE_FAILED: "Failed to update the GL Account.",
        GL_ACCOUNT_CREATE_FAILED: "Failed to create the GL Account.",
        ACCOUNT_CODE_GENERATION_FAILED: "Failed to generate a unique account code.",
        GL_ACCOUNT_INVALID_PARENT: "Cannot assign account as child to itself.",
        UNEXPECTED_ERROR: "An unexpected error occurred. Please try again.",
        DEFAULT: "An unexpected error occurred. Please try again.",
        GL_ACCOUNT_ASSIGNED: "GL Account assigned successfully.",
    },
    ar: {
        // Success messages
        GL_ACCOUNT_CREATED: "تم إنشاء حساب دفتر الأستاذ بنجاح.",
        GL_ACCOUNT_UPDATED: "تم تحديث حساب دفتر الأستاذ بنجاح.",
        GL_ACCOUNT_ASSIGNED: "تم تعيين حساب دفتر الأستاذ بنجاح.",
        // Error messages
        USER_NOT_FOUND: "غير مصرح: المستخدم غير موجود.",
        GL_ACCOUNT_ID_REQUIRED: "معرف حساب دفتر الأستاذ مطلوب.",
        GL_ACCOUNT_NOT_FOUND: "حساب دفتر الأستاذ المحدد غير موجود.",
        GL_ACCOUNT_SAVE_FAILED: "فشل في حفظ حساب دفتر الأستاذ.",
        GL_ACCOUNT_UPDATE_FAILED: "فشل في تحديث حساب دفتر الأستاذ.",
        GL_ACCOUNT_CREATE_FAILED: "فشل في إنشاء حساب دفتر الأستاذ.",
        ACCOUNT_CODE_GENERATION_FAILED: "فشل في إنشاء رمز حساب فريد.",
        UNEXPECTED_ERROR: "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.",
        GL_ACCOUNT_INVALID_PARENT: "لا يمكن تعيين الحساب كتابع لنفسه.",
        DEFAULT: "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.",
    },
};

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
    const language = useAppSelector((state) => state.localization.language || "en");
    const companyId = user?.organizationPartyId || "";

    const getMessage = (code: string) => {
        return messages[language]?.[code] || messages["en"]?.[code] || code;
    };

    const handleApiError = (error: any, defaultMessage: string) => {
        const errorCode = error?.data?.errorCode || "DEFAULT";
        const errorMessage = error?.data?.title || defaultMessage;
        const localizedMessage = messages[language]?.[errorCode] || errorMessage || defaultMessage;
        setCreationError(localizedMessage);
        toast.error(localizedMessage);
        console.error(error);
    };

    const { data: glAccounts, isLoading: isLoadingGlAccounts } = useFetchTopLevelGlobalGlAccountsQuery(undefined);
    const [createGlAccount, { isLoading: isCreating }] = useCreateGlAccountMutation();
    const [updateGlAccount, { isLoading: isUpdating }] = useUpdateGlAccountMutation();
    const [assignGlAccountToOrganization] = useAssignGlAccountToOrganizationMutation();

    const [isSubmitting, setIsSubmitting] = useState(false);
    const [creationError, setCreationError] = useState<string | null>(null);
    const [justCreatedAccount, setJustCreatedAccount] = useState<GlAccount | null>(null);
    const [formKey, setFormKey] = useState(0);
    /* ------------------------------------------------------------------
       Initial values – empty on create (unless creating similar), populated on edit
       ------------------------------------------------------------------ */
    const initialValues = useMemo(() => {
        // If create mode and no account provided, return empty form
        if (editMode === 1 && !account) return {};

        // If create mode with account (creating similar), pre-fill with parent settings
        if (editMode === 1 && account) {
            return {
                accountName: account.accountName ?? "",
                glAccountTypeId: account.glAccountTypeId ?? null,
                glAccountClassId: account.glAccountClassId ?? null,
                glResourceTypeId: account.glResourceTypeId ?? null,
                parentGlAccountId: account.parentGlAccountId
                    ? {
                        glAccountId: account.parentGlAccountId,
                        text: account.parentAccountName || account.parentGlAccountId,
                    }
                    : null,
                description: null,
            };
        }

        // Edit mode
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

                toast.success(getMessage("GL_ACCOUNT_CREATED"));

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
                // edit - only send editable fields
                const updatePayload = {
                    glAccountId: account!.glAccountId!,
                    accountName: data.accountName,
                    description: data.description,
                    parentGlAccountId: data.parentGlAccountId?.glAccountId ?? null,
                };

                const result = await updateGlAccount(updatePayload).unwrap();

                toast.success(getMessage("GL_ACCOUNT_UPDATED"));

                const updatedAccount: GlAccount = {
                    ...account,
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

                // Reset form to untouched state
                setFormKey(prev => prev + 1);
                onAccountUpdated?.(updatedAccount);
            }
        } catch (err: any) {
            handleApiError(err, getMessage("GL_ACCOUNT_SAVE_FAILED"));
        } finally {
            setIsSubmitting(false);
        }
    };

    async function handleAssign() {
        try {
            if (!companyId || !account?.glAccountId) return;
            await assignGlAccountToOrganization({
                glAccountId: account?.glAccountId,
                companyId: companyId,
            }).unwrap();
            toast.success(getMessage("GL_ACCOUNT_ASSIGNED"));
        } catch (error) {
            handleApiError(error, getMessage("DEFAULT"));
        }
    }

    return (
        <Paper elevation={6} sx={{ p: 4, mt: 2, mx: 2 }}>
            <Grid container spacing={2}>
                <Grid item xs={11}>
                    <Typography variant="h5" gutterBottom>
                        {editMode === 1
                            ? getTranslatedLabel("accounting.glAccount.form.new", "New GL Account")
                            : `${getTranslatedLabel("accounting.glAccount.form.edit", "Edit GL Account")} – ${account?.glAccountId
                            }`}
                    </Typography>
                </Grid>
                <Grid item xs={1}>
                    <Button
                        onClick={() => handleAssign()}
                        variant="contained"
                        color='success'
                    >
                        {getTranslatedLabel("general.assign-to-org", "Assign to Organization")}
                    </Button>
                </Grid>
            </Grid>

            <Form
                key={formKey}
                initialValues={initialValues}
                onSubmit={handleSubmit}
                render={(formRenderProps: FormRenderProps) => (
                    <FormElement>
                        <Grid container spacing={3}>
                            {/* Row 1 */}
                            <Grid item xs={12}>
                                <Field
                                    name="accountName"
                                    label={getTranslatedLabel("accounting.glAccount.form.accountName", "Account Name *")}
                                    component={FormInput}
                                    validator={requiredValidator}
                                />
                            </Grid>

                            {/* Row 2 – Dropdowns */}
                            <Grid item xs={12} sm={4}>
                                <Field
                                    name="glAccountTypeId"
                                    label={getTranslatedLabel("accounting.glAccount.form.accountType", "Account Type *")}
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
                                    label={getTranslatedLabel("accounting.glAccount.form.accountClass", "Account Class")}
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
                                    label={getTranslatedLabel("accounting.glAccount.form.resourceType", "Resource Type")}
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
                                    label={getTranslatedLabel("accounting.glAccount.form.parentAccount", "Parent Account")}
                                    component={FormDropDownTreeGlAccountWithChildren}
                                    dataItemKey="glAccountId"
                                    textField="accountName"
                                    subItemsField="children"
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
                                    label={getTranslatedLabel("accounting.glAccount.form.description", "Description")}
                                    component={FormInput}
                                    multiline
                                    rows={3}
                                />
                            </Grid>

                            {/* Buttons */}
                            <Grid item xs={12}>
                                <Box sx={{ mt: 2 }}>
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        type="submit"
                                        disabled={isSubmitting || isCreating || isUpdating || !formRenderProps.allowSubmit}
                                        startIcon={(isCreating || isUpdating) ? <CircularProgress size={20} /> : null}
                                    >
                                        {isCreating
                                            ? getTranslatedLabel("accounting.glAccount.form.creating", "Creating...")
                                            : isUpdating
                                                ? getTranslatedLabel("accounting.glAccount.form.updating", "Updating...")
                                                : editMode === 1
                                                    ? getTranslatedLabel("accounting.glAccount.form.createAccount", "Create Account")
                                                    : getTranslatedLabel("accounting.glAccount.form.updateAccount", "Update Account")}
                                    </Button>

                                    <Button
                                        variant="outlined"
                                        onClick={cancelEdit}
                                        sx={{ mx: 2 }}
                                    >
                                        {getTranslatedLabel("general.cancel", "Cancel")}
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