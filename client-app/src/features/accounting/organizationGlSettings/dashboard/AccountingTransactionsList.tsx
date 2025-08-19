import React, { useEffect, useState } from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { DataResult, State } from "@progress/kendo-data-query";
import Button from "@mui/material/Button";
import { Grid, Paper } from "@mui/material";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { MenuSelectEvent } from "@progress/kendo-react-layout";
import { handleDatesArray } from "../../../../app/util/utils";

import {
  RootState,
  useAppDispatch,
  useAppSelector,
  useFetchAcctTransQuery,
} from "../../../../app/store/configureStore";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useLocation } from "react-router-dom";
import { AcctgTrans } from "../../../../app/models/accounting/acctgTrans";
import {
  setSelectedInvoice,
  setWhatWasClicked,
} from "../../slice/accountingSharedUiSlice";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import SetupAccountingMenu from "../menu/SetupAccountingMenu";
import AccountingSummaryMenu from "../menu/AccountingSummaryMenu";
import { useSelector } from "react-redux";
import { router } from "../../../../app/router/Routes";
import { useNavigate } from "react-router";

export default function AccountingTransactionsList() {
  const navigate = useNavigate();
  const [accountingTrans, setAccountingTrans] = React.useState<DataResult>({
    data: [],
    total: 0,
  });
  const [dataState, setDataState] = React.useState<State>({ take: 6, skip: 0 });

  const { whatWasClicked } = useAppSelector(
    (state) => state.accountingSharedUi
  );

  const dataStateChange = (e: GridDataStateChangeEvent) => {
    setDataState(e.dataState);
  };

  const dispatch = useAppDispatch();
  const companyName = useSelector(
    (state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName
  );

  // if company name is not set, then redirect to the orgGl

  useEffect(() => {
    if (!companyName) {
      router.navigate("/orgGl");
    }
  }, [companyName]);

  const { getTranslatedLabel } = useTranslationHelper();
  const location = useLocation();
  const [shouldShowTransactionForm, setShouldShowTransactionForm] =
    useState(false);

  const [editMode, setEditMode] = useState(0);
  const [acctTrans, setAcctTrans] = useState<AcctgTrans | undefined>(undefined);

  const [show, setShow] = useState(false);

  const { data, error, isFetching } = useFetchAcctTransQuery({ ...dataState });

  useEffect(() => {
    if (data) {
      const adjustedData = handleDatesArray(data.data);
      setAccountingTrans({ data: adjustedData, total: data.total });
    }
  }, [data]);

  console.log("accountingTrans", accountingTrans);
  // When component mounts or location.state.acctTrans changes, decide whether to show form
  useEffect(() => {
    if (location.state?.acctTrans) {
      setShouldShowTransactionForm(true);
    }
  }, [location.state?.acctTrans]);

  useEffect(() => {
    if (location.state?.isPosted && location.state?.isPosted === "N") {
      setDataState({
        ...dataState,
        filter: {
          logic: "and",
          filters: [
            {
              field: "isPosted",
              operator: "equals",
              value: "N",
            },
          ],
        },
      });
    }
  }, [location.state?.isPosted]);

  function handleSelectAcctTrans(acctTransId: string) {
    dispatch(setWhatWasClicked("acctTransId"));
    console.log("acctTransId param", acctTransId);
    // select the acctTrans from data array based on acctTransId
    const selectedAcctTrans: AcctgTrans | undefined = data?.data.find(
      (acctTrans: any) => acctTrans.acctgTransId === acctTransId
    );

    console.log("selectedAcctTrans", selectedAcctTrans);
    // set component selected acctTrans
    setAcctTrans(selectedAcctTrans);
  }

  const AcctTransDescriptionCell = (props: any) => {
    const field = props.field || "";
    const value = props.dataItem[field];
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
      <td
        className={props.className}
        style={{ ...props.style, color: "blue" }}
        colSpan={props.colSpan}
        role={"gridcell"}
        aria-colindex={props.ariaColumnIndex}
        aria-selected={props.isSelected}
        {...{
          [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
        }}
        {...navigationAttributes}
      >
        <Button
          onClick={() => {
            handleSelectAcctTrans(props.dataItem.acctgTransId);
          }}
        >
          {props.dataItem.acctgTransId}
        </Button>
      </td>
    );
  };

  // convert cancelEdit function to memoized function
  const cancelEdit = React.useCallback(() => {
    setEditMode(0);
    setAcctTrans(undefined);
    setShouldShowTransactionForm(false);
  }, [setEditMode, setAcctTrans, setShouldShowTransactionForm]);



  function handleMenuSelect(e: MenuSelectEvent) {
    if (e.item.text === "New AcctTrans") {
      return;
    }
    // clear already populated acctTrans data if any
    setEditMode(1);
  }

  const AccountingTransInvoiceCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    const invoiceId = props.dataItem.invoiceId;
    return (
      <td
        className={props.className}
        style={{ ...props.style, color: "blue" }}
        colSpan={props.colSpan}
        role={"gridcell"}
        aria-colindex={props.ariaColumnIndex}
        aria-selected={props.isSelected}
        {...{
          [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
        }}
        {...navigationAttributes}
      >
        {invoiceId ? (
          <Button
            onClick={() => {
              dispatch(setSelectedInvoice(props.dataItem));
              router.navigate("/invoices", {
                state: { invoice: props.dataItem },
              });
            }}
          >
            {props.dataItem.invoiceId}
          </Button>
        ) : (
          <span>{invoiceId}</span>
        )}
      </td>
    );
  };
  
  console.log("acctTrans", acctTrans);
  if (acctTrans) {
    navigate(`/editAcctgTrans/${acctTrans.acctgTransId}`, {
      state: { selectedAcctgTrans: acctTrans },
    });
  }
  return (
    <>
      <AccountingMenu selectedMenuItem={"acctTrans"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          <SetupAccountingMenu />
          <AccountingSummaryMenu />

          <Grid item xs={12}>
            <div className="div-container">
              <KendoGrid
                resizable={true}
                filterable={true}
                sortable={true}
                pageable={true}
                {...dataState}
                data={
                  accountingTrans ? accountingTrans : { data: [], total: 77 }
                }
                onDataStateChange={dataStateChange}
              >
                <GridToolbar>
                  <Grid container>
                    <Grid item xs={4}>
                      <Button
                        color={"secondary"}
                        onClick={() => setEditMode(1)}
                        variant="outlined"
                      >
                        Create Accounting Transaction
                      </Button>
                    </Grid>
                  </Grid>
                </GridToolbar>

                <Column
                  field="acctgTransId"
                  cell={AcctTransDescriptionCell}
                  width={110}
                  locked={!show}
                  title={"Acctg Trans Id"}
                />
                <Column
                  field="transactionDate"
                  title="Transaction Date"
                  width={130}
                  format="{0: dd/MM/yyyy}"
                />
                <Column
                  field="acctgTransTypeDescription"
                  title="Acctg Trans Type"
                  width={150}
                />
                <Column field="description" title="Description" width={130} />

                <Column
                  field="glFiscalTypeId"
                  title="Fiscal Gl Type"
                  width={100}
                />
                <Column
                  field="invoiceId"
                  title="Invoice Id"
                  width={100}
                  cell={AccountingTransInvoiceCell}
                />
                <Column field="paymentId" title="Payment Id" width={100} />
                <Column
                  field="workEffortId"
                  title="WorkEffort Id"
                  width={100}
                />
                <Column field="shipmentId" title="Shipment Id" width={100} />
                <Column field="isPosted" title="Is Posted" width={100} />
                <Column
                  field="postedDate"
                  title="Posted Date"
                  width={150}
                  format="{0: dd/MM/yyyy}"
                />
              </KendoGrid>
              {isFetching && (
                <LoadingComponent message="Loading Accounting Transactions..." />
              )}
            </div>
          </Grid>
        </Grid>
      </Paper>
    </>
  );
}
