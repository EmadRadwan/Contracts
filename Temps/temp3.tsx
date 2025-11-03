const handleMenuSelect = useCallback(
    (e: any) => {
        const data = e.item.data;
        const formValues = formRef.current?.values;
        const isValid = formRef.current?.isValid();

        console.debug("handleMenuSelect", {data, isValid, formValues});

        if (data === "update") {
            handleCreate({
                values: formValues,
                isValid,
                menuItem: "Update Payment",
            });
        } else if (data === "receive") {
            handleStatusChange({
                values: formValues,
                isValid,
                menuItem: "Status to Received",
            });
        } else if (data === "send") {
            handleStatusChange({
                values: formValues,
                isValid,
                menuItem: "Status to Sent",
            });
        } else if (data === "cancel") {
            handleStatusChange({
                values: formValues,
                isValid,
                menuItem: "Status to Cancelled",
            });
        } else if (data === "confirm") {
            handleStatusChange({
                values: formValues,
                isValid,
                menuItem: "Status to Confirmed",
            });
        } else if (data === "incoming") {
            handleNewPayment(1);
        } else if (data === "outgoing") {
            handleNewPayment(2);
        } else if (data === "transactions") {
            setShowTransactionsList(true);
        } else if (data === "applications") {
            setShowPaymentApplicationsList(true);
        }
    },
    [handleCreate, handleNewPayment]
);
