import React, { useEffect, useRef, useState } from "react";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper } from "@mui/material";
import { BookOnline } from "@mui/icons-material";
import ProductForm from "../../form/product/ProductForm";
import {
  useAppDispatch,
  useAppSelector,
  useFetchProductsQuery,
} from "../../../../app/store/configureStore";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import CatalogMenu from "../../menu/CatalogMenu";
import { Product } from "../../../../app/models/product/product";
import { setSelectedProduct } from "../../slice/productUiSlice";
import { useLocation } from "react-router-dom";
import { State } from "@progress/kendo-data-query";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions } from "@mui/material";
import { useReserveApartmentMutation } from "../../../../app/store/apis";

const colorMap = {
  "available": "green",
  "reserved": "orange",
  "sold": "red"
}


function ProductsList() {
  const [editMode, setEditMode] = useState(0);
  const location = useLocation();
  const dispatch = useAppDispatch();
  const { getTranslatedLabel } = useTranslationHelper();
  const [dataState, setDataState] = React.useState<State>({ take: 9, skip: 0 });

  const [reserveTargetId, setReserveTargetId] = useState<string | null>(null);
  const [reserveApartment, { isLoading: isReserving }] = useReserveApartmentMutation();



  const { data, isFetching } = useFetchProductsQuery({ ...dataState });

  const dataStateChange = (e: GridDataStateChangeEvent) => {
    console.log("dataStateChange", e.dataState);
    setDataState(e.dataState);
  };

  function handleReserveApartment(productId: string) {
    setReserveTargetId(productId);
  }

  async function confirmReserve() {
    if (!reserveTargetId) return;
    try {
      await reserveApartment(reserveTargetId).unwrap();
    } catch (e) {
      console.error("Failed to reserve apartment:", e);
    } finally {
      setReserveTargetId(null);
    }
  }

  const selectedProduct = useAppSelector(
    (state) => state.productUi.selectedProduct
  );

  // Purpose: Updates selectedProduct state with the newly created product to ensure ProductForm receives updated data
  // Context: Enables barcode rendering after product creation by passing the new product to ProductForm
  const handleProductCreated = (newProduct: Product) => {
    dispatch(setSelectedProduct(newProduct));
  };

  function handleSelectProduct(productId: string) {
    const selectedProduct: Product | undefined = data?.data?.find(
      (product: any) => product.productId === productId
    );
    dispatch(setSelectedProduct(selectedProduct));
    setEditMode(2);
  }

  function cancelEdit() {
    dispatch(setSelectedProduct(undefined));
    setEditMode(0);
  }

  const ProductIdCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    return (
      <td
        className={props.className}
        style={{ ...props.style, color: "blue" }}
        colSpan={props.colSpan}
        role={"gridcell"}
        aria-colindex={props.ariaColumnIndex}
        aria-selected={props.isSelected}
        {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
        {...navigationAttributes}
      >
        <Grid container>
          <Grid item xs={4}>
            <Button onClick={() => handleSelectProduct(props.dataItem.productId)}>
              {props.dataItem.productId}
            </Button>
          </Grid>
          {props.dataItem.apartmentStatusId === "APARTMENT_AVAILABLE" && <Grid item xs={4}>
            <Button sx={{ '&.MuiButton-root:hover': { bgcolor: 'transparent' } }} disableRipple onClick={() => handleReserveApartment(props.dataItem.productId)}>
              <BookOnline style={{ color: "red" }} />
            </Button>
          </Grid>}
        </Grid>
      </td>
    );
  };

  const ProductDescriptionCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);
    const statusId = props.dataItem.apartmentStatusId
    if (statusId) {
      const statusKey = statusId === "APARTMENT_AVAILABLE" ? "available" : statusId === "APARTMENT_RESERVED" ? "reserved" : "sold"
      return (
        <td
          className={props.className}
          style={{ ...props.style, color: colorMap[statusKey] }}
          colSpan={props.colSpan}
          role={"gridcell"}
          aria-colindex={props.ariaColumnIndex}
          aria-selected={props.isSelected}
          {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
          {...navigationAttributes}
        >
          {getTranslatedLabel(`product.products.list.${statusKey}`, statusId)}
        </td>
      )
    }
    return <td></td>
  }

  const setEditingMode = (mode: number) => {
    if (mode === 1) {
      dispatch(setSelectedProduct(undefined)); // or null
    }
    setEditMode(mode);
  };

  if (location.state?.myStateProp === "bar" && selectedProduct) {
    console.log("Navigated from bar:", selectedProduct);
    return (
      <ProductForm
        product={selectedProduct}
        cancelEdit={cancelEdit}
        editMode={2}
        setEditMode={setEditingMode}
        onProductCreated={handleProductCreated}
      />
    );
  }

  if (editMode) {
    return (
      <ProductForm
        product={editMode === 1 ? undefined : selectedProduct}
        cancelEdit={cancelEdit}
        editMode={editMode}
        setEditMode={setEditingMode}
        onProductCreated={handleProductCreated}
      />
    );
  }

  return (
    <>
      <CatalogMenu selectedMenuItem="/products" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item xs={12}>
            <div className={`div-container`}>
              <KendoGrid
                style={{ height: "65vh", flex: 1 }}
                data={
                  data
                    ? { data: data.data, total: data.total }
                    : { data: [], total: 0 }
                }
                resizable={true}
                filterable={true}
                sortable={true}
                pageable={true}
                {...dataState}
                onDataStateChange={dataStateChange}
              >
                <GridToolbar>
                  <Grid container>
                    <Grid item xs={2}>
                      <Button
                        color={"secondary"}
                        onClick={() => setEditMode(1)}
                        variant="outlined"
                      >
                        {getTranslatedLabel(
                          "product.products.list.create",
                          "Create Product"
                        )}
                      </Button>
                    </Grid>
                    {/* REFACTOR: Removed PDF export button and all related PDF generation logic */}
                    {/* Purpose: Eliminate unused imports, components, and UI elements tied to PDF export */}
                    {/* Improves: Reduces bundle size, eliminates unused dependencies, and simplifies component */}
                  </Grid>
                </GridToolbar>

                <Column
                  field="productId"
                  title={getTranslatedLabel("product.products.list.title", "Number")}
                  cell={ProductIdCell}
                  width={150}
                />
                <Column
                  field="productName"
                  title={getTranslatedLabel("product.products.list.title", "Name")}
                  width={280}
                />
                {/*<Column
                      field="quantityUomDescription"
                      title={getTranslatedLabel("product.products.list.quantity", "Quantity Uom")}
                      width={250}
                  />*/}
                <Column
                  field="productTypeDescription"
                  title={getTranslatedLabel(
                    "product.products.list.type",
                    "Product Type"
                  )}
                />
                <Column
                  field="primaryProductCategoryDescription"
                  title={getTranslatedLabel(
                    "product.products.list.category",
                    "Product Category"
                  )}
                />
                <Column
                  field="projectName"
                  title={getTranslatedLabel(
                    "product.products.list.projectName",
                    "Project Name"
                  )}
                /><Column
                  field="buildingNumber"
                  title={getTranslatedLabel(
                    "product.products.list.buildingNumber",
                    "Building Number"
                  )}
                />
                <Column
                  field="apartmentStatusId"
                  title={getTranslatedLabel(
                    "product.products.list.description",
                    "Description"
                  )}
                  cell={ProductDescriptionCell}
                />
              </KendoGrid>
              {isFetching && <LoadingComponent message="Loading Products..." />}
            </div>
          </Grid>
        </Grid>
      </Paper>

      <Dialog open={!!reserveTargetId} onClose={() => setReserveTargetId(null)}>
        <DialogTitle>
          {getTranslatedLabel("product.products.reserve.title", "Reserve Apartment")}
        </DialogTitle>
        <DialogContent>
          <DialogContentText>
            {getTranslatedLabel("product.products.reserve.confirm", "Are you sure you want to reserve apartment")}{" "}
            <strong>{reserveTargetId}</strong>?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setReserveTargetId(null)} disabled={isReserving}>
            {getTranslatedLabel("product.products.reserve.cancel", "Cancel")}
          </Button>
          <Button onClick={confirmReserve} color="error" variant="contained" disabled={isReserving}>
            {isReserving
              ? getTranslatedLabel("product.products.reserve.reserving", "Reserving...")
              : getTranslatedLabel("product.products.reserve.confirmButton", "Confirm Reserve")}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}

export default ProductsList;