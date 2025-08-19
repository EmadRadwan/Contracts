import React from 'react'
import GlAccountDefaults from './GlAccountDefaults'
import { Grid, Paper } from '@mui/material';
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
  } from "@progress/kendo-react-grid";
import { SortDescriptor, State, orderBy } from '@progress/kendo-data-query';
import { useAppSelector, useFetchTaxAuthoritiesGlAccountsQuery } from '../../../../../app/store/configureStore';
import { router } from "../../../../../app/router/Routes";
import { toast } from 'react-toastify';
import FinAccountGlAccountsForm from '../../form/FinAccountGlAccountsForm';
import TaxAuthorityGlAccountsForm from '../../form/TaxAuthorityGlAccountsForm';

const TaxAuthorityGlAccounts = () => {
    const selectedAccountingCompanyId = useAppSelector(
        (state) => state.accountingSharedUi.selectedAccountingCompanyId
    );
    if (!selectedAccountingCompanyId) {
        router.navigate("/orgGl");
      }
    const initialSort: Array<SortDescriptor> = [
        { field: "taxAuthPartyName", dir: "desc" },
    ];
    const { data: taxAuthGlAccounts } =
    useFetchTaxAuthoritiesGlAccountsQuery(selectedAccountingCompanyId, {
            skip: selectedAccountingCompanyId === undefined,
        });
    console.log(taxAuthGlAccounts)
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = { skip: 0, take: 9 };
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    const onSubmit = async (values: any) => {
        console.log(values);

        if (Object.values(values).some((v) => v === undefined)) {
            toast.warn("Please select an account and an account type.");
            return;
        }
    };
  return (
    <>
        <GlAccountDefaults />
        <Grid container padding={2} columnSpacing={1} justifyContent={"center"}>
        <Grid item xs={8}>
          <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <TaxAuthorityGlAccountsForm selectedAccountingCompanyId={selectedAccountingCompanyId} onSubmit={onSubmit} />

            <div className="div-container">
              <KendoGrid
                data={orderBy(
                  taxAuthGlAccounts ? taxAuthGlAccounts : [],
                    sort
                  ).slice(page.skip, page.take + page.skip)}
                sortable={true}
                sort={sort}
                onSortChange={(e: GridSortChangeEvent) => {
                  setSort(e.sort);
                }}
                skip={page.skip}
                take={page.take}
                total={
                  0
                }
                pageable={true}
                onPageChange={pageChange}
              >
                <Column
                  field="taxAuthGeoId"
                  title="Tax Authority Party Geo"
                  width={200}
                />
                <Column
                  field="taxAuthPartyName"
                  title="Tax Authority Party"
                  width={200}
                />
                <Column field="glAccountName" title="GL Account" />
                {/* <Column cell={CommandCell} width="auto" /> */}
              </KendoGrid>
            </div>
          </Paper>
        </Grid>
      </Grid>
    </>
  )
}

export default TaxAuthorityGlAccounts