import React, { useState } from 'react';
import { Button, Menu, MenuItem, Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions, TextField } from '@mui/material';
import { Payment } from "../../../../app/models/accounting/payment";
import {Can} from "../../../account/Can";

const LOCALIZATION_KEY = "accounting.payments.form";

interface PaymentActionsProps {
    payment?: Payment;
    formEditMode: number;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    handleMenuSelect: (e: { item: { data: string } }) => void;
    handleReset: () => void;
    handleVoid?: (reason: string) => void;
    /** True when the payment has accounting entries (posted or reversed). */
    hasLedgerHistory?: boolean;
    getAvailableStatusTransitions?: (payment?: Payment) => any;
    isProcessing?: boolean;
}


const PaymentActions: React.FC<PaymentActionsProps> = ({
                                                           payment,
                                                           formEditMode,
                                                           getTranslatedLabel,
                                                           handleMenuSelect,
                                                           handleReset,
                                                           handleVoid,
                                                           hasLedgerHistory = false,
                                                           getAvailableStatusTransitions: getAvailableStatusTransitionsFromProp,
                                                           isProcessing = false
                                                       }) => {
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const [resetDialogOpen, setResetDialogOpen] = useState(false);
    const [voidDialogOpen, setVoidDialogOpen] = useState(false);
    const [voidReason, setVoidReason] = useState('');

    // use prop transitions if available, fallback to internal one
    const getTransitions = (p?: Payment) => {
        if (getAvailableStatusTransitionsFromProp) {
            return getAvailableStatusTransitionsFromProp(p);
        }
        // internal fallback
        if (!p) {
            return { toSent: false, toReceived: false, toCancelled: false, toConfirmed: false, toVoid: false };
        }
        const isOutgoing = p.isDisbursement;
        return {
            toSent: p.statusId === 'PMNT_NOT_PAID' && isOutgoing,
            toReceived: p.statusId === 'PMNT_NOT_PAID' && !isOutgoing,
            toCancelled: p.statusId === 'PMNT_NOT_PAID',
            toConfirmed: p.statusId === 'PMNT_SENT' || p.statusId === 'PMNT_RECEIVED',
            toVoid: p.statusId !== 'PMNT_CONFIRMED' && p.statusId !== 'PMNT_VOID',
        };
    };

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
    const handleVoidClick = () => {
        setVoidReason('');
        setVoidDialogOpen(true);
        handleClose();
    };
    const handleConfirmVoid = () => {
        if (!voidReason.trim()) return;
        handleVoid?.(voidReason.trim());
        setVoidDialogOpen(false);
    };
    // Void replaces "delete" for a payment that has reached the ledger: any sent or received
    // payment, and a draft only when it carries ledger entries (it was reset earlier, so it cannot
    // be deleted). A clean draft is deleted from the list instead. A confirmed payment is settled
    // with the bank and cannot be voided; an already-void one has nothing left to do.
    const reachedLedger = ['PMNT_RECEIVED', 'PMNT_SENT'].includes(payment?.statusId ?? '') ||
        (payment?.statusId === 'PMNT_NOT_PAID' && hasLedgerHistory);
    const canVoid = payment && reachedLedger && formEditMode !== 1 && !!handleVoid;
    const isVoided = payment?.statusId === 'PMNT_VOID';

    // Applications exist only while the payment is applied to something: not for a draft, and
    // not after a void (they are removed). Transactions may exist for any saved payment, including
    // a draft that was reset (original + reversal), so the view is always offered.
    const canViewApplications = payment?.statusId !== 'PMNT_NOT_PAID' && !isVoided;
    const canViewTransactions = !!payment?.paymentId && (payment?.statusId !== 'PMNT_NOT_PAID' || hasLedgerHistory);

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
                    disabled={!payment || isProcessing} 
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
                disabled={!payment || isProcessing} 
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
                        {getTransitions(payment).toSent && (
                            <MenuItem onClick={() => onMenuSelect('send')}>
                                {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.send`, "Status to Sent")}
                            </MenuItem>
                        )}

                        {getTransitions(payment).toReceived  && (
                            <MenuItem onClick={() => onMenuSelect('receive')}>
                                {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.receive`, "Status to Received")}
                            </MenuItem>
                        )}
                    </>
                </Can>

                {payment?.paymentId && !isVoided && (
                    <Can perform={["Process_Payment", "Duplicate_Payment"]}>
                        <MenuItem onClick={() => onMenuSelect('duplicate')}>
                            {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.duplicate`, "Duplicate Payment")}
                        </MenuItem>
                    </Can>
                )}

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
                {canVoid && (
                    <Can perform="Reset_Payment">
                        <MenuItem
                            onClick={handleVoidClick}
                            sx={{ color: '#d32f2f' }}
                        >
                            {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.void`, "إلغاء الدفعة (Void)")}
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

                {/* Field-level change history for this payment, from ENTITY_AUDIT_LOG.
                    Gated on Admin to match the Audit Trail screen; loosen if accountants need it. */}
                {payment?.paymentId && (
                    <Can perform="Admin">
                        <MenuItem onClick={() => onMenuSelect('history')}>
                            {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.history`, "سجل التغييرات")}
                        </MenuItem>
                    </Can>
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
                            "هل أنت متأكد من إعادة تعيين الدفعة رقم {paymentId}؟\n\nسيؤدي ذلك إلى:\n• إرجاع الحالة إلى \"غير مدفوع\"\n• فك ربط الدفعة بالفواتير\n• عكس القيود المحاسبية المرحّلة بقيود عكسية مرتبطة (يبقى الأصل والعكس في الدفتر)\n\nيمكنك بعدها تعديل الدفعة وإرسالها من جديد."
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
            <Dialog
                open={voidDialogOpen}
                onClose={() => setVoidDialogOpen(false)}
                aria-labelledby="void-payment-dialog-title"
                fullWidth
                maxWidth="sm"
            >
                <DialogTitle id="void-payment-dialog-title">
                    {getTranslatedLabel(`${LOCALIZATION_KEY}.void.dialogTitle`, "إلغاء الدفعة")}
                </DialogTitle>
                <DialogContent>
                    <DialogContentText sx={{ whiteSpace: 'pre-line', mb: 2 }}>
                        {getTranslatedLabel(
                            `${LOCALIZATION_KEY}.void.dialogMessage`,
                            "سيتم إلغاء الدفعة رقم {paymentId} مع الاحتفاظ بها في السجل بحالة \"ملغاة\".\n\n• تُعكس قيودها المحاسبية بقيود عكسية مرتبطة\n• تُلغى حركتها البنكية\n• يُفك ربطها بالفواتير\n\nلا يُحذف أي سجل. اذكر سبب الإلغاء."
                        ).replace("{paymentId}", payment?.paymentId || "")}
                    </DialogContentText>
                    <TextField
                        id="void-payment-reason"
                        autoFocus
                        fullWidth
                        multiline
                        minRows={2}
                        label={getTranslatedLabel(`${LOCALIZATION_KEY}.void.reason`, "سبب الإلغاء")}
                        value={voidReason}
                        onChange={(e) => setVoidReason(e.target.value)}
                        inputProps={{ maxLength: 500 }}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setVoidDialogOpen(false)}>
                        {getTranslatedLabel("global.cancel", "Cancel")}
                    </Button>
                    <Button
                        onClick={handleConfirmVoid}
                        color="error"
                        variant="contained"
                        disabled={!voidReason.trim() || isProcessing}
                    >
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.void.confirm`, "إلغاء الدفعة")}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};

export default PaymentActions;