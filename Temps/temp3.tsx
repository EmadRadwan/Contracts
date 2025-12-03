// REFACTOR: handleSubmit – now bullet-proof against { isSuccess: false } responses
const handleSubmit = async (data: any) => {
    setButtonFlag(true);
    setSubmitError(null);

    try {
        const payload = {
            partyId: data.partyId?.fromPartyId ?? data.partyId,
            projectId: data.projectId?.projectId ?? data.projectId,
            accountLimit: data.accountLimit,
            fromDate: data.fromDate,
            thruDate: data.thruDate || null,
            description: data.description || "",
        };

        const result = await createBillingAccount(payload).unwrap();

        // ─────────────────────────────────────────────────────
        // CRITICAL: Check the actual result from your API
        // ─────────────────────────────────────────────────────
        if (!result?.isSuccess) {
            // Backend explicitly told us it failed
            const errorMsg = result?.error || result?.message || "فشل إنشاء حساب الأجل";
            throw new Error(errorMsg); // go to catch block
        }

        // Only now we know it REALLY succeeded
        toast.success("تم إنشاء حساب الأجل بنجاح");

        if (onBillingAccountCreated) {
            // Pass the whole successful response (contains the new account)
            onBillingAccountCreated(result);
        }
        if (setEditMode) setEditMode(2);

    } catch (error: any) {
        // This now catches BOTH network errors AND explicit { isSuccess: false } cases
        const errorMessage =
            error?.message ||                    // from the throw above
            error?.data?.error ||
            error?.data?.message ||
            "حدث خطأ أثناء إنشاء حساب الأجل";

        toast.error(errorMessage);
        setSubmitError(errorMessage);

        // Form stays exactly as-is (editMode stays 1 → no remount → values preserved)
    } finally {
        setButtonFlag(false);
    }
};