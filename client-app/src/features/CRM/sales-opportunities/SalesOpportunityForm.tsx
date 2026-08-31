import React, { useState } from 'react';
import { Grid, Paper, Box, Typography, Divider, Button, Alert, AlertTitle } from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import {
    useFetchOpportunityStagesQuery,
    useCreateOpportunityMutation,
    useUpdateOpportunityMutation,
    useFetchOpenOpportunitiesByLeadsQuery,
    useAppSelector
} from '../../../app/store/configureStore';
import { SalesOpportunity, SalesOpportunityLead } from '../models/salesOpportunity';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import { MemoizedFormDropDownList } from '../../../app/common/form/MemoizedFormDropDownList';
import { requiredValidator } from '../../../app/common/form/Validators';
import LeadPicker from '../components/LeadPicker';
import { FormSimpleComboBoxVirtualApartmentsByProject } from '../../../app/common/form/FormSimpleComboBoxVirtualApartmentsByProject';
import CRMMenu from '../menu/CRMMenu';
import AddActionModal from './components/AddActionsModal';
import AddIcon from '@mui/icons-material/Add';
import AddLeadModal from '../leads/AddLeadModal';
import { FormComboBoxVirtualPartySalesRep } from '../../../app/common/form/FormComboBoxVirtualPartySalesRep';
import { FormComboBoxVirtualPartyBroker } from '../../../app/common/form/FormComboBoxVirtualPartyBroker';
import {
    FormComboBoxVirtualProjectWithApartments
} from "../../../app/common/form/FormComboBoxVirtualProjectWithApartments";

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
}: OpportunityFormProps) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.opportunities';
    const language = useAppSelector((state) => state.localization.language);

    const [openActionModal, setOpenActionModal] = useState(false);
    const [openLeadModal, setOpenLeadModal] = useState(false);   // ← New state

    const { data: stages, isLoading: loadingStages } = useFetchOpportunityStagesQuery();
    const [createOpportunity, { isLoading: creating }] = useCreateOpportunityMutation();
    const [updateOpportunity, { isLoading: updating }] = useUpdateOpportunityMutation();
    const [showBroker, setShowBroker] = useState(opportunity ? opportunity.leads.some(lead => lead.dataSourceId === "INDIRECT") : false);
    const [leadsError, setLeadsError] = useState<string | null>(null);



    const handleLeadCreated = (newLead: SalesOpportunityLead) => {
        // Add the newly created lead to the selected leads
        const updatedLeads = [newLead];
        console.log('New lead created and added to opportunity:', newLead);
        setSelectedLeads(updatedLeads);
        setLeadsModified(true);
        setShowBroker(newLead?.dataSourceId === "INDIRECT");
        setOpenLeadModal(false);   // Close the modal
    };

    const [selectedLeads, setSelectedLeads] = useState<SalesOpportunityLead[]>(opportunity?.leads || []);
    const [leadsModified, setLeadsModified] = useState(false);

    // Form state for cascading
    const [selectedProjectId, setSelectedProjectId] = useState<string | null>(
        opportunity?.workEffortId || null
    );

    // A lead may legitimately be on several opportunities - one buyer often pursues
    // more than one unit - so this warns, it does not block. It is here to make the
    // existing deals visible at the moment of linking, which is the only point where
    // someone can tell a genuine second deal from an accidental duplicate.
    const linkedLeadIds = selectedLeads
        .map((lead) => lead.partyId)
        .filter((id): id is string => !!id);

    const { data: leadsOtherOpportunities = [] } = useFetchOpenOpportunitiesByLeadsQuery(
        {
            leadPartyIds: linkedLeadIds,
            // Editing an opportunity must not warn about itself.
            excludeOpportunityId: opportunity?.salesOpportunityId,
        },
        { skip: linkedLeadIds.length === 0 }
    );

    const otherOpportunitiesByLead = leadsOtherOpportunities.reduce<
        Record<string, typeof leadsOtherOpportunities>
    >((acc, entry) => {
        const key = entry.leadName || entry.leadPartyId || '';
        (acc[key] ||= []).push(entry);
        return acc;
    }, {});

    const handleLeadsChange = (leads: SalesOpportunityLead[]) => {
        setSelectedLeads(leads);
        setLeadsError(null);
        setLeadsModified(true);
        if (leads && leads?.length > 0 && leads[0].dataSourceId === "INDIRECT") {
            setShowBroker(true);
        } else {
            setShowBroker(false);
        }
    };

    const handleOpenAction = () => {
        if (opportunity) setOpenActionModal(true);
    };

    const handleSubmit = async (values: any) => {
        setLeadsError(null);
        if (selectedLeads.length === 0) {
            setLeadsError(getTranslatedLabel(`${localizationKey}.validation.leadsRequired`, 'At least one lead must be linked to the opportunity.'));
            return;
        }
        const opportunityData: SalesOpportunity = {
            ...values,
            workEffortId: values.workEffortId?.projectId || null,
            productId: values.productId?.apartmentId || null,
            ownerPartyId: values.ownerPartyId?.fromPartyId || null,
            brokerPartyId: values.brokerPartyId?.fromPartyId || null,
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
            ownerPartyId: opportunity.ownerPartyId
                ? {
                    fromPartyId: opportunity.ownerPartyId,
                    fromPartyName: opportunity.ownerName || '',     // optional: if you have the name
                }
                : null,
            brokerPartyId: opportunity.brokerPartyId
                ? {
                    fromPartyId: opportunity.brokerPartyId,
                    fromPartyName: opportunity.brokerName || '',     // optional: if you have the name
                }
                : null,
        };
    };

    if (loadingStages) {
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
                            ? getTranslatedLabel(`${localizationKey}.editTitle`, 'Edit Opportunity: {0}').replace("{0}", opportunity?.salesOpportunityId || "")
                            : getTranslatedLabel(`${localizationKey}.createNew`, 'Create New Opportunity')}
                    </Typography>

                    {editMode === 2 && opportunity && (
                        <Button
                            variant="outlined"
                            startIcon={<MoreVertIcon />}
                            onClick={handleOpenAction}
                        >
                            {getTranslatedLabel(`${localizationKey}.actionModal.menu`, 'Add Action')}
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
                                            label={getTranslatedLabel(`${localizationKey}.form.stage`, 'Stage *')}
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
                                            component={FormComboBoxVirtualProjectWithApartments}
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
                                            label={getTranslatedLabel(`${localizationKey}.form.unit`, 'Unit')}
                                            component={FormSimpleComboBoxVirtualApartmentsByProject}
                                            projectId={selectedProjectId}   // ← This is the key fix
                                        />
                                    </Grid>

                                    <Grid item xs={4}>
                                        <Field
                                            name="ownerPartyId"
                                            label={getTranslatedLabel(`${localizationKey}.form.owner`, 'Owner *')}
                                            component={FormComboBoxVirtualPartySalesRep}
                                            validator={requiredValidator}
                                        />
                                    </Grid>

                                    {showBroker && (
                                        <Grid item xs={4}>
                                            <Field
                                                name="brokerPartyId"
                                                label={getTranslatedLabel(`${localizationKey}.form.broker`, 'Broker *')}
                                                component={FormComboBoxVirtualPartyBroker}
                                                validator={requiredValidator}
                                            />
                                        </Grid>)}

                                    {/* Linked Leads Section */}
                                    <Grid item xs={12}>
                                        {/* Inside the Linked Leads Section */}
                                        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
                                            <Typography variant="subtitle2" color="text.secondary">
                                                {getTranslatedLabel(`${localizationKey}.form.linkedLeads`, 'Linked Leads *')}
                                            </Typography>

                                            <Button
                                                variant="outlined"
                                                size="small"
                                                disabled={editMode > 1}
                                                onClick={() => setOpenLeadModal(true)}
                                            >
                                                <AddIcon sx={{ ml: language === "ar" ? 0.5 : 0, mr: language === "ar" ? 0 : 0.5 }} /> {getTranslatedLabel(`${localizationKey}.addNewLead`, 'Add New Lead')}
                                            </Button>
                                        </Box>
                                        <Divider sx={{ mb: 2 }} />
                                    </Grid>

                                    <Grid item xs={12}>
                                        <LeadPicker
                                            label={getTranslatedLabel(`${localizationKey}.leads`, 'Leads')}
                                            value={selectedLeads}
                                            onChange={handleLeadsChange}
                                            multiple
                                        />
                                        {leadsError && (
                                            <Typography variant="caption" color="error">
                                                {leadsError}
                                            </Typography>
                                        )}

                                        {Object.keys(otherOpportunitiesByLead).length > 0 && (
                                            <Alert severity="warning" sx={{ mt: 2 }}>
                                                <AlertTitle>
                                                    {getTranslatedLabel(
                                                        `${localizationKey}.duplicateLeadWarningTitle`,
                                                        'This lead is already on another open opportunity'
                                                    )}
                                                </AlertTitle>

                                                {Object.entries(otherOpportunitiesByLead).map(([leadName, entries]) => (
                                                    <Box key={leadName} sx={{ mb: 1 }}>
                                                        <Typography variant="body2" fontWeight="bold">
                                                            {leadName}
                                                        </Typography>
                                                        {entries.map((entry) => (
                                                            <Typography
                                                                key={entry.salesOpportunityId}
                                                                variant="body2"
                                                                component="div"
                                                            >
                                                                {'\u2022 '}
                                                                {entry.opportunityName || `#${entry.salesOpportunityId}`}
                                                                {' \u2014 '}
                                                                {entry.stageDescription || entry.opportunityStageId}
                                                                {entry.productId && (
                                                                    <>
                                                                        {' \u00b7 '}
                                                                        {getTranslatedLabel(`${localizationKey}.form.unit`, 'Unit')}
                                                                        {' '}
                                                                        {entry.productId}
                                                                    </>
                                                                )}
                                                            </Typography>
                                                        ))}
                                                    </Box>
                                                ))}

                                                <Typography variant="caption" color="text.secondary">
                                                    {getTranslatedLabel(
                                                        `${localizationKey}.duplicateLeadWarningHint`,
                                                        'This is allowed - one buyer can pursue several units. Continue only if this is a genuinely different deal.'
                                                    )}
                                                </Typography>
                                            </Alert>
                                        )}
                                    </Grid>
                                </Grid>

                                {/* Action Buttons */}
                                <Box sx={{ mt: 4, display: 'flex', gap: 2 }}>
                                    <Button
                                        variant="contained"
                                        onClick={formRenderProps.onSubmit}
                                        disabled={isProcessing || (!formRenderProps.allowSubmit && !leadsModified)}
                                    >
                                        {editMode === 2
                                            ? getTranslatedLabel(`general.update`, 'Update')
                                            : getTranslatedLabel(`general.create`, 'Create')}
                                    </Button>
                                    <Button
                                        variant="outlined"
                                        onClick={onClose}
                                        disabled={isProcessing}
                                    >
                                        {getTranslatedLabel(`general.cancel`, 'Cancel')}
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
                onClose={() => {
                    setOpenActionModal(false);
                    // onClose();
                }}
                opportunity={opportunity || null}
            />

            <AddLeadModal
                open={openLeadModal}
                onClose={() => setOpenLeadModal(false)}
                onLeadCreated={handleLeadCreated}
            />
        </>
    );
};

export default OpportunityForm;