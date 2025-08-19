// src/features/orders/return/components/ReturnTotals.tsx
import React from 'react';
import { Table, TableBody, TableCell, TableRow, TableContainer, Typography, Box } from '@mui/material';
import { GetTranslatedLabel } from '../../../../app/hooks/useTranslationHelper';
import { ReturnableItems } from '../../../../app/models/order/return';

// REFACTOR: Extract totals table into a separate component
// Purpose: Isolate totals display for clarity
// Why: Simplifies main component and improves reusability
interface ReturnTotalsProps {
    returnableItems?: ReturnableItems;
    returnTotal: number;
    getTranslatedLabel: GetTranslatedLabel;
}

export const ReturnTotals: React.FC<ReturnTotalsProps> = ({ returnableItems, returnTotal, getTranslatedLabel }) => {
    return (
        <Box sx={{ mt: 2 }}>
            {returnableItems && (
                <TableContainer>
                    <Table>
                        <TableBody>
                            <TableRow>
                                <TableCell width="25%">{getTranslatedLabel('Order', 'OrderTotal')}</TableCell>
                                <TableCell>{returnableItems.orderTotal?.toFixed(2)}</TableCell>
                            </TableRow>
                            <TableRow>
                                <TableCell>{getTranslatedLabel('Order', 'AmountAlreadyCredited')}</TableCell>
                                <TableCell>{returnableItems.creditedTotal?.toFixed(2)}</TableCell>
                            </TableRow>
                            <TableRow>
                                <TableCell>{getTranslatedLabel('Order', 'AmountAlreadyRefunded')}</TableCell>
                                <TableCell>{returnableItems.refundedTotal?.toFixed(2)}</TableCell>
                            </TableRow>
                        </TableBody>
                    </Table>
                </TableContainer>
            )}
            <Typography>{getTranslatedLabel('Order', 'ReturnTotal')}: {returnTotal.toFixed(2)}</Typography>
        </Box>
    );
};