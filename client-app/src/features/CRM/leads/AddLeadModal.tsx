import React from 'react';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    IconButton,
    Box,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';

import LeadForm from "./LeadsForm";
import { SalesOpportunityLead } from '../models/salesOpportunity';

interface AddLeadModalProps {
    open: boolean;
    onClose: () => void;
    onLeadCreated: (newLead: SalesOpportunityLead) => void;
}

const AddLeadModal: React.FC<AddLeadModalProps> = ({
    open,
    onClose,
    onLeadCreated,
}) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.leads.form';

    const handleLeadSuccess = (createdLead?: any) => {
        if (createdLead) {
            onLeadCreated(createdLead);
        }
        onClose(); // Always close the modal after success
    };

    return (
        <Dialog
            open={open}
            onClose={onClose}
            maxWidth="md"
            fullWidth
            scroll="paper"
            aria-labelledby="add-lead-dialog-title"
            disableEnforceFocus          // ← Important for Kendo popups
            disableRestoreFocus          // ← Helps with focus issues
            disablePortal
        >
            <DialogTitle id="add-lead-dialog-title" sx={{ 
                display: 'flex', 
                justifyContent: 'space-between', 
                alignItems: 'center',
                borderBottom: '1px solid #e0e0e0',
                pb: 2
            }}>
                {getTranslatedLabel(`${localizationKey}.createTitle`, 'Create New Lead')}
                <IconButton onClick={onClose} size="small" aria-label="close">
                    <CloseIcon />
                </IconButton>
            </DialogTitle>

            <DialogContent sx={{ p: 0 }}>
                <LeadForm
                    editMode={1}
                    onClose={onClose}
                    onSuccess={handleLeadSuccess}
                    isModal={true}
                />
            </DialogContent>
        </Dialog>
    );
};

export default AddLeadModal;