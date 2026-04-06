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
    const [isAnswered, setIsAnswered] = useState(true);
    const [cancelReason, setCancelReason] = useState('');

    const handleSaveAction = () => {
        if (!opportunity) return;
        console.log('Action saved for:', opportunity.opportunityName, {
            nextAction,
            stageDate,
            comment,
            isAnswered,
            cancelReason,
        });
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
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                        {getTranslatedLabel(`${localizationKey}.subtitle`, 'Select which columns you need to see lead table')}
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
                            <MenuItem value="Set meeting">{getTranslatedLabel(`${localizationKey}.actions.setMeeting`, 'Set meeting')}</MenuItem>
                            <MenuItem value="Follow up">{getTranslatedLabel(`${localizationKey}.actions.followUp`, 'Follow up')}</MenuItem>
                            <MenuItem value="Meeting">{getTranslatedLabel(`${localizationKey}.actions.meeting`, 'Meeting')}</MenuItem>
                            <MenuItem value="Following after meeting">{getTranslatedLabel(`${localizationKey}.actions.followingAfterMeeting`, 'Following after meeting')}</MenuItem>
                            <MenuItem value="Cancellation">{getTranslatedLabel(`${localizationKey}.actions.cancellation`, 'Cancellation')}</MenuItem>
                            <MenuItem value="Done deal">{getTranslatedLabel(`${localizationKey}.actions.doneDeal`, 'Done deal')}</MenuItem>
                            <MenuItem value="No Answer">{getTranslatedLabel(`${localizationKey}.actions.noAnswer`, 'No Answer')}</MenuItem>
                        </Select>
                    </FormControl>

                    {/* Conditional Field */}
                    {nextAction === 'Cancellation' ? (
                        <FormControl fullWidth sx={{ mb: 3 }}>
                            <InputLabel>{getTranslatedLabel(`${localizationKey}.cancelReason`, 'Cancel Reason *')}</InputLabel>
                            <Select
                                value={cancelReason}
                                label={getTranslatedLabel(`${localizationKey}.cancelReason`, 'Cancel Reason *')}
                                onChange={(e) => setCancelReason(e.target.value)}
                            >
                                <MenuItem value="low budget">{getTranslatedLabel(`${localizationKey}.reasons.lowBudget`, 'Low budget')}</MenuItem>
                                <MenuItem value="not interested">{getTranslatedLabel(`${localizationKey}.reasons.notInterested`, 'Not interested')}</MenuItem>
                                <MenuItem value="no answer">{getTranslatedLabel(`${localizationKey}.reasons.noAnswer`, 'No answer')}</MenuItem>
                                <MenuItem value="other">{getTranslatedLabel(`${localizationKey}.reasons.other`, 'Other')}</MenuItem>
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