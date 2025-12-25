import React, { useState, useCallback } from 'react';
import { Paper, Box, Typography } from '@mui/material';
import Button from '@mui/material/Button';
import AddIcon from '@mui/icons-material/Add';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import CRMMenu from '../menu/CRMMenu';
import ContactsList from './ContactsList';
import ContactForm from './ContactForm';
import { Contact } from '../models/contact';

type EditMode = 'none' | 'create' | 'edit';

const ContactsDashboard: React.FC = () => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.contacts';

    const [editMode, setEditMode] = useState<EditMode>('none');
    const [selectedContact, setSelectedContact] = useState<Contact | undefined>();

    const handleCreateNew = useCallback(() => {
        setSelectedContact(undefined);
        setEditMode('create');
    }, []);

    const handleEditContact = useCallback((contact: Contact) => {
        setSelectedContact(contact);
        setEditMode('edit');
    }, []);

    const handleCloseForm = useCallback(() => {
        setEditMode('none');
        setSelectedContact(undefined);
    }, []);

    const handleFormSuccess = useCallback(() => {
        // RTK Query cache invalidation will refresh the data
    }, []);

    return (
        <>
            <CRMMenu selectedMenuItem="contacts" />

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
                        {getTranslatedLabel(`${localizationKey}.title`, 'People')}
                    </Typography>

                    {editMode === 'none' && (
                        <Button
                            variant="contained"
                            color="primary"
                            startIcon={<AddIcon />}
                            onClick={handleCreateNew}
                        >
                            {getTranslatedLabel(`${localizationKey}.createNew`, 'New Contact')}
                        </Button>
                    )}
                </Box>

                {/* Content */}
                {editMode !== 'none' ? (
                    <ContactForm
                        contact={selectedContact}
                        editMode={editMode}
                        onClose={handleCloseForm}
                        onSuccess={handleFormSuccess}
                    />
                ) : (
                    <ContactsList
                        onCreateNew={handleCreateNew}
                        onEditContact={handleEditContact}
                    />
                )}
            </Paper>
        </>
    );
};

export default ContactsDashboard;
