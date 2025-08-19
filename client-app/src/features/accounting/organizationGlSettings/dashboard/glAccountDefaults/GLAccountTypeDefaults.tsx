import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { Fragment } from "react";
import {
  Grid as KendoGrid,
  GridCellProps,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import { Grid, Paper } from "@mui/material";
import { OrderAdjustment } from "../../../../../app/models/order/orderAdjustment";
import {
  useAppDispatch,
  useAppSelector,
  useFetchGlAccountTypeDefaultsQuery
} from "../../../../../app/store/configureStore";
import GlAccountDefaults from "./GlAccountDefaults";
import { router } from "../../../../../app/router/Routes";
import { toast } from "react-toastify";
import GlAccountTypeDefaultsForm from "../../form/GlAccountTypeDefaultsForm";

export default function GLAccountTypeDefaults() {
  // get selectedAccountingCompanyId from redux
  const selectedAccountingCompanyId = useAppSelector(
    (state) => state.accountingSharedUi.selectedAccountingCompanyId
  );
  const { data: glAccountTypeDefaultsData } =
    useFetchGlAccountTypeDefaultsQuery(selectedAccountingCompanyId, {
      skip: selectedAccountingCompanyId === undefined,
    });

  const initialSort: Array<SortDescriptor> = [
    { field: "partyId", dir: "desc" },
  ];
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 9 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };

  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  } else {
    setTimeout(() => router.navigate("/gLAccountTypeDefaults"), 0)
  }


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

  const onSubmit = async (values: any) => {
    console.log(values);

    if (Object.values(values).some((v) => v === undefined)) {
      toast.warn("Please select an account and an account type.");
      return;
    }
  };

  return (
    <Fragment>
      <GlAccountDefaults />

      <Grid container padding={2} columnSpacing={1} justifyContent={"center"}>
        <Grid item xs={8}>
          <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <GlAccountTypeDefaultsForm selectedAccountingCompanyId={selectedAccountingCompanyId} onSubmit={onSubmit} />

            <div className="div-container">
              <KendoGrid
                data={orderBy(
                  glAccountTypeDefaultsData ? glAccountTypeDefaultsData : [],
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
                  glAccountTypeDefaultsData
                    ? glAccountTypeDefaultsData.length
                    : 0
                }
                pageable={true}
                onPageChange={pageChange}
              >
                <Column
                  field="glAccountTypeDescription"
                  title="GL Account Type"
                  width={300}
                />
                <Column field="glAccountId" title="Gl Account" width={350} />
                <Column cell={CommandCell} width="auto" />
              </KendoGrid>
            </div>
          </Paper>
        </Grid>
      </Grid>
    </Fragment>
  );
}
