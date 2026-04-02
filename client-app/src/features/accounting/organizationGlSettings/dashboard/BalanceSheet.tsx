import {Box, Button, Grid, Paper, Typography} from '@mui/material'
import {useMemo, useState} from 'react'
import {router} from '../../../../app/router/Routes'
import {useAppDispatch, useAppSelector} from '../../../../app/store/configureStore'
import AccountingMenu from '../../invoice/menu/AccountingMenu'
import BalanceSheetForm from '../form/BalanceSheetForm'
import AccountingReportBreadcrumbs from '../menu/AccountingReportBreadcrumbs'
import {useTranslationHelper} from '../../../../app/hooks/useTranslationHelper'
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridFilterChangeEvent,
    GridPageChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import {useTableKeyboardNavigation} from '@progress/kendo-react-data-tools'
import {CompositeFilterDescriptor, filterBy, orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import {useLazyFetchBalanceSheetReportQuery} from "../../../../app/store/apis/accounting/accountingReportsApi";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import BalanceSheetGlAccountTransactionsModal from "./BalanceSheetGlAccountTransactionsModal";

type ReportData = {
    glFiscalTypeId: string;
    thruDate?: string;
};

const BalanceSheet = () => {
    const dispatch = useAppDispatch()
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.orgGL.reports.balance-sheet.list";
    const [filter, setFilter] = useState<CompositeFilterDescriptor | undefined>(undefined);
    const [filter2, setFilter2] = useState<CompositeFilterDescriptor | undefined>(undefined);
    const [filter3, setFilter3] = useState<CompositeFilterDescriptor | undefined>(undefined);
    const [showTransactionsModal, setShowTransactionsModal] = useState(false);
    const [selectedGlAccountId, setSelectedGlAccountId] = useState<string | null>(null);

    const initialSort: Array<SortDescriptor> = [
        {field: "accountCode", dir: "asc"},
    ];
    const [sort, setSort] = useState(initialSort);
    const [sort2, setSort2] = useState(initialSort);
    const [sort3, setSort3] = useState(initialSort);
    const initialDataState: State = {skip: 0, take: 25};
    const [page, setPage] = useState<State>(initialDataState);
    const [page2, setPage2] = useState<State>(initialDataState);
    const [page3, setPage3] = useState<State>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    const pageChange2 = (event: GridPageChangeEvent) => {
        setPage2(event.page);
    };
    const pageChange3 = (event: GridPageChangeEvent) => {
        setPage3(event.page);
    };

    const onFilterChange = (event: GridFilterChangeEvent) => {
        setFilter(event.filter);
    };
    const onFilterChange2 = (event: GridFilterChangeEvent) => {
        setFilter2(event.filter);
    };
    const onFilterChange3 = (event: GridFilterChangeEvent) => {
        setFilter3(event.filter);
    };
    
    
    const {selectedAccountingCompanyName, selectedAccountingCompanyId} =
        useAppSelector((state) => state.accountingSharedUi);
    if (!selectedAccountingCompanyId) {
        router.navigate("/orgGl");
    }

    const initialData = {
        glFiscalTypeId: "ACTUAL",
        thruDate: undefined,
    };
    const [reportData, setReportData] = useState<ReportData>(initialData);

    const [trigger, {
        data: balanceSheetReportData,
        isFetching,
        isSuccess,
        isLoading,
    }] = useLazyFetchBalanceSheetReportQuery();

    const processedDataAssets = useMemo(() => {
        const filtered = filterBy(balanceSheetReportData?.assetAccountBalanceList ?? [], filter);

        const sorted = orderBy(filtered, sort);

        return sorted.slice(page.skip, page.skip + page.take);
    }, [balanceSheetReportData?.assetAccountBalanceList, filter, sort, page.skip, page.take]);
    
    const totalAssets = filter ? filterBy(balanceSheetReportData?.assetAccountBalanceList ?? [], filter).length : balanceSheetReportData?.assetAccountBalanceList?.length ?? 0;

    const processedDataLiabilities = useMemo(() => {
        const filtered = filterBy(balanceSheetReportData?.liabilityAccountBalanceList ?? [], filter2);
        const sorted = orderBy(filtered, sort2);
        return sorted.slice(page2.skip, page2.skip + page2.take);
    }, [balanceSheetReportData?.liabilityAccountBalanceList, filter2, sort, page2.skip, page2.take]);
    
    const totalLiabilities = filter2 ? filterBy(balanceSheetReportData?.liabilityAccountBalanceList ?? [], filter2).length : balanceSheetReportData?.liabilityAccountBalanceList?.length ?? 0;

    const processedDataEquities = useMemo(() => {
        const filtered = filterBy(balanceSheetReportData?.equityAccountBalanceList ?? [], filter3);
        const sorted = orderBy(filtered, sort3);
        return sorted.slice(page3.skip, page3.skip + page3.take);
    }, [balanceSheetReportData?.equityAccountBalanceList, filter3, sort3, page3.skip, page3.take]);
    const totalEquities = filter3 ? filterBy(balanceSheetReportData?.equityAccountBalanceList ?? [], filter3).length : balanceSheetReportData?.equityAccountBalanceList?.length ?? 0;


    const onSubmit = (values: any) => {
        console.log("Form Values:", values);   // ← Check this in console

        const glFiscalTypeId = values.glFiscalTypeId || "ACTUAL"; // Fallback

        let formattedThruDate: string;

        const rawDate = values.thruDate;

        if (rawDate instanceof Date && !isNaN(rawDate.getTime())) {
            formattedThruDate = rawDate.toISOString().split('T')[0];
        } else if (typeof rawDate === 'string') {
            const parsed = new Date(rawDate);
            formattedThruDate = !isNaN(parsed.getTime())
                ? parsed.toISOString().split('T')[0]
                : new Date().toISOString().split('T')[0];
        } else {
            formattedThruDate = new Date().toISOString().split('T')[0];
        }

        console.log("Sending to API:", {
            glFiscalTypeId,
            thruDate: formattedThruDate
        });

        setReportData({
            glFiscalTypeId,
            thruDate: formattedThruDate
        });

        trigger({
            organizationPartyId: selectedAccountingCompanyId!,
            glFiscalTypeId: glFiscalTypeId,
            thruDate: formattedThruDate,
        });
    };

    const formatNumber = (value: number | undefined) => {
        return value?.toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }) || '0.00';
    };

    const AccountCodeCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: "blue"}}
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
                        setSelectedGlAccountId(props.dataItem.glAccountId);
                        setShowTransactionsModal(true);
                    }}
                >
                    {props.dataItem.accountCode}
                </Button>
            </td>
        );
    }
    return (
        <>
            <AccountingMenu selectedMenuItem={"/orgGl"}/>
            <Grid container padding={2} columnSpacing={1}>
                <Paper elevation={5} className={`div-container-withBorderCurved`} sx={{width: '100%'}}>
                    <AccountingReportBreadcrumbs/>

                    <Typography variant="h4" margin={3}>
                        {getTranslatedLabel(`${localizationKey}.title-for`, "Balance Sheet For")}: {selectedAccountingCompanyName}
                    </Typography>
                    <Grid item xs={12} sx={{margin: 3}}>
                        <BalanceSheetForm onSubmit={onSubmit}/>
                    </Grid>
                    {isSuccess && (
                        <>
                            <Grid item xs={12}>
                                <div className="div-container">
                                    <KendoGrid
                                        style={{height: "500px", flex: 1}}
                                        data={processedDataAssets}           // ← now use processedData
                                        sortable={true}
                                        resizable={true}
                                        sort={sort}
                                        onSortChange={(e) => setSort(e.sort)}

                                        filterable={true}              // ← enable filter row
                                        filter={filter}                // ← controlled filter
                                        onFilterChange={onFilterChange}

                                        skip={page.skip}
                                        take={page.take}
                                        total={totalAssets}                  // ← important!
                                        pageable={true}
                                        onPageChange={pageChange}
                                    >
                                        <GridToolbar>
                                            <Typography variant="body1">
                                                {getTranslatedLabel(`${localizationKey}.assets`, "Assets")}
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
                                            format="{0:n2}"
                                        />
                                    </KendoGrid>
                                </div>
                            </Grid>
                            <Grid item xs={12}>
                                <div className="div-container">
                                    <KendoGrid
                                        style={{height: "500px", flex: 1}}
                                        sortable={true}
                                        resizable={true}
                                        sort={sort2}
                                        onSortChange={(e) => setSort2(e.sort)}

                                        filterable={true}
                                        filter={filter2}
                                        onFilterChange={onFilterChange2}

                                        skip={page2.skip}
                                        take={page2.take}
                                        total={totalLiabilities}
                                        pageable={true}
                                        onPageChange={pageChange2}
                                        data={processedDataLiabilities}
                                    >
                                        <GridToolbar>
                                            <Typography variant="body1">
                                                {getTranslatedLabel(`${localizationKey}.liabilities`, "Liabilities")}
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
                                            format="{0:n2}"
                                        />
                                    </KendoGrid>
                                </div>
                            </Grid>
                            <Grid item xs={12}>
                                <div className="div-container">
                                    <KendoGrid
                                        style={{height: "500px", flex: 1}}
                                        sortable={true}
                                        resizable={true}
                                        sort={sort3}
                                        onSortChange={(e) => setSort3(e.sort)}

                                        filterable={true}
                                        filter={filter3}
                                        onFilterChange={onFilterChange3}

                                        skip={page3.skip}
                                        take={page3.take}
                                        total={totalEquities}
                                        pageable={true}
                                        onPageChange={pageChange3}
                                        data={processedDataEquities}
                                    >
                                        <GridToolbar>
                                            <Typography variant="body1">
                                                {getTranslatedLabel(`${localizationKey}.equities`, "Equities")}
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
                                            format="{0:n2}"
                                        />
                                    </KendoGrid>
                                </div>
                            </Grid>
                            <Grid item xs={12}/>
                            <Typography variant="h5" ml={2} mt={3}>
                                {getTranslatedLabel(`${localizationKey}.totals`, "Totals")}
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
                                                    {getTranslatedLabel(`${localizationKey}.current-assets`, "Current Assets")}
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {formatNumber(balanceSheetReportData?.currentAssetBalanceTotal)}
                                                </Typography>
                                            </Grid>
                                        </Grid>
                                        <Grid item container xs={12}>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {getTranslatedLabel(`${localizationKey}.long-term-assets`, "Long Term Assets")}
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {formatNumber(balanceSheetReportData?.longtermAssetBalanceTotal)}
                                                </Typography>
                                            </Grid>
                                        </Grid>
                                        <Grid item container xs={12}>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {getTranslatedLabel(`${localizationKey}.accumulated-depreciation`, "Total Accumulated Depriciation")}
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {formatNumber(balanceSheetReportData?.accumDepreciationBalanceTotal)}
                                                </Typography>
                                            </Grid>
                                        </Grid>
                                        <Grid item container xs={12}>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {getTranslatedLabel(`${localizationKey}.total-assets`, "Total Assets")}
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {formatNumber(balanceSheetReportData?.assetBalanceTotal)}
                                                </Typography>
                                            </Grid>
                                        </Grid>
                                        <Grid item container xs={12}>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {getTranslatedLabel(`${localizationKey}.current-liabilities`, "Current Liabilities")}
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {formatNumber(balanceSheetReportData?.currentLiabilityBalanceTotal)}
                                                </Typography>
                                            </Grid>
                                        </Grid>
                                        <Grid item container xs={12}>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {getTranslatedLabel(`${localizationKey}.total-equities`, "Equities")}
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {formatNumber(balanceSheetReportData?.equityBalanceTotal)}
                                                </Typography>
                                            </Grid>
                                        </Grid>
                                        <Grid item container xs={12}>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {getTranslatedLabel(`${localizationKey}.total-liabilities-equities`, "Total Liabilities and Equities")}
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Typography variant="body1">
                                                    {formatNumber(balanceSheetReportData?.liabilityEquityBalanceTotal)}
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
            {showTransactionsModal && (
                <ModalContainer
                    show={showTransactionsModal}
                    onClose={() => setShowTransactionsModal(false)}
                    width={950}
                >
                    <BalanceSheetGlAccountTransactionsModal
                        onClose={() => setShowTransactionsModal(false)}
                        organizationPartyId={selectedAccountingCompanyId!}
                        thruDate={reportData.thruDate || new Date().toISOString().split('T')[0]}
                        glFiscalTypeId={reportData.glFiscalTypeId}
                        glAccountId={selectedGlAccountId!}
                    />
                </ModalContainer>
            )}
        </>
    )
}

export default BalanceSheet