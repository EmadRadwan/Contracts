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