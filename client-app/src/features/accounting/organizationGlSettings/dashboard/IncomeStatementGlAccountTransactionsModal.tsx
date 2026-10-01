import { useCallback, useState } from 'react';

import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import {
    useFetchIncomeStatementGlAccountTransactionDetailsQuery,
    useLazyFetchIncomeStatementGlAccountTransactionsPdfQuery,
} from '../../../../app/store/apis/accounting/accountingReportsApi';
import { GlAccountTransactionsExcel } from '../report/GlAccountTransactionsExcel';
import { GlAccountTransactionsPdf } from '../report/GlAccountTransactionsPdf';
import GlAccountTransactionsModalBase from './GlAccountTransactionsModalBase';

// Drill-down behind the Income Statement report. Presentation lives in
// GlAccountTransactionsModalBase; this file supplies the date-range/month query and the two
// exports. The on-screen grid keeps the `compact` column set this modal has always shown; the
// running balance and cost center the backend now returns still reach the user through the Excel
// and PDF exports. Switch `columnSet` to "detailed" to surface them in the grid too.

interface Props {
    onClose: () => void;
    organizationPartyId: string;
    fromDate?: string;
    thruDate?: string;
    selectedMonth?: number;
    glFiscalTypeId: string;
    glAccountId: string;
    /** "Y" / "N" to restrict, "ALL" for both. Omitted means posted only, which is what the
     *  income statement itself reports. Transaction Totals passes the tab's own setting. */
    isPosted?: string;
}

export default function IncomeStatementGlAccountTransactionsModal({
    onClose,
    organizationPartyId,
    fromDate,
    thruDate,
    selectedMonth,
    glFiscalTypeId,
    glAccountId,
    isPosted,
}: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const [includePrePeriod, setIncludePrePeriod] = useState(false);

    const { data, isLoading, isFetching } = useFetchIncomeStatementGlAccountTransactionDetailsQuery(
        {
            organizationPartyId,
            fromDate,
            thruDate,
            selectedMonth,
            glFiscalTypeId,
            glAccountId,
            includePrePeriodTransactions: includePrePeriod,
            isPosted,
        },
        {
            skip:
                !organizationPartyId ||
                !glAccountId ||
                (!fromDate && !thruDate && selectedMonth === undefined),
        }
    );

    const [triggerPdf] = useLazyFetchIncomeStatementGlAccountTransactionsPdfQuery();
    const fetchPdf = useCallback(
        () =>
            triggerPdf({
                organizationPartyId,
                fromDate,
                thruDate,
                selectedMonth,
                glFiscalTypeId,
                glAccountId,
                includePrePeriodTransactions: includePrePeriod,
                isPosted,
            }).unwrap(),
        [
            triggerPdf,
            organizationPartyId,
            fromDate,
            thruDate,
            selectedMonth,
            glFiscalTypeId,
            glAccountId,
            includePrePeriod,
            isPosted,
        ]
    );

    return (
        <GlAccountTransactionsModalBase
            onClose={onClose}
            data={data}
            isLoading={isLoading}
            isFetching={isFetching}
            includePrePeriod={includePrePeriod}
            onIncludePrePeriodChange={setIncludePrePeriod}
            localizationKey="accounting.orgGL.reports.income-statement.transactions"
            columnSet="compact"
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
                    {/* Server-side Telerik PDF of exactly what the grid shows (same query +
                        pre-period toggle), so Arabic shapes correctly. */}
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
