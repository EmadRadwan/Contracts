// ... (all imports remain unchanged)

export default function PurchaseOrderForm({ selectedOrder, cancelEdit, editMode }: Props) {
    const formRef = React.useRef<any>(null);
    const formRef2 = React.useRef<boolean>(false);
    const [selectedMenuItem, setSelectedMenuItem] = React.useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'order.po.form';
    const selectedMenuItemRef = useRef<string>(""); // To store selectedMenuItem for submission
    const { language } = useAppSelector(state => state.localization);
    // ... (other state and hooks remain unchanged)

    // REFACTOR: Modified handleMenuSelect to store selectedMenuItem in a ref and trigger form submission correctly
    // Purpose: Ensure selectedMenuItem is preserved and passed to handleSubmit, fixing the issue where formProps.selectedMenuItem was undefined
    // This avoids relying on formProps to carry the selectedMenuItem and ensures the "Approve Order" action is correctly triggered
    const handleMenuSelect = useCallback(
        (e: MenuSelectEvent) => {
            const menuItem = e.item.text;
            setSelectedMenuItem(menuItem); // Update state for UI consistency
            selectedMenuItemRef.current = menuItem; // Store in ref for submission
            if (menuItem === "New Order") {
                handleNewOrder();
            } else if (menuItem === "Receive Inventory") {
                dispatch(setSelectedApprovedPurchaseOrder({ orderId: selectedOrder ? selectedOrder.orderId! : order?.orderId }));
                navigate("/receiveInventory");
            } else if (menuItem === "Approve Order") {
                // Trigger form submission without passing selectedMenuItem as part of form values
                if (formRef.current) {
                    console.log('Approve Order selected. Form state:', {
                        isValid: formRef.current.isValid,
                        modified: formRef.current.modified,
                        allowSubmit: formRef.current.allowSubmit,
                        values: formRef.current.valueGetter(),
                        touched: formRef.current.touched
                    });
                    formRef.current.onSubmit(); // Trigger form submission
                }
            }
        },
        [dispatch, navigate, selectedOrder, order, handleNewOrder]
    );

    // REFACTOR: Updated handleSubmit to use selectedMenuItemRef instead of formProps.selectedMenuItem
    // Purpose: Fix the issue where formProps.selectedMenuItem was undefined by using a ref to store the selected menu item
    // This ensures the "Approve Order" action is correctly passed to handleCreate, sending the "APPROVE" flag to the backend
    const handleSubmit = useCallback(
        async (formProps: any) => {
            if (!formProps.isValid) {
                toast.error("Form is invalid");
                setIsLoading(false);
                return false;
            }
            if (isSubmitting) {
                // Prevent multiple submissions
                return false;
            }
            setIsSubmitting(true); // Lock submission
            const values = formProps.values;
            // Use selectedMenuItemRef.current instead of formProps.selectedMenuItem
            const actionType =
                selectedMenuItemRef.current === "Approve Order"
                    ? "Approve Order"
                    : formEditMode === 1
                        ? "Create Order"
                        : formEditMode === 2
                            ? "Update Order"
                            : "Approve Order";

            try {
                // Perform the primary action (Create, Update, or Approve)
                const result = await handleCreate({ values, selectedMenuItem: actionType });
                /*if (formEditMode === 1 && result?.orderId) {
                    await handleCreate({
                        values: { ...values, orderId: result.orderId },
                        selectedMenuItem: "Approve Order"
                    });
                }*/
            } catch (error) {
                toast.error("Operation failed");
                setIsSubmitting(false);
            } finally {
                // Clear selectedMenuItemRef after submission to prevent stale values
                selectedMenuItemRef.current = "";
            }
        },
        [handleCreate, formEditMode, isSubmitting]
    );

    // ... (rest of the code remains unchanged, including the Menu component and Form rendering)

    return (
        <>
            {isLoadingCombined && (
                <LoadingComponent
                    message='Processing Order...'
                    style={{ zIndex: 9999, position: 'fixed', top: 0, left: 0, width: '100%', height: '100%' }}
                />
            )}
            <OrderMenu selectedMenuItem={'/orders'} />

            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container spacing={2} alignItems={"center"} position={"relative"}>
                    <Grid item xs={10}>
                        <Box display='flex' justifyContent='space-between'>
                            <Typography
                                sx={{
                                    fontWeight: "bold",
                                    paddingLeft: 3,
                                    fontSize: '18px',
                                    color: formEditMode === 1 ? "green" : "black"
                                }}
                                variant="h6"
                            >
                                {" "}
                                {order && order?.orderId ? `Purchase Order No: ${order?.orderId}` : "New Purchase Order"}{" "}
                            </Typography>
                            {order?.certificateNumber && (
                                <Typography
                                    sx={{ paddingLeft: 3, fontSize: '16px', color: 'textSecondary' }}
                                    variant="subtitle1"
                                >
                                    Certificate Number: {order.certificateNumber}
                                </Typography>
                            )}
                        </Box>
                    </Grid>

                    <Grid item xs={1}>
                        <Menu onSelect={handleMenuSelect}>
                            <MenuItem text={getTranslatedLabel("general.actions", "Actions")}>
                                <MenuItem text="New Order" />
                                {formEditMode === 3 && <MenuItem text="Receive Inventory" />}
                                {formEditMode === 2 && <MenuItem text="Approve Order" />}
                            </MenuItem>
                        </Menu>
                    </Grid>
                    <Grid item xs={1}>
                        {formEditMode > 1 && (
                            <RibbonContainer>
                                <Ribbon
                                    side={language === "ar" ? "left" : "right"}
                                    type="corner"
                                    size="large"
                                    backgroundColor={status.backgroundColor}
                                    color={status.foreColor}
                                    fontFamily="sans-serif"
                                >
                                    {order?.statusDescription}
                                </Ribbon>
                            </RibbonContainer>
                        )}
                    </Grid>
                </Grid>

                <Form
                    ref={formRef}
                    initialValues={initialFormValues}
                    key={formKey}
                    onSubmitClick={values => handleSubmit(values)}
                    render={(formRenderProps) => (
                        <FormElement>
                            {/* ... (rest of the Form and other components remain unchanged) */}
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}