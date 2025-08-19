import { Paper, Typography, Grid } from '@mui/material';
import { router } from '../../../../app/router/Routes';
import { useAppSelector } from '../../../../app/store/configureStore';
import AccountingMenu from '../../invoice/menu/AccountingMenu';
import GlAccountTrialBalanceForm from '../form/GlAccountTrialBalanceForm';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import AccountingReportBreadcrumbs from '../menu/AccountingReportBreadcrumbs';
import { useEffect, useState } from 'react';
import { useFetchGlAccountTrialBalanceReportQuery } from '../../../../app/store/apis/accounting/accountingReportsApi';
import LoadingComponent from '../../../../app/layout/LoadingComponent';

type ReportData = {
  timePeriodId: string;
  glAccountId: string;
  isPosted?: string;
};

const GlAccountTrialBalance = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.gl-trial-balance"
    const { selectedAccountingCompanyName, selectedAccountingCompanyId } =
    useAppSelector((state) => state.accountingSharedUi);
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }
  const initialData = {
    timePeriodId: "",
    glAccountId: "",
    isPosted: "ALL"
  }
  const [reportData, setReportData] = useState<ReportData>(initialData)

  const {data: trialBalanceReportData, isSuccess, isFetching, isLoading} = useFetchGlAccountTrialBalanceReportQuery({
    organizationPartyId: selectedAccountingCompanyId!,
    glAccountId: reportData?.glAccountId,
    timePeriodId: reportData?.timePeriodId,
    isPosted: reportData?.isPosted
  }, {
    skip: !reportData.glAccountId || !reportData.timePeriodId
  })

  useEffect(() => {
    setReportData(initialData)
  }, [trialBalanceReportData])

  const onSubmit = (values: any) => {
    console.log(values);
    const {timePeriodId, isPosted, glAccountId} = values
    setReportData({
      timePeriodId,
      isPosted,
      glAccountId
    })
  }
  return (
    <>
      <AccountingMenu selectedMenuItem={"/orgGl"} />
      <Grid container padding={2} columnSpacing={1} >
      <Paper elevation={5} className={`div-container-withBorderCurved`} sx={{width: '100%'}}>
      <AccountingReportBreadcrumbs />

        <Typography variant="h4" margin={3}>
          {getTranslatedLabel(`${localizationKey}.title`, "Gl Account Trial Balance For: ")}{selectedAccountingCompanyName}
        </Typography>
        <Grid item xs={12} sx={{margin: 3}}>
            <GlAccountTrialBalanceForm onSubmit={onSubmit}/>
        </Grid>
        {(isFetching || isLoading) && <LoadingComponent message={getTranslatedLabel("general.loading-report", "Loading Report Data...")} />}
      </Paper>
      </Grid>
    </>
  )
}

export default GlAccountTrialBalance