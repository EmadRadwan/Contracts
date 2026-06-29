
export interface EmployeeAdvance {
    advanceId: string;
    advanceTypeId: string;
    partyId: string;
    employeeName: string;           // ← Full employee name from Party.Description
    paymentId?: string | null;
    paymentStatusId?: string | null;
    advanceDate: string;            // ISO string from backend (or Date if you parse it)
    amount: number;
    currencyUomId: string;          // e.g. "EGP"
    installmentCount: number;
    installmentAmount: number;
    startDate: string;              // ISO string
    statusId: string;
    statusDescription: string;      // Translated Arabic/English
    description?: string | null;
    payrollInvoiceId?: string | null;

    // Audit fields (usually returned by backend)
    createdStamp: string;
    lastUpdatedStamp: string;

    schedules?: Schedule[];
}

// app/models/humanResources/employeeAdvance.ts  (extend existing type)

export interface Schedule {
    scheduleId: string;
    installmentNumber: number;
    dueDate: string;           // ISO date string "2025-04-15"
    scheduledAmount: number;
    deductedAmount: number;
    statusId?: string;
    payrollInvoiceId?: string;
    notes?: string;
}

export interface EmployeeAdvanceDetail extends EmployeeAdvance {
    // fields already in your form model
    schedules: Schedule[];
    // optionally add
    isCustomPlan?: boolean;   // you can derive from advanceTypeId
}