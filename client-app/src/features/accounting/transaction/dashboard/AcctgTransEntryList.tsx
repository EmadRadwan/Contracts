import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { Fragment, useCallback, useEffect, useState } from "react";
import {
  Grid as KendoGrid,
  GridCellProps,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
  GridCustomFooterCellProps,
  GridToolbar, GridRowProps,
} from "@progress/kendo-react-grid";
import { handleDatesArray } from "../../../../app/util/utils";
import { AcctgTransEntry } from "../../../../app/models/accounting/acctgTransEntry";
import { AcctgTrans } from "../../../../app/models/accounting/acctgTrans";
import Button from "@mui/material/Button";
import { Grid, Skeleton } from "@mui/material";
import { nonDeletedAcctgTransEntriesSelector } from "../../slice/accountingSelectors";
import {useAppSelector, useFetchGeneralAcctTransEntriesQuery} from "../../../../app/store/configureStore";
import { toast } from "react-toastify";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import AcctgTransEntryForm from "../form/AcctgTransEntryForm";
import {useDeleteAcctgTransEntryMutation} from "../../../../app/store/apis";

interface Props {
  acctgTrans?: AcctgTrans;
}

export default function AcctgTransEntryList({ acctgTrans }: Props) {
  const initialSort: Array<SortDescriptor> = [{ field: "acctgTransEntrySeqId", dir: "asc" }];
  const { acctgTransId } = acctgTrans || {};
  const [sort, setSort] = useState(initialSort);
  // REFACTORED: Enabled pagination to match SalesOrderItemsList's pageable grid
  const initialDataState: State = { skip: 0, take: 10 };
  const [page, setPage] = useState<State>(initialDataState);
  // REFACTORED: Added states for modal form display, edit mode, and selected entry, inspired by SalesOrderItemsList's show, editMode, and orderItem states
  const [editMode, setEditMode] = useState(0);
  const [showForm, setShowForm] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<AcctgTransEntry | undefined>(undefined);
  const [selectedMenuItem, setSelectedMenuItem] = useState("Create Entry");
  const [acctTransEntries, setAcctTransEntries] = useState<AcctgTransEntry[]>([]);
  

  // REFACTORED: Used useAppSelector for consistency with SalesOrderItemsList's useAppSelector
  const uiAcctgTransEntries: AcctgTransEntry[] = useAppSelector(nonDeletedAcctgTransEntriesSelector);
  const [deleteAcctgTransEntry, { isLoading: isDeleting }] = useDeleteAcctgTransEntryMutation();
  // REFACTORED: Added localization helper to match SalesOrderItemsList's useTranslationHelper
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.trans.entries.list";

  const { data: acctTransEntryData } = useFetchGeneralAcctTransEntriesQuery(
      acctgTransId,
      { skip: acctgTransId === undefined }
  );
  
  useEffect(() => {
    if (uiAcctgTransEntries) {
      const adjustedData = handleDatesArray(uiAcctgTransEntries);
      setAcctTransEntries(adjustedData);
    }
  }, [uiAcctgTransEntries]);

  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };

  // REFACTORED: Added handleSelectEntry to select an entry for editing, similar to SalesOrderItemsList's handleSelectOrderItem
  const handleSelectEntry = (entryId: string) => {
    if (acctgTrans?.isPosted === "Y") return;
    const selectedEntry = uiAcctgTransEntries.find(
      (entry) => entry.acctgTransId + entry.acctgTransEntrySeqId === entryId
    );
    setSelectedEntry(selectedEntry);
    setSelectedMenuItem("Update Entry");
    setEditMode(2);
    setShowForm(true);
  };

  // REFACTORED: Added handleAddEntry to trigger create mode, inspired by SalesOrderItemsList's Add Product button logic
  const handleAddEntry = () => {
    setSelectedEntry(undefined);
    setSelectedMenuItem("Create Entry");
    setEditMode(1);
    setShowForm(true);
  };

  // REFACTORED: Added memoizedOnClose to handle modal closure, matching SalesOrderItemsList's memoizedOnClose
  const memoizedOnClose = useCallback(() => {
    setShowForm(false);
    setEditMode(0);
    setSelectedEntry(undefined);
    setSelectedMenuItem("Create Entry");
  }, []);

  // REFACTORED: Updated remove to use API mutation with translated toasts, but kept API-based deletion (note: SalesOrderItemsList uses Redux-based deletion)
  const remove = async (dataItem: AcctgTransEntry) => {
    try {
      await deleteAcctgTransEntry({
        acctgTransId: dataItem.acctgTransId,
        acctgTransEntrySeqId: dataItem.acctgTransEntrySeqId,
      }).unwrap();
      toast.success(getTranslatedLabel("general.removed", "Transaction Entry Removed Successfully"));
    } catch (error: any) {
      const message =
        error?.message ||
        error?.data?.errorMessage ||
        error?.data?.title ||
        getTranslatedLabel("general.error", "Failed to remove transaction entry.");
      toast.error(message);
    }
  };

  const rowRender = (
    trElement: React.ReactElement<HTMLTableRowElement>,
    props: GridRowProps
  ) => {
    const actionable = props.dataItem.debitCreditFlag !== "C";
    const green = { backgroundColor: "rgba(55, 180, 0, 0.32)" };
    const white = { backgroundColor: "#ffffff" };
    const trProps: any = { style: actionable ? green : white };
    return React.cloneElement(trElement, { ...trProps }, trElement.props.children);
  };

  const TransEntryCell = (props: GridCellProps) => {
    return (
      <td>
        <Button
          onClick={() =>
            handleSelectEntry(props.dataItem.acctgTransId + props.dataItem.acctgTransEntrySeqId)
          }
          disabled={acctgTrans?.isPosted === "Y"}
        >
          {props.dataItem.acctgTransId}
        </Button>
      </td>
    );
  };

  // REFACTORED: Updated DeleteTransEntryCell to use translated label, matching SalesOrderItemsList's DeleteOrderItemCell
  const DeleteTransEntryCell = (props: GridCellProps) => {
    const { dataItem } = props;
    return (
      <td className="k-command-cell">
        <Button
          className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
          onClick={() => remove(dataItem)}
          color="error"
          disabled={isDeleting || acctgTrans?.isPosted === "Y"}
        >
          {getTranslatedLabel("general.remove", "Remove")}
        </Button>
      </td>
    );
  };

  const totalCredit = acctTransEntries
    ?.filter((t: AcctgTransEntry) => t.debitCreditFlag === "C")
    .reduce((a: number, b: AcctgTransEntry) => a + (b.amount || 0), 0);

  const totalDebit = acctTransEntries
    ?.filter((t: AcctgTransEntry) => t.debitCreditFlag !== "C")
    .reduce((a: number, b: AcctgTransEntry) => a + (b.amount || 0), 0);

  // REFACTORED: Added translated labels to TotalsFooterCell, inspired by SalesOrderItemsList's SubtotalDisplayCell
  const TotalsFooterCell = (props: GridCustomFooterCellProps) => {
    return (
      <td
        colSpan={14}
        style={{
          textAlign: "left",
          fontWeight: "bold",
          paddingRight: "20px",
          color: "#1565C0",
        }}
      >
        {getTranslatedLabel(`${localizationKey}.totals`, "Total Credit")}: {totalCredit} |{" "}
        {getTranslatedLabel(`${localizationKey}.totals`, "Total Debit")}: {totalDebit}
      </td>
    );
  };

  return (
    <Fragment>
      {/* REFACTORED: Added ModalContainer for AcctgTransEntry form, matching SalesOrderItemsList's modal pattern */}
      {showForm && (
        <ModalContainer show={showForm} onClose={memoizedOnClose} width={700}>
          <AcctgTransEntryForm
            selectedMenuItem={selectedMenuItem}
            editMode={editMode}
            selectedEntry={selectedEntry}
            acctgTransId={acctgTransId || ""}
          />
        </ModalContainer>
      )}
      <Grid container columnSpacing={1} direction="column" alignItems={"center"} sx={{ mt: 1 }}>
        {/* REFACTORED: Updated grid to use pagination and styling from SalesOrderItemsList */}
        <KendoGrid
            className="main-grid"
            style={{ height: "40vh", width: "60vw" }}
            data={orderBy(acctTransEntries || [], sort).slice(page.skip, page.take + page.skip)}
            sortable={true}
            sort={sort}
            onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
            skip={page.skip}
            take={page.take}
            total={acctTransEntries?.length || 0}
            pageable={true}
            onPageChange={pageChange}
            rowRender={rowRender}
        >
          {/* REFACTORED: Updated toolbar to use translated button, matching SalesOrderItemsList's GridToolbar */}
          <GridToolbar>
            <Grid container direction="row" justifyContent="space-between">
              <Grid item>
                <Button
                    color="secondary"
                    onClick={handleAddEntry}
                    variant="outlined"
                    disabled={acctgTrans?.isPosted === "Y"}
                >
                  {getTranslatedLabel(`${localizationKey}.add`, "Add Transaction Entry")}
                </Button>
              </Grid>
            </Grid>
          </GridToolbar>
          <Column
              field="acctgTransId"
              title={getTranslatedLabel(`${localizationKey}.transId`, "Acctg Trans")}
              width={100}
              cell={TransEntryCell}
              footerCell={TotalsFooterCell}
          />
          <Column
              field="acctgTransEntrySeqId"
              title={getTranslatedLabel(`${localizationKey}.seqId`, "Seq Id")}
              width={100}
              footerCell={() => null}
          />
          <Column
              field="amount"
              title={getTranslatedLabel(`${localizationKey}.amount`, "Amount")}
              width={100}
              footerCell={() => null}
          />
          <Column
              field="origAmount"
              title={getTranslatedLabel(`${localizationKey}.origAmount`, "Orig Amount")}
              width={100}
              footerCell={() => null}
          />
          <Column
              field="debitCreditFlag"
              title={getTranslatedLabel(`${localizationKey}.debitCredit`, "Debit/Credit")}
              width={70}
              footerCell={() => null}
          />
          <Column
              field="glAccountId"
              title={getTranslatedLabel(`${localizationKey}.glAccount`, "GL Account")}
              width={100}
              footerCell={() => null}
          />
          <Column
              field="glAccountTypeDescription"
              title={getTranslatedLabel(`${localizationKey}.accountName`, "Account Name")}
              width={220}
              footerCell={() => null}
          />
          <Column
              field="productName"
              title={getTranslatedLabel(`${localizationKey}.product`, "Product")}
              width={200}
              footerCell={() => null}
          />
          <Column
              field="isPosted"
              title={getTranslatedLabel(`${localizationKey}.isPosted`, "Is Posted")}
              width={100}
              footerCell={() => null}
          />
          <Column
              field="glFiscalTypeId"
              title={getTranslatedLabel(`${localizationKey}.fiscalType`, "GL Fiscal Type")}
              width={100}
              footerCell={() => null}
          />
          <Column
              field="acctgTransTypeDescription"
              title={getTranslatedLabel(`${localizationKey}.transType`, "Acctg Trans Type")}
              width={130}
              footerCell={() => null}
          />
          <Column
              field="transactionDate"
              title={getTranslatedLabel(`${localizationKey}.transDate`, "Transaction Date")}
              width={150}
              format="{0: dd/MM/yyyy}"
              footerCell={() => null}
          />
          <Column
              field="postedDate"
              title={getTranslatedLabel(`${localizationKey}.postedDate`, "Posted Date")}
              width={150}
              format="{0: dd/MM/yyyy}"
              footerCell={() => null}
          />
          <Column
              field="glAccountClassDescription"
              title={getTranslatedLabel(`${localizationKey}.accountClass`, "Account Class")}
              width={140}
              footerCell={() => null}
          />
          <Column
              field="origCurrencyUomId"
              title={getTranslatedLabel(`${localizationKey}.currency`, "Currency")}
              width={110}
              footerCell={() => null}
          />
          <Column
              cell={DeleteTransEntryCell}
              width="100px"
              footerCell={() => null}
          />
        </KendoGrid>
      </Grid>
    </Fragment>
  );
}