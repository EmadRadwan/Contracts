import Grid from "@mui/material/Grid";
import {Typography} from "@mui/material";
import {useSelector} from "react-redux";
import {quoteLevelAdjustmentsTotal, quoteSubTotal} from "../../orders/slice/quoteSelectors";

export default function QuoteTotals() {
    const sTotal: any = useSelector(quoteSubTotal);
    const aTotal: any = useSelector(quoteLevelAdjustmentsTotal)

    return <Grid container alignItems="flex-end" direction={"column"}>
        <Grid item xs={3}>
            <Grid container>
                <Typography sx={{p: 0}} variant="h6">Sub Total </Typography>
                <Typography sx={{color: "red", pl: 1}}
                            variant="h6"> {sTotal.toFixed(2)} </Typography>
            </Grid>

        </Grid>
        <Grid item xs={3}>
            <Grid container>
                <Typography sx={{p: 0}} variant="h6">Adjustments </Typography>
                <Typography sx={{color: "red", pl: 1}}
                            variant="h6"> {aTotal.toFixed(2)} </Typography>
            </Grid>

        </Grid>
        <Grid item xs={3}>
            <Grid container>
                <Typography sx={{p: 0}} variant="h6">Grand Total </Typography>
                <Typography sx={{color: "red", pl: 1}}
                            variant="h6"> {(sTotal + aTotal).toFixed(2)} </Typography>
            </Grid>

        </Grid>

    </Grid>
}