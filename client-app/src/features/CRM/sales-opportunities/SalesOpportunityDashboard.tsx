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
import OpportunityForm from './SalesOpportunityForm';
import { SalesOpportunity } from '../models/salesOpportunity';
import { useAppSelector } from '../../../app/store/configureStore';

type ViewMode = 'board' | 'list';

const SalesOpportunityDashboard: React.FC = () => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.opportunities';
    const language = useAppSelector((state) => state.localization.language);

    const [viewMode, setViewMode] = useState<ViewMode>('board');
    const [editMode, setEditMode] = useState<number>(0); // 0 = none, 1 = create, 2 = edit
    const [selectedOpportunity, setSelectedOpportunity] = useState<SalesOpportunity | undefined>();

    const handleViewChange = useCallback((_: React.MouseEvent<HTMLElement>, newView: ViewMode | null) => {
        if (newView !== null) {
            setViewMode(newView);
        }
    }, []);

    const handleCreateNew = useCallback(() => {
        setSelectedOpportunity(undefined);
        setEditMode(1);
    }, []);

    const handleEditOpportunity = useCallback((opportunity: SalesOpportunity) => {
        setSelectedOpportunity(opportunity);
        setEditMode(2);
    }, []);

    const handleCloseForm = useCallback(() => {
        setEditMode(0);
        setSelectedOpportunity(undefined);
    }, []);

    const handleFormSuccess = useCallback(() => {
        // RTK Query will auto-refresh the list/board
    }, []);

    // Show form when in edit or create mode
    if (editMode > 0) {
        return (
            <OpportunityForm
                opportunity={selectedOpportunity}
                editMode={editMode}
                onClose={handleCloseForm}
                onSuccess={handleFormSuccess}
            />
        );
    }

    return (
        <>
            <CRMMenu selectedMenuItem="sales-opportunities" />

            <Paper
                elevation={5}
                className="div-container-withBorderCurved"
                sx={{ mt: 2, mx: 2 }}
            >
                {/* Header */}
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

                    <Box sx={{ display: 'flex', gap: 2, flexDirection: language === "ar" ? "row-reverse" : "row", alignItems: 'center' }}>
                        {/* View Toggle */}
                        <ToggleButtonGroup
                            value={viewMode}
                            exclusive
                            onChange={handleViewChange}
                            size="small"
                            sx={{ flexDirection: language === "ar" ? "row-reverse" : "row" }}
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


                        <Button
                            variant="contained"
                            color="primary"

                            onClick={handleCreateNew}
                        >
                            <AddIcon sx={{ ml: language === "ar" ? 0.5 : 0, mr: language === "ar" ? 0 : 0.5 }} /> {getTranslatedLabel(`${localizationKey}.createNew`, 'New Opportunity')}
                        </Button>
                    </Box>
                </Box>

                {/* Main Content */}
                {viewMode === 'board' ? (
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