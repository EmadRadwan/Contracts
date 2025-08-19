import React, { useState } from 'react';
import { Button, Menu, MenuItem } from '@mui/material';
import { Payment } from "../../../../app/models/accounting/payment";

// REFACTOR: Define constant for localization key to avoid repetition and improve maintainability
const LOCALIZATION_KEY = "accounting.payments.form";

interface PaymentActionsProps {
    payment?: Payment;
    formEditMode: number;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    handleMenuSelect: (e: { item: { data: string } }) => void;
}

// REFACTOR: Move status transitions logic to a separate function for clarity and reusability
const getAvailableStatusTransitions = (payment?: Payment) => {
    if (!payment) {
        return { toSent: false, toReceived: false, toCancelled: false, toConfirmed: false, toVoid: false };
    }
    const isOutgoing = payment.isDisbursement; // Determines if payment is outgoing (true) or incoming (false)
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
    // REFACTOR: Add state for managing MUI Menu anchor element, following the CreateCustomerMenu example
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);

    // REFACTOR: Handle button click to open the menu, aligning with MUI Menu behavior
    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    // REFACTOR: Handle menu close to reset anchor element, aligning with MUI Menu behavior
    const handleClose = () => {
        setAnchorEl(null);
    };

    // REFACTOR: Adapt menu selection handler to MUI Menu, maintaining confirmation logic for destructive actions
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

    // In create mode (formEditMode === 1), only show create options
    if (formEditMode === 1) {
        return (
            <>
                <Button
                    variant="contained"
                    color="primary"
                    onClick={handleClick}
                    sx={{ mt: 2, mr: 2 }}
                    disabled={!payment} // REFACTOR: Disable button if no payment is provided
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
                disabled={!payment} // REFACTOR: Disable button if no payment is provided
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
                
                {getAvailableStatusTransitions(payment).toSent && (
                    <MenuItem onClick={() => onMenuSelect('send')}>
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.send`, "Status to Sent")}
                    </MenuItem>
                )}
                {getAvailableStatusTransitions(payment).toReceived && (
                    <MenuItem onClick={() => onMenuSelect('receive')}>
                        {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.receive`, "Status to Received")}
                    </MenuItem>
                )}
                {getAvailableStatusTransitions(payment).toCancelled && (
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
                </MenuItem>
                <MenuItem onClick={() => onMenuSelect('transactions')}>
                    {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.transactions`, "Transactions")}
                </MenuItem>
                <MenuItem onClick={() => onMenuSelect('applications')}>
                    {getTranslatedLabel(`${LOCALIZATION_KEY}.actions.applications`, "Payment Applications")}
                </MenuItem>
            </Menu>
        </>
    );
};

export default PaymentActions;