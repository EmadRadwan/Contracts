// In handleReset (usePayment hook)

toast.success(
    getTranslatedLabel("accounting.payments.form.reset.resetSuccess", "تم إعادة تعيين الدفعة بنجاح")
);

toast.error(
    getTranslatedLabel("accounting.payments.form.reset.resetFailed", "فشل إعادة تعيين الدفعة")
);