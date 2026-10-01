import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { useCallback, useEffect, useMemo, useState } from "react";
import { toast } from "react-toastify";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridToolbar,
  GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";

import { router } from "../../../../app/router/Routes";
import { useAppSelector } from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
import GlAccountTrialBalanceForm from "../form/GlAccountTrialBalanceForm";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useLazyFetchGlAccountTrialBalanceReportQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { formatNumber, handleDatesArray } from "../../../../app/util/utils";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import IncomeStatementGlAccountTransactionsModal from "./IncomeStatementGlAccountTransactionsModal";
import {
  GlAccountTrialBalanceExcel,
  GlAccountTrialBalanceMonthRow,
} from "../report/GlAccountTrialBalanceExcel";

// GenerateGlAccountTrialBalance walks the selected fiscal year one month at a time and returns a
// row per month — debit/credit totals, their difference, year-to-date debit/credit, the month's
// balance and the running balance carried from the opening balance. This screen used to render
// the parameter form and drop that response; it now shows the breakdown, with each month
// expandable into the transactions the backend already ships inside the row.

type ReportData = {
  timePeriodId: string;
  glAccountId: string;
  isPosted: string;
};

const LAST_PARAMS_KEY = "lastGlAccountTrialBalanceParams";

const monthLabel = (from?: string) => {
  if (!from) return "";
  const d = new Date(from);
  if (Number.isNaN(d.getTime())) return "";
  // Month + year reads correctly in both locales and matches the backend's monthly walk.
  return d.toLocaleDateString(undefined, { month: "short", year: "numeric" });
};

const GlAccountTrialBalance = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.gl-trial-balance";

  const { selectedAccountingCompanyName, selectedAccountingCompanyId } = useAppSelector(
    (state) => state.accountingSharedUi
  );

  const [reportData, setReportData] = useState<ReportData | null>(null);
  // KendoReact v16 drives detail rows with a controlled { [dataItemKey]: boolean } map plus
  // onDetailExpandChange — the v6 expandField / onExpandChange pair is gone.
  const [detailExpand, setDetailExpand] = useState<Record<string, boolean>>({});
  // The inline detail rows are a quick look at the month's postings; the shared drill-down adds
  // party, description, running balance, reversal colouring and the Excel/PDF exports, scoped to
  // that same month.
  const [drillMonth, setDrillMonth] = useState<{ fromDate: string; thruDate: string } | null>(null);

  const [trigger, { data: report, isFetching, isSuccess, isLoading }] =
    useLazyFetchGlAccountTrialBalanceReportQuery();

  const runReport = useCallback(
    (values: ReportData) => {
      if (!selectedAccountingCompanyId) return;
      setReportData(values);
      setDetailExpand({});
      setDrillMonth(null);
      try {
        localStorage.setItem(LAST_PARAMS_KEY, JSON.stringify(values));
      } catch {
        /* storage unavailable — the report still runs, it just won't be restored next visit */
      }
      trigger({
        organizationPartyId: selectedAccountingCompanyId,
        glAccountId: values.glAccountId,
        timePeriodId: values.timePeriodId,
        isPosted: values.isPosted,
      });
    },
    [selectedAccountingCompanyId, trigger]
  );

  // Re-run the last account/period on return, the way both trial balance reports do.
  useEffect(() => {
    if (!selectedAccountingCompanyId || reportData) return;
    try {
      const raw = localStorage.getItem(LAST_PARAMS_KEY);
      if (!raw) return;
      const saved = JSON.parse(raw) as ReportData;
      if (saved?.glAccountId && saved?.timePeriodId) runReport(saved);
    } catch {
      /* unreadable or malformed — start clean */
    }
  }, [selectedAccountingCompanyId, reportData, runReport]);

  const onSubmit = (values: any) => {
    const { timePeriodId, isPosted, glAccountId } = values;

    // The form validates the account but not the period, and the report is meaningless without
    // one — the backend needs it to know which fiscal year to walk.
    if (!glAccountId || !timePeriodId) {
      toast.error(
        getTranslatedLabel(
          `${localizationKey}.account-and-period-required`,
          "Select both a GL account and a time period."
        )
      );
      return;
    }

    runReport({
      glAccountId,
      timePeriodId,
      isPosted: isPosted ?? "ALL",
    });
  };

  // The backend stamps each row with the month it covers (PeriodFromDate/PeriodThruDate), so the
  // label does not depend on the row's position in the list.
  const monthRows = useMemo(
    () =>
      (report?.glAcctgTrialBalanceList ?? []).map((m: any, index: number) => ({
        // Stable string key for the detail-expansion map; the month window is unique per row.
        monthKey: m.periodFromDate ? String(m.periodFromDate) : `month-${index}`,
        periodLabel: monthLabel(m.periodFromDate),
        periodFromDate: m.periodFromDate,
        periodThruDate: m.periodThruDate,
        debitTotal: m.debitTotal ?? 0,
        creditTotal: m.creditTotal ?? 0,
        debitCreditDifference: m.debitCreditDifference ?? 0,
        totalOfYearToDateDebit: m.totalOfYearToDateDebit ?? 0,
        totalOfYearToDateCredit: m.totalOfYearToDateCredit ?? 0,
        balance: m.balance ?? 0,
        balanceOfTheAcctgForYear: m.balanceOfTheAcctgForYear ?? 0,
        // Kendo's {0:dd/MM/yyyy} column format needs real Dates; the API sends ISO strings.
        transactions: handleDatesArray(m.acctgTransAndEntries ?? []),
        transactionCount: (m.acctgTransAndEntries ?? []).length,
      })),
    [report]
  );

  const excelRows: GlAccountTrialBalanceMonthRow[] = useMemo(
    () =>
      monthRows.map((r) => ({
        periodLabel: r.periodLabel,
        debitTotal: r.debitTotal,
        creditTotal: r.creditTotal,
        debitCreditDifference: r.debitCreditDifference,
        totalOfYearToDateDebit: r.totalOfYearToDateDebit,
        totalOfYearToDateCredit: r.totalOfYearToDateCredit,
        balance: r.balance,
        balanceOfTheAcctgForYear: r.balanceOfTheAcctgForYear,
        transactionCount: r.transactionCount,
      })),
    [monthRows]
  );

  const yearTotals = useMemo(() => {
    const debit = monthRows.reduce((sum, r) => sum + r.debitTotal, 0);
    const credit = monthRows.reduce((sum, r) => sum + r.creditTotal, 0);
    const last = monthRows[monthRows.length - 1];
    return {
      debit,
      credit,
      // The running-balance column already accumulates, so the closing figure is the last
      // month's value, not a sum of the column.
      closing: last ? last.balanceOfTheAcctgForYear : (report?.openingBalance ?? 0),
    };
  }, [monthRows, report]);

  const MonthCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    const item = props.dataItem;
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
        <Button
          disabled={!item.periodFromDate || !item.periodThruDate}
          onClick={() =>
            setDrillMonth({
              fromDate: String(item.periodFromDate).split("T")[0],
              thruDate: String(item.periodThruDate).split("T")[0],
            })
          }
        >
          {item.periodLabel}
        </Button>
      </td>
    );
  };

  // Each month already carries its transactions, so the detail row is a local expansion rather
  // than another round trip.
  const DetailComponent = (props: any) => {
    const rows = props.dataItem.transactions ?? [];
    if (rows.length === 0) {
      return (
        <Typography variant="body2" sx={{ p: 2 }}>
          {getTranslatedLabel(`${localizationKey}.noTransactions`, "No transactions in this month.")}
        </Typography>
      );
    }
    return (
      <KendoGrid scrollable="scrollable" data={rows} style={{ maxHeight: "280px" }} resizable={true}>
        <Column
          field="acctgTransId"
          title={getTranslatedLabel(`${localizationKey}.transId`, "Trans ID")}
          width={130}
        />
        <Column
          field="transactionDate"
          title={getTranslatedLabel(`${localizationKey}.transDate`, "Date")}
          format="{0:dd/MM/yyyy}"
          width={120}
        />
        <Column
          field="debitCreditFlag"
          title={getTranslatedLabel(`${localizationKey}.debitCredit`, "D/C")}
          width={70}
        />
        <Column
          field="amount"
          title={getTranslatedLabel(`${localizationKey}.amount`, "Amount")}
          format="{0:n2}"
          width={140}
        />
        <Column
          field="glFiscalTypeId"
          title={getTranslatedLabel(`${localizationKey}.fiscalType`, "Fiscal Type")}
          width={120}
        />
        <Column
          field="isPosted"
          title={getTranslatedLabel(`${localizationKey}.isPosted`, "Is Posted")}
          width={100}
        />
      </KendoGrid>
    );
  };

  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
    return null;
  }

  const account = report?.glAccount;
  const period = report?.currentTimePeriod;
  const periodLabel = period
    ? `${period.periodName ?? ""} ${
        period.fromDate ? new Date(period.fromDate).toLocaleDateString("en-GB") : ""
      } - ${period.thruDate ? new Date(period.thruDate).toLocaleDateString("en-GB") : ""}`.trim()
    : "";

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
            {getTranslatedLabel(`${localizationKey}.title`, "Gl Account Trial Balance For: ")}
            {selectedAccountingCompanyName}
          </Typography>

          <Grid item xs={12} sx={{ margin: 3 }}>
            <GlAccountTrialBalanceForm onSubmit={onSubmit} />
          </Grid>

          {isSuccess && report && (
            <>
              <Grid item xs={12} sx={{ mx: 3, mb: 2 }}>
                <Box
                  sx={{
                    p: 2,
                    border: "1px solid #e0e0e0",
                    borderRadius: 2,
                    backgroundColor: "#f9f9f9",
                  }}
                >
                  <Typography variant="body1">
                    {getTranslatedLabel(`${localizationKey}.account`, "Account: ")}{" "}
                    {account?.accountCode} — {account?.accountNameArabic ?? account?.accountName}
                  </Typography>
                  <Typography variant="body1">
                    {getTranslatedLabel(`${localizationKey}.period`, "Period: ")} {periodLabel}
                  </Typography>
                  <Typography variant="body1">
                    {getTranslatedLabel(`${localizationKey}.accountSide`, "Account Side: ")}{" "}
                    {report.isDebitAccount
                      ? getTranslatedLabel(`${localizationKey}.debitSide`, "Debit")
                      : getTranslatedLabel(`${localizationKey}.creditSide`, "Credit")}
                  </Typography>
                  <Typography variant="body1" sx={{ fontWeight: "bold", mt: 1 }}>
                    {getTranslatedLabel(`${localizationKey}.openingBalance`, "Opening Balance: ")}{" "}
                    {formatNumber(report.openingBalance)}
                  </Typography>
                </Box>
              </Grid>

              <Grid item xs={12}>
                <div className="div-container">
                  <KendoGrid
                    scrollable="scrollable"
                    className="main-grid"
                    data={monthRows}
                    resizable={true}
                    dataItemKey="monthKey"
                    detail={DetailComponent}
                    detailExpand={detailExpand}
                    onDetailExpandChange={(e) => setDetailExpand(e.detailExpand)}
                  >
                    <GridToolbar>
                      <Typography variant="body1">
                        {getTranslatedLabel(`${localizationKey}.yearDebit`, "Year Debit: ")}
                        <Box component="span" fontWeight="bold" color="success.main">
                          {formatNumber(yearTotals.debit)}
                        </Box>
                      </Typography>
                      <Typography variant="body1">
                        {getTranslatedLabel(`${localizationKey}.yearCredit`, "Year Credit: ")}
                        <Box component="span" fontWeight="bold" color="error.main">
                          {formatNumber(yearTotals.credit)}
                        </Box>
                      </Typography>
                      <Typography variant="body1">
                        {getTranslatedLabel(`${localizationKey}.closingBalance`, "Closing Balance: ")}
                        <Box component="span" fontWeight="bold">
                          {formatNumber(yearTotals.closing)}
                        </Box>
                      </Typography>

                      <GlAccountTrialBalanceExcel
                        companyName={selectedAccountingCompanyName ?? ""}
                        accountCode={account?.accountCode ?? ""}
                        accountName={account?.accountNameArabic ?? account?.accountName ?? ""}
                        periodLabel={periodLabel}
                        openingBalance={report.openingBalance ?? 0}
                        rows={excelRows}
                        getTranslatedLabel={getTranslatedLabel}
                        isFetching={isFetching}
                      />
                    </GridToolbar>

                    <Column
                      field="periodLabel"
                      title={getTranslatedLabel(`${localizationKey}.month`, "Month")}
                      width="140px"
                      cells={{ data: MonthCell }}
                    />
                    <Column
                      field="debitTotal"
                      title={getTranslatedLabel(`${localizationKey}.debit`, "Debit")}
                      format="{0:n2}"
                    />
                    <Column
                      field="creditTotal"
                      title={getTranslatedLabel(`${localizationKey}.credit`, "Credit")}
                      format="{0:n2}"
                    />
                    <Column
                      field="debitCreditDifference"
                      title={getTranslatedLabel(`${localizationKey}.difference`, "Difference")}
                      format="{0:n2}"
                    />
                    <Column
                      field="totalOfYearToDateDebit"
                      title={getTranslatedLabel(`${localizationKey}.ytdDebit`, "YTD Debit")}
                      format="{0:n2}"
                    />
                    <Column
                      field="totalOfYearToDateCredit"
                      title={getTranslatedLabel(`${localizationKey}.ytdCredit`, "YTD Credit")}
                      format="{0:n2}"
                    />
                    <Column
                      field="balance"
                      title={getTranslatedLabel(`${localizationKey}.monthBalance`, "Month Balance")}
                      format="{0:n2}"
                    />
                    <Column
                      field="balanceOfTheAcctgForYear"
                      title={getTranslatedLabel(`${localizationKey}.runningBalance`, "Running Balance")}
                      format="{0:n2}"
                    />
                    <Column
                      field="transactionCount"
                      title={getTranslatedLabel(`${localizationKey}.transactions`, "Transactions")}
                      width="120px"
                    />
                  </KendoGrid>
                </div>
              </Grid>
            </>
          )}

          {(isFetching || isLoading) && (
            <LoadingComponent
              message={getTranslatedLabel("general.loading-report", "Loading Report Data...")}
            />
          )}
        </Paper>
      </Grid>

      {drillMonth && reportData && (
        <ModalContainer show={true} onClose={() => setDrillMonth(null)} width={1280}>
          <IncomeStatementGlAccountTransactionsModal
            onClose={() => setDrillMonth(null)}
            organizationPartyId={selectedAccountingCompanyId!}
            fromDate={drillMonth.fromDate}
            thruDate={drillMonth.thruDate}
            glFiscalTypeId="ACTUAL"
            glAccountId={reportData.glAccountId}
            isPosted={reportData.isPosted}
          />
        </ModalContainer>
      )}
    </>
  );
};

export default GlAccountTrialBalance;
