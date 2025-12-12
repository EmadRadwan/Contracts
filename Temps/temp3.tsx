// REFACTOR: Added "Add Cost Center" button with inline modal
// Purpose: Allow creating cost centers without leaving the payment form
// Improves: UX, reduces context switching
// Context: Uses same pattern as "New Customer" modal

const [showCreateCostCenter, setShowCreateCostCenter] = useState(false);

const handleCostCenterCreated = (newCc: { costCenterId: string; description: string }) => {
    formRenderProps.onChange('costCenterId', { value: newCc.costCenterId });
};

return (
    <>
        {/* ... existing fields ... */}

        <Grid item xs={3}>
            {loadingCostCenters ? (
                <Skeleton variant="rounded" height={56}/>
            ) : (
                <>
                    <Field
                        id="costCenterId"
                        name="costCenterId"
                        label={getTranslatedLabel(`${localizationKey}.costCenter`, "Cost Center")}
                        component={MemoizedFormComboBox2}
                        data={paymentCostCenters || []}
                        dataItemKey="costCenterId"
                        textField="description"
                    />
                    <Button
                        size="small"
                        variant="outlined"
                        color="secondary"
                        onClick={() => setShowCreateCostCenter(true)}
                        sx={{ mt: 1 }}
                    >
                        + إضافة مركز تكلفة
                    </Button>
                </>
            )}
        </Grid>

        {/* Modal */}
        <CreateCostCenterModal
            open={showCreateCostCenter}
            onClose={() => setShowCreateCostCenter(false)}
            onCreated={handleCostCenterCreated}
            isOutPayment={!!payment?.isDisbursement}
        />
    </>
);