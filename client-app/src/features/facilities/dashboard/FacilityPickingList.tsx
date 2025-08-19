import FacilityMenu from "../menu/FacilityMenu";
import { Paper, Grid, Button, Typography } from "@mui/material";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GRID_COL_INDEX_ATTRIBUTE,
  GridToolbar,
} from "@progress/kendo-react-grid";
import {
  useCreatePickListMutation,
  useFetchFinishedProductFacilitiesQuery,
  useFetchOrdersToPickMoveQuery,
} from "../../../app/store/apis";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { toast } from "react-toastify";
import { useEffect, useState } from "react";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import { useAppSelector } from "../../../app/store/configureStore";

const FacilityPickingList = () => {
  const { data: facilities } =
    useFetchFinishedProductFacilitiesQuery(undefined);
  const [selectedFacility, setSelectedFacility] = useState<string | undefined>(
    undefined
  );
  const { data, isLoading, isFetching } = useFetchOrdersToPickMoveQuery(
    selectedFacility!,
    {
      skip: !selectedFacility,
    }
  );
  const {ordersForPickOrMoveStock} = useAppSelector(state => state.facilityInventoryUi)
  const [createPicklist, { isLoading: isCreateLoading }] =
    useCreatePickListMutation();

  const handleCreatePicklist = async (orderId: string) => {
    const selectedGroup = data?.find((g) => g.groupName === orderId);
    if (selectedGroup) {
      const createPicklistBody = {
        facilityId: selectedFacility,
        orderReadyToPickInfoList: selectedGroup.orderReadyToPickInfoList,
        orderNeedsStockMoveInfoList: selectedGroup.orderNeedsStockMoveInfoList,
      };
      try {
        await createPicklist(createPicklistBody);
        toast.success("Picklist created successfully");
      } catch (e) {
        console.log(e);
        toast.error("Something went wrong.");
      }
    }
  };

  const CreatPicklistCell = (props: any) => {
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
        {props.dataItem.readyToPick === "Y" ? (
          <Button
            onClick={() => {
              handleCreatePicklist(props.dataItem.orderId);
            }}
          >
            Create Picklist
          </Button>
        ) : (
          null
        )}
      </td>
    );
  };
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
              <KendoGrid style={{ height: "35vh" }} data={ordersForPickOrMoveStock ?? []}>
                <GridToolbar>
                  <Typography variant="body1">
                    Orders to pick move
                  </Typography>
                </GridToolbar>
                <Column field="orderId" title="Order" />
                <Column field="readyToPick" title="Ready to pick" />
                <Column field="needStockMove" title="Need stock move" />
                <Column cell={CreatPicklistCell} />
              </KendoGrid>
            </div>
          </Grid>
        </Grid>
        {(isLoading || isFetching) && (
          <LoadingComponent message="Loading Orders" />
        )}
        {isCreateLoading && <LoadingComponent message="Processing Picklist" />}
      </Paper>
    </>
  );
};

export default FacilityPickingList;
