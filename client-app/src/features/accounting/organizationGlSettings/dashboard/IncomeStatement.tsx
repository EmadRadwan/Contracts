import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import IncomeStatementForm from "../form/IncomeStatementForm";
import { router } from "../../../../app/router/Routes";
import { useAppDispatch, useAppSelector } from "../../../../app/store/configureStore";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
import { useEffect, useState } from "react";
import { toast } from "react-toastify";
import { useFetchIncomeStatementReportQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridToolbar,
  GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import { formatCurrency } from "../../../../app/util/utils";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { setSeletedCustomTimePeriodId } from "../../slice/accountingSharedUiSlice";

type ReportData = {
  glFiscalTypeId: string;
  fromDate?: string;
  thruDate?: string;
  selectedMonth?: number | undefined;
};

const IncomeStatement = () => {
  const dispatch = useAppDispatch()
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.income-statement.list";
  const initialData = {
    glFiscalTypeId: "",
    fromDate: undefined,
    thruDate: undefined,
    selectedMonth: undefined,
  };
  const [reportData, setReportData] = useState<ReportData>(initialData);
  const { selectedAccountingCompanyName, selectedAccountingCompanyId } =
    useAppSelector((state) => state.accountingSharedUi);
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }

  const {
    data: incomeStatementReportData,
    isFetching,
    isSuccess,
    isLoading,
  } = useFetchIncomeStatementReportQuery(
    {
      organizationPartyId: selectedAccountingCompanyId!,
      glFiscalTypeId: reportData!.glFiscalTypeId,
      fromDate: reportData!.fromDate,
      thruDate: reportData!.thruDate,
    },
    {
      skip: !reportData.glFiscalTypeId,
    }
  );

  const onSubmit = (values: any) => {
    console.log(values);
    const { fromDate, thruDate, selectedMonth, glFiscalTypeId } = values;
    if (!fromDate && !thruDate && !selectedMonth) {
      toast.error(
        getTranslatedLabel(
          "general.date-range-or-month-error",
          "Must select month or date range for report!"
        )
      );
      return;
    }
    setReportData({
      glFiscalTypeId,
      fromDate: new Date(fromDate).toLocaleDateString(),
      thruDate: new Date(thruDate).toLocaleDateString(),
      selectedMonth: selectedMonth ? selectedMonth - 1 : undefined,
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
          dispatch(setSeletedCustomTimePeriodId(undefined))
          router.navigate("/accountingTransactionEntries", {state: {glAccountId: props.dataItem.glAccountId}});
        }}
      >
        {props.dataItem.glAccountId}
      </Button>
    </td>
  );
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
            {getTranslatedLabel(
              `${localizationKey}.title`,
              "Income Statement For: "
            )}{" "}
            {selectedAccountingCompanyName}
          </Typography>
          <Grid item xs={12} sx={{ margin: 3 }}>
            <IncomeStatementForm onSubmit={onSubmit} />
          </Grid>
          {isSuccess && (
            <>
              <Grid item xs={12}>
                <div className="div-container">
                  <KendoGrid
                    style={{ height: "250px", flex: 1 }}
                    resizable={true}
                    pageable={true}
                    data={incomeStatementReportData?.revenueAccountBalances ?? []}
                  >
                    <GridToolbar>
                      <Typography variant="body1">
                        Account Revenues
                      </Typography>
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
                      field="balance"
                      title={getTranslatedLabel(
                        `${localizationKey}.balance`,
                        "Balance"
                      )}
                      format="{0:c2}"
                    />
                  </KendoGrid>
                </div>
              </Grid>
              <Grid item xs={12}>
              <div className="div-container">
                <KendoGrid
                  style={{ height: "250px", flex: 1 }}
                  resizable={true}
                  pageable={true}
                  data={incomeStatementReportData?.expenseAccountBalances ?? []}
                >
                  <GridToolbar>
                    <Typography variant="body1">
                      Expenses
                    </Typography>
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
                    field="balance"
                    title={getTranslatedLabel(
                      `${localizationKey}.balance`,
                      "Balance"
                    )}
                    format="{0:c2}"
                  />
                </KendoGrid>
              </div>
              </Grid>
              <Grid item xs={12}>
              <div className="div-container">
                <KendoGrid
                  style={{ height: "250px", flex: 1 }}
                  resizable={true}
                  pageable={true}
                  data={incomeStatementReportData?.incomeAccountBalances ?? []}
                >
                  <GridToolbar>
                    <Typography variant="body1">
                      Income
                    </Typography>
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
                    field="balance"
                    title={getTranslatedLabel(
                      `${localizationKey}.balance`,
                      "Balance"
                    )}
                    format="{0:c2}"
                  />
                </KendoGrid>
              </div>
              </Grid>
              <Grid item xs={12}/>
              <Typography variant="h5" ml={2} mt={3}>
                Totals
              </Typography>
              <Grid item xs={6} ml={2}>
                <Box sx={{
                  border: "1px solid",
                  borderColor: "grey.400",
                  padding: 1,
                  borderRadius: "0.5rem"
                }}>
                  <div className="div-container">
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Total Contra Revenue
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(incomeStatementReportData?.contraRevenueBalanceTotal)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Cost of Goods Sold
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(incomeStatementReportData?.cogsExpenseBalanceTotal)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Net Sales
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(incomeStatementReportData?.netSales)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Gross Margin
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(incomeStatementReportData?.grossMargin)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Depreciation
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(incomeStatementReportData?.depreciationBalanceTotal)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Income From Operations
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(incomeStatementReportData?.incomeFromOperations)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Net Income
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(incomeStatementReportData?.netIncome)}
                        </Typography>
                      </Grid>
                    </Grid>
                  </div>
                </Box>
              </Grid>
            </>            
          )}
          {(isLoading || isFetching) && (
            <LoadingComponent
              message={getTranslatedLabel(
                `general.loading-report`,
                "Loading Report Data..."
              )}
            />
          )}
        </Paper>
      </Grid>
    </>
  );
};

export default IncomeStatement;
