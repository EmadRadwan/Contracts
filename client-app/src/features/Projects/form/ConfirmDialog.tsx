import React from 'react';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
} from '@mui/material';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';

interface ConfirmDialogProps {
    open: boolean;
    title?: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
    onConfirm: () => void;
    onCancel: () => void;
    loading?: boolean;
    danger?: boolean; // makes confirm button red
}

export const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
                                                                open,
                                                                title = 'Confirm Action',
                                                                message,
                                                                confirmText = 'Yes, Delete',
                                                                cancelText = 'Cancel',
                                                                onConfirm,
                                                                onCancel,
                                                                loading = false,
                                                                danger = false,
                                                            }) => {
    const { getTranslatedLabel } = useTranslationHelper();

    return (
        <Dialog
            open={open}
            onClose={onCancel}
            aria-labelledby="confirm-dialog-title"
            aria-describedby="confirm-dialog-description"
            maxWidth="sm"
            fullWidth
        >
            <DialogTitle id="confirm-dialog-title">
                {getTranslatedLabel('common.confirm', title)}
            </DialogTitle>
            <DialogContent>
                <DialogContentText id="confirm-dialog-description">
                    {message}
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Button
                    onClick={onCancel}
                    disabled={loading}
                >
                    {getTranslatedLabel('common.cancel', cancelText)}
                </Button>
                <Button
                    onClick={onConfirm}
                    disabled={loading}
                    color={danger ? 'error' : 'primary'}
                    variant="contained"
                    autoFocus
                >
                    {loading
                        ? getTranslatedLabel('common.deleting', 'Deleting...')
                        : getTranslatedLabel('common.delete', confirmText)}
                </Button>
            </DialogActions>
        </Dialog>
    );
};