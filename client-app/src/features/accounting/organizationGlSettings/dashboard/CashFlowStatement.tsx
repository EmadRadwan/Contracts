import { Grid, Paper, Typography } from "@mui/material";
import { router } from "../../../../app/router/Routes";
import { useAppSelector } from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import CashFlowStatementForm from "../form/CashFlowStatementForm";
import { toast } from "react-toastify";
import { useEffect, useState } from "react";
import { useFetchCashFlowStatementReportQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import LoadingComponent from "../../../../app/layout/LoadingComponent";

type ReportData = {
  glFiscalTypeId: string;
  fromDate?: string;
  thruDate?: string;
  selectedMonth?: number | undefined;
};

const CashFlowStatement = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.cash-flow.list";
  const { selectedAccountingCompanyName, selectedAccountingCompanyId } =
    useAppSelector((state) => state.accountingSharedUi);
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }
  const initialData = {
    glFiscalTypeId: "",
    fromDate: undefined,
    thruDate: undefined,
    selectedMonth: undefined,
  };
  const [reportData, setReportData] = useState<ReportData>(initialData);
  const {
    data: cashFlowReportData,
    isFetching,
    isSuccess,
    isLoading,
  } = useFetchCashFlowStatementReportQuery(
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
  useEffect(() => {
    setReportData(initialData);
  }, [cashFlowReportData]);
  const onSubmit = (values: any) => {
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
              "Cash Flow Statement For: "
            )}
            {selectedAccountingCompanyName}
          </Typography>
          <Grid item xs={12} sx={{ margin: 3 }}>
            <CashFlowStatementForm onSubmit={onSubmit} />
          </Grid>
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

export default CashFlowStatement;
