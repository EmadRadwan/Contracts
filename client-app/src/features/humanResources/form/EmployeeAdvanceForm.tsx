import React, {useEffect, useMemo, useRef, useState} from "react";
import { Form, FormElement, FormRenderProps, Field } from "@progress/kendo-react-form";
import {Paper, Grid, Button, Typography, Box, Alert} from "@mui/material";
import { toast } from "react-toastify";


import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import { requiredValidator } from "../../../app/common/form/Validators";
import {EmployeeAdvance, Schedule} from "../../../app/models/humanResources/employeeAdvance";
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
interface DeductionSchedule {
    dueDate: string;      // YYYY-MM-DD
    scheduledAmount: number;
}

interface Props {
    advance?: EmployeeAdvance & { schedules?: Schedule[] }; // union with detail shape
    editMode: number; // 1=create, 2=edit, 3=read-only
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
    const [customSchedules, setCustomSchedules] = useState<DeductionSchedule[]>([]);

    const { getTranslatedLabel } = useTranslationHelper();
    const formRef = useRef<FormRenderProps | null>(null);

    const isReadOnly = editMode === 3 || advance?.statusId === "ADVANCE_APPROVED";
    const isCreate = editMode === 1;
    const isEdit = editMode === 2;
    
    const employeeAdvanceTypes = [{advanceTypeId: "EMPLOYEE_ADVANCE", description: "سلفة راتب"}, {advanceTypeId: "EMPLOYEE_LONG_TERM_ADVANCE", description: "سلفة طويلة الأجل "}]


    // -----------------------------------------------------------------
    // Initialize customSchedules from queried data (only on mount/edit)
    // -----------------------------------------------------------------
    useEffect(() => {
        if (!isCreate && advance?.advanceTypeId === "EMPLOYEE_LONG_TERM_ADVANCE" && advance?.schedules?.length) {
            // Load existing schedules from detail query
            const loadedSchedules = advance.schedules.map((s) => ({
                dueDate: s.dueDate.split("T")[0], // ensure YYYY-MM-DD
                scheduledAmount: Number(s.scheduledAmount),
            }));

            setCustomSchedules(loadedSchedules);
        }
    }, [advance, isCreate]);
    
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
                advanceTypeId: "EMPLOYEE_ADVANCE",
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
            advanceTypeId: advance?.advanceTypeId ?? "EMPLOYEE_ADVANCE",
            description: advance?.description ?? "",
            partyId: {"fromPartyId": advance?.partyId, "fromPartyName": advance?.employeeName},
        };
    }, [advance, isCreate]);

    // -----------------------------------------------------------------
    // Submit Handler
    // -----------------------------------------------------------------
    const handleSubmit = async (values: any) => {
        try {
            const currentAdvanceType = formRef.current?.valueGetter("advanceTypeId");
            const isLongTerm = currentAdvanceType === "EMPLOYEE_LONG_TERM_ADVANCE";

            // Client-side validation
            if (!values.amount || values.amount <= 0) {
                toast.error(getTranslatedLabel("party.employeeAdvance.form.amountRequired", "Amount must be greater than zero."));
                return;
            }

            if (!isLongTerm && values.installmentCount > 0 && !values.startDate) {
                toast.error(getTranslatedLabel("party.employeeAdvance.form.startDateRequired", "Start date is required when installments > 0."));
                return;
            }

            if (isLongTerm) {
                if (customSchedules.length === 0) {
                    toast.error(getTranslatedLabel("party.employeeAdvance.form.deductionPlanRequired", "Long-term advance requires a deduction plan."));
                    return;
                }

                const scheduledTotal = customSchedules.reduce((sum, s) => sum + s.scheduledAmount, 0);
                if (Math.abs(scheduledTotal - Number(values.amount)) > 0.01) {
                    toast.error(getTranslatedLabel("party.employeeAdvance.form.planMismatch", "Deduction plan total does not match advance amount."));
                    return;
                }
            }

            const payload = {
                ...values,
                amount: Number(values.amount) || null,
                partyId: values.partyId?.fromPartyId,
                installmentCount: isLongTerm ? customSchedules.length : values.installmentCount,
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
            
            let errorMessage = getTranslatedLabel("party.employeeAdvance.form.error", "Failed to save employee advance");
            
            if (err?.data?.errors) {
                errorMessage = Object.values(err.data.errors).flat().join(" ");
            } else if (err?.data?.errorMessage) {
                errorMessage = err.data.errorMessage;
            } else if (typeof err?.data === 'string') {
                errorMessage = err.data;
            }
            
            toast.error(errorMessage);
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

                        const { valid, modified, valueGetter } = formRenderProps;
                        const isSubmitting = isCreating || isUpdating;
                        const advanceTypeId = valueGetter("advanceTypeId");
                        const isLongTermAdvance = advanceTypeId === "EMPLOYEE_LONG_TERM_ADVANCE";
                        const totalAmount = Number(valueGetter("amount") || 0);
                        const canOpenPlan = isLongTermAdvance && totalAmount > 0;

                        const hasExistingPlan = !isCreate && advance?.schedules?.length > 0;
                        const isInstallmentDisabled = !isLongTermAdvance || isReadOnly || isSubmitting;

                        return (
                            <FormElement>
                                <fieldset disabled={isReadOnly || isSubmitting}>
                                    <Grid container spacing={2}>
                                        {/* Employee, Type, Date, Amount */}
                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                id="partyId"
                                                name="partyId"
                                                component={FormComboBoxVirtualPartyEmployee}
                                                label={getTranslatedLabel("party.employeeAdvance.form.employee", "Employee")}
                                                valueField="fromPartyId"
                                                textField="fromPartyName"
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={12} sm={6} md={4}>
                                            <Field
                                                id="advanceTypeId"
                                                name="advanceTypeId"
                                                label={getTranslatedLabel("party.employeeAdvance.form.advanceType", "Advance Type *")}
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

                                        {/* Installments & Start Date (disabled for long-term) */}
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

                                        {/* Description */}
                                        <Grid item xs={12}>
                                            <Field
                                                name="description"
                                                label={getTranslatedLabel("party.employeeAdvance.form.description", "Description / Notes")}
                                                component={FormInput}
                                                multiline
                                                rows={3}
                                            />
                                        </Grid>

                                        {/* Long-term deduction plan section */}
                                        {isLongTermAdvance && (
                                            <Grid item xs={12} sx={{ mt: 2 }}>
                                                <Box display="flex" alignItems="center" gap={2} flexWrap="wrap">
                                                    <Typography variant="subtitle1">
                                                        {customSchedules.length > 0
                                                            ? `Custom deduction plan (${customSchedules.length} installments)`
                                                            : "No deduction plan defined yet"}
                                                    </Typography>

                                                    {!isReadOnly && (
                                                        <Button
                                                            variant="outlined"
                                                            size="small"
                                                            onClick={() => setShowDeductionPlan(true)}
                                                            disabled={!canOpenPlan}
                                                        >
                                                            {customSchedules.length > 0 ? "Edit Plan" : "Create Deduction Plan"}
                                                        </Button>
                                                    )}
                                                </Box>

                                                {customSchedules.length > 0 && (
                                                    <>
                                                        <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: "block" }}>
                                                            Total scheduled:{" "}
                                                            {customSchedules
                                                                .reduce((s, r) => s + r.scheduledAmount, 0)
                                                                .toLocaleString(undefined, { minimumFractionDigits: 2 })}{" "}
                                                            EGP
                                                        </Typography>

                                                        {hasExistingPlan && isEdit && (
                                                            <Alert severity="info" sx={{ mt: 1 }}>
                                                                This advance has an existing deduction plan loaded from the server.
                                                            </Alert>
                                                        )}
                                                    </>
                                                )}
                                            </Grid>
                                        )}

                                        {/* Status display */}
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

                                {/* Deduction Plan Modal */}
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
                                            initialInstallmentCount={customSchedules.length || Number(valueGetter("installmentCount")) || 12}
                                            initialStartDate={
                                                customSchedules.length > 0
                                                    ? new Date(customSchedules[0].dueDate)
                                                    : valueGetter("startDate")
                                            }
                                            onApply={(schedules) => {
                                                setCustomSchedules(schedules);
                                                setShowDeductionPlan(false);
                                                toast.success(getTranslatedLabel("employeeAdvance.deductionPlan.applied", "Deduction plan applied"));

                                                // Sync form fields
                                                if (formRef.current) {
                                                    formRef.current.onChange("installmentCount", { value: schedules.length });
                                                    if (schedules.length > 0) {
                                                        formRef.current.onChange("startDate", { value: new Date(schedules[0].dueDate) });
                                                    }
                                                }
                                            }}
                                            isPreview={isReadOnly || customSchedules.length > 0}
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