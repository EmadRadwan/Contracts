// ... other imports
import { ChartOfAccountsExcel } from './ChartOfAccountsExcel'; // Adjust path as needed

const OrganizationChartOfAccountsList = ({ companyId }: Props) => {
    // ... existing state and hooks

    // REFACTOR: Add getTranslatedLabel function
    // Purpose: Provides translation support for Excel export, matching SupplyCertificateExcel
    // Improvement: Ensures consistent UI labels and supports Arabic translations
    // Context: Placeholder; replace with actual translation logic from your app
    const getTranslatedLabel = (key: string, defaultValue: string): string => {
        // Replace with your actual translation logic, e.g., using i18n
        return defaultValue;
    };

    return (
        <>
            <div className="div-container">
                <TabContext value={value}>
                    <Box sx={{ display: "flex", typography: "body1", ml: 2, mt: 1 }}>
                        <StyledTabs onChange={handleChange} value={value}>
                            <StyledTab label="Accounts Tree" value={"1"} />
                            <StyledTab label="List of Accounts" value={"2"} />
                        </StyledTabs>
                    </Box>
                    <TabPanel value="1">
                        <KendoGrid
                            style={{ height: "65vh", flex: 1 }}
                            resizable={true}
                            sortable={true}
                            detail={DetailComponent}
                            expandField="expanded"
                            onExpandChange={expandChange}
                            data={accounts ?? []}
                            reorderable={true}
                        >
                            {/* REFACTOR: Add Excel export button to GridToolbar */}
                            {/* Purpose: Allows users to export COA tree to Excel from Accounts Tree tab */}
                            {/* Improvement: Enhances usability by providing export functionality */}
                            {/* Context: Matches SupplyCertificateExcel's button placement */}
                            <GridToolbar>
                                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                    <Typography variant="body1">Chart of Accounts</Typography>
                                    <ChartOfAccountsExcel
                                        accounts={accounts}
                                        companyId={companyId}
                                        getTranslatedLabel={getTranslatedLabel}
                                    />
                                </Box>
                            </GridToolbar>
                            <Column field="glAccountId" title="Account Number" width={120} cell={AccountDescriptionCell} />
                            <Column field="text" title="Account Name" width={400} />
                            <Column field="parentAccountName" title="Parent Account Name" width={400} />
                        </KendoGrid>
                    </TabPanel>
                    {/* ... rest of the component unchanged */}
                </TabContext>
                {isFetching && <LoadingComponent message="Loading Accounts..." />}
            </div>
        </>
    );
};

export default OrganizationChartOfAccountsList;