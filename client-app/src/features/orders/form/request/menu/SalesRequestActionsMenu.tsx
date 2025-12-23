import {SalesRequest} from "../../../../../app/models/order/SalesRequest";
import React, {useState} from "react";
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";
import {
    useApproveSalesRequestMutation,
    useDeleteSalesRequestMutation
} from "../../../../../app/store/apis/salesRequestApi";
import {useNavigate} from "react-router";
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
import {Can} from "../../../../account/Can";

interface SalesRequestActionsMenuProps {
    salesRequestId: string | undefined;
    currentStatusId: string | undefined;
    disabled: boolean;
    onSalesRequestUpdated?: (updated: SalesRequest) => void;
    onSalesRequestDeleted?: () => void;
}

export const SalesRequestActionsMenu: React.FC<SalesRequestActionsMenuProps> = ({
                                                                             salesRequestId,
                                                                             currentStatusId,
                                                                             disabled,
                                                                             onSalesRequestUpdated, onSalesRequestDeleted
                                                                         }) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const [approveSR, {isLoading}] = useApproveSalesRequestMutation();
    const [deleteSR, { isLoading: isDeleting }] = useDeleteSalesRequestMutation();  // ← new mutation
    const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);  // ← NEW: Confirmation state

    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const navigate = useNavigate();
    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const handleApprove = async () => {
        if (!salesRequestId) return;

        try {
            // This returns the full updated object from backend
            const updatedSalesRequest = await approveSR(salesRequestId).unwrap();

            toast.success(getTranslatedLabel("salesRequest.approved", "Sales Request Approved"));

            // THIS IS THE KEY: Notify parent so ribbon updates instantly
            onSalesRequestUpdated?.(updatedSalesRequest);
        } catch (error) {
            toast.error(getTranslatedLabel("salesRequest.approveError", "Failed to approve sales request"));
        } finally {
            handleClose();
        }
    };

    const handleDeleteClick = () => {
        setConfirmDeleteOpen(true);
        handleClose();  // Close the menu
    };

    const handleDeleteConfirm = async () => {
        if (!salesRequestId) return;

        try {
            await deleteSR(salesRequestId).unwrap();
            toast.success(getTranslatedLabel("salesRequest.deleted", "Sales Request Deleted"));
            onSalesRequestDeleted?.();
        } catch (error) {
            toast.error(getTranslatedLabel("salesRequest.deleteError", "Failed to delete sales request"));
        } finally {
            setConfirmDeleteOpen(false);  // Always close dialog
        }
    };

    const isApproveDisabled = !salesRequestId || currentStatusId === "SALES_REQUEST_APPROVED";

    return (
        <>
            <Button
                variant="contained"
                color="primary"
                onClick={handleClick}
                disabled={disabled || isLoading || !salesRequestId}
                sx={{mt: 2, mr: 2}}
            >
                {getTranslatedLabel('salesRequest.form.actions', 'Actions')}
            </Button>

            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
                anchorOrigin={{vertical: 'bottom', horizontal: 'right'}}
                transformOrigin={{vertical: 'top', horizontal: 'right'}}
            >
                <MenuItem onClick={handleApprove} disabled={isApproveDisabled || isLoading}>
                    {getTranslatedLabel('salesRequest.form.approve', 'Approve Sales Request')}
                </MenuItem>

                <Can perform="DeleteSalesRequest">
                    <MenuItem
                        onClick={handleDeleteClick}
                        disabled={isDeleting}
                        sx={{ color: "error.main" }} // visual cue that it's destructive
                    >
                        {getTranslatedLabel('salesRequest.form.delete', 'Delete Sales Request')}
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
                    {getTranslatedLabel('salesRequest.form.deleteConfirmTitle', 'Confirm Delete')}
                </DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        {getTranslatedLabel('salesRequest.form.deleteConfirmMessage',
                            'Are you sure you want to permanently delete this Sales Request? ' +
                            'This action will also remove all related payments and accounting entries. ' +
                            'This cannot be undone.'
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
                            ? getTranslatedLabel('salesRequest.form.deleting', 'Deleting...')
                            : getTranslatedLabel('salesRequest.form.delete', 'Delete')
                        }
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};