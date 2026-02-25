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
    RadioGroup,
    FormControlLabel,
    Radio,
    Checkbox,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import { Grid as KendoGrid, GridColumn as Column, GridToolbar } from "@progress/kendo-react-grid";
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
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
    const [roundingMode, setRoundingMode] = useState<'100' | '1000'>('100');
    const [bulkEditOpen, setBulkEditOpen] = useState(false);
    const [bulkAmount, setBulkAmount] = useState<number | "">("");

    const [editModal, setEditModal] = useState<EditModalData | null>(null);
    const [dateError, setDateError] = useState<string>("");
    const [amountError, setAmountError] = useState<string>("");

    const advanceRows = rows.filter(r => r.isAdvance);
    const regularRows = rows.filter(r => !r.isAdvance);
    const advanceSum = advanceRows.reduce((s, r) => s + r.amount, 0);
    const regularSum = regularRows.reduce((s, r) => s + r.amount, 0);
    const grandTotal = advanceSum + regularSum;
    const isValid = Math.abs(grandTotal - totalPrice) < 0.01;


    const applyRounding = (amount: number, mode: '100' | '1000'): number => {
        const factor = mode === '100' ? 100 : 1000;
        return Math.round(amount / factor) * factor;
    };

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
                amount: applyRounding(advancePayment, roundingMode),
                isAdvance: true,
            });
        }

        if ((totalPrice - advancePayment) > 0 && numberOfInstallments > 0 && dateOfFirstInstallment) {
            const totalToSplit = totalPrice - (advanceRowsLocal.length > 0 ? advanceRowsLocal[0].amount : advancePayment);
            const installmentAmount = applyRounding(totalToSplit / numberOfInstallments, roundingMode);
            let currentDate = new Date(dateOfFirstInstallment);

            for (let i = 1; i <= numberOfInstallments; i++) {
                regularRowsLocal.push({
                    id: `inst-${i}`,
                    number: i + advanceRowsLocal.length,
                    dueDate: currentDate.toISOString().split("T")[0],
                    amount: i === 1 ? 0 : installmentAmount, // Will fix first installment below
                    isAdvance: false,
                });
                currentDate.setMonth(currentDate.getMonth() + monthsBetweenInstallments);
            }

            // Adjust first regular installment to take the remainder
            if (regularRowsLocal.length > 0) {
                const totalAdvance = advanceRowsLocal.reduce((s, r) => s + r.amount, 0);
                const otherRegularsSum = regularRowsLocal.slice(1).reduce((s, r) => s + r.amount, 0);
                regularRowsLocal[0].amount = totalPrice - totalAdvance - otherRegularsSum;
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
        roundingMode
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
            dateError = getTranslatedLabel("salesRequest.paymentPlan.dueDateRequired", "Due date is required");
        } else {
            const selected = new Date(row.dueDate);
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            /*if (selected < today) {
                dateError = "Due date cannot be in the past";
            }*/
        }

        if (!row.amount || row.amount <= 0) {
            amountError = getTranslatedLabel("salesRequest.paymentPlan.amountGreaterThanZero", "Amount must be greater than 0");
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
        const rawEqualAmount = advancePayment / count;
        const equalAmount = applyRounding(rawEqualAmount, roundingMode);

        if (diff > 0) {
            // Adding new rows
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

            const combinedAdvance = [...updatedCurrent, ...newAdvanceRows];
            // Fix first advance row if needed (though usually advances match advancePayment exactly)
            const sumAdv = combinedAdvance.reduce((s, r) => s + r.amount, 0);
            if (sumAdv !== advancePayment) {
                combinedAdvance[0].amount += (advancePayment - sumAdv);
            }

            setRows([...combinedAdvance, ...rows.filter((r) => !r.isAdvance)]);
        } else {
            // Removing rows
            const keptAdvanceRows = currentAdvanceRows.slice(0, count).map((row) => ({
                ...row,
                amount: equalAmount,
            }));

            if (keptAdvanceRows.length > 0) {
                const sumAdv = keptAdvanceRows.reduce((s, r) => s + r.amount, 0);
                keptAdvanceRows[0].amount += (advancePayment - sumAdv);
            }

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
                    title={getTranslatedLabel("salesRequest.paymentPlan.edit", "Edit")}
                    onClick={() => openEditModal(dataItem)}
                >
                    <EditIcon fontSize="small" />
                </IconButton>

                {dataItem.isAdvance && advanceSplitCount > 1 && (
                    <IconButton
                        size="small"
                        color="error"
                        title={getTranslatedLabel("salesRequest.paymentPlan.delete", "Delete")}
                        onClick={() => deleteAdvanceRow(dataItem.id)}
                    >
                        <DeleteIcon fontSize="small" />
                    </IconButton>
                )}
            </td>
        );
    };
    
    const handleBulkApply = () => {
        if (bulkAmount === "" || bulkAmount <= 0) return;

        const roundedBulkAmount = applyRounding(bulkAmount, roundingMode);
        
        setRows(prevRows => {
            const newRows = prevRows.map(row => {
                if (selectedIds.has(row.id)) {
                    return { ...row, amount: roundedBulkAmount };
                }
                return row;
            });

            // Always add the remaining in the first installment (regular)
            const totalAdvance = newRows.filter(r => r.isAdvance).reduce((s, r) => s + r.amount, 0);
            const otherRegularsSum = newRows.filter(r => !r.isAdvance).slice(1).reduce((s, r) => s + r.amount, 0);
            
            const firstRegularIdx = newRows.findIndex(r => !r.isAdvance);
            if (firstRegularIdx !== -1) {
                newRows[firstRegularIdx].amount = totalPrice - totalAdvance - otherRegularsSum;
            }

            return newRows;
        });

        setBulkEditOpen(false);
        setBulkAmount("");
        setSelectedIds(new Set());
    };

    const toggleSelection = (id: string) => {
        const newSelection = new Set(selectedIds);
        if (newSelection.has(id)) {
            newSelection.delete(id);
        } else {
            newSelection.add(id);
        }
        setSelectedIds(newSelection);
    };

    const toggleSelectAll = () => {
        if (selectedIds.size === rows.length) {
            setSelectedIds(new Set());
        } else {
            setSelectedIds(new Set(rows.map(r => r.id)));
        }
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
                    <Alert severity="info">{getTranslatedLabel("salesRequest.paymentPlan.previewAlert", "Preview – changes will be saved on submit")}</Alert>
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

            {/* Advance Split Control & Rounding */}
            <Grid item xs={12}>
                <Box display="flex" alignItems="center" flexWrap="wrap" gap={3}>
                    {advancePayment > 0 && (
                        <Box display="flex" alignItems="center" gap={2}>
                            <Typography variant="subtitle1">
                                {getTranslatedLabel("salesRequest.paymentPlan.splitAdvance", "Split Advance:")}
                            </Typography>
                            <TextField
                                select
                                size="small"
                                value={advanceSplitCount}
                                onChange={(e) => handleSplitChange(Number(e.target.value))}
                                sx={{ width: 80 }}
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
                    )}

                    <Box display="flex" alignItems="center" gap={1}>
                        <Typography variant="subtitle1">{getTranslatedLabel("salesRequest.paymentPlan.rounding", "Rounding:")}</Typography>
                        <RadioGroup
                            row
                            value={roundingMode}
                            onChange={(e) => setRoundingMode(e.target.value as '100' | '1000')}
                        >
                            <FormControlLabel value="100" control={<Radio size="small" />} label={getTranslatedLabel("salesRequest.paymentPlan.nearest100", "Nearest 100")} />
                            <FormControlLabel value="1000" control={<Radio size="small" />} label={getTranslatedLabel("salesRequest.paymentPlan.nearest1000", "Nearest 1000")} />
                        </RadioGroup>
                    </Box>
                </Box>
            </Grid>

            {/* Kendo Grid */}
            <Grid item xs={12}>
                <div style={{ height: '400px', overflow: 'auto' }}>
                    <KendoGrid
                        data={rows}
                        sortable
                        scrollable={"none"}
                    >
                        <GridToolbar>
                            <Button
                                variant="outlined"
                                size="small"
                                onClick={() => setBulkEditOpen(true)}
                                disabled={selectedIds.size === 0}
                            >
                                {getTranslatedLabel("salesRequest.paymentPlan.setInstallmentValue", "Set Installment Value")}
                            </Button>
                        </GridToolbar>
                        <Column
                            width="50"
                            headerCell={() => (
                                <Checkbox
                                    size="small"
                                    checked={selectedIds.size === rows.length && rows.length > 0}
                                    indeterminate={selectedIds.size > 0 && selectedIds.size < rows.length}
                                    onChange={toggleSelectAll}
                                />
                            )}
                            cell={(props) => (
                                <td>
                                    <Checkbox
                                        size="small"
                                        checked={selectedIds.has(props.dataItem.id)}
                                        onChange={() => toggleSelection(props.dataItem.id)}
                                    />
                                </td>
                            )}
                        />
                        <Column
                            field="number"
                            title="#"
                            width="60"
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
                    <Column title={getTranslatedLabel("salesRequest.paymentPlan.actions", "Actions")} width="140" cell={ActionsCell} />

                    </KendoGrid>
                </div>
            </Grid>

            {/* Summary */}
            <Grid item xs={12}>
                <Box display="flex" flexDirection="column" gap={1}>
                    <Box display="flex" justifyContent="space-between">
                        <Typography variant="body2">
                            {getTranslatedLabel("salesRequest.paymentPlan.advanceTotal", "Advance Total:")} {advanceSum.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                        </Typography>
                        <Typography variant="body2">
                            {getTranslatedLabel("salesRequest.paymentPlan.installmentsTotal", "Installments Total:")} {regularSum.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                        </Typography>
                        <Typography variant="body2" fontWeight="bold">
                            {getTranslatedLabel("salesRequest.paymentPlan.grandTotal", "Grand Total:")} {grandTotal.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                            {" "}{isValid ? "✓" : "✗"}
                        </Typography>
                    </Box>
                    {!isValid && (
                        <Alert severity="warning">
                            {getTranslatedLabel("salesRequest.paymentPlan.mismatchWarning", "Grand total must equal apartment total ({totalPrice}). Adjust to enable Apply.").replace("{totalPrice}", totalPrice.toLocaleString())}
                        </Alert>
                    )}
                </Box>
            </Grid>

            {/* Actions */}
            <Grid item xs={12}>
                <Box display="flex" justifyContent="flex-end" gap={2}  sx={{ pt: 1 }}>
                    <Button onClick={onClose}>{getTranslatedLabel("salesRequest.paymentPlan.cancel", "Cancel")}</Button>
                    <Button variant="contained" color="primary" onClick={handleApply} disabled={!isValid}>
                        {getTranslatedLabel("salesRequest.paymentPlan.applyToForm", "Apply to Form")}
                    </Button>
                </Box>
            </Grid>

            {/* Edit Modal */}
            <Dialog open={!!editModal} onClose={() => setEditModal(null)} maxWidth="sm" fullWidth>
                <DialogTitle>{getTranslatedLabel("salesRequest.paymentPlan.editInstallment", "Edit Installment")}</DialogTitle>
                <DialogContent>
                    {editModal && (
                        <Grid container spacing={2} sx={{ mt: 1 }}>
                            <Grid item xs={12}>
                                <TextField
                                    label={getTranslatedLabel("salesRequest.paymentPlan.dueDate", "Due Date")}
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
                                    label={getTranslatedLabel("salesRequest.paymentPlan.amount", "Amount")}
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
                                    helperText={amountError || getTranslatedLabel("salesRequest.paymentPlan.mustBeGreaterThanZero", "Must be greater than 0")}
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
                        {getTranslatedLabel("salesRequest.paymentPlan.cancel", "Cancel")}
                    </Button>
                    <Button
                        variant="contained"
                        onClick={saveEdit}
                        disabled={!!dateError || !!amountError} // REFACTOR: Disable Save if validation fails
                    >
                        {getTranslatedLabel("salesRequest.paymentPlan.save", "Save")}
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Bulk Edit Dialog */}
            <Dialog open={bulkEditOpen} onClose={() => setBulkEditOpen(false)} maxWidth="xs" fullWidth>
                <DialogTitle>{getTranslatedLabel("salesRequest.paymentPlan.setInstallmentValue", "Set Installment Value")}</DialogTitle>
                <DialogContent>
                    <Box sx={{ mt: 1 }}>
                        <TextField
                            label={getTranslatedLabel("salesRequest.paymentPlan.installmentAmount", "Installment Amount")}
                            type="number"
                            fullWidth
                            value={bulkAmount}
                            onChange={(e) => setBulkAmount(parseFloat(e.target.value) || "")}
                            InputProps={{
                                startAdornment: <InputAdornment position="start">$</InputAdornment>,
                            }}
                            helperText={getTranslatedLabel("salesRequest.paymentPlan.roundingHelper", "Will be rounded to nearest {roundingMode}").replace("{roundingMode}", roundingMode)}
                        />
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setBulkEditOpen(false)}>{getTranslatedLabel("salesRequest.paymentPlan.cancel", "Cancel")}</Button>
                    <Button variant="contained" onClick={handleBulkApply} disabled={!bulkAmount}>
                        {getTranslatedLabel("salesRequest.paymentPlan.apply", "Apply")}
                    </Button>
                </DialogActions>
            </Dialog>
        </Grid>
    );
}