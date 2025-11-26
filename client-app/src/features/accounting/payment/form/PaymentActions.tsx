import React, { useState } from 'react';
import { Button, Menu, MenuItem } from '@mui/material';
import { Payment } from "../../../../app/models/accounting/payment";

const LOCALIZATION_KEY = "accounting.payments.form";

interface PaymentActionsProps {
    payment?: Payment;
    formEditMode: number;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    handleMenuSelect: (e: { item: { data: string } }) => void;
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
                                                       }) => {
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);

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

    const canViewApplications = payment?.statusId !== 'PMNT_NOT_PAID';
    const canViewTransactions = payment?.statusId !== 'PMNT_NOT_PAID';

    console.log('PaymentActions Rendered with payment:', payment);

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
                
                {getAvailableStatusTransitions(payment).toSent  && (
                    <MenuItem onClick={() => onMenuSelect('send')}>
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.send`, "Status to Sent")}
                    </MenuItem>
                )}
                {getAvailableStatusTransitions(payment).toReceived  && (
                    <MenuItem onClick={() => onMenuSelect('receive')}>
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.receive`, "Status to Received")}
                    </MenuItem>
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
        </>
    );
};

export default PaymentActions;