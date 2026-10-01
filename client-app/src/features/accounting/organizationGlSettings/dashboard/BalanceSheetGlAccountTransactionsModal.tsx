import { useCallback, useState } from 'react';

import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import {
    useFetchBalanceSheetGlAccountTransactionDetailsQuery,
    useLazyFetchBalanceSheetGlAccountTransactionsPdfQuery,
} from '../../../../app/store/apis/accounting/accountingReportsApi';
import { GlAccountTransactionsExcel } from '../report/GlAccountTransactionsExcel';
import { GlAccountTransactionsPdf } from '../report/GlAccountTransactionsPdf';
import GlAccountTransactionsModalBase from './GlAccountTransactionsModalBase';

// Drill-down behind the Balance Sheet report. Presentation lives in
// GlAccountTransactionsModalBase; this file supplies the thru-date query and the two exports.
// The on-screen grid keeps the `compact` column set this modal has always shown; the running
// balance and cost center the backend now returns still reach the user through the Excel and
// PDF exports. Switch `columnSet` to "detailed" to surface them in the grid too.

interface Props {
    onClose: () => void;
    organizationPartyId: string;
    thruDate: string;
    glFiscalTypeId: string;
    glAccountId: string;
}

export default function BalanceSheetGlAccountTransactionsModal({
    onClose,
    organizationPartyId,
    thruDate,
    glFiscalTypeId,
    glAccountId,
}: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const [includePrePeriod, setIncludePrePeriod] = useState(false);

    const { data, isLoading, isFetching } = useFetchBalanceSheetGlAccountTransactionDetailsQuery(
        {
            organizationPartyId,
            thruDate,
            glFiscalTypeId,
            glAccountId,
            includePrePeriodTransactions: includePrePeriod,
        },
        { skip: !organizationPartyId || !thruDate || !glAccountId }
    );

    const [triggerPdf] = useLazyFetchBalanceSheetGlAccountTransactionsPdfQuery();
    const fetchPdf = useCallback(
        () =>
            triggerPdf({
                organizationPartyId,
                thruDate,
                glFiscalTypeId,
                glAccountId,
                includePrePeriodTransactions: includePrePeriod,
            }).unwrap(),
        [triggerPdf, organizationPartyId, thruDate, glFiscalTypeId, glAccountId, includePrePeriod]
    );

    return (
        <GlAccountTransactionsModalBase
            onClose={onClose}
            data={data}
            isLoading={isLoading}
            isFetching={isFetching}
            includePrePeriod={includePrePeriod}
            onIncludePrePeriodChange={setIncludePrePeriod}
            localizationKey="accounting.orgGL.reports.balance-sheet.transactions"
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
