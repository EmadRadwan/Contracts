import React, { useState, useEffect } from 'react';
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
    Tabs,
    Tab,
    Grid,
} from '@mui/material';
import {
    Person as PersonIcon,
    History as HistoryIcon,
    Assignment as AssignmentIcon,
    LocationCity as ProjectIcon,
    Apartment as UnitIcon,
} from '@mui/icons-material';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import { OpportunityMeetingLocation, OpportunityMeetingType, SalesOpportunity, SalesOpportunityAction } from '../../models/salesOpportunity';
import {
    useAppSelector,
    useCreateOpportunityActionMutation,
    useFetchActionTypesQuery,
    useFetchCancellationReasonsQuery,
    useFetchMeetingLocationsQuery,
    useFetchMeetingTypesQuery,
    useFetchOpportunityActionsQuery,
    useFetchOpportunityHistoryQuery
} from '../../../../app/store/configureStore';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs from 'dayjs';
import { FormComboBoxVirtualProject } from '../../../../app/common/form/FormComboBoxVirtualProject';
import { FormSimpleComboBoxVirtualApartmentsByProject } from '../../../../app/common/form/FormSimpleComboBoxVirtualApartmentsByProject';
import { toast } from "react-toastify";
interface AddActionModalProps {
    open: boolean;
    onClose: () => void;
    opportunity: SalesOpportunity | null;
}


// Small helper to keep tab panels declarative and avoid unmounting state on switch
interface TabPanelProps {
    children?: React.ReactNode;
    value: number;
    index: number;
}

const TabPanel: React.FC<TabPanelProps> = ({ children, value, index }) => (
    <Box role="tabpanel" hidden={value !== index} sx={{ display: value === index ? 'block' : 'none' }}>
        {value === index && children}
    </Box>
);

const AddActionsModal: React.FC<AddActionModalProps> = ({ open, onClose, opportunity }: AddActionModalProps) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'crm.opportunities.actionModal';

    // Tab state
    const [activeTab, setActiveTab] = useState(0);

    // Form State
    const [nextAction, setNextAction] = React.useState('FOLLOW_UP');
    const [comment, setComment] = React.useState('');
    const [stageDate, setStageDate] = React.useState<string>('');
    const [cancelReason, setCancelReason] = React.useState('');
    const [meetingType, setMeetingType] = React.useState('');
    const [meetingLocation, setMeetingLocation] = React.useState('');
    const [note, setNote] = React.useState('');

    const language = useAppSelector((state) => state.localization.language);

    const hasDateField = ['FOLLOW_UP', 'SET_MEETING', 'FRESH_STAGE', 'INTERESTED', 'FOLLOWING_UP_AFTER_MEETING', 'NO_ANSWER', 'MEETING', 'SITE_VISIT', 'RESERVATION', 'DONE_DEAL'].includes(nextAction);
    const hasCancelReasonField = ['CANCELLATION'].includes(nextAction);
    const hasMeetingDropdownsAndNote = ['MEETING', 'SITE_VISIT'].includes(nextAction);
    const isUnitRelated = ['RESERVATION', 'DONE_DEAL'].includes(nextAction);

    // Queries
    const { data: cancellationReasons, isLoading: loadingCancellationReasons } = useFetchCancellationReasonsQuery();
    const { data: actionTypes, isLoading: loadingActionTypes } = useFetchActionTypesQuery();
    const { data: meetingTypes, isLoading: loadingMeetingTypes } = useFetchMeetingTypesQuery();
    const { data: meetingLocations, isLoading: loadingMeetingLocations } = useFetchMeetingLocationsQuery();

    const [selectedProject, setSelectedProject] = useState<{ projectId: string, projectName: string } | null>(null);
    const [selectedUnit, setSelectedUnit] = useState<{ apartmentId: string, apartmentName: string } | null>(null);

    useEffect(function populateApartmentAndProject() {
        if (!opportunity) {
            setSelectedProject(null);
            setSelectedUnit(null);
            return;
        }
        if (opportunity.productId || opportunity.workEffortId) {
            if (opportunity.productId) {
                setSelectedUnit({ apartmentId: opportunity.productId, apartmentName: opportunity.productName || '' });
            }
            if (opportunity.workEffortId) {
                setSelectedProject({ projectId: opportunity.workEffortId, projectName: opportunity.workEffortName || '' });
            }
        } else {
            setSelectedProject(null);
            setSelectedUnit(null);
        }
    }, [opportunity])

    // Reset to the Actions tab whenever a different opportunity is opened
    useEffect(() => {
        if (open) setActiveTab(0);
    }, [open, opportunity?.salesOpportunityId]);

    const {
        data: opportunityActions = [],
        isLoading: loadingActions
    } = useFetchOpportunityActionsQuery(
        opportunity?.salesOpportunityId!,
        { skip: !opportunity?.salesOpportunityId }
    );

    const {
        data: opportunityHistory = [],
        isLoading: loadingHistory
    } = useFetchOpportunityHistoryQuery(
        opportunity?.salesOpportunityId!,
        { skip: !opportunity?.salesOpportunityId || activeTab !== 1 }
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
            meetingTypeId: hasMeetingDropdownsAndNote && meetingType ? meetingType : undefined,
            meetingLocationId: hasMeetingDropdownsAndNote && meetingLocation ? meetingLocation : undefined,
            note: hasMeetingDropdownsAndNote && note.trim() ? note.trim() : undefined,
            productId: isUnitRelated && selectedUnit ? selectedUnit.apartmentId : undefined,
            workEffortId: isUnitRelated && selectedProject ? selectedProject.projectId : undefined,
        };

        try {
            await createAction({ id: opportunity.salesOpportunityId!, action: payload }).unwrap();
            // Reset form after success
            setComment('');
            setStageDate('');
            setCancelReason('');
            setMeetingType('');
            setMeetingLocation('');
            setNote('');
            toast.success(getTranslatedLabel(`${localizationKey}.actionSaved`, 'Action saved successfully'));
        } catch (error) {
            console.error('Failed to create action:', error);
        }
        onClose();
    };

    if (!opportunity) return null;

    const isActionDisabled = opportunity.isClosed || opportunity.isWon

    // Who the deal is with, which is what identifies it to a salesperson - the
    // opportunity id means nothing to them. Falls back through the deal's own
    // name to the id so the header is never empty. `leads` is optional-chained:
    // an opportunity with no linked lead rows would otherwise throw here.
    const headerName =
        opportunity.leads?.[0]?.partyName?.trim() ||
        opportunity.opportunityName?.trim() ||
        opportunity.salesOpportunityId ||
        '';

    // More than one lead on the deal - name the first and count the rest.
    const otherLeadCount = Math.max((opportunity.leads?.length ?? 0) - 1, 0);

    // What the deal is actually for. The project falls back to its id because
    // WORK_EFFORT_NAME is frequently null - the readable name lives in
    // PROJECT_NAME, which the API already prefers. The unit is shown by id
    // rather than name: "A1-01" is what people quote to each other, not
    // "Apartment A1-01".
    const projectLabel = opportunity.workEffortName?.trim() || opportunity.workEffortId?.trim();
    const unitLabel = opportunity.productId?.trim();

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
                    width: 720,
                    maxWidth: '95vw',
                    maxHeight: '95vh',
                    bgcolor: 'background.paper',
                    borderRadius: 2,
                    boxShadow: 24,
                    overflowY: 'auto',   // ← only this, no flex
                }}
            >
                {/* Header */}
                <Box sx={{ p: 3, pb: 2, borderBottom: '1px solid', borderColor: 'divider' }}>
                    <Typography variant="h6" fontWeight="bold" sx={{ textAlign: 'center' }}>
                        {headerName}
                        {otherLeadCount > 0 && (
                            <Typography component="span" variant="body2" color="text.secondary" sx={{ ml: 1 }}>
                                {getTranslatedLabel(`${localizationKey}.moreLeads`, '+{0} more')
                                    .replace('{0}', String(otherLeadCount))}
                            </Typography>
                        )}
                    </Typography>

                    {/* What the deal is for. Only rendered when there is something to
                        show, so an opportunity with no project or unit keeps a clean
                        one-line header rather than an empty band. */}
                    {(projectLabel || unitLabel) && (
                        <Stack
                            direction="row"
                            spacing={1}
                            justifyContent="center"
                            sx={{ mt: 1.5 }}
                            dir={language === 'ar' ? 'rtl' : 'ltr'}
                        >
                            {projectLabel && (
                                <Chip
                                    icon={<ProjectIcon />}
                                    label={projectLabel}
                                    size="small"
                                    color="primary"
                                    sx={{ fontWeight: 600 }}
                                />
                            )}
                            {unitLabel && (
                                <Chip
                                    icon={<UnitIcon />}
                                    label={unitLabel}
                                    size="small"
                                    color="success"
                                    // The unit id is always latin even in Arabic.
                                    sx={{ fontWeight: 600, direction: 'ltr' }}
                                />
                            )}
                        </Stack>
                    )}
                </Box>

                {/* Tabs */}
                <Tabs
                    value={activeTab}
                    onChange={(_, newValue) => setActiveTab(newValue)}
                    variant="fullWidth"
                    dir={language === 'ar' ? 'rtl' : 'ltr'}
                    sx={{ borderBottom: '1px solid', borderColor: 'divider' }}
                >
                    <Tab
                        icon={<AssignmentIcon fontSize="small" sx={{mr: 1}} />}
                        iconPosition={language === "ar" ? "end" : "start"}
                        spacing={2}
                        label={getTranslatedLabel(`${localizationKey}.actionsTab`, 'Actions')}
                    />
                    <Tab
                        icon={<HistoryIcon fontSize="small" sx={{mr: 1}} />}
                        iconPosition={language === "ar" ? "end" : "start"}
                        label={getTranslatedLabel(`${localizationKey}.historyTab`, 'History')}
                    />
                </Tabs>
                <TabPanel value={activeTab} index={0}>

                    {/* Form Section. Two columns: the short selects pair up so that
                        picking Meeting - which reveals four extra fields - does not
                        push the Save button below the fold. */}
                    <Box sx={{ p: 3, pb: 2 }}>
                      {/* The theme carries direction: "rtl" but nothing sets dir on the
                          DOM, so a flex row stays left-to-right and the first field
                          lands on the left. In Arabic that puts the Date ahead of the
                          Next Action it depends on - hence dir here, as on the Tabs
                          and the Selects. */}
                      <Grid container spacing={2} dir={language === 'ar' ? 'rtl' : 'ltr'}>
                        {/* Next Action */}
                        <Grid item xs={12} sm={6}>
                        <FormControl fullWidth>
                            <InputLabel>{getTranslatedLabel(`${localizationKey}.nextAction`, 'Next Action')}</InputLabel>
                            <Select
                                value={nextAction}
                                label={getTranslatedLabel(`${localizationKey}.nextAction`, 'Next Action')}
                                onChange={(e) => setNextAction(e.target.value)}
                                dir={language === "ar" ? "rtl" : "ltr"}
                                disabled={isActionDisabled}
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
                        </Grid>

                        {/* Cancel Reason - never shown at the same time as the date,
                            so both claim the same half of the row. */}
                        {hasCancelReasonField && (
                            <Grid item xs={12} sm={6}>
                            <FormControl fullWidth>
                                <InputLabel>{getTranslatedLabel(`${localizationKey}.cancelReason`, 'Cancel Reason *')}</InputLabel>
                                <Select
                                    value={cancelReason}
                                    label={getTranslatedLabel(`${localizationKey}.cancelReason`, 'Cancel Reason *')}
                                    onChange={(e) => setCancelReason(e.target.value)}
                                    dir={language === "ar" ? "rtl" : "ltr"}
                                    disabled={isActionDisabled}
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
                            </Grid>
                        )}

                        {/* Action Date */}
                        {hasDateField && (
                            <Grid item xs={12} sm={6}>
                            <LocalizationProvider dateAdapter={AdapterDayjs}>
                                <DateTimePicker
                                    label={getTranslatedLabel(`${localizationKey}.stageDate`, 'Next Action Date')}
                                    value={stageDate ? dayjs(stageDate) : null}
                                    onChange={(newValue) => setStageDate(newValue ? newValue.format('YYYY-MM-DDTHH:mm') : '')}
                                    disablePast
                                    dir={language === "ar" ? "rtl" : "ltr"}
                                    disabled={isActionDisabled}
                                    slotProps={{
                                        textField: {
                                            fullWidth: true,
                                            required: true,
                                        },
                                    }}
                                />
                            </LocalizationProvider>
                            </Grid>
                        )}

                        {isUnitRelated && (
                            <>
                                <Grid item xs={12} sm={6}>
                                    <FormComboBoxVirtualProject
                                        value={selectedProject}
                                        label={getTranslatedLabel("projects.certificate.form.project", "Project")}
                                        dataItemKey="projectId"
                                        textField="ProjectName"
                                        disabled={isActionDisabled}
                                        popupSettings={{ appendTo: document.querySelector(".MuiModal-root") as HTMLElement }}
                                        onChange={(e: any) => {
                                            const newProjectId = e.value?.projectId || null;
                                            const newProjectName = e.value?.projectName || '';
                                            const newProject = newProjectId ? { projectId: newProjectId, projectName: newProjectName } : null;
                                            setSelectedProject(newProject);
                                            setSelectedUnit(null); // clear unit when project changes
                                        }}
                                    />
                                </Grid>

                                <Grid item xs={12} sm={6}>
                                    <FormSimpleComboBoxVirtualApartmentsByProject
                                        value={selectedUnit}
                                        label={getTranslatedLabel(`${localizationKey}.unit`, 'Unit')}
                                        projectId={selectedProject?.projectId || undefined}
                                        disabled={isActionDisabled}
                                        popupSettings={{ appendTo: document.querySelector(".MuiModal-root") as HTMLElement }}
                                        onChange={(e: any) => {
                                            setSelectedUnit({ apartmentId: e.value?.apartmentId, apartmentName: e.value?.apartmentName });
                                        }}
                                    />
                                </Grid>
                            </>
                        )}

                        {hasMeetingDropdownsAndNote && (
                            <>
                                <Grid item xs={12} sm={6}>
                                <FormControl fullWidth>
                                    <InputLabel>{getTranslatedLabel(`${localizationKey}.meetingType`, 'Meeting Type *')}</InputLabel>
                                    <Select
                                        value={meetingType}
                                        label={getTranslatedLabel(`${localizationKey}.meetingType`, 'Meeting Type *')}
                                        onChange={(e) => setMeetingType(e.target.value)}
                                        dir={language === "ar" ? "rtl" : "ltr"}
                                        disabled={isActionDisabled}
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
                                </Grid>

                                <Grid item xs={12} sm={6}>
                                <FormControl fullWidth>
                                    <InputLabel>{getTranslatedLabel(`${localizationKey}.meetingLocation`, 'Meeting Location *')}</InputLabel>
                                    <Select
                                        value={meetingLocation}
                                        label={getTranslatedLabel(`${localizationKey}.meetingLocation`, 'Meeting Location *')}
                                        onChange={(e) => setMeetingLocation(e.target.value)}
                                        dir={language === "ar" ? "rtl" : "ltr"}
                                        disabled={isActionDisabled}
                                    >
                                        {loadingMeetingLocations ? (
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
                                </Grid>

                                {/* Note. minRows/maxRows instead of a fixed 4 rows: it
                                    starts small and grows with what is typed, so the
                                    empty state costs two lines rather than four. */}
                                <Grid item xs={12}>
                                <TextField
                                    fullWidth
                                    label={getTranslatedLabel(`${localizationKey}.note`, 'Note')}
                                    multiline
                                    minRows={2}
                                    maxRows={6}
                                    value={note}
                                    disabled={isActionDisabled}
                                    onChange={(e) => setNote(e.target.value)}
                                    placeholder={getTranslatedLabel(`${localizationKey}.notePlaceholder`, 'Note...')}
                                    dir={language === "ar" ? "rtl" : "ltr"}
                                />
                                </Grid>
                            </>

                        )}



                        {/* Comment */}
                        <Grid item xs={12}>
                        <TextField
                            fullWidth
                            label={getTranslatedLabel(`${localizationKey}.comment`, 'Comment')}
                            multiline
                            minRows={2}
                            maxRows={6}
                            disabled={isActionDisabled}
                            value={comment}
                            onChange={(e) => setComment(e.target.value)}
                            placeholder={getTranslatedLabel(`${localizationKey}.commentPlaceholder`, 'Write your comment here...')}
                        />
                        </Grid>
                      </Grid>

                        {/* Action Buttons */}
                        <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 2 }}>
                            <Button variant="outlined" onClick={onClose}>
                                {getTranslatedLabel(`${localizationKey}.cancel`, 'Cancel')}
                            </Button>
                            <Button
                                variant="contained"
                                onClick={handleSaveAction}
                                disabled={(hasDateField && !stageDate) || (hasCancelReasonField && !cancelReason) || creatingAction || isActionDisabled}
                                loading={creatingAction}
                            >
                                {getTranslatedLabel(`${localizationKey}.saveAction`, 'Save Action')}
                            </Button>
                        </Box>
                    </Box>

                    <Divider />


                    <Box sx={{ p: 3, pb: 1 }}>
                        <Typography variant="subtitle2" gutterBottom sx={{ textAlign: language === 'ar' ? 'end' : 'start' }}>
                            {getTranslatedLabel(`${localizationKey}.actionHistory`, 'Comments')}
                        </Typography>
                    </Box>

                    <Box sx={{
                        flex: 1,
                        overflowY: 'auto',
                        px: 3,
                        pb: 3,
                    }}>
                        {loadingActions ? (
                            <Typography variant="body2" color="text.secondary" sx={{ textAlign: language === 'ar' ? 'end' : 'start' }}>
                                {getTranslatedLabel(`${localizationKey}.loadingActions`, 'Loading actions...')}
                            </Typography>
                        ) : opportunityActions.length === 0 ? (
                            <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic', textAlign: language === 'ar' ? 'end' : 'start' }}>
                                {getTranslatedLabel(`${localizationKey}.noActionsYet`, 'No actions recorded yet.')}
                            </Typography>
                        ) : (
                            <Stack spacing={2}>
                                {opportunityActions.map((action: SalesOpportunityAction) => (
                                    <Card key={action.salesOpportunityActionId} variant="outlined" sx={{ p: 2.5 }} dir={language === 'ar' ? 'rtl' : 'ltr'}>
                                        <Box sx={{ display: 'flex', justifyContent: 'flex-center', alignItems: 'flex-center', gap: 1.5, mb: 1.5 }} >
                                            <Avatar sx={{ width: 34, height: 34, bgcolor: 'primary.main' }}>
                                                <PersonIcon fontSize="small" />
                                            </Avatar>
                                            <Box sx={{ flex: 1 }}>
                                                <Typography variant="subtitle1" fontWeight="medium">
                                                    {action.actionTypeDescription || action.actionTypeId}
                                                </Typography>
                                                {action.meetingTypeId && (
                                                    <Typography variant="subtitle2" fontWeight="medium">
                                                        {action.meetingTypeDescription || action.meetingTypeId}
                                                    </Typography>
                                                )}
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
                                            <Typography variant="body2" sx={{ mb: 1, mr: language === 'ar' ? 2 : 0, gap: 2 }}>
                                                <strong>
                                                    {getTranslatedLabel(`${localizationKey}.nextActionDate`, 'Action Date')}
                                                </strong>{' '}
                                                <p>{dayjs(action.actionDate).format('ddd DD/MM/YYYY - hh:mm A')}</p>
                                            </Typography>
                                        )}

                                        {/* Cancel Reason */}
                                        {action.cancelReasonDescription && (
                                            <Typography variant="body2" color="error" sx={{ mb: 1 }}>
                                                <strong>
                                                    {getTranslatedLabel(`${localizationKey}.cancelReason`, 'Cancel Reason')}
                                                </strong>{' '}
                                                <p>{action.cancelReasonDescription}</p>
                                            </Typography>
                                        )}

                                        {/* Note */}
                                        {action.note && (
                                            <Typography
                                                variant="body2"
                                                sx={{ mt: 1, whiteSpace: 'pre-wrap', lineHeight: 1.6, mr: language === 'ar' ? 2 : 0 }}
                                            >
                                                <strong>
                                                    {getTranslatedLabel(`${localizationKey}.note`, 'Note')}
                                                </strong>{' '}
                                                <p>{action.note}</p>
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

                                        {!action.comment && !action.actionDate && !action.cancelReasonDescription && !action.note && (
                                            <Typography variant="body2" color="text.secondary" fontStyle="italic">
                                                {getTranslatedLabel(`${localizationKey}.noDetails`, 'No additional details')}
                                            </Typography>
                                        )}
                                    </Card>
                                ))}
                            </Stack>
                        )}
                    </Box>
                </TabPanel>

                {/* ===== History Tab ===== */}
                <TabPanel value={activeTab} index={1}>
                    <Box sx={{ p: 3 }}>
                        {loadingHistory ? (
                            <Typography variant="body2" color="text.secondary" sx={{ textAlign: language === 'ar' ? 'end' : 'start' }}>
                                {getTranslatedLabel(`${localizationKey}.loadingHistory`, 'Loading history...')}
                            </Typography>
                        ) : opportunityHistory.length === 0 ? (
                            <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic', textAlign: language === 'ar' ? 'end' : 'start' }}>
                                {getTranslatedLabel(`${localizationKey}.noHistoryYet`, 'No history recorded yet.')}
                            </Typography>
                        ) : (
                            <Stack spacing={2}>
                                {opportunityHistory.map((entry) => (
                                    <Card key={entry.salesOpportunityHistoryId} variant="outlined" sx={{ p: 2.5 }} dir={language === 'ar' ? 'rtl' : 'ltr'}>
                                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1.5 }}>
                                            <Box>
                                                <Typography variant="subtitle1" fontWeight="medium">
                                                    {entry.opportunityStageDescription || entry.opportunityStageId || getTranslatedLabel(`${localizationKey}.stageChange`, 'Stage Update')}
                                                </Typography>
                                                <Typography variant="caption" color="text.secondary">
                                                    {dayjs(entry.createdStamp).format('ddd DD/MM/YYYY - hh:mm A')}
                                                    {!!entry.modifiedByDisplayName && ` · ${entry.modifiedByDisplayName}`}
                                                </Typography>
                                            </Box>
                                        </Box>

                                        {entry.changeNote && (
                                            <Typography variant="body2" sx={{ mt: 1, whiteSpace: 'pre-wrap', lineHeight: 1.6 }}>
                                                <strong>{getTranslatedLabel(`${localizationKey}.changeNote`, 'Change Note')}:</strong> {entry.changeNote}
                                            </Typography>
                                        )}

                                        {entry.description && (
                                            <Typography variant="body2" sx={{ mt: 1, whiteSpace: 'pre-wrap', lineHeight: 1.6 }}>
                                                {entry.description}
                                            </Typography>
                                        )}
                                    </Card>
                                ))}
                            </Stack>
                        )}
                    </Box>
                </TabPanel>
            </Box>

        </Modal>
    );
};

export default AddActionsModal;