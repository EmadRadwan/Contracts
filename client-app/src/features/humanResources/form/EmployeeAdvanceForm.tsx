/*
import React, { useEffect, useMemo, useRef, useState } from "react";
import { Form, FormElement, FormRenderProps, Field } from "@progress/kendo-react-form";
import { Paper, Grid, Button, Typography, Box } from "@mui/material";
import { toast } from "react-toastify";


import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import { requiredValidator } from "../../../app/common/form/Validators";
import {EmployeeAdvance} from "../../../app/models/humanResources/employeeAdvance";
import EmployeeAdvanceMenu from "../menu/EmployeeAdvanceMenu";
import FormInput from "../../../app/common/form/FormInput";

// -----------------------------------------------------------------
// Props
// -----------------------------------------------------------------
interface Props {
    advance?: EmployeeAdvance;
    editMode: number; // 1 = create, 2 = edit, 3 = read-only (paid/cancelled)
    cancelEdit: () => void;
    onAdvanceCreated?: (created: EmployeeAdvance) => void;
    onAdvanceUpdated?: (updated: EmployeeAdvance) => void;
}

// -----------------------------------------------------------------
// Main Form Component
// -----------------------------------------------------------------
function EmployeeAdvanceForm({
                                 advance,
                                 editMode,
                                 cancelEdit,
                                 onAdvanceCreated,
                                 onAdvanceUpdated,
                             }: Props) {
    const [createAdvance, { isLoading: isCreating }] = useAddEmployeeAdvanceMutation();
    const [updateAdvance, { isLoading: isUpdating }] = useUpdateEmployeeAdvanceMutation();

    const { getTranslatedLabel } = useTranslationHelper();
    const formRef = useRef<FormRenderProps | null>(null);

    const isReadOnly = editMode === 3;
    const isCreate = editMode === 1;

    // -----------------------------------------------------------------
    // Initial Values
    // -----------------------------------------------------------------
    const initialValues = useMemo(() => {
        if (isCreate) {
            return {
                advanceDate: new Date(),
                amount: null,
                currencyUomId: "EGP",
                installmentCount: null,
                installmentAmount: null,
                startDate: null,
                statusId: "ADVANCE_ACTIVE",
                description: "",
            };
        }

        // Edit / View mode
        return {
            advanceId: advance?.advanceId,
            advanceDate: advance?.advanceDate ? new Date(advance.advanceDate) : null,
            amount: advance?.amount ?? null,
            currencyUomId: advance?.currencyUomId ?? "EGP",
            installmentCount: advance?.installmentCount ?? null,
            installmentAmount: advance?.installmentAmount ?? null,
            startDate: advance?.startDate ? new Date(advance.startDate) : null,
            statusId: advance?.statusId ?? "ADVANCE_ACTIVE",
            description: advance?.description ?? "",
        };
    }, [advance, isCreate]);

    // -----------------------------------------------------------------
    // Submit Handler
    // -----------------------------------------------------------------
    const handleSubmit = async (values: any) => {
        try {
            // Basic client-side checks (beyond validators)
            if (!values.amount || values.amount <= 0) {
                toast.error(getTranslatedLabel("employeeAdvance.form.amountRequired", "Amount must be greater than zero."));
                return;
            }
            if (values.installmentCount > 0 && !values.startDate) {
                toast.error(getTranslatedLabel("employeeAdvance.form.startDateRequired", "Start date is required when installments > 0."));
                return;
            }

            const payload = {
                ...values,
                // Ensure numeric fields are numbers or null
                amount: Number(values.amount) || null,
                installmentCount: Number(values.installmentCount) || null,
                installmentAmount: Number(values.installmentAmount) || null,
            };

            if (isCreate) {
                const created = await createAdvance(payload).unwrap();
                toast.success(getTranslatedLabel("employeeAdvance.form.created", "Employee advance created successfully"));
                onAdvanceCreated?.(created);
            } else {
                const updated = await updateAdvance({ advanceId: advance!.advanceId, ...payload }).unwrap();
                toast.success(getTranslatedLabel("employeeAdvance.form.updated", "Employee advance updated successfully"));
                onAdvanceUpdated?.(updated);
            }
        } catch (err: any) {
            console.error("Advance save failed:", err);
            toast.error(
                err?.data?.errors
                    ? Object.values(err.data.errors).flat().join(" ")
                    : getTranslatedLabel("employeeAdvance.form.error", "Failed to save employee advance")
            );
        }
    };

    // -----------------------------------------------------------------
    // Render
    // -----------------------------------------------------------------
    return (
        <>
            <EmployeeAdvanceMenu
                selectedMenuItem="/employee-advances"
                onMenuSelect={(key) => {
                    if (key === "employeeAdvance.menu.advances") {
                        cancelEdit(); // force back to list
                    }
                }}
            />

            <Paper elevation={5} className="div-container-withBorderCurved" sx={{ p: 3 }}>
                <Typography variant="h5" gutterBottom>
                    {isCreate
                        ? getTranslatedLabel("employeeAdvance.form.new", "New Employee Advance")
                        : isReadOnly
                            ? getTranslatedLabel("employeeAdvance.form.view", "View Employee Advance")
                            : getTranslatedLabel("employeeAdvance.form.edit", "Edit Employee Advance")}
                    {advance?.advanceId && (
                        <Box component="span" sx={{ ml: 2, color: "text.secondary", fontSize: "0.8em" }}>
                            #{advance.advanceId}
                        </Box>
                    )}
                </Typography>

                <Form
                    initialValues={initialValues}
                    onSubmit={handleSubmit}
                    render={(formRenderProps: FormRenderProps) => {
                        formRef.current = formRenderProps;

                        const { valid, modified, submitting } = formRenderProps;
                        const isSubmitting = isCreating || isUpdating || submitting;

                        return (
                            <FormElement>
                                <fieldset disabled={isReadOnly || isSubmitting}>
                                    <Grid container spacing={3}>
                                        {/!* Row 1 *!/}
                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                name="advanceDate"
                                                label={getTranslatedLabel("employeeAdvance.form.advanceDate", "Advance Date *")}
                                                component={FormDatePicker}
                                                validator={requiredValidator}
                                            />
                                        </Grid>

                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                name="amount"
                                                label={getTranslatedLabel("employeeAdvance.form.amount", "Amount *")}
                                                component={FormNumericTextBox}
                                                format="n2"
                                                validator={requiredValidator}
                                            />
                                        </Grid>

                                        {/!* Row 2 *!/}
                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                name="installmentCount"
                                                label={getTranslatedLabel("employeeAdvance.form.installmentCount", "Number of Installments")}
                                                component={FormNumericTextBox}
                                                min={0}
                                                format="n0"
                                            />
                                        </Grid>

                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                name="installmentAmount"
                                                label={getTranslatedLabel("employeeAdvance.form.installmentAmount", "Installment Amount")}
                                                component={FormNumericTextBox}
                                                format="n2"
                                            />
                                        </Grid>

                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                name="startDate"
                                                label={getTranslatedLabel("employeeAdvance.form.startDate", "First Installment Date")}
                                                component={FormDatePicker}
                                            />
                                        </Grid>

                                        {/!* Row 3 – Notes *!/}
                                        <Grid item xs={12}>
                                            <Field
                                                name="description"
                                                label={getTranslatedLabel("employeeAdvance.form.description", "Description / Notes")}
                                                component={FormInput}
                                                multiline
                                                rows={3}
                                            />
                                        </Grid>

                                        {/!* Status – can be combo later if more statuses *!/}
                                        <Grid item xs={12} sm={6}>
                                            <Typography variant="caption" color="text.secondary">
                                                {getTranslatedLabel("employeeAdvance.form.status", "Status")}:{" "}
                                                {advance?.statusDescription || "Active"}
                                            </Typography>
                                        </Grid>
                                    </Grid>
                                </fieldset>

                                {/!* Actions *!/}
                                <Box sx={{ mt: 4, display: "flex", justifyContent: "flex-end", gap: 2 }}>
                                    <Button
                                        variant="outlined"
                                        color="inherit"
                                        onClick={cancelEdit}
                                        disabled={isSubmitting}
                                    >
                                        {getTranslatedLabel("general.cancel", "Cancel")}
                                    </Button>

                                    {!isReadOnly && (
                                        <Button
                                            type="submit"
                                            variant="contained"
                                            color="primary"
                                            disabled={!valid || !modified || isSubmitting}
                                        >
                                            {isCreate
                                                ? getTranslatedLabel("general.create", "Create")
                                                : getTranslatedLabel("general.save", "Save")}
                                        </Button>
                                    )}
                                </Box>
                            </FormElement>
                        );
                    }}
                />
            </Paper>
        </>
    );
}

export default React.memo(EmployeeAdvanceForm);*/
