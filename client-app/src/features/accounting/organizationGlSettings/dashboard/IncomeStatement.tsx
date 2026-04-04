import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Box, Grid, Paper, Typography, Button } from "@mui/material"; 
import IncomeStatementForm from "../form/IncomeStatementForm";
import { router } from "../../../../app/router/Routes";
import { useAppSelector } from "../../../../app/store/configureStore";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
import { useState } from "react";                                      // Removed unused useEffect
import { toast } from "react-toastify";
import { useLazyFetchIncomeStatementReportQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridToolbar,
  GRID_COL_INDEX_ATTRIBUTE,
  GridPageChangeEvent,
  GridFilterChangeEvent
} from "@progress/kendo-react-grid";
import {CompositeFilterDescriptor, filterBy, orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { IncomeStatementExcel } from "../report/IncomeStatementExcel";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import IncomeStatementGlAccountTransactionsModal from "./IncomeStatementGlAccountTransactionsModal";
import {useMemo} from "react";

type ReportData = {
  glFiscalTypeId: string;
  fromDate?: string;
  thruDate?: string;
  selectedMonth?: number | undefined;
};

const IncomeStatement = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.income-statement.list";

  const { selectedAccountingCompanyName, selectedAccountingCompanyId } = useAppSelector(
      (state) => state.accountingSharedUi
  );

  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
    return null;                    // Added return to prevent further rendering
  }

  const [filter, setFilter] = useState<CompositeFilterDescriptor | undefined>(undefined);
  const [filter2, setFilter2] = useState<CompositeFilterDescriptor | undefined>(undefined);
  const [filter3, setFilter3] = useState<CompositeFilterDescriptor | undefined>(undefined);
  const [filter4, setFilter4] = useState<CompositeFilterDescriptor | undefined>(undefined);

  const initialSort: Array<SortDescriptor> = [
    {field: "accountCode", dir: "asc"},
  ];
  const [sort, setSort] = useState(initialSort);
  const [sort2, setSort2] = useState(initialSort);
  const [sort3, setSort3] = useState(initialSort);
  const [sort4, setSort4] = useState(initialSort);

  const initialDataState: State = {skip: 0, take: 25};
  const [page, setPage] = useState<State>(initialDataState);
  const [page2, setPage2] = useState<State>(initialDataState);
  const [page3, setPage3] = useState<State>(initialDataState);
  const [page4, setPage4] = useState<State>(initialDataState);

  const pageChange = (event: GridPageChangeEvent) => setPage(event.page);
  const pageChange2 = (event: GridPageChangeEvent) => setPage2(event.page);
  const pageChange3 = (event: GridPageChangeEvent) => setPage3(event.page);
  const pageChange4 = (event: GridPageChangeEvent) => setPage4(event.page);

  const onFilterChange = (event: GridFilterChangeEvent) => setFilter(event.filter);
  const onFilterChange2 = (event: GridFilterChangeEvent) => setFilter2(event.filter);
  const onFilterChange3 = (event: GridFilterChangeEvent) => setFilter3(event.filter);
  const onFilterChange4 = (event: GridFilterChangeEvent) => setFilter4(event.filter);

  const [reportData, setReportData] = useState<ReportData>({
    glFiscalTypeId: "ACTUAL",
    fromDate: undefined,
    thruDate: undefined,
    selectedMonth: undefined,
  });

  const [showTransactionsModal, setShowTransactionsModal] = useState(false);
  const [selectedGlAccountId, setSelectedGlAccountId] = useState<string | null>(null);

  const [trigger, { data: report, isFetching, isSuccess, isLoading }] =
      useLazyFetchIncomeStatementReportQuery();

  const processedDataRevenues = useMemo(() => {
    const filtered = filterBy(report?.revenueAccountBalances ?? [], filter);
    const sorted = orderBy(filtered, sort);
    return sorted.slice(page.skip, page.skip + page.take);
  }, [report?.revenueAccountBalances, filter, sort, page.skip, page.take]);

  const totalRevenues = filter ? filterBy(report?.revenueAccountBalances ?? [], filter).length : report?.revenueAccountBalances?.length ?? 0;

  const processedDataCogs = useMemo(() => {
    const filtered = filterBy(report?.cogsExpenseAccountBalances ?? [], filter2);
    const sorted = orderBy(filtered, sort2);
    return sorted.slice(page2.skip, page2.skip + page2.take);
  }, [report?.cogsExpenseAccountBalances, filter2, sort2, page2.skip, page2.take]);

  const totalCogs = filter2 ? filterBy(report?.cogsExpenseAccountBalances ?? [], filter2).length : report?.cogsExpenseAccountBalances?.length ?? 0;

  const processedDataExpenses = useMemo(() => {
    const filtered = filterBy(report?.expenseAccountBalances ?? [], filter3);
    const sorted = orderBy(filtered, sort3);
    return sorted.slice(page3.skip, page3.skip + page3.take);
  }, [report?.expenseAccountBalances, filter3, sort3, page3.skip, page3.take]);

  const totalExpenses = filter3 ? filterBy(report?.expenseAccountBalances ?? [], filter3).length : report?.expenseAccountBalances?.length ?? 0;

  const processedDataOtherIncome = useMemo(() => {
    const filtered = filterBy(report?.incomeAccountBalances ?? [], filter4);
    const sorted = orderBy(filtered, sort4);
    return sorted.slice(page4.skip, page4.skip + page4.take);
  }, [report?.incomeAccountBalances, filter4, sort4, page4.skip, page4.take]);

  const totalOtherIncome = filter4 ? filterBy(report?.incomeAccountBalances ?? [], filter4).length : report?.incomeAccountBalances?.length ?? 0;

  const onSubmit = (values: any) => {
    const { fromDate, thruDate, selectedMonth, glFiscalTypeId } = values;

    if (!fromDate && !thruDate && (selectedMonth === undefined || selectedMonth === null)) {
      toast.error(
          getTranslatedLabel("general.date-range-or-month-error", "Must select month or date range for report!")
      );
      return;
    }

    const formattedFromDate = fromDate ? new Date(fromDate).toISOString().split('T')[0] : undefined;
    const formattedThruDate = thruDate ? new Date(thruDate).toISOString().split('T')[0] : undefined;
    const processedMonth = selectedMonth ? selectedMonth - 1 : undefined;

    setReportData({
      glFiscalTypeId,
      fromDate: formattedFromDate,
      thruDate: formattedThruDate,
      selectedMonth: processedMonth,
    });

    trigger({
      organizationPartyId: selectedAccountingCompanyId!,
      glFiscalTypeId,
      fromDate: formattedFromDate,
      thruDate: formattedThruDate,
      selectedMonth: processedMonth,
    });
  };

  const formatNumber = (value: number | undefined) => {
    return value?.toLocaleString('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }) || '0.00';
  };

  const AccountCodeCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
        <td
            className={props.className}
            style={{ ...props.style, color: "blue" }}
            {...navigationAttributes}
            {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
        >
          <Button
              onClick={() => {
                setSelectedGlAccountId(props.dataItem.glAccountId);
                setShowTransactionsModal(true);
              }}
          >
            {props.dataItem.accountCode}
          </Button>
        </td>
    );
  };

  return (
      <>
        <AccountingMenu selectedMenuItem="/orgGl" />
        <Grid container padding={2} columnSpacing={1}>
          <Paper elevation={5} className="div-container-withBorderCurved" sx={{ width: "100%" }}>
            <AccountingReportBreadcrumbs />
            <Typography variant="h4" margin={3}>
              {getTranslatedLabel(`${localizationKey}.title`, "Income Statement For: ")}
              {selectedAccountingCompanyName}
            </Typography>

            <Grid item xs={12} sx={{ margin: 3, display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <IncomeStatementForm onSubmit={onSubmit} />

              {isSuccess && report && (
                  <IncomeStatementExcel
                      companyName={selectedAccountingCompanyName!}
                      revenueAccountBalances={report.revenueAccountBalances || []}
                      expenseAccountBalances={report.expenseAccountBalances || []}
                      incomeAccountBalances={report.incomeAccountBalances || []}
                      totals={{
                        contraRevenueBalanceTotal: report.contraRevenueBalanceTotal,
                        cogsExpenseBalanceTotal: report.cogsExpenseBalanceTotal,
                        netSales: report.netSales,
                        grossMargin: report.grossMargin,
                        depreciationBalanceTotal: report.depreciationBalanceTotal,
                        incomeFromOperations: report.incomeFromOperations,
                        netIncome: report.netIncome,
                      }}
                      getTranslatedLabel={getTranslatedLabel}
                      isFetching={isFetching}
                      fromDate={reportData.fromDate}
                      thruDate={reportData.thruDate}
                  />
              )}
            </Grid>

            {isSuccess && report && (
                <>
                  {/* 1. Revenue Grid */}
                  <Grid item xs={12}>
                    <div className="div-container">
                      <KendoGrid
                          style={{ height: "450px" }}
                          resizable={true}
                          pageable={true}
                          sortable={true}
                          filterable={true}
                          data={processedDataRevenues}
                          skip={page.skip}
                          take={page.take}
                          total={totalRevenues}
                          sort={sort}
                          filter={filter}
                          onPageChange={pageChange}
                          onSortChange={(e) => setSort(e.sort)}
                          onFilterChange={onFilterChange}
                      >
                        <GridToolbar>
                          <Typography variant="body1">
                            {getTranslatedLabel(`${localizationKey}.revenues`, "Revenues")}
                          </Typography>
                        </GridToolbar>
                        <Column field="accountCode" title={getTranslatedLabel(`${localizationKey}.code`, "Account Code")} cell={AccountCodeCell} />
                        <Column field="accountName" title={getTranslatedLabel(`${localizationKey}.name`, "Account Name")} />
                        <Column field="balance" title={getTranslatedLabel(`${localizationKey}.balance`, "Balance")} format="{0:n2}" />
                      </KendoGrid>
                    </div>
                  </Grid>

                  {/* 2. NEW: Cost of Goods Sold (COGS) Grid */}
                  {report.cogsExpenseAccountBalances?.length > 0 && (
                      <Grid item xs={12}>
                        <div className="div-container">
                          <KendoGrid
                              style={{ height: "450px" }}
                              resizable={true}
                              pageable={true}
                              sortable={true}
                              filterable={true}
                              data={processedDataCogs}
                              skip={page2.skip}
                              take={page2.take}
                              total={totalCogs}
                              sort={sort2}
                              filter={filter2}
                              onPageChange={pageChange2}
                              onSortChange={(e) => setSort2(e.sort)}
                              onFilterChange={onFilterChange2}
                          >
                            <GridToolbar>
                              <Typography variant="body1">
                                {getTranslatedLabel(`${localizationKey}.cost-of-goods-sold`, "Cost of Goods Sold")}
                              </Typography>
                            </GridToolbar>
                            <Column field="accountCode" title={getTranslatedLabel(`${localizationKey}.code`, "Account Code")} cell={AccountCodeCell} />
                            <Column field="accountName" title={getTranslatedLabel(`${localizationKey}.name`, "Account Name")} />
                            <Column field="balance" title={getTranslatedLabel(`${localizationKey}.balance`, "Balance")} format="{0:n2}" />
                          </KendoGrid>
                        </div>
                      </Grid>
                  )}

                  {/* 3. Expenses Grid (SGA + Depreciation) */}
                  <Grid item xs={12}>
                    <div className="div-container">
                      <KendoGrid
                          style={{ height: "450px" }}
                          resizable={true}
                          pageable={true}
                          sortable={true}
                          filterable={true}
                          data={processedDataExpenses}
                          skip={page3.skip}
                          take={page3.take}
                          total={totalExpenses}
                          sort={sort3}
                          filter={filter3}
                          onPageChange={pageChange3}
                          onSortChange={(e) => setSort3(e.sort)}
                          onFilterChange={onFilterChange3}
                      >
                        <GridToolbar>
                          <Typography variant="body1">
                            {getTranslatedLabel(`${localizationKey}.expenses`, "Expenses")}
                          </Typography>
                        </GridToolbar>
                        <Column field="accountCode" title={getTranslatedLabel(`${localizationKey}.code`, "Account Code")} cell={AccountCodeCell} />
                        <Column field="accountName" title={getTranslatedLabel(`${localizationKey}.name`, "Account Name")} />
                        <Column field="balance" title={getTranslatedLabel(`${localizationKey}.balance`, "Balance")} format="{0:n2}" />
                      </KendoGrid>
                    </div>
                  </Grid>

                  {/* 4. Other Income Grid (only show if has data) */}
                  {report.incomeAccountBalances?.length > 0 && (
                      <Grid item xs={12}>
                        <div className="div-container">
                          <KendoGrid
                              style={{ height: "450px" }}
                              resizable={true}
                              pageable={true}
                              sortable={true}
                              filterable={true}
                              data={processedDataOtherIncome}
                              skip={page4.skip}
                              take={page4.take}
                              total={totalOtherIncome}
                              sort={sort4}
                              filter={filter4}
                              onPageChange={pageChange4}
                              onSortChange={(e) => setSort4(e.sort)}
                              onFilterChange={onFilterChange4}
                          >
                            <GridToolbar>
                              <Typography variant="body1">
                                {getTranslatedLabel(`${localizationKey}.income`, "Other Income")}
                              </Typography>
                            </GridToolbar>
                            <Column field="accountCode" title={getTranslatedLabel(`${localizationKey}.code`, "Account Code")} cell={AccountCodeCell} />
                            <Column field="accountName" title={getTranslatedLabel(`${localizationKey}.name`, "Account Name")} />
                            <Column field="balance" title={getTranslatedLabel(`${localizationKey}.balance`, "Balance")} format="{0:n2}" />
                          </KendoGrid>
                        </div>
                      </Grid>
                  )}

                  {/* 5. Improved Totals Section */}
                  <Grid item xs={12} sx={{ mt: 4 }}>
                    <Typography variant="h5" ml={2}>
                      {getTranslatedLabel(`${localizationKey}.totals`, "Totals")}
                    </Typography>
                    <Grid item xs={6} ml={2}>
                      <Box sx={{ border: "1px solid", borderColor: "grey.400", padding: 2, borderRadius: "0.5rem" }}>
                        <Grid container spacing={1}>
                          <Grid item xs={6}><Typography variant="body1">{getTranslatedLabel(`${localizationKey}.net-sales`, "Net Sales")}</Typography></Grid>
                          <Grid item xs={6}><Typography align="right">{formatNumber(report.netSales)}</Typography></Grid>

                          <Grid item xs={6}><Typography variant="body1">{getTranslatedLabel(`${localizationKey}.gross-margin`, "Gross Margin")}</Typography></Grid>
                          <Grid item xs={6}><Typography align="right">{formatNumber(report.grossMargin)}</Typography></Grid>

                          <Grid item xs={6}><Typography variant="body1">{getTranslatedLabel(`${localizationKey}.cost-of-goods-sold`, "Cost of Goods Sold")}</Typography></Grid>
                          <Grid item xs={6}><Typography align="right">{formatNumber(report.cogsExpenseBalanceTotal)}</Typography></Grid>

                          <Grid item xs={6}><Typography variant="body1">{getTranslatedLabel(`${localizationKey}.operating-expenses`, "Operating Expenses")}</Typography></Grid>
                          <Grid item xs={6}><Typography align="right">
                            {formatNumber((report.sgaExpenseBalanceTotal || 0) + (report.depreciationBalanceTotal || 0))}
                          </Typography></Grid>

                          <Grid item xs={6}><Typography variant="body1">{getTranslatedLabel(`${localizationKey}.income-from-operations`, "Income From Operations")}</Typography></Grid>
                          <Grid item xs={6}><Typography align="right">{formatNumber(report.incomeFromOperations)}</Typography></Grid>

                          <Grid item xs={6}><Typography variant="body1">{getTranslatedLabel(`${localizationKey}.income`, "Other Income")}</Typography></Grid>
                          <Grid item xs={6}><Typography align="right">{formatNumber(report.incomeBalanceTotal)}</Typography></Grid>

                          <Grid item xs={6}><Typography variant="h6" fontWeight="bold">{getTranslatedLabel(`${localizationKey}.net-income`, "Net Income")}</Typography></Grid>
                          <Grid item xs={6}>
                            <Typography
                                variant="h6"
                                fontWeight="bold"
                                align="right"
                                color={report.netIncome >= 0 ? "success.main" : "error.main"}
                            >
                              {formatNumber(report.netIncome)}
                            </Typography>
                          </Grid>
                        </Grid>
                      </Box>
                    </Grid>
                  </Grid>
                </>
            )}

            {(isLoading || isFetching) && (
                <LoadingComponent
                    message={getTranslatedLabel("general.loading-report", "Loading Report Data...")}
                />
            )}
          </Paper>
        </Grid>

        {/* Transactions Modal */}
        {showTransactionsModal && selectedGlAccountId && (
            <ModalContainer
                show={showTransactionsModal}
                onClose={() => setShowTransactionsModal(false)}
                width={950}
            >
              <IncomeStatementGlAccountTransactionsModal
                  onClose={() => setShowTransactionsModal(false)}
                  organizationPartyId={selectedAccountingCompanyId!}
                  fromDate={reportData.fromDate}
                  thruDate={reportData.thruDate}
                  selectedMonth={reportData.selectedMonth}
                  glFiscalTypeId={reportData.glFiscalTypeId}
                  glAccountId={selectedGlAccountId}
              />
            </ModalContainer>
        )}
      </>
  );
};

export default IncomeStatement;