import { useMemo, useState } from "react";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { TabContext, TabPanel } from "@mui/lab";
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

import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { useAppSelector } from "../../../../app/store/configureStore";
import { router } from "../../../../app/router/Routes";
import TransactionTotalsForm from "../form/TransactionTotalsForm";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useLazyFetchTransactionTotalsReportQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { StyledTabs } from "../../../../app/components/StyledTabs";
import { StyledTab } from "../../../../app/components/StyledTab";
import { formatNumber } from "../../../../app/util/utils";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import IncomeStatementGlAccountTransactionsModal from "./IncomeStatementGlAccountTransactionsModal";
import { TransactionTotalsExcel, TransactionTotalsRow } from "../report/TransactionTotalsExcel";

type ReportData = {
  glFiscalTypeId: string;
  fromDate?: string;
  thruDate?: string;
  selectedMonth?: number;
};

// Per-tab grid state. The three tabs are independent data sets, so each keeps its own
// sort/filter/page instead of sharing one and jumping when another tab is paged.
type TabState = {
  sort: SortDescriptor[];
  filter?: CompositeFilterDescriptor;
  page: State;
};

const initialTabState: TabState = {
  sort: [{ field: "accountCode", dir: "asc" }],
  filter: undefined,
  page: { skip: 0, take: 20 },
};

const toRows = (list: any[] | undefined): TransactionTotalsRow[] =>
  (list ?? []).map((t) => ({
    glAccountId: t.glAccountId ?? "",
    accountCode: t.accountCode ?? "",
    accountName: t.accountName ?? "",
    openingD: t.openingD ?? 0,
    openingC: t.openingC ?? 0,
    d: t.d ?? 0,
    c: t.c ?? 0,
    balance: t.balance ?? 0,
  }));

const TransactionTotals = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  // The key is "transaction-totals" — this file used to look up "transation-totals", which
  // matches nothing in ar.json/en.json, so every label on this screen silently fell back to its
  // English default even with the UI in Arabic.
  const localizationKey = "accounting.orgGL.reports.transaction-totals.list";

  const { selectedAccountingCompanyName, selectedAccountingCompanyId } = useAppSelector(
    (state) => state.accountingSharedUi
  );

  const [reportData, setReportData] = useState<ReportData>({
    glFiscalTypeId: "ACTUAL",
    fromDate: undefined,
    thruDate: undefined,
    selectedMonth: undefined,
  });
  const [tab, setTab] = useState("1");
  // Which account the user drilled into, and the posted setting of the tab they were on — the
  // Unposted tab has to open an unposted listing or the modal comes up empty.
  const [drill, setDrill] = useState<{ glAccountId: string; isPosted: string } | null>(null);
  const [posted, setPosted] = useState<TabState>(initialTabState);
  const [unposted, setUnposted] = useState<TabState>(initialTabState);
  const [all, setAll] = useState<TabState>(initialTabState);

  const [trigger, { data: report, isFetching, isSuccess, isLoading }] =
    useLazyFetchTransactionTotalsReportQuery();

  const postedRows = useMemo(() => toRows(report?.postedTransactionTotals), [report]);
  const unpostedRows = useMemo(() => toRows(report?.unpostedTransactionTotals), [report]);
  const allRows = useMemo(() => toRows(report?.allTransactionTotals), [report]);

  // Filter -> sort -> page. The grid was previously given `sortable`/`filterable`/`pageable`
  // with a raw array and no handlers, so the controls rendered but did nothing — KendoReact
  // only processes data when you feed it back the processed slice.
  const processTab = (rows: TransactionTotalsRow[], state: TabState) => {
    const filtered = filterBy(rows, state.filter);
    return {
      data: orderBy(filtered, state.sort).slice(
        state.page.skip!,
        state.page.skip! + state.page.take!
      ),
      total: filtered.length,
      sumDebit: filtered.reduce((sum, r) => sum + (r.d ?? 0), 0),
      sumCredit: filtered.reduce((sum, r) => sum + (r.c ?? 0), 0),
    };
  };

  const postedView = useMemo(() => processTab(postedRows, posted), [postedRows, posted]);
  const unpostedView = useMemo(() => processTab(unpostedRows, unposted), [unpostedRows, unposted]);
  const allView = useMemo(() => processTab(allRows, all), [allRows, all]);

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

    setPosted(initialTabState);
    setUnposted(initialTabState);
    setAll(initialTabState);
    setDrill(null);

    trigger({
      organizationPartyId: selectedAccountingCompanyId!,
      glFiscalTypeId,
      fromDate: formattedFromDate,
      thruDate: formattedThruDate,
      selectedMonth: processedMonth,
    });
  };

  // Plain component, not wrapped in useCallback — it calls a hook internally, which is legal in
  // a component but not inside a callback.
  //
  // This used to jump to /accountingTransactionEntries, an OData list filtered by account only:
  // it ignored the report's date range and fiscal type, so the entries shown never reconciled
  // with the row that was clicked. The shared drill-down honours both.
  const makeAccountCodeCell = (isPosted: string) => (props: any) => {
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
        <Button onClick={() => setDrill({ glAccountId: props.dataItem.glAccountId, isPosted })}>
          {props.dataItem.accountCode}
        </Button>
      </td>
    );
  };
  const PostedAccountCell = makeAccountCodeCell("Y");
  const UnpostedAccountCell = makeAccountCodeCell("N");
  const AllAccountCell = makeAccountCodeCell("ALL");

  const renderTab = (
    view: ReturnType<typeof processTab>,
    state: TabState,
    setState: (s: TabState) => void,
    AccountCodeCell: (props: any) => JSX.Element
  ) => (
    <Grid item xs={12}>
      <div className="div-container">
        <KendoGrid
          scrollable="scrollable"
          style={{ height: "65vh", flex: 1 }}
          resizable={true}
          data={view.data}
          sortable={true}
          sort={state.sort}
          onSortChange={(e) => setState({ ...state, sort: e.sort })}
          filterable={true}
          filter={state.filter}
          onFilterChange={(e: GridFilterChangeEvent) =>
            setState({ ...state, filter: e.filter, page: { ...state.page, skip: 0 } })
          }
          pageable={true}
          skip={state.page.skip}
          take={state.page.take}
          total={view.total}
          onPageChange={(e: GridPageChangeEvent) => setState({ ...state, page: e.page })}
        >
          <GridToolbar>
            {/* Totals of the filtered set, not just the visible page, and run through
                formatNumber — these used to render as `$` plus a raw float, e.g.
                $12602390.450000003, with a dollar sign on an EGP ledger. */}
            <Typography variant="body1">
              {getTranslatedLabel(`${localizationKey}.debit`, "DR")}:{" "}
              <Box component="span" fontWeight="bold" color="success.main">
                {formatNumber(view.sumDebit)}
              </Box>
            </Typography>
            <Typography variant="body1">
              {getTranslatedLabel(`${localizationKey}.credit`, "CR")}:{" "}
              <Box component="span" fontWeight="bold" color="error.main">
                {formatNumber(view.sumCredit)}
              </Box>
            </Typography>
          </GridToolbar>
          <Column
            field="accountCode"
            title={getTranslatedLabel(`${localizationKey}.code`, "Account Code")}
            cells={{ data: AccountCodeCell }}
          />
          <Column
            field="accountName"
            title={getTranslatedLabel(`${localizationKey}.name`, "Account Name")}
          />
          <Column
            field="openingD"
            title={getTranslatedLabel(`${localizationKey}.openingD`, "Opening D")}
            format="{0:n2}"
            filter={"numeric"}
          />
          <Column
            field="openingC"
            title={getTranslatedLabel(`${localizationKey}.openingC`, "Opening C")}
            format="{0:n2}"
            filter={"numeric"}
          />
          <Column
            field="d"
            title={getTranslatedLabel(`${localizationKey}.debit`, "DR")}
            format="{0:n2}"
            filter={"numeric"}
          />
          <Column
            field="c"
            title={getTranslatedLabel(`${localizationKey}.credit`, "CR")}
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
            {getTranslatedLabel(`${localizationKey}.title`, "Transaction Totals For: ")}{" "}
            {selectedAccountingCompanyName}
          </Typography>

          <Grid
            item
            xs={12}
            sx={{ margin: 3, display: "flex", alignItems: "center", justifyContent: "space-between" }}
          >
            <TransactionTotalsForm onSubmit={onSubmit} />

            {isSuccess && report && (
              <TransactionTotalsExcel
                companyName={selectedAccountingCompanyName!}
                postedRows={postedRows}
                unpostedRows={unpostedRows}
                allRows={allRows}
                getTranslatedLabel={getTranslatedLabel}
                isFetching={isFetching}
                fromDate={reportData.fromDate}
                thruDate={reportData.thruDate}
              />
            )}
          </Grid>

          {isSuccess && report && (
            <Grid container spacing={1} alignItems={"center"}>
              <TabContext value={tab}>
                <Box sx={{ display: "flex", typography: "body1", ml: 2, mt: 1 }}>
                  <StyledTabs onChange={(_e: any, v: string) => setTab(v)} value={tab}>
                    <StyledTab
                      label={getTranslatedLabel(`${localizationKey}.posted`, "Posted Totals")}
                      value={"1"}
                    />
                    <StyledTab
                      label={getTranslatedLabel(`${localizationKey}.unposted`, "Unposted Totals")}
                      value={"2"}
                    />
                    <StyledTab
                      label={getTranslatedLabel(`${localizationKey}.all`, "All Totals")}
                      value={"3"}
                    />
                  </StyledTabs>
                </Box>
                <TabPanel value={"1"}>
                  {renderTab(postedView, posted, setPosted, PostedAccountCell)}
                </TabPanel>
                <TabPanel value={"2"}>
                  {renderTab(unpostedView, unposted, setUnposted, UnpostedAccountCell)}
                </TabPanel>
                <TabPanel value={"3"}>
                  {renderTab(allView, all, setAll, AllAccountCell)}
                </TabPanel>
              </TabContext>
            </Grid>
          )}

          {(isLoading || isFetching) && (
            <LoadingComponent
              message={getTranslatedLabel(`general.loading-report`, "Loading Report Data...")}
            />
          )}
        </Paper>
      </Grid>

      {drill && (
        <ModalContainer show={true} onClose={() => setDrill(null)} width={1280}>
          <IncomeStatementGlAccountTransactionsModal
            onClose={() => setDrill(null)}
            organizationPartyId={selectedAccountingCompanyId!}
            fromDate={reportData.fromDate}
            thruDate={reportData.thruDate}
            selectedMonth={reportData.selectedMonth}
            glFiscalTypeId={reportData.glFiscalTypeId}
            glAccountId={drill.glAccountId}
            isPosted={drill.isPosted}
          />
        </ModalContainer>
      )}
    </>
  );
};

export default TransactionTotals;
