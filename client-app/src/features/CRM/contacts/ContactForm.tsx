import React, { useState } from 'react';
import { Grid, Paper, Box, Typography, IconButton } from '@mui/material';
import Button from '@mui/material/Button';
import CloseIcon from '@mui/icons-material/Close';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import {
    useCreateContactMutation,
    useUpdateContactMutation,
    useFetchDataSourcesQuery,
    useFetchCountriesQuery
} from '../../../app/store/configureStore';
import { Contact } from '../models/contact';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import FormInput from '../../../app/common/form/FormInput';
import { MemoizedFormDropDownList } from '../../../app/common/form/MemoizedFormDropDownList';
import { requiredValidator } from '../../../app/common/form/Validators';

interface ContactFormProps {
    contact?: Contact;
    editMode: 1 | 2;
    onClose: () => void;
    onSuccess: () => void;
}

const ContactForm: React.FC<ContactFormProps> = ({ contact, editMode, onClose, onSuccess }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.contacts.form';

    const [createContact, { isLoading: creating }] = useCreateContactMutation();
    const [updateContact, { isLoading: updating }] = useUpdateContactMutation();

    // Fetch dropdown data
    const { data: dataSources, isLoading: loadingDataSources } = useFetchDataSourcesQuery();
    const { data: countries, isLoading: loadingCountries } = useFetchCountriesQuery({});

    const [submitError, setSubmitError] = useState<string | null>(null);

    const isProcessing = creating || updating;
    const isLoading = loadingDataSources || loadingCountries;

    const initialValues: Partial<Contact> = editMode === 'edit' && contact
        ? { ...contact }
        : {
            firstName: '',
            lastName: '',
            personalTitle: '',
            email: '',
            phone: '',
            mobilePhone: '',
            address1: '',
            address2: '',
            city: '',
            postalCode: '',
            countryGeoId: '',
            dataSourceId: ''
        };

    const handleSubmit = async (values: any) => {
        setSubmitError(null);

        const contactData: Contact = {
            ...values
        };

        try {
            if (editMode === 2 && contact?.partyId) {
                await updateContact({
                    id: contact.partyId,
                    contact: contactData
                }).unwrap();
            } else {
                await createContact(contactData).unwrap();
            }
            onSuccess();
            onClose();
        } catch (error: any) {
            console.error('Failed to save contact:', error);
            setSubmitError(error?.data?.title || getTranslatedLabel(`${localizationKey}.saveError`, 'Failed to save contact'));
        }
    };

    if (isLoading) {
        return <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`, 'Loading...')} />;
    }

    return (
        <Paper elevation={5} className="div-container-withBorderCurved" sx={{ p: 3, mt: 2 }}>
            {/* Header */}
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h6">
                    {editMode === 2
                        ? getTranslatedLabel(`${localizationKey}.editTitle`, 'Edit Contact')
                        : getTranslatedLabel(`${localizationKey}.createTitle`, 'Create New Contact')
                    }
                </Typography>
                <IconButton onClick={onClose} size="small">
                    <CloseIcon />
                </IconButton>
            </Box>

            <Form
                initialValues={initialValues}
                onSubmit={handleSubmit}
                render={(formRenderProps) => (
                    <FormElement>
                        <fieldset className="k-form-fieldset">
                            <Grid container spacing={3}>
                                {/* Row 1: Title, First Name, Last Name */}
                                <Grid item xs={12} md={2}>
                                    <Field
                                        id="personalTitle"
                                        name="personalTitle"
                                        label={getTranslatedLabel(`${localizationKey}.title`, 'Title')}
                                        component={FormInput}
                                        placeholder={getTranslatedLabel(`${localizationKey}.titlePlaceholder`, 'Mr/Mrs/Dr')}
                                    />
                                </Grid>
                                <Grid item xs={12} md={5}>
                                    <Field
                                        id="firstName"
                                        name="firstName"
                                        label={getTranslatedLabel(`${localizationKey}.firstName`, 'First Name *')}
                                        component={FormInput}
                                        validator={requiredValidator}
                                    />
                                </Grid>
                                <Grid item xs={12} md={5}>
                                    <Field
                                        id="lastName"
                                        name="lastName"
                                        label={getTranslatedLabel(`${localizationKey}.lastName`, 'Last Name *')}
                                        component={FormInput}
                                        validator={requiredValidator}
                                    />
                                </Grid>

                                {/* Row 2: Email and Phones */}
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="email"
                                        name="email"
                                        label={getTranslatedLabel(`${localizationKey}.email`, 'Email')}
                                        component={FormInput}
                                        type="email"
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="phone"
                                        name="phone"
                                        label={getTranslatedLabel(`${localizationKey}.phone`, 'Phone')}
                                        component={FormInput}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="mobilePhone"
                                        name="mobilePhone"
                                        label={getTranslatedLabel(`${localizationKey}.mobile`, 'Mobile')}
                                        component={FormInput}
                                    />
                                </Grid>

                                {/* Row 3: Address */}
                                <Grid item xs={12} md={6}>
                                    <Field
                                        id="address1"
                                        name="address1"
                                        label={getTranslatedLabel(`${localizationKey}.address1`, 'Address Line 1')}
                                        component={FormInput}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <Field
                                        id="address2"
                                        name="address2"
                                        label={getTranslatedLabel(`${localizationKey}.address2`, 'Address Line 2')}
                                        component={FormInput}
                                    />
                                </Grid>

                                {/* Row 4: City, Postal Code, Country */}
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="city"
                                        name="city"
                                        label={getTranslatedLabel(`${localizationKey}.city`, 'City')}
                                        component={FormInput}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="postalCode"
                                        name="postalCode"
                                        label={getTranslatedLabel(`${localizationKey}.postalCode`, 'Postal Code')}
                                        component={FormInput}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="countryGeoId"
                                        name="countryGeoId"
                                        label={getTranslatedLabel(`${localizationKey}.country`, 'Country')}
                                        component={MemoizedFormDropDownList}
                                        dataItemKey="geoId"
                                        textField="geoName"
                                        data={countries || []}
                                    />
                                </Grid>

                                {/* Row 5: Data Source */}
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="dataSourceId"
                                        name="dataSourceId"
                                        label={getTranslatedLabel(`${localizationKey}.source`, 'Lead Source')}
                                        component={MemoizedFormDropDownList}
                                        dataItemKey="dataSourceId"
                                        textField="description"
                                        data={dataSources || []}
                                    />
                                </Grid>
                            </Grid>

                            {/* Error Message */}
                            {submitError && (
                                <Box sx={{ mt: 2, p: 2, bgcolor: 'error.light', borderRadius: 1 }}>
                                    <Typography color="error.dark">{submitError}</Typography>
                                </Box>
                            )}

                            {/* Buttons */}
                            <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
                                <Button
                                    variant="contained"
                                    color="primary"
                                    type="submit"
                                    disabled={!formRenderProps.allowSubmit || isProcessing}
                                >
                                    {editMode === 2
                                        ? getTranslatedLabel(`${localizationKey}.update`, 'Update')
                                        : getTranslatedLabel(`${localizationKey}.create`, 'Create')
                                    }
                                </Button>
                                <Button
                                    variant="outlined"
                                    onClick={onClose}
                                    disabled={isProcessing}
                                >
                                    {getTranslatedLabel(`${localizationKey}.cancel`, 'Cancel')}
                                </Button>
                            </Box>

                            {isProcessing && (
                                <LoadingComponent message={getTranslatedLabel(`${localizationKey}.processing`, 'Processing...')} />
                            )}
                        </fieldset>
                    </FormElement>
                )}
            />
        </Paper>
    );
};

export default ContactForm;
