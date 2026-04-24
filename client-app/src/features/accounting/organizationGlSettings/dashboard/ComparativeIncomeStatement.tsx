import { Paper, Typography, Grid, Box } from "@mui/material";
import React, { useState, useMemo } from "react";
import { router } from "../../../../app/router/Routes";
import { useAppSelector } from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import ComparativeIncomeStatementForm from "../form/ComparativeIncomeStatementForm";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
import { useLazyFetchComparativeIncomeStatementReportQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { filterBy, orderBy, SortDescriptor } from "@progress/kendo-data-query";
import { toast } from "react-toastify";

const ComparativeIncomeStatement = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.comparative-income-statement.list";

  const { selectedAccountingCompanyName, selectedAccountingCompanyId } =
    useAppSelector((state) => state.accountingSharedUi);

  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
    return null;
  }

  const [trigger, { data: report, isFetching, isSuccess, isLoading }] =
      useLazyFetchComparativeIncomeStatementReportQuery();

  const [sort, setSort] = useState<Array<SortDescriptor>>([{ field: "accountCode", dir: "asc" }]);

  const onSubmit = (values: any) => {
    const { fromDate1, thruDate1, selectedMonth1, glFiscalTypeId1, fromDate2, thruDate2, selectedMonth2, glFiscalTypeId2 } = values;

    if ((!fromDate1 && !thruDate1 && !selectedMonth1) || (!fromDate2 && !thruDate2 && !selectedMonth2)) {
      toast.error(getTranslatedLabel("general.date-range-or-month-error", "Must select month or date range for both periods!"));
      return;
    }

    trigger({
      organizationPartyId: selectedAccountingCompanyId!,
      glFiscalTypeId1,
      fromDate1: fromDate1 ? new Date(fromDate1).toISOString().split('T')[0] : undefined,
      thruDate1: thruDate1 ? new Date(thruDate1).toISOString().split('T')[0] : undefined,
      selectedMonth1: selectedMonth1 ? selectedMonth1 - 1 : undefined,
      glFiscalTypeId2,
      fromDate2: fromDate2 ? new Date(fromDate2).toISOString().split('T')[0] : undefined,
      thruDate2: thruDate2 ? new Date(thruDate2).toISOString().split('T')[0] : undefined,
      selectedMonth2: selectedMonth2 ? selectedMonth2 - 1 : undefined,
    });
  };

  const formatNumber = (value: number | undefined) => {
    return value?.toLocaleString('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }) || '0.00';
  };

  const renderGrid = (data: any[], title: string) => {
    const processedData = orderBy(data || [], sort);
    return (
        <Grid item xs={12} sx={{ mt: 2 }}>
          <KendoGrid
              style={{ height: "400px" }}
              data={processedData}
              sortable={true}
              sort={sort}
              onSortChange={(e) => setSort(e.sort)}
          >
            <GridToolbar>
              <Typography variant="body1">{title}</Typography>
            </GridToolbar>
            <Column field="accountCode" title={getTranslatedLabel(`${localizationKey}.code`, "Code")} />
            <Column field="accountName" title={getTranslatedLabel(`${localizationKey}.name`, "Name")} />
            <Column field="balance1" title={getTranslatedLabel(`${localizationKey}.period1`, "Period 1")} format="{0:n2}" />
            <Column field="balance2" title={getTranslatedLabel(`${localizationKey}.period2`, "Period 2")} format="{0:n2}" />
          </KendoGrid>
        </Grid>
    );
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
            {getTranslatedLabel(`${localizationKey}.title`, "Comparative Income Statement For: ")}
            {selectedAccountingCompanyName}
          </Typography>
          <Grid item xs={12} sx={{ margin: 3 }}>
            <ComparativeIncomeStatementForm onSubmit={onSubmit} />
          </Grid>

          {isSuccess && report && (
              <Box sx={{ p: 3 }}>
                {renderGrid(report.revenueAccountBalances, getTranslatedLabel(`${localizationKey}.revenues`, "Revenues"))}
                {report.cogsExpenseAccountBalances?.length > 0 && renderGrid(report.cogsExpenseAccountBalances, getTranslatedLabel(`${localizationKey}.cost-of-goods-sold`, "Cost of Goods Sold"))}
                {renderGrid(report.expenseAccountBalances, getTranslatedLabel(`${localizationKey}.expenses`, "Expenses"))}
                {report.incomeAccountBalances?.length > 0 && renderGrid(report.incomeAccountBalances, getTranslatedLabel(`${localizationKey}.income`, "Other Income"))}

                {/* Totals Section */}
                <Grid container spacing={2} sx={{ mt: 4 }}>
                  <Grid item xs={12} md={6}>
                    <Box sx={{ border: "1px solid", borderColor: "grey.400", p: 2, borderRadius: 2 }}>
                      <Typography variant="h6" gutterBottom>{getTranslatedLabel(`${localizationKey}.totals`, "Totals")}</Typography>
                      <Grid container spacing={1}>
                        <Grid item xs={6}><Typography>{getTranslatedLabel(`${localizationKey}.net-sales`, "Net Sales")}</Typography></Grid>
                        <Grid item xs={3}><Typography align="right">{formatNumber(report.netSales1)}</Typography></Grid>
                        <Grid item xs={3}><Typography align="right">{formatNumber(report.netSales2)}</Typography></Grid>

                        <Grid item xs={6}><Typography>{getTranslatedLabel(`${localizationKey}.gross-margin`, "Gross Margin")}</Typography></Grid>
                        <Grid item xs={3}><Typography align="right">{formatNumber(report.grossMargin1)}</Typography></Grid>
                        <Grid item xs={3}><Typography align="right">{formatNumber(report.grossMargin2)}</Typography></Grid>

                        <Grid item xs={6}><Typography>{getTranslatedLabel(`${localizationKey}.operating-expenses`, "Operating Expenses")}</Typography></Grid>
                        <Grid item xs={3}><Typography align="right">{formatNumber(report.operatingExpenses1)}</Typography></Grid>
                        <Grid item xs={3}><Typography align="right">{formatNumber(report.operatingExpenses2)}</Typography></Grid>

                        <Grid item xs={6}><Typography variant="h6" fontWeight="bold">{getTranslatedLabel(`${localizationKey}.net-income`, "Net Income")}</Typography></Grid>
                        <Grid item xs={3}><Typography align="right" fontWeight="bold" color={report.netIncome1 >= 0 ? "success.main" : "error.main"}>{formatNumber(report.netIncome1)}</Typography></Grid>
                        <Grid item xs={3}><Typography align="right" fontWeight="bold" color={report.netIncome2 >= 0 ? "success.main" : "error.main"}>{formatNumber(report.netIncome2)}</Typography></Grid>
                      </Grid>
                    </Box>
                  </Grid>
                </Grid>
              </Box>
          )}

          {(isLoading || isFetching) && (
              <LoadingComponent
                  message={getTranslatedLabel("general.loading-report", "Loading Report Data...")}
              />
          )}
        </Paper>
      </Grid>
    </>
  );
};

export default ComparativeIncomeStatement;
