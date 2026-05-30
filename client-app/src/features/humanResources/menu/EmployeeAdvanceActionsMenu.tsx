import React, {useState} from "react";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {
    useApproveEmployeeAdvanceMutation,
    useDeleteEmployeeAdvanceMutation
} from "../../../app/store/apis/partiesApi";
import {toast} from "react-toastify";
import Button from "@mui/material/Button";
import {
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Menu,
    MenuItem
} from "@mui/material";
import {Can} from "../../account/Can";
import {EmployeeAdvance} from "../../../app/models/humanResources/employeeAdvance";

interface EmployeeAdvanceActionsMenuProps {
    advanceId: string | undefined;
    currentStatusId: string | undefined;
    disabled: boolean;
    onAdvanceUpdated?: (updated: EmployeeAdvance) => void;
    onAdvanceDeleted?: () => void;
}

export const EmployeeAdvanceActionsMenu: React.FC<EmployeeAdvanceActionsMenuProps> = ({
                                                                             advanceId,
                                                                             currentStatusId,
                                                                             disabled,
                                                                             onAdvanceUpdated, onAdvanceDeleted
                                                                         }) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const [approveAdvance, {isLoading}] = useApproveEmployeeAdvanceMutation();
    const [deleteAdvance, { isLoading: isDeleting }] = useDeleteEmployeeAdvanceMutation();
    const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);

    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    
    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const handleApprove = async () => {
        if (!advanceId) return;

        try {
            const updatedAdvance = await approveAdvance(advanceId).unwrap();
            toast.success(getTranslatedLabel("party.employeeAdvance.approved", "Employee Advance Approved"));
            onAdvanceUpdated?.(updatedAdvance);
        } catch (error) {
            toast.error(getTranslatedLabel("party.employeeAdvance.approveError", "Failed to approve employee advance"));
        } finally {
            handleClose();
        }
    };

    const handleDeleteClick = () => {
        setConfirmDeleteOpen(true);
        handleClose();
    };

    const handleDeleteConfirm = async () => {
        if (!advanceId) return;

        try {
            await deleteAdvance(advanceId).unwrap();
            toast.success(getTranslatedLabel("party.employeeAdvance.deleted", "Employee Advance Deleted"));
            onAdvanceDeleted?.();
        } catch (error) {
            toast.error(getTranslatedLabel("party.employeeAdvance.deleteError", "Failed to delete employee advance"));
        } finally {
            setConfirmDeleteOpen(false);
        }
    };

    const isApproveDisabled = !advanceId || currentStatusId !== "ADVANCE_REQUESTED";
    const isDeleteDisabled = !advanceId || currentStatusId === "ADVANCE_FULLY_PAID" || currentStatusId === "ADVANCE_PARTIALLY_PAID";

    return (
        <>
            <Button
                variant="contained"
                color="primary"
                onClick={handleClick}
                disabled={disabled || isLoading || !advanceId}
                sx={{mt: 2, mr: 2}}
            >
                {getTranslatedLabel('party.employeeAdvance.form.actions', 'Actions')}
            </Button>

            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
                anchorOrigin={{vertical: 'bottom', horizontal: 'right'}}
                transformOrigin={{vertical: 'top', horizontal: 'right'}}
            >
                <Can perform="ApproveEmployeeAdvance">
                    <MenuItem onClick={handleApprove} disabled={isApproveDisabled || isLoading}>
                        {getTranslatedLabel('party.employeeAdvance.form.approve', 'Approve Employee Advance')}
                    </MenuItem>
                </Can>

                <Can perform="DeleteEmployeeAdvance">
                    <MenuItem
                        onClick={handleDeleteClick}
                        disabled={isDeleting || isDeleteDisabled}
                        sx={{ color: "error.main" }}
                    >
                        {getTranslatedLabel('party.employeeAdvance.form.delete', 'Delete Employee Advance')}
                    </MenuItem>
                </Can>
            </Menu>

            <Dialog
                open={confirmDeleteOpen}
                onClose={() => setConfirmDeleteOpen(false)}
                maxWidth="sm"
                fullWidth
            >
                <DialogTitle sx={{ color: "error.main" }}>
                    {getTranslatedLabel('party.employeeAdvance.form.deleteConfirmTitle', 'Confirm Delete')}
                </DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        {getTranslatedLabel('party.employeeAdvance.form.deleteConfirmMessage',
                            'Are you sure you want to permanently delete this Employee Advance? ' +
                            'This action cannot be undone.'
                        )}
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setConfirmDeleteOpen(false)} disabled={isDeleting}>
                        {getTranslatedLabel('general.cancel', 'Cancel')}
                    </Button>
                    <Button
                        onClick={handleDeleteConfirm}
                        variant="contained"
                        color="error"
                        disabled={isDeleting}
                        startIcon={isDeleting ? <CircularProgress size={16} /> : null}
                    >
                        {isDeleting
                            ? getTranslatedLabel('party.employeeAdvance.form.deleting', 'Deleting...')
                            : getTranslatedLabel('party.employeeAdvance.form.delete', 'Delete')
                        }
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};
