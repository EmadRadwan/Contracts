import React, { useEffect, useState } from "react";
import {
  useAppDispatch,
  useAppSelector,
  useFetchInvoiceStatusItemsQuery,
} from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Button, Grid, Paper, Typography } from "@mui/material";
import BillingAccountsMenu from "../menu/BillingAccountsMenu";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { useFetchBillingAccountsInvoicesQuery } from "../../../../app/store/apis";
import { router } from "../../../../app/router/Routes";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GRID_COL_INDEX_ATTRIBUTE,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { handleDatesArray } from "../../../../app/util/utils";
import {
  setSelectedBillingAccount,
  setSelectedInvoice,
} from "../../slice/accountingSharedUiSlice";
import LoadingComponent from "../../../../app/layout/LoadingComponent";

const BillingAccountInvoicesList = () => {
  const dispatch = useAppDispatch();
  const { data: invoiceStatusItems } =
    useFetchInvoiceStatusItemsQuery(undefined);
  const { selectedBillingAccount } = useAppSelector(
    (state) => state.accountingSharedUi
  );
  const [billingAccountInvoices, setBillingAccountInvoices] = useState([]);
  const [statusId, setStatusId] = useState<string>("");
  const {
    data: billingAccountInvoicesData,
    isSuccess,
    refetch,
    isFetching,
    isLoading,
  } = useFetchBillingAccountsInvoicesQuery({
    billingAccountId: selectedBillingAccount?.billingAccountId,
    statusId,
  });

  if (!selectedBillingAccount) {
    router.navigate("/billingAccounts");
  }

  useEffect(() => {
    if (billingAccountInvoicesData) {
      setBillingAccountInvoices(
        handleDatesArray(billingAccountInvoicesData.billingAccountInvoices)
      );
    }
  }, [billingAccountInvoicesData]);

  const handleSelectStatus = (values: any) => {
    setStatusId(values.statusId);
    refetch();
  };

  const InvoicePaidCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
      <td
        className={props.className}
        style={{
          ...props.style,
          color: props.dataItem.paidInvoice ? "green" : "red",
        }}
        colSpan={props.colSpan}
        role={"gridcell"}
        aria-colindex={props.ariaColumnIndex}
        aria-selected={props.isSelected}
        {...{
          [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
        }}
        {...navigationAttributes}
      >
        {props.dataItem.paidInvoice ? "Y" : "N"}
      </td>
    );
  };

  const InvoiceCell = (props: any) => {
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
            dispatch(setSelectedInvoice(props.dataItem));
            dispatch(setSelectedBillingAccount(undefined));
            router.navigate("/invoices", {
              state: { invoice: props.dataItem },
            });
          }}
        >
          {props.dataItem.invoiceId}
        </Button>
      </td>
    );
  };
  return (
    <>
      <AccountingMenu selectedMenuItem="billingAcc" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <BillingAccountsMenu selectedMenuItem="/billingAccounts/invoices" />
        <Grid item xs={12} sx={{ ml: 2, mt: 3 }}>
          <Typography variant="h5">
            Invoices for Billing Account:{" "}
            {selectedBillingAccount?.billingAccountId}
          </Typography>
        </Grid>
        <Grid item xs={12} sx={{ ml: 2 }}>
          <Form
            initialValues={{ statusId: "" }}
            onSubmit={(values) => handleSelectStatus(values)}
            render={(formRenderProps) => (
              <FormElement>
                <fieldset className={"k-form-fieldset"}>
                  <Grid container spacing={2}>
                    <Grid item xs={3}>
                      <Field
                        name="statusId"
                        id="statusId"
                        label="Status"
                        component={MemoizedFormDropDownList2}
                        data={invoiceStatusItems ?? []}
                        dataItemKey="statusItemId"
                        textField="description"
                      />
                    </Grid>
                  </Grid>
                </fieldset>
                <div className="k-form-buttons">
                  <Button variant="contained" type={"submit"} color="success">
                    Find
                  </Button>
                </div>
              </FormElement>
            )}
          />
        </Grid>
        <Grid item xs={12}>
          <div className="div-container">
            <KendoGrid style={{ height: "35vh" }} data={billingAccountInvoices}>
              <Column field="invoiceId" title="Invoice" cell={InvoiceCell} />
              <Column field="invoiceTypeDescription" title="Invoice Type" />
              <Column field="statusDescription" title="Status" />
              <Column
                field="invoiceDate"
                title="Invoice Date"
                format="{0: dd/MM/yyyy}"
              />
              <Column
                field="paidInvoice"
                title="Is Paid?"
                cell={InvoicePaidCell}
              />
              <Column field="amountToApply" title="Amount to apply" />
              <Column field="total" title="Total" />
            </KendoGrid>
          </div>
        </Grid>
        {(isLoading || isFetching) && (
          <LoadingComponent message="Loading Biling Account Invoices..." />
        )}
      </Paper>
    </>
  );
};

export default BillingAccountInvoicesList;
