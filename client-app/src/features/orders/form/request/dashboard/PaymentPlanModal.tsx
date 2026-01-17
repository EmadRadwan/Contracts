import React, { useState, useEffect } from "react";
import {
    Grid,
    Typography,
    Button,
    TextField,
    MenuItem,
    Alert,
    Box,
    IconButton,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions, InputAdornment,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import { Grid as KendoGrid, GridColumn as Column } from "@progress/kendo-react-grid";
import { SalesRequest } from "../../../../../app/models/order/SalesRequest";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";

interface InstallmentRow {
    id: string;
    number: number;
    dueDate: string; // YYYY-MM-DD
    amount: number;
    isAdvance?: boolean;
}

interface EditModalData {
    row: InstallmentRow;
    index: number;
}

interface PaymentPlanModalProps {
    onClose: () => void;
    salesRequest: SalesRequest;
    apartment?: { productName: string };
    onApply: (installments: Array<{
        dueDate: string;
        amount: number;
        isAdvance: boolean;  // ← NEW
    }>) => void;
    isPreview?: boolean;
    initialInstallments?: Array<{
        dueDate: string;
        amount: number;
        isAdvance: boolean;  // ← MUST include this now
    }>; // ← NEW

}



export default function PaymentPlanModal({
                                             onClose,
                                             salesRequest,
                                             apartment,
                                             onApply,
                                             isPreview = false, initialInstallments
                                         }: PaymentPlanModalProps) {
    const { getTranslatedLabel } = useTranslationHelper();

    const {
        totalPrice = 0,
        advancePayment = 0,
        numberOfInstallments = 0,
        dateOfFirstInstallment,
        monthsBetweenInstallments = 0,
    } = salesRequest;

    const remaining = totalPrice - advancePayment;

    const [advanceSplitCount, setAdvanceSplitCount] = useState(1);
    const [rows, setRows] = useState<InstallmentRow[]>([]);
    const [editModal, setEditModal] = useState<EditModalData | null>(null);
    const [dateError, setDateError] = useState<string>("");
    const [amountError, setAmountError] = useState<string>("");

    const advanceRows = rows.filter(r => r.isAdvance);
    const regularRows = rows.filter(r => !r.isAdvance);
    const advanceSum = advanceRows.reduce((s, r) => s + r.amount, 0);
    const regularSum = regularRows.reduce((s, r) => s + r.amount, 0);
    const grandTotal = advanceSum + regularSum;
    const isValid = Math.abs(grandTotal - totalPrice) < 0.01;


    useEffect(() => {
        if (initialInstallments && initialInstallments.length > 0) {
            const mapped: InstallmentRow[] = initialInstallments.map((inst, idx) => ({
                id: `custom-${idx}`,
                number: idx + 1,
                dueDate: inst.dueDate,
                amount: inst.amount,
                isAdvance: inst.isAdvance,
            }));

            const advanceCount = mapped.filter(r => r.isAdvance).length;
            setAdvanceSplitCount(advanceCount > 0 ? advanceCount : 1);

            setRows(mapped);
            return;
        }

        // Default generation
        const advanceRowsLocal: InstallmentRow[] = [];
        const regularRowsLocal: InstallmentRow[] = [];

        if (advancePayment > 0) {
            const singleAdvanceDate = new Date();
            singleAdvanceDate.setDate(singleAdvanceDate.getDate() + 7);
            advanceRowsLocal.push({
                id: "adv-1",
                number: 1,
                dueDate: singleAdvanceDate.toISOString().split("T")[0],
                amount: advancePayment,
                isAdvance: true,
            });
        }

        if ((totalPrice - advancePayment) > 0 && numberOfInstallments > 0 && dateOfFirstInstallment) {
            const installmentAmount = (totalPrice - advancePayment) / numberOfInstallments;
            let currentDate = new Date(dateOfFirstInstallment);

            for (let i = 1; i <= numberOfInstallments; i++) {
                regularRowsLocal.push({
                    id: `inst-${i}`,
                    number: i + advanceRowsLocal.length,
                    dueDate: currentDate.toISOString().split("T")[0],
                    amount: installmentAmount,
                    isAdvance: false,
                });
                currentDate.setMonth(currentDate.getMonth() + monthsBetweenInstallments);
            }
        }

        setRows([...advanceRowsLocal, ...regularRowsLocal]);
        setAdvanceSplitCount(advanceRowsLocal.length);
    }, [
        initialInstallments,
        advancePayment,
        totalPrice,
        numberOfInstallments,
        dateOfFirstInstallment,
        monthsBetweenInstallments,
    ]);

    useEffect(() => {
        if (editModal) {
            const { dateError, amountError } = validateRow(editModal.row);
            setDateError(dateError);
            setAmountError(amountError);
        }
    }, [editModal?.row.dueDate, editModal?.row.amount]);

    const validateRow = (row: InstallmentRow): { dateError: string; amountError: string } => {
        let dateError = "";
        let amountError = "";

        if (!row.dueDate) {
            dateError = "Due date is required";
        } else {
            const selected = new Date(row.dueDate);
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            /*if (selected < today) {
                dateError = "Due date cannot be in the past";
            }*/
        }

        if (!row.amount || row.amount <= 0) {
            amountError = "Amount must be greater than 0";
        }

        return { dateError, amountError };
    };

    const handleSplitChange = (count: number) => {
        setAdvanceSplitCount(count);

        const currentAdvanceRows = rows.filter((r) => r.isAdvance);
        const diff = count - currentAdvanceRows.length;

        if (diff === 0) return; // No change

        const baseDate = new Date();
        baseDate.setDate(baseDate.getDate() + 7);
        const equalAmount = advancePayment / count; // Even share for all advance rows

        if (diff > 0) {
            // Adding new rows → update existing + create new ones with equal amounts
            const updatedCurrent = currentAdvanceRows.map((row) => ({
                ...row,
                amount: equalAmount,
            }));

            const newAdvanceRows = Array.from({ length: diff }, (_, i) => ({
                id: `adv-${currentAdvanceRows.length + i + 1}`,
                number: currentAdvanceRows.length + i + 1,
                dueDate: baseDate.toISOString().split("T")[0],
                amount: equalAmount,
                isAdvance: true,
            }));

            setRows([...updatedCurrent, ...newAdvanceRows, ...rows.filter((r) => !r.isAdvance)]);
        } else {
            // Removing rows (diff < 0) → keep first 'count' rows and redistribute amount across them
            const keptAdvanceRows = currentAdvanceRows.slice(0, count).map((row) => ({
                ...row,
                amount: equalAmount,
            }));

            setRows([...keptAdvanceRows, ...rows.filter((r) => !r.isAdvance)]);
        }
    };
    
    
    const openEditModal = (dataItem: InstallmentRow) => {
        const index = rows.findIndex((r) => r.id === dataItem.id);
        setEditModal({ row: { ...dataItem }, index });
    };

    const saveEdit = () => {
        if (!editModal) return;

        const { dateError, amountError } = validateRow(editModal.row);
        setDateError(dateError);
        setAmountError(amountError);

        if (dateError || amountError) return;

        const oldRow = rows[editModal.index];
        const isEditingAdvance = oldRow.isAdvance === true;

        setRows((prev) => {
            const newRows = [...prev];
            const updatedRow = { ...editModal.row };
            newRows[editModal.index] = updatedRow;

            if (isEditingAdvance) {
                // Step 1: Calculate new total advance sum
                const newAdvanceSum = newRows
                    .filter(r => r.isAdvance)
                    .reduce((sum, r) => sum + r.amount, 0);

                // Step 2: Calculate new remaining for regulars
                const newRemaining = totalPrice - newAdvanceSum;

                // Step 3: Count regular installments
                const regularCount = newRows.filter(r => !r.isAdvance).length;

                if (regularCount > 0) {
                    // Step 4: Make all regulars equal
                    const equalRegularAmount = newRemaining / regularCount;

                    // Apply rounded amount to all regular rows
                    let accumulatedRoundingError = 0;
                    newRows.forEach((row, idx) => {
                        if (!row.isAdvance) {
                            const rounded = Math.round(equalRegularAmount * 100) / 100;
                            accumulatedRoundingError += (equalRegularAmount - rounded);
                            newRows[idx].amount = rounded;
                        }
                    });

                    // Step 5: Fix rounding error on last regular row
                    if (Math.abs(accumulatedRoundingError) > 0.001) {
                        const lastRegularIdx = newRows.findLastIndex(r => !r.isAdvance);
                        if (lastRegularIdx !== -1) {
                            newRows[lastRegularIdx].amount += accumulatedRoundingError;
                            newRows[lastRegularIdx].amount =
                                Math.round(newRows[lastRegularIdx].amount * 100) / 100;
                        }
                    }
                }
            }

            return newRows;
        });

        setEditModal(null);
        setDateError("");
        setAmountError("");
    };
    
    const addAdvanceRow = () => {
        if (advanceSplitCount >= 3) return;
        handleSplitChange(advanceSplitCount + 1);
    };

    // REFACTOR: Delete advance row
    const deleteAdvanceRow = (id: string) => {
        if (advanceSplitCount <= 1) return;

        setRows((prev) => {
            const remainingAdvance = prev
                .filter((r) => r.isAdvance && r.id !== id);

            if (remainingAdvance.length === 0) return prev; // Safety (should not happen)

            const newCount = remainingAdvance.length;
            const equalAmount = advancePayment / newCount;

            const updatedAdvance = remainingAdvance.map((row) => ({
                ...row,
                amount: equalAmount,
            }));

            // Update state and split count
            setAdvanceSplitCount(newCount);

            return [...updatedAdvance, ...prev.filter((r) => !r.isAdvance)];
        });
    };
    
    const ActionsCell = (props: any) => {
        const { dataItem } = props;

        return (
            <td className="k-command-cell" style={{ textAlign: "center" }}>
                <IconButton
                    size="small"
                    color="primary"
                    title="Edit"
                    onClick={() => openEditModal(dataItem)}
                >
                    <EditIcon fontSize="small" />
                </IconButton>

                {dataItem.isAdvance && advanceSplitCount > 1 && (
                    <IconButton
                        size="small"
                        color="error"
                        title="Delete"
                        onClick={() => deleteAdvanceRow(dataItem.id)}
                    >
                        <DeleteIcon fontSize="small" />
                    </IconButton>
                )}
            </td>
        );
    };
    
    const handleApply = () => {
        if (!isValid) return;

        const finalInstallments = rows
            .sort((a, b) => a.dueDate.localeCompare(b.dueDate))
            .map((r) => ({
                dueDate: r.dueDate,
                amount: r.amount,
                isAdvance: r.isAdvance ?? false,  // ← NEW field
            }));

        onApply(finalInstallments);
        onClose();
    };

    return (
        <Grid container padding={2} spacing={2}>
            {isPreview && (
                <Grid item xs={12}>
                    <Alert severity="info">Preview – changes will be saved on submit</Alert>
                </Grid>
            )}

            <Grid item xs={12}>
                <Typography variant="h6">
                    {getTranslatedLabel("salesRequest.paymentPlan.title", "Payment Plan Schedule")}
                </Typography>
            </Grid>

            {apartment && (
                <Grid item xs={12}>
                    <Typography variant="body2" color="text.secondary">
                        {apartment.productName} - Total: {totalPrice.toLocaleString()}
                    </Typography>
                </Grid>
            )}

            {/* Advance Split Control */}
            {advancePayment > 0 && (
                <Grid item xs={12}>
                    <Box display="flex" alignItems="center" gap={2}>
                        <Typography variant="subtitle1">
                            Split Advance Payment ({advancePayment.toLocaleString()}) into:
                        </Typography>
                        <TextField
                            select
                            size="small"
                            value={advanceSplitCount}
                            onChange={(e) => handleSplitChange(Number(e.target.value))}
                            sx={{ width: 100 }}
                        >
                            {[1, 2, 3].map((n) => (
                                <MenuItem key={n} value={n}>{n}</MenuItem>
                            ))}
                        </TextField>
                        {advanceSplitCount < 3 && (
                            <IconButton color="primary" onClick={addAdvanceRow}>
                                <AddIcon />
                            </IconButton>
                        )}
                        
                    </Box>
                </Grid>
            )}

            {/* Kendo Grid */}
            <Grid item xs={12}>
                <div style={{ height: '400px', overflow: 'auto' }}>
                    <KendoGrid
                        data={rows}
                        sortable
                        scrollable={"none"}
                    >
                    <Column
                        field="number"
                        title="#"
                        width="80"
                        cell={(props) => (
                            <td>{props.dataItem.isAdvance ? `A${props.dataItem.number}` : props.dataItem.number}</td>
                        )}
                    />
                    <Column
                        field="dueDate"
                        title={getTranslatedLabel("salesRequest.paymentPlan.dueDate", "Due Date")}
                        width="180"
                        format="{0:dd/MM/yyyy}"
                    />
                    <Column
                        field="amount"
                        title={getTranslatedLabel("salesRequest.paymentPlan.amount", "Amount")}
                        width="180"
                        format="{0:n2}"
                    />
                        <Column title="Actions" width="140" cell={ActionsCell} />

                    </KendoGrid>
                </div>
            </Grid>

            {/* Summary */}
            <Grid item xs={12}>
                <Box display="flex" flexDirection="column" gap={1}>
                    <Box display="flex" justifyContent="space-between">
                        <Typography variant="body2">
                            Advance Total: {advanceSum.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                        </Typography>
                        <Typography variant="body2">
                            Installments Total: {regularSum.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                        </Typography>
                        <Typography variant="body2" fontWeight="bold">
                            Grand Total: {grandTotal.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                            {" "}{isValid ? "✓" : "✗"}
                        </Typography>
                    </Box>
                    {!isValid && (
                        <Alert severity="warning">
                            Grand total must equal apartment total ({totalPrice.toLocaleString()}). Adjust to enable Apply.
                        </Alert>
                    )}
                </Box>
            </Grid>

            {/* Actions */}
            <Grid item xs={12}>
                <Box display="flex" justifyContent="flex-end" gap={2}  sx={{ pt: 1 }}>
                    <Button onClick={onClose}>Cancel</Button>
                    <Button variant="contained" color="primary" onClick={handleApply} disabled={!isValid}>
                        Apply to Form
                    </Button>
                </Box>
            </Grid>

            {/* Edit Modal */}
            <Dialog open={!!editModal} onClose={() => setEditModal(null)} maxWidth="sm" fullWidth>
                <DialogTitle>Edit Installment</DialogTitle>
                <DialogContent>
                    {editModal && (
                        <Grid container spacing={2} sx={{ mt: 1 }}>
                            <Grid item xs={12}>
                                <TextField
                                    label="Due Date"
                                    type="date"
                                    fullWidth
                                    value={editModal.row.dueDate}
                                    onChange={(e) =>
                                        setEditModal({
                                            ...editModal,
                                            row: {
                                                ...editModal.row,
                                                dueDate: e.target.value,
                                            },
                                        })
                                    }
                                    InputLabelProps={{ shrink: true }}
                                    error={!!dateError}
                                    helperText={dateError}
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <TextField
                                    label="Amount"
                                    type="number"
                                    fullWidth
                                    value={editModal.row.amount}
                                    onChange={(e) =>
                                        setEditModal({
                                            ...editModal,
                                            row: { ...editModal.row, amount: parseFloat(e.target.value) || 0 },
                                        })
                                    }
                                    inputProps={{ step: "0.01", min: "0.01" }} // REFACTOR: Enforce min via input to prevent negative/zero entry easily
                                    InputProps={{
                                        startAdornment: <InputAdornment position="start">$</InputAdornment>,
                                    }}
                                    error={!!amountError}
                                    helperText={amountError || "Must be greater than 0"}
                                />
                            </Grid>
                        </Grid>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => {
                        setEditModal(null);
                        setDateError("");
                        setAmountError("");
                    }}>
                        Cancel
                    </Button>
                    <Button
                        variant="contained"
                        onClick={saveEdit}
                        disabled={!!dateError || !!amountError} // REFACTOR: Disable Save if validation fails
                    >
                        Save
                    </Button>
                </DialogActions>
            </Dialog>
        </Grid>
    );
}