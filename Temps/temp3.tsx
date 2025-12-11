render={(formRenderProps: FormRenderProps) => {
    const { valueGetter, valid, onSubmit, values } = formRenderProps;

    // REFACTOR: Build fresh Excel data right here, using latest form values
    // Purpose: Eliminate stale payment prop problem completely
    // This runs on every render → always up-to-date → Excel always correct
    const excelPaymentData = {
        paymentId: payment.paymentId ?? "NEW",
        paymentType: paymentTypeDesc,
        fromParty: paymentType === 1 ? payment.partyIdFromName ?? "" : payment.partyIdToName ?? "",
        toParty: paymentType === 1 ? payment.partyIdToName ?? "" : payment.partyIdFromName ?? "",
        amount: values.amount ?? payment.amount ?? 0,
        currency: payment.currencyUomId ?? "",
        effectiveDate: payment.effectiveDate ?? "",
        status: statusDesc,
        paymentMethod:
            paymentMethods?.find(m => m.paymentMethodId === values.paymentMethodId)
                ?.description ?? payment.paymentMethodId ?? "",
        chequeNumber: values.paymentMethodId === CASH_PAYMENT_METHOD_ID ? "" : (values.chequeNumber ?? ""),
        chequeDate: values.paymentMethodId === CASH_PAYMENT_METHOD_ID ? undefined : values.chequeDate,
        costCenter: (() => {
            const id = values.costCenterId || payment.costCenterId;
            if (!id) return "غير محدد";
            return paymentCostCenters.find(cc => cc.costCenterId === id)?.description ?? id;
        })(),
        project: (() => {
            const proj = values.projectId;
            if (proj) return proj.projectName ?? proj.projectId ?? "غير محدد";
            return payment.projectName ?? payment.projectId ?? "غير محدد";
        })(),
    };

    // ... rest of your form UI

    return (
        <FormElement>
            {/* Your entire form grid here */}
            {/* ... */}

            <div className="k-form-buttons">
                <Grid container spacing={2}>
                    <Grid item xs={2}>
                        <Button
                            type="submit"
                            variant="contained"
                            disabled={!valid || isFormDisabled || balanceLoading || hasBillingAccountIssue}
                            onClick={onSubmit}
                            sx={{ mt: 2, mr: 1 }}
                        >
                            Update Payment
                        </Button>
                    </Grid>

                    {/* Excel Buttons — now use fresh every time */}
                    <Grid item xs={2}>
                        <PaymentExcelTechnical
                            companyName={companyName ?? "N/A"}
                            payment={excelPaymentData}
                            applications={excelApplications}
                            transactions={excelTransactions}
                            getTranslatedLabel={getTranslatedLabel}
                            isFetching={isExcelFetching}
                        />
                    </Grid>

                    <Grid item xs={2}>
                        <PaymentExcelParty
                            companyName={companyName ?? "N/A"}
                            payment={excelPaymentData}   {/* Always fresh! */}
                            getTranslatedLabel={getTranslatedLabel}
                            isFetching={isExcelFetching}
                        />
                    </Grid>

                    <Grid item xs={1}>
                        <Button
                            sx={{ mt: 2 }}
                            onClick={handleCancelForm}
                            color="error"
                            variant="contained"
                        >
                            Cancel
                        </Button>
                    </Grid>
                </Grid>
            </div>
        </FormElement>
    );
}}