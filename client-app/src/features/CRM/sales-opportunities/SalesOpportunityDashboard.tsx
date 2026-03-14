import React, { useState, useCallback } from 'react';
import {
    Paper,
    Box,
    ToggleButtonGroup,
    ToggleButton,
    Typography
} from '@mui/material';
import Button from '@mui/material/Button';
import ViewListIcon from '@mui/icons-material/ViewList';
import ViewKanbanIcon from '@mui/icons-material/ViewKanban';
import AddIcon from '@mui/icons-material/Add';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import CRMMenu from '../menu/CRMMenu';
import SalesOpportunityBoard from './SalesOpportunityBoard';
import SalesOpportunityList from './SalesOpportunityList';
import LeadForm from './SalesOpportunityForm';
import { SalesOpportunity } from '../models/salesOpportunity';

type ViewMode = 'board' | 'list';
type EditMode = 'none' | 'create' | 'edit';

const SalesOpportunityDashboard: React.FC = () => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.leads';

    const [viewMode, setViewMode] = useState<ViewMode>('board');
    const [editMode, setEditMode] = useState<EditMode>('none');
    const [selectedOpportunity, setSelectedOpportunity] = useState<SalesOpportunity | undefined>();
    

    const handleViewChange = useCallback((_: React.MouseEvent<HTMLElement>, newView: ViewMode | null) => {
        if (newView !== null) {
            setViewMode(newView);
        }
    }, []);

    const handleCreateNew = useCallback(() => {
        setSelectedOpportunity(undefined);
        setEditMode('create');
    }, []);

    const handleEditOpportunity = useCallback((opportunity: SalesOpportunity) => {
        setSelectedOpportunity(opportunity);
        setEditMode('edit');
    }, []);

    const handleCloseForm = useCallback(() => {
        setEditMode('none');
        setSelectedOpportunity(undefined);
    }, []);

    const handleFormSuccess = useCallback(() => {
        // The RTK Query cache invalidation will automatically refresh the data
    }, []);

    return (
        <>
            <CRMMenu selectedMenuItem="sales-opportunities" />

            <Paper
                elevation={5}
                className="div-container-withBorderCurved"
                sx={{ mt: 2, mx: 2 }}
            >
                {/* Header with View Switcher */}
                <Box
                    sx={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center',
                        p: 2,
                        borderBottom: '1px solid',
                        borderColor: 'divider'
                    }}
                >
                    <Typography variant="h5" fontWeight="medium">
                        {getTranslatedLabel(`${localizationKey}.title`, 'Sales Pipeline')}
                    </Typography>

                    {editMode !== "create" && <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                        {/* View Toggle */}
                        <ToggleButtonGroup
                            value={viewMode}
                            exclusive
                            onChange={handleViewChange}
                            size="small"
                        >
                            <ToggleButton value="board" aria-label="board view" sx={{ gap: 1 }}>
                                <ViewKanbanIcon />
                                {getTranslatedLabel(`${localizationKey}.boardView`, 'Board')}
                            </ToggleButton>
                            <ToggleButton value="list" aria-label="list view" sx={{ gap: 1 }}>
                                <ViewListIcon />
                                {getTranslatedLabel(`${localizationKey}.listView`, 'List')}
                            </ToggleButton>
                        </ToggleButtonGroup>

                        {/* Create Button (shown in board view) */}
                        {viewMode === 'board' && (
                            <Button
                                variant="contained"
                                color="primary"
                                sx={{ gap: 1 }}
                                onClick={handleCreateNew}
                            >
                                <AddIcon />
                                {getTranslatedLabel(`${localizationKey}.createNew`, 'New Opportunity')}
                            </Button>
                        )}
                        
                    </Box>}
                </Box>

                {/* Content Area */}
                {editMode !== 'none' ? (
                    <LeadForm
                        opportunity={selectedOpportunity}
                        editMode={editMode}
                        onClose={handleCloseForm}
                        onSuccess={handleFormSuccess}
                    />
                ) : viewMode === 'board' ? (

                    <SalesOpportunityBoard onEditOpportunity={handleEditOpportunity} />

                ) : (
                    <SalesOpportunityList
                        onCreateNew={handleCreateNew}
                        onEditOpportunity={handleEditOpportunity}
                    />
                )}
            </Paper>
        </>
    );
};

export default SalesOpportunityDashboard;
