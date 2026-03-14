// InvoiceTransactionsList.tsx
import { orderBy, SortDescriptor } from "@progress/kendo-data-query";
import React, { Fragment, useEffect, useMemo, useState } from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridSortChangeEvent,
    GridRowProps,
    GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import { Grid, Typography, Box } from "@mui/material";
import { TabContext, TabPanel } from "@mui/lab";
import { StyledTabs } from "../../../../app/components/StyledTabs";
import { StyledTab } from "../../../../app/components/StyledTab";
import {
    useFetchInvoiceAcctTransEntriesQuery,
    useGetGlAccountDiagramQuery,
} from "../../../../app/store/apis";
import { handleDatesArray } from "../../../../app/util/utils";
import { AcctgTransEntry } from "../../../../app/models/accounting/acctgTransEntry";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import MermaidChart from "../../../manufacturing/dashboard/MermaidChart";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

interface Props {
    onClose: () => void;
    invoiceId: string | undefined;
    invoiceType: string | undefined;
}

export default function InvoiceTransactionsList({ onClose, invoiceId, invoiceType }: Props) {
    // ───────────────────── State ─────────────────────────────
    const initialSort: SortDescriptor[] = [
        { field: "acctgTransEntrySeqId", dir: "asc" },
    ];
    const [sort, setSort] = useState(initialSort);
    const [acctTransEntries, setAcctTransEntries] = useState<AcctgTransEntry[]>([]);
    const [paymentTransEntries, setPaymentTransEntries] = useState<AcctgTransEntry[]>([]);
    const [selectedAcctgTransId, setSelectedAcctgTransId] = useState<string | null>(null);
    const [showDiagramModal, setShowDiagramModal] = useState(false);
    const [tabValue, setTabValue] = useState("1");
    const { getTranslatedLabel } = useTranslationHelper();

    // ───────────────────── Data Fetching ─────────────────────────────
    const { data: acctTransEntryDataInvoice } = useFetchInvoiceAcctTransEntriesQuery(
        { invoiceId, acctgTransTypeId: invoiceType || "SALES_INVOICE" }, // Fallback to "SALES_INVOICE" if invoiceType is undefined
        { skip: !invoiceId || !invoiceType }
    );

    const { data: acctTransEntryDataPaymentApplication } = useFetchInvoiceAcctTransEntriesQuery(
        { invoiceId, acctgTransTypeId: "PAYMENT_APPL" },
        { skip: !invoiceId }
    );
    

    useEffect(() => {
        if (acctTransEntryDataInvoice) {
            setAcctTransEntries(handleDatesArray(acctTransEntryDataInvoice));
        }
    }, [acctTransEntryDataInvoice]);

    useEffect(() => {
        if (acctTransEntryDataPaymentApplication) {
            setPaymentTransEntries(handleDatesArray(acctTransEntryDataPaymentApplication));
        }
    }, [acctTransEntryDataPaymentApplication]);
    

    /** Row colouring: credits white, debits light‑green */
    const rowRender = (
        trElement: React.ReactElement<HTMLTableRowElement>,
        props: GridRowProps
    ) => {
        const isDebit = props.dataItem.debitCreditFlag !== "C";
        const style = { backgroundColor: isDebit ? "rgba(55,180,0,0.32)" : "#fff" };
        return React.cloneElement(trElement, { style }, trElement.props.children);
    };

    // ───────────────────── Totals Calculation for Sales Invoice ─────────────────────────────
    const { totalDebit, totalCredit } = useMemo(() => {
        return acctTransEntries.reduce(
            (totals, e) => {
                if (e.debitCreditFlag === "C") {
                    totals.totalCredit += e.origAmount ?? 0;
                } else {
                    totals.totalDebit += e.origAmount ?? 0;
                }
                return totals;
            },
            { totalDebit: 0, totalCredit: 0 }
        );
    }, [acctTransEntries]);

    const TotalsFooterCell = () => (
        <td colSpan={15} style={{ fontWeight: "bold", color: "#1565C0" }}>
            {getTranslatedLabel("accounting.transactions.totalDebit", "Total Debit")}: {totalDebit.toFixed(2)} | {getTranslatedLabel("accounting.transactions.totalCredit", "Total Credit")}: {totalCredit.toFixed(2)}
        </td>
    );

    // ───────────────────── Totals Calculation for Payment Application ─────────────────────────────
    const { totalDebit: totalDebitPayment, totalCredit: totalCreditPayment } = useMemo(() => {
        return paymentTransEntries.reduce(
            (totals, e) => {
                if (e.debitCreditFlag === "C") {
                    totals.totalCredit += e.origAmount ?? 0;
                } else {
                    totals.totalDebit += e.origAmount ?? 0;
                }
                return totals;
            },
            { totalDebit: 0, totalCredit: 0 }
        );
    }, [paymentTransEntries]);

    const PaymentTotalsFooterCell = () => (
        <td colSpan={15} style={{ fontWeight: "bold", color: "#1565C0" }}>
            {getTranslatedLabel("accounting.transactions.totalDebit", "Total Debit")}: {totalDebitPayment.toFixed(2)} | {getTranslatedLabel("accounting.transactions.totalCredit", "Total Credit")}: {totalCreditPayment.toFixed(2)}
        </td>
    );

    const handleTabChange = (event: React.SyntheticEvent, newValue: string) => {
        setTabValue(newValue);
    };

    // ───────────────────── Render ─────────────────────────────
    return (
        <Fragment>
            <Box sx={{ width: "100%", p: 2 }}>
                <TabContext value={tabValue}>
                    <StyledTabs value={tabValue} onChange={handleTabChange}>
                        <StyledTab label={getTranslatedLabel("accounting.invoices.display.form.actions.invoice", "Sales Invoice")} value="1" />
                        <StyledTab label={getTranslatedLabel("accounting.invoices.display.form.actions.payment-applications", "Payment Application")} value="2" />
                    </StyledTabs>
                    <TabPanel value="1">
                        <Grid container>
                            <Grid item xs={12}>
                                <KendoGrid
                                    style={{  height: "450px", width: 850 }}
                                    data={orderBy(acctTransEntries, sort)}
                                    sortable
                                    sort={sort}
                                    onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                                    pageable={false}
                                    resizable
                                    rowRender={rowRender}
                                >
                                    <Column
                                        field="acctgTransId"
                                        title={getTranslatedLabel("accounting.payments.transactions.columns.acctgTransId", "Acctg Trans")}
                                        width={100}
                                        footerCell={TotalsFooterCell}
                                    />
                                    <Column field="amount" title={getTranslatedLabel("accounting.payments.transactions.columns.origAmount", "Orig Amount")} width={100} />
                                    <Column field="debitCreditFlag" title={getTranslatedLabel("accounting.payments.transactions.columns.debitCreditFlag", "Debit/Credit")} width={90} />
                                    <Column field="glAccountId" title={getTranslatedLabel("accounting.payments.transactions.columns.glAccountId", "GL Account")} width={100} />
                                    <Column field="glAccountTypeDescription" title={getTranslatedLabel("accounting.payments.transactions.columns.glAccountTypeDescription", "Account Name")} width={400} />
                                    <Column field="productName" title={getTranslatedLabel("accounting.payments.transactions.columns.productName", "Product")} width={200} />
                                    <Column field="isPosted" title={getTranslatedLabel("accounting.payments.transactions.columns.isPosted", "Posted?")} width={80} />
                                    <Column field="acctgTransTypeDescription" title={getTranslatedLabel("accounting.payments.transactions.columns.acctgTransType", "Trans Type")} width={130} />
                                    <Column
                                        field="transactionDate"
                                        title={getTranslatedLabel("accounting.transactions.transactionDate", "Trans Date")}
                                        width={130}
                                        format="{0:dd/MM/yyyy}"
                                    />
                                    <Column
                                        field="postedDate"
                                        title={getTranslatedLabel("accounting.transactions.postedDate", "Posted Date")}
                                        width={130}
                                        format="{0:dd/MM/yyyy}"
                                    />
                                    <Column field="glAccountClassDescription" title={getTranslatedLabel("accounting.transactions.accountClass", "Account Class")} width={140} />
                                    <Column field="origCurrencyUomId" title={getTranslatedLabel("accounting.transactions.currency", "Currency")} width={90} />
                                    
                                </KendoGrid>
                            </Grid>
                        </Grid>
                    </TabPanel>
                    <TabPanel value="2">
                        <Grid container>
                            <Grid item xs={12}>
                                <KendoGrid
                                    style={{ height: "450px", width: 850 }}
                                    data={orderBy(paymentTransEntries, sort)}
                                    sortable
                                    sort={sort}
                                    onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                                    pageable={false}
                                    resizable
                                    rowRender={rowRender}
                                >
                                    
                                    {/* ───── Columns ───── */}
                                    <Column
                                        field="acctgTransId"
                                        title={getTranslatedLabel("accounting.transactions.acctgTransId", "Acctg Trans")}
                                        width={100}
                                        footerCell={PaymentTotalsFooterCell}
                                    />
                                    <Column field="amount" title={getTranslatedLabel("accounting.transactions.origAmount", "Orig Amount")} width={100} />
                                    <Column field="debitCreditFlag" title={getTranslatedLabel("accounting.transactions.debitCredit", "Debit/Credit")} width={90} />
                                    <Column field="glAccountId" title={getTranslatedLabel("accounting.transactions.glAccountId", "GL Account")} width={100} />
                                    <Column field="glAccountTypeDescription" title={getTranslatedLabel("accounting.transactions.accountName", "Account Name")} width={300} />
                                    <Column field="productName" title={getTranslatedLabel("accounting.transactions.productName", "Product")} width={200} />
                                    <Column field="isPosted" title={getTranslatedLabel("accounting.transactions.isPosted", "Posted?")} width={80} />
                                    <Column field="glFiscalTypeId" title={getTranslatedLabel("accounting.transactions.glFiscalType", "Fiscal Type")} width={100} />
                                    <Column field="acctgTransTypeDescription" title={getTranslatedLabel("accounting.transactions.acctgTransType", "Trans Type")} width={130} />
                                    <Column
                                        field="transactionDate"
                                        title={getTranslatedLabel("accounting.transactions.transactionDate", "Trans Date")}
                                        width={130}
                                        format="{0:dd/MM/yyyy}"
                                    />
                                    <Column
                                        field="postedDate"
                                        title={getTranslatedLabel("accounting.transactions.postedDate", "Posted Date")}
                                        width={130}
                                        format="{0:dd/MM/yyyy}"
                                    />
                                    <Column field="glAccountClassDescription" title={getTranslatedLabel("accounting.transactions.accountClass", "Account Class")} width={140} />
                                    <Column field="origCurrencyUomId" title={getTranslatedLabel("accounting.transactions.currency", "Currency")} width={90} />
                                </KendoGrid>
                            </Grid>
                        </Grid>
                    </TabPanel>
                </TabContext>
            </Box>

            {/* ───────── Diagram Modal ───────── */}
        </Fragment>
    );
}
