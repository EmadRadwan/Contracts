import { Button, Grid, Paper, Typography } from "@mui/material";
import {
  useAppDispatch,
  useAppSelector,
} from "../../../../app/store/configureStore";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import TrialBalanceCustomTimePeriodForm from "../form/TrialBalanceCustomTimePeriodForm";
import { useFetchTrialBalanceReportQuery } from "../../../../app/store/apis/accounting/accountingReportsApi";
import { setSeletedCustomTimePeriodId } from "../../slice/accountingSharedUiSlice";
import AccountingReportBreadcrumbs from "../menu/AccountingReportBreadcrumbs";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
  GridToolbar,
  GRID_COL_INDEX_ATTRIBUTE, GridFilterChangeEvent
} from "@progress/kendo-react-grid";
import {useEffect, useMemo, useState} from "react";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { formatCurrency } from "../../../../app/util/utils";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import GlAccountTransactionsModal from "./GlAccountTransactionsModal";
import {TrialBalanceExcel} from "../report/TrialBalanceExcel";
import {
  filterBy,
  CompositeFilterDescriptor,
} from "@progress/kendo-data-query";

const TrialBalance = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.trial-balance"
  const [showTransactionsModal, setShowTransactionsModal] = useState(false);
  const [selectedGlAccountId, setSelectedGlAccountId] = useState<string | null>(null);
  const [filter, setFilter] = useState<CompositeFilterDescriptor | undefined>(undefined);

  const initialSort: Array<SortDescriptor> = [
    { field: "accountCode", dir: "asc" },
  ];
  const [sort, setSort] = useState(initialSort);
  const initialDataState: State = { skip: 0, take: 25 };
  const [page, setPage] = useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };

  const onFilterChange = (event: GridFilterChangeEvent) => {
    setFilter(event.filter);
  };


  const {
    seletedCustomTimePeriodId,
  } = useAppSelector((state) => state.accountingSharedUi);
  const dispatch = useAppDispatch();
  const {user} = useAppSelector((state) => state.account);
  const companyId = user?.organizationPartyId || "";
  const organizationPartyName = user?.organizationPartyName || "";
  

  const handleSelectTimePeriod = (value: any) => {
    dispatch(setSeletedCustomTimePeriodId(value.customTimePeriodId));
  };
  const { data, isLoading, isFetching, isSuccess } =
    useFetchTrialBalanceReportQuery(
      {
        customTimePeriodId: seletedCustomTimePeriodId!,
        organizationPartyId: companyId!,
      }!,
      {
        skip: !seletedCustomTimePeriodId || !companyId,
      }
    );

  // Then compute the filtered + sorted + paged data
  const processedData = useMemo(() => {
    const filtered = filterBy(data?.accountBalances ?? [], filter);

    const sorted = orderBy(filtered, sort);

    return sorted.slice(page.skip, page.skip + page.take);
  }, [data?.accountBalances, filter, sort, page.skip, page.take]);

// Also update total for correct pager
  const total = filter ? filterBy(data?.accountBalances ?? [], filter).length : data?.accountBalances?.length ?? 0;


  useEffect(() => {
    return () => {
      // Clear the selected custom time period so future mounts start fresh
      dispatch(setSeletedCustomTimePeriodId(null));

      // Optional: If you want to fully remove this query's data from cache
      // You can dispatch api.util.resetApiState() globally (not recommended per query)
      // Or use the unstable reset if available in your RTK Query version
      // For most cases, just resetting the param (above) + skip: true is sufficient
    };
  }, [dispatch]);
  
  const excelRows = useMemo(() => {
    if (!data?.accountBalances) return [];
    return data.accountBalances.map(ab => ({
      accountCode: ab.accountCode ?? '',
      accountName: ab.accountName ?? '',
      openingBalance: ab.openingBalance ?? 0,
      postedDebits: ab.postedDebits ?? 0,
      postedCredits: ab.postedCredits ?? 0,
      endingBalance: ab.endingBalance ?? 0,
    }));
  }, [data?.accountBalances]);

  const AccountCodeCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
        <td
            className={props.className}
            style={{ ...props.style, color: 'blue' }}
            colSpan={props.colSpan}
            role={'gridcell'}
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
            {getTranslatedLabel(`${localizationKey}.title`, "Trial Balance For: ")} {organizationPartyName}
          </Typography>
          <Grid item xs={12} sx={{ margin: 3 }}>
            <TrialBalanceCustomTimePeriodForm
              onSubmit={handleSelectTimePeriod}
            />
          </Grid>
          {isSuccess && (
            <Grid container>
              <div className="div-container">
                <KendoGrid
                    className="main-grid"
                    data={processedData}           // ← now use processedData
                    sortable={true}
                    resizable={true}
                    sort={sort}
                    onSortChange={(e) => setSort(e.sort)}

                    filterable={true}              // ← enable filter row
                    filter={filter}                // ← controlled filter
                    onFilterChange={onFilterChange}

                    skip={page.skip}
                    take={page.take}
                    total={total}                  // ← important!
                    pageable={true}
                    onPageChange={pageChange}
                >
                  <GridToolbar>
                    <Typography variant="body1">
                      {getTranslatedLabel(`${localizationKey}.debits`, "Debits total: ")} {formatCurrency(data.postedDebitsTotal)}
                    </Typography>
                    <Typography variant="body1">
                      {getTranslatedLabel(`${localizationKey}.credits`, "Credits total: ")} {formatCurrency(data.postedCreditsTotal)}
                    </Typography>

                    <TrialBalanceExcel
                        companyName={organizationPartyName ?? ''}
                        rows={excelRows}
                        totals={{
                          postedDebitsTotal: data.postedDebitsTotal ?? 0,
                          postedCreditsTotal: data.postedCreditsTotal ?? 0,
                        }}
                        getTranslatedLabel={getTranslatedLabel}
                        isFetching={isFetching}
                    />
                  </GridToolbar>
                  <Column field="accountCode" title={getTranslatedLabel(`${localizationKey}.accountCode`, "Account Code")} cell={AccountCodeCell} />
                  <Column field="accountName" title={getTranslatedLabel(`${localizationKey}.accountName`, "Account Name")} />
                  <Column field="openingBalance" title={getTranslatedLabel(`${localizationKey}.openingBalance`, "Opening Balance")} format="{0:n2}" />
                  <Column field="postedDebits" title={getTranslatedLabel(`${localizationKey}.postedDebits`, "Debit")} format="{0:n2}" />
                  <Column field="postedCredits" title={getTranslatedLabel(`${localizationKey}.postedCredits`, "Credit")} format="{0:n2}" />
                  <Column field="endingBalance" title={getTranslatedLabel(`${localizationKey}.endingBalance`, "Ending Balance")} format="{0:n2}" />
                </KendoGrid>
              </div>
            </Grid>
          )}
          {(isFetching || isLoading) && <LoadingComponent message={getTranslatedLabel(`general.loading-report`, "Loading Report Data...")} />}
        </Paper>
      </Grid>
      {showTransactionsModal && (
          <ModalContainer
              show={showTransactionsModal}
              onClose={() => setShowTransactionsModal(false)}
              width={950}
          >
            <GlAccountTransactionsModal
                onClose={() => setShowTransactionsModal(false)}
                organizationPartyId={companyId!}
                customTimePeriodId={seletedCustomTimePeriodId!}
                glAccountId={selectedGlAccountId!}
            />
          </ModalContainer>
      )}
    </>
  );
};

export default TrialBalance;
