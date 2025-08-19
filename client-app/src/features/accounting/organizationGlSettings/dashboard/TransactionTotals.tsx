import { useEffect, useState } from "react";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { useAppDispatch, useAppSelector } from "../../../../app/store/configureStore";
import { router } from "../../../../app/router/Routes";
import TransactionTotalsForm from "../form/TransactionTotalsForm";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import { useFetchTransactionTotalsReportQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { TabContext, TabPanel } from "@mui/lab";
import { StyledTabs } from "../../../../app/components/StyledTabs";
import { StyledTab } from "../../../../app/components/StyledTab";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridCustomFooterCellProps,
  GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { setSeletedCustomTimePeriodId } from "../../slice/accountingSharedUiSlice";

type TransactionTotalsData = {
  glFiscalTypeId: string;
  fromDate?: string;
  thruDate?: string;
  selectedMonth?: number | undefined;
};


const TransactionTotals = () => {
  const dispatch = useAppDispatch()
  const initialData = {
    glFiscalTypeId: "",
    fromDate: undefined,
    thruDate: undefined,
    selectedMonth: undefined,
  }
  const [transactionTotalsData, setTransactionTotalsData] =
    useState<TransactionTotalsData>(initialData);
  const [value, setValue] = useState("1");
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.transation-totals.list";
  const { selectedAccountingCompanyName, selectedAccountingCompanyId } =
    useAppSelector((state) => state.accountingSharedUi);
  const {
    data: transactionTotalsReportData,
    isFetching,
    isSuccess,
    isLoading,
  } = useFetchTransactionTotalsReportQuery(
    {
      organizationPartyId: selectedAccountingCompanyId!,
      glFiscalTypeId: transactionTotalsData!.glFiscalTypeId,
      fromDate: transactionTotalsData!.fromDate,
      thruDate: transactionTotalsData!.thruDate,
      selectedMonth: transactionTotalsData!.selectedMonth,
    },
    {
      skip: !transactionTotalsData.glFiscalTypeId,
    }
  );

  useEffect(() => {
    setTransactionTotalsData(initialData)
  }, [transactionTotalsReportData])
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }
  const handleChange = (event: any, newValue: string) => {
    setValue(newValue);
  };
  const onSubmit = (values: any) => {
    console.log(values);
    const { fromDate, thruDate, selectedMonth, glFiscalTypeId } = values;
    if (!fromDate && !thruDate && !selectedMonth) {
      toast.error(getTranslatedLabel("general.date-range-or-month-error", "Must select month or date range for report!"))
      return;
    }
    setTransactionTotalsData({
      glFiscalTypeId,
      fromDate: fromDate ? new Date(fromDate).toLocaleDateString() : undefined,
      thruDate: thruDate ? new Date(thruDate).toLocaleDateString() : undefined,
      selectedMonth: selectedMonth ? selectedMonth - 1 : undefined,
    });
  };

const TotalFooterCustomCellPosted = (props: GridCustomFooterCellProps) => {
  const sumC = transactionTotalsReportData.postedTransactionTotals.reduce((a: number, b: any) => {
    return a + b['c']
  }, 0)
  const sumD = transactionTotalsReportData.postedTransactionTotals.reduce((a: number, b: any) => {
    return a + b['d']
  }, 0)
  return (props.field === 'c' || props.field === "d") ? (
      <td colSpan={props.colSpan} style={{ ...props.style, color: '#000000' }}>
          ${props.field === 'c' ? sumC : sumD}
      </td>
  ) : (
      <td
          {...props.tdProps}
          style={{
              color: '#f97e6d'
          }}
      >
          {" "}
      </td>
  );
};
const TotalFooterCustomCellUnposted = (props: GridCustomFooterCellProps) => {
  const sumC = transactionTotalsReportData.unpostedTransactionTotals.reduce((a: number, b: any) => {
    return a + b['c']
  }, 0)
  const sumD = transactionTotalsReportData.unpostedTransactionTotals.reduce((a: number, b: any) => {
    return a + b['d']
  }, 0)
  return (props.field === 'c' || props.field === "d") ? (
      <td colSpan={props.colSpan} style={{ ...props.style, color: '#000000' }}>
          ${props.field === 'c' ? sumC : sumD}
      </td>
  ) : (
      <td
          {...props.tdProps}
          style={{
              color: '#f97e6d'
          }}
      >
          {" "}
      </td>
  );
};
const TotalFooterCustomCellAll = (props: GridCustomFooterCellProps) => {
  const sumC = transactionTotalsReportData.allTransactionTotals.reduce((a: number, b: any) => {
    return a + b['c']
  }, 0)
  const sumD = transactionTotalsReportData.allTransactionTotals.reduce((a: number, b: any) => {
    return a + b['d']
  }, 0)
  return (props.field === 'c' || props.field === "d") ? (
      <td colSpan={props.colSpan} style={{ ...props.style, color: '#000000' }}>
          ${props.field === 'c' ? sumC : sumD}
      </td>
  ) : (
      <td
          {...props.tdProps}
          style={{
              color: '#f97e6d'
          }}
      >
          {" "}
      </td>
  );
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
              "Transaction Totals For: "
            )}{" "}
            {selectedAccountingCompanyName}
          </Typography>
          <Grid item xs={12} sx={{ margin: 3 }}>
            <TransactionTotalsForm onSubmit={onSubmit} />
          </Grid>
          {isSuccess && <Grid container spacing={1} alignItems={"center"}>
            <TabContext value={value}>
              <Box sx={{ display: "flex", typography: "body1", ml: 2, mt: 1 }}>
                <StyledTabs onChange={handleChange} value={value}>
                  <StyledTab label="Posted Totals" value={"1"} />
                  <StyledTab label="Unposted Totals" value={"2"} />
                  <StyledTab label="All Totals" value={"3"} />
                </StyledTabs>
              </Box>
              <TabPanel value={"1"}>
                <Grid item xs={12}>
                  <div className="div-container">
                    <KendoGrid
                      style={{ height: "65vh", flex: 1 }}
                      resizable={true}
                      filterable={true}
                      sortable={true}
                      pageable={true}
                      cells={{
                        footerCell: TotalFooterCustomCellPosted
                      }}
                      data={transactionTotalsReportData?.postedTransactionTotals ?? []}
                    >
                      <Column
                        field="accountCode"
                        title={getTranslatedLabel(`${localizationKey}.code`, "Account Code")}
                        cell={AccountCodeCell}
                      />
                      <Column
                        field="accountName"
                        title={getTranslatedLabel(`${localizationKey}.name`, "Account Name")}
                      />
                      <Column
                        field="openingD"
                        title={getTranslatedLabel(`${localizationKey}.openingD`, "Opening D")}
                        format="{0:c2}"
                      />
                      <Column
                        field="openingC"
                        title={getTranslatedLabel(`${localizationKey}.openingC`, "Opening C")}
                        format="{0:c2}"
                      />
                      <Column
                        field="d"
                        title={getTranslatedLabel(`${localizationKey}.debit`, "DR")}
                        format="{0:c2}"
                      />
                      <Column
                        field="c"
                        title={getTranslatedLabel(`${localizationKey}.credit`, "CR")}
                        format="{0:c2}"
                      />
                      <Column
                        field="balance"
                        title={getTranslatedLabel(`${localizationKey}.balance`, "Balance")}
                        format="{0:c2}"
                      />
                    </KendoGrid>
                  </div>
                </Grid>
              </TabPanel>
              <TabPanel value={"2"}>
                <Grid item xs={12}>
                  <div className="div-container">
                    <KendoGrid
                      style={{ height: "65vh", flex: 1 }}
                      resizable={true}
                      filterable={true}
                      sortable={true}
                      pageable={true}
                      cells={{
                        footerCell: TotalFooterCustomCellUnposted
                      }}
                      data={transactionTotalsReportData?.unpostedTransactionTotals ?? []}
                    >
                      <Column
                        field="accountCode"
                        title={getTranslatedLabel(`${localizationKey}.code`, "Account Code")}
                        cell={AccountCodeCell}
                      />
                      <Column
                        field="accountName"
                        title={getTranslatedLabel(`${localizationKey}.name`, "Account Name")}
                      />
                      <Column
                        field="openingD"
                        title={getTranslatedLabel(`${localizationKey}.openingD`, "Opening D")}
                        format="{0:c2}"
                      />
                      <Column
                        field="openingC"
                        title={getTranslatedLabel(`${localizationKey}.openingC`, "Opening C")}
                        format="{0:c2}"
                      />
                      <Column
                        field="d"
                        title={getTranslatedLabel(`${localizationKey}.debit`, "DR")}
                        format="{0:c2}"
                      />
                      <Column
                        field="c"
                        title={getTranslatedLabel(`${localizationKey}.credit`, "CR")}
                        format="{0:c2}"
                      />
                      <Column
                        field="balance"
                        title={getTranslatedLabel(`${localizationKey}.balance`, "Balance")}
                        format="{0:c2}"
                      />
                    </KendoGrid>
                  </div>
                </Grid>
              </TabPanel>
              <TabPanel value={"3"}>
                <Grid item xs={12}>
                  <div className="div-container">
                    <KendoGrid
                      style={{ height: "65vh", flex: 1 }}
                      resizable={true}
                      filterable={true}
                      sortable={true}
                      pageable={true}
                      cells={{
                        footerCell: TotalFooterCustomCellAll
                      }}
                      data={transactionTotalsReportData?.allTransactionTotals ?? []}
                    >
                      <Column
                        field="accountCode"
                        title={getTranslatedLabel(`${localizationKey}.code`, "Account Code")}
                        cell={AccountCodeCell}
                      />
                      <Column
                        field="accountName"
                        title={getTranslatedLabel(`${localizationKey}.name`, "Account Name")}
                      />
                      <Column
                        field="openingD"
                        title={getTranslatedLabel(`${localizationKey}.openingD`, "Opening D")}
                        format="{0:c2}"
                      />
                      <Column
                        field="openingC"
                        title={getTranslatedLabel(`${localizationKey}.openingC`, "Opening C")}
                        format="{0:c2}"
                      />
                      <Column
                        field="d"
                        title={getTranslatedLabel(`${localizationKey}.debit`, "DR")}
                        format="{0:c2}"
                      />
                      <Column
                        field="c"
                        title={getTranslatedLabel(`${localizationKey}.credit`, "CR")}
                        format="{0:c2}"
                      />
                      <Column
                        field="balance"
                        title={getTranslatedLabel(`${localizationKey}.balance`, "Balance")}
                        format="{0:c2}"
                      />
                    </KendoGrid>
                  </div>
                </Grid>
              </TabPanel>
            </TabContext>
          </Grid>}
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

export default TransactionTotals;
