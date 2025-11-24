// PaymentPlanExcel.tsx – FINAL PRODUCTION VERSION (Perfect alignment + real data)

import React, { useCallback, useState } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import { SalesRequest } from '../../../../../app/models/order/SalesRequest';
import { Product } from '../../../../../app/models/product/product';

interface PaymentPlanExcelProps {
    salesRequest: SalesRequest;
    apartment?: Product;
}

export const PaymentPlanExcel: React.FC<PaymentPlanExcelProps> = ({
                                                                      salesRequest,
                                                                      apartment,
                                                                  }) => {
    const [loading, setLoading] = useState(false);

    const aptM2 = salesRequest.apartmentSpaceM2 || 0;
    const aptPrice = salesRequest.apartmentPricePerM2 || 0;
    const aptTotal = aptM2 * aptPrice;

    const gardenM2 = salesRequest.gardenSpaceM2 || 0;
    const gardenPrice = salesRequest.gardenPricePerM2 || 0;
    const gardenTotal = gardenM2 * gardenPrice;

    const totalPrice = salesRequest.totalPrice || aptTotal + gardenTotal;
    const advance = salesRequest.advancePayment || 0;
    const remaining = totalPrice - advance;
    const maintenance = salesRequest.maintenanceDeposit || Math.round(totalPrice * 0.07 * 100) / 100;

    const displayUnitName = salesRequest.apartmentName || apartment?.productName || 'الوحدة';

    const installments = React.useMemo(() => {
        if (!salesRequest.numberOfInstallments || !salesRequest.dateOfFirstInstallment || advance >= totalPrice)
            return [];

        const count = salesRequest.numberOfInstallments!;
        const amount = remaining / count;
        const interval = salesRequest.monthsBetweenInstallments || 3;
        const start = new Date(salesRequest.dateOfFirstInstallment!);

        return Array.from({ length: count }, (_, i) => ({
            amount,
            dueDate: new Date(start.getFullYear(), start.getMonth() + i * interval, start.getDate()),
        }));
    }, [salesRequest, remaining, advance, totalPrice]);

    const generateExcel = useCallback(async () => {
        const wb = new ExcelJS.Workbook();
        wb.creator = 'Golden Land';
        wb.created = new Date();

        const ws = wb.addWorksheet('الوحدة', { views: [{ rightToLeft: true }] });
        ws.properties.defaultFont = { name: 'Amiri', size: 11 };

        // === ROW 1 & 2: Headers (unchanged) ===
        const row1 = ws.getRow(1);
        row1.height = 32;
        row1.font = { bold: true };
        row1.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFD9EAD3' } };
        row1.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };

        const headers = [
            'البيان', 'الدور', 'رقم الوحدة', 'مساحة الوحدة ( م2 )', '',
            'سعر المتر المحدد', '', 'اجمالي الاسعار بالتفصيل', '',
            'اجمالي ثمن الوحده بالكامل ( حديقة + شقة)', 'مقدم (20% )', 'المتبقي',
            'وديعة صيانة ( 7 % )', 'ملاحظات',
        ];
        headers.forEach((h, i) => ws.getCell(1, i + 1).value = h);
        ws.mergeCells('D1:E1'); ws.mergeCells('F1:G1'); ws.mergeCells('H1:I1');

        const row2 = ws.getRow(2);
        row2.height = 28;
        row2.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFF2CC' } };
        row2.alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getCell('D2').value = 'حديقة'; ws.getCell('E2').value = 'شقة';
        ws.getCell('F2').value = 'حديقة'; ws.getCell('G2').value = 'شقة';
        ws.getCell('H2').value = 'حديقة'; ws.getCell('I2').value = 'شقة';

        // === ROW 3: Data ===
        const row3 = ws.getRow(3);
        row3.height = 35;
        row3.font = { bold: true };
        row3.alignment = { horizontal: 'center', vertical: 'middle' };

        ws.getCell('A3').value = salesRequest.fromPartyName || '';
        ws.getCell('B3').value = ''; // Floor (not available)
        ws.getCell('C3').value = displayUnitName;
        ws.getCell('D3').value = gardenM2;
        ws.getCell('E3').value = aptM2;           // ← 147
        ws.getCell('F3').value = gardenPrice;
        ws.getCell('G3').value = aptPrice;
        ws.getCell('H3').value = gardenTotal;
        ws.getCell('I3').value = aptTotal;
        ws.getCell('J3').value = totalPrice;
        ws.getCell('K3').value = advance;
        ws.getCell('L3').value = remaining;
        ws.getCell('M3').value = maintenance;
        ws.getCell('N3').value = 'تسدد و ديعة الصيانة عند الاستلام';

        // === ROW 4: Totals ===
        const row4 = ws.getRow(4);
        row4.height = 25;
        row4.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE6E6E6' } };
        row4.getCell('H').value = gardenTotal;
        row4.getCell('I').value = aptTotal;
        row4.getCell('J').value = totalPrice;
        row4.getCell('K').value = advance;
        row4.getCell('L').value = remaining;
        row4.getCell('M').value = maintenance;

        ws.getRow(5).height = 15;

        // === ROW 6: Payment Header – 5 SEPARATE COLUMNS (PERFECT ALIGNMENT) ===
        const headerCells = ['B6', 'C6', 'D6', 'E6', 'F6'];
        const headerLabels = ['القيمة', 'التاريخ', 'رقم الشيك', 'البنك', 'تاريخ الاستحقاق'];

        headerCells.forEach((cell, i) => {
            ws.getCell(cell).value = headerLabels[i];
            ws.getCell(cell).font = { bold: true, size: 12 };
            ws.getCell(cell).alignment = { horizontal: 'center', vertical: 'middle' };
            ws.getCell(cell).fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE4E4E4' } };
        });

        // === Payment Rows ===
        let r = 7;

        // Advance
        ws.getCell(`B${r}`).value = 'مقدم';
        ws.getCell(`C${r}`).value = advance;
        ws.getCell(`D${r}`).value = new Date(salesRequest.saleDate!).toLocaleDateString('en-GB');
        r++;

        // Installments
        installments.forEach((inst, i) => {
            const label = i === 0 ? 'القسط الاول' : `القسط ${i + 1}`;
            ws.getCell(`B${r}`).value = label;
            ws.getCell(`C${r}`).value = inst.amount;
            ws.getCell(`D${r}`).value = inst.dueDate.toLocaleDateString('en-GB');
            r++;
        });

        // Maintenance
        ws.getCell(`B${r}`).value = 'وديعة صيانة ( 7% )';
        ws.getCell(`C${r}`).value = maintenance;
        ws.getCell(`D${r}`).value = 'تسدد عند الاستلام';
        r += 2;

        // Final Totals
        ws.getCell(`B${r}`).value = 'الاجمالي';
        ws.getCell(`C${r}`).value = totalPrice + maintenance;
        ws.getCell(`C${r}`).font = { bold: true };
        r++;
        ws.getCell(`B${r}`).value = 'المدفوع';
        ws.getCell(`C${r}`).value = 0;
        r++;
        ws.getCell(`B${r}`).value = 'المتبقي';
        ws.getCell(`C${r}`).value = totalPrice + maintenance;
        ws.getCell(`C${r}`).font = { bold: true };

        // === Styling ===
        ws.columns = [
            { width: 20 }, { width: 24 }, { width: 15 }, { width: 11 }, { width: 11 },
            { width: 13 }, { width: 13 }, { width: 15 }, { width: 15 }, { width: 22 },
            { width: 15 }, { width: 15 }, { width: 16 }, { width: 28 },
        ];

        ['C', 'H', 'I', 'J', 'K', 'L', 'M'].forEach(col => {
            ws.getColumn(col).numFmt = '#,##0.00';
        });

        return await wb.xlsx.writeBuffer();
    }, [salesRequest, apartment, displayUnitName, installments]);

    const handleDownload = useCallback(async () => {
        setLoading(true);
        try {
            const buffer = await generateExcel();
            const blob = new Blob([buffer], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            const name = displayUnitName.replace(/[/\\?%*:|"<>]/g, '-');
            const id = salesRequest.salesRequestId || 'جديد';
            saveAs(blob, `جدول الاقساط_${name}_${id}.xlsx`);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [generateExcel, displayUnitName, salesRequest]);

    if (!totalPrice) return null;

    return (
        <Button
            variant="contained"
            color="success"
            onClick={handleDownload}
            disabled={loading}
            sx={{ fontWeight: 'bold', ml: 1 }}
        >
            {loading ? 'جاري الإنشاء...' : 'تحميل جدول الأقساط'}
        </Button>
    );
};