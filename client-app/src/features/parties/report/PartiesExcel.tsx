// src/features/parties/report/PartiesExcel.tsx
import React, { useCallback, useState } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import { useLazyFetchAllPartiesForReportQuery } from "../../../app/store/apis";

interface PartiesExcelProps {
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? '' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
};

export const PartiesExcel: React.FC<PartiesExcelProps> = ({ getTranslatedLabel }) => {
    const [trigger, { isFetching }] = useLazyFetchAllPartiesForReportQuery();
    const [isGenerating, setIsGenerating] = useState(false);

    const generateExcel = useCallback(async (data: any[]) => {
        if (!data || data.length === 0) return null;

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'Golden Land System';

        // --- FETCH LOGO ---
        let logoBuffer: ArrayBuffer | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) logoBuffer = await resp.blob().then(b => b.arrayBuffer());
        } catch (e) {
            console.warn('Logo not found:', e);
        }

        const ws = workbook.addWorksheet('Parties Report');
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];

        ws.getRow(1).height = logoBuffer ? 75 : 30;

        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
            ws.addImage(imageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 120, height: 100 },
            });
        } else {
            ws.getCell('A1').value = 'Golden Land';
            ws.getCell('A1').font = { name: 'Amiri', size: 16, bold: true };
        }

        const startRow = logoBuffer ? 5 : 3;

        // === TITLE ===
        const title = utils.rtlEmbed(getTranslatedLabel('party.parties.report.allParties.title', 'All Parties Report'));
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:F${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(startRow).height = 40;

        // === HEADER ===
        const headerRowNum = startRow + 2;
        const headers = [
            getTranslatedLabel('party.parties.list.partyId', 'Party ID'),
            getTranslatedLabel('party.parties.list.description', 'Party Name'),
            getTranslatedLabel('party.parties.list.mainRole', 'Main Role'),
            getTranslatedLabel('party.parties.list.contactNumber', 'Phone Number'),
            getTranslatedLabel('accounting.glAccount.id', 'GL Account ID'),
            getTranslatedLabel('accounting.glAccount.nameArabic', 'GL Account Name (Arabic)'),
        ];

        ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

        // === DATA ROWS ===
        data.forEach((party) => {
            if (party.glAccounts && party.glAccounts.length > 0) {
                party.glAccounts.forEach((gl: any, idx: number) => {
                    const row = ws.addRow([
                        idx === 0 ? utils.safeString(party.partyId) : '',
                        idx === 0 ? utils.rtlEmbed(utils.safeString(party.description)) : '',
                        idx === 0 ? utils.safeString(party.mainRole) : '',
                        idx === 0 ? utils.safeString(party.mobileContactNumber) : '',
                        utils.safeString(gl.glAccountId),
                        utils.rtlEmbed(utils.safeString(gl.accountNameArabic)),
                    ]);
                    row.font = { name: 'Amiri', size: 10 };
                    row.alignment = { horizontal: 'right', wrapText: true };
                });
            } else {
                const row = ws.addRow([
                    utils.safeString(party.partyId),
                    utils.rtlEmbed(utils.safeString(party.description)),
                    utils.safeString(party.mainRole),
                    utils.safeString(party.mobileContactNumber),
                    '',
                    '',
                ]);
                row.font = { name: 'Amiri', size: 10 };
                row.alignment = { horizontal: 'right', wrapText: true };
            }
        });

        // === COLUMN WIDTHS ===
        ws.columns = [
            { width: 15 }, // Party ID
            { width: 35 }, // Description
            { width: 15 }, // Main Role
            { width: 20 }, // Mobile
            { width: 20 }, // GL ID
            { width: 40 }, // GL Name Arabic
        ];

        return await workbook.xlsx.writeBuffer();
    }, [getTranslatedLabel]);

    const handleDownload = useCallback(async () => {
        setIsGenerating(true);
        try {
            const result = await trigger().unwrap();
            const buffer = await generateExcel(result);
            if (buffer) {
                const blob = new Blob([buffer], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                });
                saveAs(blob, `Parties_Report_${new Date().toISOString().split('T')[0]}.xlsx`);
            }
        } catch (err) {
            console.error('Excel generation failed:', err);
            alert('Failed to generate report. Please try again.');
        } finally {
            setIsGenerating(false);
        }
    }, [trigger, generateExcel]);

    const isLoading = isFetching || isGenerating;

    return (
        <Button
            variant="contained"
            color="secondary"
            disabled={isLoading}
            onClick={handleDownload}
            sx={{ ml: 1 }}
        >
            {isLoading
                ? getTranslatedLabel('common.generating', 'Generating...')
                : getTranslatedLabel('party.parties.report.exportExcel', "Export Parties Excel")
            }
        </Button>
    );
};
