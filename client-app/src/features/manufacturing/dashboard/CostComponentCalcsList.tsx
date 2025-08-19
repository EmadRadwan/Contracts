import React, { useState } from "react";
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
import { State } from "@progress/kendo-data-query";
import {
  useFetchCostComponentCalcsQuery,
} from "../../../app/store/configureStore";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {CostComponentCalc} from "../../../app/models/manufacturing/costComponentCalc";
import {EditCostComponentCalc} from "../form/EditCostComponentCalc";

function CostComponentCalcList() {
const [editMode, setEditMode] = useState(0)
  const [dataState, setDataState] = React.useState<State>({ take: 9, skip: 0 });
  const { getTranslatedLabel } = useTranslationHelper();
  const [selectedCostComponentCalc, setSelectedCostComponentCalc] = useState<CostComponentCalc | undefined>(undefined);
  const localizationKey = "accounting.invoices.display.form";

  const dataStateChange = (e: GridDataStateChangeEvent) => {
    setDataState(e.dataState);
  };
  const { data, isFetching } = useFetchCostComponentCalcsQuery({
    ...dataState,
  });

  const handleSelectCostComponentCalc = (costComponentCalcId: string) => {
    const selected = data?.data?.find(
        (calc: CostComponentCalc) => calc.costComponentCalcId === costComponentCalcId
    );
    setSelectedCostComponentCalc(selected);
    setEditMode(2);
  };

  const cancelEdit = () => {
    setEditMode(0);
    setSelectedCostComponentCalc(undefined);
  };

  const CostComponentCalcIdCell = (props: any) => {
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
          onClick={() =>
            handleSelectCostComponentCalc(props.dataItem.costComponentCalcId)
          }
        >
          {props.dataItem.costComponentCalcId}
        </Button>
      </td>
    );
  };

  return (
      <>
        <ManufacturingMenu selectedMenuItem="costs" />
        {editMode === 0 ? (
            <Paper elevation={5} className="div-container-withBorderCurved">
              <Grid container columnSpacing={1} alignItems="center">
                <Grid item xs={12}>
                  <div className="div-container">
                    <KendoGrid
                        style={{ height: '65vh', flex: 1 }}
                        data={data ? { data: data.data, total: data.total } : { data: [], total: 0 }}
                        resizable={true}
                        filterable={true}
                        sortable={true}
                        pageable={true}
                        {...dataState}
                        onDataStateChange={dataStateChange}
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
                                  `${localizationKey}.createCostComponentCalculation`,
                                  'Create Cost Component Calculation'
                              )}
                            </Button>
                          </Grid>
                        </Grid>
                      </GridToolbar>
                      <Column
                          field="costComponentCalcId"
                          title={getTranslatedLabel(`${localizationKey}.costComponentCalcId`, 'Routing Id')}
                          cell={CostComponentCalcIdCell}
                          width={200}
                          locked={true}
                      />
                      <Column
                          field="description"
                          title={getTranslatedLabel(`${localizationKey}.description`, 'Description')}
                          width={350}
                      />
                      <Column
                          field="fixedCost"
                          title={getTranslatedLabel(`${localizationKey}.fixedCost`, 'Fixed Cost')}
                          width={150}
                      />
                      <Column
                          field="variableCost"
                          title={getTranslatedLabel(`${localizationKey}.variableCost`, 'Variable Cost')}
                          width={150}
                      />
                      <Column
                          field="perMilliSecond"
                          title={getTranslatedLabel(`${localizationKey}.perMilliSecond`, 'Per Milli Second')}
                          width={150}
                      />
                      <Column
                          field="currencyUomId"
                          title={getTranslatedLabel(`${localizationKey}.currency`, 'Currency')}
                          width={250}
                      />
                    </KendoGrid>
                    {isFetching && (
                        <LoadingComponent
                            message={getTranslatedLabel(`${localizationKey}.loading`, 'Loading Cost Components...')}
                        />
                    )}
                  </div>
                </Grid>
              </Grid>
            </Paper>
        ) : (
            <EditCostComponentCalc
                selectedCostComponentCalc={selectedCostComponentCalc}
                editMode={editMode}
                cancelEdit={cancelEdit}
            />
        )}
      </>
  );
}

export default CostComponentCalcList;
