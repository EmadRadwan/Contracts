import React from "react";
import { Grid, Paper, Typography } from "@mui/material";
import AccountingMenu from "../menu/AccountingMenu";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import NewSalesInvoice from "./NewSalesInvoice";
import NewPurchaseInvoice from "./NewPurchaseInvoice";

const NewInvoice = () => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.invoices.form";

    return (
        <>
            <AccountingMenu selectedMenuItem="/invoices" />
            <Grid container spacing={2}>
                <Grid item xs={6}>
                    <Paper elevation={5} className="div-container-withBorderCurved">
                        <Typography variant="h4" sx={{ p: 2, color: "green" }}>
                            {getTranslatedLabel(
                                `${localizationKey}.sales`,
                                "New Sales Invoice"
                            )}
                        </Typography>
                        <NewSalesInvoice onClose={() => {}} />
                    </Paper>
                </Grid>
                <Grid item xs={6}>
                    <Paper elevation={5} className="div-container-withBorderCurved">
                        <Typography variant="h4" sx={{ p: 2, color: "green" }}>
                            {getTranslatedLabel(
                                `${localizationKey}.purchase`,
                                "New Purchase Invoice"
                            )}
                        </Typography>

                        <NewPurchaseInvoice onClose={() => {}} />
                    </Paper>
                </Grid>
            </Grid>
        </>
    );
};

export default NewInvoice;