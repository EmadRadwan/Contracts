import React, { useState } from "react";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import {
    useApproveSalesCommissionMutation,
    useResetSalesCommissionMutation,
    useDeleteSalesCommissionMutation,
} from "../../../../../app/store/apis/salesCommissionsApi";
import { toast } from "react-toastify";
import Button from "@mui/material/Button";
import {
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Menu,
    MenuItem,
} from "@mui/material";

interface SalesCommissionActionsMenuProps {
    salesCommissionId: string | undefined;
    currentStatusId: string | undefined;
    disabled: boolean;
    // Party slots left unassigned on the saved record — they generate no payment on approval, so the
    // approve dialog names them. Recovering from a premature approval means a reset, which purges
    // every commission payment for the sales request, including ones already disbursed.
    unassignedParties?: string[];
    onCommissionApproved?: () => void;
    onCommissionReset?: () => void;
    onCommissionDeleted?: () => void;
}

export const SalesCommissionActionsMenu: React.FC<SalesCommissionActionsMenuProps> = ({
    salesCommissionId,
    currentStatusId,
    disabled,
    unassignedParties = [],
    onCommissionApproved,
    onCommissionReset,
    onCommissionDeleted,
}) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [approveCommission, { isLoading: isApproving }] = useApproveSalesCommissionMutation();
    const [resetCommission, { isLoading: isResetting }] = useResetSalesCommissionMutation();
    const [deleteCommission, { isLoading: isDeleting }] = useDeleteSalesCommissionMutation();

    const [confirmApproveOpen, setConfirmApproveOpen] = useState(false);
    const [confirmResetOpen, setConfirmResetOpen] = useState(false);
    const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const handleApproveClick = () => {
        setConfirmApproveOpen(true);
        handleClose();
    };

    const handleApproveConfirm = async () => {
        if (!salesCommissionId) return;
        try {
            await approveCommission(salesCommissionId).unwrap();
            toast.success(getTranslatedLabel("salesCommission.form.approveSuccess", "تم اعتماد العمولة بنجاح"));
            onCommissionApproved?.();
        } catch (err: any) {
            toast.error(err?.data?.title ?? getTranslatedLabel("salesCommission.form.approveError", "فشل في اعتماد العمولة"));
        } finally {
            setConfirmApproveOpen(false);
        }
    };

    const handleResetClick = () => {
        setConfirmResetOpen(true);
        handleClose();
    };

    const handleResetConfirm = async () => {
        if (!salesCommissionId) return;
        try {
            await resetCommission(salesCommissionId).unwrap();
            toast.success(getTranslatedLabel("salesCommission.form.resetSuccess", "تم إعادة تعيين العمولة بنجاح"));
            onCommissionReset?.();
        } catch (err: any) {
            toast.error(err?.data?.title ?? getTranslatedLabel("salesCommission.form.resetError", "فشل في إعادة تعيين العمولة"));
        } finally {
            setConfirmResetOpen(false);
        }
    };

    const handleDeleteClick = () => {
        setConfirmDeleteOpen(true);
        handleClose();
    };

    const handleDeleteConfirm = async () => {
        if (!salesCommissionId) return;
        try {
            await deleteCommission(salesCommissionId).unwrap();
            toast.success(getTranslatedLabel("salesCommission.form.deleteSuccess", "تم حذف العمولة بنجاح"));
            onCommissionDeleted?.();
        } catch (err: any) {
            toast.error(err?.data?.title ?? getTranslatedLabel("salesCommission.form.deleteError", "فشل في حذف العمولة"));
        } finally {
            setConfirmDeleteOpen(false);
        }
    };

    const isApproveDisabled = !salesCommissionId || currentStatusId !== "COMMISSION_PENDING";
    const isResetDisabled = !salesCommissionId || currentStatusId !== "COMMISSION_APPROVED";

    return (
        <>
            <Button
                variant="contained"
                color="primary"
                onClick={handleClick}
                disabled={disabled || !salesCommissionId}
                sx={{ ml: 2 }}
            >
                {getTranslatedLabel("salesCommission.form.actions", "Actions")}
            </Button>

            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
                anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
                transformOrigin={{ vertical: "top", horizontal: "right" }}
            >
                <MenuItem onClick={handleApproveClick} disabled={isApproveDisabled || isApproving}>
                    {getTranslatedLabel("salesCommission.form.approve", "اعتماد")}
                </MenuItem>

                <MenuItem onClick={handleResetClick} disabled={isResetDisabled || isResetting}>
                    {getTranslatedLabel("salesCommission.form.reset", "إعادة تعيين")}
                </MenuItem>

                <MenuItem
                    onClick={handleDeleteClick}
                    disabled={!salesCommissionId || isDeleting}
                    sx={{ color: "error.main" }}
                >
                    {getTranslatedLabel("salesCommission.form.delete", "حذف")}
                </MenuItem>
            </Menu>

            {/* Approve Confirmation */}
            <Dialog open={confirmApproveOpen} onClose={() => setConfirmApproveOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel("salesCommission.form.approveConfirmTitle", "تأكيد الاعتماد")}
                </DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        {getTranslatedLabel(
                            "salesCommission.form.approveConfirmMessage",
                            "هل أنت متأكد من اعتماد هذه العمولة؟ سيتم إنشاء مدفوعات العمولة للأطراف المعنية."
                        )}
                    </DialogContentText>
                    {unassignedParties.length > 0 && (
                        <DialogContentText sx={{ mt: 2, color: "warning.main" }}>
                            {getTranslatedLabel(
                                "salesCommission.form.approveUnassignedWarning",
                                "الأطراف التالية غير محددة ولن يتم إنشاء دفعات لها"
                            )}
                            {`: ${unassignedParties.join("، ")}. `}
                            {getTranslatedLabel(
                                "salesCommission.form.approveUnassignedHint",
                                "لإضافتها بعد الاعتماد يلزم إعادة تعيين العمولة، وهو ما يحذف جميع مدفوعاتها نهائياً."
                            )}
                        </DialogContentText>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setConfirmApproveOpen(false)} disabled={isApproving}>
                        {getTranslatedLabel("general.cancel", "Cancel")}
                    </Button>
                    <Button
                        onClick={handleApproveConfirm}
                        variant="contained"
                        color="primary"
                        disabled={isApproving}
                        startIcon={isApproving ? <CircularProgress size={16} /> : null}
                    >
                        {isApproving
                            ? getTranslatedLabel("salesCommission.form.approving", "جاري الاعتماد...")
                            : getTranslatedLabel("salesCommission.form.approve", "اعتماد")}
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Reset Confirmation */}
            <Dialog open={confirmResetOpen} onClose={() => setConfirmResetOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel("salesCommission.form.resetConfirmTitle", "تأكيد إعادة التعيين")}
                </DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        {getTranslatedLabel(
                            "salesCommission.form.resetConfirmMessage",
                            "هل أنت متأكد من إعادة تعيين هذه العمولة؟ سيتم حذف جميع مدفوعات العمولة — بما فيها المدفوعات التي تم صرفها بالفعل — وجميع القيود المحاسبية والحركات البنكية المرتبطة بها نهائياً، وسيعود الحالة إلى قيد الانتظار. لا يمكن التراجع عن هذا الإجراء."
                        )}
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setConfirmResetOpen(false)} disabled={isResetting}>
                        {getTranslatedLabel("general.cancel", "Cancel")}
                    </Button>
                    <Button
                        onClick={handleResetConfirm}
                        variant="contained"
                        color="primary"
                        disabled={isResetting}
                        startIcon={isResetting ? <CircularProgress size={16} /> : null}
                    >
                        {isResetting
                            ? getTranslatedLabel("salesCommission.form.resetting", "جاري إعادة التعيين...")
                            : getTranslatedLabel("salesCommission.form.reset", "إعادة تعيين")}
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Delete Confirmation */}
            <Dialog open={confirmDeleteOpen} onClose={() => setConfirmDeleteOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle sx={{ color: "error.main" }}>
                    {getTranslatedLabel("salesCommission.form.deleteConfirmTitle", "تأكيد الحذف")}
                </DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        {getTranslatedLabel(
                            "salesCommission.form.deleteConfirmMessage",
                            "هل أنت متأكد من حذف هذه العمولة نهائياً؟ إذا كانت معتمدة، سيتم حذف جميع المدفوعات — بما فيها التي تم صرفها بالفعل — والقيود المحاسبية والحركات البنكية المرتبطة بها. لا يمكن التراجع عن هذا الإجراء."
                        )}
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setConfirmDeleteOpen(false)} disabled={isDeleting}>
                        {getTranslatedLabel("general.cancel", "Cancel")}
                    </Button>
                    <Button
                        onClick={handleDeleteConfirm}
                        variant="contained"
                        color="error"
                        disabled={isDeleting}
                        startIcon={isDeleting ? <CircularProgress size={16} /> : null}
                    >
                        {isDeleting
                            ? getTranslatedLabel("salesCommission.form.deleting", "جاري الحذف...")
                            : getTranslatedLabel("salesCommission.form.delete", "حذف")}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};
