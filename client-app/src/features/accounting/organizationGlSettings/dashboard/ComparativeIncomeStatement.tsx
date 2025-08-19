import { Paper, Typography, Grid } from "@mui/material";
import React from "react";
import { router } from "../../../../app/router/Routes";
import { useAppSelector } from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import AccountingReportsMenu from "../menu/AccountingReportsMenu";
import AccountingSummaryMenu from "../menu/AccountingSummaryMenu";
import SetupAccountingMenu from "../menu/SetupAccountingMenu";
import ComparativeIncomeStatementForm from "../form/ComparativeIncomeStatementForm";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";

const ComparativeIncomeStatement = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const { selectedAccountingCompanyName, selectedAccountingCompanyId } =
    useAppSelector((state) => state.accountingSharedUi);
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }

  const onSubmit = (values: any) => {
    console.log(values);
  };
  return (
    <>
      <AccountingMenu selectedMenuItem={"/orgGl"} />
      <Grid container padding={2} columnSpacing={1}>
        <Paper
          elevation={5}
          className={`div-container-withBorderCurved`}
          sx={{ width: "100%" }}
        >
          <AccountingReportBreadcrumbs />

          <Typography variant="h4" margin={3}>
            Comparative Income Statement For: {selectedAccountingCompanyName}
          </Typography>
          <Grid item xs={12} sx={{ margin: 3 }}>
            <ComparativeIncomeStatementForm onSubmit={onSubmit} />
          </Grid>
        </Paper>
      </Grid>
    </>
  );
};

export default ComparativeIncomeStatement;
