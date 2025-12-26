import React, { useState, useEffect } from 'react';
import { Grid, Paper, Box, Typography, IconButton, Divider } from '@mui/material';
import Button from '@mui/material/Button';
import CloseIcon from '@mui/icons-material/Close';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import {
    useFetchOpportunityStagesQuery,
    useCreateOpportunityMutation,
    useUpdateOpportunityMutation,
    useFetchDataSourcesQuery,
    useAppDispatch,
    useAppSelector
} from '../../../app/store/configureStore';
import { currenciesSelectors, fetchCurrenciesAsync } from '../../catalog/slice/currencySlice';
import { SalesOpportunity, SalesOpportunityContact } from '../models/salesOpportunity';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import FormInput from '../../../app/common/form/FormInput';
import FormNumericTextBox from '../../../app/common/form/FormNumericTextBox';
import FormDatePicker from '../../../app/common/form/FormDatePicker';
import { MemoizedFormDropDownList } from '../../../app/common/form/MemoizedFormDropDownList';
import { requiredValidator } from '../../../app/common/form/Validators';
import ContactPicker from '../components/ContactPicker';

interface LeadFormProps {
    opportunity?: SalesOpportunity;
    editMode: 'create' | 'edit';
    onClose: () => void;
    onSuccess: () => void;
}

const LeadForm: React.FC<LeadFormProps> = ({ opportunity, editMode, onClose, onSuccess }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.leads.form';
    const dispatch = useAppDispatch();

    const { data: stages, isLoading: loadingStages } = useFetchOpportunityStagesQuery();
    const [createOpportunity, { isLoading: creating }] = useCreateOpportunityMutation();
    const [updateOpportunity, { isLoading: updating }] = useUpdateOpportunityMutation();

    // Currency dropdown
    const { currenciesLoaded } = useAppSelector(state => state.currency);
    const currencies = useAppSelector(currenciesSelectors.selectAll);

    // Data sources dropdown
    const { data: dataSources, isLoading: loadingDataSources } = useFetchDataSourcesQuery();

    useEffect(() => {
        if (!currenciesLoaded) dispatch(fetchCurrenciesAsync());
    }, [currenciesLoaded, dispatch]);

    const [submitError, setSubmitError] = useState<string | null>(null);
    const [selectedContacts, setSelectedContacts] = useState<SalesOpportunityContact[]>(
        opportunity?.contacts || []
    );
    const [contactsModified, setContactsModified] = useState(false);

    const handleContactsChange = (contacts: SalesOpportunityContact[]) => {
        setSelectedContacts(contacts);
        setContactsModified(true);
    };

    const isProcessing = creating || updating;

    // Helper to safely convert date string to Date object for Kendo DatePicker
    const parseDate = (dateValue: string | Date | undefined): Date | null => {
        if (!dateValue) return null;
        try {
            const date = typeof dateValue === 'string' ? new Date(dateValue) : dateValue;
            if (isNaN(date.getTime())) return null;
            return date;
        } catch {
            return null;
        }
    };

    const initialValues = editMode === 'edit' && opportunity
        ? {
            ...opportunity,
            estimatedCloseDate: parseDate(opportunity.estimatedCloseDate),
            nextStepDate: parseDate(opportunity.nextStepDate)
        }
        : {
            opportunityName: '',
            estimatedAmount: 0,
            estimatedProbability: 0,
            currencyUomId: 'USD',
            opportunityStageId: stages?.[0]?.opportunityStageId || '',
            contacts: []
        };

    const handleSubmit = async (values: any) => {
        setSubmitError(null);

        const opportunityData: SalesOpportunity = {
            ...values,
            contacts: selectedContacts
        };

        try {
            if (editMode === 'edit' && opportunity?.salesOpportunityId) {
                await updateOpportunity({
                    id: opportunity.salesOpportunityId,
                    opportunity: opportunityData
                }).unwrap();
            } else {
                await createOpportunity(opportunityData).unwrap();
            }
            onSuccess();
            onClose();
        } catch (error: any) {
            console.error('Failed to save opportunity:', error);
            setSubmitError(error?.data?.title || 'Failed to save opportunity');
        }
    };

    if (loadingStages || !currenciesLoaded || loadingDataSources) {
        return <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`, 'Loading...')} />;
    }

    return (
        <Paper elevation={5} className="div-container-withBorderCurved" sx={{ p: 3, mt: 2 }}>
            {/* Header */}
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h6">
                    {editMode === 'edit'
                        ? getTranslatedLabel(`${localizationKey}.editTitle`, 'Edit Opportunity')
                        : getTranslatedLabel(`${localizationKey}.createTitle`, 'Create New Opportunity')
                    }
                </Typography>
                <IconButton onClick={onClose} size="small">
                    <CloseIcon />
                </IconButton>
            </Box>

            <Form
                key={opportunity?.salesOpportunityId || 'new'}
                initialValues={initialValues}
                onSubmit={handleSubmit}
                render={(formRenderProps) => (
                    <FormElement>
                        <fieldset className="k-form-fieldset">
                            <Grid container spacing={3}>
                                {/* Row 1: Name and Stage */}
                                <Grid item xs={12} md={6}>
                                    <Field
                                        id="opportunityName"
                                        name="opportunityName"
                                        label={getTranslatedLabel(`${localizationKey}.name`, 'Opportunity Name *')}
                                        component={FormInput}
                                        validator={requiredValidator}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <Field
                                        id="opportunityStageId"
                                        name="opportunityStageId"
                                        label={getTranslatedLabel(`${localizationKey}.stage`, 'Stage *')}
                                        component={MemoizedFormDropDownList}
                                        dataItemKey="opportunityStageId"
                                        textField="description"
                                        data={stages || []}
                                        validator={requiredValidator}
                                    />
                                </Grid>

                                {/* Row 2: Amount and Probability */}
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="estimatedAmount"
                                        name="estimatedAmount"
                                        label={getTranslatedLabel(`${localizationKey}.amount`, 'Estimated Amount')}
                                        component={FormNumericTextBox}
                                        format="c0"
                                        min={0}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="currencyUomId"
                                        name="currencyUomId"
                                        label={getTranslatedLabel(`${localizationKey}.currency`, 'Currency')}
                                        component={MemoizedFormDropDownList}
                                        dataItemKey="currencyUomId"
                                        textField="description"
                                        data={currencies}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="estimatedProbability"
                                        name="estimatedProbability"
                                        label={getTranslatedLabel(`${localizationKey}.probability`, 'Probability (%)')}
                                        component={FormNumericTextBox}
                                        format="n0"
                                        min={0}
                                        max={100}
                                    />
                                </Grid>

                                {/* Row 3: Close Date and Description */}
                                <Grid item xs={12} md={6}>
                                    <Field
                                        id="estimatedCloseDate"
                                        name="estimatedCloseDate"
                                        label={getTranslatedLabel(`${localizationKey}.closeDate`, 'Expected Close Date')}
                                        component={FormDatePicker}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
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

                                {/* Row 4: Description */}
                                <Grid item xs={12}>
                                    <Field
                                        id="description"
                                        name="description"
                                        label={getTranslatedLabel(`${localizationKey}.description`, 'Description')}
                                        component={FormInput}
                                    />
                                </Grid>

                                {/* Row 5: Next Step */}
                                <Grid item xs={12} md={8}>
                                    <Field
                                        id="nextStep"
                                        name="nextStep"
                                        label={getTranslatedLabel(`${localizationKey}.nextStep`, 'Next Step')}
                                        component={FormInput}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <Field
                                        id="nextStepDate"
                                        name="nextStepDate"
                                        label={getTranslatedLabel(`${localizationKey}.nextStepDate`, 'Next Step Date')}
                                        component={FormDatePicker}
                                    />
                                </Grid>

                                {/* Divider */}
                                <Grid item xs={12}>
                                    <Divider sx={{ my: 1 }} />
                                    <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
                                        {getTranslatedLabel(`${localizationKey}.linkedContacts`, 'Linked Contacts')}
                                    </Typography>
                                </Grid>

                                {/* Row 6: Contact Picker */}
                                <Grid item xs={12}>
                                    <ContactPicker
                                        label={getTranslatedLabel(`${localizationKey}.contacts`, 'Contacts')}
                                        value={selectedContacts}
                                        onChange={handleContactsChange}
                                        multiple={true}
                                        placeholder={getTranslatedLabel(`${localizationKey}.searchContacts`, 'Search and add contacts...')}
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
                                    type={formRenderProps.allowSubmit ? "submit" : "button"}
                                    onClick={() => {
                                        // If form hasn't changed but contacts have, submit with initial values
                                        if (!formRenderProps.allowSubmit && contactsModified) {
                                            handleSubmit(initialValues);
                                        }
                                    }}
                                    disabled={(!formRenderProps.allowSubmit && !contactsModified) || isProcessing}
                                >
                                    {editMode === 'edit'
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

export default LeadForm;
