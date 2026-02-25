import React, { useState } from 'react';
import { Button, Menu, MenuItem, Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions } from '@mui/material';
import { Payment } from "../../../../app/models/accounting/payment";
import {Can} from "../../../account/Can";

const LOCALIZATION_KEY = "accounting.payments.form";

interface PaymentActionsProps {
    payment?: Payment;
    formEditMode: number;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    handleMenuSelect: (e: { item: { data: string } }) => void;
    handleReset: () => void;
}

const getAvailableStatusTransitions = (payment?: Payment) => {
    if (!payment) {
        return { toSent: false, toReceived: false, toCancelled: false, toConfirmed: false, toVoid: false };
    }
    const isOutgoing = payment.isDisbursement; 
    return {
        toSent: payment.statusId === 'PMNT_NOT_PAID' && isOutgoing,
        toReceived: payment.statusId === 'PMNT_NOT_PAID' && !isOutgoing,
        toCancelled: payment.statusId === 'PMNT_NOT_PAID',
        toConfirmed: payment.statusId === 'PMNT_SENT' || payment.statusId === 'PMNT_RECEIVED',
        toVoid: payment.statusId !== 'PMNT_CONFIRMED' && payment.statusId !== 'PMNT_VOID',
    };
};

const PaymentActions: React.FC<PaymentActionsProps> = ({
                                                           payment,
                                                           formEditMode,
                                                           getTranslatedLabel,
                                                           handleMenuSelect,
                                                           handleReset,
                                                       }) => {
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const [resetDialogOpen, setResetDialogOpen] = useState(false);

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const onMenuSelect = (action: string) => {
        if (action === 'cancel' && !window.confirm(`Cancel payment ${payment?.paymentId}?`)) {
            return;
        }
        if (action === 'void' && !window.confirm(`Void payment ${payment?.paymentId}?`)) {
            return;
        }
        handleMenuSelect({ item: { data: action } });
        handleClose();
    };

    const handleResetClick = () => {
        setResetDialogOpen(true);
        handleClose(); // close menu
    };

    const handleConfirmReset = () => {
        handleReset();
        setResetDialogOpen(false);
    };

    const handleCancelReset = () => {
        setResetDialogOpen(false);
    };

    const canViewApplications = payment?.statusId !== 'PMNT_NOT_PAID';
    const canViewTransactions = payment?.statusId !== 'PMNT_NOT_PAID';

    const canReset = payment &&
        ['PMNT_RECEIVED', 'PMNT_SENT'].includes(payment.statusId ?? '') &&
        formEditMode !== 1; // not in create mode


    // In create mode (formEditMode === 1), only show create options
    if (formEditMode === 1) {
        return (
            <>
                <Button
                    variant="contained"
                    color="primary"
                    onClick={handleClick}
                    sx={{ mt: 2, mr: 2 }}
                    disabled={!payment} 
                >
                    {getTranslatedLabel("general.actions", "Actions")}
                </Button>
                <Menu
                    anchorEl={anchorEl}
                    open={open}
                    onClose={handleClose}
                    anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                    transformOrigin={{ vertical: 'top', horizontal: 'right' }}
                >
                    <MenuItem onClick={() => onMenuSelect('incoming')}>
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.new-incoming`, "New Incoming Payment")}
                    </MenuItem>
                    <MenuItem onClick={() => onMenuSelect('outgoing')}>
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.new-outgoing`, "New Outgoing Payment")}
                    </MenuItem>
                </Menu>
            </>
        );
    }
    

    // For other modes (edit mode), show all actions
    return (
        <>
            <Button
                variant="contained"
                color="primary"
                onClick={handleClick}
                sx={{ mt: 2, mr: 2 }}
                disabled={!payment} 
            >
                {getTranslatedLabel("general.actions", "Actions")}
            </Button>
            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
                anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                transformOrigin={{ vertical: 'top', horizontal: 'right' }}
            >

                <Can
                    perform="Process_Payment"
                >
                    <>
                        {getAvailableStatusTransitions(payment).toSent && (
                            <MenuItem onClick={() => onMenuSelect('send')}>
                                {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.send`, "Status to Sent")}
                            </MenuItem>
                        )}

                        {getAvailableStatusTransitions(payment).toReceived  && (
                            <MenuItem onClick={() => onMenuSelect('receive')}>
                                {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.receive`, "Status to Received")}
                            </MenuItem>
                        )}

                        {!!payment?.paymentId && (
                            <MenuItem onClick={() => onMenuSelect('duplicate')}>
                                {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.duplicate`, "Duplicate Payment")}
                            </MenuItem>
                        )}
                    </>
                </Can>

                {canReset && (
                    <Can perform="Reset_Payment"> {/* optional - add permission if needed */}
                        <MenuItem
                            onClick={handleResetClick}
                            sx={{ color: '#d32f2f' }}
                        >
                            {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.reset`, "إعادة تعيين الدفعة")}
                        </MenuItem>
                    </Can>
                )}
                
                {/*{getAvailableStatusTransitions(payment).toCancelled && (
                    <MenuItem onClick={() => onMenuSelect('cancel')}>
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.cancel`, "Status to Cancelled")}
                    </MenuItem>
                )}
                {getAvailableStatusTransitions(payment).toConfirmed && (
                    <MenuItem onClick={() => onMenuSelect('confirm')}>
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.confirm`, "Status to Confirmed")}
                    </MenuItem>
                )}
                {getAvailableStatusTransitions(payment).toVoid && (
                    <MenuItem onClick={() => onMenuSelect('void')}>
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.void`, "Status to Void")}
                    </MenuItem>
                )}
                <MenuItem onClick={() => onMenuSelect('incoming')}>
                    {getTranslatedLabel(`${LOCALIZATION_KEY}.new-incoming`, "New Incoming Payment")}
                </MenuItem>
                <MenuItem onClick={() => onMenuSelect('outgoing')}>
                    {getTranslatedLabel(`${LOCALIZATION_KEY}.new-outgoing`, "New Outgoing Payment")}
                </MenuItem>*/}
                {canViewTransactions && <MenuItem onClick={() => onMenuSelect('transactions')}>
                    {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.transactions`, "Transactions")}
                </MenuItem>}
                {canViewApplications && (
                    <MenuItem onClick={() => onMenuSelect('applications')}>
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.applications`, "Payment Applications")}
                    </MenuItem>
                )}
            </Menu>

            <Dialog
                open={resetDialogOpen}
                onClose={handleCancelReset}
                aria-labelledby="reset-payment-dialog-title"
                aria-describedby="reset-payment-dialog-description"
            >
                <DialogTitle id="reset-payment-dialog-title">
                    {getTranslatedLabel(
                        `${LOCALIZATION_KEY}.reset.dialogTitle`,
                        "تأكيد إعادة تعيين الدفعة"
                    )}
                </DialogTitle>
                <DialogContent>
                    <DialogContentText id="reset-payment-dialog-description">
                        {getTranslatedLabel(
                            `${LOCALIZATION_KEY}.reset.dialogMessage`,
                            "هل أنت متأكد من إعادة تعيين الدفعة رقم {paymentId}؟\n\nسيؤدي ذلك إلى:\n• إرجاع الحالة إلى \"غير مدفوع\"\n• حذف جميع تطبيقات الدفعة\n• حذف الحركات المحاسبية المرتبطة\n\nهذا الإجراء لا يمكن التراجع عنه."
                        ).replace("{paymentId}", payment?.paymentId || "")}
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button
                        onClick={handleCancelReset}
                        disabled={false} // can add isResetting if you pass loading state
                    >
                        {getTranslatedLabel("global.cancel", "Cancel")}
                    </Button>
                    <Button
                        onClick={handleConfirmReset}
                        color="error"
                        variant="contained"
                        autoFocus
                    >
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.reset.resetConfirm`, "إعادة تعيين الدفعة")}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};

export default PaymentActions;