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
      period1ThruDate: period1ThruDate
        ? new Date(period1ThruDate).toLocaleDateString()
        : undefined,
      period2ThruDate: period2ThruDate
        ? new Date(period2ThruDate).toLocaleDateString()
        : undefined,
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
        {...{
          [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
        }}
        {...navigationAttributes}
      >
        <Button
          onClick={() => {
            router.navigate("/accountingTransactionEntries", {
              state: { glAccountId: props.dataItem.glAccountId },
            });
          }}
        >
          {props.dataItem.glAccountId}
        </Button>
      </td>
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
            Comparative Balance Sheet For: {selectedAccountingCompanyName}
          </Typography>
          <Grid item xs={12} sx={{ margin: 3 }}>
            <ComparativeBalanceSheetForm onSubmit={onSubmit} />
          </Grid>
          {isSuccess && (
            <>
              <KendoGrid
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
                  cell={AccountCodeCell}
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
                  format="{0:c2}"
                />
                <Column
                  field="balance2"
                  title={getTranslatedLabel(
                    `${localizationKey}.b2`,
                    "Period 2 Balance"
                  )}
                  format="{0:c2}"
                />
              </KendoGrid>
              <KendoGrid
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
                  cell={AccountCodeCell}
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
                  format="{0:c2}"
                />
                <Column
                  field="balance2"
                  title={getTranslatedLabel(
                    `${localizationKey}.b2`,
                    "Period 2 Balance"
                  )}
                  format="{0:c2}"
                />
              </KendoGrid>
              <KendoGrid
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
                  cell={AccountCodeCell}
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
                  format="{0:c2}"
                />
                <Column
                  field="balance2"
                  title={getTranslatedLabel(
                    `${localizationKey}.b2`,
                    "Period 2 Balance"
                  )}
                  format="{0:c2}"
                />
              </KendoGrid>
              <KendoGrid
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
                  format="{0:c2}"
                />
                <Column
                  field="balance2"
                  title={getTranslatedLabel(
                    `${localizationKey}.b2`,
                    "Period 2 Balance"
                  )}
                  format="{0:c2}"
                />
              </KendoGrid>
            </>
          )}
        </Paper>
      </Grid>
    </>
  );
};

export default ComparativeBalanceSheet;
