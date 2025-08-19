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
import { CostComponentCalc } from "../../../../app/models/manufacturing/costComponentCalc";

interface Props {
    onClose: () => void;
    fohCosts: {
      data: CostComponentCalc[];
      total: number;
    };
  }

const FactoryOverheadCostList = ({onClose, fohCosts}: Props) => {
  console.log('fohCosts', fohCosts)
  const initialSort: Array<SortDescriptor> = [
    // { field: "thruDate", dir: "asc" },
    { field: "workEffortId", dir: "asc" },
  ];
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 10 };
  const [adjustedFohCosts, setAdjustedFohCosts] = useState({data: [], total: 0})
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };

  useEffect(() => {
    if (fohCosts) {
      const adjustedCosts = handleDatesArray(fohCosts.data)
      setAdjustedFohCosts({data: adjustedCosts, total: fohCosts.total})
    }
  }, [fohCosts]);

  const handleSelectOverheadCost = (costId: string) => {}

  const FactoryOverheadCell = (props: any) => {
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
            handleSelectOverheadCost(props.dataItem.costComponentTypeId);
          }}
        >
          {props.dataItem.workEffortName}
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
              Factory Overhead Costs
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
                total={fohCosts ? fohCosts.total : 0}
                sortable={true}
                pageable={true}
                onPageChange={pageChange}
                data={orderBy(
                  adjustedFohCosts ? adjustedFohCosts.data : [],
                  sort
                ).slice(page.skip, page.take + page.skip)}
              >
                <Column
                  field="workEffortName"
                  cell={FactoryOverheadCell}
                  title="Work Effort"
                  width={150}
                />
                <Column
                  field="costComponentTypeDescription"
                  title="Cost Component Type"
                  width={220}
                  reorderable={false}
                />
                <Column
                  field="description"
                  title="Description"
                  width={300}
                />
                {/* <Column field="productId" title="Product ID" width={200} /> */}
                {/* <Column
                  field="fixedCost"
                  title="Fixed Cost"
                  width={150}
                  format="{0:n2}"
                /> */}
                <Column
                  field="variableCost"
                  title="Variable Cost"
                  width={100}
                  format="{0:n2}"
                />
                <Column
                  field="permilliSecond"
                  title="Per Millisecond Cost"
                  width={120}
                  format="{0:n2}"
                />
                <Column
                  field="currencyUomId"
                  title="Currency UOM"
                  width={100}
                />
                {/* <Column
                  field="costCustomMethodId"
                  title="Cost Custom Method ID"
                  width={200}
                />
                <Column
                  field="fromDate"
                  title="From Date"
                  width={160}
                  format="{0: dd/MM/yyyy}"
                />
                <Column
                  field="thruDate"
                  title="Thru Date"
                  width={160}
                  format="{0: dd/MM/yyyy}"
                /> */}
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

export default FactoryOverheadCostList