import React, { useEffect, useState } from "react";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";

import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper } from "@mui/material";
import Button from "@mui/material/Button";
import { DataResult, State } from "@progress/kendo-data-query";
import {
  useFetchRoutingTasksQuery,
} from "../../../app/store/configureStore";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { handleDatesArray } from "../../../app/util/utils";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {WorkEffort} from "../../../app/models/manufacturing/workEffort";
import EditRoutingTask from "../form/EditRoutingTask";

function RoutingTasksList() {
  const [editMode, setEditMode] = useState(0);
  const initialDataState = { take: 6, skip: 0 };
  const [dataState, setDataState] = React.useState<State>(initialDataState);
  const dataStateChange = (e: GridDataStateChangeEvent) => {
    console.log("dataStateChange", e.dataState);
    setDataState(e.dataState);
  };
  const [routingTasks, setRoutingTasks] = useState<DataResult>({
    data: [],
    total: 0,
  });
  const [selectedRoutingTask, setSelectedRoutingTask] = useState<WorkEffort | undefined>(undefined);

  const { data, error, isFetching } = useFetchRoutingTasksQuery({
    ...dataState,
  });
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = 'manufacturing.routingTask.form';

  useEffect(() => {
    if (data) {
      const adjustedData = handleDatesArray(data.data);
      setRoutingTasks({ data: adjustedData, total: data.totalCount });
    }
  }, [data]);

  function handleSelectRoutingTask(workEffortId: string) {
    const selected = data?.data?.find((routingTask: WorkEffort) => routingTask.workEffortId === workEffortId);
    setSelectedRoutingTask(selected);
    setEditMode(2);
  }

  function cancelEdit() {
    setEditMode(0);
    setSelectedRoutingTask(undefined);
  }

  const RoutingIdCell = (props: any) => {
    const field = props.field || "";
    const value = props.dataItem[field];
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
        <td
            className={props.className}
            style={{ ...props.style, color: "blue" }}
            colSpan={props.colSpan}
            role="gridcell"
            aria-colindex={props.ariaColumnIndex}
            aria-selected={props.isSelected}
            {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
            {...navigationAttributes}
        >
          <Button onClick={() => handleSelectRoutingTask(props.dataItem.workEffortId)}>
            {props.dataItem.workEffortId}
          </Button>
        </td>
    );
  };

  return (
    <>
      <ManufacturingMenu selectedMenuItem={"routingtasks"} />
      {editMode === 0 ? (
          <Paper elevation={5} className="div-container-withBorderCurved">
            <Grid container columnSpacing={1} alignItems="center">
              <Grid item xs={12}>
                <div className="div-container">
                  <KendoGrid
                      style={{ height: "65vh", flex: 1 }}
                      data={routingTasks}
                      resizable={true}
                      filterable={true}
                      sortable={true}
                      pageable={true}
                      {...dataState}
                      onDataStateChange={(e: GridDataStateChangeEvent) => setDataState(e.dataState)}
                  >
                    <GridToolbar>
                      <Grid container>
                        <Grid item xs={4}>
                          <Button
                              color="secondary"
                              onClick={() => setEditMode(1)}
                              variant="outlined"
                          >
                            {getTranslatedLabel(
                                `${localizationKey}.createRoutingTask`,
                                "Create Routing Task"
                            )}
                          </Button>
                        </Grid>
                      </Grid>
                    </GridToolbar>
                    <Column
                        field="workEffortId"
                        title={getTranslatedLabel(`${localizationKey}.taskId`, "Task Id")}
                        cell={RoutingIdCell}
                        width={250}
                        locked={true}
                    />
                    <Column
                        field="workEffortName"
                        title={getTranslatedLabel(`${localizationKey}.routingName`, "Routing Name")}
                        width={250}
                    />
                    <Column
                        field="description"
                        title={getTranslatedLabel(`${localizationKey}.description`, "Description")}
                        width={250}
                    />
                    <Column
                        field="workEffortPurposeTypeDescription"
                        title={getTranslatedLabel(`${localizationKey}.taskType`, "Task Type")}
                        width={180}
                    />
                    <Column
                        field="fixedAssetId"
                        title={getTranslatedLabel(`${localizationKey}.fixedAssetId`, "Fixed Asset Id")}
                        width={250}
                    />
                    <Column
                        field="estimatedSetupMillis"
                        title={getTranslatedLabel(`${localizationKey}.estimatedSetupTime`, "Estimated Setup Time")}
                        width={120}
                    />
                    <Column
                        field="estimatedMilliSeconds"
                        title={getTranslatedLabel(`${localizationKey}.estimatedUnitRunTime`, "Estimated Unit Run Time")}
                        width={120}
                    />
                  </KendoGrid>
                  {isFetching && (
                      <LoadingComponent
                          message={getTranslatedLabel(`${localizationKey}.loading`, "Loading Routing Tasks...")}
                      />
                  )}
                </div>
              </Grid>
            </Grid>
          </Paper>
      ) : (
          <EditRoutingTask
              selectedRoutingTask={selectedRoutingTask}
              editMode={editMode}
              cancelEdit={cancelEdit}
          />
      )}
    </>
  );
}

export default RoutingTasksList;
