import { useEffect, useState } from "react";
import {
  useAppDispatch,
  useAppSelector
} from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Button, Grid, Paper, Typography } from "@mui/material";
import BillingAccountsMenu from "../menu/BillingAccountsMenu";
import {
  useFetchBillingAccountsOrdersQuery
} from "../../../../app/store/apis";
import { router } from "../../../../app/router/Routes";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { handleDatesArray } from "../../../../app/util/utils";
import {
  setSelectedBillingAccount,
  setSelectedOrder
} from "../../slice/accountingSharedUiSlice";
import LoadingComponent from "../../../../app/layout/LoadingComponent";

const BillingAccountOrders = () => {
  const dispatch = useAppDispatch();
  const { selectedBillingAccount } = useAppSelector(
    (state) => state.accountingSharedUi
  );
  const [billingAccountOrders, setBillingAccountOrders] = useState([]);
  const {
    data: billingAccountOrdersData,
    isFetching,
    isLoading,
  } = useFetchBillingAccountsOrdersQuery({
    billingAccountId: selectedBillingAccount?.billingAccountId!,
  });

  if (!selectedBillingAccount) {
    router.navigate("/billingAccounts");
  }

  useEffect(() => {
    if (billingAccountOrdersData) {
      setBillingAccountOrders(handleDatesArray(billingAccountOrdersData));
    }
  }, [billingAccountOrdersData]);

  const OrderCell = (props: any) => {
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
            dispatch(setSelectedOrder(props.dataItem));
            dispatch(setSelectedBillingAccount(undefined));
            router.navigate("/orders", { state: { payment: props.dataItem } });
          }}
        >
          {props.dataItem.orderId}
        </Button>
      </td>
    );
  };
  return (
    <>
      <AccountingMenu selectedMenuItem="billingAcc" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <BillingAccountsMenu selectedMenuItem="/billingAccounts/orders" />
        <Grid item xs={12} sx={{ ml: 2, mt: 3 }}>
          <Typography variant="h5">
            Orders for Billing Account:{" "}
            {selectedBillingAccount?.billingAccountId}
          </Typography>
        </Grid>
        <Grid item xs={12}>
          <div className="div-container">
            <KendoGrid style={{ height: "35vh" }} data={billingAccountOrders}>
              <Column title="Order" cell={OrderCell} />
              <Column
                field="paymentMethodTypeDescription"
                title="Payment Method Type"
              />
              <Column
                field="orderDate"
                title="Effective Date"
                format="{0: dd/MM/yyyy}"
              />
              <Column field="paymentStatusDescription" title="Payment status" />
              <Column
                field="maxAmount"
                title="Maximum Amount"
                format="{0: c2}"
              />
            </KendoGrid>
          </div>
        </Grid>
        {(isLoading || isFetching) && (
          <LoadingComponent message="Loading Biling Account Order..." />
        )}
      </Paper>
    </>
  );
};

export default BillingAccountOrders;
