import Grid from "@mui/material/Grid";
import { Typography } from "@mui/material";
import { useSelector } from "react-redux";
import { useAppSelector } from "../../../../app/store/configureStore";
import {
  orderAdjustmentsSelector,
  orderLevelAdjustmentsTotal,
  orderLevelTaxTotal,
  orderSubTotal,
} from "../../slice/orderSelectors";
import { OrderAdjustment } from "../../../../app/models/order/orderAdjustment";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

// REFACTOR: Define explicit types for clarity and type safety
interface OrderTotalsValues {
  subTotal: number;
  adjustmentsTotal: number;
  taxTotal: number;
  adjustments: OrderAdjustment[];
}

export default function OrderTotals() {
  const localizationKey = "order.totals";
  const { getTranslatedLabel } = useTranslationHelper();

  // REFACTOR: Use explicit types instead of 'any' to prevent type errors
  const sTotal: number = useSelector(orderSubTotal);
  const aTotal: number = useSelector(orderLevelAdjustmentsTotal);
  const taxTotal: number = useAppSelector(orderLevelTaxTotal);
  const uiOrderAdjustments: OrderAdjustment[] = useSelector(orderAdjustmentsSelector);

  // REFACTOR: Calculate discount total with null check for amount
  const discounts = uiOrderAdjustments.filter(
      (a: OrderAdjustment) => a.orderAdjustmentTypeId === "DISCOUNT_ADJUSTMENT"
  );
  const discountTotal = -discounts.reduce(
      (sum: number, adj: OrderAdjustment) => sum + Math.abs(adj.amount || 0),
      0
  );

  // REFACTOR: Calculate non-discount adjustments (shipping, misc, etc.) for all non-deleted adjustments
  const nonDiscountAdjustments = uiOrderAdjustments.filter(
      (adj: OrderAdjustment) =>
          adj.orderAdjustmentTypeId !== "DISCOUNT_ADJUSTMENT" &&
          !adj.isAdjustmentDeleted
  );
  const nonDiscountAdjustmentsTotal = nonDiscountAdjustments.reduce(
      (sum: number, adj: OrderAdjustment) => sum + (adj.amount || 0),
      0
  );

  // REFACTOR: Calculate grand total including all relevant adjustments
  const grandTotal = sTotal + taxTotal + nonDiscountAdjustmentsTotal + discountTotal;

  // REFACTOR: Log values for debugging to verify calculations
  // console.log({
  //   sTotal,
  //   taxTotal,
  //   discountTotal,
  //   nonDiscountAdjustmentsTotal,
  //   grandTotal,
  // });

  return (
      <Grid container alignItems="flex-end" direction="column" mx={1} pt={2}>
        <Grid item>
          <Grid container>
            <Typography sx={{ p: 0 }} variant="h6">
              {getTranslatedLabel(`${localizationKey}.sub`, "Sub Total: ")}
            </Typography>
            <Typography sx={{ color: "red", px: 1 }} variant="h6">
              {sTotal.toFixed(2)}
            </Typography>
          </Grid>
        </Grid>
        <Grid item>
          <Grid container>
            <Typography sx={{ p: 0 }} variant="h6">
              {getTranslatedLabel(`${localizationKey}.tax`, "Tax Total: ")}
            </Typography>
            <Typography sx={{ color: "red", px: 1 }} variant="h6">
              {taxTotal.toFixed(2)}
            </Typography>
          </Grid>
        </Grid>
        {discounts.length > 0 && (
            <Grid item>
              <Grid container>
                <Typography sx={{ p: 0 }} variant="h6">
                  {getTranslatedLabel(`${localizationKey}.discount`, "Discounts Total: ")}
                </Typography>
                <Typography sx={{ color: "red", px: 1 }} variant="h6">
                  {discountTotal.toFixed(2)}
                </Typography>
              </Grid>
            </Grid>
        )}
        {nonDiscountAdjustments.length > 0 && (
            // REFACTOR: Conditionally display non-discount adjustments to avoid empty rows
            <Grid item xs={6}>
              <Grid container>
                <Typography sx={{ p: 0 }} variant="h6">
                  {getTranslatedLabel(`${localizationKey}.otherAdj`, "Other Adjustments: ")}
                </Typography>
                <Typography sx={{ color: "red", px: 1 }} variant="h6">
                  {nonDiscountAdjustmentsTotal.toFixed(2)}
                </Typography>
              </Grid>
            </Grid>
        )}
        <Grid item>
          <Grid container>
            <Typography sx={{ p: 0 }} variant="h6">
              {getTranslatedLabel(`${localizationKey}.grand`, "Grand Total: ")}
            </Typography>
            <Typography sx={{ color: "red", px: 1 }} variant="h6">
              {grandTotal.toFixed(2)}
            </Typography>
          </Grid>
        </Grid>
      </Grid>
  );
}