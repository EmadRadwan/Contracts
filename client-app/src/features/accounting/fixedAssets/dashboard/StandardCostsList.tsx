import React, { useState } from "react";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import {
  Grid as KendoGrid,
  GridCellProps,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
  GridToolbar,
  GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import { State } from "@progress/kendo-data-query";
import StandardCostsForm from "../form/StandardCostsForm";
import { useFetchFixedAssetStandardCostsQuery } from "../../../../app/store/apis";
import { useAppSelector } from "../../../../app/store/configureStore";
import CreateFixedAssetMenu from "../CreateFixedAssetMenu";
import { useNavigate } from "react-router";
import { handleDatesArray } from "../../../../app/util/utils";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";

const StandardCostsList = () => {
  const navigate = useNavigate();
  const [editMode, setEditMode] = useState(0);
  const [selectedStandardCost, setSelectedStandardCost] = useState(null);

  const initialDataState: State = { skip: 0, take: 9 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };

  const selectedFixedAsset = useAppSelector(
    (state) => state.accountingSharedUi.selectedFixedAsset
  );
  if (!selectedFixedAsset) {
    navigate("/fixedAssets")
  }

  const { data: standardCosts } = useFetchFixedAssetStandardCostsQuery(
    selectedFixedAsset?.fixedAssetId!
  );
  console.log(standardCosts);

  function handleBackClick() {
    navigate("/fixedAssets", { state: { myStateProp: "bar" } });
  }
  const cancelEdit = () => {
    setSelectedStandardCost(null)
    setEditMode(0)
  }

  const handleSelectStandardCost = (costId: string) => {
    const selectedCost = standardCosts!.find((c: any) => c.fixedAssetStdCostTypeId === costId)
    setSelectedStandardCost(selectedCost)
    setEditMode(2)
  }

  const StandardCostNameCell = (props: any) => {
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
        <Button onClick={() => handleSelectStandardCost(props.dataItem.fixedAssetStdCostTypeId)}>
          {props.dataItem.cleanCostName}
        </Button>
      </td>
    );
  };

  if (editMode) {
    return (
      <StandardCostsForm
        selectedFixedAsset={selectedFixedAsset!}
        editMode={editMode}
        cancelEdit={cancelEdit}
        selectedCost={selectedStandardCost}
      />
    );
  }

  return (
    <>
      <AccountingMenu selectedMenuItem={"fixedAssets"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container spacing={2} mb={3}>
          <Grid item xs={5}>
            {
              <Box display="flex" justifyContent="space-between">
                <Typography
                  color={selectedFixedAsset?.fixedAssetName ? "black" : "green"}
                  sx={{ p: 2 }}
                  variant="h4"
                >
                  {" "}
                  {selectedFixedAsset?.fixedAssetName
                    && `Standard Cost for ${selectedFixedAsset.fixedAssetName}`}
                </Typography>
              </Box>
            }
          </Grid>

          <Grid item xs={7}>
            <CreateFixedAssetMenu />
          </Grid>
        </Grid>
        {/* <StandardCostsForm selectedFixedAsset={selectedFixedAsset!} /> */}
        <div className="div-container">
          <KendoGrid
            data={standardCosts ? handleDatesArray(standardCosts) : []}
            sortable={true}
            skip={page.skip}
            take={page.take}
            total={0}
            pageable={true}
            onPageChange={pageChange}
          >
            <GridToolbar>
              <Grid container>
                <Grid item xs={4}>
                  <Button variant="outlined" color={"secondary"} onClick={() => setEditMode(1)}>
                    Create Standard Cost
                  </Button>
                </Grid>
              </Grid>
            </GridToolbar>
            <Column field="cleanCostName" title="Standard Cost" cell={StandardCostNameCell} />
            <Column field="amount" title="Amount" />
            <Column field="amountUomId" title="Currency" />
            <Column
              field="fromDate"
              title="From Date"
              format="{0: dd/MM/yyyy}"
            />
            <Column
              field="thruDate"
              title="Through date"
              format="{0: dd/MM/yyyy}"
            />
          </KendoGrid>
        <Grid container mt={2}>
          <Grid item xs={4}>
            <Button
              variant="contained"
              type={"button"}
              color="error"
              onClick={handleBackClick}
            >
              Back
            </Button>
          </Grid>
        </Grid>
        </div>
      </Paper>
    </>
  );
};

export default StandardCostsList;
