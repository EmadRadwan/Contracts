import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { Fragment, useEffect, useState } from "react";
import {
  Grid as KendoGrid,
  GridCellProps,
  GridColumn as Column,
  GridPageChangeEvent,
  GridRowProps,
  GridSortChangeEvent,
  GridCustomFooterCellProps,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { useFetchGeneralAcctTransEntriesQuery } from "../../../../app/store/apis";
import { handleDatesArray } from "../../../../app/util/utils";
import { AcctgTransEntry } from "../../../../app/models/accounting/acctgTransEntry";
import Button from "@mui/material/Button";
import { nonDeletedAcctgTransEntriesSelector } from "../../slice/accountingSelectors";
import { useSelector } from "react-redux";
import { AcctgTrans } from "../../../../app/models/accounting/acctgTrans";
import { Grid, Typography } from "@mui/material";

interface Props {
  acctgTrans?: AcctgTrans;
}

export default function TransactionsList({ acctgTrans }: Props) {
  const initialSort: Array<SortDescriptor> = [
    { field: "acctgTransEntrySeqId", dir: "asc" },
  ];
  const { acctgTransId } = acctgTrans || {};
  const [sort, setSort] = useState(initialSort);
  const initialDataState: State = { skip: 0, take: 10 };
  const [page, setPage] = useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };
  const [editMode, setEditMode] = useState(0);
  const [acctTransEntries, setAcctTransEntries] = useState<AcctgTransEntry[]>([]);

  const { data: acctTransEntryData } = useFetchGeneralAcctTransEntriesQuery(
      acctgTransId,
      { skip: acctgTransId === undefined }
  );

  const uiAcctgTransEntries: any = useSelector(nonDeletedAcctgTransEntriesSelector);

  useEffect(() => {
    if (uiAcctgTransEntries) {
      const adjustedData = handleDatesArray(uiAcctgTransEntries);
      setAcctTransEntries(adjustedData);
    }
  }, [uiAcctgTransEntries]);

  const rowRender = (
      trElement: React.ReactElement<HTMLTableRowElement>,
      props: GridRowProps
  ) => {
    // Color the row green if it is not a credit entry.
    const actionable = props.dataItem.debitCreditFlag !== "C";
    const green = { backgroundColor: "rgba(55, 180, 0, 0.32)" };
    const white = { backgroundColor: "#ffffff" };
    const trProps: any = { style: actionable ? green : white };
    return React.cloneElement(trElement, { ...trProps }, trElement.props.children);
  };

  const DeleteTransEntryCell = (props: any) => {
    const { dataItem } = props;
    return (
        <td className="k-command-cell">
          <Button
              className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
              onClick={() => props.remove(dataItem)}
              color="error"
          >
            Remove
          </Button>
        </td>
    );
  };

  const remove = (dataItem: AcctgTransEntry) => {
    // Implement your removal logic here.
  };

  // Compute totals for credits and debits using the 'amount' field.
  const totalCredit = acctTransEntries
      ?.filter((t: AcctgTransEntry) => t.debitCreditFlag === "C")
      .reduce((a: number, b: AcctgTransEntry) => a + (b.amount || 0), 0);

  const totalDebit = acctTransEntries
      ?.filter((t: AcctgTransEntry) => t.debitCreditFlag !== "C")
      .reduce((a: number, b: AcctgTransEntry) => a + (b.amount || 0), 0);

  // Custom footer cell that spans all columns (16 in this example).
  const TotalsFooterCell = (props: GridCustomFooterCellProps) => {
    return (
        <td
            colSpan={14}
            style={{
              textAlign: "left",
              fontWeight: "bold",
              paddingRight: "20px",
              color: "#1565C0", // blue color; adjust as needed
            }}
        >
          Total Credit: {totalCredit} | Total Debit: {totalDebit}
        </td>
    );
  };

  return (
      <Fragment>
        <KendoGrid
            style={{ height: "50vh", width: "60vw" }}
            data={orderBy(acctTransEntries || [], sort).slice(
                page.skip,
                page.take + page.skip
            )}
            sortable={true}
            sort={sort}
            onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
            pageable={false} // Disables the grid pager entirely
            resizable
            rowRender={rowRender}
        >
          <GridToolbar>
            <Grid container alignItems="center">
              <Grid item>
                <Button
                    color="secondary"
                    onClick={() => setEditMode(1)}
                    variant="outlined"
                    disabled={acctgTrans?.isPosted === "Y"}
                >
                  Add Transaction Entry
                </Button>
              </Grid>
            </Grid>
          </GridToolbar>

          {/* Assign TotalsFooterCell only to the first column; all others render an empty footer */}
          <Column
              field="acctgTransId"
              title="Acctg Trans"
              width={100}
              footerCell={TotalsFooterCell}
          />
          <Column field="acctgTransEntrySeqId" title="Acctg Trans Seq Id" width={0} footerCell={() => null} />
          <Column field="amount" title="Amount" width={100} footerCell={() => null} />
          <Column field="origAmount" title="Orig Amount" width={100} footerCell={() => null} />
          <Column field="debitCreditFlag" title="Debit/Credit" width={70} footerCell={() => null} />
          <Column field="glAccountId" title="GL Account" width={100} footerCell={() => null} />
          <Column field="glAccountTypeDescription" title="Account Name" width={220} footerCell={() => null} />
          <Column field="productName" title="Product" width={200} footerCell={() => null} />
          <Column field="isPosted" title="Is Posted" width={100} footerCell={() => null} />
          <Column field="glFiscalTypeId" title="GL Fiscal Type" width={100} footerCell={() => null} />
          <Column field="acctgTransTypeDescription" title="Acctg Trans Type" width={130} footerCell={() => null} />
          <Column field="transactionDate" title="Transaction Date" width={150} format="{0: dd/MM/yyyy}" footerCell={() => null} />
          <Column field="postedDate" title="Posted Date" width={150} format="{0: dd/MM/yyyy}" footerCell={() => null} />
          <Column field="glAccountClassDescription" title="Account Class" width={140} footerCell={() => null} />
          <Column field="origCurrencyUomId" title="Currency" width={110} footerCell={() => null} />
          <Column
              cell={(props: GridCellProps) => <DeleteTransEntryCell {...props} remove={remove} />}
              width="100px"
              footerCell={() => null}
          />
        </KendoGrid>
      </Fragment>
  );
}
