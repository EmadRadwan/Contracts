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
    CircularProgress,
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
import { requiredValidator, optionalEmailValidator } from '../../../app/common/form/Validators';
import { FormComboBoxVirtualPartyBroker } from '../../../app/common/form/FormComboBoxVirtualPartyBroker';
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

/**
 * An indirect lead reached us through an outside company, so the form has to
 * capture which one - the commission side downstream depends on it. The server
 * enforces the same rule; this only keeps the UI honest.
 */
const INDIRECT_DATA_SOURCE_ID = 'INDIRECT';
const requiresBroker = (dataSourceId?: string) => dataSourceId === INDIRECT_DATA_SOURCE_ID;

/** Collapse whitespace so a folded-together name never carries double spaces. */
const tidyName = (value: string) => value.trim().replace(/\s+/g, ' ');

/**
 * The lead's whole name as one string. Prefers the grid's computed fullName and
 * falls back to the stored parts, so this works whichever shape the record is in.
 */
const wholeNameOf = (lead: Lead) =>
    tidyName(
        lead.fullName ||
        `${lead.firstName ?? ''} ${lead.middleName ?? ''} ${lead.lastName ?? ''}`
    );

/** The existing lead a save collided with. */
interface DuplicateInfo {
    partyId: string;
    name: string;
    matchedField?: 'EMAIL' | 'MOBILE';
    matchedValue?: string;
}

/**
 * Create returns PartyDto2 (name in `description`) and update returns LeadDto
 * (name in `fullName`), so normalise both into one shape for the dialog.
 */
const toDuplicateInfo = (result: any): DuplicateInfo => ({
    partyId: result.partyId,
    name:
        result.fullName ||
        result.description ||
        `${result.firstName ?? ''} ${result.middleName ?? result.lastName ?? ''}`.trim() ||
        result.partyId,
    matchedField: result.duplicateMatchedField,
    matchedValue: result.duplicateMatchedValue,
});

const LeadForm: React.FC<LeadFormProps> = ({
    lead,
    editMode,
    onClose,
    onSuccess,
    isModal = false
}) => {
    const { getTranslatedLabel } = useTranslationHelper();
    // The existing lead that already holds the contact details we tried to save.
    // Both create and update answer with it, so both feed the same dialog.
    const [duplicate, setDuplicate] = useState<DuplicateInfo | null>(null);
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
            // One name field holding the whole name, as everywhere else in the app.
            // Records created through the Excel import arrive split across
            // FirstName/MiddleName, so fold the parts back together for display —
            // saving then writes the whole string back into one column.
            firstName: wholeNameOf(lead),
            geoId: lead.countryGeoId,
            mobileContactNumber: lead.mobilePhone,
            infoString: lead.email,
            // The broker combo works in {fromPartyId, fromPartyName} shape.
            brokerPartyId: lead.brokerPartyId
                ? { fromPartyId: lead.brokerPartyId, fromPartyName: lead.brokerName }
                : undefined,
        }
        : {
            firstName: '',
            infoString: '',
            mobileContactNumber: '',
            address1: '',
            address2: '',
            geoId: 'EGY',
            dataSourceId: '',
            leadTemperatureId: 'F',
        };

    const handleSubmit = async (values: any) => {
        setSubmitError(null);

        try {
            if (editMode === 2 && lead?.partyId) {
                // The name is stored whole. Send it as both the display name and the
                // first-name column, and clear the split parts — a record that came in
                // through the Excel import collapses back to the convention on its
                // next save. Without this, Party.Description kept the stale name the
                // grid handed us while Person.FirstName took the edit.
                const wholeName = tidyName(values.firstName ?? '');

                const updatePayload = {
                    ...values,
                    firstName: wholeName,
                    fullName: wholeName,
                    middleName: '',
                    lastName: '',
                    email: values.infoString,
                    mobilePhone: values.mobileContactNumber,
                    countryGeoId: values.geoId,
                    leadTemperatureId: values.leadTemperatureId,
                    // Only INDIRECT carries a broker; anything else clears it, which
                    // closes the existing link server-side.
                    brokerPartyId: requiresBroker(values.dataSourceId)
                        ? values.brokerPartyId?.fromPartyId ?? null
                        : null,
                };
                const updated: any = await updateLead({ id: lead.partyId, lead: updatePayload }).unwrap();

                // The server refused the edit because another lead already owns
                // this email/mobile - it hands back that lead rather than saving.
                if (updated?.isAlreadyCreated) {
                    setDuplicate(toDuplicateInfo(updated));
                    return; // stop - nothing was saved
                }

                onSuccess(); // No need to return lead on update
            } else {
                // CREATE NEW LEAD
                const result = await createLead({
                    ...values,
                    brokerPartyId: requiresBroker(values.dataSourceId)
                        ? values.brokerPartyId?.fromPartyId ?? null
                        : null,
                }).unwrap();

                if (result.isAlreadyCreated) {
                    setDuplicate(toDuplicateInfo(result));
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
            render={(formRenderProps) => {
                const showBroker = requiresBroker(formRenderProps.valueGetter('dataSourceId'));

                return (
                <FormElement>
                    <Grid container spacing={2}>
                        {/* Full Name */}
                        <Grid item xs={6}>
                            <Field
                                name="firstName"
                                label={getTranslatedLabel(`${localizationKey}.firstName`, 'First Name')}
                                component={FormInput}
                                validator={requiredValidator}
                                placeholder={getTranslatedLabel(`${localizationKey}.firstNamePlaceholder`, 'Enter Name')}
                            />
                        </Grid>

                        <Grid item xs={6}>
                            <Field
                                name="infoString"
                                label={getTranslatedLabel(`${localizationKey}.email`, 'Email Address')}
                                component={FormInput}
                                type="email"
                                validator={optionalEmailValidator}
                                placeholder="example@company.com"
                            />
                        </Grid>

                        {/* Deliberately not a second name field. The convention here is a
                            single name input stored whole — see CreateCustomerForm, and
                            Party.Description, which holds the full name in one column. */}

                        {/* Contact Information */}
                        <Grid item xs={12} sx={{ pb: 0 }}>
                            <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 0.5 }}>
                                {getTranslatedLabel(`${localizationKey}.contactInformation`, 'Contact Information')}
                            </Typography>
                            <Divider />
                        </Grid>

                        <Grid item xs={6}>
                            {isModal ? (
                                <Field
                                    name="geoId"
                                    label={getTranslatedLabel(`${localizationKey}.country`, 'Country Code')}
                                    component={MemoizedModalFormDropdown}
                                    data={countries || []}
                                    dataItemKey="geoId"
                                    textField="geoName"
                                />
                            ) : (
                                <Field
                                    name="geoId"
                                    label={getTranslatedLabel(`${localizationKey}.country`, 'Country')}
                                    component={MemoizedFormDropDownList}
                                    dataItemKey="geoId"
                                    textField="geoName"
                                    data={countries || []}
                                />
                            )}
                        </Grid>

                        <Grid item xs={6}>
                            <Field
                                name="mobileContactNumber"
                                label={getTranslatedLabel(`${localizationKey}.mobile`, 'Mobile')}
                                component={FormInput}
                                validator={requiredValidator}
                                placeholder="01XXXXXXXXX"
                            />
                        </Grid>

                        {/* Lead Source, Broker and Temperature share one row - the
                            temperature used to sit alone on a line of its own, leaving
                            two thirds of it empty. */}
                        <Grid item xs={showBroker ? 4 : 6}>
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

                        {/* Broker - only for indirect leads, and required when shown */}
                        {showBroker && (
                            <Grid item xs={4}>
                                <Field
                                    name="brokerPartyId"
                                    label={getTranslatedLabel(`${localizationKey}.broker`, 'Broker *')}
                                    component={FormComboBoxVirtualPartyBroker}
                                    validator={requiredValidator}
                                    popupSettings={
                                        isModal
                                            ? { appendTo: document.querySelector('.MuiModal-root') as HTMLElement }
                                            : undefined
                                    }
                                />
                            </Grid>
                        )}

                        {/* Lead Temperature */}
                        <Grid item xs={showBroker ? 4 : 6}>
                            <FormControl component="fieldset">
                                <FormLabel component="legend" sx={{ mb: 0, color: 'text.primary', fontWeight: 500 }}>
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
                        <Grid item xs={12} sx={{ pb: 0 }}>
                            <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 0.5 }}>
                                {getTranslatedLabel(`${localizationKey}.addressDetails`, 'Address Details')}
                            </Typography>
                            <Divider />
                        </Grid>

                        <Grid item xs={6}>
                            <Field
                                name="address1"
                                label={getTranslatedLabel(`${localizationKey}.address1`, 'Address Line 1')}
                                component={FormInput}
                            />
                        </Grid>

                        <Grid item xs={6}>
                            <Field
                                name="address2"
                                label={getTranslatedLabel(`${localizationKey}.address2`, 'Address Line 2 (Optional)')}
                                component={FormInput}
                            />
                        </Grid>

                        
                    </Grid>

                    {/* Error Alert */}
                    {submitError && (
                        <Alert severity="error" sx={{ mt: 2 }}>
                            {submitError}
                        </Alert>
                    )}

                    {/* Action Buttons */}
                    <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
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
                            {isProcessing ? (
                                <CircularProgress
                                    size={16}
                                    color="inherit"
                                    sx={{ ml: language === "ar" ? 0.5 : 0, mr: language === "ar" ? 0 : 0.5 }}
                                />
                            ) : (
                                <SaveIcon sx={{ ml: language === "ar" ? 0.5 : 0, mr: language === "ar" ? 0 : 0.5 }} />
                            )}
                            {isProcessing
                                ? getTranslatedLabel(`${localizationKey}.processing`, 'Saving...')
                                : editMode === 2
                                    ? getTranslatedLabel(`${localizationKey}.update`, 'Update Lead')
                                    : getTranslatedLabel(`${localizationKey}.create`, 'Create Lead')}
                        </Button>
                    </Box>
                </FormElement>
                );
            }}
        />
    );

    // Add this just before the final return statements, after formContent

    // Name the lead that already holds the details and the field that matched,
    // rather than just asserting "this is a duplicate" - the user's next move is
    // to go look at that lead, so tell them which one it is.
    const duplicateMatchSentence =
        duplicate?.matchedField === 'EMAIL'
            ? getTranslatedLabel(`${localizationKey}.duplicateHasEmail`, 'already has this email address:')
            : duplicate?.matchedField === 'MOBILE'
                ? getTranslatedLabel(`${localizationKey}.duplicateHasMobile`, 'already has this mobile number:')
                : getTranslatedLabel(`${localizationKey}.duplicateHasContact`, 'already has these contact details.');

    const duplicateAlertDialog = (
        <Dialog
            open={!!duplicate}
            onClose={() => setDuplicate(null)}
            maxWidth="xs"
            fullWidth
        >
            {/* Header */}
            <Box sx={{ p: 3, pb: 2, borderBottom: '1px solid', borderColor: 'divider' }}>
                <Typography variant="h6" fontWeight="bold" sx={{ textAlign: 'center' }}>
                    {getTranslatedLabel(`${localizationKey}.duplicateLeadTitle`, 'Existing Lead Found')}
                </Typography>
            </Box>

            {/* Body */}
            <Box sx={{ p: 3, textAlign: 'center' }} dir={language === 'ar' ? 'rtl' : 'ltr'}>
                <Typography variant="body1">
                    <Box component="span" fontWeight="bold">{duplicate?.name}</Box>
                    {duplicate?.partyId ? ` (${duplicate.partyId}) ` : ' '}
                    {duplicateMatchSentence}
                </Typography>

                {duplicate?.matchedValue && (
                    <Typography variant="body1" fontWeight="bold" dir="ltr" sx={{ mt: 1 }}>
                        {duplicate.matchedValue}
                    </Typography>
                )}

                <Typography variant="body2" color="text.secondary" sx={{ mt: 2, mb: 3 }}>
                    {editMode === 2
                        ? getTranslatedLabel(`${localizationKey}.duplicateUpdateHint`, 'Your changes were not saved. Use different contact details, or open the existing lead.')
                        : getTranslatedLabel(`${localizationKey}.duplicateCreateHint`, 'No new lead was created. Open the existing lead to continue working on it.')}
                </Typography>

                <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2 }}>
                    <Button
                        variant="outlined"
                        onClick={() => setDuplicate(null)}
                    >
                        {getTranslatedLabel(`${localizationKey}.dismiss`, 'Dismiss')}
                    </Button>
                    <Button
                        variant="contained"
                        onClick={() => {
                            // LeadsList reads this state, filters to the lead and
                            // opens its form.
                            navigate(`/leads`, { state: { duplicateLeadId: duplicate?.partyId } });
                            setDuplicate(null);
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
                        py: 1.5,
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

                <Box sx={{ px: 3, pt: 1, pb: 3 }}>
                    {formContent}
                </Box>
            </Paper>
            {duplicateAlertDialog}
        </>
    );
};

export default LeadForm;