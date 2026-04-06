import React, { useState, useCallback } from 'react';
import { Paper, Box, Typography } from '@mui/material';
import Button from '@mui/material/Button';
import AddIcon from '@mui/icons-material/Add';
import DriveFolderUploadIcon from '@mui/icons-material/DriveFolderUpload';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';

import CRMMenu from '../menu/CRMMenu';
import LeadsList from './LeadsList';
import LeadsForm from './LeadsForm';           // ← Fixed import name (was LeadsForm)
import { Lead } from '../models/lead';
import ExcelUploadDialog from './ExcelUploadDialog';
import ImportedDataGrid from './ImportedDataGrid';

const LeadsDashboard: React.FC = () => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.leads';

    const [editMode, setEditMode] = useState<number>(0); // 0 = List, 1 = Create, 2 = Edit
    const [selectedLead, setSelectedLead] = useState<Lead | undefined>(undefined);

    const [uploadOpen, setUploadOpen] = useState(false);
    const [importedData, setImportedData] = useState<any[]>([]);
    const [importedFileName, setImportedFileName] = useState('');
    const [showImportedGrid, setShowImportedGrid] = useState(false);

    // Handlers
    const handleUploadExcel = useCallback(() => {
        setUploadOpen(true);
    }, []);

    const handleDataParsed = useCallback((data: any[], fileName: string) => {
        setImportedData(data);
        setImportedFileName(fileName);
        setShowImportedGrid(true);
        setUploadOpen(false);
    }, []);

    const handleCloseImported = useCallback(() => {
        setShowImportedGrid(false);
        setImportedData([]);
        setImportedFileName('');
    }, []);

    const handleCreateNew = useCallback(() => {
        setSelectedLead(undefined);
        setEditMode(1);
    }, []);

    const handleEditLead = useCallback((lead: Lead) => {
        setSelectedLead(lead);
        setEditMode(2);
    }, []);

    const handleCloseForm = useCallback(() => {
        setEditMode(0);
        setSelectedLead(undefined);
    }, []);

    const handleFormSuccess = useCallback(() => {
        // RTK Query will automatically refresh the list via cache invalidation
        handleCloseForm();
    }, [handleCloseForm]);

    const handleCreateBatchLeads = useCallback(() => {
        // TODO: Implement batch creation logic
        console.log('Creating batch leads from imported data...', importedData);
        // After success, you can call handleCloseImported() and refresh list
    }, [importedData]);

    // ────── Conditional Rendering ──────

    // 1. Show Imported Data Grid (highest priority when active)
    if (showImportedGrid) {
        return (
            <ImportedDataGrid
                data={importedData}
                fileName={importedFileName}
                onClose={handleCloseImported}
            />
        );
    }

    // 2. Show Create/Edit Form
    if (editMode > 0) {
        return (
            <LeadsForm
                lead={selectedLead}
                editMode={editMode as 1 | 2}
                onClose={handleCloseForm}
                onSuccess={handleFormSuccess}
            />
        );
    }

    // 3. Default: Show Dashboard with List
    return (
        <>
            <CRMMenu selectedMenuItem="leads" />

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
                        p: 3,
                        borderBottom: '1px solid',
                        borderColor: 'divider',
                    }}
                >
                    <Typography variant="h5" fontWeight="600">
                        {getTranslatedLabel(`${localizationKey}.title`, 'Leads')}
                    </Typography>

                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <Button
                            variant="contained"
                            color="primary"
                            startIcon={<AddIcon />}
                            onClick={handleCreateNew}
                        >
                            {getTranslatedLabel(`${localizationKey}.createNew`, 'New Lead')}
                        </Button>

                        <Button
                            variant="contained"
                            color="secondary"
                            startIcon={<DriveFolderUploadIcon />}
                            onClick={handleUploadExcel}
                        >
                            {getTranslatedLabel(`${localizationKey}.uploadExcel`, 'Upload Excel')}
                        </Button>
                    </Box>
                </Box>

                {/* Leads List */}
                <LeadsList
                    onCreateNew={handleCreateNew}
                    onEditLead={handleEditLead}
                />
            </Paper>

            {/* Excel Upload Dialog */}
            <ExcelUploadDialog
                open={uploadOpen}
                onClose={() => setUploadOpen(false)}
                onDataParsed={handleDataParsed}
            />
        </>
    );
};

export default LeadsDashboard;