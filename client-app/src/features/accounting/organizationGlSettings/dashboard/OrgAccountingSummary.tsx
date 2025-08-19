import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Grid, Paper, Typography } from "@mui/material";
import React, { useEffect } from "react";
import { RootState } from "../../../../app/store/configureStore";
import { useSelector } from "react-redux";
import SetupAccountingMenu from "../menu/SetupAccountingMenu";
import AccountingSummaryMenu from "../menu/AccountingSummaryMenu";
import { router } from "../../../../app/router/Routes";
import { Button } from "@progress/kendo-react-buttons";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

const OrgAccountingSummary = () => {
  const companyName = useSelector(
    (state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName
  );
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.accounting.summary";

  // if company name is not set, then redirect to the orgGl
  useEffect(() => {
    if (!companyName) {
      router.navigate("/orgGl");
    }
  }, [companyName]);

  return (
    <>
      <AccountingMenu selectedMenuItem={"/orgGl"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container spacing={1} alignItems={"center"}>
          <Grid item xs={12}>
            <Typography sx={{ p: 2 }} variant="h5">
              {getTranslatedLabel(
                `${localizationKey}.title`,
                "Accounting Summary for"
              )}{": "}
              {companyName}
            </Typography>
          </Grid>
          <SetupAccountingMenu />
          <AccountingSummaryMenu />
        </Grid>
        <Grid item xs={12} sx={{ paddingBottom: 1, marginInlineStart: 2 }}>
          <Button
            themeColor={"success"}
            onClick={() => router.navigate("/glQuickCreateAccountingTransaction")}
          >
            {getTranslatedLabel(`${localizationKey}.qCreateTxn`, "Quick Create an Accounting Transaction")}
          </Button>
        </Grid>
        
        <Grid item xs={12} sx={{ paddingBottom: 1, marginInlineStart: 2 }}>
          <Button
            themeColor={"success"}
            onClick={() => router.navigate("/glCreateAccountingTransaction")}
          >
            {getTranslatedLabel(`${localizationKey}.createTxn`, "Create an Accounting Transaction")}
          </Button>
        </Grid>
        <Grid item xs={12} sx={{ paddingBottom: 1, marginInlineStart: 2 }}>
          <Button
            themeColor={"success"}
            onClick={() => router.navigate("/accountingTransaction", {state: {isPosted: "N"}})}
          >
            {getTranslatedLabel(`${localizationKey}.unposted`, "Un-posted Accounting Transactions")}
          </Button>
        </Grid>
        <Grid item xs={12} sx={{ paddingBottom: 1, marginInlineStart: 2 }}>
          <Button
            themeColor={"success"}
            onClick={() => router.navigate("/trialBalance")}
          >
            {getTranslatedLabel(`${localizationKey}.trial`, "Trial Balance")}
          </Button>
        </Grid>
        <Grid item xs={12} sx={{ paddingBottom: 1, marginInlineStart: 2 }}>
          <Button
            themeColor={"success"}
            onClick={() => router.navigate("/incomeStatement")}
          >
            {getTranslatedLabel(`${localizationKey}.income`, "Income Statement")}
          </Button>
        </Grid>
        <Grid item xs={12} sx={{ paddingBottom: 1, marginInlineStart: 2 }}>
          <Button
            themeColor={"success"}
            onClick={() => router.navigate("/balanceSheet")}
          >
            {getTranslatedLabel(`${localizationKey}.balanceSheet`, "Balance Sheet")}
          </Button>
        </Grid>
      </Paper>
    </>
  );
};

export default OrgAccountingSummary;
