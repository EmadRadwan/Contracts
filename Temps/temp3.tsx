// Inside the Form render function
// ==================== INSIDE FORM RENDER ====================
render={(formRenderProps: FormRenderProps) => {
    const { valid, valueGetter } = formRenderProps;
    const amount = valueGetter("amount") || 0;

    // === Updated Logic ===
    const currentPaymentTypeId = payment?.paymentTypeId;
    const shouldCheckBalance = needsBalanceCheck(currentPaymentTypeId);

    const hasBillingAccountIssue =
        shouldCheckBalance &&
        balanceData &&
        (balanceData.initialBalance === 0 || amount > balanceData.remainingBalance);

    const isSubmitDisabled = !valid || isFormDisabled || balanceLoading || hasBillingAccountIssue;

    // ... other handlers ...

    return (
        <FormElement>
            <fieldset className={`k-form-fieldset ${isFormDisabled ? 'grid-disabled' : 'grid-normal'}`}>

                {/* ... other fields ... */}

                {/* Project Field - Updated Validator */}
                <Grid item xs={3}>
                    <Field
                        id="projectId"
                        name="projectId"
                        component={FormComboBoxVirtualProject}
                        label={getTranslatedLabel("projects.certificate.form.project", "Project")}
                        dataItemKey="projectId"
                        textField="ProjectName"
                        // Updated: Now required for both ADVANCE and VENDOR_PAYMENT
                        validator={(value) =>
                            shouldCheckBalance ? requiredValidator(value) : undefined
                        }
                        onChange={handleProjectChange}
                    />
                </Grid>

                {/* Balance Display Box - Updated */}
                {shouldCheckBalance && partyIdTo && currentProjectId && (
                    <Grid item xs={12} sx={{ mt: 2 }}>
                        <Box sx={{
                            p: 2,
                            border: "1px solid #e0e0e0",
                            borderRadius: 2,
                            bgcolor: "#f9f9f9"
                        }}>
                            {/* Your existing balance UI remains the same */}
                            {balanceLoading ? (
                                <Skeleton height={80}/>
                            ) : balanceData ? (
                                balanceData.initialBalance === 0 ? (
                                    <Alert severity="warning">
                                        {balanceData.message || "لا يوجد سقف دفع مُعيَّن لهذا المورد على المشروع"}
                                    </Alert>
                                ) : (
                                    <Grid container spacing={2}>
                                        {/* ... balance cards ... */}
                                        {amount > balanceData.remainingBalance && (
                                            <Grid item xs={12}>
                                                <Alert severity="error">
                                                    المبلغ المطلوب ({amount.toLocaleString("ar-EG")} ج.م) يتجاوز الرصيد المتاح
                                                </Alert>
                                            </Grid>
                                        )}
                                    </Grid>
                                )
                            ) : (
                                <Typography color="text.secondary">جاري تحميل بيانات الحساب...</Typography>
                            )}
                        </Box>
                    </Grid>
                )}

                {/* Submit Button */}
                <Button
                    type="submit"
                    variant="contained"
                    disabled={isSubmitDisabled}
                    sx={{mt: 2, mr: 1}}
                >
                    {getTranslatedLabel(`${localizationKey}.update`, "Update Payment")}
                </Button>

            </fieldset>
            {/* ... rest of your code (modals, pdf viewer, etc.) */}
        </FormElement>
    );
}}