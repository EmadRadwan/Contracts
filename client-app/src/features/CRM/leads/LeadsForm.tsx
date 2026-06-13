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
    Dialog,
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
    useAppSelector,
} from '../../../app/store/configureStore';
import { useNavigate } from "react-router";

import { Lead } from '../models/lead';
import FormInput from '../../../app/common/form/FormInput';
import { MemoizedFormDropDownList } from '../../../app/common/form/MemoizedFormDropDownList';
import { requiredValidator } from '../../../app/common/form/Validators';
import CRMMenu from '../menu/CRMMenu';
import { MemoizedModalFormDropdown } from '../../../app/common/form/ModalFormDropdown';

interface LeadFormProps {
    lead?: Lead;
    editMode: 1 | 2; // 1 = Create, 2 = Edit
    onClose: () => void;
    onSuccess: (createdLead?: any) => void;   // Updated to optionally return the new lead
    open?: boolean;                           // For modal usage
    isModal?: boolean;                        // NEW: Flag to render as modal content
}

const LeadForm: React.FC<LeadFormProps> = ({
    lead,
    editMode,
    onClose,
    onSuccess,
    isModal = false
}) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [duplicateLeadId, setDuplicateLeadId] = useState<string | null>(null);
    const localizationKey = 'crm.leads.form';
    const language = useAppSelector((state) => state.localization.language);

    const [createLead, { isLoading: creating }] = useCreateLeadMutation();
    const [updateLead, { isLoading: updating }] = useUpdateLeadMutation();

    const { data: dataSources, isLoading: loadingDataSources } = useFetchDataSourcesQuery();
    const { data: countries = [], isLoading: loadingCountries } = useFetchCountriesQuery({});

    const [submitError, setSubmitError] = useState<string | null>(null);

    const isProcessing = creating || updating;
    const isLoading = loadingDataSources || loadingCountries;

    const navigate = useNavigate()

    const initialValues: Partial<Lead> = editMode === 2 && lead
        ? {
            ...lead,
            geoId: lead.countryGeoId,
            mobileContactNumber: lead.mobilePhone,
            infoString: lead.email
        }
        : {
            firstName: '',
            middleName: '',
            infoString: '',
            mobileContactNumber: '',
            address1: '',
            address2: '',
            city: '',
            geoId: 'EGY',
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
                };
                await updateLead({ id: lead.partyId, lead: updatePayload }).unwrap();
                onSuccess(); // No need to return lead on update
            } else {
                // CREATE NEW LEAD
                const result = await createLead(values).unwrap();

                if (result.isAlreadyCreated) {
                    setDuplicateLeadId(result.partyId);
                    return; // stop — don't call onSuccess
                }

                // Prepare the lead object to be added to Opportunity's LeadPicker
                const newLeadForOpportunity: any = {
                    partyId: result.partyId || result.id,           // Adjust based on your API response
                    firstName: values.firstName,
                    middleName: values.middleName || '',
                    fullName: `${values.firstName} ${values.middleName || ''}`.trim(),
                    email: values.infoString,
                    mobilePhone: values.mobileContactNumber,
                    dataSourceId: values.dataSourceId,
                    // Add any other fields your SalesOpportunityLead model expects
                };

                onSuccess?.(newLeadForOpportunity);
            }

            // Only close if it's not a modal (modal will be closed by parent)
            if (!isModal) {
                onClose();
            }
        } catch (error: any) {
            console.error('Failed to save lead:', error);
            setSubmitError(
                error?.data?.title ||
                getTranslatedLabel(`${localizationKey}.saveError`, 'Failed to save lead')
            );
        }
    };

    const formContent = (
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
                                placeholder={getTranslatedLabel(`${localizationKey}.firstNamePlaceholder`, 'Enter First Name')}
                            />
                        </Grid>

                        <Grid item xs={6}>
                            <Field
                                name="middleName"
                                label={getTranslatedLabel(`${localizationKey}.lastName`, 'Last Name')}
                                component={FormInput}
                                validator={requiredValidator}
                                placeholder={getTranslatedLabel(`${localizationKey}.lastNamePlaceholder`, 'Enter Last Name')}
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

                        <Grid item xs={6}>
                            <Field
                                name="mobileContactNumber"
                                label={getTranslatedLabel(`${localizationKey}.mobile`, 'Mobile')}
                                component={FormInput}
                                placeholder="+20 987 654 321"
                            />
                        </Grid>

                        {/* Lead Source */}
                        <Grid item xs={6}>
                            {isModal ? (
                                <Field
                                    name="dataSourceId"
                                    label={getTranslatedLabel(`${localizationKey}.source`, 'Lead Source')}
                                    component={MemoizedModalFormDropdown}
                                    data={dataSources || []}
                                    dataItemKey="dataSourceId"
                                    textField="description"
                                    validator={requiredValidator}
                                />
                            ) : (
                                <Field
                                    name="dataSourceId"
                                    label={getTranslatedLabel(`${localizationKey}.source`, 'Lead Source')}
                                    component={MemoizedFormDropDownList}
                                    dataItemKey="dataSourceId"
                                    textField="description"
                                    data={dataSources || []}
                                    validator={requiredValidator}
                                />
                            )}
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

                        <Grid item container xs={12}>
                            <Grid item xs={6}>
                                <Field
                                    name="city"
                                    label={getTranslatedLabel(`${localizationKey}.city`, 'City')}
                                    component={FormInput}
                                    placeholder="Cairo"
                                />
                            </Grid>
                        </Grid>

                        <Grid item xs={6}>
                            {isModal ? (
                                <Field
                                    name="geoId"
                                    label={getTranslatedLabel(`${localizationKey}.country`, 'Country')}
                                    component={MemoizedModalFormDropdown}
                                    data={countries || []}
                                    dataItemKey="geoId"
                                    textField="geoName"
                                    validator={requiredValidator}
                                />
                            ) : (
                                <Field
                                    name="geoId"
                                    label={getTranslatedLabel(`${localizationKey}.country`, 'Country')}
                                    component={MemoizedFormDropDownList}
                                    dataItemKey="geoId"
                                    textField="geoName"
                                    data={countries || []}
                                    validator={requiredValidator}
                                />
                            )}
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
                            // startIcon={<SaveIcon />}
                            sx={{ px: 5, fontWeight: 600 }}
                        >
                            <SaveIcon sx={{ ml: language === "ar" ? 0.5 : 0, mr: language === "ar" ? 0 : 0.5 }} />
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
    );

    // Add this just before the final return statements, after formContent

const duplicateAlertDialog = (
    <Dialog
        open={!!duplicateLeadId}
        onClose={() => setDuplicateLeadId(null)}
        maxWidth="xs"
        fullWidth
    >
        {/* Header */}
        <Box sx={{ p: 3, pb: 2, borderBottom: '1px solid', borderColor: 'divider' }}>
            <Typography variant="h6" fontWeight="bold" sx={{ textAlign: 'center' }}>
                {getTranslatedLabel(`${localizationKey}.duplicateLeadTitle`, 'Lead Already Exists')}
            </Typography>
        </Box>

        {/* Body */}
        <Box sx={{ p: 3, textAlign: 'center' }}>
            <Typography variant="body1" sx={{ mb: 3 }}>
                {getTranslatedLabel(`${localizationKey}.duplicateLeadMessage`, 'This lead already exists.')}
            </Typography>

            <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2 }}>
                <Button
                    variant="outlined"
                    onClick={() => setDuplicateLeadId(null)}
                >
                    {getTranslatedLabel(`${localizationKey}.dismiss`, 'Dismiss')}
                </Button>
                <Button
                    variant="contained"
                    onClick={() => {
                        navigate(`/leads`, {state: {duplicateLeadId}}); // adjust to your route
                        setDuplicateLeadId(null);
                        onClose();
                    }}
                >
                    {getTranslatedLabel(`${localizationKey}.goToLeadDetails`, 'Go to Lead Details')}
                </Button>
            </Box>
        </Box>
    </Dialog>
);

    // ==================== MODAL MODE ====================
    if (isModal) {
        return (
            <Box sx={{ p: 3, pt: 1 }}>
                {formContent}
                {duplicateAlertDialog}
            </Box>
        );
    }

    // ==================== FULL PAGE MODE ====================
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
                    {formContent}
                </Box>
            </Paper>
             {duplicateAlertDialog}
        </>
    );
};

export default LeadForm;