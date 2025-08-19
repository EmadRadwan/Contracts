import {
  Grid as KendoGrid,
  GridColumn as Column,
} from "@progress/kendo-react-grid";
import { Grid, Typography } from "@mui/material";
import { useFetchOrderPaymentPreferenceQuery } from "../../../../../app/store/apis";

interface OrderPaymentPrefernceListProps {
  orderId: string
}

const OrderPaymentPreferenceList = ({
  orderId,
}: OrderPaymentPrefernceListProps) => {
    const { data: orderPaymentPreference } = useFetchOrderPaymentPreferenceQuery(
        orderId,
        {
          skip: !orderId,
        }
      )
  return (
    <>
      <Grid container justifyContent={"center"} py={2}>
        <Typography variant="h5">
          {orderPaymentPreference ? `Payment Preferences for ${orderPaymentPreference![0]?.orderId}` : ""}
        </Typography>
      </Grid>
      <Grid container>
        <KendoGrid
          style={{ height: "30vh" }}
          resizable={true}
          pageable={true}
          data={orderPaymentPreference ?? []}
        >
          <Column
            field="paymentMethodTypeDescription"
            title="Payment Method Type"
          />
          <Column field="statusDescription" title="Payment Status" />
          <Column field="maxAmount" title="Maximum Amount" />
          <Column field="uomDescription" title="Currency" />
        </KendoGrid>
      </Grid>
    </>
  );
};

export default OrderPaymentPreferenceList;
