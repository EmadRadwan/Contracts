import { useFetchFacilityInventoriesByProductQuery } from "../../../app/store/apis";
import React, { useEffect, useState } from "react";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridCellProps,
  GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import {
  selectProductById,
  setSelectedProductName,
} from "../slice/facilityInventoryUiSlice";
import { Grid, Paper, useMediaQuery, useTheme } from "@mui/material";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { router } from "../../../app/router/Routes";
import { FacilityInventory } from "../../../app/models/facility/facilityInventory";
import {
  useAppDispatch,
  useAppSelector,
} from "../../../app/store/configureStore";
import FacilityMenu from "../menu/FacilityMenu";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { State } from "@progress/kendo-data-query";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

export default function FacilityInventoryList() {
  const { getTranslatedLabel } = useTranslationHelper();

  const { selectedProduct } = useAppSelector((state) => state.productUi);
  const initialDataState: State = {
    take: 6,
    skip: 0,
  };
  const [dataState, setDataState] = React.useState<State>(initialDataState);
  const [isDataStateInitial, setIsDataStateInitial] = useState(true)
  useEffect(() => {
    const initial = Object.keys(dataState).length === Object.values(initialDataState).length
    setIsDataStateInitial(initial)
  }, [dataState])
  useEffect(() => {
    if (selectedProduct) {
      setDataState({
        filter: {
          logic: "and",
          filters: [
            {
              field: "productId",
              operator: "equals",
              value: selectedProduct?.productId,
            },
          ],
        },
        take: 6,
        skip: 0
      });
    }
  }, [selectedProduct]);

  const handleResetFiltration = () => {
    setDataState(initialDataState)
  }

  const [show, setShow] = useState(false);

  const dataStateChange = (e: GridDataStateChangeEvent) => {
    console.log("dataStateChange", e.dataState);
    setDataState({ ...e.dataState, take: dataState.take });
  };

  const dispatch = useAppDispatch();

  const { data, isFetching } = useFetchFacilityInventoriesByProductQuery({
    ...dataState,
  });

  function handleSelectInventory(dataItem: FacilityInventory) {
    dispatch(selectProductById(dataItem.productId));
    dispatch(setSelectedProductName(dataItem.productName));
    router.navigate("/inventoryItems");
  }

  const InventoryDescriptionCell = (props: any) => {
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
            console.log(props.dataItem);
            handleSelectInventory(props.dataItem);
          }}
        >
          {props.dataItem.productName}
        </Button>
      </td>
    );
  };

  const DetailsCell = (props: GridCellProps) => (
    <NavigateToDetailsCell {...props} />
  );

  const goToDetails = (dataItem: any) => {
    const { productId, productName } = dataItem;
    dispatch(selectProductById(productId));
    dispatch(setSelectedProductName(productName));
    router.navigate("/inventoryItemDetails");
  };

  const NavigateToDetailsCell = (props: any) => {
    // console.log('dataItem from TransferInventoryItemCell', dataItem);
    return (
      <td className="k-command-cell">
        <Button
          className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
          onClick={() => goToDetails(props.dataItem)}
        >
          {getTranslatedLabel("general.details", "Details")}
        </Button>
      </td>
    );
  };

  return (
    <>
      <FacilityMenu />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item xs={12}>
            <div className="div-container">
              <KendoGrid
                style={{ height: "65vh" }}
                resizable={true}
                filterable={true}
                sortable={true}
                pageable={true}
                {...dataState}
                data={data ? data : { data: [], total: 77 }}
                onDataStateChange={dataStateChange}
              >
                <GridToolbar>
                    <Grid container justifyContent={"flex-end"}>
                        <Grid item>
                            <Button disabled={isDataStateInitial} variant="outlined" color="secondary" onClick={handleResetFiltration}>
                                {getTranslatedLabel("general.clear", "Clear")}
                            </Button>
                        </Grid>
                    </Grid>
                </GridToolbar>
                <Column
                  field="productId"
                  title={getTranslatedLabel("facility.list.product", "Product")}
                  width={0}
                  locked={!show}
                />
                <Column
                  field="productName"
                  title={getTranslatedLabel("facility.list.product", "Product")}
                  width={200}
                  locked={!show}
                  cell={InventoryDescriptionCell}
                />
                <Column
                  field="facilityName"
                  title={getTranslatedLabel(
                    "facility.list.facility",
                    "Facility"
                  )}
                  width={200}
                />
                <Column
                  field="quantityOnHandTotal"
                  title={getTranslatedLabel("facility.list.qoh", "QOH Total")}
                />
                <Column
                  field="availableToPromiseTotal"
                  title={getTranslatedLabel("facility.list.atp", "ATP Total")}
                />
                <Column
                  field="minimumStock"
                  title={getTranslatedLabel(
                    "facility.list.minStock",
                    "Minimum Stock"
                  )}
                />
                <Column
                  field="availableToPromiseMinusMinimumStock"
                  title={getTranslatedLabel(
                    "facility.list.atpMinusMin",
                    "ATP Minus Minimum Stock"
                  )}
                />
                <Column
                  field="quantityOnHandMinusMinimumStock"
                  title={getTranslatedLabel(
                    "facility.list.qohMinusMin",
                    "QOH Minus Minimum Stock"
                  )}
                />
                <Column
                  field="reorderQuantity"
                  title={getTranslatedLabel(
                    "facility.list.reorder",
                    "Reorder Quantity"
                  )}
                />
                <Column
                  field="quantityOnOrder"
                  title={getTranslatedLabel(
                    "facility.list.ordered",
                    "Ordered Quantity"
                  )}
                />
                <Column
                  field="defaultPrice"
                  title={getTranslatedLabel(
                    "facility.list.price",
                    "List Price"
                  )}
                />
                <Column
                  cell={DetailsCell}
                  width={100}
                  filterable={false}
                  sortable={false}
                />
              </KendoGrid>
            </div>
          </Grid>
        </Grid>
        {isFetching && (
          <LoadingComponent
            message={getTranslatedLabel(
              "facility.list.loading",
              "Loading Inventory..."
            )}
          />
        )}
      </Paper>
    </>
  );
}
