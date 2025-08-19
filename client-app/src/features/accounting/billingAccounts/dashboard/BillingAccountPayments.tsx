import { useEffect, useState } from "react";
import {
  useAppDispatch,
  useAppSelector
} from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Button, Grid, Paper, Typography } from "@mui/material";
import BillingAccountsMenu from "../menu/BillingAccountsMenu";
import { useFetchBillingAccountsPaymentsQuery } from "../../../../app/store/apis";
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
  setSelectedPayment,
} from "../../slice/accountingSharedUiSlice";
import LoadingComponent from "../../../../app/layout/LoadingComponent"
import BillingAccountPaymentForm from "../form/BillingAccountPaymentForm";

const BillingAccountPayments = () => {
  const dispatch = useAppDispatch();
  const [showPaymentForm, setShowPaymentForm] = useState<boolean>(false)
  const { selectedBillingAccount } = useAppSelector(
    (state) => state.accountingSharedUi
  );
  const [billingAccountPayments, setBillingAccountPayments] = useState([]);
  const {
    data: billingAccountPaymentsData,
    isFetching,
    isLoading,
  } = useFetchBillingAccountsPaymentsQuery({
    billingAccountId: selectedBillingAccount?.billingAccountId!,
  });

  if (!selectedBillingAccount) {
    router.navigate("/billingAccounts");
  }

  useEffect(() => {
    if (billingAccountPaymentsData) {
      setBillingAccountPayments(handleDatesArray(billingAccountPaymentsData));
    }
  }, [billingAccountPaymentsData]);

  const PaymentCell = (props: any) => {
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
            dispatch(setSelectedPayment(props.dataItem));
            dispatch(setSelectedBillingAccount(undefined));
            router.navigate("/payments", {
              state: { payment: props.dataItem },
            });
          }}
        >
          {props.dataItem.paymentId}
        </Button>
      </td>
    );
  };
  if (showPaymentForm) {
    return <BillingAccountPaymentForm onClose={() => setShowPaymentForm(false)} />
  }
  return (
    <>
      <AccountingMenu selectedMenuItem="billingAcc" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <BillingAccountsMenu selectedMenuItem="/billingAccounts/payments" />
        <Grid item xs={12} sx={{ ml: 2, mt: 3 }}>
          <Typography variant="h5">
            Payments for Billing Account:{" "}
            {selectedBillingAccount?.billingAccountId}
          </Typography>
        </Grid>
        <Grid item xs={12}>
          <div className="div-container">
            <KendoGrid style={{ height: "35vh" }} data={billingAccountPayments}>
              <GridToolbar>
                <Grid container>
                  <Grid item xs={3}>
                    <Button variant="outlined" color="secondary" onClick={() => setShowPaymentForm(true)}>
                      Create Payment
                    </Button>
                  </Grid>
                </Grid>
              </GridToolbar>
              <Column field="paymentId" title="Invoice" cell={PaymentCell} />
              <Column field="invoiceId" title="Invoice" />
              <Column
                field="paymentMethodTypeDescription"
                title="Payment Method Type"
              />
              <Column
                field="effectiveDate"
                title="Effective Date"
                format="{0: dd/MM/yyyy}"
              />
              <Column
                field="amountApplied"
                title="Amount to apply"
                format="{0: c2}"
              />
              <Column field="amount" title="Total" />
            </KendoGrid>
          </div>
        </Grid>
        {(isLoading || isFetching) && (
          <LoadingComponent message="Loading Biling Account Payments..." />
        )}
      </Paper>
    </>
  );
};

export default BillingAccountPayments;
