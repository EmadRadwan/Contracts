import React, {useEffect, useMemo, useState} from 'react';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridSortChangeEvent,
    GridRowProps,
    GridToolbar,
} from '@progress/kendo-react-grid';
import {DataResult, orderBy, SortDescriptor, State} from '@progress/kendo-data-query';
import { Box, Button, Checkbox, FormControlLabel, Grid, Typography } from '@mui/material';
import ModalContainer from '../../../../app/common/modals/ModalContainer';
import {formatCurrency, handleDatesArray} from '../../../../app/util/utils';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import LoadingComponent from '../../../../app/layout/LoadingComponent';
import {useFetchGlAccountTransactionDetailsQuery} from "../../../../app/store/apis/accounting/accountingReportsApi";
import {GlAccountTransactionsExcel} from "../report/GlAccountTransactionsExcel";

interface Props {
    onClose: () => void;
    organizationPartyId: string;
    customTimePeriodId: string;
    glAccountId: string;
}

export default function GlAccountTransactionsModal({ onClose, organizationPartyId, customTimePeriodId, glAccountId }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'accounting.orgGL.reports.trial-balance.transactions';

    // State
    const initialSort: SortDescriptor[] = [{ field: 'transactionDate', dir: 'asc' }, { field: 'acctgTransEntrySeqId', dir: 'asc' }];
    const [sort, setSort] = useState(initialSort);
    const [includePrePeriod, setIncludePrePeriod] = useState(false);
    const [accountingTransEntries, setAccountingTransEntries] = useState<any[]>([]);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);

    const pageChange = (event: any) => {
        setPage(event.page);
    };
    // Data Fetching
    const { data, isLoading, isFetching } = useFetchGlAccountTransactionDetailsQuery(
        {
            organizationPartyId,
            customTimePeriodId,
            glAccountId,
            includePrePeriodTransactions: includePrePeriod,
        },
        { skip: !organizationPartyId || !customTimePeriodId || !glAccountId }
    );

    useEffect(() => {
            if (data) {
                const adjustedData = handleDatesArray(data.transactions);
                setAccountingTransEntries(adjustedData)
            }
        }
        , [data, setAccountingTransEntries]);

    // Totals Calculation
    const { totalDebit, totalCredit } = data?.transactions?.reduce(
        (totals, e) => {
            if (e.debitCreditFlag === 'D') {
                totals.totalDebit += e.amount;
            } else {
                totals.totalCredit += e.amount;
            }
            return totals;
        },
        { totalDebit: 0, totalCredit: 0 }
    ) ?? { totalDebit: 0, totalCredit: 0 };

    const excelRows = useMemo(() => {
        if (!data?.transactions) return [];
        return data.transactions.map(t => ({
            acctgTransId: t.acctgTransId ?? '',
            acctgTransEntrySeqId: t.acctgTransEntrySeqId ?? '',
            transactionDate: t.transactionDate ?? '',
            acctgTransTypeId: t.acctgTransTypeId ?? '',
            glFiscalTypeId: t.glFiscalTypeId ?? '',
            invoiceId: t.invoiceId,
            paymentId: t.paymentId,
            workEffortId: t.certificateNumber,
            partyName: t.partyName,
            productName: t.productName,
            isPosted: t.isPosted ?? false,
            postedDate: t.postedDate,
            debitCreditFlag: t.debitCreditFlag ?? 'C',
            amount: t.amount ?? 0,
            description: t.description,
            projectName: t.projectName ?? '',
        }));
    }, [data?.transactions]);

    // Row Coloring
    const rowRender = (trElement: React.ReactElement<HTMLTableRowElement>, props: GridRowProps) => {
        const isDebit = props.dataItem.debitCreditFlag === 'D';
        const style = { backgroundColor: isDebit ? 'rgba(55,180,0,0.32)' : '#fff' };
        return React.cloneElement(trElement, { style }, trElement.props.children);
    };

    // Footer Cell for Totals
    const TotalsFooterCell = () => (
        <td colSpan={15} style={{ fontWeight: 'bold', color: '#1565C0' }}>
            {getTranslatedLabel(`${localizationKey}.totalDebit`, 'Total Debit: ')} {formatCurrency(totalDebit)} |{' '}
            {getTranslatedLabel(`${localizationKey}.totalCredit`, 'Total Credit: ')} {formatCurrency(totalCredit)}
        </td>
    );

    return (
        <ModalContainer show={true} onClose={onClose} width={1200}>
            <Box sx={{ p: 2 }}>
                <Typography variant="h6" sx={{ mb: 2 }}>
                    {getTranslatedLabel(`${localizationKey}.title`, 'Transaction Details for')} {data?.accountName} ({data?.accountCode})
                </Typography>

                {(isLoading || isFetching) && (
                    <LoadingComponent message={getTranslatedLabel('general.loading-transactions', 'Loading Transactions...')} />
                )}

                {data && (
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <Box sx={{ mb: 2 }}>
                                <Typography variant="body1">
                                    {getTranslatedLabel(`${localizationKey}.accountCode`, 'Account Code: ')} {data.accountCode}
                                </Typography>
                                <Typography variant="body1">
                                    {getTranslatedLabel(`${localizationKey}.accountName`, 'Description: ')} {data.accountName}
                                </Typography>
                                <Typography variant="body1">
                                    {getTranslatedLabel(`${localizationKey}.openingBalance`, 'Opening Balance: ')} {formatCurrency(data.openingBalance)}
                                </Typography>
                                <Typography variant="body1">
                                    {getTranslatedLabel(`${localizationKey}.postedDebits`, 'Posted Debits: ')} {formatCurrency(data.postedDebits)}
                                </Typography>
                                <Typography variant="body1">
                                    {getTranslatedLabel(`${localizationKey}.postedCredits`, 'Posted Credits: ')} {formatCurrency(data.postedCredits)}
                                </Typography>
                                <Typography variant="body1">
                                    {getTranslatedLabel(`${localizationKey}.endingBalance`, 'Ending Balance: ')} {formatCurrency(data.endingBalance)}
                                </Typography>
                            </Box>
                        </Grid>
                        <Grid item xs={12}>
                            <FormControlLabel
                                control={
                                    <Checkbox
                                        checked={includePrePeriod}
                                        onChange={(e) => setIncludePrePeriod(e.target.checked)}
                                    />
                                }
                                label={getTranslatedLabel(`${localizationKey}.includePrePeriod`, 'Include transactions before period')}
                            />
                        </Grid>
                        <Grid item xs={12}>
                            <KendoGrid
                                style={{ height: '300px' }}
                                data={orderBy(accountingTransEntries ?? [], sort).slice(page.skip, page.take + page.skip)}
                                sortable
                                skip={page.skip}
                                take={page.take}
                                total={accountingTransEntries ? accountingTransEntries.length : 0}
                                onPageChange={pageChange}
                                sort={sort}
                                onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                                pageable={true}
                                resizable
                                rowRender={rowRender}
                            >
                                <GridToolbar>
                                    <Typography variant="h6">
                                        {getTranslatedLabel(`${localizationKey}.transactions`, 'Transactions')}
                                    </Typography>

                                    <GlAccountTransactionsExcel
                                        accountCode={data?.accountCode ?? ''}
                                        accountName={data?.accountName ?? ''}
                                        openingBalance={data?.openingBalance ?? 0}
                                        postedDebits={data?.postedDebits ?? 0}
                                        postedCredits={data?.postedCredits ?? 0}
                                        endingBalance={data?.endingBalance ?? 0}
                                        rows={excelRows}
                                        totalDebit={totalDebit}
                                        totalCredit={totalCredit}
                                        getTranslatedLabel={getTranslatedLabel}
                                        isFetching={isFetching}
                                    />
                                </GridToolbar>
                                <Column
                                    field="acctgTransId"
                                    title={getTranslatedLabel(`${localizationKey}.transId`, 'Acctg Trans ID')}
                                    width={120}
                                    footerCell={TotalsFooterCell}
                                />
                                <Column
                                    field="acctgTransEntrySeqId"
                                    title={getTranslatedLabel(`${localizationKey}.transEntrySeqId`, 'Entry ID')}
                                    width={100}
                                />
                                <Column
                                    field="transactionDate"
                                    title={getTranslatedLabel(`${localizationKey}.transDate`, 'Transaction Date')}
                                    width={150}
                                    format="{0:dd/MM/yyyy}"
                                />
                                <Column
                                    field="acctgTransTypeDescription"
                                    title={getTranslatedLabel(`${localizationKey}.transType`, 'Acctg Trans Type')}
                                    width={150}
                                />
                                <Column
                                    field="debitCreditFlag"
                                    title={getTranslatedLabel(`${localizationKey}.debitCredit`, 'Debit/Credit')}
                                    width={100}
                                />
                                <Column
                                    field="amount"
                                    title={getTranslatedLabel(`${localizationKey}.amount`, 'Amount')}
                                    width={130}
                                    format="{0:c2}"
                                />
                                {/*<Column
                                    field="glFiscalTypeId"
                                    title={getTranslatedLabel(`${localizationKey}.fiscalType`, 'Fiscal GL Type')}
                                    width={100}
                                />*/}
                                <Column
                                    field="invoiceId"
                                    title={getTranslatedLabel(`${localizationKey}.invoiceId`, 'Invoice ID')}
                                    width={100}
                                />
                                <Column
                                    field="paymentId"
                                    title={getTranslatedLabel(`${localizationKey}.paymentId`, 'Payment ID')}
                                    width={100}
                                />
                                <Column
                                    field="certificateNumber"
                                    title={getTranslatedLabel(`${localizationKey}.workEffortId`, 'Work Effort ID')}
                                    width={100}
                                />
                                <Column
                                    field="partyName"
                                    title={getTranslatedLabel(`${localizationKey}.partyId`, 'Party Name')}
                                    width={150}
                                />
                                <Column
                                    field="productName"
                                    title={getTranslatedLabel(`${localizationKey}.productId`, 'Product Name')}
                                    width={150}
                                />
                                <Column
                                    field="isPosted"
                                    title={getTranslatedLabel(`${localizationKey}.isPosted`, 'Is Posted')}
                                    width={80}
                                />
                                <Column
                                    field="postedDate"
                                    title={getTranslatedLabel(`${localizationKey}.postedDate`, 'Posted Date')}
                                    width={150}
                                    format="{0:dd/MM/yyyy}"
                                />
                               
                                <Column
                                    field="description"
                                    title={getTranslatedLabel(`${localizationKey}.description`, 'Description')}
                                    width={200}
                                />
                            </KendoGrid>
                        </Grid>
                    </Grid>
                )}
            </Box>
        </ModalContainer>
    );
}