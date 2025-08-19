import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { Fragment, useEffect, useState } from "react";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import { Grid, Paper, Typography } from "@mui/material";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { handleDatesArray } from "../../../../app/util/utils";

interface Props {
    onClose: () => void;
    materialCosts: {
      data: any[];
      total: number;
    };
  }

const ProductMaterialCostList = ({materialCosts, onClose}: Props) => {
  console.log(materialCosts)
  const initialSort: Array<SortDescriptor> = [
    { field: "thruDate", dir: "asc" },
    { field: "productId", dir: "asc" },
  ];
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 10 };
  const [adjustedMaterialCosts, setAdjustedMaterialCosts] = useState({data: [], total: 0})
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };

  useEffect(() => {
    if (materialCosts) {
      const adjustedCosts = handleDatesArray(materialCosts.data)
      setAdjustedMaterialCosts({data: adjustedCosts, total: materialCosts.total})
    }
  }, [materialCosts]);

  const handleSelectRoutingCost = (costId: string) => {}

  const FixedCostCell = (props: any) => {
    const field = props.field || "";
    const value = props.dataItem[field];
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
      <td
        className={props.className}
        style={{ ...props.style, color: "blue" }}
        colSpan={props.colSpan}
        role={"gridcell"}
        aria-colindex={props.ariaColumnIndex}
        aria-selected={props.isSelected}
        {...{
          [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
        }}
        {...navigationAttributes}
      >
        <Button
          onClick={() => {
            handleSelectRoutingCost(props.dataItem.productId);
          }}
        >
          {props.dataItem.productName}
        </Button>
      </td>
    );
  };
  return (
    <Fragment>
    <Paper elevation={5} className={`div-container-withBorderCurved`}>
      <Grid container padding={2} columnSpacing={1}>
        <Grid item xs={6}>
          <Typography sx={{ p: 2, fontWeight: "bold" }} variant="h6">
            Material Costs
          </Typography>
        </Grid>
        <Grid container>
          <div className="div-container" style={{ overflow: "auto" }}>
            <KendoGrid
              style={{ height: "35vh", width: "100%" }}
              resizable={true}
              sort={sort}
              onSortChange={(e: GridSortChangeEvent) => {
                setSort(e.sort);
              }}
              skip={page.skip}
              take={page.take}
              total={materialCosts ? materialCosts.total : 0}
              sortable={true}
              pageable={true}
              onPageChange={pageChange}
              data={orderBy(
                adjustedMaterialCosts ? adjustedMaterialCosts.data : [],
                sort
              ).slice(page.skip, page.take + page.skip)}
            >
              <Column
                field="productId"
                cell={FixedCostCell}
                title="Material"
                width={230}
                reorderable={false}
              />
              <Column
                field="costComponentTypeDescription"
                title="Cost Component Type"
                width={180}
              />
              <Column
                field="estimatedUnitCost"
                title="Estimated Unit Cost"
                width={100}
                format="{0:n2}"
              />
              <Column
                field="costUomId"
                title="Cost UOM"
                width={120}
              />
              <Column
                field="quantity"
                title="Quantity"
                width={100}
                format="{0:n2}"
              />
              <Column
                field="uomId"
                title="UOM"
                width={120}
              />
              <Column
                field="fromDate"
                title="From Date"
                width={100}
                format="{0: dd/MM/yyyy}"
              />
              <Column
                field="thruDate"
                title="Thru Date"
                width={100}
                format="{0: dd/MM/yyyy}"
              />
            </KendoGrid>
          </div>
        </Grid>
      </Grid>
      <Grid item xs={2}>
        <Button onClick={() => onClose()} color="error" variant="contained">
          Close
        </Button>
      </Grid>
    </Paper>
  </Fragment>
)
}

export default ProductMaterialCostList