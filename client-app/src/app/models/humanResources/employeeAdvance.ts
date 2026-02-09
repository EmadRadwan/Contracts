
export interface EmployeeAdvance {
    advanceId: string;
    employeePartyId: string;
    employeeName: string;           // ← Full employee name from Party.Description
    paymentId?: string | null;
    advanceDate: string;            // ISO string from backend (or Date if you parse it)
    amount: number;
    currencyUomId: string;          // e.g. "EGP"
    installmentCount: number;
    installmentAmount: number;
    startDate: string;              // ISO string
    statusId: string;
    statusDescription: string;      // Translated Arabic/English
    description?: string | null;

    // Audit fields (usually returned by backend)
    createdStamp: string;
    lastUpdatedStamp: string;

    // Optional: if you later want to include schedules or more details
    // employeeAdvanceSchedules?: EmployeeAdvanceScheduleRecord[];
}