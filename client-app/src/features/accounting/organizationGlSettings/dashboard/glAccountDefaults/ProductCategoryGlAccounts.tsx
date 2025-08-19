import React from "react";
import GlAccountDefaults from "./GlAccountDefaults";
import { Grid, Paper } from "@mui/material";
import {
  Grid as KendoGrid,
  GridCellProps,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import { SortDescriptor, State, orderBy } from "@progress/kendo-data-query";
import {
  useAppSelector,
  useFetchProductCategoryGlAccountsQuery,
} from "../../../../../app/store/configureStore";
import { router } from "../../../../../app/router/Routes";
import { toast } from "react-toastify";
import ProductCategoryGlAccountsForm from "../../form/ProductCategoryGlAccountsForm";

const ProductCategoryGlAccounts = () => {
  const selectedAccountingCompanyId = useAppSelector(
    (state) => state.accountingSharedUi.selectedAccountingCompanyId
  );
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }
  const initialSort: Array<SortDescriptor> = [
    { field: "partyId", dir: "desc" },
  ];
  const { data: productCategoryGlAccounts } =
    useFetchProductCategoryGlAccountsQuery(selectedAccountingCompanyId, {
      skip: selectedAccountingCompanyId === undefined,
    });
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 9 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };
  const onSubmit = async (values: any) => {
    console.log(values);

    if (Object.values(values).some((v) => v === undefined)) {
      toast.warn(
        "Please select an account, a product category, and an account type."
      );
      return;
    }
  };
  return (
    <>
      <GlAccountDefaults />
      <Grid container padding={2} columnSpacing={1} justifyContent={"center"}>
        <Grid item xs={8}>
          <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <ProductCategoryGlAccountsForm
              selectedAccountingCompanyId={selectedAccountingCompanyId}
              onSubmit={onSubmit}
            />

            <div className="div-container">
              <KendoGrid
                data={orderBy(
                  productCategoryGlAccounts ? productCategoryGlAccounts : [],
                  sort
                ).slice(page.skip, page.take + page.skip)}
                sortable={true}
                sort={sort}
                onSortChange={(e: GridSortChangeEvent) => {
                  setSort(e.sort);
                }}
                skip={page.skip}
                take={page.take}
                total={0}
                pageable={true}
                onPageChange={pageChange}
              >
                <Column
                  field="glAccountTypeDescription"
                  title="GL Account Type"
                  width={300}
                />
                <Column field="glAccountId" title="Gl Account" width={350} />
              </KendoGrid>
            </div>
          </Paper>
        </Grid>
      </Grid>
    </>
  );
};

export default ProductCategoryGlAccounts;
