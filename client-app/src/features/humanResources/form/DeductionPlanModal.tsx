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

// Step a YYYY-MM-DD string by whole months, clamping the day to the target month's length.
// Date.setMonth() overflows instead ("Feb 30" → Mar 2), which silently skipped February and
// then drifted every later installment to the 2nd — and the payroll run keys installments by month.
const addMonthsClamped = (isoDate: string, months: number): string => {
    const [y, m, d] = isoDate.split("-").map(Number);
    const monthIndex = m - 1 + months;
    const year = y + Math.floor(monthIndex / 12);
    const month = ((monthIndex % 12) + 12) % 12;
    const daysInMonth = new Date(Date.UTC(year, month + 1, 0)).getUTCDate();
    const day = Math.min(d, daysInMonth);
    return `${year}-${String(month + 1).padStart(2, "0")}-${String(day).padStart(2, "0")}`;
};

interface DeductionPlanModalProps {
    onClose: () => void;
    totalAdvance: number;           // total amount to be deducted
    initialInstallmentCount?: number;
    initialStartDate?: Date | null;
    initialSchedules?: DeductionRow[];   // for edit/preview
    onApply: (schedules: Array<{ dueDate: string; scheduledAmount: number }>) => void;
    isPreview?: boolean;
    isReadOnly?: boolean;
    /** YYYY-MM-DD: pending rows may not fall before this month (advance date). The server also
     *  enforces "after the employee's latest payroll run". */
    minDueDate?: string | null;
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
                                               minDueDate = null,
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
            // Hint = how many installments "Generate" would (re)create: the pending ones only,
            // since deducted rows are pinned.
            const pendingCount = initialSchedules.filter(s => !s.payrollInvoiceId).length;
            setInstallmentCountHint(pendingCount > 0 ? pendingCount : initialSchedules.length);
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

    // Rows a payroll run already deducted. They are pinned: never regenerated, edited or removed —
    // the remaining balance is what gets re-spread.
    const processedRows = useMemo(() => rows.filter(r => !!r.payrollInvoiceId), [rows]);
    const isAnyProcessed = processedRows.length > 0;
    const processedTotal = useMemo(
        () => processedRows.reduce((sum, r) => sum + r.scheduledAmount, 0),
        [processedRows]
    );
    const remainingToSchedule = Math.round((totalAdvance - processedTotal) * 100) / 100;

    // The payroll run deducts one installment per month (mirrors DUPLICATE_MONTH_INSTALLMENT server-side).
    const duplicateMonth = useMemo(() => {
        const seen = new Set<string>();
        for (const r of rows) {
            const key = r.dueDate?.substring(0, 7);
            if (!key) continue;
            if (seen.has(key)) return key;
            seen.add(key);
        }
        return null;
    }, [rows]);

    // Earliest month (YYYY-MM) a pending row may use: the advance month, and strictly after the last
    // deducted installment. Runs are sequential, so anything earlier can never be collected.
    const earliestPendingMonth = useMemo(() => {
        let earliest = minDueDate ? minDueDate.substring(0, 7) : "";
        if (processedRows.length > 0) {
            const lastProcessed = [...processedRows].sort((a, b) => a.dueDate.localeCompare(b.dueDate))[processedRows.length - 1].dueDate;
            const afterLast = addMonthsClamped(lastProcessed, 1).substring(0, 7);
            if (afterLast > earliest) earliest = afterLast;
        }
        return earliest;
    }, [minDueDate, processedRows]);

    const tooEarlyRow = useMemo(
        () => earliestPendingMonth
            ? rows.find(r => !r.payrollInvoiceId && r.dueDate && r.dueDate.substring(0, 7) < earliestPendingMonth) ?? null
            : null,
        [rows, earliestPendingMonth]
    );

    const totalMatches = Math.abs(totalScheduled - totalAdvance) < 0.01;
    const isValid = totalMatches && !duplicateMonth && !tooEarlyRow;

    const validateRow = (row: DeductionRow): { dateError: string; amountError: string } => {
        let dateErr = "";
        let amountErr = "";

        if (!row.dueDate) {
            dateErr = getTranslatedLabel("validation.dateRequired", "Due date is required");
        } else if (earliestPendingMonth && row.dueDate.substring(0, 7) < earliestPendingMonth) {
            dateErr = `${getTranslatedLabel("party.employeeAdvance.deductionPlan.tooEarly", "Earliest month that can still be deducted is")} ${earliestPendingMonth}`;
        }

        if (!row.scheduledAmount || row.scheduledAmount <= 0) {
            amountErr = getTranslatedLabel("validation.amountPositive", "Amount must be greater than 0");
        }

        return { dateError: dateErr, amountError: amountErr };
    };

    // ────────────────────────────────────────────────
    // Generate equal monthly deductions
    // ────────────────────────────────────────────────
    // With processed rows present, `count` is the number of NEW installments and they start the
    // month after the last deducted one; only the remaining balance is spread.
    const generateEqualPlan = (count: number, firstDateStr: string) => {
        if (count < 1 || totalAdvance <= 0) return;

        const pinned = rows.filter(r => !!r.payrollInvoiceId);
        const pinnedTotal = pinned.reduce((s, r) => s + r.scheduledAmount, 0);
        const amountToSpread = Math.round((totalAdvance - pinnedTotal) * 100) / 100;
        if (amountToSpread <= 0) return;

        let anchor: string;
        let firstOffset: number;
        if (pinned.length > 0) {
            anchor = [...pinned].sort((a, b) => a.dueDate.localeCompare(b.dueDate))[pinned.length - 1].dueDate;
            firstOffset = 1;
        } else {
            if (!firstDateStr) return;
            anchor = firstDateStr;
            firstOffset = 0;
        }

        const amountEach = amountToSpread / count;
        const newRows: DeductionRow[] = [];

        for (let i = 1; i <= count; i++) {
            newRows.push({
                id: `gen-${i}`,
                number: 0, // renumbered below
                dueDate: addMonthsClamped(anchor, firstOffset + i - 1),
                scheduledAmount: Math.round(amountEach * 100) / 100,
            });
        }

        // Fix rounding difference on last installment
        const sumSoFar = newRows.slice(0, -1).reduce((s, r) => s + r.scheduledAmount, 0);
        newRows[newRows.length - 1].scheduledAmount = Math.round((amountToSpread - sumSoFar) * 100) / 100;

        const all = [...pinned, ...newRows]
            .sort((a, b) => a.dueDate.localeCompare(b.dueDate))
            .map((r, idx) => ({ ...r, number: idx + 1 }));

        setRows(all);
        setInstallmentCountHint(count);
    };

    const openEdit = (dataItem: DeductionRow) => {
        const idx = rows.findIndex((r) => r.id === dataItem.id);
        const { dateError, amountError } = validateRow(dataItem);
        setDateError(dateError);
        setAmountError(amountError);
        setEditModal({ row: { ...dataItem }, index: idx });
    };

    // Validate as the user types so a fixed value re-enables Save (errors used to be recomputed
    // only on Save, which was disabled while an error was showing).
    const updateEditRow = (patch: Partial<DeductionRow>) => {
        if (!editModal) return;
        const row = { ...editModal.row, ...patch };
        const { dateError, amountError } = validateRow(row);
        setDateError(dateError);
        setAmountError(amountError);
        setEditModal({ ...editModal, row });
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
            // Keep the grid in due-date order after a date edit (the server renumbers the same way).
            return copy
                .sort((a, b) => a.dueDate.localeCompare(b.dueDate))
                .map((r, idx) => ({ ...r, number: idx + 1 }));
        });

        setEditModal(null);
    };

    const addRow = () => {
        const lastDueDate = rows.length > 0
            ? rows[rows.length - 1].dueDate
            : new Date().toISOString().split("T")[0];

        setRows((prev) => [
            ...prev,
            {
                id: `manual-${prev.length + 1}`,
                number: prev.length + 1,
                dueDate: addMonthsClamped(lastDueDate, 1),
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
                {isAnyProcessed && (
                    <Typography variant="body2" color="text.secondary">
                        {getTranslatedLabel("party.employeeAdvance.deductionPlan.alreadyDeducted", "Already deducted")}: {processedTotal.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                        {" · "}
                        {getTranslatedLabel("party.employeeAdvance.deductionPlan.remainingToSchedule", "Remaining to schedule")}: {remainingToSchedule.toLocaleString(undefined, { minimumFractionDigits: 2 })} {getTranslatedLabel("general.currency.egp", "EGP")}
                    </Typography>
                )}
            </Grid>

            {duplicateMonth && (
                <Grid item xs={12}>
                    <Alert severity="error">
                        {getTranslatedLabel("party.employeeAdvance.deductionPlan.duplicateMonth", "Only one installment is allowed per month")} ({duplicateMonth})
                    </Alert>
                </Grid>
            )}

            {tooEarlyRow && (
                <Grid item xs={12}>
                    <Alert severity="error">
                        {getTranslatedLabel("party.employeeAdvance.deductionPlan.tooEarly", "Earliest month that can still be deducted is")} {earliestPendingMonth} ({tooEarlyRow.dueDate})
                    </Alert>
                </Grid>
            )}

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
                            disabled={totalAdvance <= 0 || installmentCountHint < 1 || (isAnyProcessed && remainingToSchedule <= 0)}
                        >
                            {isAnyProcessed
                                ? getTranslatedLabel("party.employeeAdvance.deductionPlan.generateRemaining", "Re-spread Remaining")
                                : getTranslatedLabel("party.employeeAdvance.deductionPlan.generateEqual", "Generate Equal Plan")}
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
                                cells={{ data: (props) => {
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
                                } }}
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
                    <Typography fontWeight="bold" color={totalMatches ? "success.main" : "error.main"}>
                        {totalMatches
                            ? getTranslatedLabel("party.employeeAdvance.deductionPlan.matchesTotal", "✓ Matches total")
                            : `${getTranslatedLabel("party.employeeAdvance.deductionPlan.difference", "Difference")}: ${(totalAdvance - totalScheduled).toFixed(2)} ${getTranslatedLabel("general.currency.egp", "EGP")}`}
                    </Typography>
                </Box>

                {!totalMatches && totalScheduled > 0 && (
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
                                    onChange={(e) => updateEditRow({ dueDate: e.target.value })}
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
                                    onChange={(e) => updateEditRow({ scheduledAmount: Number(e.target.value) || 0 })}
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