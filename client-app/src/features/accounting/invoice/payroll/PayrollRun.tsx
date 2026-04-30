import React, { useState, useMemo, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
    Box,
    Typography,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableFooter,
    TableHead,
    TableRow,
    TextField,
    CircularProgress,
    Button
} from "@mui/material";
import { LoadingButton } from "@mui/lab";
import { toast } from "react-toastify";
import { Alert, AlertTitle } from "@mui/material";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {
    useAppSelector,
    useBatchCreatePayrollInvoicesMutation, useCheckExistingPayrollInvoicesQuery,
    useDeletePayrollInvoicesMutation
} from "../../../../app/store/configureStore";
import {
    useFetchEmployeesWithSalaryQuery,
    useFetchPayrollAdvancesQuery
} from "../../../../app/store/apis";
import { EmployeeAdvance } from "../../../../app/models/humanResources/employeeAdvance";

interface EmployeePayrollData {
    employeeId: string;
    name: string;
    baseSalary: number;
    salaryAccountNameArabic: string;
    glAccountIdAdvancedPayment: string;
    advancedPaymentAccountNameArabic: string;
    preferredPayrollPaymentMethodId: string;
    absenceDays: number;
    absenceValue: number;
    overtimeDays: number;
    overtimeValue: number;
    netSalary: number;
    isSelected: boolean;
    advances: {
        advanceId: string;
        advanceTypeId: string;
        amount: number;
    }[];
}

const PayrollRun: React.FC = () => {
    const navigate = useNavigate();
    const { getTranslatedLabel } = useTranslationHelper();
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "Company";
    
    const [invoiceDate, setInvoiceDate] = useState<string>(new Date().toISOString().split('T')[0]);
    const [payrollData, setPayrollData] = useState<EmployeePayrollData[]>([]);

    const totals = useMemo(() => {
        return payrollData.reduce((acc, emp) => {
            if (emp.isSelected) {
                acc.baseSalary += emp.baseSalary;
                acc.advances += emp.advances.reduce((sum, adv) => sum + adv.amount, 0);
                acc.absenceValue += emp.absenceValue;
                acc.overtimeValue += emp.overtimeValue;
                acc.netSalary += emp.netSalary;
            }
            return acc;
        }, {
            baseSalary: 0,
            advances: 0,
            absenceValue: 0,
            overtimeValue: 0,
            netSalary: 0
        });
    }, [payrollData]);

    // Fetch Employees with Salary
    const { data: employeesResponse, isLoading: isEmployeesLoading } = useFetchEmployeesWithSalaryQuery();

    // Fetch Payroll-specific Advances
    const { data: advancesResponse, isLoading: isAdvancesLoading  } = useFetchPayrollAdvancesQuery({
        invoiceDate,
        organizationPartyId: companyId
    }, {
        skip: !invoiceDate || !companyId,
        refetchOnMountOrArgChange: true
    });

    const [batchCreate, { isLoading: isSubmitting }] = useBatchCreatePayrollInvoicesMutation();
    const [deleteInvoices, { isLoading: isDeleting }] = useDeletePayrollInvoicesMutation();

    // Check for draft in localStorage
    // Load draft - but only if no data yet (runs first)
    useEffect(() => {
        const savedDraft = localStorage.getItem(`payroll_draft_${companyId}`);
        if (savedDraft) {
            try {
                const { invoiceDate: savedDate, data: savedData } = JSON.parse(savedDraft);
                if (savedDate === invoiceDate) {
                    setPayrollData(savedData);   // This is fine as initial load
                }
            } catch (e) {
                console.error("Failed to parse payroll draft", e);
            }
        }
    }, [companyId, invoiceDate]);

    const handleSaveDraft = () => {
        localStorage.setItem(`payroll_draft_${companyId}`, JSON.stringify({
            invoiceDate,
            data: payrollData
        }));
        toast.info(getTranslatedLabel("accounting.payroll.run.draft-saved", "Draft saved locally"));
    };

    // Check for existing invoices for this month
    const { data: hasExistingInvoices, isLoading: isCheckingExisting } = useCheckExistingPayrollInvoicesQuery({
        invoiceDate,
        organizationPartyId: companyId
    }, { skip: !invoiceDate || !companyId });

    useEffect(() => {
        if (!employeesResponse?.data || !advancesResponse?.data || !invoiceDate) return;

        const initialData = employeesResponse.data.map((emp: any) => {
            const baseSalary = emp.monthlyBaseSalary || 0;

            const empAdvances = advancesResponse.data
                .filter((adv: EmployeeAdvance) => adv.partyId === emp.partyId)
                .map((adv: EmployeeAdvance) => {
                    let amountToDeduct = 0;

                    if (adv.advanceTypeId === "EMPLOYEE_LONG_TERM_ADVANCE") {
                        const schedule = adv.schedules?.find(s =>
                            s.dueDate && s.dueDate.startsWith(invoiceDate.substring(0, 7))
                        ) || adv.schedules?.[0];

                        amountToDeduct = schedule?.scheduledAmount || 0;
                    } else {
                        amountToDeduct = adv.amount || 0;
                    }

                    return {
                        advanceId: adv.advanceId,
                        advanceTypeId: adv.advanceTypeId,
                        amount: Math.round(amountToDeduct)
                    };
                })
                .filter(a => a.amount > 0);

            return {
                employeeId: emp.partyId,
                name: emp.name,
                baseSalary: baseSalary,
                salaryAccountNameArabic: emp.salaryAccountNameArabic || "",
                glAccountIdAdvancedPayment: emp.glAccountIdAdvancedPayment || "",
                advancedPaymentAccountNameArabic: emp.advancedPaymentAccountNameArabic || "",
                preferredPayrollPaymentMethodId: emp.preferredPayrollPaymentMethodId || "",
                absenceDays: 0,
                absenceValue: 0,
                overtimeDays: 0,
                overtimeValue: 0,
                netSalary: baseSalary - empAdvances.reduce((sum, adv) => sum + adv.amount, 0),
                isSelected: baseSalary > 0 && !!(emp.advancedPaymentAccountNameArabic || emp.salaryAccountNameArabic),
                advances: empAdvances
            };
        });

        setPayrollData(prev => {
            // No previous data → use fresh data
            if (prev.length === 0) return initialData;

            // Merge: Keep user changes (absence, overtime, selection), refresh base + advances
            return initialData.map(freshEmp => {
                const oldEmp = prev.find(o => o.employeeId === freshEmp.employeeId);

                if (!oldEmp) return freshEmp;

                // Recalculate netSalary with possibly updated advances
                const totalAdvances = freshEmp.advances.reduce((sum, adv) => sum + adv.amount, 0);
                const newNetSalary = freshEmp.baseSalary
                    + oldEmp.overtimeValue
                    - oldEmp.absenceValue
                    - totalAdvances;

                return {
                    ...freshEmp,
                    absenceDays: oldEmp.absenceDays,
                    absenceValue: oldEmp.absenceValue,
                    overtimeDays: oldEmp.overtimeDays,
                    overtimeValue: oldEmp.overtimeValue,
                    isSelected: oldEmp.isSelected,
                    netSalary: Math.round(newNetSalary)
                };
            });
        });

    }, [employeesResponse, advancesResponse, invoiceDate]);
    
    const handleCalculate = (index: number, type: 'absence' | 'overtime') => {
        const newData = [...payrollData];
        const row = { ...newData[index] };
        
        if (type === 'absence') {
            row.absenceValue = Math.round((row.baseSalary / 30) * row.absenceDays);
        } else {
            row.overtimeValue = Math.round((row.baseSalary / 30) * row.overtimeDays);
        }

        const totalAdvances = row.advances.reduce((sum, adv) => sum + adv.amount, 0);
        row.netSalary = row.baseSalary + row.overtimeValue - row.absenceValue - totalAdvances;

        newData[index] = row;
        setPayrollData(newData);
    };

    const handleDataChange = (index: number, field: keyof EmployeePayrollData, value: any) => {
        const newData = [...payrollData];
        newData[index] = { ...newData[index], [field]: value };
        setPayrollData(newData);
    };

    const handleSelectAll = (checked: boolean) => {
        setPayrollData(prev => prev.map(emp => ({ ...emp, isSelected: checked })));
    };

    const isEmployeeInvalid = (emp: EmployeePayrollData) => {
        const hasAccount = emp.advancedPaymentAccountNameArabic || emp.salaryAccountNameArabic;
        return emp.baseSalary <= 0 || !hasAccount;
    };

    const isAnyEmployeeInvalid = useMemo(() => {
        return payrollData.some(emp => emp.isSelected && isEmployeeInvalid(emp));
    }, [payrollData]);

    const handleSubmit = async () => {
        const selectedEmployees = payrollData.filter(emp => emp.isSelected);
        if (selectedEmployees.length === 0) {
            toast.warning(getTranslatedLabel("accounting.payroll.run.no-employees-selected", "No employees selected"));
            return;
        }
        
        if (selectedEmployees.some(emp => isEmployeeInvalid(emp))) {
            toast.error(getTranslatedLabel("accounting.payroll.run.invalid-data", "Please fix invalid employee data before proceeding"));
            return;
        }
        try {
            const command = {
                employees: selectedEmployees.map(emp => ({
                    employeeId: emp.employeeId,
                    baseSalary: emp.baseSalary,
                    absenceDays: emp.absenceDays,
                    absenceValue: emp.absenceValue,
                    overtimeDays: emp.overtimeDays,
                    overtimeValue: emp.overtimeValue,
                    advances: emp.advances
                })),
                invoiceDate: invoiceDate,
                organizationPartyId: companyId
            };

            await batchCreate(command).unwrap();
            localStorage.removeItem(`payroll_draft_${companyId}`);
            toast.success(getTranslatedLabel("accounting.payroll.run.success", "Payroll Run completed successfully"));
            navigate("/invoicesDashboard");
        } catch (error) {
            console.error("Payroll Run failed", error);
            toast.error(getTranslatedLabel("accounting.payroll.run.failed", "Payroll Run failed"));
        }
    };

    const handleDeleteExisting = async () => {
        if (!window.confirm(getTranslatedLabel("accounting.payroll.run.confirm-delete", "Are you sure you want to delete existing invoices for this month? This action cannot be undone."))) {
            return;
        }

        try {
            await deleteInvoices({
                invoiceDate,
                organizationPartyId: companyId
            }).unwrap();
            toast.success(getTranslatedLabel("accounting.payroll.run.delete-success", "Existing payroll invoices deleted successfully"));
            // Refreshing happens automatically via tag invalidation in RTK Query
        } catch (error) {
            console.error("Failed to delete payroll invoices", error);
            toast.error(getTranslatedLabel("accounting.payroll.run.delete-failed", "Failed to delete payroll invoices"));
        }
    };

    if (isEmployeesLoading || isAdvancesLoading) return <CircularProgress />;

    console.log("Payroll Data:", payrollData);
    return (
        <Box p={3}>
            <Typography variant="h4" gutterBottom>
                {getTranslatedLabel("accounting.menu.payrollRun", "Payroll Run")}
            </Typography>

            {isAnyEmployeeInvalid && (
                <Box mb={3}>
                    <Alert severity="error">
                        <AlertTitle>{getTranslatedLabel("accounting.payroll.run.invalid-data", "Invalid Data")}</AlertTitle>
                        {getTranslatedLabel("accounting.payroll.run.invalid-data-warning", "Cannot proceed: Some employees have incomplete data (Salary is 0 or Salary Account is missing). Please fix the data first.")}
                    </Alert>
                </Box>
            )}

            {!isCheckingExisting && hasExistingInvoices && (
                <Box mb={3}>
                    <Alert 
                        severity="warning"
                        action={
                            <LoadingButton 
                                color="inherit" 
                                size="small" 
                                onClick={handleDeleteExisting}
                                loading={isDeleting}
                                variant="outlined"
                            >
                                {getTranslatedLabel("accounting.payroll.run.delete-existing", "Delete Existing Invoices")}
                            </LoadingButton>
                        }
                    >
                        <AlertTitle>{getTranslatedLabel("accounting.payroll.run.existing-invoices", "Existing Payroll Invoices Detected")}</AlertTitle>
                        {getTranslatedLabel("accounting.payroll.run.existing-invoices-warning", "Payroll invoices already exist for the selected month. Proceeding will delete these invoices and all related data (Absence, Overtime, etc.). You will need to re-enter this information.")}
                    </Alert>
                </Box>
            )}

            <Box mb={3} display="flex" alignItems="center" gap={2}>
                <TextField
                    label={getTranslatedLabel("accounting.invoices.display.form.invoice-date", "Invoice Date")}
                    type="date"
                    value={invoiceDate}
                    onChange={(e) => setInvoiceDate(e.target.value)}
                    InputLabelProps={{ shrink: true }}
                />
                <Button variant="outlined" onClick={handleSaveDraft}>
                    {getTranslatedLabel("accounting.payroll.run.save-draft", "Save as Draft")}
                </Button>
            </Box>

            <TableContainer component={Paper}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            <TableCell>
                                <Box display="flex" alignItems="center" gap={1}>
                                    <input 
                                        type="checkbox" 
                                        checked={payrollData.length > 0 && payrollData.every(emp => emp.isSelected)}
                                        onChange={(e) => handleSelectAll(e.target.checked)}
                                    />
                                    {getTranslatedLabel("accounting.payroll.run.included", "Incl.")}
                                </Box>
                            </TableCell>
                            <TableCell>{getTranslatedLabel("accounting.payroll.run.employee-name", "Employee Name")}</TableCell>
                            <TableCell align="center">{getTranslatedLabel("accounting.payroll.run.base-salary", "Base Salary")}</TableCell>
                            <TableCell>{getTranslatedLabel("accounting.payroll.run.salary-account", "Salary Account")}</TableCell>
                            <TableCell>{getTranslatedLabel("accounting.payroll.run.payment-method", "Payment Method")}</TableCell>
                            <TableCell align="center">{getTranslatedLabel("accounting.payroll.run.absence-days", "Absence Days")}</TableCell>
                            <TableCell align="center">{getTranslatedLabel("accounting.payroll.run.absence-value", "Absence Value")}</TableCell>
                            <TableCell align="center">{getTranslatedLabel("accounting.payroll.run.overtime-days", "Overtime Days")}</TableCell>
                            <TableCell align="center">{getTranslatedLabel("accounting.payroll.run.overtime-value", "Overtime Value")}</TableCell>
                            <TableCell align="center">{getTranslatedLabel("accounting.payroll.run.net-salary", "Net Salary")}</TableCell>
                            <TableCell>{getTranslatedLabel("accounting.payroll.run.advances", "Advances")}</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {payrollData.map((emp, index) => {
                            const invalid = isEmployeeInvalid(emp);
                            return (
                                <TableRow key={emp.employeeId} sx={{ opacity: (!emp.isSelected || invalid) ? 0.5 : 1 }}>
                                    <TableCell>
                                        <input 
                                            type="checkbox" 
                                            checked={emp.isSelected} 
                                            onChange={(e) => handleDataChange(index, 'isSelected', e.target.checked)}
                                        />
                                    </TableCell>
                                    <TableCell>{emp.name}</TableCell>
                                    <TableCell align="center">{emp.baseSalary}</TableCell>
                                    <TableCell>{emp.advancedPaymentAccountNameArabic || emp.salaryAccountNameArabic}</TableCell>
                                    <TableCell>
                                        {emp.preferredPayrollPaymentMethodId === "BANK_TRANSFER" 
                                            ? getTranslatedLabel("accounting.payroll.run.bank-transfer", "Bank Transfer")
                                            : emp.preferredPayrollPaymentMethodId === "CASH"
                                            ? getTranslatedLabel("accounting.payroll.run.cash", "Cash")
                                            : emp.preferredPayrollPaymentMethodId
                                        }
                                    </TableCell>
                                    <TableCell>
                                        <Box display="flex" alignItems="center" gap={1} justifyContent="center">
                                            <TextField
                                                type="number"
                                                size="small"
                                                value={emp.absenceDays}
                                                onChange={(e) => handleDataChange(index, 'absenceDays', parseFloat(e.target.value) || 0)}
                                                inputProps={{ min: 0, step: 0.5 }}
                                                sx={{ width: '60px' }}
                                                disabled={invalid}
                                            />
                                            <Button 
                                                variant="outlined" 
                                                size="small" 
                                                onClick={() => handleCalculate(index, 'absence')}
                                                sx={{ minWidth: 'unset', px: 1 }}
                                                disabled={invalid}
                                            >
                                                {getTranslatedLabel("accounting.payroll.run.calculate", "Calc")}
                                            </Button>
                                        </Box>
                                    </TableCell>
                                    <TableCell align="center">{emp.absenceValue}</TableCell>
                                    <TableCell>
                                        <Box display="flex" alignItems="center" gap={1} justifyContent="center">
                                            <TextField
                                                type="number"
                                                size="small"
                                                value={emp.overtimeDays}
                                                onChange={(e) => handleDataChange(index, 'overtimeDays', parseFloat(e.target.value) || 0)}
                                                inputProps={{ min: 0, step: 0.5 }}
                                                sx={{ width: '60px' }}
                                                disabled={invalid}
                                            />
                                            <Button 
                                                variant="outlined" 
                                                size="small" 
                                                onClick={() => handleCalculate(index, 'overtime')}
                                                sx={{ minWidth: 'unset', px: 1 }}
                                                disabled={invalid}
                                            >
                                                {getTranslatedLabel("accounting.payroll.run.calculate", "Calc")}
                                            </Button>
                                        </Box>
                                    </TableCell>
                                    <TableCell align="center">{emp.overtimeValue}</TableCell>
                                    <TableCell align="center" sx={{ fontWeight: 'bold' }}>{emp.netSalary}</TableCell>
                                    <TableCell>
                                        {emp.advances.map(a => (
                                            <div key={a.advanceId} style={{ whiteSpace: 'nowrap', fontSize: '0.8rem' }}>
                                                {a.advanceId}: {a.amount}
                                            </div>
                                        ))}
                                    </TableCell>
                                </TableRow>
                            );
                        })}
                    </TableBody>
                    <TableFooter>
                        <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
                            <TableCell colSpan={2} align="right" sx={{ fontWeight: 'bold' }}>
                                {getTranslatedLabel("accounting.payroll.run.total", "Total")}
                            </TableCell>
                            <TableCell align="center" sx={{ fontWeight: 'bold' }}>{totals.baseSalary}</TableCell>
                            <TableCell colSpan={3} />
                            <TableCell align="center" sx={{ fontWeight: 'bold' }}>{totals.absenceValue}</TableCell>
                            <TableCell align="center" />
                            <TableCell align="center" sx={{ fontWeight: 'bold' }}>{totals.overtimeValue}</TableCell>
                            <TableCell align="center" sx={{ fontWeight: 'bold' }}>{totals.netSalary}</TableCell>
                            <TableCell sx={{ fontWeight: 'bold' }}>{totals.advances}</TableCell>
                        </TableRow>
                    </TableFooter>
                </Table>
            </TableContainer>

            <Box mt={3} display="flex" justifyContent="flex-end">
                <LoadingButton
                    variant="contained"
                    color="primary"
                    onClick={handleSubmit}
                    loading={isSubmitting}
                    disabled={isAnyEmployeeInvalid}
                >
                    {getTranslatedLabel("accounting.payroll.run.submit", "Run Payroll")}
                </LoadingButton>
            </Box>
        </Box>
    );
};

export default PayrollRun;
