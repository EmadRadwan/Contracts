import { Button, Grid, Paper, Typography } from "@mui/material";
import {
  useAppDispatch,
  useAppSelector,
} from "../../../../app/store/configureStore";
import { router } from "../../../../app/router/Routes";
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
  GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import { useState } from "react";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { formatCurrency } from "../../../../app/util/utils";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import GlAccountTransactionsModal from "./GlAccountTransactionsModal";

const TrialBalance = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.orgGL.reports.trial-balance"
  const [showTransactionsModal, setShowTransactionsModal] = useState(false);
  const [selectedGlAccountId, setSelectedGlAccountId] = useState<string | null>(null);

  const initialSort: Array<SortDescriptor> = [
    { field: "accountCode", dir: "asc" },
  ];
  const [sort, setSort] = useState(initialSort);
  const initialDataState: State = { skip: 0, take: 6 };
  const [page, setPage] = useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };
  const {
    selectedAccountingCompanyName,
    selectedAccountingCompanyId,
    seletedCustomTimePeriodId,
  } = useAppSelector((state) => state.accountingSharedUi);
  const dispatch = useAppDispatch();
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }
  const handleSelectTimePeriod = (value: any) => {
    dispatch(setSeletedCustomTimePeriodId(value.customTimePeriodId));
  };
  const { data, isLoading, isFetching, isSuccess } =
    useFetchTrialBalanceReportQuery(
      {
        customTimePeriodId: seletedCustomTimePeriodId!,
        organizationPartyId: selectedAccountingCompanyId!,
      }!,
      {
        skip: !seletedCustomTimePeriodId || !selectedAccountingCompanyId,
      }
    );


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
            {getTranslatedLabel(`${localizationKey}.title`, "Trial Balance For: ")} {selectedAccountingCompanyName}
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
                  style={{ height: "300px" }}
                  data={orderBy(data?.accountBalances ?? [], sort).slice(
                    page.skip,
                    page.take + page.skip
                  )}
                  sortable={true}
                  sort={sort}
                  onSortChange={(e: GridSortChangeEvent) => {
                    setSort(e.sort);
                  }}
                  skip={page.skip}
                  take={page.take}
                  total={data?.accountBalances.length ?? 0}
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
                  </GridToolbar>
                  <Column field="accountCode" title={getTranslatedLabel(`${localizationKey}.accountCode`, "Account Code")} cell={AccountCodeCell} />
                  <Column field="accountName" title={getTranslatedLabel(`${localizationKey}.accountName`, "Account Name")} />
                  <Column field="openingBalance" title={getTranslatedLabel(`${localizationKey}.openingBalance`, "Opening Balance")} format="{0:c2}" />
                  <Column field="postedDebits" title={getTranslatedLabel(`${localizationKey}.postedDebits`, "Debit")} format="{0:c2}" />
                  <Column field="postedCredits" title={getTranslatedLabel(`${localizationKey}.postedCredits`, "Credit")} format="{0:c2}" />
                  <Column field="endingBalance" title={getTranslatedLabel(`${localizationKey}.endingBalance`, "Ending Balance")} format="{0:c2}" />
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
                organizationPartyId={selectedAccountingCompanyId!}
                customTimePeriodId={seletedCustomTimePeriodId!}
                glAccountId={selectedGlAccountId!}
            />
          </ModalContainer>
      )}
    </>
  );
};

export default TrialBalance;
