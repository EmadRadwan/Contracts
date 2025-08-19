import React from "react";
import { useAppSelector } from "../../../../../app/store/configureStore";
import { router } from "../../../../../app/router/Routes";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import {
  GridPageChangeEvent,
  GridSortChangeEvent,
  GridColumn as Column,
  Grid as KendoGrid,
} from "@progress/kendo-react-grid";
import GlAccountDefaults from "./GlAccountDefaults";
import { Grid, Paper } from "@mui/material";
import FixedAssetTypeGlMappingsForm from "../../form/FixedAssetTypeGlMappingsForm";

const FixedAssetTypeGlMappings = () => {
  const selectedAccountingCompanyId = useAppSelector(
    (state) => state.accountingSharedUi.selectedAccountingCompanyId
  );
  if (!selectedAccountingCompanyId) {
    router.navigate("/orgGl");
  }
  const initialSort: Array<SortDescriptor> = [
    { field: "fixedAssetId", dir: "desc" },
  ];
  // TODO: Add query to get grid data
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 9 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };
  const onSubmit = async (values: any) => {
    console.log(values);

    if (Object.values(values).some((v) => v === undefined)) {
      return;
    }
  };
  return (
    <>
      <GlAccountDefaults />
      <Grid container padding={2} columnSpacing={1} justifyContent={"center"}>
        <Grid item xs={8}>
          <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <FixedAssetTypeGlMappingsForm
              selectedAccountingCompanyId={selectedAccountingCompanyId}
              onSubmit={onSubmit}
            />

            <div className="div-container">
              <KendoGrid
                data={orderBy([], sort).slice(page.skip, page.take + page.skip)}
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
                <Column field="fixedAssetTypeId" title="Fixed Asset Type" />
                <Column field="fixedAssetId" title="Fixed Asset" />
                <Column field="assetGlAccountId" title="Asset Gl Account" />
                <Column field="accumulatedDepreciationGlAccountId" title="Accumulated depreciation GL account" />
                <Column field="depreciationGlAccountId" title="Depreciation GL account" />
                <Column field="profitGlAccountId" title="Profit GL account" />
                <Column field="lossGlAccountId" title="Loss GL account" />
              </KendoGrid>
            </div>
          </Paper>
        </Grid>
      </Grid>
    </>
  );
};

export default FixedAssetTypeGlMappings;
