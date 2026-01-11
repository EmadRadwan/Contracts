import React, { useState, useMemo } from 'react';
import {
    Grid, Skeleton, Typography, Tabs, Tab, Box, Paper
} from '@mui/material';
import {
    Grid as KendoGrid, GridColumn as Column, GridPageChangeEvent,
    GridSortChangeEvent, GridCellProps
} from '@progress/kendo-react-grid';
import { orderBy, SortDescriptor, State } from '@progress/kendo-data-query';
import { Link } from 'react-router-dom';
import './partyFinancialHistory.css';
import { useGetPartyFinancialHistoryQuery } from "../../../app/store/apis";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useLocation } from 'react-router-dom';
import {PartyFinancialHistoryExcel} from "../report/PartyFinancialHistoryExcel";

interface Props { partyId: string; partyName?: string;}

interface LedgerRow {
    date: string;
    description: string;
    invoiceNumber?: string;
    paymentNumber?: string;
    value: number;      // raw invoice total
    toPay: number;      // for invoices (debit)
    paid: number;       // for payments (credit)
    balance: number;    // running balance
    notes?: string;
    transactionType?: 'invoice' | 'payment' | 'rentalPosting' | 'partnerAccrual'; // ← new
}

interface TabPanelProps {
    children?: React.ReactNode;
    index: number;
    value: number;
}

const TabPanel: React.FC<TabPanelProps> = ({ children, value, index }) => {
    return (
        <div role="tabpanel" hidden={value !== index} className="tab-panel">
            {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
        </div>
    );
};

const PartyFinancialHistory: React.FC<Props> = ({ partyId , partyName}) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'party.financial.history';
    const location = useLocation();
    const partyNameFromState = (location.state as { partyName?: string } | null)?.partyName;
    const displayName = partyNameFromState || getTranslatedLabel(`${localizationKey}.unknownParty`, 'Unknown Party');
    
    // ALL HOOKS — UNCONDITIONAL
    const { data, error, isLoading } = useGetPartyFinancialHistoryQuery(partyId, { skip: !partyId,
        refetchOnMountOrArgChange: true, });
    const [tabValue, setTabValue] = useState(0);
    const [sort, setSort] = useState<SortDescriptor[]>([{ field: 'invoiceDate', dir: 'asc' }]);
    const [page, setPage] = useState<State>({ skip: 0, take: 10 });

    const pageChange = (event: GridPageChangeEvent) => setPage(event.page);
    const sortChange = (event: GridSortChangeEvent) => setSort(event.sort);

    const isExternalView = useMemo(() => data?.LedgerPerspective?.startsWith('External') ?? false, [data]);

    const formatCurrency = useMemo(() => {
        return (value: number, currency?: string) => {
            const curr = currency || 'EGP';
            return new Intl.NumberFormat('en-US', { style: 'currency', currency: curr }).format(value);
        };
    }, []);

    const InvoiceIdCell = useMemo(() => (props: GridCellProps) => {
        const id = props.dataItem.invoiceId;
        return <td><Link to={`/accounting/invoices/${id}`}>[{id}]</Link></td>;
    }, []);

    const PaymentIdCell = useMemo(() => (props: GridCellProps) => {
        const id = props.dataItem.paymentId;
        return id ? <td><Link to={`/accounting/payments/${id}`}>[{id}]</Link></td> : <td>-</td>;
    }, []);

    const CurrencyCell = useMemo(() => (props: GridCellProps) => {
        const value = props.dataItem[props.field!];
        const currency = props.dataItem.currencyUomId || (data ? data.preferredCurrencyUomId : 'EGP');
        return <td className="currency-cell">{formatCurrency(value, currency)}</td>;
    }, [formatCurrency, data?.preferredCurrencyUomId]);

    const parseDate = useMemo(() => (dateStr: string | null | undefined): Date | null => {
        if (!dateStr) return null;
        const d = new Date(dateStr);
        return isNaN(d.getTime()) ? null : d;
    }, []);


    const ledgerItems = useMemo((): LedgerRow[] => {
        if (!data) return [];

        const rows: LedgerRow[] = [];
        let runningBalance = 0;

        const formatDate = (dateStr?: string | null) =>
            dateStr ? new Date(dateStr).toISOString().split('T')[0] : '';

        // 1. Opening Balance
        const openingTotalImpact = (data.openingBalances || []).reduce((sum, ob) => sum + ob.ImpactOnBalance, 0);
        if (openingTotalImpact !== 0) {
            const adjustedImpact = isExternalView ? -openingTotalImpact : openingTotalImpact;
            runningBalance += adjustedImpact;

            rows.push({
                date: '',
                description: getTranslatedLabel('openingBalance', 'الرصيد الإفتتاحي'),
                value: 0,
                toPay: adjustedImpact > 0 ? adjustedImpact : 0,
                paid: adjustedImpact < 0 ? -adjustedImpact : 0,
                balance: runningBalance,
                notes: data.openingBalances?.length ? `${data.openingBalances.length} حركات` : '',
            });
        }

        // 2. All transactions
        interface UnifiedTransaction {
            date: string;
            type: "invoice" | "payment" | "rental" | "partnerAccrual";
            id?: string;
            description: string;
            grossAmount: number;
            isSalesInvoice?: boolean;
            isPurchaseInvoice?: boolean;
            isReceiptFromParty?: boolean;
            isDisbursementToParty?: boolean;
            isRentalDebit?: boolean;
            isPartnerCredit?: boolean;
        }

        const transactions: UnifiedTransaction[] = [];

        // Invoices
        [...(data.invoicesApplPayments || []), ...(data.unappliedInvoices || [])].forEach((inv) => {
            const isSales = inv.InvoiceTypeId?.startsWith('SALES') || false;
            if (!transactions.some(t => t.id === inv.InvoiceId)) {
                transactions.push({
                    date: inv.InvoiceDate!,
                    type: 'invoice',
                    id: inv.InvoiceId,
                    description: `فاتورة ${inv.InvoiceId} ${inv.InvoiceTypeId || ''}${inv.UnappliedAmount ? ' (غير مطبقة)' : ''}`,
                    grossAmount: inv.Total ?? inv.Amount ?? 0,
                    isSalesInvoice: isSales,
                    isPurchaseInvoice: !isSales,
                });
            }
        });

        // Payments
        (data.unappliedPayments || []).forEach((pmt) => {
            const isReceipt = pmt.PaymentParentTypeId === 'RECEIPT';
            transactions.push({
                date: pmt.EffectiveDate!,
                type: 'payment',
                id: pmt.PaymentId,
                description: `دفعة ${pmt.PaymentId} - ${pmt.PaymentTypeDescription}`,
                grossAmount: pmt.Amount,
                isReceiptFromParty: isReceipt,
                isDisbursementToParty: !isReceipt,
            });
        });

        // Rental postings
        (data.rentalPropertyPostings || []).forEach((rp) => {
            if (!rp.TransactionDate) return;
            transactions.push({
                date: rp.TransactionDate,
                type: 'rental',
                description: rp.Description || `تسجيل إيجاري ${rp.GlAccountTypeId || ''}`,
                grossAmount: Math.abs(rp.ImpactOnBalance),
                isRentalDebit: rp.ImpactOnBalance > 0,
            });
        });

        // Partner accruals
        (data.partnerAccrualPostings || []).forEach((pa) => {
            if (!pa.TransactionDate) return;
            transactions.push({
                date: pa.TransactionDate,
                type: 'partnerAccrual',
                description: pa.Description || 'استحقاق شركاء',
                grossAmount: Math.abs(pa.ImpactOnBalance),
                isPartnerCredit: pa.ImpactOnBalance > 0,
            });
        });

        // Sort chronologically
        transactions.sort((a, b) => a.date.localeCompare(b.date));

        // Build rows
        transactions.forEach((t) => {
            let madin = 0; // مدين
            let dain = 0;  // دائن

            if (t.type === 'invoice') {
                if (t.isSalesInvoice) {
                    dain = t.grossAmount; // they owe us / I owe them (same column!)
                } else if (t.isPurchaseInvoice) {
                    isExternalView ? dain = t.grossAmount : madin = t.grossAmount;
                }
            } else if (t.type === 'payment') {
                if (t.isReceiptFromParty) {
                    isExternalView ? dain = t.grossAmount : madin = t.grossAmount;
                } else if (t.isDisbursementToParty) {
                    isExternalView ? madin = t.grossAmount : dain = t.grossAmount;
                }
            } else if (t.type === 'rental' && t.isRentalDebit) {
                dain = t.grossAmount;
            } else if (t.type === 'partnerAccrual' && t.isPartnerCredit) {
                isExternalView ? madin = t.grossAmount : dain = t.grossAmount;
            }

            const periodChange = isExternalView ? (dain - madin) : (madin - dain);
            runningBalance += periodChange;

            const notes = t.type === 'payment' && data.unappliedPayments?.some(p => p.PaymentId === t.id)
                ? 'غير مطبقة'
                : undefined;

            rows.push({
                date: formatDate(t.date),
                description: t.description,
                invoiceNumber: t.type === 'invoice' ? t.id : undefined,
                paymentNumber: t.type === 'payment' ? t.id : undefined,
                value: t.grossAmount,
                toPay: dain,
                paid: madin,
                balance: Math.round(runningBalance * 100) / 100,
                notes,
                transactionType: t.type,
            });
        });

        return rows;
    }, [data, isExternalView, getTranslatedLabel]);

    const processedData = useMemo(() => {
        if (!data) {
            return {
                invoices: [],
                unappliedInv: [],
                unappliedPay: [],
                billing: [],
                returns: [],
            };
        }

        const { invoicesApplPayments, unappliedInvoices, unappliedPayments, billingAccounts, returns } = data;

        const withDates = <T extends Record<string, any>>(items: T[], dateFields: (keyof T)[]): T[] => {
            return (items || []).map(item => {
                const copy = { ...item };
                dateFields.forEach(field => {
                    if (copy[field]) {
                        copy[field] = parseDate(copy[field] as unknown as string);
                    }
                });
                return copy;
            });
        };

        const invoices = withDates(invoicesApplPayments || [], ['invoiceDate', 'paymentEffectiveDate']);
        const unappliedInv = withDates(unappliedInvoices || [], ['invoiceDate']);
        const unappliedPay = withDates(unappliedPayments || [], ['effectiveDate']);


        return {
            invoices: orderBy(invoices, sort).slice(page.skip, page.skip + page.take),
            unappliedInv: orderBy(unappliedInv, sort).slice(page.skip, page.skip + page.take),
            unappliedPay: orderBy(unappliedPay, sort).slice(page.skip, page.skip + page.take),
            billing: orderBy(billingAccounts || [], sort).slice(page.skip, page.skip + page.take),
            returns: orderBy(returns || [], sort).slice(page.skip, page.skip + page.take),
        };
    }, [data, sort, page]);

    // EARLY RETURNS — AFTER ALL HOOKS
    if (!partyId) {
        return (
            <div className="empty-state">
                <Typography color="error">
                    {getTranslatedLabel(`${localizationKey}.noPartyId`, 'No Party ID')}
                </Typography>
            </div>
        );
    }

    if (isLoading) {
        return <LoadingSkeleton />;
    }

    if (error) {
        return (
            <div className="empty-state">
                <Typography color="error">
                    {getTranslatedLabel(`${localizationKey}.error`, 'Error loading data')} {JSON.stringify(error)}
                </Typography>
            </div>
        );
    }

    if (!data) {
        return (
            <div className="empty-state">
                <Typography>
                    {getTranslatedLabel(`${localizationKey}.noData`, 'No financial data available')}
                </Typography>
            </div>
        );
    }

    // SAFE: data is guaranteed
    const { financialSummary } = data;
    const currency = data.preferredCurrencyUomId || 'EGP';

    const gridProps = {
        sortable: true,
        sort,
        onSortChange: sortChange,
        pageable: true,
        onPageChange: pageChange,
        skip: page.skip,
        take: page.take,
    };
/*
    const renderExcelButton = () => {
        if (ledgerItems.length === 0) return null;

        return (
            <PartyFinancialHistoryExcel
                party={{
                    partyId,
                    partyName: displayName,
                }}
                ledgerItems={ledgerItems}
                getTranslatedLabel={getTranslatedLabel}
                isFetching={isLoading}
            />
        );
    };*/
    
    console.log('ledgerItems:', ledgerItems)
    console.log('data:', data)

    return (
        <Grid
            container
            direction="column"
            spacing={3}
            sx={{ p: 3 }}
            className="party-financial-history"
        >
           
            
            {/* Financial Summary */}
            <Paper elevation={2} sx={{ p: 3, mb: 3 }} className="financial-summary-card">
                <Grid container spacing={3}>
                    <Grid item xs={12} >
                        <Typography variant="h4" gutterBottom>
                            {getTranslatedLabel(`${localizationKey}.title`, 'Financial History for')} {displayName}
                        </Typography>
                    </Grid>
                    <Grid item xs={12} md={4} sx={{ textAlign: { md: 'right' } }}>
                        {ledgerItems.length > 0 && (
                            <PartyFinancialHistoryExcel
                                party={{ partyId, partyName: displayName }}
                                ledgerItems={ledgerItems}
                                getTranslatedLabel={getTranslatedLabel}
                                isFetching={isLoading}
                                perspective={data.LedgerPerspective || 'Company'}
                            />
                        )}
                    </Grid>
                </Grid>
                <Grid container spacing={3}>
                    <Grid item xs={12} md={6}>
                        <Typography>
                            <strong>{getTranslatedLabel(`${localizationKey}.totalSalesInvoices`, 'Total Sales Invoices')}</strong>: {formatCurrency(financialSummary.totalSalesInvoice, currency)}
                        </Typography>
                        <Typography>
                            <strong>{getTranslatedLabel(`${localizationKey}.totalPurchaseInvoices`, 'Total Purchase Invoices')}</strong>: {formatCurrency(financialSummary.totalPurchaseInvoice, currency)}
                        </Typography>
                        <Typography>
                            <strong>{getTranslatedLabel(`${localizationKey}.totalPaymentsIn`, 'Total Payments In')}</strong>: {formatCurrency(financialSummary.totalPaymentsIn, currency)}
                        </Typography>
                        <Typography>
                            <strong>{getTranslatedLabel(`${localizationKey}.totalPaymentsOut`, 'Total Payments Out')}</strong>: {formatCurrency(financialSummary.totalPaymentsOut, currency)}
                        </Typography>
                    </Grid>
                    <Grid item xs={12} md={6}>
                        <Typography>
                            <strong>{getTranslatedLabel(`${localizationKey}.totalInvoicesNotApplied`, 'Total Invoices Not Applied')}</strong>: {formatCurrency(financialSummary.totalInvoiceNotApplied, currency)}
                        </Typography>
                        <Typography>
                            <strong>{getTranslatedLabel(`${localizationKey}.totalPaymentsNotApplied`, 'Total Payments Not Applied')}</strong>: {formatCurrency(financialSummary.totalPaymentNotApplied, currency)}
                        </Typography>
                        {financialSummary.totalToBePaid > 0 && (
                            <Typography className="negative">
                                <strong>{getTranslatedLabel(`${localizationKey}.totalToBePaid`, 'Total To Be Paid')}</strong>: {formatCurrency(financialSummary.totalToBePaid, currency)}
                            </Typography>
                        )}
                        {financialSummary.totalToBeReceived > 0 && (
                            <Typography className="positive">
                                <strong>{getTranslatedLabel(`${localizationKey}.totalToBeReceived`, 'Total To Be Received')}</strong>: {formatCurrency(financialSummary.totalToBeReceived, currency)}
                            </Typography>
                        )}
                    </Grid>
                </Grid>
            </Paper>

            {/* Tabs */}
            <Tabs
                value={tabValue}
                onChange={(_, v) => setTabValue(v)}
                variant="scrollable"
                className="party-financial-tabs"
            >
                <Tab label={getTranslatedLabel('party.financial.history.tabs.invoicesPayments', 'Invoices & Payments')} />
                <Tab label={getTranslatedLabel('party.financial.history.tabs.unappliedInvoices', 'Unapplied Invoices')} />
                <Tab label={getTranslatedLabel('party.financial.history.tabs.unappliedPayments', 'Unapplied Payments')} />
                {/*<Tab label={getTranslatedLabel('party.financial.history.tabs.billingAccounts', 'Billing Accounts')} />
                <Tab label={getTranslatedLabel('party.financial.history.tabs.returns', 'Returns')} />*/}
            </Tabs>

            {/* Tab Panels */}
            <TabPanel value={tabValue} index={0}>
                <div className="party-financial-grid">
                    <KendoGrid data={processedData.invoices} total={data.invoicesApplPayments?.length || 0} {...gridProps} resizable>
                        <Column
                            field="invoiceId"
                            title={getTranslatedLabel('party.financial.history.grid.invoiceId', 'Invoice ID')}
                            cell={InvoiceIdCell}
                            width={150}
                        />
                        <Column
                            field="invoiceTypeId"
                            title={getTranslatedLabel('party.financial.history.grid.type', 'Type')}
                            width={130}
                        />
                        <Column
                            field="invoiceDate"
                            title={getTranslatedLabel('party.financial.history.grid.date', 'Date')}
                            format="{0:MM/dd/yyyy}"
                            width={120}
                        />
                        <Column
                            field="total"
                            title={getTranslatedLabel('party.financial.history.grid.total', 'Total')}
                            cell={CurrencyCell}
                            width={120}
                        />
                        <Column
                            field="amountApplied"
                            title={getTranslatedLabel('party.financial.history.grid.applied', 'Applied')}
                            cell={CurrencyCell}
                            width={120}
                        />
                        <Column
                            field="amountToApply"
                            title={getTranslatedLabel('party.financial.history.grid.toApply', 'To Apply')}
                            cell={CurrencyCell}
                            width={120}
                        />
                        <Column
                            field="paymentId"
                            title={getTranslatedLabel('party.financial.history.grid.paymentId', 'Payment ID')}
                            cell={PaymentIdCell}
                            width={150}
                        />
                        <Column
                            field="paymentEffectiveDate"
                            title={getTranslatedLabel('party.financial.history.grid.payDate', 'Pay Date')}
                            format="{0:MM/dd/yyyy}"
                            width={120}
                        />
                        <Column
                            field="paymentAmount"
                            title={getTranslatedLabel('party.financial.history.grid.payAmount', 'Pay Amount')}
                            cell={CurrencyCell}
                            width={120}
                        />
                    </KendoGrid>
                </div>
            </TabPanel>

            <TabPanel value={tabValue} index={1}>
                <div className="party-financial-grid">
                    <KendoGrid data={processedData.unappliedInv} total={data.unappliedInvoices?.length || 0} {...gridProps} resizable>
                        <Column
                            field="invoiceId"
                            title={getTranslatedLabel('party.financial.history.grid.invoiceId', 'Invoice ID')}
                            cell={InvoiceIdCell}
                            width={150}
                        />
                        <Column
                            field="typeDescription"
                            title={getTranslatedLabel('party.financial.history.grid.type', 'Type')}
                            width={180}
                        />
                        <Column
                            field="invoiceDate"
                            title={getTranslatedLabel('party.financial.history.grid.date', 'Date')}
                            format="{0:MM/dd/yyyy}"
                            width={120}
                        />
                        <Column
                            field="amount"
                            title={getTranslatedLabel('party.financial.history.grid.amount', 'Amount')}
                            cell={CurrencyCell}
                            width={120}
                        />
                        <Column
                            field="unappliedAmount"
                            title={getTranslatedLabel('party.financial.history.grid.unapplied', 'Unapplied')}
                            cell={CurrencyCell}
                            width={120}
                        />
                    </KendoGrid>
                </div>
            </TabPanel>

            <TabPanel value={tabValue} index={2}>
                <div className="party-financial-grid">
                    <KendoGrid data={processedData.unappliedPay} total={data.unappliedPayments?.length || 0} {...gridProps} resizable>
                        <Column
                            field="paymentId"
                            title={getTranslatedLabel('party.financial.history.grid.paymentId', 'Payment ID')}
                            cell={PaymentIdCell}
                            width={150}
                        />
                        <Column
                            field="effectiveDate"
                            title={getTranslatedLabel('party.financial.history.grid.date', 'Date')}
                            format="{0:MM/dd/yyyy}"
                            width={120}
                        />
                        <Column
                            field="paymentTypeDescription"
                            title={getTranslatedLabel('party.financial.history.grid.type', 'Type')}
                            width={180}
                        />
                        <Column
                            field="amount"
                            title={getTranslatedLabel('party.financial.history.grid.amount', 'Amount')}
                            cell={CurrencyCell}
                            width={120}
                        />
                        <Column
                            field="unappliedAmount"
                            title={getTranslatedLabel('party.financial.history.grid.unapplied', 'Unapplied')}
                            cell={CurrencyCell}
                            width={120}
                        />
                    </KendoGrid>
                </div>
            </TabPanel>

            <TabPanel value={tabValue} index={3}>
                <div className="party-financial-grid">
                    <KendoGrid data={processedData.billing} total={data.billingAccounts?.length || 0} {...gridProps}>
                        <Column
                            field="billingAccountId"
                            title={getTranslatedLabel('party.financial.history.grid.accountId', 'Account ID')}
                            width={150}
                        />
                        <Column
                            field="accountLimit"
                            title={getTranslatedLabel('party.financial.history.grid.limit', 'Limit')}
                            cell={CurrencyCell}
                            width={120}
                        />
                        <Column
                            field="accountBalance"
                            title={getTranslatedLabel('party.financial.history.grid.balance', 'Balance')}
                            cell={CurrencyCell}
                            width={120}
                        />
                        <Column
                            field="description"
                            title={getTranslatedLabel('party.financial.history.grid.description', 'Description')}
                            width={250}
                        />
                    </KendoGrid>
                </div>
            </TabPanel>

            <TabPanel value={tabValue} index={4}>
                <div className="party-financial-grid">
                    <KendoGrid data={processedData.returns} total={data.returns?.length || 0} {...gridProps}>
                        <Column
                            field="returnId"
                            title={getTranslatedLabel('party.financial.history.grid.returnId', 'Return ID')}
                            width={150}
                        />
                        <Column
                            field="statusDescription"
                            title={getTranslatedLabel('party.financial.history.grid.status', 'Status')}
                            width={150}
                        />
                        <Column
                            field="fromPartyId"
                            title={getTranslatedLabel('party.financial.history.grid.from', 'From')}
                            width={120}
                        />
                        <Column
                            field="toPartyId"
                            title={getTranslatedLabel('party.financial.history.grid.to', 'To')}
                            width={120}
                        />
                    </KendoGrid>
                </div>
            </TabPanel>
        </Grid>
    );
};

const LoadingSkeleton = () => (
    <Grid container spacing={3} sx={{ p: 3 }} className="loading-skeleton">
        {[...Array(5)].map((_, i) => (
            <Grid item xs={12} key={i}>
                <Skeleton variant="rectangular" height={300} animation="wave" />
            </Grid>
        ))}
    </Grid>
);

export default PartyFinancialHistory;