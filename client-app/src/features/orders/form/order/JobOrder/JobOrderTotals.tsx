import Grid from "@mui/material/Grid";
import {Typography} from "@mui/material";
import React from "react";
import {useSelector} from "react-redux";
import {jobOrderLevelAdjustmentsTotal, jobOrderPaymentsTotal, jobOrderSubTotal,} from "../../../slice/jobOrderUiSlice";

export default function JobOrderTotals() {
    const sTotal: any = useSelector(jobOrderSubTotal);
    const aTotal: any = useSelector(jobOrderLevelAdjustmentsTotal);
    const paidAmount: any = useSelector(jobOrderPaymentsTotal);

    return (
        <Grid container alignItems="flex-end" direction={"column"}>
            <Grid item xs={3}>
                <Grid container>
                    <Typography sx={{p: 0}} variant="h6">
                        Sub Total{" "}
                    </Typography>
                    <Typography sx={{color: "red", pl: 1}} variant="h6">
                        {" "}
                        {sTotal.toFixed(2)}{" "}
                    </Typography>
                </Grid>
            </Grid>
            <Grid item xs={3}>
                <Grid container>
                    <Typography sx={{p: 0}} variant="h6">
                        Order Adjustments{" "}
                    </Typography>
                    <Typography sx={{color: "red", pl: 1}} variant="h6">
                        {" "}
                        {aTotal.toFixed(2)}{" "}
                    </Typography>
                </Grid>
            </Grid>
            <Grid item xs={3}>
                <Grid container>
                    <Typography sx={{p: 0}} variant="h6">
                        Grand Total{" "}
                    </Typography>
                    <Typography sx={{color: "red", pl: 1}} variant="h6">
                        {" "}
                        {(sTotal + aTotal).toFixed(2)}{" "}
                    </Typography>
                </Grid>
            </Grid>
            <Grid item xs={3}>
                <Grid container>
                    <Typography sx={{p: 0}} variant="h6">
                        Paid Amount
                    </Typography>
                    <Typography sx={{color: "red", pl: 1}} variant="h6">
                        {" "}
                        {paidAmount.toFixed(2)}{" "}
                    </Typography>
                </Grid>
            </Grid>
        </Grid>
    );
}
