import React, { useMemo, useState } from 'react';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridSortChangeEvent,
    GridToolbar,
} from '@progress/kendo-react-grid';
import { SortDescriptor, State, orderBy } from '@progress/kendo-data-query';
import { Box, Checkbox, FormControlLabel, Grid, Typography } from '@mui/material';

import ModalContainer from '../../../../app/common/modals/ModalContainer';
import { formatCurrency, handleDatesArray } from '../../../../app/util/utils';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import LoadingComponent from '../../../../app/layout/LoadingComponent';
import { createReversalAwareRow, ReversalLegend } from '../../../../app/common/grid';
import { GlAccountTransactionDetails } from '../../../../app/store/apis/accounting/accountingReportsApi';

// Single presentational shell behind every GL-account drill-down modal (trial balance, trial
// balance by level, balance sheet, income statement). Each report keeps its own thin wrapper
// because the RTK Query hook and its parameters differ per report; everything below the fetch —
// the summary block, the pre-period toggle, the grid, the totals and the Excel row shape — lives
// here once.
//
// The wrappers deliberately pass the exact widths, page sizes, sort orders, row tints and column
// sets each modal had before consolidation, so no report changes behaviour on adoption.

/** Column layout. `detailed` is the trial-balance grid (separate Debit/Credit columns, running
 *  balance, cost center, invoice/payment ids). `compact` is the balance-sheet / income-statement
 *  grid, whose backing queries do not populate running balance, cost center or payment ref. */
export type TransactionsColumnSet = 'detailed' | 'compact';

/** Row shape consumed by GlAccountTransactionsExcel (and the date-range variant). */
export interface GlAccountTransactionExcelRow {
    acctgTransId: string;
    acctgTransEntrySeqId?: string;
    transactionDate: string;
    acctgTransTypeId: string;
    acctgTransTypeDescription?: string;
    glFiscalTypeId?: string;
    invoiceId?: string;
    paymentId?: string;
    workEffortId?: string;
    certificateNumber?: string;
    partyName?: string;
    productName?: string;
    isPosted: boolean;
    debitCreditFlag: 'D' | 'C';
    amount: number;
    runningBalance: number;
    description?: string;
    projectName?: string;
    costCenterDescription?: string;
    paymentRefNum?: string;
}

export interface TransactionsExportContext {
    data: GlAccountTransactionDetails;
    rows: GlAccountTransactionExcelRow[];
    totalDebit: number;
    totalCredit: number;
    isFetching: boolean;
    includePrePeriod: boolean;
}

interface Props {
    onClose: () => void;
    data?: GlAccountTransactionDetails;
    isLoading: boolean;
    isFetching: boolean;

    includePrePeriod: boolean;
    onIncludePrePeriodChange: (value: boolean) => void;

    /** Per-report i18n prefix, e.g. `accounting.orgGL.reports.trial-balance.transactions`. */
    localizationKey: string;
    columnSet: TransactionsColumnSet;

    /** Export buttons for the grid toolbar — the only piece that still differs per report. */
    renderExports?: (ctx: TransactionsExportContext) => React.ReactNode;

    // Presentation knobs, defaulted to each modal's pre-consolidation values by its wrapper.
    width?: number;
    gridHeight?: string;
    pageSize?: number;
    initialSort?: SortDescriptor[];
    debitRowBackground?: string;
    /** `plain` = the trial balance's bare label lines; `boxed` = the bordered grey panel. */
    summaryVariant?: 'plain' | 'boxed';
    /** Totals footer under the first column — only the trial balance grid showed one. */
    showFooterTotals?: boolean;
    includePrePeriodLabel?: string;
}

// Row tint must be created outside render so KendoReact v16 keeps a stable row identity. Cached
// per colour because the tint is the only thing that varies between reports.
const rowRendererCache = new Map<string, ReturnType<typeof createReversalAwareRow>>();
const getDebitCreditRow = (debitBackground: string) => {
    let renderer = rowRendererCache.get(debitBackground);
    if (!renderer) {
        renderer = createReversalAwareRow((dataItem) => ({
            backgroundColor: dataItem.debitCreditFlag === 'D' ? debitBackground : '#ffffff',
        }));
        rowRendererCache.set(debitBackground, renderer);
    }
    return renderer;
};

// One mapping for every report. `isPosted` arrives from the API as the string 'Y'/'N'; the Excel
// export takes a boolean, so it is normalised here (passing the raw string made every row read
// "Yes" in the trial balance export). `workEffortId` is what the export's Work Effort column
// reads, but the value the grid shows lives in `certificateNumber`, so both are supplied.
const toExcelRow = (t: any): GlAccountTransactionExcelRow => ({
    acctgTransId: t.acctgTransId ?? '',
    acctgTransEntrySeqId: t.acctgTransEntrySeqId ?? '',
    transactionDate: t.transactionDate ?? '',
    acctgTransTypeId: t.acctgTransTypeId ?? '',
    acctgTransTypeDescription: t.acctgTransTypeDescription ?? t.acctgTransTypeId ?? '',
    glFiscalTypeId: t.glFiscalTypeId ?? '',
    invoiceId: t.invoiceId,
    paymentId: t.paymentId,
    workEffortId: t.certificateNumber ?? t.workEffortId,
    certificateNumber: t.certificateNumber,
    partyName: t.partyName,
    productName: t.productName,
    isPosted: t.isPosted === true || t.isPosted === 'Y',
    debitCreditFlag: (t.debitCreditFlag ?? 'C') as 'D' | 'C',
    amount: t.amount ?? 0,
    runningBalance: t.runningBalance ?? 0,
    description: t.description,
    projectName: t.projectName ?? '',
    costCenterDescription: t.costCenterDescription ?? '',
    paymentRefNum: t.paymentRefNum ?? '',
});

export default function GlAccountTransactionsModalBase({
    onClose,
    data,
    isLoading,
    isFetching,
    includePrePeriod,
    onIncludePrePeriodChange,
    localizationKey,
    columnSet,
    renderExports,
    width = 1280,
    gridHeight = '460px',
    pageSize = 15,
    initialSort,
    debitRowBackground = 'rgba(55, 180, 0, 0.15)',
    summaryVariant = 'boxed',
    showFooterTotals = false,
    includePrePeriodLabel = 'Include transactions before the reporting period',
}: Props) {
    const { getTranslatedLabel } = useTranslationHelper();

    const [sort, setSort] = useState<SortDescriptor[]>(
        initialSort ?? [
            { field: 'transactionDate', dir: 'asc' },
            { field: 'acctgTransEntrySeqId', dir: 'asc' },
        ]
    );
    const [page, setPage] = useState<State>({ skip: 0, take: pageSize });

    const transactions = useMemo(
        () => handleDatesArray(data?.transactions ?? []),
        [data?.transactions]
    );

    const { totalDebit, totalCredit } = useMemo(
        () =>
            transactions.reduce(
                (totals, e: any) => {
                    if (e.debitCreditFlag === 'D') totals.totalDebit += e.amount || 0;
                    else totals.totalCredit += e.amount || 0;
                    return totals;
                },
                { totalDebit: 0, totalCredit: 0 }
            ),
        [transactions]
    );

    const excelRows = useMemo(() => transactions.map(toExcelRow), [transactions]);

    const DebitCreditRow = useMemo(
        () => getDebitCreditRow(debitRowBackground),
        [debitRowBackground]
    );

    const label = (key: string, fallback: string) =>
        getTranslatedLabel(`${localizationKey}.${key}`, fallback);

    const TotalsFooterCell = () => (
        <td colSpan={15} style={{ fontWeight: 'bold', color: '#1565C0' }}>
            {label('totalDebit', 'Total Debit: ')} {formatCurrency(totalDebit)} |{' '}
            {label('totalCredit', 'Total Credit: ')} {formatCurrency(totalCredit)}
        </td>
    );

    const summaryLines = data
        ? [
              { key: 'accountCode', fallback: 'Account Code: ', value: data.accountCode },
              { key: 'accountName', fallback: 'Description: ', value: data.accountName },
              {
                  key: 'openingBalance',
                  fallback: 'Opening Balance: ',
                  value: formatCurrency(data.openingBalance),
              },
              {
                  key: 'postedDebits',
                  fallback: 'Posted Debits: ',
                  value: formatCurrency(data.postedDebits),
              },
              {
                  key: 'postedCredits',
                  fallback: 'Posted Credits: ',
                  value: formatCurrency(data.postedCredits),
              },
              {
                  key: 'endingBalance',
                  fallback: 'Ending Balance: ',
                  value: formatCurrency(data.endingBalance),
              },
          ]
        : [];

    const detailedColumns = [
        <Column
            key="acctgTransId"
            field="acctgTransId"
            title={label('transId', 'Acctg Trans ID')}
            width={120}
            cells={showFooterTotals ? { footerCell: TotalsFooterCell } : undefined}
        />,
        <Column
            key="transactionDate"
            field="transactionDate"
            title={label('transDate', 'Transaction Date')}
            width={150}
            format="{0:dd/MM/yyyy}"
        />,
        <Column
            key="transType"
            field="acctgTransTypeDescription"
            title={label('transType', 'Acctg Trans Type')}
            width={150}
        />,
        <Column
            key="debit"
            title={label('debit', 'Debit')}
            width={120}
            cells={{
                data: (props: any) => (
                    <td>
                        {props.dataItem.debitCreditFlag === 'D'
                            ? formatCurrency(props.dataItem.amount)
                            : ''}
                    </td>
                ),
            }}
        />,
        <Column
            key="credit"
            title={label('credit', 'Credit')}
            width={120}
            cells={{
                data: (props: any) => (
                    <td>
                        {props.dataItem.debitCreditFlag === 'C'
                            ? formatCurrency(props.dataItem.amount)
                            : ''}
                    </td>
                ),
            }}
        />,
        <Column
            key="runningBalance"
            field="runningBalance"
            title={label('balance', 'Balance')}
            width={120}
            format="{0:c2}"
        />,
        <Column key="invoiceId" field="invoiceId" title={label('invoiceId', 'Invoice ID')} width={100} />,
        <Column key="paymentId" field="paymentId" title={label('paymentId', 'Payment ID')} width={100} />,
        <Column
            key="costCenter"
            field="costCenterDescription"
            title={label('costCenter', 'Cost Center')}
            width={150}
        />,
        <Column
            key="certificateNumber"
            field="certificateNumber"
            title={label('workEffortId', 'Work Effort ID')}
            width={100}
        />,
        <Column key="partyName" field="partyName" title={label('partyId', 'Party Name')} width={150} />,
        <Column key="productName" field="productName" title={label('productId', 'Product Name')} width={150} />,
        <Column key="isPosted" field="isPosted" title={label('isPosted', 'Is Posted')} width={80} />,
        <Column key="description" field="description" title={label('description', 'Description')} width={200} />,
    ];

    const compactColumns = [
        <Column
            key="transactionDate"
            field="transactionDate"
            title={label('transDate', 'Date')}
            format="{0:dd/MM/yyyy}"
            width={110}
            cells={showFooterTotals ? { footerCell: TotalsFooterCell } : undefined}
        />,
        <Column key="acctgTransId" field="acctgTransId" title={label('transId', 'Trans ID')} width={110} />,
        <Column
            key="transType"
            field="acctgTransTypeDescription"
            title={label('transType', 'Type')}
            width={160}
        />,
        <Column key="debitCreditFlag" field="debitCreditFlag" title={label('debitCredit', 'D/C')} width={70} />,
        <Column key="amount" field="amount" title={label('amount', 'Amount')} format="{0:n2}" width={130} />,
        <Column key="description" field="description" title={label('description', 'Description')} width={280} />,
        <Column key="partyName" field="partyName" title={label('partyName', 'Party')} width={180} />,
        <Column key="productName" field="productName" title={label('productName', 'Product')} width={150} />,
        <Column
            key="certificateNumber"
            field="certificateNumber"
            title={label('certificateNumber', 'Certificate')}
            width={120}
        />,
    ];

    return (
        <ModalContainer show={true} onClose={onClose} width={width}>
            <Box sx={{ p: summaryVariant === 'plain' ? 2 : 3 }}>
                <Typography variant="h6" sx={{ mb: summaryVariant === 'plain' ? 2 : 3 }}>
                    {label('title', 'Transaction Details for')} {data?.accountName} ({data?.accountCode})
                </Typography>

                {(isLoading || isFetching) && (
                    <LoadingComponent
                        message={getTranslatedLabel('general.loading-transactions', 'Loading Transactions...')}
                    />
                )}

                {data && (
                    <Grid container spacing={summaryVariant === 'plain' ? 2 : 3}>
                        <Grid item xs={12}>
                            <Box
                                sx={
                                    summaryVariant === 'boxed'
                                        ? {
                                              p: 2,
                                              border: '1px solid #e0e0e0',
                                              borderRadius: 2,
                                              backgroundColor: '#f9f9f9',
                                          }
                                        : { mb: 2 }
                                }
                            >
                                {summaryLines.map((line, index) => (
                                    <Typography
                                        key={line.key}
                                        variant="body1"
                                        sx={
                                            summaryVariant === 'boxed' && index === summaryLines.length - 1
                                                ? { fontWeight: 'bold', mt: 1 }
                                                : undefined
                                        }
                                    >
                                        {label(line.key, line.fallback)} {line.value}
                                    </Typography>
                                ))}
                            </Box>
                        </Grid>

                        <Grid item xs={12}>
                            <FormControlLabel
                                control={
                                    <Checkbox
                                        checked={includePrePeriod}
                                        onChange={(e) => onIncludePrePeriodChange(e.target.checked)}
                                    />
                                }
                                label={label('includePrePeriod', includePrePeriodLabel)}
                            />
                        </Grid>

                        <Grid item xs={12}>
                            <ReversalLegend />
                            <KendoGrid
                                scrollable="scrollable"
                                style={{ height: gridHeight }}
                                data={orderBy(transactions, sort).slice(page.skip!, page.skip! + page.take!)}
                                sortable={true}
                                sort={sort}
                                onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                                pageable={true}
                                skip={page.skip}
                                take={page.take}
                                total={transactions.length}
                                onPageChange={(event: any) => setPage(event.page)}
                                rows={{ data: DebitCreditRow }}
                                resizable={true}
                            >
                                <GridToolbar>
                                    <Typography
                                        variant="h6"
                                        sx={summaryVariant === 'boxed' ? { flex: 1 } : undefined}
                                    >
                                        {label('transactions', 'Transactions')}
                                    </Typography>
                                    {renderExports?.({
                                        data,
                                        rows: excelRows,
                                        totalDebit,
                                        totalCredit,
                                        isFetching,
                                        includePrePeriod,
                                    })}
                                </GridToolbar>
                                {columnSet === 'detailed' ? detailedColumns : compactColumns}
                            </KendoGrid>
                        </Grid>
                    </Grid>
                )}
            </Box>
        </ModalContainer>
    );
}
