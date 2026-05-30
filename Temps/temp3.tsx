import React, {useEffect, useMemo, useRef, useState} from "react";
import { Form, FormElement, FormRenderProps, Field } from "@progress/kendo-react-form";
import {Paper, Grid, Button, Typography, Box} from "@mui/material";
import { toast } from "react-toastify";

import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import { requiredValidator } from "../../../app/common/form/Validators";
import {EmployeeAdvance, Schedule} from "../../../app/models/humanResources/employeeAdvance";
import EmployeeAdvanceMenu from "../menu/EmployeeAdvanceMenu";
import {EmployeeAdvanceActionsMenu} from "../menu/EmployeeAdvanceActionsMenu";
import FormInput from "../../../app/common/form/FormInput";
import {useCreateEmployeeAdvanceMutation, useUpdateEmployeeAdvanceMutation} from "../../../app/store/apis";
import {MemoizedFormComboBox2} from "../../../app/common/form/FormComboBox2";
import {FormComboBoxVirtualPartyEmployee} from "../../../app/common/form/FormComboBoxVirtualPartyEmployee";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import DeductionPlanModal from "./DeductionPlanModal";
import LoadingComponent from "../../../app/layout/LoadingComponent";

// -----------------------------------------------------------------
// Props
// -----------------------------------------------------------------
interface DeductionSchedule {
    dueDate: string;
    scheduledAmount: number;
    payrollInvoiceId?: string | null;
}

interface Props {
    advance?: EmployeeAdvance & { schedules?: Schedule[] };
    editMode: number; // 1=create, 2=edit, 3=read-only
    cancelEdit: () => void;
    onAdvanceCreated?: (created: EmployeeAdvance) => void;
    onAdvanceUpdated?: (updated: EmployeeAdvance) => void;
}

// -----------------------------------------------------------------
// Main Form
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

    const isCreate = editMode === 1;
    const isEdit = editMode === 2;

    // ─────────────────────────────────────────────────────────────
    // New Read-Only Logic (Aligned with Approval Flow)
    // ─────────────────────────────────────────────────────────────
    const isReadOnly = editMode === 3
        || advance?.statusId === "ADVANCE_FULLY_PAID"
        || advance?.statusId === "ADVANCE_CANCELLED"
        || advance?.statusId === "ADVANCE_REJECTED";

    const canEditSchedules = advance?.statusId === "ADVANCE_REQUESTED"
        || advance?.statusId === "ADVANCE_APPROVED"
        || advance?.statusId === "ADVANCE_ACTIVE"
        || advance?.statusId === "ADVANCE_PARTIALLY_PAID";

    const employeeAdvanceTypes = [
        {advanceTypeId: "EMPLOYEE_ADVANCE", description: "سلفة راتب"},
        {advanceTypeId: "EMPLOYEE_LONG_TERM_ADVANCE", description: "سلفة طويلة الأجل "}
    ];

    // Load existing schedules for long-term advances
    useEffect(() => {
        if (!isCreate && advance?.advanceTypeId === "EMPLOYEE_LONG_TERM_ADVANCE" && advance?.schedules?.length) {
            const loadedSchedules = advance.schedules.map((s) => ({
                dueDate: s.dueDate.split("T")[0],
                scheduledAmount: Number(s.scheduledAmount),
                payrollInvoiceId: s.payrollInvoiceId
            }));
            setCustomSchedules(loadedSchedules);
        }
    }, [advance, isCreate]);

    // Initial Values
    const initialValues = useMemo(() => {
        if (isCreate) {
            return {
                advanceDate: new Date(),
                amount: null,
                currencyUomId: "EGP",
                installmentCount: null,
                startDate: null,
                statusId: "ADVANCE_REQUESTED",
                advanceTypeId: "EMPLOYEE_ADVANCE",
                description: "",
            };
        }

        return {
            advanceId: advance?.advanceId,
            advanceDate: advance?.advanceDate ? new Date(advance.advanceDate) : null,
            amount: advance?.amount ?? null,
            currencyUomId: advance?.currencyUomId ?? "EGP",
            installmentCount: advance?.installmentCount ?? null,
            startDate: advance?.startDate ? new Date(advance.startDate) : null,
            statusId: advance?.statusId ?? "ADVANCE_REQUESTED",
            advanceTypeId: advance?.advanceTypeId ?? "EMPLOYEE_ADVANCE",
            description: advance?.description ?? "",
            partyId: {"fromPartyId": advance?.partyId, "fromPartyName": advance?.employeeName},
        };
    }, [advance, isCreate]);

    // Submit Handler - FIXED PAYLOAD
    const handleSubmit = async (values: any) => {
        try {
            const currentAdvanceType = formRef.current?.valueGetter("advanceTypeId");
            const isLongTerm = currentAdvanceType === "EMPLOYEE_LONG_TERM_ADVANCE";

            // Client validations
            if (!values.amount || values.amount <= 0) {
                toast.error("يجب أن يكون المبلغ أكبر من صفر");
                return;
            }

            if (isLongTerm && customSchedules.length === 0) {
                toast.error("السلفة طويلة الأجل تتطلب جدول سداد");
                return;
            }

            const payload = {
                advanceDto: {
                    AdvanceId: isCreate ? undefined : advance!.advanceId,
                    PartyId: values.partyId?.fromPartyId,
                    AdvanceDate: values.advanceDate,
                    Amount: Number(values.amount),
                    AdvanceTypeId: values.advanceTypeId,
                    Description: values.description,
                    InstallmentCount: isLongTerm ? customSchedules.length : values.installmentCount,
                    StartDate: isLongTerm ? null : values.startDate,
                    CustomDeductionSchedules: isLongTerm && customSchedules.length > 0 ? customSchedules : undefined,
                },
                Language: "ar"
            };

            if (isCreate) {
                const created = await createAdvance(payload).unwrap();
                toast.success("تم إنشاء السلفة بنجاح وهي في انتظار الموافقة");
                onAdvanceCreated?.(created);
            } else {
                const updated = await updateAdvance(payload).unwrap();
                toast.success("تم تحديث السلفة بنجاح");
                onAdvanceUpdated?.(updated);
            }
        } catch (err: any) {
            console.error(err);
            toast.error(err?.data?.title || "فشل في حفظ السلفة");
        }
    };

    return (
        <>
            <EmployeeAdvanceMenu ... />

            <Paper elevation={5} sx={{ p: 3 }}>
                <Typography variant="h5" gutterBottom display="flex" justifyContent="space-between" alignItems="center">
                    <Box>
                        {isCreate ? "سلفة جديدة" : isReadOnly ? "عرض السلفة" : "تعديل السلفة"}
                        {advance?.advanceId && <span style={{fontSize: '0.85em', color: '#666'}}> #{advance.advanceId}</span>}
                    </Box>

                    {!isCreate && (
                        <EmployeeAdvanceActionsMenu
                            advanceId={advance?.advanceId}
                            currentStatusId={advance?.statusId}
                            disabled={isCreating || isUpdating}
                            onAdvanceUpdated={onAdvanceUpdated}
                            onAdvanceDeleted={cancelEdit}
                        />
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

                        return (
                            <FormElement>
                                <Grid container spacing={2}>
                                    {/* Employee, Type, Date, Amount fields ... (keep as is) */}

                                    {/* Long-term deduction plan */}
                                    {isLongTermAdvance && (
                                        <Grid item xs={12} sx={{ mt: 2 }}>
                                            <Box display="flex" alignItems="center" gap={2}>
                                                <Typography variant="subtitle1">
                                                    {customSchedules.length > 0
                                                        ? `خطة السداد (${customSchedules.length} أقساط)`
                                                        : "لم يتم تحديد خطة السداد بعد"}
                                                </Typography>
                                                <Button
                                                    variant="outlined"
                                                    size="small"
                                                    onClick={() => setShowDeductionPlan(true)}
                                                    disabled={!isLongTermAdvance || totalAmount <= 0 || (!canEditSchedules && !isCreate)}
                                                >
                                                    {isReadOnly ? "عرض الخطة" : customSchedules.length > 0 ? "تعديل الخطة" : "إنشاء خطة السداد"}
                                                </Button>
                                            </Box>
                                        </Grid>
                                    )}

                                    {/* Status */}
                                    <Grid item xs={12}>
                                        <Typography variant="caption" color="text.secondary">
                                            الحالة: {advance?.statusDescription || "غير محدد"}
                                        </Typography>
                                    </Grid>
                                </Grid>

                                {/* Submit Buttons */}
                                <Box sx={{ mt: 4, display: "flex", justifyContent: "flex-end", gap: 2 }}>
                                    <Button onClick={cancelEdit} disabled={isSubmitting}>
                                        إلغاء
                                    </Button>
                                    {!isReadOnly && (
                                        <Button
                                            type="submit"
                                            variant="contained"
                                            disabled={!valid || !modified || isSubmitting}
                                        >
                                            {isCreate ? "إنشاء السلفة" : "حفظ التعديلات"}
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
                                                payrollInvoiceId: s.payrollInvoiceId
                                            }))}
                                            onApply={(schedules) => {
                                                setCustomSchedules(schedules);
                                                setShowDeductionPlan(false);
                                                toast.success("تم تطبيق خطة السداد");
                                            }}
                                            isReadOnly={!canEditSchedules}
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