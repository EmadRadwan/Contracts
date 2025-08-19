import Grid from "@mui/material/Grid";
import { Typography } from "@mui/material";
import { useSelector } from "react-redux";
import { useAppSelector } from "../../../../app/store/configureStore";
import {
  orderAdjustmentsSelector,
  orderLevelAdjustmentsTotal,
  orderLevelTaxTotal,
  orderPaymentsTotal,
  orderSubTotal,
} from "../../slice/orderSelectors";
import { OrderAdjustment } from "../../../../app/models/order/orderAdjustment";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

export default function OrderTotals() {
  const localizationKey = "order.totals";
  const { getTranslatedLabel } = useTranslationHelper();
  const sTotal: any = useSelector(orderSubTotal);
  //console.log("sTotal from OrderTotals", sTotal);
  const aTotal: any = useSelector(orderLevelAdjustmentsTotal);
  //console.log("aTotal from OrderTotals", aTotal);

  const taxTotal: any = useAppSelector(orderLevelTaxTotal);
  //console.log("taxTotal from OrderTotals", taxTotal);

  const orderType: any = useAppSelector(
    (state) => state.sharedOrderUi.currentOrderType
  );
  const uiOrderAdjustments: any = useSelector(orderAdjustmentsSelector);
  const discounts = uiOrderAdjustments.filter(
    (a: OrderAdjustment) => a.orderAdjustmentTypeId === "DISCOUNT_ADJUSTMENT"
  );
  const discountTotal = -discounts.reduce(
    (a: number, b: OrderAdjustment) => a + Math.abs(b.amount!),
    0
  );
  //console.log("discountTotal from discountTotal", taxTotal);

  const nonDiscountAdjustmentsTotal = aTotal + Math.abs(discountTotal);

  return (
    <Grid container alignItems="flex-end" direction={"column"} mx={1} pt={2}>
      <Grid item>
        <Grid container>
          <Typography sx={{ p: 0 }} variant="h6">
            {getTranslatedLabel(`${localizationKey}.sub`, "Sub Total: ")}
          </Typography>
          <Typography sx={{ color: "red", px: 1 }} variant="h6">
            {" "}
            {sTotal.toFixed(2)}{" "}
          </Typography>
        </Grid>
      </Grid>

      <Grid item>
        <Grid container>
          <Typography sx={{ p: 0 }} variant="h6">
            {getTranslatedLabel(`${localizationKey}.tax`, "Tax Total: ")}
          </Typography>
          <Typography sx={{ color: "red", px: 1 }} variant="h6">
            {" "}
            {taxTotal.toFixed(2)}{" "}
          </Typography>
        </Grid>
      </Grid>

      {discounts.length > 0 && (
        <Grid>
          <Grid container>
            <Typography sx={{ p: 0 }} variant="h6">
              {getTranslatedLabel(
                `${localizationKey}.discount`,
                "Discounts Total: "
              )}
            </Typography>
            <Typography sx={{ color: "red", px: 1 }} variant="h6">
              {" "}
              {discountTotal.toFixed(2)}{" "}
            </Typography>
          </Grid>
        </Grid>
      )}

      <Grid item xs={6}>
        <Grid container>
          <Typography sx={{ p: 0 }} variant="h6">
            {discountTotal > 0
              ? `${getTranslatedLabel(
                  `${localizationKey}.otherAdj`,
                  "Other Adjustments: "
                )}`
              : `${getTranslatedLabel(
                  `${localizationKey}.adj`,
                  "Adjustments: "
                )}`}
          </Typography>
          <Typography sx={{ color: "red", px: 1 }} variant="h6">
            {" "}
            {nonDiscountAdjustmentsTotal.toFixed(2)}{" "}
          </Typography>
        </Grid>
      </Grid>

      <Grid item>
        <Grid container>
          <Typography sx={{ p: 0 }} variant="h6">
            {getTranslatedLabel(`${localizationKey}.grand`, "Grand Total: ")}
          </Typography>
          <Typography sx={{ color: "red", px: 1 }} variant="h6">
            {" "}
            {(sTotal + taxTotal + aTotal+ discountTotal).toFixed(2)}{" "}
          </Typography>
        </Grid>
      </Grid>
    </Grid>
  );
}
