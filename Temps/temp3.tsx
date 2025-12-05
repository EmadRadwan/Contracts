const EditPaymentForm: React.FC<EditPaymentFormProps> = ({ ... }) => {
    const localizationKey = "accounting.payments.form";
    const ADVANCE_TO_VENDOR_CONTRACTOR = "ADVANCE_TO_VENDOR_CONTRACTOR";
    const CASH_PAYMENT_METHOD_ID = "CASH";

    // REFACTOR: Extract partyIdTo from payment (never changes in edit mode)
    const partyIdTo = payment?.partyIdTo ?? "";

    // REFACTOR: Balance query hook
    const [triggerBalanceFetch, { data: balanceData, isFetching: balanceLoading }] =
        useLazyFetchBalancesForVendorAndProjectQuery();

    // REFACTOR: These will be updated from form state when project changes
    const [currentProjectId, setCurrentProjectId] = useState<string>("");

    // REFACTOR: Determine if project field should be shown
    const showProjectField = payment?.paymentTypeId === ADVANCE_TO_VENDOR_CONTRACTOR;

    // Initialize currentProjectId from payment on mount
    useEffect(() => {
        if (payment?.projectId) {
            setCurrentProjectId(payment.projectId);
        }
    }, [payment?.projectId]);

    // REFACTOR: Trigger balance fetch whenever party + project changes
    useEffect(() => {
        if (showProjectField && partyIdTo && currentProjectId) {
            triggerBalanceFetch(
                { partyId: partyIdTo, projectId: currentProjectId },
                false // fresh data
            );
        }
    }, [showProjectField, partyIdTo, currentProjectId, triggerBalanceFetch]);

    // ... rest of hooks (glAccounts, cost centers, etc.)

    const initialValues = useMemo(() => {
        // ... same as before
        projectId: payment.projectId
            ? { projectId: payment.projectId, projectName: payment.projectName }
            : null,
        // ...
    }, [payment]);

    const amountValidator = (value: number, getter: any) => {
        if (!value || value <= 0) return "الرجاء إدخال مبلغ صحيح";

        const paymentTypeId = getter("paymentTypeId");
        if (paymentTypeId !== ADVANCE_TO_VENDOR_CONTRACTOR) return;

        if (!balanceData) return;

        if (balanceData.initialBalance === 0) {
            return "لا يمكن إنشاء دفعة مقدمة: لا يوجد سقف دفع مُعيَّن لهذا المورد على المشروع";
        }

        if (value > balanceData.remainingBalance) {
            return `المبلغ المُدخل (${value.toLocaleString("ar-EG")}) يتجاوز الرصيد المتاح (${balanceData.remainingBalance.toLocaleString("ar-EG")})`;
        }
    };

    return (
        <Form
            initialValues={initialValues}
            onSubmit={handleSubmit}
            key={payment.paymentId}
            render={(formRenderProps: FormRenderProps) => {
                const { valueGetter, onChange, valid } = formRenderProps;
                const amount = valueGetter("amount") || 0;

                const hasBillingAccountIssue =
                    showProjectField &&
                    balanceData &&
                    (balanceData.initialBalance === 0 || amount > balanceData.remainingBalance);

                const isSubmitDisabled = !valid || isFormDisabled || balanceLoading || hasBillingAccountIssue;

                // REFACTOR: Handle project selection change → update state + trigger balance fetch
                const handleProjectChange = (event: any) => {
                    const selectedProject = event.value;
                    const newProjectId = selectedProject?.projectId || "";

                    setCurrentProjectId(newProjectId);
                    onChange("projectId", { value: selectedProject });
                };

                return (
                    <FormElement>
                        {/* ... other fields ... */}

                        {showProjectField && (
                            <Grid item xs={3}>
                                <Field
                                    id="projectId"
                                    name="projectId"
                                    component={FormComboBoxVirtualProject}
                                    label={getTranslatedLabel("projects.certificate.form.project", "Project")}
                                    dataItemKey="projectId"
                                    textField="projectName"
                                    validator={requiredValidator}
                                    onChange={handleProjectChange} // This triggers balance refetch
                                />
                            </Grid>
                        )}

                        {/* Balance Box – now reacts to project changes */}
                        {showProjectField && partyIdTo && currentProjectId && (
                            <Grid item xs={12} sx={{ mt: 2 }}>
                                <Box sx={{ p: 2, border: "1px solid #e0e0e0", borderRadius: 2, bgcolor: "#f9f9f9" }}>
                                    {balanceLoading ? (
                                        <Skeleton height={80} />
                                    ) : balanceData ? (
                                        balanceData.initialBalance === 0 ? (
                                            <Alert severity="warning">
                                                {balanceData.message || "لا يوجد سقف دفع مُعيَّن لهذا المورد على المشروع"}
                                            </Alert>
                                        ) : (
                                            <Grid container spacing={2}>
                                                <Grid item xs={4}>
                                                    <Typography variant="body2" color="text.secondary">السقف المتاح</Typography>
                                                    <Typography variant="h6" color="success.main" fontWeight="bold">
                                                        {balanceData.initialBalance.toLocaleString("ar-EG")} ج.م
                                                    </Typography>
                                                </Grid>
                                                <Grid item xs={4}>
                                                    <Typography variant="body2" color="text.secondary">المستخدم</Typography>
                                                    <Typography variant="h6" color="warning.main">
                                                        {balanceData.usedBalance.toLocaleString("ar-EG")} ج.م
                                                    </Typography>
                                                </Grid>
                                                <Grid item xs={4}>
                                                    <Typography variant="body2" color="text.secondary">المتبقي</Typography>
                                                    <Typography variant="h6"
                                                                color={balanceData.remainingBalance > 0 ? "primary" : "error"}
                                                                fontWeight="bold">
                                                        {balanceData.remainingBalance.toLocaleString("ar-EG")} ج.م
                                                    </Typography>
                                                </Grid>
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

                        {/* Amount field with validator */}
                        <Grid item xs={2}>
                            <Field
                                id="amount"
                                name="amount"
                                label={getTranslatedLabel(`${localizationKey}.amount`, "Amount *")}
                                component={FormNumericTextBox}
                                format="n2"
                                min={0}
                                validator={(value) => requiredValidator(value) || amountValidator(value, valueGetter)}
                            />
                        </Grid>

                        {/* Submit button */}
                        <Button
                            type="submit"
                            variant="contained"
                            disabled={isSubmitDisabled}
                            sx={{ mt: 2, mr: 1 }}
                        >
                            Update Payment
                        </Button>
                    </FormElement>
                );
            }}
        />
    );
};