import Grid from "@mui/material/Grid";
import {Typography} from "@mui/material";
import {useSelector} from "react-redux";
import {quoteLevelAdjustmentsTotal, quoteSubTotal} from "../../orders/slice/quoteSelectors";
import { formatNumber } from "../../../app/util/utils";

export default function QuoteTotals() {
    const sTotal: any = useSelector(quoteSubTotal);
    const aTotal: any = useSelector(quoteLevelAdjustmentsTotal)

    return <Grid container alignItems="flex-end" direction={"column"}>
        <Grid item xs={3}>
            <Grid container>
                <Typography sx={{p: 0}} variant="h6">Sub Total </Typography>
                <Typography sx={{color: "red", pl: 1}}
                            variant="h6"> {formatNumber(sTotal)} </Typography>
            </Grid>

        </Grid>
        <Grid item xs={3}>
            <Grid container>
                <Typography sx={{p: 0}} variant="h6">Adjustments </Typography>
                <Typography sx={{color: "red", pl: 1}}
                            variant="h6"> {formatNumber(aTotal)} </Typography>
            </Grid>

        </Grid>
        <Grid item xs={3}>
            <Grid container>
                <Typography sx={{p: 0}} variant="h6">Grand Total </Typography>
                <Typography sx={{color: "red", pl: 1}}
                            variant="h6"> {formatNumber(sTotal + aTotal)} </Typography>
            </Grid>

        </Grid>

    </Grid>
}