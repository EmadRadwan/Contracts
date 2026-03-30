import React, { useState, useMemo, useEffect } from "react";
import {
    Box,
    Typography,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
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
import {useAppSelector, useBatchCreatePayrollInvoicesMutation} from "../../../../app/store/configureStore";
import {
    useFetchEmployeesWithSalaryQuery,
    useFetchEmployeeAdvancesQuery
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
    const { getTranslatedLabel } = useTranslationHelper();
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "Company";
    
    const [invoiceDate, setInvoiceDate] = useState<string>(new Date().toISOString().split('T')[0]);
    const [payrollData, setPayrollData] = useState<EmployeePayrollData[]>([]);

    // Fetch Employees with Salary
    const { data: employeesResponse, isLoading: isEmployeesLoading } = useFetchEmployeesWithSalaryQuery();

    // Fetch All Approved Advances
    const { data: advancesResponse, isLoading: isAdvancesLoading } = useFetchEmployeeAdvancesQuery({
        filter: {
            field: "statusId", operator: "eq", value: "ADVANCE_APPROVED"
        }
    } as any);

    const [batchCreate, { isLoading: isSubmitting }] = useBatchCreatePayrollInvoicesMutation();

    // Process initial data
    useEffect(() => {
        if (employeesResponse?.data && advancesResponse?.data && invoiceDate) {
            const invDate = new Date(invoiceDate);
            const invMonth = invDate.getMonth();
            const invYear = invDate.getFullYear();

            const initialData = employeesResponse.data.map((emp: any) => {
                const baseSalary = emp.monthlyBaseSalary || 0;
                
                // Calculate Advances for this employee for this month
                const empAdvances = advancesResponse.data.filter((adv: EmployeeAdvance) => {
                    if (adv.partyId !== emp.partyId) return false;
                    
                    if (adv.advanceTypeId === "EMPLOYEE_ADVANCE") {
                        const advDate = new Date(adv.advanceDate);
                        return advDate.getMonth() === invMonth - 1 && advDate.getFullYear() === invYear;
                    } else if (adv.advanceTypeId === "EMPLOYEE_LONG_TERM_ADVANCE") {
                        return adv.schedules?.some(s => {
                            const dueDate = new Date(s.dueDate);
                            return dueDate.getMonth() === invMonth && dueDate.getFullYear() === invYear && s.statusId === "SCHEDULED";
                        });
                    }
                    return false;
                }).map((adv: EmployeeAdvance) => {
                    let amountToDeduct = adv.amount;
                    if (adv.advanceTypeId === "EMPLOYEE_LONG_TERM_ADVANCE") {
                        const schedule = adv.schedules?.find(s => {
                            const dueDate = new Date(s.dueDate);
                            return dueDate.getMonth() === invMonth && dueDate.getFullYear() === invYear && s.statusId === "SCHEDULED";
                        });
                        amountToDeduct = schedule ? schedule.scheduledAmount : 0;
                    }
                    return {
                        advanceId: adv.advanceId,
                        advanceTypeId: adv.advanceTypeId,
                        amount: amountToDeduct
                    };
                });

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
                    isSelected: baseSalary > 0 && (emp.advancedPaymentAccountNameArabic || emp.salaryAccountNameArabic),
                    advances: empAdvances
                };
            });
            setPayrollData(initialData);
        }
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
            toast.success(getTranslatedLabel("accounting.payroll.run.success", "Payroll Run completed successfully"));
        } catch (error) {
            console.error("Payroll Run failed", error);
            toast.error(getTranslatedLabel("accounting.payroll.run.failed", "Payroll Run failed"));
        }
    };

    if (isEmployeesLoading || isAdvancesLoading) return <CircularProgress />;

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

            <Box mb={3} display="flex" alignItems="center" gap={2}>
                <TextField
                    label={getTranslatedLabel("accounting.invoices.display.form.invoice-date", "Invoice Date")}
                    type="date"
                    value={invoiceDate}
                    onChange={(e) => setInvoiceDate(e.target.value)}
                    InputLabelProps={{ shrink: true }}
                />
            </Box>

            <TableContainer component={Paper}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            <TableCell>{getTranslatedLabel("accounting.payroll.run.included", "Incl.")}</TableCell>
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
