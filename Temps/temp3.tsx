// Process initial data from API + merge with existing draft/user changes
useEffect(() => {
    if (!employeesResponse?.data || !advancesResponse?.data || !invoiceDate) return;

    const initialData: EmployeePayrollData[] = employeesResponse.data.map((emp: any) => {
        const baseSalary = emp.monthlyBaseSalary || 0;

        // Calculate Advances
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

    // Merge fresh data with previous user modifications
    setPayrollData(prev => {
        // First time loading → use fresh data
        if (prev.length === 0) {
            return initialData;
        }

        // Merge logic: Keep user changes (absence, overtime, selection), refresh base data + advances
        return initialData.map((freshEmp) => {
            const oldEmp = prev.find((o) => o.employeeId === freshEmp.employeeId);

            // If employee is new (not in previous state), use fresh data
            if (!oldEmp) {
                return freshEmp;
            }

            // Recalculate absence and overtime values based on possibly new baseSalary
            const calculatedAbsenceValue = Math.round((freshEmp.baseSalary / 30) * oldEmp.absenceDays);
            const calculatedOvertimeValue = Math.round((freshEmp.baseSalary / 30) * oldEmp.overtimeDays);

            const totalAdvances = freshEmp.advances.reduce((sum, adv) => sum + adv.amount, 0);

            const newNetSalary =
                freshEmp.baseSalary +
                calculatedOvertimeValue -
                calculatedAbsenceValue -
                totalAdvances;

            return {
                ...freshEmp,                    // Fresh baseSalary, advances, accounts, etc.
                absenceDays: oldEmp.absenceDays,     // Keep user input
                absenceValue: calculatedAbsenceValue,
                overtimeDays: oldEmp.overtimeDays,   // Keep user input
                overtimeValue: calculatedOvertimeValue,
                isSelected: oldEmp.isSelected,       // Keep user selection
                netSalary: Math.round(newNetSalary)
            };
        });
    });

}, [employeesResponse, advancesResponse, invoiceDate]);