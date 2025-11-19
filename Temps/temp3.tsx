<Form
    key={editMode}
    initialValues={formInitialValues}
    onSubmit={handleSubmitData}
    validator={salesRequestValidator}
    render={(formRenderProps: FormRenderProps) => {
        formRef.current = formRenderProps;

        // Extract status safely here
        const statusId = formRenderProps.valueGetter("statusId") as string | undefined;
        const statusDescription = formRenderProps.valueGetter("statusDescription") as string | undefined;

        const ribbonLabel = statusDescription ?? {
            SALES_REQUEST_CREATED: "Created",
            SALES_REQUEST_APPROVED: "Approved",
            SALES_REQUEST_REJECTED: "Rejected",
            SALES_REQUEST_CONVERTED: "Converted",
        }[statusId ?? ""] ?? "Unknown";

        const ribbonBg = {
            SALES_REQUEST_CREATED: "#1976d2",
            SALES_REQUEST_APPROVED: "#4caf50",
            SALES_REQUEST_REJECTED: "#d32f2f",
            SALES_REQUEST_CONVERTED: "#ff9800",
        }[statusId ?? ""] ?? "#757575";

        return (
            <>
                {/* Header with ribbon */}
                <Grid container spacing={2} alignItems="center" position="relative">
                    <Grid item xs={11}>
                        <Box display="flex" justifyContent="space-between" sx={{p: 2}}>
                            <Typography color={salesRequest?.salesRequestId ? "black" : "green"} variant="h4">
                                {salesRequest?.salesRequestId
                                    ? salesRequest.salesRequestId
                                    : getTranslatedLabel("salesRequest.form.new", "New Sales Request")}
                            </Typography>
                        </Box>
                    </Grid>

                    {editMode === 2 && (
                        <Grid item xs={1}>
                            <RibbonContainer>
                                <Ribbon
                                    side={language === "ar" ? "left" : "right"}
                                    type="corner"
                                    size="large"
                                    backgroundColor={ribbonBg}
                                    color="#ffffff"
                                    fontFamily="sans-serif"
                                >
                                    {ribbonLabel}
                                </Ribbon>
                            </RibbonContainer>
                        </Grid>
                    )}
                </Grid>

                <FormElement>
                    <fieldset className="k-form-fieldset">
                        {/* ← ALL YOUR EXISTING FIELDS HERE (unchanged) */}
                    </fieldset>
                </FormElement>
            </>
        );
    }}
/>