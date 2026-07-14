import { useState } from 'react';
import { Box, Typography, CircularProgress, Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper } from '@mui/material';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {useGetReservationItemsQuery, useIssueProductionRunReservationsMutation} from "../../../app/store/apis";

interface Props {
    workEffortId: string;
    onClose: () => void;
    language: string;
}

interface ReservationItem {
    workEffortInvResId: string;
    productId: string;
    productName: string;
    inventoryItemId: string;
    colorDescription: string;
    quantity: number;
}

export default function IssueReservationsModal({ workEffortId, onClose, language }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const [error, setError] = useState<string | null>(null);

    // REFACTOR: Pass language to getReservationItems query
    // Purpose: Support localized ColorDescription
    // Benefit: Matches backend handler's language handling
    const { data: reservations = [], isLoading, error: fetchError } = useGetReservationItemsQuery(workEffortId);
    const [issueReservations, { isLoading: isIssuing, error: issueError }] = useIssueProductionRunReservationsMutation();

    const handleIssue = async () => {
        setError(null);
        try {
            const response = await issueReservations({
                issueParams: {
                    workEffortId,
                    failIfNotEnoughQoh: true,
                    description: "Issuing reserved BOM components",
                },
            }).unwrap();

            if (response.success) {
                onClose();
            } else {
                setError(response.message);
            }
        } catch (err: any) {
            setError(err.data?.message || 'Failed to issue materials');
        }
    };

    return (
        <Box sx={{ p: 3, maxWidth: 600, mx: 'auto' }}>
            <Typography variant="h6">
                {getTranslatedLabel("manufacturing.reservations.issueMaterials", "Issue Reserved Materials")}
            </Typography>
            {isLoading && <CircularProgress />}
            {fetchError && <Typography color="error">{fetchError.message || 'Failed to fetch reservations'}</Typography>}
            {!isLoading && !fetchError && (
                <>
                    {reservations.length === 0 ? (
                        <Typography>{getTranslatedLabel("manufacturing.reservations.noReservations", "No reservations found for this WorkEffort.")}</Typography>
                    ) : (
                        <TableContainer component={Paper} sx={{ mt: 2 }}>
                            <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableCell>{getTranslatedLabel("manufacturing.reservations.product", "Product")}</TableCell>
                                        <TableCell>{getTranslatedLabel("manufacturing.reservations.color", "Color")}</TableCell>
                                        <TableCell>{getTranslatedLabel("manufacturing.reservations.inventoryItem", "Inventory Item")}</TableCell>
                                        <TableCell>{getTranslatedLabel("manufacturing.reservations.quantity", "Quantity")}</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {reservations.map((item: ReservationItem) => (
                                        <TableRow key={item.workEffortInvResId}>
                                            <TableCell>{item.productName}</TableCell>
                                            <TableCell>{item.colorDescription}</TableCell>
                                            <TableCell>{item.inventoryItemId}</TableCell>
                                            <TableCell>{item.quantity}</TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    )}
                    {error && <Typography color="error" sx={{ mt: 2 }}>{error}</Typography>}
                    {issueError && <Typography color="error" sx={{ mt: 2 }}>{issueError.data?.message}</Typography>}
                    <Box sx={{ mt: 2, display: 'flex', justifyContent: 'flex-end' }}>
                        <Button onClick={onClose} disabled={isLoading || isIssuing} sx={{ mr: 1 }}>
                            {getTranslatedLabel("general.cancel", "Cancel")}
                        </Button>
                        <Button
                            variant="contained"
                            color="primary"
                            onClick={handleIssue}
                            disabled={isLoading || isIssuing || !reservations.length}
                        >
                            {isIssuing ? (
                                <>
                                    <CircularProgress size={20} />
                                    <span>{getTranslatedLabel("manufacturing.reservations.issuing", "Issuing Materials")}</span>
                                </>
                            ) : (
                                getTranslatedLabel("manufacturing.reservations.issue", "Issue Materials")
                            )}
                        </Button>
                    </Box>
                </>
            )}
        </Box>
    );
}