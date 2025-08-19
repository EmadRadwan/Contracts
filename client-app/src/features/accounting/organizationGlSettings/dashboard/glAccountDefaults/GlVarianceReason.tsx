import React from "react";
import GlVarianceReasonForm from "../../form/GlVarianceReasonForm";
import GlAccountDefaults from "./GlAccountDefaults";
import { Button, Grid, Paper } from "@mui/material";
import {
  Grid as KendoGrid,
  GridCellProps,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import { router } from "../../../../../app/router/Routes";
import { useAppSelector } from "../../../../../app/store/configureStore";
import { toast } from "react-toastify";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import { useFetchVarianceReasonGlAccountsQuery } from "../../../../../app/store/apis/accounting/glVarianceReasonsApi";

const GlVarianceReason = () => {
  const selectedAccountingCompanyId = useAppSelector(
    (state) => state.accountingSharedUi.selectedAccountingCompanyId
  );

  const { data: glAccountsVarianceReasons } =
    useFetchVarianceReasonGlAccountsQuery(selectedAccountingCompanyId, {
      skip: selectedAccountingCompanyId === undefined
    });

  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }
  console.log(glAccountsVarianceReasons)
  const initialSort: Array<SortDescriptor> = [
    { field: "varianceReasonId", dir: "desc" },
  ];
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 9 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };
  const DeleteOrderItemAdjustmentCell = (props: any) => {
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
  const remove = (dataItem: any) => {
    console.log(dataItem)
  };

  const CommandCell = (props: GridCellProps) => (
    <DeleteOrderItemAdjustmentCell {...props} remove={remove} />
  );

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
          <GlVarianceReasonForm
              selectedAccountingCompanyId={selectedAccountingCompanyId}
              onSubmit={onSubmit}
            />

            <div className="div-container">
              <KendoGrid
                data={orderBy(
                  glAccountsVarianceReasons ? glAccountsVarianceReasons : [],
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
                  glAccountsVarianceReasons
                    ? glAccountsVarianceReasons.length
                    : 0
                }
                pageable={true}
                onPageChange={pageChange}
              >
                <Column
                    field="varianceReasonDescription"
                    title="Variance Reason"
                    width={300}
                />
                <Column field="glAccountId" title="Gl Account" width={350} />
                <Column cell={CommandCell} width="auto" />

              </KendoGrid>
            </div>
          </Paper>
        </Grid>
      </Grid>
    </>
  );
};

export default GlVarianceReason;
