import { Paper, Typography, Grid, Button } from "@mui/material";
import { useState } from "react";
import { router } from "../../../../app/router/Routes";
import { useAppSelector } from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import ComparativeBalanceSheetForm from "../form/ComparativeBalanceSheetForm";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useFetchComparativeBalanceSheetReportQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridToolbar,
  GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import BalanceSheetGlAccountTransactionsModal from "./BalanceSheetGlAccountTransactionsModal";
import { formatNumber } from "../../../../app/util/utils";

type ReportData = {
  period1GlFiscalTypeId: string;
  period2GlFiscalTypeId: string;
  period1ThruDate?: string;
  period2ThruDate?: string;
};

const ComparativeBalanceSheet = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey =
    "accounting.orgGL.reports.comparative-balance-sheet.list";
  const { selectedAccountingCompanyName, selectedAccountingCompanyId } =
    useAppSelector((state) => state.accountingSharedUi);
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }

  const initialData = {
    period1GlFiscalTypeId: "",
    period2GlFiscalTypeId: "",
    period1ThruDate: undefined,
    period2ThruDate: undefined,
  };
  const [reportData, setReportData] = useState<ReportData>(initialData);
  // Each row carries the same account in two periods, so the drill-down records which column
  // was clicked — Period 1 and Period 2 are different as-of dates.
  const [drill, setDrill] = useState<{ glAccountId: string; period: 1 | 2 } | null>(null);
  const {
    data: comparativeBalanceSheetReportData,
    isFetching,
    isSuccess,
    isLoading,
  } = useFetchComparativeBalanceSheetReportQuery(
    {
      organizationPartyId: selectedAccountingCompanyId!,
      period1GlFiscalTypeId: reportData!.period1GlFiscalTypeId,
      period2GlFiscalTypeId: reportData!.period2GlFiscalTypeId,
      period1ThruDate: reportData!.period1ThruDate,
      period2ThruDate: reportData!.period2ThruDate,
    },
    {
      skip:
        !reportData.period1GlFiscalTypeId || !reportData.period2GlFiscalTypeId,
    }
  );

  const onSubmit = (values: any) => {
    console.log(values);
    const {
      period1ThruDate,
      period2ThruDate,
      period1GlFiscalTypeId,
      period2GlFiscalTypeId,
    } = values;
    setReportData({
      period1GlFiscalTypeId,
      period2GlFiscalTypeId,
      // ISO, not toLocaleDateString() — this value is sent to the API and is now also handed to
      // the transaction drill-down, and a locale-formatted date binds differently per browser.
      period1ThruDate: period1ThruDate
        ? new Date(period1ThruDate).toISOString().split("T")[0]
        : undefined,
      period2ThruDate: period2ThruDate
        ? new Date(period2ThruDate).toISOString().split("T")[0]
        : undefined,
    });
    setDrill(null);
  };

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
            Comparative Balance Sheet For: {selectedAccountingCompanyName}
          </Typography>
          <Grid item xs={12} sx={{ margin: 3 }}>
            <ComparativeBalanceSheetForm onSubmit={onSubmit} />
          </Grid>
          {isSuccess && (
            <>
              <KendoGrid scrollable="scrollable"
                style={{ height: "250px", flex: 1 }}
                resizable={true}
                pageable={true}
                data={
                  comparativeBalanceSheetReportData.assetAccountBalanceList ??
                  []
                }
              >
                <GridToolbar>
                  <Typography variant="body1">Assets</Typography>
                </GridToolbar>
                <Column
                  field="accountCode"
                  title={getTranslatedLabel(
                    `${localizationKey}.code`,
                    "Account Code"
                  )}
                />
                <Column
                  field="accountName"
                  title={getTranslatedLabel(
                    `${localizationKey}.name`,
                    "Account Name"
                  )}
                />
                <Column
                  field="balance1"
                  title={getTranslatedLabel(
                    `${localizationKey}.b1`,
                    "Period 1 Balance"
                  )}
                  cells={{ data: Balance1Cell }}
                />
                <Column
                  field="balance2"
                  title={getTranslatedLabel(
                    `${localizationKey}.b2`,
                    "Period 2 Balance"
                  )}
                  cells={{ data: Balance2Cell }}
                />
              </KendoGrid>
              <KendoGrid scrollable="scrollable"
                style={{ height: "250px", flex: 1 }}
                resizable={true}
                pageable={true}
                data={
                  comparativeBalanceSheetReportData.equityAccountBalanceList ??
                  []
                }
              >
                <GridToolbar>
                  <Typography variant="body1">Equities</Typography>
                </GridToolbar>
                <Column
                  field="accountCode"
                  title={getTranslatedLabel(
                    `${localizationKey}.code`,
                    "Account Code"
                  )}
                />
                <Column
                  field="accountName"
                  title={getTranslatedLabel(
                    `${localizationKey}.name`,
                    "Account Name"
                  )}
                />
                <Column
                  field="balance1"
                  title={getTranslatedLabel(
                    `${localizationKey}.b1`,
                    "Period 1 Balance"
                  )}
                  cells={{ data: Balance1Cell }}
                />
                <Column
                  field="balance2"
                  title={getTranslatedLabel(
                    `${localizationKey}.b2`,
                    "Period 2 Balance"
                  )}
                  cells={{ data: Balance2Cell }}
                />
              </KendoGrid>
              <KendoGrid scrollable="scrollable"
                style={{ height: "250px", flex: 1 }}
                resizable={true}
                pageable={true}
                data={
                  comparativeBalanceSheetReportData.liabilityAccountBalanceList ??
                  []
                }
              >
                <GridToolbar>
                  <Typography variant="body1">Liabilities</Typography>
                </GridToolbar>
                <Column
                  field="accountCode"
                  title={getTranslatedLabel(
                    `${localizationKey}.code`,
                    "Account Code"
                  )}
                />
                <Column
                  field="accountName"
                  title={getTranslatedLabel(
                    `${localizationKey}.name`,
                    "Account Name"
                  )}
                />
                <Column
                  field="balance1"
                  title={getTranslatedLabel(
                    `${localizationKey}.b1`,
                    "Period 1 Balance"
                  )}
                  cells={{ data: Balance1Cell }}
                />
                <Column
                  field="balance2"
                  title={getTranslatedLabel(
                    `${localizationKey}.b2`,
                    "Period 2 Balance"
                  )}
                  cells={{ data: Balance2Cell }}
                />
              </KendoGrid>
              <KendoGrid scrollable="scrollable"
                style={{ height: "250px", flex: 1 }}
                resizable={true}
                pageable={true}
                data={comparativeBalanceSheetReportData.balanceTotalList ?? []}
              >
                <GridToolbar>
                  <Typography variant="body1">Totals</Typography>
                </GridToolbar>
                <Column
                  field="totalName"
                  title={getTranslatedLabel(
                    `${localizationKey}.total`,
                    "Total Name"
                  )}
                />
                <Column
                  field="balance1"
                  title={getTranslatedLabel(
                    `${localizationKey}.b1`,
                    "Period 1 Balance"
                  )}
                  cells={{ data: Balance1Cell }}
                />
                <Column
                  field="balance2"
                  title={getTranslatedLabel(
                    `${localizationKey}.b2`,
                    "Period 2 Balance"
                  )}
                  cells={{ data: Balance2Cell }}
                />
              </KendoGrid>
            </>
          )}
        </Paper>
      </Grid>

      {drill && (
        <ModalContainer show={true} onClose={() => setDrill(null)} width={1280}>
          <BalanceSheetGlAccountTransactionsModal
            onClose={() => setDrill(null)}
            organizationPartyId={selectedAccountingCompanyId!}
            thruDate={
              (drill.period === 1 ? reportData.period1ThruDate : reportData.period2ThruDate) ?? ""
            }
            glFiscalTypeId={
              drill.period === 1
                ? reportData.period1GlFiscalTypeId
                : reportData.period2GlFiscalTypeId
            }
            glAccountId={drill.glAccountId}
          />
        </ModalContainer>
      )}
    </>
  );
};

export default ComparativeBalanceSheet;
