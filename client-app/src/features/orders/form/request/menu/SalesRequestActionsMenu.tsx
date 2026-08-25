import {SalesRequest} from "../../../../../app/models/order/SalesRequest";
import React, {useState} from "react";
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";
import {
    useApproveSalesRequestMutation,
    useDeleteSalesRequestMutation,
    useResetSalesRequestMutation
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
import {RecordChequesDialog} from "./RecordChequesDialog";

interface SalesRequestActionsMenuProps {
    salesRequestId: string | undefined;
    currentStatusId: string | undefined;
    isChequesDelivered?: boolean | string | null;
    disabled: boolean;
    onSalesRequestUpdated?: (updated: SalesRequest) => void;
    onSalesRequestDeleted?: () => void;
    fromPartyName?: string | null;
    apartmentName?: string | null;
}

export const SalesRequestActionsMenu: React.FC<SalesRequestActionsMenuProps> = ({
                                                                             salesRequestId,
                                                                             currentStatusId,
                                                                             isChequesDelivered,
                                                                             disabled,
                                                                             onSalesRequestUpdated, onSalesRequestDeleted,
                                                                             fromPartyName, apartmentName
                                                                         }) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const [approveSR, {isLoading}] = useApproveSalesRequestMutation();
    const [deleteSR, { isLoading: isDeleting }] = useDeleteSalesRequestMutation();  // ← new mutation
    const [resetSR, { isLoading: isResetting }] = useResetSalesRequestMutation();
    const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);  // ← NEW: Confirmation state
    const [confirmResetOpen, setConfirmResetOpen] = useState(false);
    const [recordChequesOpen, setRecordChequesOpen] = useState(false);

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
        } catch (err: any) {
            // Show what the server actually said (FK violations, business-rule blocks) instead of
            // swallowing every failure behind the same generic sentence.
            toast.error(err?.data?.title ?? getTranslatedLabel("salesRequest.deleteError", "Failed to delete sales request"));
        } finally {
            setConfirmDeleteOpen(false);  // Always close dialog
        }
    };

    const handleResetClick = () => {
        setConfirmResetOpen(true);
        handleClose();
    };

    const handleResetConfirm = async () => {
        if (!salesRequestId) return;

        try {
            const updatedSalesRequest = await resetSR(salesRequestId).unwrap();
            toast.success(getTranslatedLabel("salesRequest.form.resetSuccess", "Sales Request Reset Successfully"));
            onSalesRequestUpdated?.(updatedSalesRequest as unknown as SalesRequest);
        } catch (err: any) {
            toast.error(err?.data?.title ?? getTranslatedLabel("salesRequest.form.resetError", "Failed to reset sales request"));
        } finally {
            setConfirmResetOpen(false);
        }
    };

    const handleRecordChequesClick = () => {
        setRecordChequesOpen(true);
        handleClose();
    };

    const isApproveDisabled = !salesRequestId || currentStatusId === "SALES_REQUEST_APPROVED";
    const isResetDisabled = !salesRequestId || currentStatusId !== "SALES_REQUEST_APPROVED";
    // Only meaningful when the sales request was declared upfront to involve cheques - if
    // "Cheques Delivered" was left unchecked, the customer is expected to pay every installment
    // directly (cash/bank transfer), so recording a cheque here shouldn't be possible at all.
    const isRecordChequesDisabled =
        !salesRequestId || currentStatusId !== "SALES_REQUEST_APPROVED" || isChequesDelivered !== true;

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

                <MenuItem onClick={handleRecordChequesClick} disabled={isRecordChequesDisabled}>
                    {getTranslatedLabel('salesRequest.cheques.action', 'Record Received Cheques')}
                </MenuItem>

                <Can perform="ResetSalesRequest">
                    <MenuItem
                        onClick={handleResetClick}
                        disabled={isResetDisabled || isResetting}
                    >
                        {getTranslatedLabel('salesRequest.form.reset', 'Reset Sales Request')}
                    </MenuItem>
                </Can>

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
                            'This action will also remove all related payments and accounting entries, ' +
                            'including the sales commission record for this request and every commission ' +
                            'payment it generated — even ones already disbursed. This cannot be undone.'
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

            <Dialog
                open={confirmResetOpen}
                onClose={() => setConfirmResetOpen(false)}
                maxWidth="sm"
                fullWidth
            >
                <DialogTitle>
                    {getTranslatedLabel('salesRequest.form.resetConfirmTitle', 'Confirm Reset')}
                </DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        {getTranslatedLabel('salesRequest.form.resetConfirmMessage',
                            'Are you sure you want to reset this Sales Request? ' +
                            'This will delete all customer payments and accounting entries generated during approval, ' +
                            'and set the status back to Created. ' +
                            'If a sales commission has already been approved for this request, the reset is refused — ' +
                            'reset or delete that commission first.'
                        )}
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setConfirmResetOpen(false)} disabled={isResetting}>
                        {getTranslatedLabel('general.cancel', 'Cancel')}
                    </Button>
                    <Button
                        onClick={handleResetConfirm}
                        variant="contained"
                        color="primary"
                        disabled={isResetting}
                        startIcon={isResetting ? <CircularProgress size={16} /> : null}
                    >
                        {isResetting
                            ? getTranslatedLabel('salesRequest.form.resetting', 'Resetting...')
                            : getTranslatedLabel('salesRequest.form.reset', 'Reset')
                        }
                    </Button>
                </DialogActions>
            </Dialog>

            {salesRequestId && (
                <RecordChequesDialog
                    open={recordChequesOpen}
                    onClose={() => setRecordChequesOpen(false)}
                    salesRequestId={salesRequestId}
                    fromPartyName={fromPartyName}
                    apartmentName={apartmentName}
                />
            )}
        </>
    );
};