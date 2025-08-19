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

    // ───────────────────── Data Fetching ─────────────────────────────
    const { data: acctTransEntryDataInvoice } = useFetchInvoiceAcctTransEntriesQuery(
        { invoiceId, acctgTransTypeId: invoiceType || "SALES_INVOICE" }, // Fallback to "SALES_INVOICE" if invoiceType is undefined
        { skip: !invoiceId || !invoiceType }
    );

    const { data: acctTransEntryDataPaymentApplication } = useFetchInvoiceAcctTransEntriesQuery(
        { invoiceId, acctgTransTypeId: "PAYMENT_APPL" },
        { skip: !invoiceId }
    );

    const { data: diagramData, refetch } = useGetGlAccountDiagramQuery(
        selectedAcctgTransId,
        { skip: !selectedAcctgTransId }
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

    // ───────────────────── Helpers ─────────────────────────────
    const handleShowDiagram = (acctgTransId: string) => {
        setSelectedAcctgTransId(acctgTransId);
        setShowDiagramModal(true);
        refetch();
    };

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
            Total Debit: {totalDebit.toFixed(2)} | Total Credit: {totalCredit.toFixed(2)}
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
            Total Debit: {totalDebitPayment.toFixed(2)} | Total Credit: {totalCreditPayment.toFixed(2)}
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
                        <StyledTab label="Sales Invoice" value="1" />
                        <StyledTab label="Payment Application" value="2" />
                    </StyledTabs>
                    <TabPanel value="1">
                        <Grid container>
                            <Grid item xs={12}>
                                <KendoGrid
                                    style={{ width: 850 }}
                                    data={orderBy(acctTransEntries, sort)}
                                    sortable
                                    sort={sort}
                                    onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                                    pageable={false}
                                    resizable
                                    rowRender={rowRender}
                                >
                                    <GridToolbar>
                                        <Typography sx={{ p: 2 }} variant="h6">
                                            Invoice Transactions
                                        </Typography>
                                    </GridToolbar>
                                    {/* ───── Columns ───── */}
                                    <Column
                                        field="acctgTransId"
                                        title="Acctg Trans"
                                        width={100}
                                        footerCell={TotalsFooterCell}
                                    />
                                    <Column field="origAmount" title="Orig Amount" width={100} />
                                    <Column field="debitCreditFlag" title="Debit/Credit" width={90} />
                                    <Column field="glAccountId" title="GL Account" width={100} />
                                    <Column field="glAccountTypeDescription" title="Account Name" width={220} />
                                    <Column field="productName" title="Product" width={200} />
                                    <Column field="isPosted" title="Posted?" width={80} />
                                    <Column field="glFiscalTypeId" title="Fiscal Type" width={100} />
                                    <Column field="acctgTransTypeDescription" title="Trans Type" width={130} />
                                    <Column
                                        field="transactionDate"
                                        title="Trans Date"
                                        width={130}
                                        format="{0:dd/MM/yyyy}"
                                    />
                                    <Column
                                        field="postedDate"
                                        title="Posted Date"
                                        width={130}
                                        format="{0:dd/MM/yyyy}"
                                    />
                                    <Column field="glAccountClassDescription" title="Account Class" width={140} />
                                    <Column field="origCurrencyUomId" title="Currency" width={90} />
                                    <Column
                                        title="Diagram"
                                        width={100}
                                        cell={(props) => (
                                            <td>
                                                <Button
                                                    variant="contained"
                                                    size="small"
                                                    onClick={() =>
                                                        handleShowDiagram(props.dataItem.acctgTransId)
                                                    }
                                                >
                                                    View
                                                </Button>
                                            </td>
                                        )}
                                    />
                                </KendoGrid>
                            </Grid>
                        </Grid>
                    </TabPanel>
                    <TabPanel value="2">
                        <Grid container>
                            <Grid item xs={12}>
                                <KendoGrid
                                    style={{ width: 850 }}
                                    data={orderBy(paymentTransEntries, sort)}
                                    sortable
                                    sort={sort}
                                    onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                                    pageable={false}
                                    resizable
                                    rowRender={rowRender}
                                >
                                    <GridToolbar>
                                        <Typography sx={{ p: 2 }} variant="h6">
                                            Payment Application Transactions
                                        </Typography>
                                    </GridToolbar>
                                    {/* ───── Columns ───── */}
                                    <Column
                                        field="acctgTransId"
                                        title="Acctg Trans"
                                        width={100}
                                        footerCell={PaymentTotalsFooterCell}
                                    />
                                    <Column field="origAmount" title="Orig Amount" width={100} />
                                    <Column field="debitCreditFlag" title="Debit/Credit" width={90} />
                                    <Column field="glAccountId" title="GL Account" width={100} />
                                    <Column field="glAccountTypeDescription" title="Account Name" width={220} />
                                    <Column field="productName" title="Product" width={200} />
                                    <Column field="isPosted" title="Posted?" width={80} />
                                    <Column field="glFiscalTypeId" title="Fiscal Type" width={100} />
                                    <Column field="acctgTransTypeDescription" title="Trans Type" width={130} />
                                    <Column
                                        field="transactionDate"
                                        title="Trans Date"
                                        width={130}
                                        format="{0:dd/MM/yyyy}"
                                    />
                                    <Column
                                        field="postedDate"
                                        title="Posted Date"
                                        width={130}
                                        format="{0:dd/MM/yyyy}"
                                    />
                                    <Column field="glAccountClassDescription" title="Account Class" width={140} />
                                    <Column field="origCurrencyUomId" title="Currency" width={90} />
                                    <Column
                                        title="Diagram"
                                        width={100}
                                        cell={(props) => (
                                            <td>
                                                <Button
                                                    variant="contained"
                                                    size="small"
                                                    onClick={() =>
                                                        handleShowDiagram(props.dataItem.acctgTransId)
                                                    }
                                                >
                                                    View
                                                </Button>
                                            </td>
                                        )}
                                    />
                                </KendoGrid>
                            </Grid>
                        </Grid>
                    </TabPanel>
                </TabContext>
            </Box>

            {/* ───────── Diagram Modal ───────── */}
            {showDiagramModal && (
                <ModalContainer
                    show={showDiagramModal}
                    onClose={() => setShowDiagramModal(false)}
                    width={600}
                >
                    <Typography variant="h6" sx={{ mb: 2 }}>
                        Accounting Transaction Flow
                    </Typography>
                    {diagramData ? (
                        <MermaidChart chart={diagramData.diagram} />
                    ) : (
                        <Typography>Loading…</Typography>
                    )}
                </ModalContainer>
            )}
        </Fragment>
    );
}
