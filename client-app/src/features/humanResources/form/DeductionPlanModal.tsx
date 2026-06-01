import { useState, useEffect, useMemo } from "react";
import {
    Grid,
    Typography,
    Button,
    TextField,
    Alert,
    Box,
    IconButton,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    InputAdornment,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import { Grid as KendoGrid, GridColumn as Column } from "@progress/kendo-react-grid";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

interface DeductionRow {
    id: string;
    number: number;
    dueDate: string;      // YYYY-MM-DD
    scheduledAmount: number;
    payrollInvoiceId?: string | null;
}

interface EditModalData {
    row: DeductionRow;
    index: number;
}

interface DeductionPlanModalProps {
    onClose: () => void;
    totalAdvance: number;           // total amount to be deducted
    initialInstallmentCount?: number;
    initialStartDate?: Date | null;
    initialSchedules?: DeductionRow[];   // for edit/preview
    onApply: (schedules: Array<{ dueDate: string; scheduledAmount: number }>) => void;
    isPreview?: boolean;
    isReadOnly?: boolean;
}

export default function DeductionPlanModal({
                                               onClose,
                                               totalAdvance,
                                               initialInstallmentCount = 12,
                                               initialStartDate = null,
                                               initialSchedules = [],
                                               onApply,
                                               isPreview = false,
                                               isReadOnly = false,
                                           }: DeductionPlanModalProps) {
    const { getTranslatedLabel } = useTranslationHelper();

    const [rows, setRows] = useState<DeductionRow[]>([]);
    const [editModal, setEditModal] = useState<EditModalData | null>(null);
    const [dateError, setDateError] = useState<string>("");
    const [amountError, setAmountError] = useState<string>("");
    const [installmentCountHint, setInstallmentCountHint] = useState<number>(initialInstallmentCount);

    
    // ────────────────────────────────────────────────
    // Initialize rows
    // ────────────────────────────────────────────────
    useEffect(() => {
        if (initialSchedules.length > 0) {
            // Edit/preview mode → load existing schedules
            setRows(
                initialSchedules.map((s, idx) => ({
                    ...s,
                    id: s.id || `row-${idx}`,
                    number: idx + 1,
                }))
            );
            // Also sync hint with actual count
            setInstallmentCountHint(initialSchedules.length);
            return;
        }

        // New plan → start empty, but auto-generate if we have useful defaults
        if (
            totalAdvance > 0 &&
            initialInstallmentCount > 0 &&
            initialStartDate &&
            rows.length === 0
        ) {
            const firstDateStr = initialStartDate.toISOString().split("T")[0];
            generateEqualPlan(initialInstallmentCount, firstDateStr);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [initialSchedules, totalAdvance, initialInstallmentCount, initialStartDate]);

    const totalScheduled = useMemo(
        () => rows.reduce((sum, r) => sum + r.scheduledAmount, 0),
        [rows]
    );

    const isAnyProcessed = useMemo(() => rows.some(r => !!r.payrollInvoiceId), [rows]);

    const isValid = useMemo(
        () => Math.abs(totalScheduled - totalAdvance) < 0.01,
        [totalScheduled, totalAdvance]
    );

    const validateRow = (row: DeductionRow): { dateError: string; amountError: string } => {
        let dateErr = "";
        let amountErr = "";

        if (!row.dueDate) {
            dateErr = getTranslatedLabel("validation.dateRequired", "Due date is required");
        }

        if (!row.scheduledAmount || row.scheduledAmount <= 0) {
            amountErr = getTranslatedLabel("validation.amountPositive", "Amount must be greater than 0");
        }

        return { dateError: dateErr, amountError: amountErr };
    };

    // ────────────────────────────────────────────────
    // Generate equal monthly deductions
    // ────────────────────────────────────────────────
    const generateEqualPlan = (count: number, firstDateStr: string) => {
        if (count < 1 || !firstDateStr || totalAdvance <= 0) return;

        const amountEach = totalAdvance / count;
        const newRows: DeductionRow[] = [];

        let current = new Date(firstDateStr);

        for (let i = 1; i <= count; i++) {
            newRows.push({
                id: `gen-${i}`,
                number: i,
                dueDate: current.toISOString().split("T")[0],
                scheduledAmount: Math.round(amountEach * 100) / 100,
            });
            current.setMonth(current.getMonth() + 1);
        }

        // Fix rounding difference on last installment
        const sumSoFar = newRows.slice(0, -1).reduce((s, r) => s + r.scheduledAmount, 0);
        newRows[newRows.length - 1].scheduledAmount = Math.round((totalAdvance - sumSoFar) * 100) / 100;

        setRows(newRows);
        setInstallmentCountHint(count);
    };

    const openEdit = (dataItem: DeductionRow) => {
        const idx = rows.findIndex((r) => r.id === dataItem.id);
        setEditModal({ row: { ...dataItem }, index: idx });
    };

    const saveEdit = () => {
        if (!editModal) return;

        const { dateError, amountError } = validateRow(editModal.row);
        setDateError(dateError);
        setAmountError(amountError);
        if (dateError || amountError) return;

        setRows((prev) => {
            const copy = [...prev];
            copy[editModal.index] = { ...editModal.row };
            return copy;
        });

        setEditModal(null);
    };

    const addRow = () => {
        const lastDate = rows.length > 0 ? new Date(rows[rows.length - 1].dueDate) : new Date();
        lastDate.setMonth(lastDate.getMonth() + 1);

        setRows((prev) => [
            ...prev,
            {
                id: `manual-${prev.length + 1}`,
                number: prev.length + 1,
                dueDate: lastDate.toISOString().split("T")[0],
                scheduledAmount: 0,
            },
        ]);
    };

    const deleteRow = (id: string) => {
        setRows((prev) => prev.filter((r) => r.id !== id).map((r, idx) => ({ ...r, number: idx + 1 })));
    };

    const handleApply = () => {
        if (!isValid) return;

        const sorted = [...rows].sort((a, b) => a.dueDate.localeCompare(b.dueDate));

        onApply(
            sorted.map((r) => ({
                dueDate: r.dueDate,
                scheduledAmount: r.scheduledAmount,
                payrollInvoiceId: r.payrollInvoiceId
            }))
        );

        onClose();
    };

    // ==================== DEBUG LOGS ====================
    console.log("=== DeductionPlanModal Debug ===");
    console.log("Props:", {
        isReadOnly,
        isPreview,
        totalAdvance,
        initialSchedulesCount: initialSchedules.length,
        initialInstallmentCount
    });

    console.log("State:", {
        rowsCount: rows.length,
        totalScheduled,
        isValid,
        isAnyProcessed
    });
    console.log("=====================================");
    // ===================================================

    return (
        <Grid container spacing={2} sx={{ p: 3, minWidth: 800 }}>
            {isReadOnly && (
                <Grid item xs={12}>
                    <Alert severity="info">{getTranslatedLabel("general.readOnlyMode", "Read-only mode")}</Alert>
                </Grid>
            )}

            {isPreview && !isReadOnly && (
                <Grid item xs={12}>
                    <Alert severity="info">{getTranslatedLabel("party.employeeAdvance.deductionPlan.previewMode", "Preview mode – changes will apply on form submit")}</Alert>
                </Grid>
            )}

            <Grid item xs={12}>
                <Typography variant="h6">
                    {getTranslatedLabel("party.employeeAdvance.deductionPlan.deductionRepaymentSchedule", "Deduction / Repayment Schedule")}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                    {getTranslatedLabel("party.employeeAdvance.deductionPlan.totalToDeduct", "Total to deduct")}: {totalAdvance.toLocaleString(undefined, { minimumFractionDigits: 2 })} {getTranslatedLabel("general.currency.egp", "EGP")}
                </Typography>
            </Grid>

            {/* Quick generation controls */}
            {!isReadOnly && (
                <Grid item xs={12}>
                    <Box
                        display="flex"
                        alignItems="center"
                        gap={2.5}
                        flexWrap="wrap"
                        sx={{ mb: 2 }}
                    >
                        <TextField
                            label={getTranslatedLabel("party.employeeAdvance.form.installmentCount", "Number of Installments")}
                            type="number"
                            size="small"
                            value={installmentCountHint}
                            onChange={(e) => {
                                const val = Number(e.target.value);
                                if (!isNaN(val) && val >= 0) setInstallmentCountHint(val);
                            }}
                            sx={{ width: 160 }}
                            InputProps={{ inputProps: { min: 1, step: 1 } }}
                        />

                        <Button
                            variant="outlined"
                            size="small"
                            onClick={() => {
                                const firstDate = rows.length > 0 ? rows[0].dueDate : null;
                                const effectiveDate = firstDate ||
                                    (initialStartDate ? initialStartDate.toISOString().split("T")[0] : null) ||
                                    new Date().toISOString().split("T")[0];
                                generateEqualPlan(installmentCountHint, effectiveDate);
                            }}
                            disabled={totalAdvance <= 0 || installmentCountHint < 1 || isAnyProcessed}
                        >
                            {getTranslatedLabel("party.employeeAdvance.deductionPlan.generateEqual", "Generate Equal Plan")}
                        </Button>

                        <Button
                            variant="outlined"
                            color="secondary"
                            size="small"
                            startIcon={<AddIcon />}
                            onClick={addRow}
                        >
                            {getTranslatedLabel("general.addRow", "Add Row")}
                        </Button>
                    </Box>
                </Grid>
            )}
            {/* Grid */}
            <Grid item xs={12}>
                <div style={{ height: "420px", overflow: "auto", border: "1px solid #ddd" }}>
                    <KendoGrid data={rows} sortable scrollable="none">
                        <Column field="number" title="#" width="60" />
                        <Column
                            field="dueDate"
                            title={getTranslatedLabel("party.employeeAdvance.deductionPlan.dueDate", "Due Date")}
                            width="160"
                            format="{0:dd/MM/yyyy}"
                        />
                        <Column
                            field="scheduledAmount"
                            title={getTranslatedLabel("party.employeeAdvance.deductionPlan.scheduledAmount", "Amount")}
                            width="160"
                            format="{0:n2}"
                        />
                        {!isReadOnly && (
                            <Column
                                title={getTranslatedLabel("general.actions", "Actions")}
                                width="120"
                                cell={(props) => {
                                    const isRowProcessed = !!props.dataItem.payrollInvoiceId;
                                    return (
                                        <td style={{ textAlign: "center" }}>
                                            <IconButton 
                                                size="small" 
                                                onClick={() => openEdit(props.dataItem)}
                                                disabled={isRowProcessed}
                                            >
                                                <EditIcon fontSize="small" />
                                            </IconButton>
                                            <IconButton
                                                size="small"
                                                color="error"
                                                onClick={() => deleteRow(props.dataItem.id)}
                                                disabled={isRowProcessed}
                                            >
                                                <DeleteIcon fontSize="small" />
                                            </IconButton>
                                        </td>
                                    );
                                }}
                            />
                        )}
                    </KendoGrid>
                </div>
            </Grid>

            {/* Summary */}
            <Grid item xs={12}>
                <Box display="flex" justifyContent="space-between" alignItems="center">
                    <Typography>
                        {getTranslatedLabel("party.employeeAdvance.deductionPlan.scheduled", "Scheduled")}: {totalScheduled.toLocaleString(undefined, { minimumFractionDigits: 2 })} {getTranslatedLabel("general.currency.egp", "EGP")}
                    </Typography>
                    <Typography fontWeight="bold" color={isValid ? "success.main" : "error.main"}>
                        {isValid 
                            ? getTranslatedLabel("party.employeeAdvance.deductionPlan.matchesTotal", "✓ Matches total") 
                            : `${getTranslatedLabel("party.employeeAdvance.deductionPlan.difference", "Difference")}: ${(totalAdvance - totalScheduled).toFixed(2)} ${getTranslatedLabel("general.currency.egp", "EGP")}`}
                    </Typography>
                </Box>

                {!isValid && totalScheduled > 0 && (
                    <Alert severity="warning" sx={{ mt: 1 }}>
                        {getTranslatedLabel("party.employeeAdvance.deductionPlan.mustMatchTotal", "Total scheduled must equal advance amount to apply.")}
                    </Alert>
                )}
            </Grid>

            {/* Buttons */}
            <Grid item xs={12} sx={{ mt: 2, textAlign: "right" }}>
                <Button onClick={onClose} variant={isReadOnly ? "contained" : "text"} sx={{ mr: 2 }}>
                    {isReadOnly ? getTranslatedLabel("general.close", "Close") : getTranslatedLabel("general.cancel", "Cancel")}
                </Button>
                {!isReadOnly && (
                    <Button variant="contained" disabled={!isValid || rows.length === 0} onClick={handleApply}>
                        {getTranslatedLabel("party.employeeAdvance.deductionPlan.applyToForm", "Apply to Form")}
                    </Button>
                )}
            </Grid>

            {/* Edit Dialog */}
            <Dialog open={!!editModal} onClose={() => setEditModal(null)} maxWidth="sm" fullWidth>
                <DialogTitle>{getTranslatedLabel("general.edit", "Edit Deduction")}</DialogTitle>
                <DialogContent>
                    {editModal && (
                        <Grid container spacing={2} sx={{ mt: 1 }}>
                            <Grid item xs={12}>
                                <TextField
                                    label={getTranslatedLabel("party.employeeAdvance.deductionPlan.dueDate", "Due Date")}
                                    type="date"
                                    fullWidth
                                    value={editModal.row.dueDate}
                                    onChange={(e) =>
                                        setEditModal({
                                            ...editModal,
                                            row: { ...editModal.row, dueDate: e.target.value },
                                        })
                                    }
                                    InputLabelProps={{ shrink: true }}
                                    error={!!dateError}
                                    helperText={dateError}
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <TextField
                                    label={getTranslatedLabel("party.employeeAdvance.deductionPlan.scheduledAmount", "Amount")}
                                    type="number"
                                    fullWidth
                                    value={editModal.row.scheduledAmount}
                                    onChange={(e) =>
                                        setEditModal({
                                            ...editModal,
                                            row: { ...editModal.row, scheduledAmount: Number(e.target.value) || 0 },
                                        })
                                    }
                                    InputProps={{
                                        startAdornment: <InputAdornment position="start">{getTranslatedLabel("general.currency.egp", "EGP")}</InputAdornment>,
                                        inputProps: { step: "0.01", min: "0.01" },
                                    }}
                                    error={!!amountError}
                                    helperText={amountError}
                                />
                            </Grid>
                        </Grid>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setEditModal(null)}>{getTranslatedLabel("general.cancel", "Cancel")}</Button>
                    <Button variant="contained" onClick={saveEdit} disabled={!!dateError || !!amountError}>
                        {getTranslatedLabel("general.save", "Save")}
                    </Button>
                </DialogActions>
            </Dialog>
        </Grid>
    );
}