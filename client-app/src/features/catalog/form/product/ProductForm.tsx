import React, {useEffect, useLayoutEffect, useMemo, useRef, useState} from "react";
import Button from "@mui/material/Button";
import Grid from "@mui/material/Grid";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import FormInput from "../../../../app/common/form/FormInput";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { Product } from "../../../../app/models/product/product";
import CreateProductMenu from "../../menu/CreateProductMenu";
import {
  useAddProductMutation,
  useFetchProductCategoriesQuery,
  useFetchProductFeatureTrademarksQuery,
  useFetchProductTypesQuery,
  useFetchProductUOMsQuery,
  useUpdateProductMutation,
} from "../../../../app/store/configureStore";
import { Box, Paper, Typography } from "@mui/material";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { FormDropDownTreeProductCategory } from "../../../../app/common/form/FormDropDownTreeProductCategory";
import { toast } from "react-toastify";
import CatalogMenu from "../../menu/CatalogMenu";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {useFetchInventoryItemColorsQuery, useFetchProductCategoriesRawMaterialsQuery} from "../../../../app/store/apis";
import JsBarcode from "jsbarcode";
import isEqual from "lodash/isEqual";

let renderCount = 0;

interface Props {
  product?: Product;
  editMode: number;
  cancelEdit: () => void;
  setEditMode?: (mode: number) => void;
  onProductCreated?: (product: Product) => void;
}

function ProductForm({
  product,
  cancelEdit,
  editMode,
  setEditMode, onProductCreated
}: Props) {
  // Debug render count
  console.log(`ProductForm render #${++renderCount}`);

  const [createProduct, { isLoading: isCreating }] = useAddProductMutation();
  const [updateProduct, { isLoading: isUpdating }] = useUpdateProductMutation();
  const [imagePreviewUrl, setImagePreviewUrl] = useState(
    product?.originalImageUrl
      ? `/images/products/${product.originalImageUrl}`
      : null
  );
  const { getTranslatedLabel } = useTranslationHelper();
  const formRef = React.useRef<boolean>(false);
  const [createdProduct, setCreatedProduct] = useState<Product | null>(null);

  const memoizedProduct = useMemo(() => product, [product?.productId, product?.productName, product?.productTypeId]);

  // Purpose: Supports generating one barcode per color, each with its own canvas and printable container
  // Context: Enables dynamic barcode generation based on fetched color data
  const [barcodeRefs, setBarcodeRefs] = useState<
      { colorId: string; canvasRef: React.MutableRefObject<HTMLCanvasElement | null>; componentRef: React.MutableRefObject<HTMLDivElement | null> }[]
      >([]);
  const [isBarcodeLoading, setIsBarcodeLoading] = useState(false);
  const isGeneratingBarcodes = useRef(false);
  const isBarcodeGenerated = useRef(false);

  // Purpose: Retrieves color data to generate barcodes for each product-color combination
  // Context: Skips query if not in edit mode or no productId is available
  const { data: colors, isFetching: isColorsFetching } = useFetchInventoryItemColorsQuery(
      (createdProduct?.productId || memoizedProduct?.productId) ?? "",
      { skip: editMode !== 2 || !memoizedProduct?.productId }
  );

  // Purpose: Stabilizes colors dependency by stringifying to compare content
  // Context: Prevents endless rendering due to new array references
  const memoizedColors = useMemo(() => colors || [], [colors]);

  // Purpose: Scopes barcode printing with CSS
  // Context: Runs only when colors change
  useEffect(() => {
    console.log("CSS useEffect triggered, colors:", memoizedColors.map(c => c.colorId));
    const styleElement = document.createElement("style");
    styleElement.innerHTML = `
@media print {
    body * {
        visibility: hidden;
    }
    .print-barcode {
        visibility: visible;
        position: absolute;
        top: 0;
        left: 0;
        width: 3in;
    }
    ${memoizedColors.map((color) => `
      .print-barcode-${color.colorId} {
        visibility: visible;
        position: absolute;
        top: 0;
        left: 0;
        width: 3in;
      }
    `).join("")}
}
canvas {
    display: block !important;
    visibility: visible !important;
}
`;
    document.head.appendChild(styleElement);
    return () => {
      document.head.removeChild(styleElement);
    };
  }, [memoizedColors]);

// Purpose: Sets barcodeRefs when colors change
// Context: Prepares refs for canvas elements
  useEffect(() => {
    console.log("Barcode useEffect triggered", { editMode, productId: memoizedProduct?.productId, colors: memoizedColors, barcodeRefs });
    const activeProduct = createdProduct || memoizedProduct;
    if (editMode !== 2 || !activeProduct?.productId || !memoizedColors || memoizedColors.length === 0 || isGeneratingBarcodes.current) {
      if (barcodeRefs.length !== 0 && !isGeneratingBarcodes.current) {
        console.log("Clearing barcodeRefs due to invalid conditions");
        setBarcodeRefs([]);
        setIsBarcodeLoading(false);
        isBarcodeGenerated.current = false;
      }
      return;
    }

    const prevColorIds = barcodeRefs.map((ref) => ref.colorId);
    const newColorIds = memoizedColors.map((color) => color.colorId);
    if (isEqual(prevColorIds, newColorIds)) {
      console.log("Colors unchanged, skipping barcode generation");
      return;
    }

    isGeneratingBarcodes.current = true;
    isBarcodeGenerated.current = false;
    setIsBarcodeLoading(true);
    const newBarcodeRefs = memoizedColors.map((color) => ({
      colorId: color.colorId,
      canvasRef: React.createRef<HTMLCanvasElement>(),
      componentRef: React.createRef<HTMLDivElement>(),
    }));
    console.log("Setting new barcodeRefs:", newBarcodeRefs.map(ref => ref.colorId));
    setBarcodeRefs(newBarcodeRefs);
  }, [editMode, memoizedProduct, createdProduct, memoizedColors]);

  useEffect(() => {
    if (!isGeneratingBarcodes.current || barcodeRefs.length === 0 || isBarcodeGenerated.current) {
      console.log("Skipping MutationObserver", { isGenerating: isGeneratingBarcodes.current, barcodeRefsLength: barcodeRefs.length, isGenerated: isBarcodeGenerated.current });
      return;
    }

    const barcodeContainer = document.querySelector(".barcode-container");
    if (!barcodeContainer) {
      console.warn("Barcode container not found in useEffect");
      return;
    }

    // Purpose: Confirms barcode-container and canvas presence
    // Context: Diagnoses DOM rendering issues
    console.log("Barcode container found, canvases:", barcodeContainer.querySelectorAll("canvas").length);

    const observer = new MutationObserver(() => {
      console.log("MutationObserver: Checking canvas mounting");
      if (barcodeRefs.every(({ canvasRef }) => canvasRef.current)) {
        console.log("MutationObserver: All canvases mounted, triggering generation");
        generateBarcodes();
        observer.disconnect();
      }
    });

    observer.observe(barcodeContainer, { childList: true, subtree: true });

    // Purpose: Fails gracefully if canvases don't mount
    // Context: Ensures user feedback if DOM is stuck
    const timeoutId = setTimeout(() => {
      if (!isBarcodeGenerated.current) {
        console.error("Timeout: Canvas elements not mounted in MutationObserver");
        barcodeRefs.forEach(({ colorId }, index) => {
          console.error(`Canvas status for ${colorId}: ${barcodeRefs[index].canvasRef.current ? 'Mounted' : 'Not mounted'}`);
        });
       /* toast.error(
            getTranslatedLabel(
                "product.products.form.barcodeError",
                "Failed to generate barcodes: Canvas not mounted"
            )
        );*/
        setIsBarcodeLoading(false);
        isGeneratingBarcodes.current = false;
        isBarcodeGenerated.current = false;
        setBarcodeRefs([...barcodeRefs]); // Trigger retry
      }
    }, 5000);

    return () => {
      observer.disconnect();
      clearTimeout(timeoutId);
    };
  }, [barcodeRefs, memoizedColors, createdProduct, memoizedProduct]);

  const generateBarcodes = () => {
    console.log("Generating barcodes, canvas refs:", barcodeRefs.map(ref => ({ colorId: ref.colorId, mounted: !!ref.canvasRef.current })));
    if (!barcodeRefs.every(({ canvasRef }) => canvasRef.current)) {
      console.warn("Not all canvases mounted, aborting generation");
      return;
    }

    const activeProduct = createdProduct || memoizedProduct;
    let success = true;
    barcodeRefs.forEach(({ colorId, canvasRef }, index) => {
      const color = memoizedColors[index];
      if (canvasRef.current && color) {
        canvasRef.current.width = 250;
        canvasRef.current.height = 80;
        const barcodeValue = `${activeProduct.productId}-${colorId}`;
        console.log(`Generating barcode for ${barcodeValue}`);
        try {
          if (!barcodeValue || !/^[A-Za-z0-9_-]+$/.test(barcodeValue)) {
            throw new Error(`Invalid barcode value: ${barcodeValue}`);
          }
          const context = canvasRef.current.getContext("2d");
          if (!context) {
            throw new Error(`Canvas context unavailable for ${colorId}`);
          }
          JsBarcode(canvasRef.current, barcodeValue, {
            format: "CODE128",
            height: 50,
            width: 2,
            font: "Helvetica",
            fontSize: 12,
            text: `${activeProduct.productId} (${color.colorName})`,
            textAlign: "center",
            textPosition: "bottom",
            displayValue: true,
          });
          console.log(`Barcode generated for ${colorId}`);
        } catch (error) {
          console.error(`JsBarcode failed for ${colorId}:`, error);
          toast.error(`Failed to generate barcode for ${color.colorName}`);
          success = false;
        }
      } else {
        console.warn(`Canvas ref missing for ${colorId}`);
        success = false;
      }
    });

    if (success) {
      isBarcodeGenerated.current = true;
      setIsBarcodeLoading(false);
      isGeneratingBarcodes.current = false;
      console.log("Barcode generation completed successfully");
    } else {
      console.error("Barcode generation failed");
      toast.error(
          getTranslatedLabel(
              "product.products.form.barcodeError",
              "Failed to generate barcodes"
          )
      );
      setIsBarcodeLoading(false);
      isGeneratingBarcodes.current = false;
      isBarcodeGenerated.current = false;
      setBarcodeRefs([...barcodeRefs]); // Trigger retry
    }
  };

// Purpose: Initiates generation after refs are set
// Context: Relies on separate useEffect for observer
  useLayoutEffect(() => {
    console.log("useLayoutEffect triggered, checking conditions", { isGenerating: isGeneratingBarcodes.current, barcodeRefsLength: barcodeRefs.length, isGenerated: isBarcodeGenerated.current });
  }, [barcodeRefs, memoizedColors, createdProduct, memoizedProduct]);


  // Purpose: Uses window.print() to trigger browser print dialog, scoping to individual barcodes via CSS
  // Context: Hides all elements except the selected barcode during printing
  const handlePrint = (componentRef: React.MutableRefObject<HTMLDivElement | null>, colorId: string) => {
    if (!componentRef.current) {
      toast.error(
          getTranslatedLabel(
              "product.products.form.printError",
              "Nothing to print"
          )
      );
      return;
    }
    console.log(`Printing barcode for colorId: ${colorId}`);
    componentRef.current.classList.add(`print-barcode-${colorId}`);
    window.print();
    componentRef.current.classList.remove(`print-barcode-${colorId}`);
  };


  // Purpose: Truncates productName and colorName at word boundaries for readability
  // Context: Used in barcode display to prevent awkward text cuts
  const truncateText = (text: string, maxLength: number) => {
    if (text.length <= maxLength) return text;
    const sliced = text.substring(0, maxLength);
    return sliced.substring(0, sliced.lastIndexOf(" ")) + "...";
  };

  const { data: productTypes, isFetching: isProductTypesFetching } = useFetchProductTypesQuery(undefined);
  const { data: productCategories, isFetching: isProductCategoriesFetching } = useFetchProductCategoriesQuery(undefined);
  const { data: productCategoriesRawMaterials, isFetching: isProductCategoriesFetchingRawMaterials } = useFetchProductCategoriesRawMaterialsQuery(undefined);
  const { data: unitsOfMeasure, isFetching: isUoMsFetching } = useFetchProductUOMsQuery(undefined);
  const memoizedProductTypes = useMemo(() => productTypes || [], [productTypes]);
  const memoizedUnitsOfMeasure = useMemo(() => unitsOfMeasure || [], [unitsOfMeasure]);

  const { data: productFeatureTrademarks } =
    useFetchProductFeatureTrademarksQuery(undefined);
 

  const [buttonFlag, setButtonFlag] = useState(false);
  const [selectedProductType, setSelectedProductType] = useState(
      product?.productTypeId || ""
  );

  const memoizedCategoryData = useMemo(() => {
    return selectedProductType === "RAW_MATERIAL"
        ? productCategoriesRawMaterials || []
        : productCategories || [];
  }, [selectedProductType, productCategories, productCategoriesRawMaterials]);

  
  const handleResetForm = (formRenderProps: any) => {
    // Purpose: Forces a re-render of the form to clear its state
    formRef.current = !formRef.current;

    // Purpose: Resets the image preview to null for a fresh form
    setImagePreviewUrl(null);
    setCreatedProduct(null);
    // Purpose: Updates editMode to 1 (create mode) and checks for prop existence
    if (setEditMode) {
      setEditMode(1);
    } else {
      console.warn('setEditMode is not provided. Cannot update editMode.');
    }

    // Purpose: Uses Kendo Form's reset method to clear all fields
    if (formRenderProps) {
      formRenderProps.onFormReset();
    }

    setSelectedProductType("");
  };


  async function handleSubmitData(data: any) {
    setButtonFlag(true);
    try {
      // Purpose: Ensures the data sent to the API matches the expected format by extracting productId if modelProductId is an object
      const transformedData = {
        ...data
      };

      if (editMode === 2) {
        await updateProduct(transformedData).unwrap();
        toast.success(
            getTranslatedLabel(
                "product.products.form.updateSuccess",
                "Product updated successfully"
            )
        );
      } else {
        const newProduct = await createProduct(transformedData).unwrap();
        setCreatedProduct(newProduct);
        toast.success(
            getTranslatedLabel(
                "product.products.form.createSuccess",
                "Product created successfully"
            )
        );
        // Purpose: Switches the form to edit mode after creating a product, allowing immediate editing of the newly created product
        // Context: Checks if setEditMode is provided to avoid errors when the prop is not passed
        if (setEditMode) {
          setEditMode(2);
        }

        if (onProductCreated) {
          onProductCreated(newProduct);
        }
      }
    } catch (error) {
      toast.error(
          getTranslatedLabel(
              "product.products.form.error",
              "Failed to process product"
          )
      );
      console.error(error);
    } finally {
      setButtonFlag(false);
    }
  }



  const formValidator = (values: any) => {
    const errors: any = {};
    // Prevents unnecessary validation errors for hidden fields
    if (
        values.productTypeId !== "RAW_MATERIAL" &&
        !values.productTrademarkId
    ) {
      errors.productTrademarkId = getTranslatedLabel(
          "product.products.form.validation.trademark",
          "Trademark is required."
      );
    }
    //  Added validation for quantityToInclude when productTypeId is FINISHED_GOOD
    // Purpose: Ensures quantityToInclude is provided and is a positive number for finished products
    if (values.productTypeId === "FINISHED_GOOD" && (!values.quantityIncluded || values.quantityIncluded <= 0)) {
      errors.quantityIncluded = getTranslatedLabel(
          "product.products.form.validation.quantityToInclude",
          "Quantity to include must be a positive number."
      );
    }

    // Purpose: Validates that piecesIncluded is provided and is a positive integer, aligning with integer type requirement
    if (values.productTypeId === "FINISHED_GOOD" && (!values.piecesIncluded || values.piecesIncluded <= 0 || !Number.isInteger(Number(values.piecesIncluded)))) {
      errors.piecesIncluded = getTranslatedLabel(
          "product.products.form.validation.piecesIncluded",
          "Pieces Included must be a positive integer."
      );
    }
    return errors;
  };

  const handleProductTypeChange = (e: any, formRenderProps: any) => {
    const newProductType = e.value || "";
    console.log("Changing product type to:", newProductType);
    setSelectedProductType(newProductType);
    if (formRenderProps) {
      formRenderProps.onChange("primaryProductCategoryId", { value: null });
      // Purpose: Ensures quantityIncluded retains its default value of 1 when product type changes, matching piecesIncluded behavior
      // Context: Prevents the field from being cleared, maintaining consistency with initialValues and avoiding validation errors
      formRenderProps.onChange("quantityIncluded", { value: 1 });
      formRenderProps.onChange("piecesIncluded", { value: 1 });
    }
  };
  
  console.log('barcodeRefs:', barcodeRefs);
  

  return (
    <>
      <CatalogMenu selectedMenuItem="/products" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container spacing={2}>
          <Grid item xs={6}>
            <Box display="flex" justifyContent="space-between">
              <Typography
                color={product?.productName ? "black" : "green"}
                sx={{ p: 2 }}
                variant="h4"
              >
                {" "}
                {product?.productName
                  ? product.productName
                  : getTranslatedLabel(
                      "product.products.form.new",
                      "New Product"
                    )}{" "}
              </Typography>
            </Box>
          </Grid>
          {editMode === 2 && (
            <Grid item xs={6}>
              <CreateProductMenu />
            </Grid>
          )}
        </Grid>

        <Form
            initialValues={{
              ...product,
              piecesIncluded: product?.piecesIncluded ?? 1,
              quantityIncluded: product?.quantityIncluded ?? 1,
            }}
          onSubmit={(values) => handleSubmitData(values)}
          validator={formValidator}
          key={JSON.stringify(product)}
          render={(formRenderProps) => {
            console.log("Using category data source:", memoizedCategoryData);
            console.log("selectedProductType:", selectedProductType);
            console.log("Rendering barcode container, colors:", memoizedColors.map(c => c.colorId));

            return (
              <FormElement>
                <fieldset className={"k-form-fieldset"}>
                  <Grid container spacing={2}>
                    <Grid item xs={3}>
                      <Field
                        id={"productId"}
                        name={"productId"}
                        label={getTranslatedLabel(
                          "product.products.form.id",
                          "Product ID *"
                        )}
                        component={FormInput}
                        autoComplete={"off"}
                        disabled={editMode === 2}
                        validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={9}>
                      <Field
                        id={"productName"}
                        name={"productName"}
                        label={getTranslatedLabel(
                          "product.products.form.name",
                          "Product Name *"
                        )}
                        component={FormInput}
                        autoComplete={"off"}
                        validator={requiredValidator}
                      />
                    </Grid>
                  </Grid>

                  <Grid container spacing={2}>
                    <Grid item xs={4}>
                      <Field
                        id={"productTypeId"}
                        name={"productTypeId"}
                        label={getTranslatedLabel(
                          "product.products.form.type",
                          "Product Type *"
                        )}
                        component={MemoizedFormDropDownList}
                        dataItemKey={"productTypeId"}
                        textField={"description"}
                        data={memoizedProductTypes}
                        validator={requiredValidator}
                        onChange={(e: any) => handleProductTypeChange(e, formRenderProps)}
                        
                      />
                    </Grid>
                    <Grid item xs={4}>
                      {memoizedCategoryData.length > 0 && (
                          <Field
                              key={`category-${selectedProductType}`}
                              id={"primaryProductCategoryId"}
                              name={"primaryProductCategoryId"}
                              label={getTranslatedLabel(
                                  "product.products.form.category",
                                  "Product Category *"
                              )}
                              data={memoizedCategoryData}
                              component={FormDropDownTreeProductCategory}
                              dataItemKey={"productCategoryId"}
                              textField={"text"}
                              validator={requiredValidator}
                              selectField={"selected"}
                              expandField={"expanded"}
                          />
                      )}
                    </Grid>
                    <Grid item xs={4}>
                      {memoizedUnitsOfMeasure && (
                        <Field
                          id={"quantityUOM"}
                          name={"quantityUomId"}
                          label={getTranslatedLabel(
                            "product.products.form.uom",
                            "Quantity UOM"
                          )}
                          component={MemoizedFormDropDownList}
                          dataItemKey={"quantityUomId"}
                          textField={"description"}
                          validator={requiredValidator}
                          data={memoizedUnitsOfMeasure}
                        />
                      )}
                    </Grid>
                  </Grid>

                 
                  
                  {selectedProductType !== "RAW_MATERIAL" && (
                  <Grid container spacing={2}>
                    <Grid item xs={4}>
                      <Field
                        id={"productTrademarkId"}
                        name={"productTrademarkId"}
                        label={getTranslatedLabel(
                          "product.products.form.trademark",
                          "Trademark *"
                        )}
                        component={MemoizedFormDropDownList}
                        dataItemKey={"productTrademarkId"}
                        textField={"description"}
                        data={
                          productFeatureTrademarks
                            ? productFeatureTrademarks
                            : []
                        }
                        validator={requiredValidator}
                      />
                    </Grid>
                    {selectedProductType === "FINISHED_GOOD" && (
                        <>
                          <Grid item xs={4}>
                            <Field
                                id={"quantityIncluded"}
                                name={"quantityIncluded"}
                                label={getTranslatedLabel(
                                    "product.products.form.quantityIncluded",
                                    "Quantity Included *"
                                )}
                                component={FormInput}
                                type="number"
                                validator={requiredValidator}
                            />
                          </Grid>
                          {/* Purpose: Adds integer input field for piecesIncluded, visible only for FINISHED_GOOD, with default value of 1 */}
                          <Grid item xs={4}>
                            <Field
                                id={"piecesIncluded"}
                                name={"piecesIncluded"}
                                label={getTranslatedLabel(
                                    "product.products.form.piecesIncluded",
                                    "Pieces Included *"
                                )}
                                component={FormInput}
                                type="number"
                                validator={requiredValidator}
                            />
                          </Grid>
                        </>
                    )}
                  </Grid>
                  )}

                 
                  
                  <Grid item xs={6}>
                    <Field
                      id={"comments"}
                      name={"comments"}
                      label={getTranslatedLabel(
                        "product.products.form.comments",
                        "Comments"
                      )}
                      autoComplete={"off"}
                      rows={3}
                      component={FormTextArea}
                    />
                  </Grid>

                  <Grid item xs={5}>
                    {imagePreviewUrl && (
                      <img
                        src={imagePreviewUrl}
                        alt="Product Image"
                        style={{ maxWidth: "200px" }}
                      />
                    )}
                  </Grid>


                  {editMode === 2 && (
                      <Grid item xs={12} className="barcode-container">
                        {isColorsFetching || isBarcodeLoading ? (
                            <Typography>
                              {getTranslatedLabel("product.products.form.loadingBarcode", "Loading barcodes...")}
                            </Typography>
                        ) : memoizedColors && memoizedColors.length > 0 ? (
                            <Box
                                sx={{
                                  display: "flex",
                                  flexDirection: "column",
                                  gap: "20px",
                                  padding: "10px",
                                  marginTop: "10px",
                                }}
                            >
                              {memoizedColors.map((color, index) => {
                                console.log(`Canvas ref for ${color.colorId}:`, barcodeRefs[index]?.canvasRef.current);
                                return (
                                    <Box
                                        key={color.colorId}
                                        sx={{
                                          display: "flex",
                                          alignItems: "center",
                                          justifyContent: "center",
                                          gap: "20px",
                                        }}
                                    >
                                      {/* REFACTOR: Increased box width from 2.5in to 2.75in to further prevent barcode overflow.
                                   Adjusted padding and text maxWidth for better fit.
                                   Context: Provides additional space for barcode and text, ensuring no clipping. */}
                                      <Box
                                          id={`printable-barcode-${color.colorId}`}
                                          ref={barcodeRefs[index]?.componentRef}
                                          sx={{
                                            textAlign: "center",
                                            padding: "15px",
                                            width: "3in",
                                            border: "1px solid #ccc",
                                            boxSizing: "border-box",
                                          }}
                                          className="print-barcode"
                                      >
                                        <Typography
                                            sx={{
                                              fontSize: "12pt",
                                              margin: "5px 0",
                                              whiteSpace: "nowrap",
                                              overflow: "hidden",
                                              textOverflow: "ellipsis",
                                              maxWidth: "2.55in", // Adjusted to fit within enlarged box
                                            }}
                                        >
                                          {truncateText(
                                              `${memoizedProduct?.productName || "Unknown Product"} (${color.colorName})`,
                                              20
                                          )}
                                        </Typography>
                                        {/* REFACTOR: Increased canvas width to 250px to match enlarged box dimensions.
                                     Context: Ensures barcode scales appropriately within the larger container. */}
                                        <canvas
                                            ref={barcodeRefs[index]?.canvasRef}
                                            style={{ width: "250px", height: "80px", display: "block" }}
                                            aria-label={`Barcode for product ${memoizedProduct?.productName || "Unknown Product"} with color ${color.colorName}`}
                                        ></canvas>
                                      </Box>
                                      <Button
                                          onClick={() => handlePrint(barcodeRefs[index]?.componentRef, color.colorId)}
                                          color="primary"
                                          variant="contained"
                                          sx={{ marginTop: "10px" }}
                                      >
                                        {getTranslatedLabel(
                                            "product.products.form.printBarcode",
                                            `Print ${color.colorName} Barcode`
                                        )}
                                      </Button>
                                    </Box>
                                );
                              })}
                            </Box>
                        ) : (
                            <Typography>
                              {getTranslatedLabel(
                                  "product.products.form.noColors",
                                  "No colors available for this product"
                              )}
                            </Typography>
                        )}
                      </Grid>
                  )}

                  <div className="k-form-buttons">
                    <Grid container rowSpacing={2}>
                      <Grid item xs={1}>
                        <Button
                            variant="contained"
                            type="submit"
                            color="success"
                            disabled={buttonFlag || isCreating || isUpdating}
                        >
                          {getTranslatedLabel("general.submit", "Submit")}
                        </Button>
                      </Grid>
                      <Grid item xs={1}>
                        <Button
                          onClick={cancelEdit}
                          color="error"
                          variant="contained"
                        >
                          {getTranslatedLabel("general.cancel", "Cancel")}
                        </Button>
                      </Grid>
                      <Grid item xs={2}>
                        <Button
                            onClick={() => handleResetForm(formRenderProps)}
                            color="primary"
                            variant="contained"
                            disabled={isCreating || isUpdating}
                        >
                          {getTranslatedLabel("product.products.form.createNew", "Clear Form")}
                        </Button>
                      </Grid>
                    </Grid>
                  </div>

                  {(buttonFlag || isCreating || isUpdating) && (
                    <LoadingComponent
                      message={getTranslatedLabel(
                        "product.products.form.processing",
                        "Processing Product..."
                      )}
                    />
                  )}
                  {(isProductCategoriesFetching ||
                    isProductTypesFetching ||
                    isUoMsFetching) && (
                    <LoadingComponent message={"Loading Data..."} />
                  )}
                </fieldset>
              </FormElement>
            );
          }}
        />
      </Paper>
    </>
  );
}

export default React.memo(ProductForm, (prevProps, nextProps) => {
  return (
      isEqual(prevProps.product, nextProps.product) &&
      prevProps.editMode === nextProps.editMode &&
      prevProps.cancelEdit === nextProps.cancelEdit &&
      prevProps.setEditMode === nextProps.setEditMode &&
      prevProps.onProductCreated === nextProps.onProductCreated
  );
});
