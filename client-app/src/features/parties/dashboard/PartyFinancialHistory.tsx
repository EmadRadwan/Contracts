import React, {useState} from 'react';
import {Grid, Skeleton, Typography} from '@mui/material';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridCellProps,
} from '@progress/kendo-react-grid';
import {orderBy, SortDescriptor, State} from '@progress/kendo-data-query';
import {Link} from 'react-router-dom';
import './partyFinancialHistory.css';
import {useGetPartyFinancialHistoryQuery} from "../../../app/store/apis";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";

interface Props {
    partyId: string;
}

const PartyFinancialHistory: React.FC<Props> = ({partyId}) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = 'party.financial.history';
    const {data, error, isLoading} = useGetPartyFinancialHistoryQuery(partyId, {
        skip: !partyId,
    });

    // Grid state management
    const initialSort: Array<SortDescriptor> = [{field: 'InvoiceDate', dir: 'asc'}];
    const [sort, setSort] = useState(initialSort);
    const initialDataState: State = {skip: 0, take: 10};
    const [page, setPage] = useState<State>(initialDataState);

    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    const sortChange = (event: GridSortChangeEvent) => {
        setSort(event.sort);
    };

    if (!partyId) {
        return (
            <Typography color="error">
                {getTranslatedLabel(`${localizationKey}.noPartyId`, 'Party ID is required.')}
            </Typography>
        );
    }

    if (isLoading) {
        return (
            <Grid container spacing={2} direction="column">
                {[...Array(5)].map((_, index) => (
                    <Grid item key={index}>
                        <Skeleton animation="wave" variant="rounded" height={40} sx={{width: '100%'}}/>
                    </Grid>
                ))}
            </Grid>
        );
    }

    if (error) {
        return (
            <Typography color="error">
                {getTranslatedLabel(`${localizationKey}.error`, 'Error: ')}{JSON.stringify(error)}
            </Typography>
        );
    }

    if (!data) {
        return (
            <Typography>
                {getTranslatedLabel(`${localizationKey}.noData`, 'No data available.')}
            </Typography>
        );
    }

  const {
    financialSummary,
    invoicesApplPayments,
    unappliedInvoices,
    unappliedPayments,
    billingAccounts,
    returns,
    preferredCurrencyUomId
  } = data;
    
    console.log('PartyFinancialHistory data:', data);
    console.log('invoicesApplPayments data:', invoicesApplPayments);

    const formatCurrency = (value: number) => {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: preferredCurrencyUomId || 'USD',
        }).format(value);
    };

    const InvoiceIdCell = (props: GridCellProps) => {
        return (
            <td>
                <Link to={`/accounting/invoices/${props.dataItem.InvoiceId}`}>
                    [{props.dataItem.invoiceId}]
                </Link>
            </td>
        );
    };

    const PaymentIdCell = (props: GridCellProps) => {
        return (
            <td>
                {props.dataItem.PaymentId ? (
                    <Link to={`/accounting/payments/${props.dataItem.PaymentId}`}>
                        [{props.dataItem.PaymentId}]
                    </Link>
                ) : (
                    ''
                )}
            </td>
        );
    };

    const CurrencyCell = (props: GridCellProps) => {
        return <td>{formatCurrency(props.dataItem[props.field!])}</td>;
    };

    return (
        <Grid container direction="column" spacing={2} sx={{mt: 1, p: 3}}>
            <Typography variant="h4" gutterBottom>
                {getTranslatedLabel(`${localizationKey}.title`, 'Party Financial History')}
            </Typography>

            {/* Financial Summary */}
          <Grid item>
            <Grid container spacing={2} sx={{ bgcolor: '#fff', p: 2, borderRadius: 1, boxShadow: 1 }}>
              <Grid item xs={12}>
                <Typography variant="h6">
                  {getTranslatedLabel(`${localizationKey}.financialSummary`, 'Financial Summary')}
                </Typography>
              </Grid>
              <Grid item xs={6}>
                {financialSummary ? (
                    <>
                      <Typography>
                        {getTranslatedLabel(`${localizationKey}.totalSalesInvoices`, 'Total Sales Invoices')}: {formatCurrency(financialSummary.totalSalesInvoice || 0)}
                      </Typography>
                      <Typography>
                        {getTranslatedLabel(`${localizationKey}.totalPurchaseInvoices`, 'Total Purchase Invoices')}: {formatCurrency(financialSummary.totalPurchaseInvoice || 0)}
                      </Typography>
                      <Typography>
                        {getTranslatedLabel(`${localizationKey}.totalPaymentsIn`, 'Total Payments In')}: {formatCurrency(financialSummary.totalPaymentsIn || 0)}
                      </Typography>
                      <Typography>
                        {getTranslatedLabel(`${localizationKey}.totalPaymentsOut`, 'Total Payments Out')}: {formatCurrency(financialSummary.totalPaymentsOut || 0)}
                      </Typography>
                    </>
                ) : (
                    <Typography>
                      {getTranslatedLabel(`${localizationKey}.nofinancialSummary`, 'No financial summary available.')}
                    </Typography>
                )}
              </Grid>
              <Grid item xs={6}>
                {financialSummary ? (
                    <>
                      <Typography>
                        {getTranslatedLabel(`${localizationKey}.totalInvoicesNotApplied`, 'Total Invoices Not Applied')}: {formatCurrency(financialSummary.totalInvoiceNotApplied || 0)}
                      </Typography>
                      <Typography>
                        {getTranslatedLabel(`${localizationKey}.totalPaymentsNotApplied`, 'Total Payments Not Applied')}: {formatCurrency(financialSummary.totalPaymentNotApplied || 0)}
                      </Typography>
                      {financialSummary.TotalToBePaid !== undefined && (
                          <Typography>
                            {getTranslatedLabel(`${localizationKey}.totalToBePaid`, 'Total To Be Paid')}: {formatCurrency(financialSummary.totalToBePaid)}
                          </Typography>
                      )}
                      {financialSummary.TotalToBeReceived !== undefined && (
                          <Typography>
                            {getTranslatedLabel(`${localizationKey}.totalToBeReceived`, 'Total To Be Received')}: {formatCurrency(financialSummary.totalToBeReceived)}
                          </Typography>
                      )}
                    </>
                ) : null}
              </Grid>
            </Grid>
          </Grid>
            {/* Invoices and Applied Payments */}
            <Grid item>
                <KendoGrid
                    className="main-grid"
                    style={{height: '40vh'}}
                    data={orderBy(invoicesApplPayments || [], sort).slice(page.skip, page.take + page.skip)}
                    sortable
                    sort={sort}
                    onSortChange={sortChange}
                    skip={page.skip}
                    take={page.take}
                    total={(invoicesApplPayments || []).length }
                    pageable
                    onPageChange={pageChange}
                >
                    <Column
                        field="invoiceId"
                        title={getTranslatedLabel(`${localizationKey}.invoiceId`, 'Invoice ID')}
                        cell={InvoiceIdCell}
                        width={150}
                    />
                    <Column
                        field="invoiceTypeId"
                        title={getTranslatedLabel(`${localizationKey}.invoiceType`, 'Invoice Type')}
                        width={120}
                    />
                    <Column
                        field="invoiceDate"
                        title={getTranslatedLabel(`${localizationKey}.invoiceDate`, 'Invoice Date')}
                        format="{0:MM/dd/yyyy}"
                        width={120}
                    />
                    <Column
                        field="total"
                        title={getTranslatedLabel(`${localizationKey}.total`, 'Total')}
                        cell={CurrencyCell}
                        width={120}
                    />
                    <Column
                        field="amountApplied"
                        title={getTranslatedLabel(`${localizationKey}.amountApplied`, 'Amount Applied')}
                        cell={CurrencyCell}
                        width={120}
                    />
                    <Column
                        field="amountToApply"
                        title={getTranslatedLabel(`${localizationKey}.amountToApply`, 'Amount To Apply')}
                        cell={CurrencyCell}
                        width={120}
                    />
                    <Column
                        field="paymentId"
                        title={getTranslatedLabel(`${localizationKey}.paymentId`, 'Payment ID')}
                        cell={PaymentIdCell}
                        width={150}
                    />
                    <Column
                        field="paymentEffectiveDate"
                        title={getTranslatedLabel(`${localizationKey}.paymentEffectiveDate`, 'Effective Date')}
                        format="{0:MM/dd/yyyy}"
                        width={120}
                    />
                    <Column
                        field="paymentAmount"
                        title={getTranslatedLabel(`${localizationKey}.paymentAmount`, 'Payment Amount')}
                        cell={CurrencyCell}
                        width={120}
                    />
                </KendoGrid>
            </Grid>

            {/* Unapplied Invoices */}
            <Grid item>
                <KendoGrid
                    className="main-grid"
                    style={{height: '40vh'}}
                    data={orderBy(unappliedInvoices || [], sort).slice(page.skip, page.take + page.skip)}
                    sortable
                    sort={sort}
                    onSortChange={sortChange}
                    skip={page.skip}
                    take={page.take}
                    total={(unappliedInvoices || []).length}
                    pageable
                    onPageChange={pageChange}
                >
                    <Column
                        field="invoiceId"
                        title={getTranslatedLabel(`${localizationKey}.invoiceId`, 'Invoice ID')}
                        cell={InvoiceIdCell}
                        width={150}
                    />
                    <Column
                        field="typeDescription"
                        title={getTranslatedLabel(`${localizationKey}.type`, 'Type')}
                        width={150}
                    />
                    <Column
                        field="invoiceDate"
                        title={getTranslatedLabel(`${localizationKey}.date`, 'Date')}
                        format="{0:MM/dd/yyyy}"
                        width={120}
                    />
                    <Column
                        field="amount"
                        title={getTranslatedLabel(`${localizationKey}.amount`, 'Amount')}
                        cell={CurrencyCell}
                        width={120}
                    />
                    <Column
                        field="unappliedAmount"
                        title={getTranslatedLabel(`${localizationKey}.unappliedAmount`, 'Unapplied Amount')}
                        cell={CurrencyCell}
                        width={120}
                    />
                </KendoGrid>
            </Grid>

            {/* Unapplied Payments */}
            <Grid item>
                <KendoGrid
                    className="main-grid"
                    style={{height: '40vh'}}
                    data={orderBy(unappliedPayments || [], sort).slice(page.skip, page.take + page.skip)}
                    sortable
                    sort={sort}
                    onSortChange={sortChange}
                    skip={page.skip}
                    take={page.take}
                    total={(unappliedPayments || []).length}
                    pageable
                    onPageChange={pageChange}
                >
                    <Column
                        field="paymentId"
                        title={getTranslatedLabel(`${localizationKey}.paymentId`, 'Payment ID')}
                        cell={PaymentIdCell}
                        width={150}
                    />
                    <Column
                        field="effectiveDate"
                        title={getTranslatedLabel(`${localizationKey}.effectiveDate`, 'Effective Date')}
                        format="{0:MM/dd/yyyy}"
                        width={120}
                    />
                    <Column
                        field="paymentTypeDescription"
                        title={getTranslatedLabel(`${localizationKey}.paymentType`, 'Payment Type')}
                        width={150}
                    />
                    <Column
                        field="amount"
                        title={getTranslatedLabel(`${localizationKey}.amount`, 'Amount')}
                        cell={CurrencyCell}
                        width={120}
                    />
                    <Column
                        field="unappliedAmount"
                        title={getTranslatedLabel(`${localizationKey}.unappliedAmount`, 'Unapplied Amount')}
                        cell={CurrencyCell}
                        width={120}
                    />
                </KendoGrid>
            </Grid>

            {/* Billing Accounts */}
            <Grid item>
                <KendoGrid
                    className="main-grid"
                    style={{height: '40vh'}}
                    data={orderBy(billingAccounts || [], sort).slice(page.skip, page.take + page.skip)}
                    sortable
                    sort={sort}
                    onSortChange={sortChange}
                    skip={page.skip}
                    take={page.take}
                    total={(billingAccounts || []).length}
                    pageable
                    onPageChange={pageChange}
                >
                    <Column
                        field="billingAccountId"
                        title={getTranslatedLabel(`${localizationKey}.billingAccountId`, 'Billing Account ID')}
                        width={150}
                    />
                    <Column
                        field="accountLimit"
                        title={getTranslatedLabel(`${localizationKey}.accountLimit`, 'Account Limit')}
                        cell={CurrencyCell}
                        width={120}
                    />
                    <Column
                        field="accountBalance"
                        title={getTranslatedLabel(`${localizationKey}.accountBalance`, 'Account Balance')}
                        cell={CurrencyCell}
                        width={120}
                    />
                    <Column
                        field="description"
                        title={getTranslatedLabel(`${localizationKey}.description`, 'Description')}
                        width={200}
                    />
                </KendoGrid>
            </Grid>

            {/* returns */}
            <Grid item>
                <KendoGrid
                    className="main-grid"
                    style={{height: '40vh'}}
                    data={orderBy(returns || [], sort).slice(page.skip, page.take + page.skip)}
                    sortable
                    sort={sort}
                    onSortChange={sortChange}
                    skip={page.skip}
                    take={page.take}
                    total={(returns || []).length}
                    pageable
                    onPageChange={pageChange}
                >
                    <Column
                        field="returnId"
                        title={getTranslatedLabel(`${localizationKey}.returnId`, 'Return ID')}
                        width={150}
                    />
                    <Column
                        field="statusDescription"
                        title={getTranslatedLabel(`${localizationKey}.status`, 'Status')}
                        width={150}
                    />
                    <Column
                        field="fromPartyId"
                        title={getTranslatedLabel(`${localizationKey}.fromPartyId`, 'From Party')}
                        width={120}
                    />
                    <Column
                        field="toPartyId"
                        title={getTranslatedLabel(`${localizationKey}.toPartyId`, 'To Party')}
                        width={120}
                    />
                </KendoGrid>
            </Grid>
        </Grid>
    );
};

export default PartyFinancialHistory;