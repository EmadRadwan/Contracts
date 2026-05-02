import React, { useState } from 'react';
import { Box, Paper, Typography, Grid } from '@mui/material';
import AccountingMenu from '../../invoice/menu/AccountingMenu';
import SetupAccountingMenu from '../menu/SetupAccountingMenu';
import AccountingSummaryMenu from '../menu/AccountingSummaryMenu';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import { CompanyReportExcel } from '../../reports/CompanyReportExcel';

const CompanyReportDashboard = () => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [reportOpen, setReportOpen] = useState(true);

    return (
        <>
            <AccountingMenu selectedMenuItem={"/orgGl"} />
            <Grid container padding={2} columnSpacing={1}>
                <Paper elevation={5} className={`div-container-withBorderCurved`}>
                    <SetupAccountingMenu selectedMenuItem="orgAccountingSummary" />
                    <AccountingSummaryMenu selectedMenuItem="accountingReports" />
                    <Box sx={{ p: 3, textAlign: 'center' }}>
                        <Typography variant="h4" gutterBottom>
                            {getTranslatedLabel("accounting.reports.companyExpensesVsRevenue.title", "Company Expenses vs Revenue Report")}
                        </Typography>
                        <Typography variant="body1" color="textSecondary">
                            Select date range to generate the company-wide report.
                        </Typography>
                        <CompanyReportExcel 
                            open={reportOpen} 
                            onClose={() => setReportOpen(false)} 
                        />
                        {!reportOpen && (
                            <Box sx={{ mt: 4 }}>
                                <Typography variant="h6" onClick={() => setReportOpen(true)} style={{ cursor: 'pointer', color: 'blue' }}>
                                    Open Report Dialog
                                </Typography>
                            </Box>
                        )}
                    </Box>
                </Paper>
            </Grid>
        </>
    );
};

export default CompanyReportDashboard;
