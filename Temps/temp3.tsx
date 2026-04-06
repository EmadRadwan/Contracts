const IncomeStatementForm = ({ onSubmit }: IncomeStatementFormProps) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.orgGL.reports.income-statement.form";

    const now = new Date();
    const firstDayOfYear = new Date(now.getFullYear(), 0, 1);

    return (
        <Form
            initialValues={{
                glFiscalTypeId: "ACTUAL",
                fromDate: firstDayOfYear,
                thruDate: now,
                selectedMonth: null,   // explicitly add this
            }}
            onSubmit={(values) => onSubmit(values)}   // ← Use onSubmit here
            render={(formRenderProps) => (
                <FormElement>
                    <fieldset className={"k-form-fieldset"}>
                        <Grid container spacing={2} alignItems={"flex-end"}>
                            <Grid container item xs={12} spacing={2}>
                                <Grid item xs={4}>
                                    <Field
                                        name={"selectedMonth"}
                                        id={"selectedMonth"}
                                        label={getTranslatedLabel(`${localizationKey}.month`, "Month")}
                                        component={MemoizedFormDropDownList2}
                                        data={months}
                                        textField="text"
                                        dataItemKey="month"
                                        onChange={(e) => {
                                            formRenderProps.onChange("fromDate", { value: null });
                                            formRenderProps.onChange("thruDate", { value: null });
                                            formRenderProps.onChange("selectedMonth", e);
                                        }}
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <Field
                                        name={"fromDate"}
                                        id={"fromDate"}
                                        label={getTranslatedLabel(`${localizationKey}.fromDate`, "From Date")}
                                        component={FormDatePicker}
                                        validator={(value) => {
                                            const thruDate = formRenderProps.valueGetter("thruDate");
                                            if (thruDate && value && new Date(value) > new Date(thruDate)) {
                                                return "From Date cannot be after Thru Date";
                                            }
                                            return "";
                                        }}
                                        onChange={(e) => {
                                            formRenderProps.onChange("selectedMonth", { value: null });
                                            formRenderProps.onChange("fromDate", e);
                                        }}
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <Field
                                        name={"thruDate"}
                                        id={"thruDate"}
                                        label={getTranslatedLabel(`${localizationKey}.thruDate`, "Thru Date")}
                                        component={FormDatePicker}
                                        validator={(value) => {
                                            const fromDate = formRenderProps.valueGetter("fromDate");
                                            if (fromDate && value && new Date(value) < new Date(fromDate)) {
                                                return "Thru Date cannot be before From Date";
                                            }
                                            return "";
                                        }}
                                        onChange={(e) => {
                                            formRenderProps.onChange("selectedMonth", { value: null });
                                            formRenderProps.onChange("thruDate", e);
                                        }}
                                    />
                                </Grid>
                            </Grid>
                        </Grid>

                        <Grid container item xs={12} spacing={2} mt={2}>
                            <Grid item xs={12}>
                                <Button
                                    variant="contained"
                                    type="submit"           // Important: keep type="submit"
                                    color="success"
                                    disabled={!formRenderProps.allowSubmit}   // Optional but recommended
                                >
                                    {getTranslatedLabel("general.generate", "Generate Report")}
                                </Button>
                            </Grid>
                        </Grid>
                    </fieldset>
                </FormElement>
            )}
        />
    );
};