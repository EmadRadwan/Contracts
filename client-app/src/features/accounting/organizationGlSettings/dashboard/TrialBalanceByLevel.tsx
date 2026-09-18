import {Box, Button, FormControl, Grid, InputLabel, MenuItem, Paper, Select, Typography} from "@mui/material";
import {
  useAppDispatch,
  useAppSelector,
} from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import TrialBalanceCustomTimePeriodForm from "../form/TrialBalanceCustomTimePeriodForm";
import { useLazyFetchTrialBalanceByLevelReportQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import { setSeletedCustomTimePeriodId } from "../../slice/accountingSharedUiSlice";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridToolbar,
  GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import {useEffect, useState, useCallback, useMemo} from "react";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import GlAccountTransactionsModal from "./GlAccountTransactionsModal";
import { TrialBalanceByLevelExcel } from "../report/TrialBalanceByLevelExcel";
import { formatNumber } from "../../../../app/util/utils";

// COA-hierarchy-level version of the flat Trial Balance (see TrialBalance.tsx). Rows include
// both leaf accounts (isLeaf = true, drillable) and their ancestor roll-up totals (isLeaf =
// false, bold), indented by `level` so the Chart of Accounts structure reads top-to-bottom.
const TrialBalanceByLevel = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.trial-balance-by-level";
  const [showTransactionsModal, setShowTransactionsModal] = useState(false);
  const [selectedGlAccountId, setSelectedGlAccountId] = useState<string | null>(null);
  const [selectedLevel, setSelectedLevel] = useState<number | "all">("all");

  const {
    seletedCustomTimePeriodId,
  } = useAppSelector((state) => state.accountingSharedUi);
  const dispatch = useAppDispatch();
  const {user} = useAppSelector((state) => state.account);
  const companyId = user?.organizationPartyId || "";
  const organizationPartyName = user?.organizationPartyName || "";

  const [runTrialBalanceByLevel, { data, isLoading, isFetching, isSuccess }] =
    useLazyFetchTrialBalanceByLevelReportQuery();

  const handleSelectTimePeriod = useCallback((value: any) => {
    const customTimePeriodId = value.customTimePeriodId;

    dispatch(setSeletedCustomTimePeriodId(customTimePeriodId));

    if (!customTimePeriodId || !companyId) return;

    localStorage.setItem("lastTrialBalanceByLevelTimePeriodId", customTimePeriodId);

    setSelectedLevel("all");
    runTrialBalanceByLevel({ customTimePeriodId, organizationPartyId: companyId });
  }, [companyId, dispatch, runTrialBalanceByLevel]);

  // Levels present in the current report, so the dropdown only ever offers real choices.
  const availableLevels = useMemo(() => {
    const levels = new Set<number>((data?.nodes ?? []).map((n: any) => n.level));
    return Array.from(levels).sort((a, b) => a - b);
  }, [data?.nodes]);

  // The report already fetches the whole rolled-up hierarchy in one call, so narrowing to a
  // single level (e.g. "just show me level 2") is a client-side filter, not a new backend query.
  const filteredNodes = useMemo(() => {
    const nodes = data?.nodes ?? [];
    return selectedLevel === "all" ? nodes : nodes.filter((n: any) => n.level === selectedLevel);
  }, [data?.nodes, selectedLevel]);

  useEffect(() => {
    if (companyId && !seletedCustomTimePeriodId && !isSuccess && !isLoading && !isFetching) {
      const lastPeriodId = localStorage.getItem("lastTrialBalanceByLevelTimePeriodId");
      if (lastPeriodId) {
        handleSelectTimePeriod({ customTimePeriodId: lastPeriodId });
      }
    }
  }, [companyId, seletedCustomTimePeriodId, isSuccess, isLoading, isFetching, handleSelectTimePeriod]);

  useEffect(() => {
    return () => {
      dispatch(setSeletedCustomTimePeriodId(null));
    };
  }, [dispatch]);

  const AccountCodeCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    const node = props.dataItem;
    const indent = (node.level ?? 0) * 20;

    if (!node.isLeaf) {
      return (
        <td
          className={props.className}
          style={{ ...props.style, paddingInlineStart: indent, fontWeight: "bold" }}
          colSpan={props.colSpan}
          role={'gridcell'}
          aria-colindex={props.ariaColumnIndex}
          aria-selected={props.isSelected}
          {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
          {...navigationAttributes}
        >
          {node.accountCode}
        </td>
      );
    }

    return (
      <td
        className={props.className}
        style={{ ...props.style, paddingInlineStart: indent }}
        colSpan={props.colSpan}
        role={'gridcell'}
        aria-colindex={props.ariaColumnIndex}
        aria-selected={props.isSelected}
        {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
        {...navigationAttributes}
      >
        <Button
          onClick={() => {
            setSelectedGlAccountId(node.glAccountId);
            setShowTransactionsModal(true);
          }}
        >
          {node.accountCode}
        </Button>
      </td>
    );
  };

  const rowRender = (trElement: any, props: any) => {
    const node = props.dataItem;
    const trProps: any = {
      style: node.isLeaf ? {} : { fontWeight: "bold", backgroundColor: "rgba(0,0,0,0.04)" },
    };
    return { ...trElement, props: { ...trElement.props, ...trProps } };
  };

  const excelRows = useMemo(() => filteredNodes.map((n: any) => ({
    accountCode: n.accountCode ?? '',
    accountName: n.accountName ?? '',
    level: n.level ?? 0,
    isLeaf: !!n.isLeaf,
    openingBalance: n.openingBalance ?? 0,
    postedDebits: n.postedDebits ?? 0,
    postedCredits: n.postedCredits ?? 0,
    endingBalance: n.endingBalance ?? 0,
  })), [filteredNodes]);

  // PDF export: a plain HTML table printed via the browser's own renderer (window.print / "Save
  // as PDF"), not KendoReact's PDFExport -- that component draws text on a canvas with no Arabic
  // shaping/bidi support and garbles Arabic labels (documented, hit and fixed once already on the
  // Project Report PDF export). The browser's native print path renders real text, so Arabic
  // account names come out correctly.
  const handlePrintPdf = useCallback(() => {
    window.print();
  }, []);

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
            {getTranslatedLabel(`${localizationKey}.title`, "Trial Balance By Level For: ")} {organizationPartyName}
          </Typography>
          <Grid item xs={12} sx={{ margin: 3 }}>
            <TrialBalanceCustomTimePeriodForm
              onSubmit={handleSelectTimePeriod}
              initialCustomTimePeriodId={seletedCustomTimePeriodId ?? undefined}
            />
          </Grid>
          {isSuccess && (
            <Grid container>
              <Grid item xs={12} sx={{ marginInlineStart: 3, marginBottom: 2 }}>
                <FormControl size="small" sx={{ minWidth: 180 }}>
                  <InputLabel id="tbbl-level-filter-label">
                    {getTranslatedLabel(`${localizationKey}.levelFilter`, "Level")}
                  </InputLabel>
                  <Select
                    labelId="tbbl-level-filter-label"
                    label={getTranslatedLabel(`${localizationKey}.levelFilter`, "Level")}
                    value={selectedLevel}
                    onChange={(e) => setSelectedLevel(e.target.value === "all" ? "all" : Number(e.target.value))}
                  >
                    <MenuItem value="all">{getTranslatedLabel(`${localizationKey}.allLevels`, "All Levels")}</MenuItem>
                    {availableLevels.map((lvl) => (
                      <MenuItem key={lvl} value={lvl}>{lvl}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <div className="div-container">
                <KendoGrid scrollable="scrollable"
                    className="main-grid"
                    data={filteredNodes}
                    resizable={true}
                    rowRender={rowRender}
                >
                  <GridToolbar>
                    <Typography variant="body1">
                      {getTranslatedLabel(`${localizationKey}.debits`, "Debits total: ")}
                      <Box component="span" fontWeight="bold" color="success.main">
                        {formatNumber(data.postedDebitsTotal)}
                      </Box>
                    </Typography>

                    <Typography variant="body1">
                      {getTranslatedLabel(`${localizationKey}.credits`, "Credits total: ")}
                      <Box component="span" fontWeight="bold" color="error.main">
                        {formatNumber(data.postedCreditsTotal)}
                      </Box>
                    </Typography>

                    <TrialBalanceByLevelExcel
                        companyName={organizationPartyName ?? ''}
                        rows={excelRows}
                        totals={{
                          postedDebitsTotal: data.postedDebitsTotal ?? 0,
                          postedCreditsTotal: data.postedCreditsTotal ?? 0,
                        }}
                        getTranslatedLabel={getTranslatedLabel}
                        isFetching={isFetching}
                    />
                    <Button variant="outlined" color="primary" onClick={handlePrintPdf} disabled={!filteredNodes.length}>
                      {getTranslatedLabel(`${localizationKey}.exportPdf`, "Export PDF")}
                    </Button>
                  </GridToolbar>
                  <Column field="accountCode" title={getTranslatedLabel(`${localizationKey}.accountCode`, "Account Code")} cells={{ data: AccountCodeCell }} />
                  <Column field="accountName" title={getTranslatedLabel(`${localizationKey}.accountName`, "Account Name")} />
                  <Column field="level" title={getTranslatedLabel(`${localizationKey}.level`, "Level")} width="80px" />
                  <Column field="openingBalance" title={getTranslatedLabel(`${localizationKey}.openingBalance`, "Opening Balance")} format="{0:n2}" />
                  <Column field="postedDebits" title={getTranslatedLabel(`${localizationKey}.postedDebits`, "Debit")} format="{0:n2}" />
                  <Column field="postedCredits" title={getTranslatedLabel(`${localizationKey}.postedCredits`, "Credit")} format="{0:n2}" />
                  <Column field="endingBalance" title={getTranslatedLabel(`${localizationKey}.endingBalance`, "Ending Balance")} format="{0:n2}" />
                </KendoGrid>
              </div>

              {/* Print-only view: plain HTML table rendered by the browser (not Kendo's canvas
                  PDFExport), so Arabic account names print/PDF correctly. Kept off-screen on
                  normal view; @media print swaps visibility so only this prints. */}
              <style>{`
                @media print {
                  body * { visibility: hidden; }
                  #tbbl-print-area, #tbbl-print-area * { visibility: visible; }
                  #tbbl-print-area { position: absolute !important; left: 0 !important; top: 0 !important; width: 100% !important; }
                }
              `}</style>
              <Box id="tbbl-print-area" dir="rtl" sx={{ position: "absolute", left: "-10000px", top: 0, fontFamily: "Arial, sans-serif" }}>
                <h2>{getTranslatedLabel(`${localizationKey}.title`, "Trial Balance By Level For: ")} {organizationPartyName}</h2>
                <table style={{ width: "100%", borderCollapse: "collapse" }}>
                  <thead>
                    <tr>
                      <th style={{ border: "1px solid #999", padding: 4 }}>{getTranslatedLabel(`${localizationKey}.accountCode`, "Account Code")}</th>
                      <th style={{ border: "1px solid #999", padding: 4 }}>{getTranslatedLabel(`${localizationKey}.accountName`, "Account Name")}</th>
                      <th style={{ border: "1px solid #999", padding: 4 }}>{getTranslatedLabel(`${localizationKey}.level`, "Level")}</th>
                      <th style={{ border: "1px solid #999", padding: 4 }}>{getTranslatedLabel(`${localizationKey}.openingBalance`, "Opening Balance")}</th>
                      <th style={{ border: "1px solid #999", padding: 4 }}>{getTranslatedLabel(`${localizationKey}.postedDebits`, "Debit")}</th>
                      <th style={{ border: "1px solid #999", padding: 4 }}>{getTranslatedLabel(`${localizationKey}.postedCredits`, "Credit")}</th>
                      <th style={{ border: "1px solid #999", padding: 4 }}>{getTranslatedLabel(`${localizationKey}.endingBalance`, "Ending Balance")}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {excelRows.map((r, i) => (
                      <tr key={i} style={{ fontWeight: r.isLeaf ? "normal" : "bold", background: r.isLeaf ? "transparent" : "#f5f5f5" }}>
                        <td style={{ border: "1px solid #ccc", padding: 4, paddingInlineStart: r.level * 12 }}>{r.accountCode}</td>
                        <td style={{ border: "1px solid #ccc", padding: 4 }}>{r.accountName}</td>
                        <td style={{ border: "1px solid #ccc", padding: 4, textAlign: "center" }}>{r.level}</td>
                        <td style={{ border: "1px solid #ccc", padding: 4 }}>{formatNumber(r.openingBalance)}</td>
                        <td style={{ border: "1px solid #ccc", padding: 4 }}>{formatNumber(r.postedDebits)}</td>
                        <td style={{ border: "1px solid #ccc", padding: 4 }}>{formatNumber(r.postedCredits)}</td>
                        <td style={{ border: "1px solid #ccc", padding: 4 }}>{formatNumber(r.endingBalance)}</td>
                      </tr>
                    ))}
                    <tr style={{ fontWeight: "bold" }}>
                      <td colSpan={4} style={{ border: "1px solid #ccc", padding: 4 }}>
                        {getTranslatedLabel(`${localizationKey}.totals`, "Totals")}
                      </td>
                      <td style={{ border: "1px solid #ccc", padding: 4 }}>{formatNumber(data.postedDebitsTotal ?? 0)}</td>
                      <td style={{ border: "1px solid #ccc", padding: 4 }}>{formatNumber(data.postedCreditsTotal ?? 0)}</td>
                      <td style={{ border: "1px solid #ccc", padding: 4 }}></td>
                    </tr>
                  </tbody>
                </table>
              </Box>
            </Grid>
          )}
          {(isFetching || isLoading) && <LoadingComponent message={getTranslatedLabel(`general.loading-report`, "Loading Report Data...")} />}
        </Paper>
      </Grid>
      {showTransactionsModal && (
          <ModalContainer
              show={showTransactionsModal}
              onClose={() => setShowTransactionsModal(false)}
              width={950}
          >
            <GlAccountTransactionsModal
                onClose={() => setShowTransactionsModal(false)}
                organizationPartyId={companyId!}
                customTimePeriodId={seletedCustomTimePeriodId!}
                glAccountId={selectedGlAccountId!}
            />
          </ModalContainer>
      )}
    </>
  );
};

export default TrialBalanceByLevel;
