import React, { useState, useCallback } from 'react';
import { Paper, Box, Typography } from '@mui/material';
import Button from '@mui/material/Button';
import AddIcon from '@mui/icons-material/Add';
import DriveFolderUploadIcon from '@mui/icons-material/DriveFolderUpload';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import CRMMenu from '../menu/CRMMenu';
import LeadsList from './LeadsList';
import LeadsForm from './LeadsForm'
import { Contact } from '../models/contact';
import ExcelUploadDialog from './ExcelUploadDialog'
import ImportedDataGrid from './ImportedDataGrid'

type EditMode = 'none' | 'create' | 'edit';

const LeadsDashboard: React.FC = () => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.contacts';

    const [editMode, setEditMode] = useState<number>(0);
    const [selectedLead, setSelectedLead] = useState<Contact | undefined>();
    const [uploadOpen, setUploadOpen] = useState(false);
    const [importedData, setImportedData] = useState<any[]>([]);
    const [importedFileName, setImportedFileName] = useState('');
    const [showImportedGrid, setShowImportedGrid] = useState(false);

    const handleUploadExcel = () => {
        setUploadOpen(true);
    };

    const handleDataParsed = (data: any[], fileName: string) => {
        setImportedData(data);
        setImportedFileName(fileName);
        setShowImportedGrid(true);
        setUploadOpen(false);
    };

    const handleCloseImported = () => {
        setShowImportedGrid(false);
        setImportedData([]);
        setImportedFileName('');
    };

    const handleCreateNew = useCallback(() => {
        setSelectedLead(undefined);
        setEditMode(1);
    }, []);

    const handleEditContact = useCallback((lead: Contact) => {
        setSelectedLead(lead);
        setEditMode(2);
    }, []);

    const handleCloseForm = useCallback(() => {
        setEditMode(0);
        setSelectedLead(undefined);
    }, []);

    const handleFormSuccess = useCallback(() => {
        // RTK Query cache invalidation will refresh the data
    }, []);

    const handleCreateBatchLeads = () => {}

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
                        p: 2,
                        borderBottom: '1px solid',
                        borderColor: 'divider'
                    }}
                >
                    <Typography variant="h5" fontWeight="medium">
                        {getTranslatedLabel(`${localizationKey}.title`, 'Leads')}
                    </Typography>

                    {editMode === 0 && (
                        <Box sx={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: 2
                        }}>
                            {!showImportedGrid && <>
                               <Button
                                    variant="contained"
                                    color="primary"
                                    sx={{ gap: 1 }}
                                    onClick={handleCreateNew}
                                >
                                    <AddIcon />
                                    {getTranslatedLabel(`${localizationKey}.createNew`, 'New Contact')}
                                </Button>
                                <Button
                                    variant="contained"
                                    color="secondary"
                                    sx={{ gap: 1 }}
                                    onClick={handleUploadExcel}
                                >
                                    <DriveFolderUploadIcon />
                                    {getTranslatedLabel(`${localizationKey}.uploadExcel`, 'Upload Excel')}
                                </Button>
                            </>}
                            {showImportedGrid && (
                                <Button
                                    variant="contained"
                                    color="primary"
                                    sx={{ gap: 1 }}
                                    onClick={handleCreateBatchLeads}
                                >
                                    <DriveFolderUploadIcon />
                                    {getTranslatedLabel(`${localizationKey}.submitLeads`, 'Create Leads')}
                                </Button>
                            )}
                        </Box>
                    )}
                </Box>

                {/* Content */}
                {showImportedGrid ? (
                    <ImportedDataGrid
                        data={importedData}
                        fileName={importedFileName}
                        onClose={handleCloseImported}
                    />
                ) : (editMode !== 0) ? (
                    <LeadsForm
                        lead={selectedLead}
                        editMode={editMode}
                        onClose={handleCloseForm}
                        onSuccess={handleFormSuccess}
                    />
                ) : (
                    <LeadsList
                        onCreateNew={handleCreateNew}
                        onEditContact={handleEditContact}
                    />
                )}
            </Paper>
            <ExcelUploadDialog
                open={uploadOpen}
                onClose={() => setUploadOpen(false)}
                onDataParsed={handleDataParsed}
            />
            
        </>
    );
};

export default LeadsDashboard;
