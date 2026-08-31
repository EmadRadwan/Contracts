import React, { useState, useEffect } from 'react';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Box,
    Button,
    IconButton,
    Typography,
    Chip,
    Alert,
    TextField,
    Divider,
    Stack,
    Card,
    CircularProgress,
    LinearProgress,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import PersonIcon from '@mui/icons-material/Person';
import { toast } from 'react-toastify';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import {
    useAppSelector,
    useAssignLeadMutation,
    useUnassignLeadMutation,
    useFetchLeadAssignmentHistoryQuery,
} from '../../../app/store/configureStore';
import { Lead } from '../models/lead';
import { FormComboBoxVirtualPartySalesRep } from '../../../app/common/form/FormComboBoxVirtualPartySalesRep';

interface AssignLeadModalProps {
    open: boolean;
    onClose: () => void;
    lead?: Lead;
}

interface SalesRepValue {
    fromPartyId: string;
    fromPartyName: string;
}

const AssignLeadModal: React.FC<AssignLeadModalProps> = ({ open, onClose, lead }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.leads.assign';
    const language = useAppSelector((state) => state.localization.language);

    const [selectedRep, setSelectedRep] = useState<SalesRepValue | null>(null);
    const [comments, setComments] = useState('');
    const [error, setError] = useState<string | null>(null);

    const [assignLead, { isLoading: assigning }] = useAssignLeadMutation();
    const [unassignLead, { isLoading: unassigning }] = useUnassignLeadMutation();

    const { data: history = [], isLoading: loadingHistory } = useFetchLeadAssignmentHistoryQuery(
        lead?.partyId!,
        { skip: !open || !lead?.partyId }
    );

    const isProcessing = assigning || unassigning;
    const isReassign = !!lead?.ownerPartyId;

    // Reset the form whenever a different lead is opened
    useEffect(() => {
        if (open) {
            setSelectedRep(null);
            setComments('');
            setError(null);
        }
    }, [open, lead?.partyId]);

    const handleAssign = async () => {
        if (!lead?.partyId || !selectedRep?.fromPartyId) return;
        setError(null);

        try {
            await assignLead({
                id: lead.partyId,
                request: {
                    ownerPartyId: selectedRep.fromPartyId,
                    comments: comments.trim() || undefined,
                },
            }).unwrap();

            toast.success(
                getTranslatedLabel(`${localizationKey}.assigned`, 'Lead assigned successfully')
            );
            onClose();
        } catch (err: any) {
            setError(
                err?.data ||
                getTranslatedLabel(`${localizationKey}.assignError`, 'Failed to assign lead')
            );
        }
    };

    const handleUnassign = async () => {
        if (!lead?.partyId) return;
        setError(null);

        try {
            await unassignLead(lead.partyId).unwrap();
            toast.success(
                getTranslatedLabel(`${localizationKey}.unassigned`, 'Lead returned to the unassigned pool')
            );
            onClose();
        } catch (err: any) {
            setError(
                err?.data ||
                getTranslatedLabel(`${localizationKey}.unassignError`, 'Failed to unassign lead')
            );
        }
    };

    if (!lead) return null;

    return (
        <Dialog
            open={open}
            onClose={onClose}
            maxWidth="sm"
            fullWidth
            disableEnforceFocus   // ← Kendo popups need this
            disableRestoreFocus
            disablePortal
        >
            <DialogTitle
                sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    borderBottom: '1px solid #e0e0e0',
                    pb: 2,
                }}
            >
                {isReassign
                    ? getTranslatedLabel(`${localizationKey}.reassignTitle`, 'Reassign Lead')
                    : getTranslatedLabel(`${localizationKey}.assignTitle`, 'Assign Lead')}
                <IconButton onClick={onClose} size="small" aria-label="close">
                    <CloseIcon />
                </IconButton>
            </DialogTitle>

            {/* Determinate progress is impossible here, so an indeterminate bar
                pinned under the title tells the user the dialog is busy without
                shifting any layout. */}
            <Box sx={{ height: 4 }}>
                {isProcessing && <LinearProgress />}
            </Box>

            <DialogContent sx={{ pt: 3 }}>
                {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

                {/* Which lead */}
                <Box sx={{ mb: 3 }} dir={language === 'ar' ? 'rtl' : 'ltr'}>
                    <Typography variant="caption" color="text.secondary">
                        {getTranslatedLabel(`${localizationKey}.lead`, 'Lead')}
                    </Typography>
                    <Typography variant="subtitle1" fontWeight="medium">
                        {lead.fullName || `${lead.firstName ?? ''} ${lead.lastName ?? ''}`.trim()}
                    </Typography>
                </Box>

                {/* Current owner */}
                <Box sx={{ mb: 3 }} dir={language === 'ar' ? 'rtl' : 'ltr'}>
                    <Typography variant="caption" color="text.secondary" display="block" gutterBottom>
                        {getTranslatedLabel(`${localizationKey}.currentOwner`, 'Current owner')}
                    </Typography>
                    {lead.ownerName ? (
                        <Chip icon={<PersonIcon />} label={lead.ownerName} size="small" color="primary" variant="outlined" />
                    ) : (
                        <Chip
                            label={getTranslatedLabel(`${localizationKey}.unassigned`, 'Unassigned')}
                            size="small"
                            variant="outlined"
                        />
                    )}
                </Box>

                {/* New owner */}
                <Box sx={{ mb: 3 }}>
                    <FormComboBoxVirtualPartySalesRep
                        value={selectedRep}
                        label={getTranslatedLabel(`${localizationKey}.newOwner`, 'Assign to *')}
                        disabled={isProcessing}
                        popupSettings={{ appendTo: document.querySelector('.MuiModal-root') as HTMLElement }}
                        onChange={(e: any) => {
                            setSelectedRep(e.value || null);
                            setError(null);
                        }}
                    />
                </Box>

                <TextField
                    fullWidth
                    multiline
                    rows={3}
                    disabled={isProcessing}
                    label={getTranslatedLabel(`${localizationKey}.comments`, 'Comments')}
                    placeholder={getTranslatedLabel(`${localizationKey}.commentsPlaceholder`, 'Optional note about this assignment...')}
                    value={comments}
                    onChange={(e) => setComments(e.target.value)}
                    dir={language === 'ar' ? 'rtl' : 'ltr'}
                />

                {/* ---------- Ownership history ---------- */}
                <Divider sx={{ my: 3 }} />

                <Typography variant="subtitle2" gutterBottom sx={{ textAlign: language === 'ar' ? 'end' : 'start' }}>
                    {getTranslatedLabel(`${localizationKey}.history`, 'Ownership history')}
                </Typography>

                {loadingHistory ? (
                    <Typography variant="body2" color="text.secondary">
                        {getTranslatedLabel(`${localizationKey}.loadingHistory`, 'Loading history...')}
                    </Typography>
                ) : history.length === 0 ? (
                    <Typography variant="body2" color="text.secondary" fontStyle="italic">
                        {getTranslatedLabel(`${localizationKey}.noHistory`, 'This lead has never been assigned.')}
                    </Typography>
                ) : (
                    <Stack spacing={1.5} sx={{ maxHeight: 220, overflowY: 'auto' }}>
                        {history.map((entry, i) => (
                            <Card
                                key={`${entry.ownerPartyId}-${entry.fromDate}-${i}`}
                                variant="outlined"
                                sx={{ p: 1.5 }}
                                dir={language === 'ar' ? 'rtl' : 'ltr'}
                            >
                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                                    <PersonIcon fontSize="small" color="action" />
                                    <Typography variant="body2" fontWeight="medium">
                                        {entry.ownerName || entry.ownerPartyId}
                                    </Typography>
                                    {entry.isCurrent && (
                                        <Chip
                                            label={getTranslatedLabel(`${localizationKey}.current`, 'Current')}
                                            size="small"
                                            color="success"
                                            sx={{ height: 20, fontSize: '0.7rem' }}
                                        />
                                    )}
                                </Box>

                                <Typography variant="caption" color="text.secondary" display="block">
                                    {new Date(entry.fromDate).toLocaleString()}
                                    {' \u2192 '}
                                    {entry.thruDate
                                        ? new Date(entry.thruDate).toLocaleString()
                                        : getTranslatedLabel(`${localizationKey}.present`, 'present')}
                                    {entry.assignedByUserLogin && (
                                        <>
                                            {' \u00b7 '}
                                            {getTranslatedLabel(`${localizationKey}.assignedBy`, 'by')}{' '}
                                            {entry.assignedByUserLogin}
                                        </>
                                    )}
                                </Typography>

                                {entry.comments && (
                                    <Typography variant="body2" sx={{ mt: 0.5, whiteSpace: 'pre-wrap' }}>
                                        {entry.comments}
                                    </Typography>
                                )}
                            </Card>
                        ))}
                    </Stack>
                )}
            </DialogContent>

            <DialogActions sx={{ px: 3, pb: 2, justifyContent: 'space-between' }}>
                {/* Unassign is only meaningful when the lead currently has an owner */}
                <Box>
                    {isReassign && (
                        <Button
                            color="error"
                            onClick={handleUnassign}
                            disabled={isProcessing}
                            startIcon={unassigning ? <CircularProgress size={16} color="inherit" /> : undefined}
                        >
                            {unassigning
                                ? getTranslatedLabel(`${localizationKey}.unassigning`, 'Unassigning...')
                                : getTranslatedLabel(`${localizationKey}.unassign`, 'Unassign')}
                        </Button>
                    )}
                </Box>

                <Box sx={{ display: 'flex', gap: 1 }}>
                    <Button onClick={onClose} disabled={isProcessing}>
                        {getTranslatedLabel('general.cancel', 'Cancel')}
                    </Button>
                    <Button
                        variant="contained"
                        onClick={handleAssign}
                        disabled={!selectedRep || isProcessing}
                        startIcon={assigning ? <CircularProgress size={16} color="inherit" /> : undefined}
                    >
                        {assigning
                            ? getTranslatedLabel(`${localizationKey}.assigning`, 'Assigning...')
                            : isReassign
                                ? getTranslatedLabel(`${localizationKey}.reassign`, 'Reassign')
                                : getTranslatedLabel(`${localizationKey}.assign`, 'Assign')}
                    </Button>
                </Box>
            </DialogActions>
        </Dialog>
    );
};

export default AssignLeadModal;
