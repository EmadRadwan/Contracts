import { Paper, Typography, Grid, Box, Button } from "@mui/material";
import { useState } from "react";
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
import { orderBy, SortDescriptor } from "@progress/kendo-data-query";
import { toast } from "react-toastify";
import { GRID_COL_INDEX_ATTRIBUTE } from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import IncomeStatementGlAccountTransactionsModal from "./IncomeStatementGlAccountTransactionsModal";
import { formatNumber } from "../../../../app/util/utils";

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

  // Each row shows the same account in two periods, so the drill-down has to know which column
  // was clicked — the transactions behind Period 1 are a different window from Period 2.
  type PeriodParams = {
    fromDate?: string;
    thruDate?: string;
    selectedMonth?: number;
    glFiscalTypeId: string;
  };
  const [periods, setPeriods] = useState<{ p1: PeriodParams; p2: PeriodParams } | null>(null);
  const [drill, setDrill] = useState<{ glAccountId: string; period: 1 | 2 } | null>(null);

  const onSubmit = (values: any) => {
    const { fromDate1, thruDate1, selectedMonth1, glFiscalTypeId1, fromDate2, thruDate2, selectedMonth2, glFiscalTypeId2 } = values;

    if ((!fromDate1 && !thruDate1 && !selectedMonth1) || (!fromDate2 && !thruDate2 && !selectedMonth2)) {
      toast.error(getTranslatedLabel("general.date-range-or-month-error", "Must select month or date range for both periods!"));
      return;
    }

    const p1: PeriodParams = {
      fromDate: fromDate1 ? new Date(fromDate1).toISOString().split('T')[0] : undefined,
      thruDate: thruDate1 ? new Date(thruDate1).toISOString().split('T')[0] : undefined,
      selectedMonth: selectedMonth1 ? selectedMonth1 - 1 : undefined,
      glFiscalTypeId: glFiscalTypeId1,
    };
    const p2: PeriodParams = {
      fromDate: fromDate2 ? new Date(fromDate2).toISOString().split('T')[0] : undefined,
      thruDate: thruDate2 ? new Date(thruDate2).toISOString().split('T')[0] : undefined,
      selectedMonth: selectedMonth2 ? selectedMonth2 - 1 : undefined,
      glFiscalTypeId: glFiscalTypeId2,
    };
    setPeriods({ p1, p2 });
    setDrill(null);

    trigger({
      organizationPartyId: selectedAccountingCompanyId!,
      glFiscalTypeId1,
      fromDate1: p1.fromDate,
      thruDate1: p1.thruDate,
      selectedMonth1: p1.selectedMonth,
      glFiscalTypeId2,
      fromDate2: p2.fromDate,
      thruDate2: p2.thruDate,
      selectedMonth2: p2.selectedMonth,
    });
  };

  // Balance cells open the drill-down for their own period.
  const makeBalanceCell = (period: 1 | 2) => (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    const value = props.dataItem[period === 1 ? "balance1" : "balance2"];
    return (
      <td
        className={props.className}
        style={{ ...props.style, color: "blue" }}
        colSpan={props.colSpan}
        role={"gridcell"}
        aria-colindex={props.ariaColumnIndex}
        aria-selected={props.isSelected}
        {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
        {...navigationAttributes}
      >
        <Button onClick={() => setDrill({ glAccountId: props.dataItem.glAccountId, period })}>
          {formatNumber(value)}
        </Button>
      </td>
    );
  };
  const Balance1Cell = makeBalanceCell(1);
  const Balance2Cell = makeBalanceCell(2);

  const renderGrid = (data: any[], title: string) => {
    const processedData = orderBy(data || [], sort);
    return (
        <Grid item xs={12} sx={{ mt: 2 }}>
          <KendoGrid scrollable="scrollable"
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
            <Column field="balance1" title={getTranslatedLabel(`${localizationKey}.period1`, "Period 1")} cells={{ data: Balance1Cell }} />
            <Column field="balance2" title={getTranslatedLabel(`${localizationKey}.period2`, "Period 2")} cells={{ data: Balance2Cell }} />
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

      {drill && periods && (
        <ModalContainer show={true} onClose={() => setDrill(null)} width={1280}>
          <IncomeStatementGlAccountTransactionsModal
            onClose={() => setDrill(null)}
            organizationPartyId={selectedAccountingCompanyId!}
            fromDate={(drill.period === 1 ? periods.p1 : periods.p2).fromDate}
            thruDate={(drill.period === 1 ? periods.p1 : periods.p2).thruDate}
            selectedMonth={(drill.period === 1 ? periods.p1 : periods.p2).selectedMonth}
            glFiscalTypeId={(drill.period === 1 ? periods.p1 : periods.p2).glFiscalTypeId}
            glAccountId={drill.glAccountId}
          />
        </ModalContainer>
      )}
    </>
  );
};

export default ComparativeIncomeStatement;
