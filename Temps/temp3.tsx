import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { Fragment, useEffect } from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridRowProps,
    GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import { Grid, Typography } from "@mui/material";
import { useFetchPaymentAcctTransEntriesQuery } from "../../../../app/store/apis";
import { handleDatesArray } from "../../../../app/util/utils";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

interface Props {
    onClose: () => void;
    paymentId: string | undefined;
}

export default function PaymentTransactionsList({ onClose, paymentId }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.payments.transactions";

    const initialSort: Array<SortDescriptor> = [
        { field: "acctgTransEntrySeqId", dir: "asc" },
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = { skip: 0, take: 10 };
    const [page, setPage] = React.useState<any>(initialDataState);
    const [acctTransEntries, setAcctTransEntries] = React.useState<AcctgTransEntry[]>([]);

    const { data: acctTransEntryData } = useFetchPaymentAcctTransEntriesQuery(paymentId, {
        skip: paymentId === undefined,
    });

    useEffect(() => {
        if (acctTransEntryData) {
            const adjustedData = handleDatesArray(acctTransEntryData);
            setAcctTransEntries(adjustedData);
        }
    }, [acctTransEntryData]);

    const rowRender = (
        trElement: React.ReactElement<HTMLTableRowElement>,
        props: GridRowProps
    ) => {
        const actionable = !(props.dataItem.debitCreditFlag === "C");
        const green = { backgroundColor: "rgb(55, 180, 0, 0.32)" };
        const white = { backgroundColor: "#ffffff" };
        const trProps: any = { style: actionable ? green : white };
        return React.cloneElement(trElement, { ...trProps }, trElement.props.children);
    };

    // Group entries by transaction ID.
    const groupedTransactions = acctTransEntries.reduce((acc, entry) => {
        if (!acc[entry.acctgTransId]) {
            acc[entry.acctgTransId] = { debit: 0, credit: 0 };
        }
        if (entry.debitCreditFlag === "C") {
            acc[entry.acctgTransId].credit = entry.origAmount || 0;
        } else {
            acc[entry.acctgTransId].debit = entry.origAmount || 0;
        }
        return acc;
    }, {} as Record<string, { debit: number; credit: number }>);

    const transactionDebitAmounts = Object.values(groupedTransactions).map((g) => g.debit);
    const transactionCreditAmounts = Object.values(groupedTransactions).map((g) => g.credit);

    const uniqueDebitAmounts = Array.from(new Set(transactionDebitAmounts));
    const uniqueCreditAmounts = Array.from(new Set(transactionCreditAmounts));

    const totalDebit =
        uniqueDebitAmounts.length === 1
            ? uniqueDebitAmounts[0]
            : transactionDebitAmounts.reduce((sum, amt) => sum + amt, 0);
    const totalCredit =
        uniqueCreditAmounts.length === 1
            ? uniqueCreditAmounts[0]
            : transactionCreditAmounts.reduce((sum, amt) => sum + amt, 0);

    const TotalsFooterCell = () => (
        <td
            colSpan={15}
            style={{
                textAlign: "left",
                fontWeight: "bold",
                paddingRight: "20px",
                color: "#1565C0",
            }}
        >
            {getTranslatedLabel(`${localizationKey}.footer.totalDebit`, "Total Debit")}: {totalDebit} |{" "}
            {getTranslatedLabel(`${localizationKey}.footer.totalCredit`, "Total Credit")}: {totalCredit}
        </td>
    );

    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    return (
        <Fragment>
            <Grid container padding={2} columnSpacing={1}>
                <Grid item xs={4}>
                    <Typography sx={{ p: 2 }} variant="h6">
                        {getTranslatedLabel(`${localizationKey}.title`, "Payment Transactions")}
                    </Typography>
                </Grid>
                <Grid container>
                    <div className="div-container">
                        <KendoGrid
                            style={{ height: "300px", width: "850px" }}
                            data={orderBy(acctTransEntries || [], sort).slice(page.skip, page.take + page.skip)}
                            sortable={true}
                            sort={sort}
                            onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                            skip={page.skip}
                            take={page.take}
                            total={acctTransEntries ? acctTransEntries.length : 0}
                            pageable={true}
                            onPageChange={pageChange}
                            resizable={true}
                            rowRender={rowRender}
                        >
                            <Column
                                field="acctgTransId"
                                title={getTranslatedLabel(`${localizationKey}.columns.acctgTransId`, "Acctg Trans")}
                                width={100}
                                footerCell={TotalsFooterCell}
                            />
                            <Column
                                field="acctgTransEntrySeqId"
                                title={getTranslatedLabel(`${localizationKey}.columns.acctgTransEntrySeqId`, "Acctg Trans Seq Id")}
                                width={0}
                                footerCell={() => null}
                            />
                            <Column
                                field="amount"
                                title={getTranslatedLabel(`${localizationKey}.columns.amount`, "Amount")}
                                width={0}
                                footerCell={() => null}
                            />
                            <Column
                                field="origAmount"
                                title={getTranslatedLabel(`${localizationKey}.columns.origAmount`, "Orig Amount")}
                                width={100}
                                footerCell={() => null}
                            />
                            <Column
                                field="debitCreditFlag"
                                title={getTranslatedLabel(`${localizationKey}.columns.debitCreditFlag`, "Debit Credit")}
                                width={70}
                                footerCell={() => null}
                            />
                            <Column
                                field="glAccountId"
                                title={getTranslatedLabel(`${localizationKey}.columns.glAccountId`, "Gl Account")}
                                width={100}
                                footerCell={() => null}
                            />
                            <Column
                                field="glAccountTypeDescription"
                                title={getTranslatedLabel(`${localizationKey}.columns.glAccountTypeDescription`, "Account Name")}
                                width={220}
                                footerCell={() => null}
                            />
                            <Column
                                field="productName"
                                title={getTranslatedLabel(`${localizationKey}.columns.productName`, "Product")}
                                width={200}
                                footerCell={() => null}
                            />
                            <Column
                                field="isPosted"
                                title={getTranslatedLabel(`${localizationKey}.columns.isPosted`, "Is Posted")}
                                width={100}
                                footerCell={() => null}
                            />
                            <Column
                                field="glFiscalTypeId"
                                title={getTranslatedLabel(`${localizationKey}.columns.glFiscalTypeId`, "Gl FiscalType")}
                                width={100}
                                footerCell={() => null}
                            />
                            <Column
                                field="acctgTransTypeDescription"
                                title={getTranslatedLabel(`${localizationKey}.columns.acctgTransTypeDescription`, "Acctg Trans Type")}
                                width={130}
                                footerCell={() => null}
                            />
                            <Column
                                field="transactionDate"
                                title={getTranslatedLabel(`${localizationKey}.columns.transactionDate`, "Transaction Date")}
                                width={150}
                                format="{0: dd/MM/yyyy}"
                                footerCell={() => null}
                            />
                            <Column
                                field="postedDate"
                                title={getTranslatedLabel(`${localizationKey}.columns.postedDate`, "Posted Date")}
                                width={150}
                                format="{0: dd/MM/yyyy}"
                                footerCell={() => null}
                            />
                            <Column
                                field="glAccountClassDescription"
                                title={getTranslatedLabel(`${localizationKey}.columns.glAccountClassDescription`, "Account Class")}
                                width={140}
                                footerCell={() => null}
                            />
                            <Column
                                field="origCurrencyUomId"
                                title={getTranslatedLabel(`${localizationKey}.columns.origCurrencyUomId`, "Currency")}
                                width={110}
                                footerCell={() => null}
                            />
                        </KendoGrid>
                    </div>
                </Grid>
            </Grid>
            <Grid item xs={2}>
                <Button onClick={onClose} color="error" variant="contained">
                    {getTranslatedLabel(`${localizationKey}.close`, "Close")}
                </Button>
            </Grid>
        </Fragment>
    );
}