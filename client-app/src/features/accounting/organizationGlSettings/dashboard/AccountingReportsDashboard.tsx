import React, { useEffect } from "react";
import { router } from "../../../../app/router/Routes";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import AccountingReportsMenu from "../menu/AccountingReportsMenu";
import AccountingSummaryMenu from "../menu/AccountingSummaryMenu";
import SetupAccountingMenu from "../menu/SetupAccountingMenu";
import { useAppSelector } from "../../../../app/store/configureStore";
import {
  Box,
  Card,
  CardActionArea,
  CardContent,
  Grid,
  Paper,
  Typography,
  useTheme,
} from "@mui/material";
import { Link } from "react-router-dom";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

const links = [
  { 
    path: "/trialBalance", 
    title: "Trial Balance", 
    key: "trial-balance" 
  },
  {
    path: "/transactionTotals",
    title: "Transaction Totals",
    key: "transaction-totals",
  },
  {
    path: "/incomeStatement",
    title: "Income Statement",
    key: "income-statement",
  },
  {
    path: "/cashFlowStatement",
    title: "Cash Flow Statement",
    key: "cash-flow-statement",
  },
  { 
    path: "/balanceSheet", 
    title: "Balance Sheet", 
    key: "balance-sheet" 
  },
  {
    path: "/comparativeIncomeStatement",
    title: "Comparative Income Statement",
    key: "comparative-income-statement",
  },
  {
    path: "/comparativeCashFlowStatement",
    title: "Comparative Cash Flow Statement",
    key: "comparative-cash-flow-statement",
  },
  {
    path: "/comparativeBalanceSheet",
    title: "Comparative Balance Sheet",
    key: "comparative-balance-sheet",
  },
  {
    path: "/glAccountTrialBalance",
    title: "GL Account Trial Balance",
    key: "gl-account-trial-balance",
  },
  {
    path: "/inventoryValuation",
    title: "Inventory Valuation",
    key: "inventory-valuation",
  },
  // { 
  //   path: "/costCenters", 
  //   title: "Cost Centers", 
  //   key: "cost-centers" 
  // },
];

const AccountingReportsDashboard = () => {
  const { selectedAccountingCompanyId } = useAppSelector(
    (state) => state.accountingSharedUi
  );
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }
  const theme = useTheme();
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports"
  return (
    <>
      <AccountingMenu selectedMenuItem={"/orgGl"} />
      <Grid container padding={2} columnSpacing={1}>
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
          <SetupAccountingMenu selectedMenuItem="orgAccountingSummary" />
          <AccountingSummaryMenu selectedMenuItem="accountingReports" />
          <Grid item xs={12} mt={4} mb={6} textAlign={"center"}>
            <Typography variant="h3">{getTranslatedLabel(`${localizationKey}.title`, "Select a report to display")}</Typography>
          </Grid>
          {/* <AccountingReportsMenu /> */}
          <Box sx={{ padding: theme.spacing(3) }}>
            <Grid container spacing={3}>
              {links.map(({ title, path, key }, index) => (
                <Grid item xs={12} sm={6} md={4} key={index}>
                  <Card
                    variant="outlined"
                    sx={{
                      borderRadius: "8px",
                      boxShadow: theme.shadows[1],
                      "&:hover": {
                        boxShadow: theme.shadows[4],
                      },
                    }}
                  >
                    <CardActionArea component={Link} to={path}>
                      <CardContent>
                        <Typography
                          variant="h6"
                          component="div"
                          sx={{
                            textAlign: "center",
                            color: theme.palette.primary.main,
                            fontWeight: "bold",
                          }}
                        >
                          {getTranslatedLabel(`${localizationKey}.menu.${key}`, title)}
                        </Typography>
                      </CardContent>
                    </CardActionArea>
                  </Card>
                </Grid>
              ))}
            </Grid>
          </Box>
        </Paper>
      </Grid>
    </>
  );
};

export default AccountingReportsDashboard;
