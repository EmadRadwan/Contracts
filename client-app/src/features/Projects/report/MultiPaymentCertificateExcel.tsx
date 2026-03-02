// src/features/Projects/report/MultiPaymentCertificateExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import { MultiPaymentCertificate, MultiPaymentItem } from '../../../app/models/project/MultiPaymentCertificate';
import { AcctgTransEntry } from '../../../app/models/accounting/acctgTransEntry';

/* ------------------------------------------------------------------ */
/* PROPS */
/* ------------------------------------------------------------------ */
interface MultiPaymentCertificateExcelProps {
    companyName: string;
    certificate: MultiPaymentCertificate;
    items: MultiPaymentItem[];
    transactions: AcctgTransEntry[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
    language: string;
}

/* ------------------------------------------------------------------ */
/* UTILS – RTL, formatting, safe values */
/* ------------------------------------------------------------------ */
const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? 'N/A' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

export const MultiPaymentCertificateExcel: React.FC<MultiPaymentCertificateExcelProps> = ({
                                                                                              companyName,
                                                                                              certificate,
                                                                                              items,
                                                                                              transactions,
                                                                                              getTranslatedLabel,
                                                                                              isFetching = false,
                                                                                              language,
                                                                                          }) => {
    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        if (isFetching) return null;

        // --- FETCH LOGO ---
        let logoBuffer: ArrayBuffer | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) {
                logoBuffer = await (await resp.blob()).arrayBuffer();
            }
        } catch (e) {
            console.warn('Logo fetch error:', e);
        }

        const safeSheet = companyName
            ? companyName.replace(/[*\?\\:\[\]\/]/g, '_').trim().slice(0, 31)
            : 'Certificate';
        const ws = workbook.addWorksheet(safeSheet, { views: [{ rightToLeft: language === 'ar' }] });
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.getColumn(1).font = { name: 'Amiri', size: 10 };

        // === ADD LOGO ===
        if (logoBuffer) {
            const imageId = workbook.addImage({
                buffer: logoBuffer,
                extension: 'jpeg',
            });
            const logoRow = ws.getRow(1);
            logoRow.height = 75;
            ws.addImage(imageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 100, height: 100 },
            });
            ws.getRow(2).height = 20;
            ws.getRow(3).height = 20;
            ws.addRow([]); ws.addRow([]); ws.addRow([]);
        } else {
            const fallbackRow = ws.getRow(1);
            fallbackRow.getCell(1).value = 'Logo Unavailable';
            fallbackRow.font = { name: 'Amiri', size: 10, color: { argb: 'FFFF0000' } };
            fallbackRow.alignment = { horizontal: 'center', vertical: 'middle' };
        }

        const startRow = logoBuffer ? 5 : 2;

        // === TITLE ===
        ws.addRow([utils.rtlEmbed(getTranslatedLabel(
            'projects.multiPaymentCertificate.report.title',
            'بيان مستند صرف متعدد'
        )) + ' - ' + (certificate.workEffortId || '')]);
        ws.mergeCells(`A${startRow}:L${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 16, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.addRow([]); // spacer

        // === HEADER DATA ===
        const headerStart = ws.lastRow!.number + 1;
        ws.addRow([
            getTranslatedLabel('projects.multiPaymentCertificate.form.date', 'التاريخ'),
            getTranslatedLabel('projects.multiPaymentCertificate.form.glAccount', 'مركز التكلفة'),
            getTranslatedLabel('projects.multiPaymentCertificate.form.paymentTo', 'صرف إلى'),
            getTranslatedLabel('projects.multiPaymentCertificate.form.referenceNum', 'رقم المرجع'),
            getTranslatedLabel('projects.multiPaymentCertificate.form.status', 'الحالة'),
            getTranslatedLabel('projects.multiPaymentCertificate.form.description', 'البيان'),
        ]);
        const headerLabelRow = ws.getRow(headerStart);
        headerLabelRow.font = { name: 'Amiri', size: 10, bold: true };
        headerLabelRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        headerLabelRow.alignment = { horizontal: 'center' };

        ws.addRow([
            utils.formatDate(certificate.date),
            utils.rtlEmbed(certificate.accountName || certificate.glAccountId || ''),
            utils.rtlEmbed(certificate.partyName || certificate.partyIdEmployee || ''),
            utils.rtlEmbed(certificate.notes || ''),
            utils.rtlEmbed(language === 'ar' ? (certificate.statusDescriptionArabic || '') : (certificate.statusDescription || '')),
            utils.rtlEmbed(certificate.description || ''),
        ]);
        ws.getRow(ws.lastRow!.number).font = { name: 'Amiri', size: 10 };
        ws.getRow(ws.lastRow!.number).alignment = { horizontal: 'right' };
        ws.addRow([]); // spacer

        // === ITEMS ===
        const itemsStart = ws.lastRow!.number + 1;
        ws.addRow([getTranslatedLabel('projects.multiPaymentCertificate.report.items', 'بنود المستند')]);
        ws.mergeCells(`A${itemsStart}:L${itemsStart}`);
        ws.getRow(itemsStart).font = { name: 'Amiri', size: 12, bold: true };
        ws.getRow(itemsStart).alignment = { horizontal: 'center' };

        ws.addRow([
            getTranslatedLabel('projects.multiPaymentCertificate.items.type', 'النوع'),
            getTranslatedLabel('projects.multiPaymentCertificate.items.service', 'الخدمة'),
            getTranslatedLabel('projects.multiPaymentCertificate.items.product', 'المنتج'),
            getTranslatedLabel('projects.multiPaymentCertificate.items.party', 'الطرف'),
            getTranslatedLabel('projects.multiPaymentCertificate.items.description', 'البيان'),
            getTranslatedLabel('projects.multiPaymentCertificate.items.amount', 'المبلغ'),
            getTranslatedLabel('projects.multiPaymentCertificate.items.discount', 'الخصم'),
            getTranslatedLabel('projects.multiPaymentCertificate.items.transport', 'النقل'),
            getTranslatedLabel('projects.multiPaymentCertificate.items.gratuities', 'إكراميات'),
            getTranslatedLabel('projects.multiPaymentCertificate.items.total', 'الإجمالي'),
        ]);
        const itemsHeaderRow = ws.getRow(ws.lastRow!.number);
        itemsHeaderRow.font = { name: 'Amiri', size: 10, bold: true };
        itemsHeaderRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'E6F3FF' } };
        itemsHeaderRow.alignment = { horizontal: 'center' };
        itemsHeaderRow.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

        items.forEach(item => {
            ws.addRow([
                utils.rtlEmbed(item.itemTypeDescription || item.itemType || ''),
                utils.rtlEmbed(item.serviceName || ''),
                utils.rtlEmbed(item.productName || ''),
                utils.rtlEmbed(item.partyIdSupplierName || item.partyIdContractorName || ''),
                utils.rtlEmbed(item.description || ''),
                item.amount || 0,
                item.discount || 0,
                item.transportationExpenses || 0,
                item.gratuities || 0,
                item.total || 0,
            ]);
        });
        ws.getRow(ws.lastRow!.number).eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });
        ws.addRow([]); // spacer

        // === TRANSACTIONS ===
        if (transactions && transactions.length > 0) {
            const transStart = ws.lastRow!.number + 1;
            ws.addRow([getTranslatedLabel('projects.multiPaymentCertificate.report.transactions', 'القيود المحاسبية')]);
            ws.mergeCells(`A${transStart}:L${transStart}`);
            ws.getRow(transStart).font = { name: 'Amiri', size: 12, bold: true };
            ws.getRow(transStart).alignment = { horizontal: 'center' };

            ws.addRow([
                getTranslatedLabel('accounting.payments.report.transId', 'رقم القيد'),
                getTranslatedLabel('accounting.payments.report.seq', 'تسلسل'),
                getTranslatedLabel('accounting.payments.report.account', 'رقم الحساب'),
                getTranslatedLabel('accounting.payments.report.name', 'اسم الحساب'),
                getTranslatedLabel('accounting.payments.report.dc', 'مدين/دائن'),
                getTranslatedLabel('accounting.payments.report.amount', 'المبلغ'),
                getTranslatedLabel('accounting.payments.report.currency', 'العملة'),
                getTranslatedLabel('accounting.payments.report.date', 'التاريخ'),
            ]);
            const transHeader = ws.getRow(ws.lastRow!.number);
            transHeader.font = { name: 'Amiri', size: 10, bold: true };
            transHeader.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFF2CC' } };
            transHeader.alignment = { horizontal: 'center' };
            transHeader.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

            transactions.forEach(t => {
                ws.addRow([
                    t.acctgTransId,
                    t.acctgTransEntrySeqId,
                    t.glAccountId,
                    utils.rtlEmbed(t.glAccountTypeDescription || ''),
                    t.debitCreditFlag,
                    t.origAmount,
                    t.origCurrencyUomId,
                    utils.formatDate(t.transactionDate),
                ]);
            });
            ws.getRow(ws.lastRow!.number).eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });
        }

        // === COLUMN WIDTHS ===
        ws.columns = [
            { width: 15 }, // A
            { width: 20 }, // B
            { width: 20 }, // C
            { width: 20 }, // D
            { width: 25 }, // E
            { width: 15 }, // F
            { width: 12 }, // G
            { width: 12 }, // H
            { width: 12 }, // I
            { width: 15 }, // J
            { width: 15 }, // K
            { width: 15 }, // L
        ];
        [6, 7, 8, 9, 10].forEach(i => ws.getColumn(i).numFmt = '#,##0.00');

        return await workbook.xlsx.writeBuffer();
    }, [companyName, certificate, items, transactions, getTranslatedLabel, isFetching, language]);

    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            saveAs(blob, `MultiPaymentCertificate_${certificate.workEffortId || 'Report'}.xlsx`);
        }
    }, [generateExcel, certificate.workEffortId]);

    return (
        <Button
            variant="contained"
            color="success"
            disabled={isFetching || !certificate.workEffortId}
            onClick={handleDownload}
            sx={{ mt: 2, mr: 1 }}
        >
            {getTranslatedLabel(
                'projects.multiPaymentCertificate.report.excel',
                'تصدير Excel'
            )}
        </Button>
    );
};
