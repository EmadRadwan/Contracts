import React, { useState } from 'react';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import FormTextArea from '../../../app/common/form/FormTextArea';
import FormInput from '../../../app/common/form/FormInput';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { FormComboBox } from '../../../app/common/form/FormComboBox';
import {
    useFetchCountriesQuery,
} from '../../../app/store/configureStore';
import agent from '../../../app/api/agent';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import {requiredValidator } from '../../../app/common/form/Validators';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';

interface Props {
    onClose: () => void;
    onUpdateCustomerDropDown?: (newCustomer: any) => void;
}

export default function CreateCustomerModalForm({ onClose, onUpdateCustomerDropDown }: Props) {
    const { data: geoCountry } = useFetchCountriesQuery(undefined);
    const [buttonFlag, setButtonFlag] = useState(false);

    const { getTranslatedLabel } = useTranslationHelper();

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        try {
            const response = await agent.Parties.createCustomer(data);
            if (onUpdateCustomerDropDown) onUpdateCustomerDropDown(response);
            onClose();
        } catch (error) {
            console.error('Customer creation failed:', error);
            // ← Optional: add toast/notification here in real app
        } finally {
            setButtonFlag(false);
        }
    }

    return (
        <Form
            onSubmit={(values) => handleSubmitData(values)}
            render={(formRenderProps) => (
                <FormElement>
                    <fieldset className="k-form-fieldset">
                        <Field
                            id="firstName"
                            name="firstName"
                            label={getTranslatedLabel('party.customers.form.firstName', 'First Name *')}
                            component={FormInput}
                            autoComplete="off"
                            validator={requiredValidator}
                        />

                        <Field
                            id="mobileContactNumber"
                            name="mobileContactNumber"
                            label={getTranslatedLabel('party.customers.form.mobileContactNumber', 'Mobile Contact Number')}
                            component={FormInput}
                            autoComplete="off"
                        />

                        <Field
                            id="infoString"
                            name="infoString"
                            label={getTranslatedLabel('party.customers.form.email', 'Email Address')}
                            component={FormInput}
                            autoComplete="off"
                        />

                        <Field
                            id="address1"
                            name="address1"
                            label={getTranslatedLabel('party.customers.form.address1', 'Address 1')}
                            component={FormInput}
                            autoComplete="off"
                        />

                        <Field
                            id="address2"
                            name="address2"
                            label={getTranslatedLabel('party.customers.form.address2', 'Address 2')}
                            rows={3}
                            component={FormTextArea}
                            autoComplete="off"
                        />

                        <Field
                            id="geoId"
                            name="geoId"
                            label={getTranslatedLabel('party.customers.form.countryCode', 'Country Code')}
                            component={FormComboBox}
                            dataItemKey="geoId"
                            textField="geoName"
                            autoComplete="off"
                            data={geoCountry ?? []}
                        />

                        <div className="k-form-buttons">
                            <Grid container spacing={2} justifyContent="flex-start">
                                <Grid item>
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        type="submit"
                                        disabled={!formRenderProps.allowSubmit || buttonFlag}
                                    >
                                        {getTranslatedLabel('party.customers.form.submit', 'Submit')}
                                    </Button>
                                </Grid>

                                <Grid item>
                                    <Button
                                        variant="contained"
                                        color="error"
                                        onClick={onClose}
                                    >
                                        {getTranslatedLabel('party.customers.form.cancel', 'Cancel')}
                                    </Button>
                                </Grid>
                            </Grid>
                        </div>

                        {buttonFlag && (
                            <LoadingComponent
                                message={getTranslatedLabel('party.customers.form.processing', 'Processing Customer...')}
                            />
                        )}
                    </fieldset>
                </FormElement>
            )}
        />
    );
}