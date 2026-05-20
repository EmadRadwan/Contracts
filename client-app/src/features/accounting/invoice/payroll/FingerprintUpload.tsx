import React from 'react';
import { Button, Box, Typography } from '@mui/material';
import * as XLSX from 'xlsx';
import { toast } from 'react-toastify';
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

interface FingerprintUploadProps {
    employees: {
        employeeId: string;
        fingerPrintAttendanceId?: string;
        attendanceStartsAt?: string;
    }[];
    onCalculated: (results: { [employeeId: string]: number }) => void;
}

const FingerprintUpload: React.FC<FingerprintUploadProps> = ({ employees, onCalculated }) => {
    const { getTranslatedLabel } = useTranslationHelper();

    const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = (evt) => {
            try {
                const bstr = evt.target?.result;
                const wb = XLSX.read(bstr, { type: 'binary', cellDates: true });
                const wsname = wb.SheetNames[0];
                const ws = wb.Sheets[wsname];
                const data = XLSX.utils.sheet_to_json(ws, { header: 'A' }) as any[];

                // Row 1 is header, so skip it
                const rows = data.slice(1);

                const absenceMap: { [employeeId: string]: number } = {};

                // Group by employee and date
                const empRecords: { [fpId: string]: { [date: string]: Date[] } } = {};

                rows.forEach(row => {
                    const fpId = row.A;
                    const timestampVal = row.D;
                    const type = typeof row.E === 'string' ? row.E.trim() : row.E;

                    if (type === 'حضور' && fpId && timestampVal) {
                        let timestamp: Date;
                        
                        if (timestampVal instanceof Date) {
                            timestamp = timestampVal;
                        } else if (typeof timestampVal === 'string') {
                            // Format: "26/04/2026 9:27:00 AM" or "26/04/2026 16:38"
                            const parts = timestampVal.trim().split(/\s+/);
                            if (parts.length >= 2) {
                                const datePart = parts[0];
                                const timePart = parts[1];
                                const ampm = parts[2]; // AM, PM, or extra noise like A4P4

                                const dateParts = datePart.split('/');
                                if (dateParts.length === 3) {
                                    const day = parseInt(dateParts[0], 10);
                                    const month = parseInt(dateParts[1], 10);
                                    const year = parseInt(dateParts[2], 10);
                                    
                                    const timeParts = timePart.split(':');
                                    let hours = parseInt(timeParts[0], 10) || 0;
                                    const minutes = parseInt(timeParts[1], 10) || 0;
                                    const seconds = parseInt(timeParts[2], 10) || 0;
                                    
                                    if (ampm === 'PM' && hours < 12) hours += 12;
                                    if (ampm === 'AM' && hours === 12) hours = 0;
                                    
                                    timestamp = new Date(year, month - 1, day, hours, minutes, seconds);
                                } else {
                                    timestamp = new Date(timestampVal);
                                }
                            } else {
                                timestamp = new Date(timestampVal);
                            }
                        } else {
                            return;
                        }

                        if (isNaN(timestamp.getTime())) return;

                        // Use a consistent date key YYYY-MM-DD
                        const year = timestamp.getFullYear();
                        const month = String(timestamp.getMonth() + 1).padStart(2, '0');
                        const day = String(timestamp.getDate()).padStart(2, '0');
                        const dateKey = `${year}-${month}-${day}`;

                        const fpIdStr = String(fpId).trim();
                        if (!empRecords[fpIdStr]) empRecords[fpIdStr] = {};
                        if (!empRecords[fpIdStr][dateKey]) empRecords[fpIdStr][dateKey] = [];
                        empRecords[fpIdStr][dateKey].push(timestamp);
                    }
                });

                employees.forEach(emp => {
                    if (!emp.fingerPrintAttendanceId || !emp.attendanceStartsAt) return;

                    const fpIdStr = String(emp.fingerPrintAttendanceId);
                    const records = empRecords[fpIdStr];
                    if (!records) return;

                    let totalAbsence = 0;
                    
                    // Parse attendanceStartsAt "10:00:00"
                    const timeParts = String(emp.attendanceStartsAt).split(':').map(Number);
                    const startH = timeParts[0] || 0;
                    const startM = timeParts[1] || 0;
                    const startS = timeParts[2] || 0;

                    Object.keys(records).forEach(dateKey => {
                        const dailyCheckins = records[dateKey];
                        // Find earliest check-in
                        const earliest = new Date(Math.min(...dailyCheckins.map(d => d.getTime())));

                        // Target time for this day
                        const target = new Date(earliest);
                        target.setHours(startH, startM, startS, 0);

                        const diffMs = earliest.getTime() - target.getTime();
                        if (diffMs > 0) {
                            const diffHours = diffMs / (1000 * 60 * 60);
                            if (diffHours >= 1) {
                                totalAbsence += 0.5;
                            } else {
                                totalAbsence += 0.25;
                            }
                        }
                    });

                    absenceMap[emp.employeeId] = totalAbsence;
                });

                onCalculated(absenceMap);
            } catch (error) {
                console.error("Error processing fingerprint file:", error);
                toast.error(getTranslatedLabel("accounting.payroll.run.upload-error", "Error processing file. Please ensure it is a valid Excel file with the expected format."));
            }
        };
        reader.readAsBinaryString(file);
    };

    return (
        <Box mb={2} p={2} sx={{ border: '1px dashed #ccc', borderRadius: 1, backgroundColor: '#fafafa' }}>
            <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                {getTranslatedLabel("accounting.payroll.run.fingerprint-upload-title", "Employee Fingerprint Attendance (Excel)")}
            </Typography>
            <Box display="flex" alignItems="center" gap={2}>
                <Button
                    variant="outlined"
                    component="label"
                    size="small"
                >
                    {getTranslatedLabel("accounting.payroll.run.upload-fingerprint", "Upload Fingerprint Data")}
                    <input
                        type="file"
                        hidden
                        accept=".xlsx, .xls"
                        onChange={handleFileUpload}
                    />
                </Button>
               {/* <Typography variant="caption" color="textSecondary">
                    {getTranslatedLabel("accounting.payroll.run.upload-hint", "Match by FingerprintAttendanceId, column A (ID), D (Time), E (Type: حضور)")}
                </Typography>*/}
            </Box>
        </Box>
    );
};

export default FingerprintUpload;
