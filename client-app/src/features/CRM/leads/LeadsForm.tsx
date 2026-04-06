import React, { useState } from 'react';
import {
    Grid,
    Paper,
    Box,
    Typography,
    IconButton,
    Button,
    Divider,
    Alert,
    Radio,
    RadioGroup,
    FormControlLabel,
    FormControl,
    FormLabel,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import SaveIcon from '@mui/icons-material/Save';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';

import {
    useCreateLeadMutation,
    useUpdateLeadMutation,
    useFetchDataSourcesQuery,
    useFetchCountriesQuery,
} from '../../../app/store/configureStore';

import { Lead } from '../models/lead';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import FormInput from '../../../app/common/form/FormInput';
import { MemoizedFormDropDownList } from '../../../app/common/form/MemoizedFormDropDownList';
import { requiredValidator } from '../../../app/common/form/Validators';
import CRMMenu from '../menu/CRMMenu';

interface LeadFormProps {
    lead?: Lead;
    editMode: 1 | 2; // 1 = Create, 2 = Edit
    onClose: () => void;
    onSuccess: () => void;
}

const LeadForm: React.FC<LeadFormProps> = ({ lead, editMode, onClose, onSuccess }: LeadFormProps) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.leads.form';

    const [createLead, { isLoading: creating }] = useCreateLeadMutation();
    const [updateLead, { isLoading: updating }] = useUpdateLeadMutation();

    const { data: dataSources = [], isLoading: loadingDataSources } = useFetchDataSourcesQuery();
    const { data: countries = [], isLoading: loadingCountries } = useFetchCountriesQuery({});

    const [submitError, setSubmitError] = useState<string | null>(null);

    const isProcessing = creating || updating;
    const isLoading = loadingDataSources || loadingCountries;

    const initialValues: Partial<Lead> = editMode === 2 && lead
        ? { ...lead, geoId: lead.countryGeoId, mobileContactNumber: lead.mobilePhone, infoString: lead.email }
        : {
            firstName: '',
            middleName: '',
            infoString: '',
            mobileContactNumber: '',
            address1: '',
            address2: '',
            city: '',
            geoId: '',
            dataSourceId: '',
            leadTemperatureId: 'F',
        };

    const handleSubmit = async (values: any) => {
        setSubmitError(null);

        try {
            if (editMode === 2 && lead?.partyId) {
                const updatePayload = {
                    ...values,
                    email: values.infoString,
                    mobilePhone: values.mobileContactNumber,
                    countryGeoId: values.geoId,
                    lastName: values.middleName,
                    leadTemperatureId: values.leadTemperatureId
                }
                await updateLead({ id: lead.partyId, lead: updatePayload }).unwrap();
            } else {
                await createLead(values).unwrap();
            }

            onSuccess();
            onClose();
        } catch (error: any) {
            console.error('Failed to save lead:', error);
            setSubmitError(
                error?.data?.title ||
                getTranslatedLabel(`${localizationKey}.saveError`, 'Failed to save lead')
            );
        }
    };

    if (isLoading) {
        return <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`, 'Loading form...')} />;
    }

    return (
        <>
        <CRMMenu />
            <Paper
                elevation={6}
                sx={{
                    borderRadius: 3,
                    overflow: 'hidden',
                    backgroundColor: 'background.paper',
                }}
            >
                {/* Modern Header */}
                <Box
                    sx={{
                        px: 3,
                        py: 2.5,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                    }}
                >
                    <Typography variant="h6" fontWeight={600}>
                        {editMode === 2
                            ? getTranslatedLabel(`${localizationKey}.editTitle`, 'Edit Lead')
                            : getTranslatedLabel(`${localizationKey}.createTitle`, 'Create New Lead')}
                    </Typography>
    
                    <IconButton onClick={onClose} sx={{ color: 'inherit' }} size="small">
                        <CloseIcon />
                    </IconButton>
                </Box>
    
                <Box sx={{ p: 4 }}>
                    <Form
                        initialValues={initialValues}
                        onSubmit={handleSubmit}
                        render={(formRenderProps) => (
                            <FormElement>
                                <Grid container spacing={3.5}>
                                    {/* Full Name */}
                                    <Grid item xs={6}>
                                        <Field
                                            name="firstName"
                                            label={getTranslatedLabel(`${localizationKey}.firstName`, 'First Name')}
                                            component={FormInput}
                                            validator={requiredValidator}
                                            placeholder="Enter first name"
                                        />
                                    </Grid>

                                    <Grid item xs={6}>
                                        <Field
                                            name="middleName"
                                            label={getTranslatedLabel(`${localizationKey}.middleName`, 'Last Name')}
                                            component={FormInput}
                                            validator={requiredValidator}
                                            placeholder="Enter last name"
                                        />
                                    </Grid>
    
                                    {/* Contact Information */}
                                    <Grid item xs={12}>
                                        <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                                            {getTranslatedLabel(`${localizationKey}.contactInformation`, 'Contact Information')}
                                        </Typography>
                                        <Divider sx={{ mb: 2 }} />
                                    </Grid>
    
                                    <Grid item xs={6}>
                                        <Field
                                            name="infoString"
                                            label={getTranslatedLabel(`${localizationKey}.email`, 'Email Address')}
                                            component={FormInput}
                                            type="email"
                                            placeholder="example@company.com"
                                        />
                                    </Grid>
    
                                    <Grid item xs={6} >
                                        <Field
                                            name="mobileContactNumber"
                                            label={getTranslatedLabel(`${localizationKey}.mobile`, 'Mobile')}
                                            component={FormInput}
                                            placeholder="+20 987 654 321"
                                        />
                                    </Grid>

                                     {/* Lead Source */}
                                    <Grid item xs={6}>
                                        <Field
                                            name="dataSourceId"
                                            label={getTranslatedLabel(`${localizationKey}.source`, 'Lead Source')}
                                            component={MemoizedFormDropDownList}
                                            dataItemKey="dataSourceId"
                                            textField="description"
                                            validator={requiredValidator}
                                            data={dataSources || []}
                                            placeholder="How did you find this lead?"
                                        />
                                    </Grid>

                                    {/* Lead Temperature */}
                                    <Grid item xs={4}>
                                        <FormControl component="fieldset">
                                            <FormLabel component="legend" sx={{ mb: 1, color: 'text.primary', fontWeight: 500 }}>
                                                {getTranslatedLabel(`${localizationKey}.leadTemperature`, 'Lead Temperature')}
                                            </FormLabel>
                                            <RadioGroup
                                                row
                                                name="leadTemperatureId"
                                                defaultValue={initialValues.leadTemperatureId || 'F'}
                                                onChange={(e) => {
                                                    // Kendo Form doesn't auto-update radio, so we use formRenderProps.onChange
                                                    formRenderProps.onChange('leadTemperatureId', {
                                                        value: e.target.value
                                                    });
                                                }}
                                            >
                                                <FormControlLabel
                                                    value="C"
                                                    control={<Radio />}
                                                    label={getTranslatedLabel(`${localizationKey}.cold`, 'Cold')}
                                                />
                                                <FormControlLabel
                                                    value="F"
                                                    control={<Radio />}
                                                    label={getTranslatedLabel(`${localizationKey}.fresh`, 'Fresh')}
                                                />
                                            </RadioGroup>
                                        </FormControl>
                                    </Grid>
    
                                    {/* Address Section */}
                                    <Grid item xs={12}>
                                        <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                                            {getTranslatedLabel(`${localizationKey}.addressDetails`, 'Address Details')}
                                        </Typography>
                                        <Divider sx={{ mb: 2 }} />
                                    </Grid>
    
                                    <Grid item xs={12}>
                                        <Field
                                            name="address1"
                                            label={getTranslatedLabel(`${localizationKey}.address1`, 'Address Line 1')}
                                            component={FormInput}
                                            placeholder="Street name and building number"
                                        />
                                    </Grid>
    
                                    <Grid item xs={12}>
                                        <Field
                                            name="address2"
                                            label={getTranslatedLabel(`${localizationKey}.address2`, 'Address Line 2 (Optional)')}
                                            component={FormInput}
                                            placeholder="Apartment, floor, etc."
                                        />
                                    </Grid>
    
                                    <Grid item xs={6}>
                                        <Field
                                            name="city"
                                            label={getTranslatedLabel(`${localizationKey}.city`, 'City')}
                                            component={FormInput}
                                            placeholder="Cairo"
                                        />
                                    </Grid>
    
                                    <Grid item xs={6}>
                                        <Field
                                            name="geoId"
                                            label={getTranslatedLabel(`${localizationKey}.country`, 'Country')}
                                            component={MemoizedFormDropDownList}
                                            dataItemKey="geoId"
                                            textField="geoName"
                                            data={countries || []}
                                            placeholder="Select country"
                                        />
                                    </Grid>
    
                                   
                                </Grid>
    
                                {/* Error Alert */}
                                {submitError && (
                                    <Alert severity="error" sx={{ mt: 3 }}>
                                        {submitError}
                                    </Alert>
                                )}
    
                                {/* Action Buttons */}
                                <Box sx={{ mt: 5, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                                    <Button
                                        variant="outlined"
                                        onClick={onClose}
                                        disabled={isProcessing}
                                        sx={{ px: 4 }}
                                    >
                                        {getTranslatedLabel(`${localizationKey}.cancel`, 'Cancel')}
                                    </Button>
    
                                    <Button
                                        variant="contained"
                                        color="primary"
                                        type="submit"
                                        disabled={!formRenderProps.allowSubmit || isProcessing}
                                        startIcon={<SaveIcon />}
                                        sx={{ px: 5, fontWeight: 600 }}
                                    >
                                        {isProcessing
                                            ? getTranslatedLabel(`${localizationKey}.processing`, 'Saving...')
                                            : editMode === 2
                                            ? getTranslatedLabel(`${localizationKey}.update`, 'Update Lead')
                                            : getTranslatedLabel(`${localizationKey}.create`, 'Create Lead')}
                                    </Button>
                                </Box>
                            </FormElement>
                        )}
                    />
                </Box>
            </Paper>
        </>
    );
};

export default LeadForm;