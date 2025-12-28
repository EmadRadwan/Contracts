const handleApplyPaymentPlan = useCallback((
    installments: Array<{ dueDate: string; amount: number; isAdvance: boolean }>
) => {
    // 1. Store installments
    setCustomInstallments(installments);

    // 2. Calculate actual advance sum from the plan
    const actualAdvanceSum = installments
        .filter(inst => inst.isAdvance)
        .reduce((sum, inst) => sum + inst.amount, 0);

    // 3. Update the form field directly using formRef
    if (formRef.current) {
        formRef.current.onChange("advancePayment", {
            value: actualAdvanceSum
        });

        // Optional: Also update maintenance deposit if you want it to follow new total logic
        // But since totalPrice is fixed, it's usually unchanged
    }

    // 4. Close modal and notify
    setShowPaymentPlan(false);
    toast.success(getTranslatedLabel("salesRequest.form.paymentPlanApplied", "Payment plan applied successfully"));
}, [getTranslatedLabel]);