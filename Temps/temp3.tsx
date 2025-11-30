<Form
    key="installment-calculator-form"   // ← CRITICAL LINE
    initialValues={{
        cashPricePerM2: 25000,
        annualDiscountRate: 0.17,
        downPaymentPercentage: 0.10,
        durationYears: 9,
        installmentsPerYear: 4,
    }}
    onSubmit={handleSubmit}
    render={(formRenderProps: FormRenderProps) => (
        <FormElement>
            {/* all your fields */}

            <Box display="flex" gap={2} mt={2}>
                <Button
                    variant="contained"
                    color="primary"
                    type="submit"                     // ← correct
                    disabled={isLoading || !formRenderProps.valid}
                >
                    {isLoading ? <CircularProgress size={24} /> : t("calculate", "احسب")}
                </Button>
                <Button variant="outlined" onClick={onClose}>
                    {t("close", "إغلاق")}
                </Button>
            </Box>
        </FormElement>
    )}
/>