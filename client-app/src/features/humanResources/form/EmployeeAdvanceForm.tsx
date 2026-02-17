import React, {useMemo, useRef, useState} from "react";
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
import {useCreateEmployeeAdvanceMutation, useUpdateEmployeeAdvanceMutation} from "../../../app/store/apis";
import {MemoizedFormComboBox2} from "../../../app/common/form/FormComboBox2";
import {FormComboBoxVirtualPartyEmployee} from "../../../app/common/form/FormComboBoxVirtualPartyEmployee";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import DeductionPlanModal from "./DeductionPlanModal";

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
    const [createAdvance, { isLoading: isCreating }] = useCreateEmployeeAdvanceMutation();
    const [updateAdvance, { isLoading: isUpdating }] = useUpdateEmployeeAdvanceMutation();
    const [showDeductionPlan, setShowDeductionPlan] = useState(false);
    const [customSchedules, setCustomSchedules] = useState<
        Array<{ dueDate: string; scheduledAmount: number }>
    >([]);
    const { getTranslatedLabel } = useTranslationHelper();
    const formRef = useRef<FormRenderProps | null>(null);

    const isReadOnly = editMode === 3;
    const isCreate = editMode === 1;
    
    const employeeAdvanceTypes = [{advanceTypeId: "EMPLOYEE_ADVANCE", description: "سلفة راتب"}, {advanceTypeId: "EMPLOYEE_LONG_TERM_ADVANCE", description: "سلفة طويلة الأجل "}]

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
            const currentAdvanceType = formRef.current?.valueGetter("advanceTypeId");
            const isLongTerm = currentAdvanceType === "EMPLOYEE_LONG_TERM_ADVANCE";

            // Basic client-side checks (beyond validators)
            if (!values.amount || values.amount <= 0) {
                toast.error(getTranslatedLabel("party.employeeAdvance.form.amountRequired", "Amount must be greater than zero."));
                return;
            }
            if (values.installmentCount > 0 && !values.startDate) {
                toast.error(getTranslatedLabel("party.employeeAdvance.form.startDateRequired", "Start date is required when installments > 0."));
                return;
            }

            if (isLongTerm) {
                if (customSchedules.length === 0) {
                    toast.error(
                        getTranslatedLabel(
                            "party.employeeAdvance.form.deductionPlanRequired",
                            "Long-term advance requires a deduction plan. Please create one."
                        )
                    );
                    return;
                }

                const scheduledTotal = customSchedules.reduce((sum, s) => sum + s.scheduledAmount, 0);
                if (Math.abs(scheduledTotal - Number(values.amount)) > 0.01) {
                    toast.error(
                        getTranslatedLabel(
                            "party.employeeAdvance.form.planMismatch",
                            "The total of the deduction plan does not match the advance amount."
                        )
                    );
                    return;
                }
            }

            const payload = {
                ...values,
                amount: Number(values.amount) || null,
                partyId: values.partyId?.fromPartyId,
                installmentCount: isLongTerm ? customSchedules.length : values.installmentCount,
                // You can decide whether to send these or null them when custom plan exists
                startDate: isLongTerm ? null : values.startDate ? new Date(values.startDate).toISOString() : null,

                ...(isLongTerm && customSchedules.length > 0 && {
                    customDeductionSchedules: customSchedules,
                }),
            };

            if (isCreate) {
                const created = await createAdvance(payload).unwrap();
                toast.success(getTranslatedLabel("party.employeeAdvance.form.created", "Employee advance created successfully"));
                onAdvanceCreated?.(created);
            } else {
                const updated = await updateAdvance({ advanceId: advance!.advanceId, ...payload }).unwrap();
                toast.success(getTranslatedLabel("party.employeeAdvance.form.updated", "Employee advance updated successfully"));
                onAdvanceUpdated?.(updated);
            }
        } catch (err: any) {
            console.error("Advance save failed:", err);
            toast.error(
                err?.data?.errors
                    ? Object.values(err.data.errors).flat().join(" ")
                    : getTranslatedLabel("party.employeeAdvance.form.error", "Failed to save employee advance")
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
                    if (key === "party.employeeAdvance.menu.advances") {
                        cancelEdit(); // force back to list
                    }
                }}
            />

            <Paper elevation={5} className="div-container-withBorderCurved" sx={{ p: 3 }}>
                <Typography variant="h5" gutterBottom>
                    {isCreate
                        ? getTranslatedLabel("party.employeeAdvance.form.new", "New Employee Advance")
                        : isReadOnly
                            ? getTranslatedLabel("party.employeeAdvance.form.view", "View Employee Advance")
                            : getTranslatedLabel("party.employeeAdvance.form.edit", "Edit Employee Advance")}
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

                        const { valid, modified , valueGetter} = formRenderProps;
                        const isSubmitting = isCreating || isUpdating;
                        const advanceTypeId = valueGetter("advanceTypeId");
                        const isLongTermAdvance = advanceTypeId === "EMPLOYEE_LONG_TERM_ADVANCE";
                        const totalAmount = Number(valueGetter("amount") || 0);
                        const canOpenPlan = isLongTermAdvance && totalAmount > 0;

                        // Fields should be disabled in read-only mode OR when submitting OR when NOT long-term
                        const isInstallmentDisabled = !isLongTermAdvance || isReadOnly || isSubmitting;

                        return (
                            <FormElement>
                                <fieldset disabled={isReadOnly || isSubmitting}>
                                    <Grid container spacing={2}>
                                        {/* Row 1 */}
                                        <Grid item  xs={12} sm={6} md={4}>
                                            <Field
                                                id="partyId"
                                                name="partyId"
                                                component={FormComboBoxVirtualPartyEmployee}
                                                label={getTranslatedLabel("salesRequest.form.employee", "Employee")}
                                                valueField="fromPartyId"
                                                textField="fromPartyName"
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                id="advanceTypeId"
                                                name="advanceTypeId"
                                                label={getTranslatedLabel(
                                                    "party.employeeAdvance.form.advanceType",
                                                    "Advance Type *"
                                                )}
                                                component={MemoizedFormComboBox2}
                                                dataItemKey="advanceTypeId"
                                                textField="description"
                                                data={employeeAdvanceTypes}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                name="advanceDate"
                                                label={getTranslatedLabel("party.employeeAdvance.form.advanceDate", "Advance Date *")}
                                                component={FormDatePicker}
                                                validator={requiredValidator}
                                            />
                                        </Grid>

                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                name="amount"
                                                label={getTranslatedLabel("party.employeeAdvance.form.amount", "Amount *")}
                                                component={FormNumericTextBox}
                                                format="n2"
                                                validator={requiredValidator}
                                            />
                                        </Grid>

                                        {/* Row 2 */}
                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                name="installmentCount"
                                                label={getTranslatedLabel("party.employeeAdvance.form.installmentCount", "Number of Installments")}
                                                component={FormNumericTextBox}
                                                min={0}
                                                format="n0"
                                                disabled={isInstallmentDisabled}
                                            />
                                        </Grid>

                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                name="startDate"
                                                label={getTranslatedLabel("party.employeeAdvance.form.startDate", "First Installment Date")}
                                                component={FormDatePicker}
                                                disabled={isInstallmentDisabled}
                                            />
                                        </Grid>

                                        {/* Row 3 – Notes */}
                                        <Grid item xs={12}>
                                            <Field
                                                name="description"
                                                label={getTranslatedLabel("party.employeeAdvance.form.description", "Description / Notes")}
                                                component={FormInput}
                                                multiline
                                                rows={3}
                                            />
                                        </Grid>

                                        {isLongTermAdvance && (
                                            <Grid item xs={12} sx={{ mt: 2 }}>
                                                <Box display="flex" alignItems="center" gap={2}>
                                                    <Typography variant="subtitle1">
                                                        {customSchedules.length > 0
                                                            ? `Custom deduction plan (${customSchedules.length} installments)`
                                                            : "No deduction plan defined yet"}
                                                    </Typography>

                                                    <Button
                                                        variant="outlined"
                                                        size="small"
                                                        onClick={() => setShowDeductionPlan(true)}
                                                        disabled={!canOpenPlan}
                                                    >
                                                        {customSchedules.length > 0 ? "Edit Plan" : "Create Deduction Plan"}
                                                    </Button>
                                                </Box>

                                                {customSchedules.length > 0 && (
                                                    <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: "block" }}>
                                                        Total scheduled: {customSchedules.reduce((s, r) => s + r.scheduledAmount, 0).toLocaleString()} EGP
                                                    </Typography>
                                                )}
                                            </Grid>
                                        )}

                                        {/* Status – can be combo later if more statuses */}
                                        <Grid item xs={12} sm={6}>
                                            <Typography variant="caption" color="text.secondary">
                                                {getTranslatedLabel("party.employeeAdvance.form.status", "Status")}:{" "}
                                                {advance?.statusDescription || "Active"}
                                            </Typography>
                                        </Grid>
                                    </Grid>
                                </fieldset>

                                {/* Actions */}
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
                                {showDeductionPlan && (
                                    <ModalContainer show={showDeductionPlan} onClose={() => setShowDeductionPlan(false)} width={950}>
                                        <DeductionPlanModal
                                            onClose={() => setShowDeductionPlan(false)}
                                            totalAdvance={totalAmount}
                                            initialSchedules={customSchedules.map((s, idx) => ({
                                                id: `s-${idx}`,
                                                number: idx + 1,
                                                dueDate: s.dueDate,
                                                scheduledAmount: s.scheduledAmount,
                                            }))}
                                            initialInstallmentCount={Number(valueGetter("installmentCount")) || 12}
                                            initialStartDate={valueGetter("startDate")}
                                            onApply={(schedules) => {
                                                setCustomSchedules(schedules);
                                                setShowDeductionPlan(false);
                                                toast.success(getTranslatedLabel("employeeAdvance.deductionPlan.applied", "Deduction plan applied"));

                                                // Optional: sync back to main form fields
                                                if (formRef.current) {
                                                    formRef.current.onChange("installmentCount", { value: schedules.length });
                                                    if (schedules.length > 0) {
                                                        formRef.current.onChange("startDate", {
                                                            value: new Date(schedules[0].dueDate),
                                                        });
                                                    }
                                                }
                                            }}
                                            isPreview={customSchedules.length > 0}
                                        />
                                    </ModalContainer>
                                )}
                            </FormElement>
                        );
                    }}
                />
                
            </Paper>
        </>
    );
}

export default React.memo(EmployeeAdvanceForm);
