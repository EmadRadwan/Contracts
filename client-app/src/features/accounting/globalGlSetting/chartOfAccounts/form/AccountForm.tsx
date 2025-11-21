
import React, { useMemo } from "react";
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
} from "@mui/material";
import { toast } from "react-toastify";

import { requiredValidator } from "../../../../../app/common/form/Validators";
import FormInput from "../../../../../app/common/form/FormInput";
import { FormDropDownTreeGlAccount2 } from "../../../../../app/common/form/FormDropDownTreeGlAccount2";



import { GlAccount } from "../../../../../app/models/accounting/globalGlSettings";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import {FormDropDownList} from "../../../../../app/common/form/MemoizedFormDropDownList2";

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
].map((id) => ({ glAccountTypeId: id }));

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
].map((id) => ({ glAccountClassId: id }));

const glResourceTypes = ["MONEY", "INVENTORY_ITEM", "FIXED_ASSET", "SERVICE"].map(
    (id) => ({ glResourceTypeId: id })
);

const AccountForm: React.FC<Props> = ({
                                          account,
                                          editMode,
                                          cancelEdit,
                                          onAccountCreated,
                                          onAccountUpdated,
                                      }) => {
    const { getTranslatedLabel } = useTranslationHelper();

    /*const [addAccount, { isLoading: isCreating }] = useAddGlAccountMutation();
    const [updateAccount, { isLoading: isUpdating }] = useUpdateGlAccountMutation();
*/
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
        try {
            // Flatten parent object → only the ID is needed on server
            const payload = {
                ...data,
                parentGlAccountId: data.parentGlAccountId?.glAccountId ?? null,
            };

            let result: GlAccount;

            // if (editMode === 2) {
            //     // edit
            //     result = await updateAccount(payload).unwrap();
            //     toast.success("GL Account updated successfully");
            //     onAccountUpdated?.(result);
            // } else {
            //     // create
            //     result = await addAccount(payload).unwrap();
            //     toast.success("GL Account created successfully");
            //     onAccountCreated?.(result);
            // }
        } catch (err: any) {
            const msg =
                err?.data?.message ||
                err?.data?.title ||
                "Failed to save GL Account";
            toast.error(msg);
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
                            <Grid item xs={12} sm={6}>
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
                            </Grid>

                            {/* Row 2 */}
                            <Grid item xs={12} sm={6}>
                                <Field
                                    name="accountName"
                                    label={getTranslatedLabel("glAccount.name", "Account Name (EN *")}
                                    component={FormInput}
                                    validator={requiredValidator}
                                />
                            </Grid>

                            <Grid item xs={12} sm={6}>
                                <Field
                                    name="accountNameArabic"
                                    label={getTranslatedLabel("glAccount.nameArabic", "Account Name (AR)")}
                                    component={FormInput}
                                />
                            </Grid>

                            {/* Row 3 – Dropdowns */}
                            <Grid item xs={12} sm={4}>
                                <Field
                                    name="glAccountTypeId"
                                    label={getTranslatedLabel("glAccount.type", "Account Type *")}
                                    component={FormDropDownList}
                                    data={glAccountTypes}
                                    textField="glAccountTypeId"
                                    dataItemKey="glAccountTypeId"
                                    validator={requiredValidator}
                                />
                            </Grid>

                            <Grid item xs={12} sm={4}>
                                <Field
                                    name="glAccountClassId"
                                    label={getTranslatedLabel("glAccount.class", "Account Class")}
                                    component={FormDropDownList}
                                    data={glAccountClasses}
                                    textField="glAccountClassId"
                                    dataItemKey="glAccountClassId"
                                />
                            </Grid>

                            <Grid item xs={12} sm={4}>
                                <Field
                                    name="glResourceTypeId"
                                    label={getTranslatedLabel("glAccount.resourceType", "Resource Type")}
                                    component={FormDropDownList}
                                    data={glResourceTypes}
                                    textField="glResourceTypeId"
                                    dataItemKey="glResourceTypeId"
                                />
                            </Grid>

                            {/* Parent Account – Tree dropdown */}
                            <Grid item xs={12}>
                                <Field
                                    id="parentGlAccountId"
                                    name="parentGlAccountId"
                                    label={getTranslatedLabel("glAccount.parent", "Parent Account")}
                                    component={FormDropDownTreeGlAccount2}
                                    dataItemKey="glAccountId"
                                    textField="text"
                                    selectField="selected"
                                    expandField="expanded"
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

                            {/* Buttons */}
                            <Grid item xs={12}>
                                <Box sx={{ mt: 2 }}>
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        type="submit"
                                        // disabled={
                                        //     !formRenderProps.allowSubmit ||
                                        //     isCreating ||
                                        //     isUpdating
                                        // }
                                    >
                                        {editMode === 1 ? "Create" : "Update"} Account
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