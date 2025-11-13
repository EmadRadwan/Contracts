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

function ProductsList() {
  const [editMode, setEditMode] = useState(0);
  const location = useLocation();
  const dispatch = useAppDispatch();
  const { getTranslatedLabel } = useTranslationHelper();
  const [dataState, setDataState] = React.useState<State>({ take: 9, skip: 0 });



  const { data, isFetching } = useFetchProductsQuery({ ...dataState });

  const dataStateChange = (e: GridDataStateChangeEvent) => {
    console.log("dataStateChange", e.dataState);
    setDataState(e.dataState);
  };

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
          <Button onClick={() => handleSelectProduct(props.dataItem.productId)}>
            {props.dataItem.productId}
          </Button>
        </td>
    );
  };

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
                      width={120}
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
                  {/*<Column
                      field="productTrademarkDescription"
                      title={getTranslatedLabel(
                          "product.products.list.trademark",
                          "Trademark"
                      )}
                  />*/}
                  <Column
                      field="description"
                      title={getTranslatedLabel(
                          "product.products.list.description",
                          "Description"
                      )}
                  />
                </KendoGrid>
                {isFetching && <LoadingComponent message="Loading Products..." />}
              </div>
            </Grid>
          </Grid>
        </Paper>
      </>
  );
}

export default ProductsList;