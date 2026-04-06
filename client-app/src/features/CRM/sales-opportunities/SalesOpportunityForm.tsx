import React, { useState, useEffect } from 'react';
import { Grid, Paper, Box, Typography, Divider, Button } from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import {
    useFetchOpportunityStagesQuery,
    useCreateOpportunityMutation,
    useUpdateOpportunityMutation,
    useFetchDataSourcesQuery,
} from '../../../app/store/configureStore';
import { currenciesSelectors, fetchCurrenciesAsync } from '../../catalog/slice/currencySlice';
import { SalesOpportunity, SalesOpportunityLead } from '../models/salesOpportunity';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import { MemoizedFormDropDownList } from '../../../app/common/form/MemoizedFormDropDownList';
import { requiredValidator } from '../../../app/common/form/Validators';
import LeadPicker from '../components/LeadPicker';
import { FormComboBoxVirtualProject } from '../../../app/common/form/FormComboBoxVirtualProject';
import { FormSimpleComboBoxVirtualApartmentsByProject } from '../../../app/common/form/FormSimpleComboBoxVirtualApartmentsByProject';
import CRMMenu from '../menu/CRMMenu';
import AddActionModal from './components/AddActionsModal';
import { useAppDispatch, useAppSelector } from '../../../app/store/configureStore';

interface OpportunityFormProps {
    opportunity?: SalesOpportunity;
    editMode: number; // 1 = create, 2 = edit
    onClose: () => void;
    onSuccess: () => void;
}

const OpportunityForm: React.FC<OpportunityFormProps> = ({ 
    opportunity, 
    editMode, 
    onClose, 
    onSuccess 
}) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.opportunities';

    const dispatch = useAppDispatch();
    const [openActionModal, setOpenActionModal] = useState(false);

    const { data: stages, isLoading: loadingStages } = useFetchOpportunityStagesQuery();
    const [createOpportunity, { isLoading: creating }] = useCreateOpportunityMutation();
    const [updateOpportunity, { isLoading: updating }] = useUpdateOpportunityMutation();

    const { currenciesLoaded } = useAppSelector(state => state.currency);
    const { data: dataSources, isLoading: loadingDataSources } = useFetchDataSourcesQuery();

    const [selectedLeads, setSelectedLeads] = useState<SalesOpportunityLead[]>(opportunity?.leads || []);
    const [leadsModified, setLeadsModified] = useState(false);

    // Form state for cascading
    const [selectedProjectId, setSelectedProjectId] = useState<string | null>(
        opportunity?.workEffortId || null
    );

    useEffect(() => {
        if (!currenciesLoaded) dispatch(fetchCurrenciesAsync());
    }, [currenciesLoaded, dispatch]);

    const handleLeadsChange = (leads: SalesOpportunityLead[]) => {
        setSelectedLeads(leads);
        setLeadsModified(true);
    };

    const handleOpenAction = () => {
        if (opportunity) setOpenActionModal(true);
    };

    const handleSubmit = async (values: any) => {
        const opportunityData: SalesOpportunity = {
            ...values,
            workEffortId: values.workEffortId?.projectId || null,
            productId: values.productId?.apartmentId || null,
            leads: selectedLeads,
        };

        try {
            if (editMode === 2 && opportunity?.salesOpportunityId) {
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
        }
    };

    const isProcessing = creating || updating;

    const getInitialValues = () => {
        if (editMode !== 2 || !opportunity) {
            return {
                opportunityStageId: 'SOSTG_PROSPECT',
                workEffortId: null,
                productId: null,
            };
        }

        return {
            opportunityStageId: opportunity.opportunityStageId || 'SOSTG_PROSPECT',
            // Project ComboBox expects full object { projectId, projectName, facilityId }
            workEffortId: opportunity.workEffortId
                ? {
                    projectId: opportunity.workEffortId,
                    projectName: opportunity.workEffortName || '',   // optional: if you have the name
                    facilityId: opportunity.facilityId || '0',
                }
                : null,

            // Unit ComboBox expects full object { apartmentId, apartmentName, ... }
            productId: opportunity.productId
                ? {
                    apartmentId: opportunity.productId,
                    apartmentName: opportunity.productName || '',     // optional: if you have the name
                    // You can add more fields if needed (floorNumber, status, etc.)
                }
                : null,
        };
    };

    if (loadingStages || !currenciesLoaded || loadingDataSources) {
        return <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`, 'Loading...')} />;
    }

    return (
        <>
            <CRMMenu selectedMenuItem="sales-opportunities" />

            <Paper elevation={5} className="div-container-withBorderCurved" sx={{ p: 3, mt: 2 }}>
                {/* Header */}
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                    <Typography variant="h6" fontWeight="medium">
                        {editMode === 2
                            ? getTranslatedLabel(`${localizationKey}.editTitle`, 'Edit Opportunity')
                            : getTranslatedLabel(`${localizationKey}.createTitle`, 'Create New Opportunity')}
                    </Typography>

                    {editMode === 2 && opportunity && (
                        <Button
                            variant="outlined"
                            startIcon={<MoreVertIcon />}
                            onClick={handleOpenAction}
                        >
                            {getTranslatedLabel(`${localizationKey}.actionModal.title`, 'Add Action')}
                        </Button>
                    )}
                </Box>

                <Form
                    initialValues={getInitialValues()}
                    onSubmit={handleSubmit}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className="k-form-fieldset">
                                <Grid container spacing={3}>
                                    {/* Stage */}
                                    <Grid item xs={4}>
                                        <Field
                                            name="opportunityStageId"
                                            label={getTranslatedLabel(`${localizationKey}.stage`, 'Stage *')}
                                            component={MemoizedFormDropDownList}
                                            data={stages || []}
                                            dataItemKey="opportunityStageId"
                                            textField="description"
                                            validator={requiredValidator}
                                        />
                                    </Grid>

                                    {/* Project */}
                                    <Grid item xs={4}>
                                        <Field
                                            name="workEffortId"
                                            component={FormComboBoxVirtualProject}
                                            label={getTranslatedLabel("projects.certificate.form.project", "Project")}
                                            dataItemKey="projectId"
                                            textField="ProjectName"
                                            onChange={(e: any) => {
                                                const newProjectId = e.value?.projectId || null;
                                                setSelectedProjectId(newProjectId);
                                                
                                                // Clear Unit when Project changes
                                                formRenderProps.onChange("productId", { value: null });
                                                formRenderProps.onChange("workEffortId", { value: e.value });
                                            }}
                                        />
                                    </Grid>

                                    {/* Unit (Apartment) - Cascading */}
                                    <Grid item xs={4}>
                                        <Field
                                            name="productId"
                                            label={getTranslatedLabel(`${localizationKey}.unit`, 'Unit')}
                                            component={FormSimpleComboBoxVirtualApartmentsByProject}
                                            projectId={selectedProjectId}   // ← This is the key fix
                                        />
                                    </Grid>

                                    {/* Linked Leads Section */}
                                    <Grid item xs={12}>
                                        <Divider sx={{ my: 2 }} />
                                        <Typography variant="subtitle2" color="text.secondary">
                                            {getTranslatedLabel(`${localizationKey}.linkedLeads`, 'Linked Leads')}
                                        </Typography>
                                    </Grid>

                                    <Grid item xs={12}>
                                        <LeadPicker
                                            label={getTranslatedLabel(`${localizationKey}.leads`, 'Leads')}
                                            value={selectedLeads}
                                            onChange={handleLeadsChange}
                                            multiple
                                        />
                                    </Grid>
                                </Grid>

                                {/* Action Buttons */}
                                <Box sx={{ mt: 4, display: 'flex', gap: 2 }}>
                                    <Button
                                        variant="contained"
                                        type="submit"
                                        disabled={isProcessing || (!formRenderProps.allowSubmit && !leadsModified)}
                                    >
                                        {editMode === 2
                                            ? getTranslatedLabel(`${localizationKey}.update`, 'Update')
                                            : getTranslatedLabel(`${localizationKey}.create`, 'Create')}
                                    </Button>
                                    <Button 
                                        variant="outlined" 
                                        onClick={onClose} 
                                        disabled={isProcessing}
                                    >
                                        {getTranslatedLabel(`${localizationKey}.cancel`, 'Cancel')}
                                    </Button>
                                </Box>
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>

            {/* Action Modal */}
            <AddActionModal
                open={openActionModal}
                onClose={() => setOpenActionModal(false)}
                opportunity={opportunity || null}
            />
        </>
    );
};

export default OpportunityForm;