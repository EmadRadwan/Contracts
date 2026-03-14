import React, { useState, useCallback } from 'react';
import {
    Box,
    Paper,
    Typography,
    Card,
    CardContent,
    Chip,
    IconButton,
    Skeleton,
    Tooltip
} from '@mui/material';
import {
    Edit as EditIcon,
    Person as PersonIcon,
    AttachMoney as MoneyIcon,
    CalendarToday as CalendarIcon
} from '@mui/icons-material';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import {
    useFetchOpportunitiesQuery,
    useFetchOpportunityStagesQuery,
    useUpdateOpportunityStageMutation
} from '../../../app/store/configureStore';
import { SalesOpportunity, OpportunityStage } from '../models/salesOpportunity';
import LoadingComponent from '../../../app/layout/LoadingComponent';

interface LeadsBoardProps {
    onEditOpportunity: (opportunity: SalesOpportunity) => void;
}

const LeadsBoard: React.FC<LeadsBoardProps> = ({ onEditOpportunity }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.leads.board';

    const { data: opportunities, isLoading: loadingOpportunities, refetch } = useFetchOpportunitiesQuery();
    const { data: stages, isLoading: loadingStages } = useFetchOpportunityStagesQuery();
    const [updateStage] = useUpdateOpportunityStageMutation();

    const [draggedItem, setDraggedItem] = useState<SalesOpportunity | null>(null);
    const [dragOverStage, setDragOverStage] = useState<string | null>(null);

    const handleDragStart = useCallback((e: React.DragEvent, opportunity: SalesOpportunity) => {
        setDraggedItem(opportunity);
        e.dataTransfer.effectAllowed = 'move';
    }, []);

    const handleDragOver = useCallback((e: React.DragEvent, stageId: string) => {
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
        setDragOverStage(stageId);
    }, []);

    const handleDragLeave = useCallback(() => {
        setDragOverStage(null);
    }, []);

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

    const getOpportunitiesByStage = (stageId: string) => {
        return opportunities?.filter(o => o.opportunityStageId === stageId) || [];
    };

    const getStageTotal = (stageId: string) => {
        return getOpportunitiesByStage(stageId).reduce((sum, o) => sum + (o.estimatedAmount || 0), 0);
    };

    const formatCurrency = (amount: number, currency?: string) => {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: currency || 'USD',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(amount);
    };

    const formatDate = (dateString?: string) => {
        if (!dateString) return '';
        return new Date(dateString).toLocaleDateString();
    };

    if (loadingStages || loadingOpportunities) {
        return <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`, 'Loading pipeline...')} />;
    }

    return (
        <Box sx={{ display: 'flex', gap: 2, overflowX: 'auto', p: 2, minHeight: '70vh' }}>
            {stages?.map((stage: OpportunityStage) => {
                const stageOpportunities = getOpportunitiesByStage(stage.opportunityStageId);
                const stageTotal = getStageTotal(stage.opportunityStageId);
                const isDropTarget = dragOverStage === stage.opportunityStageId;

                return (
                    <Paper
                        key={stage.opportunityStageId}
                        elevation={isDropTarget ? 8 : 2}
                        onDragOver={(e) => handleDragOver(e, stage.opportunityStageId)}
                        onDragLeave={handleDragLeave}
                        onDrop={(e) => handleDrop(e, stage.opportunityStageId)}
                        sx={{
                            minWidth: 280,
                            maxWidth: 320,
                            bgcolor: isDropTarget ? 'action.hover' : 'background.paper',
                            borderRadius: 2,
                            display: 'flex',
                            flexDirection: 'column',
                            transition: 'all 0.2s ease',
                            border: isDropTarget ? '2px dashed primary.main' : '1px solid',
                            borderColor: isDropTarget ? 'primary.main' : 'divider'
                        }}
                    >
                        {/* Column Header */}
                        <Box
                            sx={{
                                p: 2,
                                borderBottom: '1px solid',
                                borderColor: 'divider',
                                bgcolor: 'grey.50'
                            }}
                        >
                            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                                <Typography variant="subtitle1" fontWeight="bold">
                                    {stage.description}
                                </Typography>
                                <Chip
                                    label={stageOpportunities.length}
                                    size="small"
                                    color="primary"
                                    variant="outlined"
                                />
                            </Box>
                            <Typography variant="body2" color="text.secondary">
                                {formatCurrency(stageTotal)}
                            </Typography>
                        </Box>

                        {/* Cards Container */}
                        <Box
                            sx={{
                                p: 1,
                                flexGrow: 1,
                                overflowY: 'auto',
                                maxHeight: 'calc(70vh - 100px)',
                                display: 'flex',
                                flexDirection: 'column',
                                gap: 1
                            }}
                        >
                            {stageOpportunities.map((opportunity) => (
                                <Card
                                    key={opportunity.salesOpportunityId}
                                    draggable
                                    onDragStart={(e) => handleDragStart(e, opportunity)}
                                    sx={{
                                        cursor: 'grab',
                                        '&:hover': {
                                            boxShadow: 4,
                                            bgcolor: 'action.hover'
                                        },
                                        '&:active': {
                                            cursor: 'grabbing'
                                        },
                                        opacity: draggedItem?.salesOpportunityId === opportunity.salesOpportunityId ? 0.5 : 1
                                    }}
                                >
                                    <CardContent sx={{ p: 1.5, '&:last-child': { pb: 1.5 } }}>
                                        {/* Header with name and edit */}
                                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                                            <Typography
                                                variant="subtitle2"
                                                fontWeight="medium"
                                                sx={{
                                                    overflow: 'hidden',
                                                    textOverflow: 'ellipsis',
                                                    whiteSpace: 'nowrap',
                                                    maxWidth: '80%'
                                                }}
                                            >
                                                {opportunity.opportunityName}
                                            </Typography>
                                            <IconButton
                                                size="small"
                                                onClick={() => onEditOpportunity(opportunity)}
                                                sx={{ p: 0.5 }}
                                            >
                                                <EditIcon fontSize="small" />
                                            </IconButton>
                                        </Box>

                                        {/* Value */}
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 0.5 }}>
                                            <MoneyIcon fontSize="small" color="success" />
                                            <Typography variant="body2" fontWeight="medium" color="success.main">
                                                {formatCurrency(opportunity.estimatedAmount || 0, opportunity.currencyUomId)}
                                            </Typography>
                                            {opportunity.estimatedProbability && (
                                                <Chip
                                                    label={`${opportunity.estimatedProbability}%`}
                                                    size="small"
                                                    sx={{ ml: 'auto', height: 20, fontSize: '0.7rem' }}
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

                                        {/* Leads count */}
                                        {opportunity.leads && opportunity.leads.length > 0 && (
                                            <Tooltip title={opportunity.leads.map(c => c.partyName).join(', ')}>
                                                <Chip
                                                    icon={<PersonIcon />}
                                                    label={opportunity.leads.length === 1
                                                        ? getTranslatedLabel(`${localizationKey}.oneLead`, '1 lead')
                                                        : getTranslatedLabel(`${localizationKey}.leads`, `${opportunity.leads.length} leads`).replace('{0}', opportunity.leads.length.toString())}
                                                    size="small"
                                                    variant="outlined"
                                                    sx={{ mt: 1, height: 22, fontSize: '0.7rem' }}
                                                />
                                            </Tooltip>
                                        )}
                                    </CardContent>
                                </Card>
                            ))}

                            {stageOpportunities.length === 0 && (
                                <Box
                                    sx={{
                                        p: 2,
                                        textAlign: 'center',
                                        color: 'text.disabled',
                                        border: '1px dashed',
                                        borderColor: 'divider',
                                        borderRadius: 1
                                    }}
                                >
                                    <Typography variant="body2">
                                        {getTranslatedLabel(`${localizationKey}.noDeals`, 'No deals')}
                                    </Typography>
                                </Box>
                            )}
                        </Box>
                    </Paper>
                );
            })}
        </Box>
    );
};

export default LeadsBoard;
