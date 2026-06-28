import React, { useState, useCallback } from 'react';
import {
    Box,
    Paper,
    Typography,
    Card,
    CardContent,
    Chip,
    IconButton,
    Tooltip,
} from '@mui/material';
import {
    Edit as EditIcon,
    Person as PersonIcon,
    AttachMoney as MoneyIcon,
    CalendarToday as CalendarIcon,
    MoreVert as MoreVertIcon,
} from '@mui/icons-material';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import {
    useAppSelector,
    useFetchOpportunitiesQuery,
    useFetchOpportunityStagesQuery,
    useUpdateOpportunityStageMutation,
} from '../../../app/store/configureStore';
import { SalesOpportunity, OpportunityStage } from '../models/salesOpportunity';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import AddActionModal from './components/AddActionsModal';

interface SalesOpportunityBoardProps {
    onEditOpportunity: (opportunity: SalesOpportunity) => void;
}

const SalesOpportunityBoard: React.FC<SalesOpportunityBoardProps> = ({ onEditOpportunity }: SalesOpportunityBoardProps) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.opportunities';
    const language = useAppSelector((state) => state.localization.language);

    const { data: opportunities, isLoading: loadingOpportunities, refetch } = useFetchOpportunitiesQuery();
    const { data: stages, isLoading: loadingStages } = useFetchOpportunityStagesQuery();
    const [updateStage] = useUpdateOpportunityStageMutation();

    const [draggedItem, setDraggedItem] = useState<SalesOpportunity | null>(null);
    const [dragOverStage, setDragOverStage] = useState<string | null>(null);

    // Action Modal State
    const [openActionModal, setOpenActionModal] = useState(false);
    const [selectedOpportunityForAction, setSelectedOpportunityForAction] = useState<SalesOpportunity | null>(null);

    const handleOpenActionModal = (opportunity: SalesOpportunity) => {
        setSelectedOpportunityForAction(opportunity);
        setOpenActionModal(true);
    };

    const handleCloseActionModal = () => {
        setOpenActionModal(false);
        setSelectedOpportunityForAction(null);
    };

    const handleDragStart = useCallback((e: React.DragEvent, opportunity: SalesOpportunity) => {
        setDraggedItem(opportunity);
        e.dataTransfer.effectAllowed = 'move';
    }, []);

    const handleDragOver = useCallback((e: React.DragEvent, stageId: string) => {
        e.preventDefault();
        setDragOverStage(stageId);
    }, []);

    const handleDragLeave = useCallback(() => setDragOverStage(null), []);

    const handleDrop = useCallback(async (e: React.DragEvent, targetStageId: string) => {
        e.preventDefault();
        setDragOverStage(null);

        if (draggedItem && draggedItem.opportunityStageId !== targetStageId) {
            try {
                await updateStage({
                    id: draggedItem.salesOpportunityId!,
                    request: {
                        stageId: targetStageId,
                        opportunityName: draggedItem.opportunityName!
                    }
                }).unwrap();
                refetch();
            } catch (error) {
                console.error('Failed to update stage:', error);
            }
        }
        setDraggedItem(null);
    }, [draggedItem, updateStage, refetch]);

    const getOpportunitiesByStage = (stageId: string) =>
        opportunities?.filter(o => o.opportunityStageId === stageId) || [];

    const formatDate = (dateString?: string) =>
        dateString ? new Date(dateString).toLocaleDateString('ar-EG') : '';

    if (loadingStages || loadingOpportunities) {
        return <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`, 'Loading pipeline...')} />;
    }

    return (
        <Box sx={{ display: 'flex', gap: 2, overflowX: 'auto', p: 2, minHeight: '70vh' }}>
            {stages?.map((stage: OpportunityStage) => {
                const stageOpportunities = getOpportunitiesByStage(stage.opportunityStageId);
                const isDropTarget = dragOverStage === stage.opportunityStageId;

                return (
                    <Paper
                        key={stage.opportunityStageId}
                        elevation={isDropTarget ? 8 : 2}
                        onDragOver={(e) => handleDragOver(e, stage.opportunityStageId)}
                        onDragLeave={handleDragLeave}
                        onDrop={(e) => handleDrop(e, stage.opportunityStageId)}
                        sx={{
                            minWidth: 216,
                            maxWidth: 320,
                            bgcolor: isDropTarget ? 'action.hover' : 'background.paper',
                            borderRadius: 2,
                            display: 'flex',
                            flexDirection: 'column',
                            border: isDropTarget ? '2px dashed primary.main' : '1px solid',
                            borderColor: isDropTarget ? 'primary.main' : 'divider',
                        }}
                    >
                        {/* Column Header */}
                        <Box sx={{ p: 2, borderBottom: '1px solid', borderColor: 'divider', bgcolor: 'grey.50' }}>
                            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                <Typography variant="subtitle1" fontWeight="bold">
                                    {stage.description}
                                </Typography>
                                <Chip label={stageOpportunities.length} size="small" color="primary" variant="outlined" />
                            </Box>
                        </Box>

                        {/* Opportunities Cards */}
                        <Box sx={{ p: 1, flexGrow: 1, overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: 1 }}>
                            {stageOpportunities.map((opportunity: SalesOpportunity) => (
                                <Card
                                    key={opportunity.salesOpportunityId}
                                    draggable
                                    onDragStart={(e) => handleDragStart(e, opportunity)}
                                    sx={{
                                        cursor: 'grab',
                                        '&:hover': { boxShadow: 4, bgcolor: 'action.hover' },
                                        '&:active': { cursor: 'grabbing' },
                                    }}
                                >
                                    <CardContent sx={{ p: 1.5 }}>
                                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                                            <Typography
                                                variant="body1"
                                                fontWeight="medium"
                                                sx={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: '75%' }}
                                            >
                                                {opportunity.leads[0]?.partyName || opportunity.opportunityName}
                                            </Typography>

                                            <Box sx={{ display: 'flex', gap: 0.5 }}>
                                                <IconButton size="small" onClick={() => onEditOpportunity(opportunity)}>
                                                    <EditIcon fontSize="small" />
                                                </IconButton>
                                                <IconButton size="small" onClick={() => handleOpenActionModal(opportunity)}>
                                                    <MoreVertIcon fontSize="small" />
                                                </IconButton>
                                            </Box>
                                        </Box>

                                        {/* Value */}
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 0.5 }}>
                                            {/* <MoneyIcon fontSize="small" color="success" />
                                            <Typography variant="body2" fontWeight="medium" color="success.main">
                                                {formatCurrency(opportunity.estimatedAmount || 0)}
                                            </Typography> */}
                                            {opportunity.estimatedProbability && (
                                                <Chip
                                                    label={`${opportunity.estimatedProbability}%`}
                                                    size="small"
                                                    sx={{ ml: language === "ar" ? 0 : 'auto', mr: language === "ar" ? 'auto' : 0, height: 20, fontSize: '0.7rem' }}
                                                />
                                            )}
                                        </Box>

                                        {/* Owner */}
                                        {opportunity.ownerName && (
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 0.5 }}>
                                                <PersonIcon fontSize="small" color="action" />
                                                <Typography variant="caption" color="text.secondary">
                                                    {opportunity.ownerName}
                                                </Typography>
                                            </Box>
                                        )}

                                        {/* Close Date */}
                                        {opportunity.estimatedCloseDate && (
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                                                <CalendarIcon fontSize="small" color="action" />
                                                <Typography variant="caption" color="text.secondary">
                                                    {formatDate(opportunity.estimatedCloseDate)}
                                                </Typography>
                                            </Box>
                                        )}

                                        {/* Leads */}
                                        {opportunity.leads && opportunity.leads.length > 0 && (
                                            <Tooltip title={opportunity.leads.map((c: any) => c.partyName).join(', ')}>
                                                <Chip
                                                    icon={<PersonIcon />}
                                                    label={opportunity.leads.length === 1 ? '1 lead' : `${opportunity.leads.length} leads`}
                                                    size="small"
                                                    variant="outlined"
                                                    sx={{ mt: 1, height: 22, fontSize: '0.7rem', width: '7em' }}
                                                />
                                            </Tooltip>
                                        )}
                                    </CardContent>
                                </Card>
                            ))}

                            {stageOpportunities.length === 0 && (
                                <Box sx={{ p: 3, textAlign: 'center', color: 'text.disabled', border: '1px dashed', borderColor: 'divider', borderRadius: 1 }}>
                                    <Typography variant="body2">No deals in this stage</Typography>
                                </Box>
                            )}
                        </Box>
                    </Paper>
                );
            })}

            {/* Action Modal */}
            <AddActionModal
                open={openActionModal}
                onClose={handleCloseActionModal}
                opportunity={selectedOpportunityForAction}
            />
        </Box>
    );
};

export default SalesOpportunityBoard;