import React, { useState, useEffect, useMemo } from "react";
import {
    Button,
    Box,
    Typography,
    Paper,
    Alert,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    IconButton,
    TextField,
    CircularProgress
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import { LoadingButton } from "@mui/lab";
import {
    useFetchEmployeeAdvancesQuery,
    useFetchEmployeeQuery,
} from "../../../../app/store/apis";
import { useAppSelector } from "../../../../app/store/configureStore";
import { InvoiceItem } from "../../../../app/models/accounting/invoiceItem";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { EmployeeAdvance } from "../../../../app/models/humanResources/employeeAdvance";
import {
    useFetchInvoiceItemTypesByInvoiceIdQuery,
    useAddInvoiceItemMutation,
    useUpdateInvoiceItemsMutation,
    useDeleteInvoiceItemMutation,
    useFetchInvoiceItemsQuery
} from "../../../../app/store/apis/invoice/invoiceItemsApi";
import { toast } from "react-toastify";

interface Props {
    invoiceItem?: InvoiceItem; // Existing item if editing (but we'll show all types)
    editMode: number; // 1 for create, 2 for edit
    onClose: () => void;
    invoiceId: string;
    refreshTotal?: () => Promise<void>;
    employeeId: string;
    invoiceDate: string;
}

interface PayrollRow {
    invoiceItemTypeId: string;
    description: string; // Label from type
    amount: number;
    invoiceItemSeqId?: string; // If already on invoice
    itemDescription?: string; // Custom description for item
    absenceDays?: number; // Special case for PAYROL_DD_ABSENCE
}

const EditPayrollInvoiceItem: React.FC<Props> = ({
    editMode,
    onClose,
    invoiceId,
    refreshTotal,
    employeeId,
    invoiceDate
}) => {
    const { user } = useAppSelector((state) => state.account);
    const [isSaving, setIsSaving] = useState(false);
    const companyId = user?.organizationPartyId || "Company";
    const { getTranslatedLabel } = useTranslationHelper();

    // Queries
    const { data: invoiceItemTypes, isLoading: isTypesLoading } = useFetchInvoiceItemTypesByInvoiceIdQuery(
        { invoiceId },
        { skip: !invoiceId }
    );

    const { data: currentInvoiceItems, isLoading: isItemsLoading } = useFetchInvoiceItemsQuery(
        invoiceId,
        { skip: !invoiceId }
    );

    const { data: employeeData, isLoading: isEmployeeLoading } = useFetchEmployeeQuery(employeeId, { skip: !employeeId });

    const { data: employeeAdvancesData, isLoading: isAdvancesLoading } = useFetchEmployeeAdvancesQuery({
        filter: {
            logic: "and",
            filters: [
                { field: "partyId", operator: "eq", value: employeeId },
                { field: "statusId", operator: "eq", value: "ADVANCE_APPROVED" }
            ]
        }
    } as any, { skip: !employeeId });

    // Mutations
    const [addItem] = useAddInvoiceItemMutation();
    const [updateItem] = useUpdateInvoiceItemsMutation();
    const [deleteItem] = useDeleteInvoiceItemMutation();

    const [rows, setRows] = useState<PayrollRow[]>([]);

    // Process Advances
    const payrollAdvances = useMemo(() => {
        if (!employeeAdvancesData?.data || !invoiceDate) return [];
        const invDate = new Date(invoiceDate);
        const invMonth = invDate.getMonth();
        const invYear = invDate.getFullYear();

        const filtered = employeeAdvancesData.data.filter((adv: EmployeeAdvance) => {
            if (adv.advanceTypeId === "EMPLOYEE_ADVANCE") {
                const advDate = new Date(adv.advanceDate);
                // Standard advance: deduct in the next month? 
                // Existing logic: return advDate.getMonth() === invMonth - 1 && advDate.getFullYear() === invYear;
                return advDate.getMonth() === invMonth - 1 && advDate.getFullYear() === invYear;
            } else if (adv.advanceTypeId === "EMPLOYEE_LONG_TERM_ADVANCE") {
                // Check if any schedule matches the current invoice month/year
                return adv.schedules?.some(s => {
                    const dueDate = new Date(s.dueDate);
                    return dueDate.getMonth() === invMonth && dueDate.getFullYear() === invYear && s.statusId === "SCHEDULED";
                });
            }
            return false;
        });

        // Map to include the correct amount for this month
        return filtered.map(adv => {
            let amountToDeduct = adv.amount;
            if (adv.advanceTypeId === "EMPLOYEE_LONG_TERM_ADVANCE") {
                const schedule = adv.schedules?.find(s => {
                    const dueDate = new Date(s.dueDate);
                    return dueDate.getMonth() === invMonth && dueDate.getFullYear() === invYear && s.statusId === "SCHEDULED";
                });
                amountToDeduct = schedule ? schedule.scheduledAmount : 0;
            }
            return { ...adv, amountToDeduct };
        });
    }, [employeeAdvancesData, invoiceDate]);

    // Initialize Rows
    useEffect(() => {
        if (!isTypesLoading && !isItemsLoading && !isEmployeeLoading && !isAdvancesLoading && invoiceItemTypes) {
            const initialRows: PayrollRow[] = [];

            // 1. Start with the fixed payroll types
            const sortedTypes = [...invoiceItemTypes].sort((a, b) => {
                if (a.invoiceItemTypeId === "PAYROL_SALARY") return -1;
                if (b.invoiceItemTypeId === "PAYROL_SALARY") return 1;
                return a.description.localeCompare(b.description);
            });

            sortedTypes.forEach(type => {
                // Find if this type is already in current items
                const existingItem = currentInvoiceItems?.find(item => item.invoiceItemTypeId === type.invoiceItemTypeId);

                if (existingItem) {
                    initialRows.push({
                        invoiceItemTypeId: type.invoiceItemTypeId,
                        description: type.description,
                        amount: existingItem.amount || 0,
                        invoiceItemSeqId: existingItem.invoiceItemSeqId,
                        itemDescription: existingItem.description || "",
                        absenceDays: type.invoiceItemTypeId === 'PAYROL_DD_ABSENCE' ? (existingItem.quantity || 0) : undefined
                    });
                } else {
                    // Pre-population logic
                    let initialAmount = 0;
                    let initialDesc = "";

                    if (type.invoiceItemTypeId === "PAYROL_SALARY") {
                        initialAmount = employeeData?.monthlyBaseSalary || 0;
                    } else if (type.invoiceItemTypeId === "PAYROL_DD_ADVANCE") {
                        initialAmount = payrollAdvances.reduce((sum, adv) => sum + adv.amountToDeduct, 0);
                        initialDesc = payrollAdvances.map(adv => `Adv #${adv.advanceId}`).join(", ");
                    }

                    initialRows.push({
                        invoiceItemTypeId: type.invoiceItemTypeId,
                        description: type.description,
                        amount: initialAmount,
                        itemDescription: initialDesc,
                        absenceDays: type.invoiceItemTypeId === 'PAYROL_DD_ABSENCE' ? 0 : undefined
                    });
                }
            });

            setRows(initialRows);
        }
    }, [isTypesLoading, isItemsLoading, isEmployeeLoading, isAdvancesLoading, invoiceItemTypes, currentInvoiceItems, employeeData, payrollAdvances]);

    const roundTo2 = (n: number): number => Math.round(n * 100) / 100;
    
    const handleRowChange = (index: number, field: keyof PayrollRow, value: any) => {
        const newRows = [...rows];
        const updatedRow = { ...newRows[index], [field]: value };

        // Special logic for PAYROL_DD_ABSENCE
        if (field === 'absenceDays' && updatedRow.invoiceItemTypeId === 'PAYROL_DD_ABSENCE') {
            const basicSalary = employeeData?.monthlyBaseSalary || 0;
            const absenceDays = parseFloat(value) || 0;
            updatedRow.amount = roundTo2((basicSalary / 30) * absenceDays);
        }

        newRows[index] = updatedRow;
        setRows(newRows);
    };

    const handleDeleteRow = async (index: number) => {
        const row = rows[index];
        if (row.invoiceItemSeqId) {
            try {
                await deleteItem({ invoiceId, invoiceItemSeqId: row.invoiceItemSeqId }).unwrap();
                toast.success(getTranslatedLabel("accounting.invoices.payroll.removed-item", `Removed ${row.description}`, [row.description]));
            } catch (err) {
                toast.error(getTranslatedLabel("accounting.invoices.payroll.delete-item-server-failed", "Failed to delete item from server"));
                return;
            }
        }
        setRows(rows.filter((_, i) => i !== index));
    };

    const handleSubmit = async () => {
        setIsSaving(true);
        try {
            for (const row of rows) {
                // Only save if amount > 0 or it's an update
                if (row.amount > 0 || row.invoiceItemSeqId) {
                    const payload: any = {
                        invoiceId,
                        invoiceItemTypeId: row.invoiceItemTypeId,
                        amount: row.amount,
                        quantity: row.invoiceItemTypeId === 'PAYROL_DD_ABSENCE' ? (row.absenceDays || 0) : 1,
                        description: row.itemDescription,
                    };

                    if (row.invoiceItemSeqId) {
                        payload.invoiceItemSeqId = row.invoiceItemSeqId;
                        await updateItem(payload).unwrap();
                    } else {
                        await addItem(payload).unwrap();
                    }
                }
            }
            toast.success(getTranslatedLabel("accounting.invoices.payroll.save-success", "Payroll items saved successfully"));
            refreshTotal?.();
            onClose();
        } catch (err) {
            console.error("Failed to save payroll items", err);
            toast.error(getTranslatedLabel("accounting.invoices.payroll.save-failed", "Failed to save payroll items"));
        } finally {
            setIsSaving(false);
        }
    };

    if (isTypesLoading || isItemsLoading || isEmployeeLoading) {
        return (
            <Box display="flex" justifyContent="center" p={5}>
                <CircularProgress />
            </Box>
        );
    }

    return (
        <Box p={3}>
            <Typography variant="h5" gutterBottom sx={{ mb: 3, fontWeight: 'bold', color: 'primary.main' }}>
                {getTranslatedLabel("accounting.invoices.payroll.edit-items", "Payroll Items Management")}
                <Typography variant="subtitle2" color="textSecondary">
                    {getTranslatedLabel("accounting.invoices.payroll.employee", "Employee")}: {employeeData?.firstName} | {getTranslatedLabel("accounting.invoices.payroll.date", "Date")}: {new Date(invoiceDate).toLocaleDateString()}
                </Typography>
            </Typography>

            <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                    <TableHead sx={{ bgcolor: '#f5f5f5' }}>
                        <TableRow>
                            <TableCell sx={{ fontWeight: 'bold', width: '25%' }}>{getTranslatedLabel("accounting.invoices.payroll.item-type", "Item Type")}</TableCell>
                            <TableCell sx={{ fontWeight: 'bold', width: '15%' }}>{getTranslatedLabel("accounting.invoices.payroll.amount", "Amount")} *</TableCell>
                            <TableCell sx={{ fontWeight: 'bold', width: '20%' }}>{getTranslatedLabel("accounting.invoices.payroll.absence-days", "Absence Days")}</TableCell>
                            <TableCell sx={{ fontWeight: 'bold', width: '30%' }}>{getTranslatedLabel("accounting.invoices.payroll.description", "Description")}</TableCell>
                            <TableCell sx={{ fontWeight: 'bold', width: '10%', textAlign: 'center' }}>{getTranslatedLabel("accounting.invoices.payroll.actions", "Actions")}</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {rows.map((row, index) => (
                            <TableRow key={row.invoiceItemTypeId}>
                                <TableCell>
                                    <Typography variant="body2" fontWeight={row.invoiceItemTypeId === 'PAYROL_SALARY' ? 'bold' : 'normal'}>
                                        {row.description}
                                    </Typography>
                                </TableCell>
                                <TableCell>
                                    <TextField
                                        type="number"
                                        size="small"
                                        fullWidth
                                        value={row.amount}
                                        onChange={(e) => handleRowChange(index, 'amount', parseFloat(e.target.value) || 0)}
                                        error={row.amount === 0 && row.invoiceItemTypeId === 'PAYROL_SALARY'}
                                        InputProps={{ inputProps: { min: 0, step: "0.01" } }}
                                        disabled={row.invoiceItemTypeId === 'PAYROL_DD_ABSENCE'} // Disabled as it's computed
                                    />
                                </TableCell>
                                <TableCell>
                                    {row.invoiceItemTypeId === 'PAYROL_DD_ABSENCE' && (
                                        <TextField
                                            type="number"
                                            size="small"
                                            fullWidth
                                            value={row.absenceDays || 0}
                                            onChange={(e) => handleRowChange(index, 'absenceDays', parseFloat(e.target.value) || 0)}
                                            InputProps={{ inputProps: { min: 0, step: "0.5" } }}
                                        />
                                    )}
                                </TableCell>
                                <TableCell>
                                    <TextField
                                        size="small"
                                        fullWidth
                                        value={row.itemDescription || ""}
                                        onChange={(e) => handleRowChange(index, 'itemDescription', e.target.value)}
                                        placeholder={getTranslatedLabel("accounting.invoices.payroll.notes-placeholder", "Notes...")}
                                    />
                                </TableCell>
                                <TableCell sx={{ textAlign: 'center' }}>
                                    {row.invoiceItemTypeId !== 'PAYROL_SALARY' && (
                                        <IconButton
                                            color="error"
                                            size="small"
                                            onClick={() => handleDeleteRow(index)}
                                            title={getTranslatedLabel("accounting.invoices.payroll.remove-item", "Remove item")}
                                        >
                                            <DeleteIcon fontSize="small" />
                                        </IconButton>
                                    )}
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>

            <Box display="flex" justifyContent="flex-end" gap={2} mt={4}>
                <Button onClick={onClose} variant="outlined" color="inherit" size="large">
                    {getTranslatedLabel("accounting.invoices.payroll.cancel", "Cancel")}
                </Button>
                <LoadingButton
                    loading={isSaving}
                    variant="contained"
                    color="primary"
                    size="large"
                    onClick={handleSubmit}
                    sx={{ minWidth: 150 }}
                >
                    {getTranslatedLabel("accounting.invoices.payroll.save-payroll", "Save Payroll")}
                </LoadingButton>
            </Box>

            {payrollAdvances.length > 0 && (
                <Box mt={3}>
                    <Alert severity="info" variant="outlined">
                        <Typography variant="subtitle2" fontWeight="bold">{getTranslatedLabel("accounting.invoices.payroll.advances-deducted", "Advances being deducted:")}</Typography>
                        <Box component="ul" sx={{ m: 0, pl: 2 }}>
                            {payrollAdvances.map(adv => (
                                <li key={adv.advanceId}>
                                    {getTranslatedLabel("accounting.invoices.payroll.advance", "Advance")} #{adv.advanceId} ({adv.advanceTypeId}): <b>{adv.amountToDeduct} {adv.currencyUomId}</b>
                                </li>
                            ))}
                        </Box>
                    </Alert>
                </Box>
            )}
        </Box>
    );
};

export default EditPayrollInvoiceItem;
