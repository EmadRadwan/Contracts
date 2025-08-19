import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { useEffect, useState } from "react";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { Grid, Typography } from "@mui/material";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useFetchProducedProductionRunInventoryQuery } from "../../../app/store/apis";
import { handleDatesArray } from "../../../app/util/utils";

interface Props {
    productionRunId?: string | undefined;
}

export default function ProductionRunProducedInventoryList({
    productionRunId
  }: Props) {
    const initialSort: Array<SortDescriptor> = [
        { field: "productName", dir: "asc" },
      ];
      const [sort, setSort] = React.useState(initialSort);
      const initialDataState: State = { skip: 0, take: 10 };
      const [page, setPage] = React.useState<any>(initialDataState);
      const [inventoryItems, setInventoryItems] = useState([])
      const { getTranslatedLabel } = useTranslationHelper();
    
      const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
      };

      const {data} = useFetchProducedProductionRunInventoryQuery(productionRunId, {
        skip: !productionRunId
      })

      useEffect(() => {
        if (data) {
          setInventoryItems(handleDatesArray(data))
        }
      }, [data])

      return (
        <KendoGrid
            style={{ height: "400px" }}
            data={orderBy(
              inventoryItems ?? [],
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
            resizable={true}
          >
            <GridToolbar>
              <Grid container justifyContent={"center"}>
                  <Grid item xs={12}>
                      <Typography
                        color="primary"
                        sx={{ fontSize: "18px", color: "blue", fontWeight: "bold", textAlign: "center" }}
                        variant="h6"
                      >
                        {getTranslatedLabel(
                          "manufacturing.jobshop.inventory.title",
                          "Produced Inventory"
                        )}
                      </Typography>
                  </Grid>
              </Grid>
            </GridToolbar>
            <Column 
              field="productName"
              title="Product"
            />
            <Column 
              field="inventoryItemId"
              title="Inventory Item"
            />
            <Column 
              field="facilityName"
              title="Facility"
            /> 
            <Column 
              field="unitCost"
              title="Unit Cost"
            />
            <Column 
              field="datetimeManufactured"
              title="Date Manufactured"
              format="{0: dd/MM/yyyy HH:mm}"
            />
            <Column 
              field="quantityOnHandTotal"
              title="QOH Total"
            />
            <Column 
              field="availableToPromiseTotal"
              title="ATP Total"
            />
        </KendoGrid>
      )
  }