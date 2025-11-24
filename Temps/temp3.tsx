// Inside the Form's render function – right after formRef.current = formRenderProps;
render={(formRenderProps: FormRenderProps) => {
    formRef.current = formRenderProps;
    const { visited, errors, valueGetter } = formRenderProps;

    // REFACTOR: Extract all derived values BEFORE JSX
    const selectedApartmentObj = valueGetter("productId");
    const selectedApartmentStatusId = typeof selectedApartmentObj === "object"
        ? selectedApartmentObj?.apartmentStatusId
        : null;

    const isApartmentSelected = !!selectedApartmentObj;
    const isApartmentNotAvailable = selectedApartmentStatusId !== "APARTMENT_AVAILABLE";
    const isCreateMode = editMode === 1;
    const showApartmentNotAvailableWarning = isCreateMode && isApartmentSelected && isApartmentNotAvailable;

    const apt = selectedApartmentObj;
    const party = valueGetter("fromPartyId");

    // Keep your currentFormValues if needed elsewhere
    const currentFormValues: SalesRequest = { /* ... same as before */ };

    const apartmentForModal = currentFormValues.apartmentId
        ? { productName: currentFormValues.apartmentName ?? "Unknown Unit" }
        : undefined;

    const statusId = valueGetter("statusId") as string | undefined;
    const statusDescription = valueGetter("statusDescription") as string | undefined;
    const ribbonLabel = statusDescription ?? /* ... your mapping ... */;
    const ribbonBg = { /* ... */ };

    // REFACTOR: Now JSX is clean and uses simple booleans/variables
    return (
        <>
            <Grid container spacing={2} alignItems="center" position="relative">
                {/* ... header ... */}
            </Grid>

            <FormElement>
                <fieldset className="k-form-fieldset">
                    <Grid container spacing={1} alignItems="flex-end">
                        {/* Warning Banner – now based on clean variable */}
                        {showApartmentNotAvailableWarning && (
                            <Grid item xs={12}>
                                <Box sx={{
                                    p: 2,
                                    backgroundColor: "#ffebee",
                                    border: "1px solid #f44336",
                                    borderRadius: 1,
                                    mb: 2
                                }}>
                                    <Typography color="error" fontWeight="medium">
                                        {getTranslatedLabel(
                                            "salesRequest.form.validation.apartmentNotAvailable",
                                            "Cannot create sales request: This apartment is already SOLD or RESERVED."
                                        )}
                                    </Typography>
                                </Box>
                            </Grid>
                        )}

                        {/* All your existing fields – unchanged */}
                        <Grid item xs={4}>
                            <Field
                                id="productId"
                                name="productId"
                                label={getTranslatedLabel("projects.certificate.items.list.product", "Product *")}
                                component={FormSimpleComboBoxVirtualApartment}
                                autoComplete="off"
                                validator={requiredValidator}
                                onChange={(e) => handleProductChange(formRenderProps, e)}
                            />
                        </Grid>
                        {/* ... rest of fields ... */}
                    </Grid>

                    {/* Rest of your form – no more valueGetter() in JSX */}
                    {/* ... */}
                </fieldset>
            </FormElement>
        </>
    );
}}