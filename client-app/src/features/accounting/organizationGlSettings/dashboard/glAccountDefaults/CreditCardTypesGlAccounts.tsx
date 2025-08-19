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
import { useAppSelector, useFetchCreditCardTypesGlAccountsQuery } from '../../../../../app/store/configureStore';
import { router } from "../../../../../app/router/Routes";
import { toast } from 'react-toastify';
import { useFetchFinAccountGlAccountsQuery } from '../../../../../app/store/apis/accounting/finAccountGlAccountsApi';
import FinAccountGlAccountsForm from '../../form/FinAccountGlAccountsForm';
import CreditCardTypesGlAccountsForm from '../../form/CreditCardTypesGlAccountsForm';

const CreditCardTypesGlAccounts = () => {
    const selectedAccountingCompanyId = useAppSelector(
        (state) => state.accountingSharedUi.selectedAccountingCompanyId
    );
    if (!selectedAccountingCompanyId) {
        router.navigate("/orgGl");
      }
    const initialSort: Array<SortDescriptor> = [
        { field: "cardTypeId", dir: "desc" },
    ];
    const { data: creditCardTypesAccounts } =
    useFetchCreditCardTypesGlAccountsQuery(selectedAccountingCompanyId, {
            skip: selectedAccountingCompanyId === undefined,
        });
    console.log(creditCardTypesAccounts)
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = { skip: 0, take: 9 };
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    const onSubmit = async (values: any) => {
        console.log(values);

        if (Object.values(values).some((v) => v === undefined)) {
            toast.warn("Please select an account and a card type.");
            return;
        }
    };
  return (
    <>
        <GlAccountDefaults />
        <Grid container padding={2} columnSpacing={1} justifyContent={"center"}>
        <Grid item xs={8}>
          <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <CreditCardTypesGlAccountsForm selectedAccountingCompanyId={selectedAccountingCompanyId} onSubmit={onSubmit} />

            <div className="div-container">
              <KendoGrid
                data={orderBy(
                  creditCardTypesAccounts ? creditCardTypesAccounts : [],
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
                  field="cardTypeDescription"
                  title="Card Type"
                  width={300}
                />
                <Column field="glAccountName" title="GL Account" width={350} />
                {/* <Column cell={CommandCell} width="auto" /> */}
              </KendoGrid>
            </div>
          </Paper>
        </Grid>
      </Grid>
    </>
  )
}

export default CreditCardTypesGlAccounts