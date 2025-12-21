
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
import {FormComboBoxVirtualGlAccountTypes} from "../../../../../app/common/form/FormComboBoxVirtualGlAccountTypes";
import {FormComboBoxVirtualGlAccountClasses} from "../../../../../app/common/form/FormComboBoxVirtualGlAccountClasses";

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


// REFACTOR: Replaced dynamic English text generation with static Arabic descriptions for consistency with backend data and Arabic UI requirements. This avoids potential mismatches and ensures exact translation as defined in the database.
const glResourceTypes = [
    {
        glResourceTypeId: "_NA_",
        descriptionArabic: "غير قابل للتطبيق"
    },
    {
        glResourceTypeId: "DELIVERED_GOODS",
        descriptionArabic: "البضائع المستلمة"
    },
    {
        glResourceTypeId: "FINISHED_GOODS",
        descriptionArabic: "البضائع النهائية"
    },
    {
        glResourceTypeId: "LABOR",
        descriptionArabic: "العمالة"
    },
    {
        glResourceTypeId: "MONEY",
        descriptionArabic: "النقود"
    },
    {
        glResourceTypeId: "RAW_MATERIALS",
        descriptionArabic: "المواد الخام"
    },
    {
        glResourceTypeId: "SERVICES",
        descriptionArabic: "الخدمات"
    }
];

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
        if (editMode === 1 && !account) return {};

        const baseValues = {
            accountName: account?.accountName ?? "",
            description: account?.description ?? null,
            glResourceTypeId: account?.glResourceTypeId ?? null,
            parentGlAccountId: account?.parentGlAccountId
                ? {
                    glAccountId: account.parentGlAccountId,
                    text: account.parentAccountName || account.parentGlAccountId,
                }
                : null,
        };

        if (editMode === 1 && account) {
            // Creating similar — inherit type/class from parent account
            return {
                ...baseValues,
                glAccountTypeId: account.glAccountTypeId
                    ? { glAccountTypeId: account.glAccountTypeId, description: "" } // description will be fetched if needed
                    : null,
                glAccountClassId: account.glAccountClassId
                    ? { glAccountClassId: account.glAccountClassId, description: "" }
                    : null,
            };
        }

        // Edit mode — shape full objects (descriptionArabic may be missing from query, but ComboBox will display ID if not available)
        return {
            ...baseValues,
            glAccountId: account?.glAccountId ?? "",
            accountCode: account?.accountCode ?? "",
            accountNameArabic: account?.accountNameArabic ?? "",

            glAccountTypeId: account?.glAccountTypeId
                ? { glAccountTypeId: account.glAccountTypeId, description: account.glAccountTypeDescription || account.glAccountTypeId }
                : null,

            glAccountClassId: account?.glAccountClassId
                ? { glAccountClassId: account.glAccountClassId, description: account.glAccountClassDescription || account.glAccountClassId }
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
            // Common fields for both create and edit
            const commonPayload = {
                accountName: data.accountName,
                description: data.description,
                parentGlAccountId: data.parentGlAccountId?.glAccountId ?? null,
            };

            if (editMode === 1) {
                // Create mode
                const createPayload = {
                    ...commonPayload,
                    glAccountTypeId: data.glAccountTypeId?.glAccountTypeId ?? null,
                    glAccountClassId: data.glAccountClassId?.glAccountClassId ?? null,
                    glResourceTypeId: data.glResourceTypeId ?? null, // string or null
                };

                const result = await createGlAccount(createPayload).unwrap();

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
                    // Include descriptions if returned by backend for proper rebinding
                    glAccountTypeDescription: result.GlAccountTypeDescription,
                    glAccountClassDescription: result.GlAccountClassDescription,
                };

                setJustCreatedAccount(createdAccount);
                onAccountCreated?.(createdAccount);
            } else {
                // Edit mode — now send all editable dropdown values
                // REFACTOR: Added glAccountTypeId, glAccountClassId, and glResourceTypeId to update payload
                // since dropdowns are now enabled for editing. Extract IDs from objects where needed.
                const updatePayload = {
                    glAccountId: account!.glAccountId!,
                    ...commonPayload,
                    glAccountTypeId: data.glAccountTypeId?.glAccountTypeId ?? null,
                    glAccountClassId: data.glAccountClassId?.glAccountClassId ?? null,
                    glResourceTypeId: data.glResourceTypeId ?? null,
                };

                const result = await updateGlAccount(updatePayload).unwrap();

                toast.success(getMessage("GL_ACCOUNT_UPDATED"));

                const updatedAccount: GlAccount = {
                    ...account!,
                    ...result,
                    accountName: result.accountName || data.accountName,
                    description: result.description ?? data.description,
                    parentGlAccountId: result.parentGlAccountId ?? data.parentGlAccountId?.glAccountId,
                    glAccountTypeId: result.glAccountTypeId ?? data.glAccountTypeId?.glAccountTypeId,
                    glAccountClassId: result.glAccountClassId ?? data.glAccountClassId?.glAccountClassId,
                    glResourceTypeId: result.glResourceTypeId ?? data.glResourceTypeId,
                };

                setFormKey(prev => prev + 1); // Reset form state
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
                                    id="glAccountTypeId"
                                    name="glAccountTypeId"
                                    label={getTranslatedLabel("accounting.glAccount.form.accountType", "Account Type *")}
                                    component={FormComboBoxVirtualGlAccountTypes}
                                    validator={requiredValidator}
                                    textField="description"
                                    dataItemKey="glAccountTypeId"
                                    //disabled={editMode > 1}
                                />
                            </Grid>

                            <Grid item xs={12} sm={4}>
                                <Field
                                    id="glAccountClassId"
                                    name="glAccountClassId"
                                    label={getTranslatedLabel("accounting.glAccount.form.accountClass", "Account Class")}
                                    component={FormComboBoxVirtualGlAccountClasses}
                                    validator={requiredValidator}
                                    textField="description"
                                    dataItemKey="glAccountClassId"
                                    //disabled={editMode > 1}
                                />
                            </Grid>

                            <Grid item xs={12} sm={4}>
                                <Field
                                    name="glResourceTypeId"
                                    label={getTranslatedLabel("accounting.glAccount.form.resourceType", "Resource Type")}
                                    component={FormDropDownList}
                                    data={glResourceTypes}
                                    textField="descriptionArabic" 
                                    dataItemKey="glResourceTypeId"
                                    //disabled={editMode > 1}
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