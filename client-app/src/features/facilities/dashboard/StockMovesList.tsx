import FacilityMenu from "../menu/FacilityMenu";
import { Paper, Grid, Button, Typography } from "@mui/material";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GRID_COL_INDEX_ATTRIBUTE,
  GridToolbar,
} from "@progress/kendo-react-grid";
import {
  useFetchFinishedProductFacilitiesQuery,
  useFetchStockMovesNeededQuery,
} from "../../../app/store/apis";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { toast } from "react-toastify";
import { useEffect, useState } from "react";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import { useAppSelector } from "../../../app/store/configureStore";

const StockMovesList = () => {
  const { data: facilities } =
    useFetchFinishedProductFacilitiesQuery(undefined);
  const [selectedFacility, setSelectedFacility] = useState<string | undefined>(
    undefined
  );
  const {data: stockMoves, isLoading, isFetching} = useFetchStockMovesNeededQuery(selectedFacility!, {
    skip: !selectedFacility
  })
  
  return (
    <>
      <FacilityMenu />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item container xs={12} ml={1}>
            <Grid item xs={4}>
              <Form
                render={() => (
                  <FormElement>
                    <Field
                      id="facilityId"
                      name="facilityId"
                      label="Facility"
                      component={MemoizedFormDropDownList2}
                      data={facilities ?? []}
                      dataItemKey="facilityId"
                      textField="facilityName"
                      onChange={(e) => setSelectedFacility(e.value)}
                    />
                  </FormElement>
                )}
              />
            </Grid>
          </Grid>
          <Grid item xs={12}>
            <div className="div-container">
              <KendoGrid style={{ height: "35vh" }} data={stockMoves?.moveByOisgirInfoList ?? []}>
                
              </KendoGrid>
            </div>
          </Grid>
        </Grid>
        {(isLoading || isFetching) && (
          <LoadingComponent message="Loading Stock Moves..." />
        )}
        {/* {isCreateLoading && <LoadingComponent message="Processing Picklist" />} */}
      </Paper>
    </>
  );
};

export default StockMovesList;
