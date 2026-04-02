import React, {useMemo, useState } from 'react';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridSortChangeEvent, GridToolbar
} from '@progress/kendo-react-grid';
import { orderBy, SortDescriptor, State } from '@progress/kendo-data-query';
import { Box, Checkbox, FormControlLabel, Grid, Typography } from '@mui/material';

import ModalContainer from '../../../../app/common/modals/ModalContainer';
import { formatCurrency, handleDatesArray } from '../../../../app/util/utils';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import LoadingComponent from '../../../../app/layout/LoadingComponent';

import { useFetchBalanceSheetGlAccountTransactionDetailsQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import { GlAccountTransactionsExcel } from "../report/GlAccountTransactionsExcel";

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
                                                                   glAccountId
                                                               }: Props) {

    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'accounting.orgGL.reports.balance-sheet.transactions';

    // State
    const initialSort: SortDescriptor[] = [
        { field: 'transactionDate', dir: 'asc' },
        { field: 'acctgTransEntrySeqId', dir: 'asc' }
    ];

    const [sort, setSort] = useState<SortDescriptor[]>(initialSort);
    const [includePrePeriod, setIncludePrePeriod] = useState(false);
    const [page, setPage] = useState<State>({ skip: 0, take: 15 });

    // Data Fetching
    const { data, isLoading, isFetching } = useFetchBalanceSheetGlAccountTransactionDetailsQuery(
        {
            organizationPartyId,
            thruDate,
            glFiscalTypeId,
            glAccountId,
            includePrePeriodTransactions: includePrePeriod,
        },
        {
            skip: !organizationPartyId || !thruDate || !glAccountId
        }
    );

    // Process dates
    const transactions = useMemo(() => {
        return handleDatesArray(data?.transactions ?? []);
    }, [data?.transactions]);

    // Totals for current displayed transactions
    const { totalDebit, totalCredit } = useMemo(() => {
        return transactions.reduce(
            (totals, e) => {
                if (e.debitCreditFlag === 'D') {
                    totals.totalDebit += e.amount || 0;
                } else {
                    totals.totalCredit += e.amount || 0;
                }
                return totals;
            },
            { totalDebit: 0, totalCredit: 0 }
        );
    }, [transactions]);

    // Excel Export Data
    const excelRows = useMemo(() => {
        return transactions.map(t => ({
            acctgTransId: t.acctgTransId ?? '',
            acctgTransEntrySeqId: t.acctgTransEntrySeqId ?? '',
            transactionDate: t.transactionDate ?? '',
            acctgTransTypeId: t.acctgTransTypeId ?? '',
            acctgTransTypeDescription: t.acctgTransTypeDescription ?? '',
            glFiscalTypeId: t.glFiscalTypeId ?? '',
            invoiceId: t.invoiceId,
            paymentId: t.paymentId,
            certificateNumber: t.certificateNumber,
            partyName: t.partyName,
            productName: t.productName,
            isPosted: t.isPosted === 'Y',
            debitCreditFlag: t.debitCreditFlag ?? 'C',
            amount: t.amount ?? 0,
            description: t.description,
            projectName: t.projectName ?? '',
        }));
    }, [transactions]);

    const pageChange = (event: any) => {
        setPage(event.page);
    };

    const rowRender = (trElement: React.ReactElement, props: any) => {
        const isDebit = props.dataItem.debitCreditFlag === 'D';
        const style = {
            backgroundColor: isDebit ? 'rgba(55, 180, 0, 0.15)' : '#ffffff'
        };
        return React.cloneElement(trElement, { style }, trElement.props.children);
    };

    return (
        <ModalContainer show={true} onClose={onClose} width={1280}>
            <Box sx={{ p: 3 }}>
                <Typography variant="h6" sx={{ mb: 3 }}>
                    {getTranslatedLabel(`${localizationKey}.title`, 'Transaction Details for')}
                    {' '}{data?.accountName} ({data?.accountCode})
                </Typography>

                {(isLoading || isFetching) && (
                    <LoadingComponent
                        message={getTranslatedLabel('general.loading-transactions', 'Loading Transactions...')}
                    />
                )}

                {data && (
                    <Grid container spacing={3}>
                        {/* Account Summary */}
                        <Grid item xs={12}>
                            <Box sx={{
                                p: 2,
                                border: '1px solid #e0e0e0',
                                borderRadius: 2,
                                backgroundColor: '#f9f9f9'
                            }}>
                                <Typography variant="body1">
                                    <strong>Opening Balance:</strong> {formatCurrency(data.openingBalance)}
                                </Typography>
                                <Typography variant="body1">
                                    <strong>Posted Debits:</strong> {formatCurrency(data.postedDebits)}
                                </Typography>
                                <Typography variant="body1">
                                    <strong>Posted Credits:</strong> {formatCurrency(data.postedCredits)}
                                </Typography>
                                <Typography variant="body1" sx={{ fontWeight: 'bold', mt: 1 }}>
                                    <strong>Ending Balance:</strong> {formatCurrency(data.endingBalance)}
                                </Typography>
                            </Box>
                        </Grid>

                        {/* Include Pre-Period Checkbox */}
                        <Grid item xs={12}>
                            <FormControlLabel
                                control={
                                    <Checkbox
                                        checked={includePrePeriod}
                                        onChange={(e) => setIncludePrePeriod(e.target.checked)}
                                    />
                                }
                                label={getTranslatedLabel(
                                    `${localizationKey}.includePrePeriod`,
                                    'Include transactions before the reporting period'
                                )}
                            />
                        </Grid>

                        {/* Transactions Grid */}
                        <Grid item xs={12}>
                            <KendoGrid
                                style={{ height: '460px' }}
                                data={orderBy(transactions, sort).slice(page.skip, page.skip + page.take)}
                                sortable={true}
                                sort={sort}
                                onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                                pageable={true}
                                skip={page.skip}
                                take={page.take}
                                total={transactions.length}
                                onPageChange={pageChange}
                                rowRender={rowRender}
                                resizable={true}
                            >
                                <GridToolbar>
                                    <Typography variant="h6" sx={{ flex: 1 }}>
                                        {getTranslatedLabel(`${localizationKey}.transactions`, 'Transactions')}
                                    </Typography>

                                    <GlAccountTransactionsExcel
                                        accountCode={data.accountCode ?? ''}
                                        accountName={data.accountName ?? ''}
                                        openingBalance={data.openingBalance ?? 0}
                                        postedDebits={data.postedDebits ?? 0}
                                        postedCredits={data.postedCredits ?? 0}
                                        endingBalance={data.endingBalance ?? 0}
                                        rows={excelRows}
                                        totalDebit={totalDebit}
                                        totalCredit={totalCredit}
                                        getTranslatedLabel={getTranslatedLabel}
                                        isFetching={isFetching}
                                    />
                                </GridToolbar>

                                <Column
                                    field="transactionDate"
                                    title={getTranslatedLabel(`${localizationKey}.transDate`, 'Date')}
                                    format="{0:dd/MM/yyyy}"
                                    width={110}
                                />
                                <Column
                                    field="acctgTransId"
                                    title={getTranslatedLabel(`${localizationKey}.transId`, 'Trans ID')}
                                    width={110}
                                />
                                <Column
                                    field="acctgTransTypeDescription"
                                    title={getTranslatedLabel(`${localizationKey}.transType`, 'Type')}
                                    width={160}
                                />
                                <Column
                                    field="debitCreditFlag"
                                    title={getTranslatedLabel(`${localizationKey}.debitCredit`, 'D/C')}
                                    width={70}
                                />
                                <Column
                                    field="amount"
                                    title={getTranslatedLabel(`${localizationKey}.amount`, 'Amount')}
                                    format="{0:n2}"
                                    width={130}
                                />
                                <Column
                                    field="description"
                                    title={getTranslatedLabel(`${localizationKey}.description`, 'Description')}
                                    width={280}
                                />
                                <Column
                                    field="partyName"
                                    title={getTranslatedLabel(`${localizationKey}.partyName`, 'Party')}
                                    width={180}
                                />
                                <Column
                                    field="productName"
                                    title={getTranslatedLabel(`${localizationKey}.productName`, 'Product')}
                                    width={150}
                                />
                                <Column
                                    field="certificateNumber"
                                    title={getTranslatedLabel(`${localizationKey}.certificateNumber`, 'Certificate')}
                                    width={120}
                                />
                            </KendoGrid>
                        </Grid>
                    </Grid>
                )}
            </Box>
        </ModalContainer>
    );
}