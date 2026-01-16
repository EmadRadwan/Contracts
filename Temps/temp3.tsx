const NewPaymentIn: React.FC<NewPaymentInProps> = ({
                                                       // ... other props
                                                       paymentMethods = [],   // safe default
                                                       // ...
                                                   }) => {
    // Create the validator functions once (they capture paymentMethods)
    const bankTransferValidator = createBankTransferAllowedValidator(paymentMethods);
    const chequeValidator = createChequeFieldsValidator(paymentMethods);

    return (
        <Form
            // ...
            render={(formRenderProps: FormRenderProps) => (
                <FormElement>
                    {/* ... other fields ... */}

                    <Field
                        id="isBankTransfer"
                        name="isBankTransfer"
                        label={...}
                        component={MemoizedFormCheckBox}
                        validator={bankTransferValidator}
                    />

                    <Field
                        id="chequeNumber"
                        name="chequeNumber"
                        label={...}
                        component={FormInput}
                        validator={chequeValidator}
                    />

                    <Field
                        id="chequeDate"
                        name="chequeDate"
                        label={...}
                        component={FormDatePicker}
                        validator={chequeValidator}
                    />

                    {/* ... */}
                </FormElement>
            )}
        />
    );
};