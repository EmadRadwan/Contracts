import React, { useState } from 'react';
import {
    Modal,
    Box,
    Typography,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    TextField,
    FormControlLabel,
    Switch,
    Button,
    Divider,
    Card,
    Chip,
    Backdrop,
} from '@mui/material';
import { Person as PersonIcon } from '@mui/icons-material';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import { SalesOpportunity } from '../../models/salesOpportunity';
import { useFetchActionTypesQuery, useFetchCancellationReasonsQuery } from '../../../../app/store/configureStore';

interface AddActionModalProps {
    open: boolean;
    onClose: () => void;
    opportunity: SalesOpportunity | null;
}

const AddActionsModal: React.FC<AddActionModalProps> = ({ open, onClose, opportunity }: AddActionModalProps) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.opportunities.actionModal';

    const [nextAction, setNextAction] = useState('Set meeting');
    const [stageDate, setStageDate] = useState('');
    const [comment, setComment] = useState('');
    const [cancelReason, setCancelReason] = useState('');

    const { data: cancellationReasons, isLoading: loadingCancellationReasons } = useFetchCancellationReasonsQuery();
    const { data: actionTypes, isLoading: loadingActionTypes } = useFetchActionTypesQuery();

    const handleSaveAction = () => {
        if (!opportunity) return;
        
        onClose();
    };

    if (!opportunity) return null;

    return (
        <Modal
            open={open}
            onClose={onClose}
            closeAfterTransition
            slots={{ backdrop: Backdrop }}
            slotProps={{ backdrop: { timeout: 500 } }}
        >
            <Box
                sx={{
                    position: 'absolute',
                    top: '50%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                    width: 540,
                    bgcolor: 'background.paper',
                    borderRadius: 2,
                    boxShadow: 24,
                    overflow: 'hidden',
                }}
            >
                {/* Header */}
                <Box sx={{ p: 3, pb: 2, borderBottom: '1px solid', borderColor: 'divider' }}>
                    <Typography variant="h6" fontWeight="bold">
                        {getTranslatedLabel(`${localizationKey}.title`, 'Add Action')}
                    </Typography>
                </Box>

                <Box sx={{ p: 3 }}>
                    {/* Next Action */}
                    <FormControl fullWidth sx={{ mb: 3 }}>
                        <InputLabel>{getTranslatedLabel(`${localizationKey}.nextAction`, 'Next Action')}</InputLabel>
                        <Select
                            value={nextAction}
                            label={getTranslatedLabel(`${localizationKey}.nextAction`, 'Next Action')}
                            onChange={(e) => setNextAction(e.target.value)}
                        >
                            {loadingActionTypes ? (
                                <MenuItem value="">
                                    <em>Loading...</em>
                                </MenuItem>
                            ) : (
                                actionTypes?.map((action) => (
                                    <MenuItem key={action.actionId} value={action.actionId}>
                                        {action.description}
                                    </MenuItem>
                                ))
                            )}
                        </Select>
                    </FormControl>

                    {/* Conditional Field */}
                    {nextAction === 'CANCELLATION' ? (
                        <FormControl fullWidth sx={{ mb: 3 }}>
                            <InputLabel>{getTranslatedLabel(`${localizationKey}.cancelReason`, 'Cancel Reason *')}</InputLabel>
                            <Select
                                value={cancelReason}
                                label={getTranslatedLabel(`${localizationKey}.cancelReason`, 'Cancel Reason *')}
                                onChange={(e) => setCancelReason(e.target.value)}
                            >
                                {loadingCancellationReasons ? (
                                    <MenuItem value="">
                                        <em>Loading...</em>
                                    </MenuItem>
                                ) : (
                                    cancellationReasons?.map((reason) => (
                                        <MenuItem key={reason.reasonId} value={reason.reasonId}>
                                            {reason.description}
                                        </MenuItem>
                                    ))
                                )}
                            </Select>
                        </FormControl>
                    ) : (
                        <TextField
                            fullWidth
                            label={getTranslatedLabel(`${localizationKey}.stageDate`, 'Stage Date *')}
                            type="datetime-local"
                            value={stageDate}
                            onChange={(e) => setStageDate(e.target.value)}
                            sx={{ mb: 3 }}
                            InputLabelProps={{ shrink: true }}
                        />
                    )}

                    {/* Comment */}
                    <TextField
                        fullWidth
                        label={getTranslatedLabel(`${localizationKey}.comment`, 'Comment *')}
                        multiline
                        rows={4}
                        value={comment}
                        onChange={(e) => setComment(e.target.value)}
                        placeholder={getTranslatedLabel(`${localizationKey}.commentPlaceholder`, 'write your comment')}
                        sx={{ mb: 3 }}
                    />

                    {/* Answer Toggle
                    <FormControlLabel
                        control={
                            <Switch
                                checked={isAnswered}
                                onChange={(e) => setIsAnswered(e.target.checked)}
                            />
                        }
                        label={getTranslatedLabel(`${localizationKey}.answer`, 'Answer')}
                        sx={{ mb: 3, display: 'block' }}
                    /> */}

                    {/* Buttons */}
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
                        <Button variant="outlined" onClick={onClose}>
                            {getTranslatedLabel(`${localizationKey}.cancel`, 'Cancel')}
                        </Button>
                        <Button
                            variant="contained"
                            onClick={handleSaveAction}
                            disabled={!comment.trim()}
                        >
                            {getTranslatedLabel(`${localizationKey}.saveAction`, 'Save Action')}
                        </Button>
                    </Box>
                </Box>

                {/* Static Comments Section - Kept hardcoded as requested */}
                <Divider />
                <Box sx={{ p: 3 }}>
                    <Typography variant="subtitle2" gutterBottom>
                        Comments
                    </Typography>
                    <Card variant="outlined" sx={{ p: 2 }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                            <PersonIcon />
                            <Typography fontWeight="medium">Mohamed Gamal</Typography>
                        </Box>
                        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 1 }}>
                            <Chip label="Naseem" size="small" />
                            <Chip label="Not interested" size="small" />
                            <Chip label="low budget" size="small" />
                            <Chip label="Answered - 00:00" color="success" size="small" />
                        </Box>
                        <Typography variant="body2" sx={{ mt: 1 }}>low budget</Typography>

                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2, fontSize: '0.85rem', color: 'text.secondary' }}>
                            <div>
                                Next Action Date:<br />
                                <strong>Wed 15/04/2026 - 11:00 AM</strong>
                            </div>
                            <div style={{ textAlign: 'right' }}>
                                Comment Date:<br />
                                <strong>Mon 30/03/2026 - 11:20 AM</strong>
                            </div>
                        </Box>
                    </Card>
                </Box>
            </Box>
        </Modal>
    );
};

export default AddActionsModal;