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
import { useAppSelector, useFetchPaymentTypeGlAccountsQuery } from '../../../../../app/store/configureStore';
import { router } from "../../../../../app/router/Routes";
import { toast } from 'react-toastify';
import { useFetchFinAccountGlAccountsQuery } from '../../../../../app/store/apis/accounting/finAccountGlAccountsApi';
import FinAccountGlAccountsForm from '../../form/FinAccountGlAccountsForm';
import PaymentTypeGlAccountsForm from '../../form/PaymentTypeGlAccountsForm';
import Button from "@mui/material/Button";
import {OrderAdjustment} from "../../../../../app/models/order/orderAdjustment";

const PaymentTypeGlAccounts = () => {
    const selectedAccountingCompanyId = useAppSelector(
        (state) => state.accountingSharedUi.selectedAccountingCompanyId
    );
    if (!selectedAccountingCompanyId) {
        router.navigate("/orgGl");
      }
    const initialSort: Array<SortDescriptor> = [
        { field: "finAccountTypeDescription", dir: "desc" },
    ];
    const { data: paymentTypeAccounts } =
    useFetchPaymentTypeGlAccountsQuery(selectedAccountingCompanyId, {
            skip: selectedAccountingCompanyId === undefined,
        });
    console.log(paymentTypeAccounts)
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

    const RemoveCell = (props: any) => {
        const { dataItem } = props;

        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    onClick={() => props.remove(dataItem)}
                    disabled={dataItem.isManual === "N"}
                    color="error"
                >
                    Remove
                </Button>
            </td>
        );
    };
    const remove = (dataItem: OrderAdjustment) => {};

    const CommandCell = (props: GridCellProps) => (
        <RemoveCell {...props} remove={remove} />
    );
    
  return (
    <>
        <GlAccountDefaults />
        <Grid container padding={2} columnSpacing={1} justifyContent={"center"}>
        <Grid item xs={8}>
          <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <PaymentTypeGlAccountsForm selectedAccountingCompanyId={selectedAccountingCompanyId} onSubmit={onSubmit} />

            <div className="div-container">
              <KendoGrid
                data={orderBy(
                  paymentTypeAccounts ? paymentTypeAccounts : [],
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
                  field="paymentTypeDescription"
                  title="Payment Type"
                  width={300}
                />
                <Column field="glAccountTypeName" title="GL Account" />
                  <Column cell={CommandCell} width="auto" />

              </KendoGrid>
            </div>
          </Paper>
        </Grid>
      </Grid>
    </>
  )
}

export default PaymentTypeGlAccounts