import React, { useCallback, useEffect, useState } from "react";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridToolbar,
  GridSortChangeEvent,
  GridPageChangeEvent,
} from "@progress/kendo-react-grid";
import {
  DataResult,
  orderBy,
  SortDescriptor,
  State,
} from "@progress/kendo-data-query";
import {
  useAppSelector,
  useCalculateProductCostsMutation,
  useFetchCostComponentsQuery
} from "../../../../app/store/configureStore";
import { Navigate, useNavigate } from "react-router";
import { Grid, Paper, Typography } from "@mui/material";
import Button from "@mui/material/Button";
import Badge from "@mui/material/Badge";
import CatalogMenu from "../../menu/CatalogMenu";
import { handleDatesArray } from "../../../../app/util/utils";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import { toast } from "react-toastify";
import { LoadingButton } from "@mui/lab";
import { useLazyFetchFOHCostQuery, useLazyFetchLaborCostQuery, useLazyFetchMaterialCostQuery } from "../../../../app/store/apis";
import ProductMaterialCostList from "./ProductMaterialCostList";
import LaborCostCalculationsList from "./LaborCostCalculationsList";
import FactoryOverheadCostList from "./FactoryOverheadCostList";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import {EditCostComponent} from "../../../manufacturing/form/EditCostComponent";

interface CostComponent {
  costComponentId: string | null;
  productId: string;
  costComponentTypeId: string;
  productFeatureId: string | null;
  partyId: string | null;
  geoId: string | null;
  workEffortId: string | null;
  fixedAssetId: string | null;
  costComponentCalcId: string | null;
  costUomId: string;
  cost: number | null;
  fromDate: Date | null;
  thruDate: Date | null;
  costComponentTypeDescription: string;
}

export default function ProductCostsList() {
  const selectedProduct = useAppSelector(
    (state) => state.productUi.selectedProduct
  );
  const navigate = useNavigate();
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "product.costs.form";
  const [costComponents, setCostComponents] = React.useState<DataResult>({
    data: [],
    total: 0,
  });
  const [editMode, setEditMode] = useState(0); // REFACTOR: Added state for edit mode (0: list, 1: create, 2: edit)
  const [selectedCostComponent, setSelectedCostComponent] = useState<CostComponent | undefined>(undefined); // REFACTOR: Added state for selected cost component

  const [
    calculateProductCosts,
    { data: costsResults, error, isLoading: isCalculateCostsLoading },
  ] = useCalculateProductCostsMutation();

  const { data: ccData, isFetching, isSuccess, isError } = useFetchCostComponentsQuery(
    selectedProduct?.productId,
    {
      skip:
        !selectedProduct ||
        !selectedProduct.productId ||
        selectedProduct.productId === undefined,
    }
  );

  const [triggerMaterial, { data: materialCosts, isFetching: isMaterialCostsFetching }] = useLazyFetchMaterialCostQuery();
  
    const [triggerLabor, { data: laborCosts, isFetching: isLaborCostsFetching }] = useLazyFetchLaborCostQuery();
  
    const [triggerFOH, { data: fohCosts, isFetching: isFOHCostsFetching }] = useLazyFetchFOHCostQuery();
  

  const [showLaborCost, setShowLaborCost] = useState(false);
  const [showMaterialCost, setShowMaterialCost] = useState(false);
  const [showFOHCost, setShowFOHCost] = useState(false);

  const initialSort: Array<SortDescriptor> = [
      { field: "thruDate", dir: "asc" },
    { field: "costComponentId", dir: "asc" },
  ];
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 9 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };
  
  console.log("ccData", ccData);

  useEffect(() => {
    if (ccData?.data?.costComponents) {
      const adjustedData = handleDatesArray(ccData?.data.costComponents);
      setCostComponents({ data: adjustedData, total: adjustedData.length });
    }
  }, [ccData]);
  

  function handleBackClick() {
    navigate("/products", { state: { myStateProp: "bar" } });
  }

  if (!selectedProduct) {
    return <Navigate to="/products" />;
  }

  function handleCostComponent(costComponentId: string) {
    const selected = ccData?.data.find((costComponent: CostComponent) => costComponent.costComponentId === costComponentId);
    setSelectedCostComponent(selected);
    setEditMode(2);
  }

  const cancelEdit = () => {
    setEditMode(0);
    setSelectedCostComponent(undefined);
  };
  
  const CostComponentCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    const isValid = !props.dataItem.thruDate
    console.log(props.dataItem.costComponentTypeDescription, props.dataItem.thruDate, isValid)
    return (
      <td
        className={props.className}
        style={{ ...props.style, color: "black" }}
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
          disableFocusRipple={!isValid}
          disableElevation={!isValid}
          disableRipple={!isValid}
          disableTouchRipple={!isValid}
          style={{color: (!isValid && "black"), pointerEvents: (!isValid && "none")}}
          onClick={() => {
            handleCostComponent(props.dataItem.costComponentId);
          }}
        >
          {props.dataItem.costComponentTypeDescription}
        </Button>
      </td>
    );
  };

  const memoizedOnClose = useCallback(() => {
    setShowLaborCost(false);
    setShowMaterialCost(false);
    setShowFOHCost(false)
  }, []);

  async function handleSubmitData() {
    try {
      const calculatedCosts = await calculateProductCosts(
        selectedProduct!.productId
      ).unwrap();
    } catch (error) {
      toast.error("Failed to calculate costs for product.");
      console.log(error);
    }
  }

  // On button click, we trigger the lazy query for detail data, then show the modal
  function handleShowLaborModal() {
    triggerLabor(selectedProduct?.productId);
    setShowLaborCost(true);
  }
  function handleShowFOHModal() {
    triggerFOH(selectedProduct?.productId);
    setShowFOHCost(true);
  }
  function handleShowMaterialModal() {
    triggerMaterial(selectedProduct?.productId);
    setShowMaterialCost(true);
  }

  // The aggregator counts from ccData
  const directLaborCount = ccData?.data?.directLaborCount ?? 0;
  const fohCount = ccData?.data?.fohCostCount ?? 0;
  const materialCount = ccData?.data?.materialCostCount ?? 0;


  return (
    <>
      {showLaborCost && (
        <ModalContainer show={showLaborCost} onClose={memoizedOnClose} width={950}>
          <LaborCostCalculationsList laborCosts={laborCosts!} onClose={() => setShowLaborCost(false)} />
        </ModalContainer>
      )}
      {showMaterialCost && (
        <ModalContainer show={showMaterialCost} onClose={memoizedOnClose} width={950}>
          <ProductMaterialCostList materialCosts={materialCosts!} onClose={() => setShowMaterialCost(false)} />
        </ModalContainer>
      )}
      {showFOHCost && (
        <ModalContainer show={showFOHCost} onClose={memoizedOnClose} width={950}>
          <FactoryOverheadCostList fohCosts={fohCosts!} onClose={() => setShowFOHCost(false)} />
        </ModalContainer>
      )}
      <CatalogMenu selectedMenuItem='/products' />

      {editMode === 0 ? (
          <Paper elevation={5} className="div-container-withBorderCurved">
            <Grid container columnSpacing={1}>
              <Grid container alignItems="center">
                <Grid item xs={8}>
                  <Typography sx={{ px: 4, py: 1 }} variant="h4">
                    {getTranslatedLabel(`${localizationKey}.title`, `Costs for ${selectedProduct.productName}`)}
                  </Typography>
                </Grid>
              </Grid>
              <Grid item xs={12}>
                <div className="div-container">
                  <KendoGrid
                      style={{ height: "55vh" }}
                      resizable={true}
                      sort={sort}
                      onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                      skip={page.skip}
                      take={page.take}
                      total={costComponents.total}
                      sortable={true}
                      pageable={true}
                      onPageChange={pageChange}
                      data={orderBy(costComponents.data, sort).slice(page.skip, page.take + page.skip)}
                  >
                    <GridToolbar>
                      <Grid container py={1} spacing={2}>
                        <Grid item>
                          <Button
                              color="success"
                              onClick={() => handleSubmitData()}
                              variant="contained"
                              type="submit"
                          >
                            {getTranslatedLabel(`${localizationKey}.calculateCosts`, "Calculate Costs")}
                          </Button>
                        </Grid>
                        {/* REFACTOR: Added button to create new cost component */}
                        <Grid item>
                          <Button
                              color="secondary"
                              onClick={() => setEditMode(1)}
                              variant="outlined"
                          >
                            {getTranslatedLabel(`${localizationKey}.createCostComponent`, "Create Cost Component")}
                          </Button>
                        </Grid>
                        <Grid item>
                          <Badge badgeContent={directLaborCount} color="secondary">
                            <LoadingButton
                                color="secondary"
                                onClick={handleShowLaborModal}
                                variant="outlined"
                                loading={isLaborCostsFetching}
                            >
                              {getTranslatedLabel(`${localizationKey}.directLabor`, "Direct Labor")}
                            </LoadingButton>
                          </Badge>
                        </Grid>
                        <Grid item>
                          <Badge badgeContent={fohCount} color="secondary">
                            <LoadingButton
                                color="secondary"
                                onClick={handleShowFOHModal}
                                variant="outlined"
                                loading={isFOHCostsFetching}
                            >
                              {getTranslatedLabel(`${localizationKey}.factoryOverhead`, "Factory Overhead")}
                            </LoadingButton>
                          </Badge>
                        </Grid>
                        <Grid item>
                          <Badge badgeContent={materialCount} color="secondary">
                            <LoadingButton
                                color="secondary"
                                onClick={handleShowMaterialModal}
                                variant="outlined"
                                loading={isMaterialCostsFetching}
                            >
                              {getTranslatedLabel(`${localizationKey}.materialCosts`, "Material Costs")}
                            </LoadingButton>
                          </Badge>
                        </Grid>
                      </Grid>
                    </GridToolbar>
                    <Column field="costComponentId" cell={CostComponentCell} title={getTranslatedLabel(`${localizationKey}.costComponentId`, "Cost Component Id")} width={600} />
                    <Column field="cost" title={getTranslatedLabel(`${localizationKey}.cost`, "Cost")} format="{0:n2}" />
                    <Column field="costUomId" title={getTranslatedLabel(`${localizationKey}.costUom`, "Cost UOM")} />
                    <Column field="fromDate" title={getTranslatedLabel(`${localizationKey}.fromDate`, "From Date")} format="{0: dd/MM/yyyy}" />
                    <Column field="thruDate" title={getTranslatedLabel(`${localizationKey}.thruDate`, "Thru Date")} format="{0: dd/MM/yyyy}" />
                  </KendoGrid>
                  <Grid item xs={3} paddingTop={1}>
                    <Button variant="contained" color="error" onClick={handleBackClick}>
                      {getTranslatedLabel(`${localizationKey}.back`, "Back")}
                    </Button>
                  </Grid>
                  {isFetching && (
                      <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`, "Loading Product Costs...")} />
                  )}
                  {isCalculateCostsLoading && (
                      <LoadingComponent message={getTranslatedLabel(`${localizationKey}.calculating`, "Calculate Product Costs...")} />
                  )}
                </div>
              </Grid>
            </Grid>
          </Paper>
      ) : (
          // REFACTOR: Render EditCostComponent when in edit or create mode
          <EditCostComponent
              selectedCostComponent={selectedCostComponent}
              productId={selectedProduct.productId}
              productName={selectedProduct.productName}
              editMode={editMode}
              cancelEdit={cancelEdit}
          />
      )}
    </>
  );
}
