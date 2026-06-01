export default function DeductionPlanModal({ ... }: DeductionPlanModalProps) {

    const { getTranslatedLabel } = useTranslationHelper();

    // ==================== DEBUG LOGS ====================
    console.log("=== DeductionPlanModal Debug ===");
    console.log("Props:", {
        isReadOnly,
        isPreview,
        totalAdvance,
        initialSchedulesCount: initialSchedules.length,
        initialInstallmentCount
    });

    console.log("State:", {
        rowsCount: rows.length,
        totalScheduled,
        isValid,
        isAnyProcessed
    });
    console.log("=====================================");
    // ===================================================

    // Add inside useEffect after setting rows:
    useEffect(() => {
        console.log("DeductionPlanModal - Rows updated:", rows);
    }, [rows]);

    // Debug Generate Button
    const handleGenerateClick = () => {
        console.log("Generate Equal Plan clicked with:", { installmentCountHint, totalAdvance, isAnyProcessed });
        // ... existing code
    };

    // In the Button:
    <Button
        onClick={handleGenerateClick}
        disabled={totalAdvance <= 0 || installmentCountHint < 1 || isAnyProcessed}
    >