import { Paper, Typography, Grid } from '@mui/material';
import React from 'react'
import { router } from '../../../../app/router/Routes';
import { useAppSelector } from '../../../../app/store/configureStore';
import AccountingMenu from '../../invoice/menu/AccountingMenu';
import AccountingReportsMenu from '../menu/AccountingReportsMenu';
import AccountingSummaryMenu from '../menu/AccountingSummaryMenu';
import SetupAccountingMenu from '../menu/SetupAccountingMenu';
import ComaparativeCashFlowStatementForm from '../form/ComaparativeCashFlowStatementForm';
import AccountingReportBreadcrumbs from '../menu/AccountingReportBreadcrumbs';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';

const ComparativeCashFlowStatement = () => {
  const { getTranslatedLabel } = useTranslationHelper();
    const { selectedAccountingCompanyName, selectedAccountingCompanyId } =
    useAppSelector((state) => state.accountingSharedUi);
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }

  const onSubmit = (values: any) => {
    console.log(values);
  }
  return (
    <>
      <AccountingMenu selectedMenuItem={"/orgGl"} />
      <Grid container padding={2} columnSpacing={1} >
      <Paper elevation={5} className={`div-container-withBorderCurved`} sx={{width: '100%'}}>
      <AccountingReportBreadcrumbs />

        <Typography variant="h4" margin={3}>
          Comparative Cash Flow Statement For: {selectedAccountingCompanyName}
        </Typography>
        <Grid item xs={12} sx={{margin: 3}}>
            <ComaparativeCashFlowStatementForm onSubmit={onSubmit}/>
        </Grid>
      </Paper>
      </Grid>
    </>
  )
}

export default ComparativeCashFlowStatement