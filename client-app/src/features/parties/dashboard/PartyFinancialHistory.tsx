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

        const isExternalView = data.ledgerPerspective?.startsWith("External") ?? false;

        const rows: LedgerRow[] = [];
        let runningBalance = 0;

        const formatDate = (dateStr?: string | null) =>
            dateStr ? new Date(dateStr).toISOString().split("T")[0] : "";

        // 1. Opening Balance – fixed columns based on debitCreditFlag
        const openingEntries = data.openingBalances || [];

        if (openingEntries.length > 0) {
            let totalMadin = 0;
            let totalDain = 0;
            let netImpact = 0;
            let transactionDate = "";

            openingEntries.forEach((ob) => {
                const rawAmount = Number(ob?.amount || 0);
                const flag = (ob?.debitCreditFlag || "").trim().toUpperCase();
                transactionDate = ob.transactionDate || transactionDate;
                if (flag === "D") {
                    totalMadin += rawAmount;
                    netImpact += rawAmount;     // Debit → increases receivable
                } else if (flag === "C") {
                    totalDain += rawAmount;
                    netImpact -= rawAmount;     // Credit → increases payable
                }
            });

            // Flip net balance sign for external view (we owe = negative)
            const viewAdjustedNet = netImpact;
            runningBalance += viewAdjustedNet;

            rows.push({
                date: formatDate(transactionDate),
                description: getTranslatedLabel("openingBalance", "الرصيد الإفتتاحي"),
                value: 0,
                toPay: totalDain,          // دائن = total credits (fixed)
                paid: totalMadin,          // مدين = total debits (fixed)
                balance: Math.round(runningBalance * 100) / 100
            });
        }

        // 2. Transactions – fixed column placement (no isExternalView here)
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
            debitCreditFlag?: string;
        }
        const transactions: UnifiedTransaction[] = [];

        // Invoices (fixed: sales → دائن, purchase → مدين)
        [...(data.invoicesApplPayments || []), ...(data.unappliedInvoices || [])].forEach((inv) => {
            if (!inv.invoiceDate || !inv.invoiceId) return;
            const isSales = inv.invoiceTypeId?.startsWith("SALES") || false;

            if (!transactions.some(t => t.id === inv.invoiceId)) {
                transactions.push({
                    date: inv.invoiceDate,
                    type: "invoice",
                    id: inv.invoiceId,
                    description: `فاتورة ${inv.invoiceId} ${inv.invoiceTypeId || ""}${inv.unappliedAmount ? " (غير مطبقة)" : ""}`,
                    grossAmount: Number(inv.total ?? inv.amount ?? 0),
                    isSalesInvoice: isSales,
                    isPurchaseInvoice: !isSales,
                });
            }
        });

        // Payments (fixed: receipt → دائن, disbursement → مدين)
        (data.unappliedPayments || []).forEach((pmt) => {
            if (!pmt.effectiveDate || !pmt.paymentId) return;
            const isReceipt = pmt.paymentParentTypeId === "RECEIPT";

            transactions.push({
                date: pmt.effectiveDate,
                type: "payment",
                id: pmt.paymentId,
                description: `دفعة ${pmt.paymentId} - ${pmt.paymentTypeDescription || "غير معروف"}`,
                grossAmount: Number(pmt.amount || 0),
                isReceiptFromParty: isReceipt,
                isDisbursementToParty: !isReceipt,
            });
        });

        // C. Rental Property Postings – now use debitCreditFlag
        (data.rentalPropertyPostings || []).forEach((rp) => {
            if (!rp.transactionDate) return;

            const rawAmount = Number(rp.amount || 0);
            const flag = (rp.debitCreditFlag || "").trim().toUpperCase();

            if (rawAmount === 0) return; // skip zero amounts

            transactions.push({
                date: rp.transactionDate,
                type: "rental",
                description: rp.description || `تسجيل إيجاري ${rp.glAccountTypeId || ""}`,
                grossAmount: rawAmount,
                debitCreditFlag: rp.debitCreditFlag,
            });
        });


        // D. Partner Accrual Postings – now use debitCreditFlag
        (data.partnerAccrualPostings || []).forEach((pa) => {
            if (!pa.transactionDate) return;

            const rawAmount = Number(pa.amount || 0);
            const flag = (pa.debitCreditFlag || "").trim().toUpperCase();

            if (rawAmount === 0) return;

            transactions.push({
                date: pa.transactionDate,
                type: "partnerAccrual",
                description: pa.description || "استحقاق شركاء",
                grossAmount: rawAmount,
                debitCreditFlag: pa.debitCreditFlag,
            });
        });

        // 3. Sort (unchanged)
        transactions.sort((a, b) => {
            const dateA = a.date || "9999-12-31";
            const dateB = b.date || "9999-12-31";
            return dateA.localeCompare(dateB);
        });

        // 4. Build rows – explicit periodChange per transaction type
        transactions.forEach((t) => {
            let madin = 0; // مدين
            let dain = 0;  // دائن
            let periodChange = 0;

            if (t.type === "invoice") {
                if (t.isSalesInvoice) {
                    dain = t.grossAmount;      // sales → party owes us more → balance increases
                    periodChange = +t.grossAmount;  // positive
                } else if (t.isPurchaseInvoice) {
                    madin = t.grossAmount;     // purchase → we owe more → balance decreases
                    periodChange = -t.grossAmount;  // negative
                }
            } else if (t.type === "payment") {
                if (t.isReceiptFromParty) {
                    dain = t.grossAmount;      // party paid us → we owe more → balance decreases
                    periodChange = -t.grossAmount;  // negative
                } else if (t.isDisbursementToParty) {
                    madin = t.grossAmount;     // we paid party → owe less → balance increases
                    periodChange = +t.grossAmount;  // positive
                }
            } else if (t.type === "rental") {
                const flag = (t.debitCreditFlag || "").trim().toUpperCase();
                if (flag === "D") {
                    madin = t.grossAmount;
                    periodChange = +t.grossAmount;  // rental debit → owe more → negative
                } else if (flag === "C") {
                    dain = t.grossAmount;
                    periodChange = -t.grossAmount;  // rental credit → owe less → positive
                }
            } else if (t.type === "partnerAccrual") {
                const flag = (t.debitCreditFlag || "").trim().toUpperCase();
                if (flag === "D") {
                    madin = t.grossAmount;
                    periodChange = +t.grossAmount;  // accrual debit → owe less → positive
                } else if (flag === "C") {
                    dain = t.grossAmount;
                    periodChange = -t.grossAmount;  // accrual credit → owe more → negative
                }
            }

            runningBalance += periodChange;
            
            rows.push({
                date: formatDate(t.date),
                description: t.description,
                invoiceNumber: t.type === "invoice" ? t.id : undefined,
                paymentNumber: t.type === "payment" ? t.id : undefined,
                value: t.grossAmount,
                toPay: dain,
                paid: madin,
                balance: Math.round(runningBalance * 100) / 100,
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