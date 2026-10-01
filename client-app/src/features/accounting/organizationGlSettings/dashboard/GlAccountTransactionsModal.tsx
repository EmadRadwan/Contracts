import { useCallback, useState } from 'react';
import { SortDescriptor } from '@progress/kendo-data-query';

import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import {
    useFetchGlAccountTransactionDetailsQuery,
    useLazyFetchGlAccountTransactionsPdfQuery,
} from '../../../../app/store/apis/accounting/accountingReportsApi';
import { GlAccountTransactionsExcel } from '../report/GlAccountTransactionsExcel';
import { GlAccountTransactionsDateRangeExcel } from '../report/GlAccountTransactionsDateRangeExcel';
import { GlAccountTransactionsPdf } from '../report/GlAccountTransactionsPdf';
import GlAccountTransactionsModalBase from './GlAccountTransactionsModalBase';

// Drill-down behind both trial balance reports (flat and by-level). Everything below the fetch is
// GlAccountTransactionsModalBase; this file only supplies the period-based query, the trial
// balance's own presentation values and its three exports.

interface Props {
    onClose: () => void;
    organizationPartyId: string;
    customTimePeriodId: string;
    glAccountId: string;
}

const initialSort: SortDescriptor[] = [
    { field: 'transactionDate', dir: 'asc' },
    { field: 'acctgTransId', dir: 'asc' },
];

export default function GlAccountTransactionsModal({
    onClose,
    organizationPartyId,
    customTimePeriodId,
    glAccountId,
}: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const [includePrePeriod, setIncludePrePeriod] = useState(false);

    const { data, isLoading, isFetching } = useFetchGlAccountTransactionDetailsQuery(
        {
            organizationPartyId,
            customTimePeriodId,
            glAccountId,
            includePrePeriodTransactions: includePrePeriod,
        },
        { skip: !organizationPartyId || !customTimePeriodId || !glAccountId }
    );

    const [triggerPdf] = useLazyFetchGlAccountTransactionsPdfQuery();
    const fetchPdf = useCallback(
        () =>
            triggerPdf({
                organizationPartyId,
                customTimePeriodId,
                glAccountId,
                includePrePeriodTransactions: includePrePeriod,
            }).unwrap(),
        [triggerPdf, organizationPartyId, customTimePeriodId, glAccountId, includePrePeriod]
    );

    return (
        <GlAccountTransactionsModalBase
            onClose={onClose}
            data={data}
            isLoading={isLoading}
            isFetching={isFetching}
            includePrePeriod={includePrePeriod}
            onIncludePrePeriodChange={setIncludePrePeriod}
            localizationKey="accounting.orgGL.reports.trial-balance.transactions"
            columnSet="detailed"
            width={1200}
            gridHeight="300px"
            pageSize={4}
            initialSort={initialSort}
            debitRowBackground="rgba(55,180,0,0.32)"
            summaryVariant="plain"
            showFooterTotals
            includePrePeriodLabel="Include transactions before period"
            renderExports={({ data: details, rows, totalDebit, totalCredit, isFetching: fetching }) => (
                <>
                    <GlAccountTransactionsExcel
                        accountCode={details.accountCode ?? ''}
                        accountName={details.accountName ?? ''}
                        openingBalance={details.openingBalance ?? 0}
                        postedDebits={details.postedDebits ?? 0}
                        postedCredits={details.postedCredits ?? 0}
                        endingBalance={details.endingBalance ?? 0}
                        rows={rows}
                        totalDebit={totalDebit}
                        totalCredit={totalCredit}
                        getTranslatedLabel={getTranslatedLabel}
                        isFetching={fetching}
                    />
                    {/* REFACTOR (2026-08-14): isDebit is forwarded from the API response so the
                        date-range export can do its balance math correctly instead of assuming
                        every account is debit-natured. Falls back to true (debit) only if the
                        field is ever missing, matching the export's old hardcoded assumption so
                        nothing regresses if the backend response is stale. */}
                    <GlAccountTransactionsDateRangeExcel
                        accountCode={details.accountCode ?? ''}
                        accountName={details.accountName ?? ''}
                        openingBalance={details.openingBalance ?? 0}
                        isDebit={details.isDebit ?? true}
                        rows={rows}
                        getTranslatedLabel={getTranslatedLabel}
                    />
                    {/* Server-side Telerik PDF of exactly what the grid shows (same query + pre-period toggle). */}
                    <GlAccountTransactionsPdf
                        fetchPdf={fetchPdf}
                        glAccountId={glAccountId}
                        accountCode={details.accountCode ?? ''}
                        accountName={details.accountName ?? ''}
                        getTranslatedLabel={getTranslatedLabel}
                        isFetching={fetching}
                    />
                </>
            )}
        />
    );
}
