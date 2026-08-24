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
    Alert,
    TextField,
    Chip,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { toast } from 'react-toastify';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import {
    useAppSelector,
    useBulkAssignLeadsMutation,
} from '../../../app/store/configureStore';
import { BulkAssignResult } from '../models/lead';
import { FormComboBoxVirtualPartySalesRep } from '../../../app/common/form/FormComboBoxVirtualPartySalesRep';

interface BulkAssignLeadsModalProps {
    open: boolean;
    onClose: () => void;
    /** PartyIds of the leads to assign */
    leadPartyIds: string[];
    /** Called after a successful assignment, e.g. to clear the grid selection */
    onAssigned?: (result: BulkAssignResult) => void;
}

interface SalesRepValue {
    fromPartyId: string;
    fromPartyName: string;
}

const BulkAssignLeadsModal: React.FC<BulkAssignLeadsModalProps> = ({
    open,
    onClose,
    leadPartyIds,
    onAssigned,
}) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.leads.assign';
    const language = useAppSelector((state) => state.localization.language);

    const [selectedRep, setSelectedRep] = useState<SalesRepValue | null>(null);
    const [comments, setComments] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [result, setResult] = useState<BulkAssignResult | null>(null);

    const [bulkAssign, { isLoading }] = useBulkAssignLeadsMutation();

    useEffect(() => {
        if (open) {
            setSelectedRep(null);
            setComments('');
            setError(null);
            setResult(null);
        }
    }, [open]);

    const handleAssign = async () => {
        if (!selectedRep?.fromPartyId || leadPartyIds.length === 0) return;
        setError(null);

        try {
            const res = await bulkAssign({
                leadPartyIds,
                ownerPartyId: selectedRep.fromPartyId,
                comments: comments.trim() || undefined,
            }).unwrap();

            setResult(res);

            if (res.failed === 0) {
                toast.success(
                    getTranslatedLabel(`${localizationKey}.bulkAssigned`, '{0} leads assigned')
                        .replace('{0}', String(res.successful))
                );
            }

            onAssigned?.(res);
        } catch (err: any) {
            setError(
                err?.data ||
                getTranslatedLabel(`${localizationKey}.assignError`, 'Failed to assign leads')
            );
        }
    };

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
                {getTranslatedLabel(`${localizationKey}.bulkTitle`, 'Assign Leads')}
                <IconButton onClick={onClose} size="small" aria-label="close">
                    <CloseIcon />
                </IconButton>
            </DialogTitle>

            <DialogContent sx={{ pt: 3 }}>
                {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

                {/* ---------- Result view ---------- */}
                {result ? (
                    <Box dir={language === 'ar' ? 'rtl' : 'ltr'}>
                        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
                            <Chip
                                label={`${getTranslatedLabel(`${localizationKey}.assigned`, 'Assigned')}: ${result.successful}`}
                                color="success"
                            />
                            {result.alreadyOwned > 0 && (
                                <Chip
                                    label={`${getTranslatedLabel(`${localizationKey}.alreadyOwned`, 'Already owned')}: ${result.alreadyOwned}`}
                                    variant="outlined"
                                />
                            )}
                            {result.failed > 0 && (
                                <Chip
                                    label={`${getTranslatedLabel(`${localizationKey}.failed`, 'Failed')}: ${result.failed}`}
                                    color="error"
                                />
                            )}
                        </Box>

                        {result.ownerName && (
                            <Typography variant="body2" sx={{ mb: 2 }}>
                                {getTranslatedLabel(`${localizationKey}.assignedTo`, 'Assigned to')}:{' '}
                                <strong>{result.ownerName}</strong>
                            </Typography>
                        )}

                        {result.errors.length > 0 && (
                            <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 240 }}>
                                <Table size="small" stickyHeader>
                                    <TableHead>
                                        <TableRow>
                                            <TableCell>{getTranslatedLabel(`${localizationKey}.lead`, 'Lead')}</TableCell>
                                            <TableCell>{getTranslatedLabel(`${localizationKey}.reason`, 'Reason')}</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {result.errors.map((e) => (
                                            <TableRow key={e.leadPartyId}>
                                                <TableCell>{e.leadName || e.leadPartyId}</TableCell>
                                                <TableCell>{e.reason}</TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        )}
                    </Box>
                ) : (
                    /* ---------- Form view ---------- */
                    <Box>
                        <Typography variant="body2" sx={{ mb: 3 }} dir={language === 'ar' ? 'rtl' : 'ltr'}>
                            {getTranslatedLabel(`${localizationKey}.bulkCount`, '{0} leads selected')
                                .replace('{0}', String(leadPartyIds.length))}
                        </Typography>

                        <Box sx={{ mb: 3 }}>
                            <FormComboBoxVirtualPartySalesRep
                                value={selectedRep}
                                label={getTranslatedLabel(`${localizationKey}.newOwner`, 'Assign to *')}
                                disabled={isLoading}
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
                            disabled={isLoading}
                            label={getTranslatedLabel(`${localizationKey}.comments`, 'Comments')}
                            placeholder={getTranslatedLabel(`${localizationKey}.commentsPlaceholder`, 'Optional note about this assignment...')}
                            value={comments}
                            onChange={(e) => setComments(e.target.value)}
                            dir={language === 'ar' ? 'rtl' : 'ltr'}
                        />
                    </Box>
                )}
            </DialogContent>

            <DialogActions sx={{ px: 3, pb: 2 }}>
                {result ? (
                    <Button variant="contained" onClick={onClose}>
                        {getTranslatedLabel('general.close', 'Close')}
                    </Button>
                ) : (
                    <>
                        <Button onClick={onClose} disabled={isLoading}>
                            {getTranslatedLabel('general.cancel', 'Cancel')}
                        </Button>
                        <Button
                            variant="contained"
                            onClick={handleAssign}
                            disabled={!selectedRep || isLoading || leadPartyIds.length === 0}
                        >
                            {getTranslatedLabel(`${localizationKey}.assign`, 'Assign')}
                        </Button>
                    </>
                )}
            </DialogActions>
        </Dialog>
    );
};

export default BulkAssignLeadsModal;
