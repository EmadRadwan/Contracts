import { useCallback } from "react";
import { FormComboBoxVirtualParty } from "../../../../app/common/form/FormComboBoxVirtualParty";
import { requiredValidator } from "../../../../app/common/form/Validators";
import {
    Field,
    Form,
    FormElement,
    FormRenderProps,
} from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { Button, Grid, Typography } from "@mui/material";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import { Error } from "@progress/kendo-react-labels";

interface NewPaymentInProps {
    formRef: React.MutableRefObject<any>;
    partyInputRef: React.RefObject<HTMLInputElement>;
    companies?: any[];
    filteredPaymentTypes: any[];
    paymentMethods?: any[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    setShowNewCustomer: (show: boolean) => void;
    onCreate: (data: { values: any; isValid: boolean; menuItem: string }) => void;
    handleCancelForm: () => void;
}

const NewPaymentIn: React.FC<NewPaymentInProps> = ({
                                                       formRef,
                                                       partyInputRef,
                                                       companies,
                                                       filteredPaymentTypes,
                                                       paymentMethods,
                                                       getTranslatedLabel,
                                                       paymentTypeValidator,
                                                       setShowNewCustomer,
                                                       onCreate,
                                                         handleCancelForm,
                                                   }) => {
    const localizationKey = "accounting.payments.form";

    
    // Handle form submission
    const handleSubmit = (values: any) => {
        onCreate({
            values,
            isValid: formRef.current?.isValid(),
            menuItem: "Create Payment",
        });
    };

    return (
        <Form
            ref={formRef}
            initialValues={{
                paymentId: "",
                paymentTypeId: "",
                paymentMethodId: "",
                statusId: "PMNT_NOT_PAID",
                partyIdFrom: "",
                partyIdFromName: "",
                amount: 0,
                paymentRefNum: "",
                currencyUomId: "EGP",
                organizationPartyId: "",
                isDepositWithDrawPayment: "Y",
                finAccountTransTypeId: "DEPOSIT",
                isDisbursement: false,
            }}
            onSubmit={handleSubmit}
            render={(formRenderProps: FormRenderProps) => {
                return (
                    <FormElement>
                        <fieldset className="k-form-fieldset">
                            <Grid container spacing={2}>
                                {/* Hidden Fields */}
                                <Field name="paymentId" component="input" type="hidden" />
                                <Field name="statusId" component="input" type="hidden" />
                                <Field name="currencyUomId" component="input" type="hidden" />
                                <Field
                                    name="isDepositWithDrawPayment"
                                    component="input"
                                    type="hidden"
                                />
                                <Field
                                    name="finAccountTransTypeId"
                                    component="input"
                                    type="hidden"
                                />
                                <Field name="isDisbursement" component="input" type="hidden" />

                                <Grid item xs={12}>
                                    <Grid container spacing={2} alignItems="flex-end">
                                        <Grid item xs={4}>
                                            <Field
                                                id="organizationPartyId"
                                                name="organizationPartyId"
                                                label={getTranslatedLabel(
                                                    `${localizationKey}.orgPartyId`,
                                                    "Organization Party Id *"
                                                )}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey="organizationPartyId"
                                                textField="organizationPartyName"
                                                data={companies || []}
                                                validator={requiredValidator}
                                            />
                                           
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id="partyIdFrom"
                                                name="partyIdFrom"
                                                label={getTranslatedLabel(
                                                    `${localizationKey}.from`,
                                                    "From Party Id *"
                                                )}
                                                component={FormComboBoxVirtualParty}
                                                autoComplete="off"
                                                validator={requiredValidator}
                                                inputRef={partyInputRef}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Button
                                                color="secondary"
                                                onClick={() => setShowNewCustomer(true)}
                                                variant="outlined"
                                            >
                                                {getTranslatedLabel(
                                                    `${localizationKey}.new-customer`,
                                                    "New Customer"
                                                )}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </Grid>

                                <Grid item xs={12}>
                                    <Grid container spacing={2} alignItems="flex-end">
                                        <Grid item xs={4}>
                                            <Field
                                                id="paymentTypeId"
                                                name="paymentTypeId"
                                                label={getTranslatedLabel(
                                                    `${localizationKey}.paymentType`,
                                                    "Payment Type *"
                                                )}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey="paymentTypeId"
                                                textField="description"
                                                data={filteredPaymentTypes}
                                                validator={paymentTypeValidator}
                                                disabled={filteredPaymentTypes.length === 0}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id="paymentMethodId"
                                                name="paymentMethodId"
                                                label={getTranslatedLabel(
                                                    `${localizationKey}.paymentMethod`,
                                                    "Payment Method *"
                                                )}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey="paymentMethodId"
                                                textField="description"
                                                data={paymentMethods || []}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                    </Grid>
                                </Grid>

                                <Grid item xs={12}>
                                    <Grid container spacing={2} alignItems="flex-end">
                                        <Grid item xs={4}>
                                            <Field
                                                id="amount"
                                                format="n2"
                                                min={0}
                                                name="amount"
                                                label={getTranslatedLabel(
                                                    `${localizationKey}.amount`,
                                                    "Amount *"
                                                )}
                                                component={FormNumericTextBox}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id="paymentRefNum"
                                                name="paymentRefNum"
                                                label={getTranslatedLabel(
                                                    `${localizationKey}.paymentRefNum`,
                                                    "Reference Number"
                                                )}
                                                component={FormTextArea}
                                                autoComplete="off"
                                            />
                                        </Grid>
                                    </Grid>
                                </Grid>

                                {formRenderProps.errors.form && (
                                    <Grid item xs={12}>
                                        <Error>{formRenderProps.errors.form}</Error>
                                    </Grid>
                                )}
                                <Grid container spacing={2}>
                                    <Grid item xs={2}>
                                        <Button
                                            type="submit"
                                            variant="contained"
                                            disabled={!formRenderProps.valid || !filteredPaymentTypes.length}
                                            sx={{ mt: 2 , ml: 2 }}
                                        >
                                            {getTranslatedLabel(`${localizationKey}.create`, "Create Payment")}
                                        </Button>
                                    </Grid>
                                    <Grid item xs={1}>
                                        <Button
                                            sx={{ mt: 2 }}
                                            onClick={handleCancelForm}
                                            color="error"
                                            variant="contained"
                                        >
                                            {getTranslatedLabel("general.cancel", "Cancel")}
                                        </Button>
                                    </Grid>
                                </Grid>
                                
                            </Grid>
                        </fieldset>
                    </FormElement>
                );
            }}
        />
    );
};

export default NewPaymentIn;