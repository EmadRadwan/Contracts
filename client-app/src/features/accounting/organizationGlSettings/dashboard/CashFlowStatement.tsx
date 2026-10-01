import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { useCallback, useMemo, useState } from "react";
import { toast } from "react-toastify";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridToolbar,
  GridPageChangeEvent,
  GridFilterChangeEvent,
  GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import {
  CompositeFilterDescriptor,
  filterBy,
  orderBy,
  SortDescriptor,
  State,
} from "@progress/kendo-data-query";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";

import { router } from "../../../../app/router/Routes";
import { useAppSelector } from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
import CashFlowStatementForm from "../form/CashFlowStatementForm";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useLazyFetchCashFlowStatementReportQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { formatNumber } from "../../../../app/util/utils";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import IncomeStatementGlAccountTransactionsModal from "./IncomeStatementGlAccountTransactionsModal";
import { CashFlowStatementExcel, CashFlowRow } from "../report/CashFlowStatementExcel";

// The backend (AcctgReportsService.GenerateCashFlowStatement) has always returned the full
// statement — opening balances, the period's movements, the closing snapshot and the totals —
// but this screen only ever rendered the parameter form and dropped the response. It now shows
// the three sections the service produces, with the same grid/Excel treatment as the other
// accounting reports.

type ReportData = {
  glFiscalTypeId: string;
  fromDate?: string;
  thruDate?: string;
  selectedMonth?: number;
};

// One grid's worth of view state. The three sections are independent, so each carries its own
// sort/filter/page rather than sharing one and jumping when another section is paged.
type SectionState = {
  sort: SortDescriptor[];
  filter?: CompositeFilterDescriptor;
  page: State;
};

const initialSectionState: SectionState = {
  sort: [{ field: "accountCode", dir: "asc" }],
  filter: undefined,
  page: { skip: 0, take: 10 },
};

const toRows = (list: any[] | undefined): CashFlowRow[] =>
  (list ?? []).map((t) => ({
    glAccountId: t.glAccountId ?? "",
    accountCode: t.accountCode ?? "",
    accountName: t.accountName ?? "",
    debit: t.d ?? 0,
    credit: t.c ?? 0,
    balance: t.balance ?? 0,
  }));

const CashFlowStatement = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.cash-flow.list";

  const { selectedAccountingCompanyName, selectedAccountingCompanyId } = useAppSelector(
    (state) => state.accountingSharedUi
  );

  const [reportData, setReportData] = useState<ReportData>({
    glFiscalTypeId: "ACTUAL",
    fromDate: undefined,
    thruDate: undefined,
    selectedMonth: undefined,
  });

  const [opening, setOpening] = useState<SectionState>(initialSectionState);
  const [period, setPeriod] = useState<SectionState>(initialSectionState);
  const [closing, setClosing] = useState<SectionState>(initialSectionState);

  const [showTransactionsModal, setShowTransactionsModal] = useState(false);
  const [selectedGlAccountId, setSelectedGlAccountId] = useState<string | null>(null);

  const [trigger, { data: report, isFetching, isSuccess, isLoading, isError, error }] =
    useLazyFetchCashFlowStatementReportQuery();

  // GenerateCashFlowStatement returns null when it cannot find a closed period before fromDate —
  // the statement needs that close as its opening-cash baseline. BaseApiController.HandleResult
  // turns a null value into a 404, so this surfaces as an error rather than an empty success.
  const noClosedPeriod = isError && (error as any)?.status === 404;

  const openingRows = useMemo(() => toRows(report?.openingCashBalanceList), [report]);
  const periodRows = useMemo(() => toRows(report?.periodCashBalanceList), [report]);
  const closingRows = useMemo(() => toRows(report?.closingCashBalanceList), [report]);

  // Filter -> sort -> page, the same pipeline TrialBalance uses, applied per section.
  const processSection = (rows: CashFlowRow[], section: SectionState) => {
    const filtered = filterBy(rows, section.filter);
    return {
      data: orderBy(filtered, section.sort).slice(
        section.page.skip!,
        section.page.skip! + section.page.take!
      ),
      total: filtered.length,
    };
  };

  const openingView = useMemo(() => processSection(openingRows, opening), [openingRows, opening]);
  const periodView = useMemo(() => processSection(periodRows, period), [periodRows, period]);
  const closingView = useMemo(() => processSection(closingRows, closing), [closingRows, closing]);

  const onSubmit = (values: any) => {
    const { fromDate, thruDate, selectedMonth, glFiscalTypeId } = values;

    if (!fromDate && !thruDate && (selectedMonth === undefined || selectedMonth === null)) {
      toast.error(
        getTranslatedLabel(
          "general.date-range-or-month-error",
          "Must select month or date range for report!"
        )
      );
      return;
    }

    // ISO, not toLocaleDateString() — the old code sent a locale-formatted string, which the
    // backend's DateTime binder reads differently depending on the browser's locale.
    const formattedFromDate = fromDate ? new Date(fromDate).toISOString().split("T")[0] : undefined;
    const formattedThruDate = thruDate ? new Date(thruDate).toISOString().split("T")[0] : undefined;
    const processedMonth = selectedMonth ? selectedMonth - 1 : undefined;

    setReportData({
      glFiscalTypeId,
      fromDate: formattedFromDate,
      thruDate: formattedThruDate,
      selectedMonth: processedMonth,
    });

    setOpening(initialSectionState);
    setPeriod(initialSectionState);
    setClosing(initialSectionState);

    trigger({
      organizationPartyId: selectedAccountingCompanyId!,
      glFiscalTypeId,
      fromDate: formattedFromDate,
      thruDate: formattedThruDate,
      selectedMonth: processedMonth,
    });
  };

  const AccountCodeCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
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

  // Section grid. Only the period section is drillable: its rows are the movements between
  // fromDate and thruDate, which is exactly the window the date-range transaction-details
  // endpoint covers. The opening and closing sections are point-in-time snapshots built from a
  // different window, so a drill-down there would list transactions that do not add up to the
  // row the user clicked.
  const renderSection = (
    titleKey: string,
    defaultTitle: string,
    view: { data: CashFlowRow[]; total: number },
    section: SectionState,
    setSection: (s: SectionState) => void,
    sectionTotal: number | undefined,
    drillable: boolean
  ) => (
    <Grid item xs={12}>
      <div className="div-container">
        <KendoGrid
          scrollable="scrollable"
          style={{ height: "340px" }}
          data={view.data}
          resizable={true}
          sortable={true}
          sort={section.sort}
          onSortChange={(e) => setSection({ ...section, sort: e.sort })}
          filterable={true}
          filter={section.filter}
          onFilterChange={(e: GridFilterChangeEvent) =>
            setSection({ ...section, filter: e.filter, page: { ...section.page, skip: 0 } })
          }
          pageable={true}
          skip={section.page.skip}
          take={section.page.take}
          total={view.total}
          onPageChange={(e: GridPageChangeEvent) => setSection({ ...section, page: e.page })}
        >
          <GridToolbar>
            <Typography variant="body1">
              {getTranslatedLabel(titleKey, defaultTitle)}
            </Typography>
            <Typography variant="body1" sx={{ ml: 2 }}>
              {getTranslatedLabel(`${localizationKey}.section-total`, "Section Total")}:{" "}
              <Box component="span" fontWeight="bold">
                {formatNumber(sectionTotal ?? 0)}
              </Box>
            </Typography>
          </GridToolbar>
          <Column
            field="accountCode"
            title={getTranslatedLabel(`${localizationKey}.code`, "Account Code")}
            cells={drillable ? { data: AccountCodeCell } : undefined}
          />
          <Column
            field="accountName"
            title={getTranslatedLabel(`${localizationKey}.name`, "Account Name")}
          />
          <Column
            field="debit"
            title={getTranslatedLabel(`${localizationKey}.debit`, "Debit")}
            format="{0:n2}"
            filter={"numeric"}
          />
          <Column
            field="credit"
            title={getTranslatedLabel(`${localizationKey}.credit`, "Credit")}
            format="{0:n2}"
            filter={"numeric"}
          />
          <Column
            field="balance"
            title={getTranslatedLabel(`${localizationKey}.balance`, "Balance")}
            format="{0:n2}"
            filter={"numeric"}
          />
        </KendoGrid>
      </div>
    </Grid>
  );

  const handleCloseModal = useCallback(() => setShowTransactionsModal(false), []);

  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
    return null;
  }

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
            {getTranslatedLabel(`${localizationKey}.title`, "Cash Flow Statement For: ")}
            {selectedAccountingCompanyName}
          </Typography>

          <Grid
            item
            xs={12}
            sx={{ margin: 3, display: "flex", alignItems: "center", justifyContent: "space-between" }}
          >
            <CashFlowStatementForm onSubmit={onSubmit} />

            {isSuccess && report && (
              <CashFlowStatementExcel
                companyName={selectedAccountingCompanyName!}
                openingRows={openingRows}
                periodRows={periodRows}
                closingRows={closingRows}
                totals={{
                  openingCashBalanceTotal: report.openingCashBalanceTotal,
                  periodCashBalanceTotal: report.periodCashBalanceTotal,
                  closingCashBalanceTotal: report.closingCashBalanceTotal,
                  endingCashBalanceTotal: report.endingCashBalanceTotal,
                }}
                getTranslatedLabel={getTranslatedLabel}
                isFetching={isFetching}
                fromDate={reportData.fromDate}
                thruDate={reportData.thruDate}
              />
            )}
          </Grid>

          {/* Without this the screen would just look empty with no explanation. */}
          {noClosedPeriod && (
            <Grid item xs={12} sx={{ mx: 3, mb: 3 }}>
              <Typography variant="body1" color="error">
                {getTranslatedLabel(
                  `${localizationKey}.no-closed-period`,
                  "No closed accounting period was found before the selected start date, so an opening cash balance cannot be established. Close the prior period, or choose a later start date."
                )}
              </Typography>
            </Grid>
          )}

          {isSuccess && report && (
            <>
              {renderSection(
                `${localizationKey}.opening`,
                "Opening Cash Balances",
                openingView,
                opening,
                setOpening,
                report.openingCashBalanceTotal,
                false
              )}
              {renderSection(
                `${localizationKey}.period`,
                "Cash Movements For The Period",
                periodView,
                period,
                setPeriod,
                report.periodCashBalanceTotal,
                true
              )}
              {renderSection(
                `${localizationKey}.closing`,
                "Closing Cash Balances",
                closingView,
                closing,
                setClosing,
                report.closingCashBalanceTotal,
                false
              )}

              <Grid item xs={12} sx={{ mt: 4, mb: 3 }}>
                <Typography variant="h5" ml={2}>
                  {getTranslatedLabel(`${localizationKey}.totals`, "Totals")}
                </Typography>
                <Grid item xs={6} ml={2}>
                  <Box
                    sx={{
                      border: "1px solid",
                      borderColor: "grey.400",
                      padding: 2,
                      borderRadius: "0.5rem",
                    }}
                  >
                    <Grid container spacing={1}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {getTranslatedLabel(`${localizationKey}.opening-total`, "Opening Cash Balance")}
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography align="right">{formatNumber(report.openingCashBalanceTotal)}</Typography>
                      </Grid>

                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {getTranslatedLabel(`${localizationKey}.period-total`, "Net Cash Movement For The Period")}
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography
                          align="right"
                          color={(report.periodCashBalanceTotal ?? 0) >= 0 ? "success.main" : "error.main"}
                        >
                          {formatNumber(report.periodCashBalanceTotal)}
                        </Typography>
                      </Grid>

                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {getTranslatedLabel(`${localizationKey}.closing-total`, "Closing Cash Balance")}
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography align="right">{formatNumber(report.closingCashBalanceTotal)}</Typography>
                      </Grid>

                      <Grid item xs={6}>
                        <Typography variant="h6" fontWeight="bold">
                          {getTranslatedLabel(`${localizationKey}.ending-total`, "Ending Cash Balance")}
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography
                          variant="h6"
                          fontWeight="bold"
                          align="right"
                          color={(report.endingCashBalanceTotal ?? 0) >= 0 ? "success.main" : "error.main"}
                        >
                          {formatNumber(report.endingCashBalanceTotal)}
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
              message={getTranslatedLabel(`general.loading-report`, "Loading Report Data...")}
            />
          )}
        </Paper>
      </Grid>

      {/* Reuses the date-range GL drill-down. Despite its name the handler behind it
          (GetIncomeStatementGlAccountTransactionDetails) is report-agnostic — it filters by
          organisation, account, fiscal type and a from/thru window, which is what a cash account's
          period movements need. Renaming it is a separate cleanup. */}
      {showTransactionsModal && selectedGlAccountId && (
        <ModalContainer show={showTransactionsModal} onClose={handleCloseModal} width={1280}>
          <IncomeStatementGlAccountTransactionsModal
            onClose={handleCloseModal}
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

export default CashFlowStatement;
