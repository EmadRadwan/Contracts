import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

// Excel export of the Inventory Valuation report. Same RTL / Amiri / logo house style as the
// other accounting exports.
//
// The logo is tracked by its fetched buffer, not by the id workbook.addImage returns — that id is
// 0 for the first image, which is falsy. See the ExcelJS gotcha in client-app/CLAUDE.md.

export interface InventoryValuationRow {
    productId: string;
    productName: string;
    quantityUomDescription: string;
    unitCost: number;
    currencyUomId: string;
    accountingQuantitySum: number;
    quantityOnHandSum: number;
    value: number;
}

interface Props {
    companyName: string;
    rows: InventoryValuationRow[];
    totalValue: number;
    facilityName?: string;
    thruDate?: string;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
}

const localizationKey = 'accounting.orgGL.reports.inventory-valuation';

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `‫${t}` : t,
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

async function fetchLogoBuffer(): Promise<ArrayBuffer | null> {
    for (let attempt = 1; attempt <= 2; attempt++) {
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) return await (await resp.blob()).arrayBuffer();
            console.warn(`InventoryValuationExcel: logo fetch returned ${resp.status} ${resp.statusText} (attempt ${attempt})`);
        } catch (e) {
            console.warn(`InventoryValuationExcel: logo fetch threw (attempt ${attempt})`, e);
        }
    }
    return null;
}

export const InventoryValuationExcel: React.FC<Props> = ({
    companyName,
    rows,
    totalValue,
    facilityName,
    thruDate,
    getTranslatedLabel,
    isFetching = false,
}) => {
    const generateExcel = useCallback(async () => {
        if (isFetching) {
            console.warn('InventoryValuationExcel: data is still fetching');
            return null;
        }

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'Golden Land System';

        const ws = workbook.addWorksheet('Inventory Valuation', {
            views: [{ rightToLeft: true }],
            pageSetup: { paperSize: 9, orientation: 'landscape', fitToPage: true, fitToWidth: 1, fitToHeight: 0 },
        });

        ws.columns = [
            { width: 44 }, // product
            { width: 20 }, // uom
            { width: 16 }, // unit cost
            { width: 12 }, // currency
            { width: 20 }, // accounting qty
            { width: 18 }, // qoh
            { width: 20 }, // value
        ];

        const logoBuffer = await fetchLogoBuffer();
        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
            ws.getRow(1).height = 90;
            ws.addImage(imageId, { tl: { col: 0, row: 0 }, ext: { width: 160, height: 115 } });
        } else {
            ws.getRow(1).height = 35;
        }

        let currentRow = logoBuffer ? 6 : 3;

        const titleRow = currentRow++;
        ws.getCell(`A${titleRow}`).value =
            `${getTranslatedLabel(`${localizationKey}.title`, 'Inventory Valuation For: ')}${utils.rtlEmbed(utils.safeString(companyName))}`;
        ws.mergeCells(`A${titleRow}:G${titleRow}`);
        ws.getRow(titleRow).font = { name: 'Amiri', size: 16, bold: true };
        ws.getRow(titleRow).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        ws.getRow(titleRow).height = 45;

        const subRow = currentRow++;
        const parts: string[] = [];
        if (facilityName) {
            parts.push(`${getTranslatedLabel(`${localizationKey}.facility`, 'Facility')}: ${utils.rtlEmbed(facilityName)}`);
        }
        if (thruDate) {
            parts.push(`${getTranslatedLabel(`${localizationKey}.date`, 'Date Thru')}: ${utils.formatDate(thruDate)}`);
        }
        parts.push(`${getTranslatedLabel(`${localizationKey}.generated-at`, 'Generated At')}: ${utils.formatDate(new Date())}`);
        ws.getCell(`A${subRow}`).value = parts.join('  |  ');
        ws.mergeCells(`A${subRow}:G${subRow}`);
        ws.getRow(subRow).font = { name: 'Amiri', size: 10, italic: true };
        ws.getRow(subRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(subRow).height = 28;

        currentRow += 2;

        const bordered = (row: ExcelJS.Row) => {
            row.eachCell((cell) => {
                cell.border = {
                    top: { style: 'thin' }, left: { style: 'thin' },
                    bottom: { style: 'thin' }, right: { style: 'thin' },
                };
            });
        };

        const headerRow = ws.getRow(currentRow++);
        headerRow.getCell(1).value = getTranslatedLabel(`${localizationKey}.product`, 'Product');
        headerRow.getCell(2).value = getTranslatedLabel(`${localizationKey}.quantityUom`, 'Quantity UOM');
        headerRow.getCell(3).value = getTranslatedLabel(`${localizationKey}.unitCost`, 'Unit Cost');
        headerRow.getCell(4).value = getTranslatedLabel(`${localizationKey}.currency`, 'Currency');
        headerRow.getCell(5).value = getTranslatedLabel(`${localizationKey}.accountingQuantity`, 'Accounting Quantity Sum');
        headerRow.getCell(6).value = getTranslatedLabel(`${localizationKey}.quantityOnHand`, 'QOH Sum');
        headerRow.getCell(7).value = getTranslatedLabel(`${localizationKey}.value`, 'Value');
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };
        bordered(headerRow);

        rows.forEach((r) => {
            const row = ws.getRow(currentRow++);
            row.getCell(1).value = utils.rtlEmbed(utils.safeString(r.productName));
            row.getCell(2).value = utils.rtlEmbed(utils.safeString(r.quantityUomDescription));
            row.getCell(3).value = r.unitCost;
            row.getCell(4).value = utils.safeString(r.currencyUomId);
            row.getCell(5).value = r.accountingQuantitySum;
            row.getCell(6).value = r.quantityOnHandSum;
            row.getCell(7).value = r.value;
            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', vertical: 'middle' };
            [3, 5, 6, 7].forEach((c) => { row.getCell(c).numFmt = '#,##0.00'; });
            bordered(row);
        });

        const totalRow = ws.getRow(currentRow++);
        totalRow.getCell(1).value = getTranslatedLabel(`${localizationKey}.totalValue`, 'Total Value');
        totalRow.getCell(7).value = totalValue;
        totalRow.getCell(7).numFmt = '#,##0.00';
        totalRow.font = { name: 'Amiri', size: 12, bold: true };
        totalRow.alignment = { horizontal: 'right', vertical: 'middle' };
        bordered(totalRow);

        return await workbook.xlsx.writeBuffer();
    }, [companyName, rows, totalValue, facilityName, thruDate, getTranslatedLabel, isFetching]);

    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            saveAs(blob, `InventoryValuation_${companyName.replace(/[^a-z0-9]/gi, '_')}_${new Date().toISOString().slice(0, 10)}.xlsx`);
        }
    }, [generateExcel, companyName]);

    return (
        <Button
            variant="outlined"
            color="primary"
            disabled={isFetching || rows.length === 0}
            onClick={handleDownload}
            sx={{ ml: 1 }}
        >
            {getTranslatedLabel(`${localizationKey}.excel`, 'Export Inventory Valuation')}
        </Button>
    );
};
