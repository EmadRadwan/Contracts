import React, { useRef } from "react";
import AccountingMenu from "../../../invoice/menu/AccountingMenu";
import { Button, Grid, Paper } from "@mui/material";
import GlSettingsMenu from "../../menu/GlSettingsMenu";
import { ExcelExport } from "@progress/kendo-react-excel-export";
import {
  Grid as KendoGrid,
  GridCellProps,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { useFetchFXRatesQuery } from "../../../../../app/store/configureStore";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import ForeignExchangeRatesForm from "../form/ForeignExchangeRatesForm";

const ForeignExchangeRatesList = () => {
  const { data: fxRates } = useFetchFXRatesQuery(undefined);

  const _export = useRef(null);
  const excelExport = () => {
    if (_export.current !== null) {
      // @ts-ignore
      _export.current!.save();
    }
  };
  const initialSort: Array<SortDescriptor> = [{ field: "uomId", dir: "desc" }];
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 9 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };
  let dataToExport = fxRates ? fxRates : [];
  return (
    <>
      <AccountingMenu selectedMenuItem="globalGL" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <GlSettingsMenu />
        {/* <ForeignExchangeRatesForm /> */}
        <Grid container spacing={1} alignItems={"center"}>
          <Grid container p={1}>
            <div className="div-container">
              <ExcelExport data={dataToExport} ref={_export}>
                <KendoGrid
                  data={orderBy(fxRates ? fxRates : [], sort).slice(
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
                  total={fxRates ? fxRates.length : 0}
                  pageable={true}
                  onPageChange={pageChange}
                >
                  <GridToolbar>
                    <Grid container>
                      <Grid item xs={4}>
                        <Button color={"secondary"} variant="outlined">
                          Create New Exchange Rate
                        </Button>
                      </Grid>
                    </Grid>
                  </GridToolbar>
                  <Column field="uomId" title="From Currency" />
                  <Column field="uomIdTo" title="To Currency" />
                  <Column field="conversionFactor" title="Rate" />
                  <Column
                    field="fromDate"
                    title="From Date"
                    format="{0: dd/MM/yyyy}"
                  />
                </KendoGrid>
              </ExcelExport>
            </div>
          </Grid>
        </Grid>
      </Paper>
    </>
  );
};

export default ForeignExchangeRatesList;
