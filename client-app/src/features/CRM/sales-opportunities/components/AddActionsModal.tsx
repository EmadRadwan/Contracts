import React from 'react';
import {
    Modal,
    Box,
    Typography,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    TextField,
    Button,
    Divider,
    Card,
    Chip,
    Backdrop,
    Stack,
    Avatar,
} from '@mui/material';
import { Person as PersonIcon } from '@mui/icons-material';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import { OpportunityMeetingLocation, OpportunityMeetingType, SalesOpportunity, SalesOpportunityAction } from '../../models/salesOpportunity';
import {
    useCreateOpportunityActionMutation,
    useFetchActionTypesQuery,
    useFetchCancellationReasonsQuery,
    useFetchMeetingLocationsQuery,
    useFetchMeetingTypesQuery,
    useFetchOpportunityActionsQuery,
} from '../../../../app/store/configureStore';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs from 'dayjs';

interface AddActionModalProps {
    open: boolean;
    onClose: () => void;
    opportunity: SalesOpportunity | null;
}

const AddActionsModal: React.FC<AddActionModalProps> = ({ open, onClose, opportunity }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.opportunities.actionModal';

    // Form State
    const [nextAction, setNextAction] = React.useState('FOLLOW_UP');
    const [comment, setComment] = React.useState('');
    const [stageDate, setStageDate] = React.useState<string>('');
    const [cancelReason, setCancelReason] = React.useState('');
    const [meetingType, setMeetingType] = React.useState('');
    const [meetingLocation, setMeetingLocation] = React.useState('');
    const [note, setNote] = React.useState('');

    const hasDateField = ['FOLLOW_UP', 'SET_MEETING', 'FRESH_STAGE', 'INTERESTED', 'FOLLOWING_UP_AFTER_MEETING', 'NO_ANSWER'].includes(nextAction);
    const hasCancelReasonField = ['CANCELLATION'].includes(nextAction);
    const hasMeetingDropdownsAndNote = ['MEETING', 'SITE_VISIT'].includes(nextAction);

    // Queries
    const { data: cancellationReasons, isLoading: loadingCancellationReasons } = useFetchCancellationReasonsQuery();
    const { data: actionTypes, isLoading: loadingActionTypes } = useFetchActionTypesQuery();
    const { data: meetingTypes, isLoading: loadingMeetingTypes } = useFetchMeetingTypesQuery();
    const { data: meetingLocations, isLoading: loadingMeetingLocations } = useFetchMeetingLocationsQuery();

    const {
        data: opportunityActions = [],
        isLoading: loadingActions
    } = useFetchOpportunityActionsQuery(
        opportunity?.salesOpportunityId!,
        { skip: !opportunity?.salesOpportunityId }
    );

    const [createAction, { isLoading: creatingAction }] = useCreateOpportunityActionMutation();

    const handleSaveAction = async () => {
        if (!opportunity) return;

        const payload: SalesOpportunityAction = {
            salesOpportunityId: opportunity.salesOpportunityId,
            actionTypeId: nextAction,
            comment: comment.trim() || undefined,
            actionDate: hasDateField && stageDate ? stageDate : undefined,
            cancelReasonId: hasCancelReasonField && cancelReason ? cancelReason : undefined,
        };

        try {
            await createAction({ id: opportunity.salesOpportunityId!, action: payload }).unwrap();
            // Reset form after success
            setComment('');
            setStageDate('');
            setCancelReason('');
        } catch (error) {
            console.error('Failed to create action:', error);
        }
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
                    width: 580,
                    maxHeight: '85vh',           // Restricted modal height
                    bgcolor: 'background.paper',
                    borderRadius: 2,
                    boxShadow: 24,
                    overflow: 'hidden',
                    display: 'flex',
                    flexDirection: 'column',
                }}
            >
                {/* Header */}
                <Box sx={{ p: 3, pb: 2, borderBottom: '1px solid', borderColor: 'divider' }}>
                    <Typography variant="h6" fontWeight="bold">
                        {getTranslatedLabel(`${localizationKey}.title`, 'Add Action')}
                    </Typography>
                </Box>

                {/* Form Section - Fixed height, not scrollable */}
                <Box sx={{ p: 3, flexShrink: 0 }}>
                    {/* Next Action */}
                    <FormControl fullWidth sx={{ mb: 3 }}>
                        <InputLabel>{getTranslatedLabel(`${localizationKey}.nextAction`, 'Next Action')}</InputLabel>
                        <Select
                            value={nextAction}
                            label={getTranslatedLabel(`${localizationKey}.nextAction`, 'Next Action')}
                            onChange={(e) => setNextAction(e.target.value)}
                        >
                            {loadingActionTypes ? (
                                <MenuItem value=""><em>Loading...</em></MenuItem>
                            ) : (
                                actionTypes?.map((action) => (
                                    <MenuItem key={action.actionId} value={action.actionId}>
                                        {action.description}
                                    </MenuItem>
                                ))
                            )}
                        </Select>
                    </FormControl>

                    {/* Cancel Reason */}
                    {hasCancelReasonField && (
                        <FormControl fullWidth sx={{ mb: 3 }}>
                            <InputLabel>{getTranslatedLabel(`${localizationKey}.cancelReason`, 'Cancel Reason *')}</InputLabel>
                            <Select
                                value={cancelReason}
                                label={getTranslatedLabel(`${localizationKey}.cancelReason`, 'Cancel Reason *')}
                                onChange={(e) => setCancelReason(e.target.value)}
                            >
                                {loadingCancellationReasons ? (
                                    <MenuItem value=""><em>Loading...</em></MenuItem>
                                ) : (
                                    cancellationReasons?.map((reason) => (
                                        <MenuItem key={reason.reasonId} value={reason.reasonId}>
                                            {reason.description}
                                        </MenuItem>
                                    ))
                                )}
                            </Select>
                        </FormControl>
                    )}

                    {hasMeetingDropdownsAndNote && (
                        <>
                            <FormControl fullWidth sx={{ mb: 3 }}>
                                <InputLabel>{getTranslatedLabel(`${localizationKey}.meetingType`, 'Meeting Type *')}</InputLabel>
                                <Select
                                    value={meetingType}
                                    label={getTranslatedLabel(`${localizationKey}.meetingType`, 'Meeting Type *')}
                                    onChange={(e) => setMeetingType(e.target.value)}
                                >
                                    {loadingMeetingTypes ? (
                                        <MenuItem value=""><em>Loading...</em></MenuItem>
                                    ) : (
                                        meetingTypes?.map((type: OpportunityMeetingType) => (
                                            <MenuItem key={type.meetingTypeId} value={type.meetingTypeId}>
                                                {type.description}
                                            </MenuItem>
                                        ))
                                    )}
                                </Select>
                            </FormControl>
                            <FormControl fullWidth sx={{ mb: 3 }}>
                                <InputLabel>{getTranslatedLabel(`${localizationKey}.meetingLocation`, 'Meeting Location *')}</InputLabel>
                                <Select
                                    value={meetingLocation}
                                    label={getTranslatedLabel(`${localizationKey}.meetingLocation`, 'Meeting Location *')}
                                    onChange={(e) => setMeetingLocation(e.target.value)}
                                >
                                    {loadingMeetingTypes ? (
                                        <MenuItem value=""><em>Loading...</em></MenuItem>
                                    ) : (
                                        meetingLocations?.map((type: OpportunityMeetingLocation) => (
                                            <MenuItem key={type.meetingLocationId} value={type.meetingLocationId}>
                                                {type.description}
                                            </MenuItem>
                                        ))
                                    )}
                                </Select>
                            </FormControl>

                            {/* Comment */}
                    <TextField
                        fullWidth
                        label={getTranslatedLabel(`${localizationKey}.note`, 'Note')}
                        multiline
                        rows={4}
                        value={note}
                        onChange={(e) => setNote(e.target.value)}
                        placeholder={getTranslatedLabel(`${localizationKey}.notePlaceholder`, 'Note...')}
                        sx={{ mb: 3 }}
                    />
                        </>
                        
                    )}

                    {/* Action Date */}
                    {hasDateField && (
                        <LocalizationProvider dateAdapter={AdapterDayjs}>
                            <DateTimePicker
                                label={getTranslatedLabel(`${localizationKey}.stageDate`, 'Next Action Date')}
                                value={stageDate ? dayjs(stageDate) : null}
                                onChange={(newValue) => setStageDate(newValue ? newValue.format('YYYY-MM-DDTHH:mm') : '')}
                                disablePast
                                slotProps={{
                                    textField: {
                                        fullWidth: true,
                                        sx: { mb: 3 },
                                        required: true,
                                    },
                                }}
                            />
                        </LocalizationProvider>
                    )}

                    {/* Comment */}
                    <TextField
                        fullWidth
                        label={getTranslatedLabel(`${localizationKey}.comment`, 'Comment')}
                        multiline
                        rows={4}
                        value={comment}
                        onChange={(e) => setComment(e.target.value)}
                        placeholder={getTranslatedLabel(`${localizationKey}.commentPlaceholder`, 'Write your comment here...')}
                        sx={{ mb: 3 }}
                    />

                    {/* Action Buttons */}
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
                        <Button variant="outlined" onClick={onClose}>
                            {getTranslatedLabel(`${localizationKey}.cancel`, 'Cancel')}
                        </Button>
                        <Button
                            variant="contained"
                            onClick={handleSaveAction}
                            disabled={(hasDateField && !stageDate) || (hasCancelReasonField && !cancelReason) || creatingAction}
                            loading={creatingAction}
                        >
                            {getTranslatedLabel(`${localizationKey}.saveAction`, 'Save Action')}
                        </Button>
                    </Box>
                </Box>

                <Divider />

                {/* Scrollable Action History Section */}
                <Box sx={{ 
                    flex: 1, 
                    minHeight: 0,                    // Important for flex scrolling
                    display: 'flex',
                    flexDirection: 'column',
                    overflow: 'hidden'
                }}>
                    <Box sx={{ p: 3, pb: 1 }}>
                        <Typography variant="subtitle2" gutterBottom>
                            {getTranslatedLabel(`${localizationKey}.actionHistory`, 'Comments')}
                        </Typography>
                    </Box>

                    <Box sx={{ 
                        flex: 1, 
                        overflowY: 'auto', 
                        px: 3, 
                        pb: 3 
                    }}>
                        {loadingActions ? (
                            <Typography variant="body2" color="text.secondary">
                                {getTranslatedLabel(`${localizationKey}.loadingActions`, 'Loading actions...')}
                            </Typography>
                        ) : opportunityActions.length === 0 ? (
                            <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic' }}>
                                {getTranslatedLabel(`${localizationKey}.noActionsYet`, 'No actions recorded yet.')}
                            </Typography>
                        ) : (
                            <Stack spacing={2}>
                                {opportunityActions.map((action: SalesOpportunityAction) => (
                                    <Card key={action.salesOpportunityActionId} variant="outlined" sx={{ p: 2.5 }}>
                                        <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5, mb: 1.5 }}>
                                            <Avatar sx={{ width: 34, height: 34, bgcolor: 'primary.main' }}>
                                                <PersonIcon fontSize="small" />
                                            </Avatar>
                                            <Box sx={{ flex: 1 }}>
                                                <Typography variant="subtitle2" fontWeight="medium">
                                                    {action.actionTypeDescription || action.actionTypeId}
                                                </Typography>
                                                <Typography variant="caption" color="text.secondary">
                                                    {dayjs(action.createdStamp).format('ddd DD/MM/YYYY - hh:mm A')}
                                                </Typography>
                                            </Box>

                                            {action.isAnswered && (
                                                <Chip
                                                    label={getTranslatedLabel(`${localizationKey}.answered`, 'Answered')}
                                                    color="success"
                                                    size="small"
                                                />
                                            )}
                                        </Box>

                                        {/* Action Date */}
                                        {action.actionDate && (
                                            <Typography variant="body2" sx={{ mb: 1 }}>
                                                <strong>
                                                    {getTranslatedLabel(`${localizationKey}.nextActionDate`, 'Next Action Date')}:
                                                </strong>{' '}
                                                {dayjs(action.actionDate).format('ddd DD/MM/YYYY - hh:mm A')}
                                            </Typography>
                                        )}

                                        {/* Cancel Reason */}
                                        {action.cancelReasonDescription && (
                                            <Typography variant="body2" color="error" sx={{ mb: 1 }}>
                                                <strong>
                                                    {getTranslatedLabel(`${localizationKey}.cancelReason`, 'Cancel Reason')}:
                                                </strong>{' '}
                                                {action.cancelReasonDescription}
                                            </Typography>
                                        )}

                                        {/* Comment */}
                                        {action.comment && (
                                            <Typography 
                                                variant="body2" 
                                                sx={{ mt: 1, whiteSpace: 'pre-wrap', lineHeight: 1.6 }}
                                            >
                                                <strong>
                                                    {getTranslatedLabel(`${localizationKey}.comment`, 'Comment')}:
                                                </strong>{' '}
                                                {action.comment}
                                            </Typography>
                                        )}

                                        {!action.comment && !action.actionDate && !action.cancelReasonDescription && (
                                            <Typography variant="body2" color="text.secondary" fontStyle="italic">
                                                {getTranslatedLabel(`${localizationKey}.noDetails`, 'No additional details')}
                                            </Typography>
                                        )}
                                    </Card>
                                ))}
                            </Stack>
                        )}
                    </Box>
                </Box>
            </Box>
        </Modal>
    );
};

export default AddActionsModal;