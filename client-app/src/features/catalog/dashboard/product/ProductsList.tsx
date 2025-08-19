import React, {useEffect, useRef, useState} from "react";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper  } from "@mui/material";
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
import {Document, Page, Text, View, StyleSheet, PDFDownloadLink, Font, pdf} from '@react-pdf/renderer';

// Purpose: Ensures proper rendering of Arabic text by using a font that supports RTL scripts
Font.register({
  family: 'Amiri',
  src: '/fonts/Amiri-Regular.ttf',
});

const styles = StyleSheet.create({
  page: {
    flexDirection: 'column',
    backgroundColor: '#E4E4E4',
    padding: 20,
    fontFamily: 'Amiri',
    textDirection: 'rtl',
    writingMode: 'rl-tb',
  },
  section: {
    margin: 10,
    padding: 10,
  },
  table: {
    display: 'table',
    width: '100%',
    borderStyle: 'solid',
    borderWidth: 1,
    borderRightWidth: 0,
    borderBottomWidth: 0,
  },
  tableRow: {
    flexDirection: 'row-reverse',
  },
  tableCol: {
    width: '120pt',
    borderStyle: 'solid',
    borderWidth: 1,
    borderLeftWidth: 0,
    borderTopWidth: 0,
    padding: 10,
  },
  tableColWide: {
    width: '180pt',
    borderStyle: 'solid',
    borderWidth: 1,
    borderLeftWidth: 0,
    borderTopWidth: 0,
    padding: 10,
  },
  tableCell: {
    margin: 5,
    fontSize: 8,
    textAlign: 'right',
    wrap: true,
    maxLines: 3,
  },
  title: {
    fontSize: 20,
    marginBottom: 15,
    textAlign: 'right',
  },
  subtitle: {
    fontSize: 10,
    marginBottom: 15,
    textAlign: 'right',
  },
  error: {
    fontSize: 10,
    color: 'red',
    textAlign: 'right',
  },
});

const MyDocument = ({ products, getTranslatedLabel }) => {
  // Purpose: Verifies Arabic text data before rendering
  console.log('MyDocument products:', products);
  if (products && products.length > 0) {
    console.log('Sample product:', products[0]);
    console.log('Sample Arabic text (Quantity Uom):', products[0]?.quantityUomDescription);
    console.log('Sample Arabic text (Category):', products[0]?.primaryProductCategoryDescription);
  }

  const pageSize = 15;
  const pages = [];
  for (let i = 0; i < products.length; i += pageSize) {
    pages.push(products.slice(i, i + pageSize));
  }

  return (
      <Document>
        {pages.map((pageProducts, index) => (
            <Page key={index} size="A4" orientation="landscape" style={styles.page}>
              <View style={styles.section}>
                <Text style={styles.title}>{getTranslatedLabel("product.products.list.reportTitle", "Products Report")}</Text>
                <Text style={styles.subtitle}>
                  {getTranslatedLabel("product.products.list.generatedOn", "Generated on")}: {new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'numeric', day: 'numeric' })}
                </Text>
                <View style={styles.table}>
                  <View style={styles.tableRow}>
                    <View style={styles.tableCol}><Text style={styles.tableCell}>{getTranslatedLabel("product.products.list.title", "Number")}</Text></View>
                    <View style={styles.tableColWide}><Text style={styles.tableCell}>{getTranslatedLabel("product.products.list.title", "Name")}</Text></View>
                    <View style={styles.tableCol}><Text style={styles.tableCell}>{getTranslatedLabel("product.products.list.quantity", "Quantity Uom")}</Text></View>
                    <View style={styles.tableCol}><Text style={styles.tableCell}>{getTranslatedLabel("product.products.list.type", "Product Type")}</Text></View>
                    <View style={styles.tableCol}><Text style={styles.tableCell}>{getTranslatedLabel("product.products.list.category", "Product Category")}</Text></View>
                  </View>
                  {pageProducts && pageProducts.length > 0 ? (
                      pageProducts.map((product) => (
                          <View key={product.productId || Math.random()} style={styles.tableRow}>
                            <View style={styles.tableCol}><Text style={styles.tableCell}>{product.productId || 'N/A'}</Text></View>
                            <View style={styles.tableColWide}><Text style={styles.tableCell}>{product.productName || 'N/A'}</Text></View>
                            <View style={styles.tableCol}><Text style={styles.tableCell}>{product.quantityUomDescription || 'N/A'}</Text></View>
                            <View style={styles.tableCol}><Text style={styles.tableCell}>{product.productTypeDescription || 'N/A'}</Text></View>
                            <View style={styles.tableCol}><Text style={styles.tableCell}>{product.primaryProductCategoryDescription || 'N/A'}</Text></View>
                          </View>
                      ))
                  ) : (
                      <View style={styles.tableRow}>
                        <View style={styles.tableCol}>
                          <Text style={styles.error}>{getTranslatedLabel("product.products.list.noData", "No products available")}</Text>
                        </View>
                      </View>
                  )}
                </View>
              </View>
            </Page>
        ))}
      </Document>
  );
};


function ProductsList() {
  const [editMode, setEditMode] = useState(0);
  const location = useLocation();
  const dispatch = useAppDispatch();
  const { getTranslatedLabel } = useTranslationHelper();
  const [dataState, setDataState] = React.useState<State>({ take: 9, skip: 0 });
  const defaultProduct: Product = {
    productId: "",
    productName: "",
    productTypeId: "",
    primaryProductCategoryId: "",
    quantityUomId: "",
    productColorId: "",
    productSizeId: "",
    productTrademarkId: "",
    isVirtual: "N",
    isVariant: "N",
    comments: "",
    modelProductId: "",
  };

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
            {...{
              [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
            }}
            {...navigationAttributes}
        >
          <Button onClick={() => handleSelectProduct(props.dataItem.productId)}>
            {props.dataItem.productId}
          </Button>
        </td>
    );
  };

  const setEditingMode = (mode: number) => {
    // Purpose: Sets defaultProduct for create mode (1) and retains selectedProduct for edit mode (2)
    if (mode === 1 && selectedProduct !== defaultProduct) {
      dispatch(setSelectedProduct(defaultProduct));
    }
    // Purpose: Ensures editMode is updated only when necessary to avoid unnecessary re-renders
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
            product={selectedProduct || defaultProduct}
            cancelEdit={cancelEdit}
            editMode={editMode}
            setEditMode={setEditingMode}
            onProductCreated={handleProductCreated}
        />
    );
  }
  

  return (
      <>
        <CatalogMenu selectedMenuItem='/products'  />
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
                      
                      <Grid item xs={2}>
                        {data && data.data ? (
                            <PDFDownloadLink
                                document={<MyDocument products={data.data} getTranslatedLabel={getTranslatedLabel} />}
                                fileName="Products_Report.pdf"
                            >
                              {({ loading }) => (
                                  <Button
                                      color='primary'
                                      variant='outlined'
                                      disabled={isFetching || loading }
                                  >
                                    {loading
                                        ? getTranslatedLabel('product.products.list.generating', 'Generating PDF...')
                                        : getTranslatedLabel('product.products.list.export', 'Export to PDF')}
                                  </Button>
                              )}
                            </PDFDownloadLink>
                        ) : (
                            <Button
                                color='primary'
                                variant='outlined'
                                disabled={true}
                            >
                              {getTranslatedLabel('product.products.list.export', 'Export to PDF')}
                            </Button>
                        )}
                      </Grid>
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
                  <Column
                      field="quantityUomDescription"
                      title={getTranslatedLabel("product.products.list.quantity", "Quantity Uom")}
                      width={250}
                  />

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
                  field="productTrademarkDescription"
                  title={getTranslatedLabel(
                    "product.products.list.trademark",
                    "Trademark"
                )}
                  />
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
