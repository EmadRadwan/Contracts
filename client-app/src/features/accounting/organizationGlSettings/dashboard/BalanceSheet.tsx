import { Grid, Paper, Typography, Box, Button } from '@mui/material'
import { useState } from 'react'
import { router } from '../../../../app/router/Routes'
import { useAppDispatch, useAppSelector } from '../../../../app/store/configureStore'
import AccountingMenu from '../../invoice/menu/AccountingMenu'
import BalanceSheetForm from '../form/BalanceSheetForm'
import AccountingReportBreadcrumbs from '../menu/AccountingReportBreadcrumbs'
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper'
import { useFetchBalanceSheetReportQuery } from '../../../../app/store/apis/accounting/accountingReportsApi'
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridToolbar,
  GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import { formatCurrency } from '../../../../app/util/utils'
import { useTableKeyboardNavigation } from '@progress/kendo-react-data-tools'
import { setSeletedCustomTimePeriodId } from '../../slice/accountingSharedUiSlice'

type ReportData = {
  glFiscalTypeId: string;
  thruDate?: string;
};

const BalanceSheet = () => {
  const dispatch = useAppDispatch()
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.balance-sheet.list";
    const { selectedAccountingCompanyName, selectedAccountingCompanyId } =
    useAppSelector((state) => state.accountingSharedUi);
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }

  const initialData = {
      glFiscalTypeId: "",
      thruDate: undefined,
    };
    const [reportData, setReportData] = useState<ReportData>(initialData);
    const {
      data: balanceSheetReportData,
      isFetching,
      isSuccess,
      isLoading,
    } = useFetchBalanceSheetReportQuery(
      {
        organizationPartyId: selectedAccountingCompanyId!,
        glFiscalTypeId: reportData!.glFiscalTypeId,
        thruDate: reportData!.thruDate,
      },
      {
        skip: !reportData.glFiscalTypeId,
      }
    );

  const onSubmit = (values: any) => {
    console.log(values);
    const { thruDate, glFiscalTypeId } = values;
    setReportData({
      glFiscalTypeId,
      thruDate: new Date(thruDate).toLocaleDateString(),
    });
  }

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
      <Grid container padding={2} columnSpacing={1} >
      <Paper elevation={5} className={`div-container-withBorderCurved`} sx={{width: '100%'}}>
      <AccountingReportBreadcrumbs />

        <Typography variant="h4" margin={3}>
          Balance Sheet For: {selectedAccountingCompanyName}
        </Typography>
        <Grid item xs={12} sx={{margin: 3}}>
            <BalanceSheetForm onSubmit={onSubmit}/>
        </Grid>
        {isSuccess && (
            <>
              <Grid item xs={12}>
                <div className="div-container">
                  <KendoGrid
                    style={{ height: "250px", flex: 1 }}
                    resizable={true}
                    pageable={true}
                    data={balanceSheetReportData?.assetAccountBalanceList ?? []}
                  >
                    <GridToolbar>
                      <Typography variant="body1">
                        Assets
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
                  data={balanceSheetReportData?.liabilityAccountBalanceList ?? []}
                >
                  <GridToolbar>
                    <Typography variant="body1">
                      Liabilities
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
                  data={balanceSheetReportData?.equityAccountBalanceList ?? []}
                >
                  <GridToolbar>
                    <Typography variant="body1">
                      Equities
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
                          Current Assets
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(balanceSheetReportData?.currentAssetBalanceTotal)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Long Term Assets
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(balanceSheetReportData?.longtermAssetBalanceTotal)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Total Accumulated Depriciation
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(balanceSheetReportData?.accumDepreciationBalanceTotal)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Total Assets
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(balanceSheetReportData?.assetBalanceTotal)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Current Liabilities
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(balanceSheetReportData?.currentLiabilityBalanceTotal)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Equities
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(balanceSheetReportData?.equityBalanceTotal)}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid item container xs={12}>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          Total Liabilities and Equities
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          {formatCurrency(balanceSheetReportData?.liabilityEquityBalanceTotal)}
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
  )
}

export default BalanceSheet