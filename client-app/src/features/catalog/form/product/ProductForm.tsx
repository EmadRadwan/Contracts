import React, { useMemo, useRef, useState } from "react";
import Button from "@mui/material/Button";
import Grid from "@mui/material/Grid";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import FormInput from "../../../../app/common/form/FormInput";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { Product } from "../../../../app/models/product/product";
import {
  useAddProductMutation,
  useFetchProductCategoriesQuery,
  useFetchProductTypesQuery,
  useUpdateProductMutation,
} from "../../../../app/store/configureStore";
import {
  Box,
  Paper,
  Typography,
} from "@mui/material";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { MemoizedFormMuiAutoCompleteProductCategory } from "../../../../app/common/form/FormMuiAutoCompleteProductCategory";
import { toast } from "react-toastify";
import CatalogMenu from "../../menu/CatalogMenu";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {
  useFetchProductCategoriesFlatQuery,
  useFetchProductCategoriesRawMaterialsQuery,
} from "../../../../app/store/apis";
import { FormComboBoxVirtualProject } from "../../../../app/common/form/FormComboBoxVirtualProject";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import ManageProductCategoryMembersModal from "./ManageProductCategoryMembersModal";
import {MemoizedFormComboBox2} from "../../../../app/common/form/FormComboBox2";

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
                       setEditMode,
                       onProductCreated,
                     }: Props) {
  console.log(`ProductForm render #${++renderCount}`);

  const [createProduct, { isLoading: isCreating }] = useAddProductMutation();
  const [updateProduct, { isLoading: isUpdating }] = useUpdateProductMutation();
  const { getTranslatedLabel } = useTranslationHelper();

  const formRef = useRef<boolean>(false);
  const [createdProduct, setCreatedProduct] = useState<Product | null>(null);
  const [selectedProductType, setSelectedProductType] = useState(
      product?.productTypeId || ""
  );
  const [buttonFlag, setButtonFlag] = useState(false);

  // Modal state
  const [manageMembersOpen, setManageMembersOpen] = useState(false);

  const { data: productTypes, isFetching: isProductTypesFetching } =
      useFetchProductTypesQuery(undefined);
  const { data: productCategories, isFetching: isProductCategoriesFetching } =
      useFetchProductCategoriesQuery(undefined);
  const { data: productCategoriesFlat, isFetching: isProductCategoriesFlatFetching } =
      useFetchProductCategoriesFlatQuery(undefined);
  const {
    data: productCategoriesRawMaterials,
    isFetching: isProductCategoriesFetchingRawMaterials,
  } = useFetchProductCategoriesRawMaterialsQuery(undefined);

  const memoizedProductTypes = useMemo(
      () => productTypes || [],
      [productTypes]
  );

  const memoizedCategoryData = useMemo(() => {
    return selectedProductType === "RAW_MATERIAL"
        ? productCategoriesRawMaterials || []
        : productCategoriesFlat || [];
  }, [
    selectedProductType,
    productCategoriesFlat,
    productCategoriesRawMaterials,
  ]);

  const formInitialValues = useMemo(() => {
    const base = editMode === 1 ? {} : product || {};

    // Only when we are editing (editMode === 2) and a project is attached
    if (editMode === 2 && base.projectId && base.projectName) {
      return {
        ...base,
        projectId: {
          projectId: base.projectId,          // the ID the API expects
          projectName: base.projectName,      // the text shown in the dropdown
          // any other fields the combo-box uses can be added here if needed
        },
      };
    }

    return base;
  }, [editMode, product]);
  
  console.log("Form Initial Values:", formInitialValues);

  const floorOptions = useMemo(
      () => [
        { floorNumber: "0", description: "الطابق الأرضي" },   // Ground
        { floorNumber: "1", description: "الطابق الأول" },    // 1st
        { floorNumber: "2", description: "الطابق الثاني" },   // 2nd
        { floorNumber: "3", description: "الطابق الثالث" },   // 3rd
        { floorNumber: "4", description: "الطابق الرابع" },   // 4th
        { floorNumber: "5", description: "الطابق الخامس" },   // 5th
        { floorNumber: "6", description: "الطابق السادس" }    // 6th
      ],
      []
  );


  const handleResetForm = (formRenderProps: any) => {
    formRef.current = !formRef.current;
    setCreatedProduct(null);
    setSelectedProductType("");
    setButtonFlag(false);
    if (setEditMode) setEditMode(1);

    if (formRenderProps) {
      formRenderProps.onFormReset();
      // REFACTOR: Explicitly set numeric fields to null (not "")
      const numericFields = [
        "apartmentSpaceM2",
        "gardenSpaceM2",
        "apartmentPricePerM2",
        "gardenPricePerM2",
        "quantityIncluded",
        "piecesIncluded",
      ];
      numericFields.forEach((f) => formRenderProps.onChange(f, { value: null }));
    }
  };
  

  const handleProductTypeChange = (e: any, formRenderProps: any) => {
    const newProductType = e.value || "";
    setSelectedProductType(newProductType);
    if (formRenderProps) {
      formRenderProps.onChange("primaryProductCategoryId", { value: null });
      if (newProductType !== "APARTMENT") {
        const apartmentFields = [
          "projectId",
          "floorNumber",
          "apartmentSpaceM2",
          "gardenSpaceM2",
          "gardenPricePerM2",
          "apartmentPricePerM2",
          "apartmentStatusId",
          "buildingNumber",
        ];
        apartmentFields.forEach((field) =>
            formRenderProps.onChange(field, { value: "" })
        );
      }
    }
  };


  async function handleSubmitData(data: any) {
    setButtonFlag(true);

    try {
      // -----------------------------------------------------------------
      // REFACTOR: Normalise numeric fields – empty string → null
      // Purpose: Prevent JsonException on the server side
      // Why: System.Text.Json cannot convert "" → decimal?
      // -----------------------------------------------------------------
      const normalize = (obj: any): any => {
        const copy = { ...obj };
        const numericFields = [
          "apartmentSpaceM2",
          "gardenSpaceM2",
          "apartmentPricePerM2",
          "gardenPricePerM2",
          "quantityIncluded",
          "piecesIncluded",
        ] as const;

        numericFields.forEach((field) => {
          if (copy[field] === "" || copy[field] == null) {
            copy[field] = null;               // server expects null
          }
        });
        return copy;
      };

      const payload = normalize(data);

      if (payload.projectId && typeof payload.projectId === "object") {
        payload.projectId = payload.projectId.projectId || null;
      }

      // -----------------------------------------------------------------
      // REFACTOR: Use the wrapped payload (already done in RTK-Query)
      // -----------------------------------------------------------------
      if (editMode === 2) {
        await updateProduct(payload).unwrap();
        toast.success(getTranslatedLabel("product.products.form.updateSuccess", "Product updated successfully"));
      } else {
        console.log("Submitting new product with payload:", payload);
        const newProduct = await createProduct(payload).unwrap();
        setCreatedProduct(newProduct);
        toast.success(getTranslatedLabel("product.products.form.createSuccess", "Product created successfully"));
        if (setEditMode) setEditMode(2);
        if (onProductCreated) onProductCreated(newProduct);
      }
    } catch (error: any) {
      console.error("Product submission failed:", error);
      toast.error(
          error?.data?.errors
              ? Object.values(error.data.errors).flat().join(" ")
              : getTranslatedLabel("product.products.form.error", "Failed to process product")
      );
    } finally {
      setButtonFlag(false);
    }
  }

  const isApartment = selectedProductType === "APARTMENT";

  return (
      <>
        <CatalogMenu selectedMenuItem="/products" />
        <Paper elevation={5} className="div-container-withBorderCurved">
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <Box display="flex" justifyContent="space-between">
                <Typography
                    color={product?.productName ? "black" : "green"}
                    sx={{ p: 2 }}
                    variant="h4"
                >
                  {product?.productName
                      ? product.productName
                      : getTranslatedLabel(
                          "product.products.form.new",
                          "New Product"
                      )}
                </Typography>
              </Box>
            </Grid>
          </Grid>

          <Form
              key={editMode === 2 ? product?.productId : "new-product-form"}
              initialValues={formInitialValues}
              onSubmit={handleSubmitData}
              //validator={formValidator}
              render={(formRenderProps) => (
                  <FormElement>
                    <fieldset className="k-form-fieldset">
                      <Grid container spacing={2}>
                        <Grid item xs={3}>
                          <Field
                              id="productId"
                              name="productId"
                              label={getTranslatedLabel(
                                  "product.products.form.id",
                                  "Product ID"
                              )}
                              hint={getTranslatedLabel(
                                  "product.products.form.idHint",
                                  "Leave blank to auto-generate"
                              )}
                              component={FormInput}
                              autoComplete="off"
                              disabled={editMode === 2}
                          />
                        </Grid>
                        <Grid item xs={9}>
                          <Field
                              id="productName"
                              name="productName"
                              label={getTranslatedLabel(
                                  "product.products.form.name",
                                  "Product Name *"
                              )}
                              component={FormInput}
                              autoComplete="off"
                              validator={requiredValidator}
                          />
                        </Grid>
                      </Grid>

                      <Grid container spacing={2}>
                        <Grid item xs={6}>
                          <Field
                              id="productTypeId"
                              name="productTypeId"
                              label={getTranslatedLabel(
                                  "product.products.form.type",
                                  "Product Type *"
                              )}
                              component={MemoizedFormDropDownList}
                              dataItemKey="productTypeId"
                              textField="description"
                              data={memoizedProductTypes}
                              validator={requiredValidator}
                              onChange={(e: any) =>
                                  handleProductTypeChange(e, formRenderProps)
                              }
                          />
                        </Grid>
                        <Grid item xs={6}>
                          <Field
                              key={`category-${selectedProductType}`}
                              id="primaryProductCategoryId"
                              name="primaryProductCategoryId"
                              label={getTranslatedLabel(
                                  "product.products.form.category",
                                  "Product Category *"
                              )}
                              data={memoizedCategoryData || []}
                              component={MemoizedFormComboBox2}
                              dataItemKey="productCategoryId"
                              textField="description"
                              validator={requiredValidator}
                          />
                        </Grid>
                      </Grid>

                      {editMode === 2 && product && (
                          <Grid container spacing={2} mt={2}>
                            <Grid item xs={12}>
                              <Button
                                  variant="outlined"
                                  color="secondary"
                                  onClick={() => setManageMembersOpen(true)}
                              >
                                {getTranslatedLabel("product.products.form.manageCategories", "Manage Additional Categories")}
                              </Button>
                            </Grid>
                          </Grid>
                      )}

                      {/* REFACTOR: Apartment fields with FormNumericTextBox */}
                      {/* Purpose: Use Kendo numeric input for precision, formatting, and validation */}
                      {/* Why: Prevents invalid input, enforces 2 decimals, improves UX */}
                      {isApartment && (
                          <>
                            <Grid container spacing={2} mt={1}>
                              <Grid item xs={6}>
                                <Field
                                    id="projectId"
                                    name="projectId"
                                    component={FormComboBoxVirtualProject}
                                    label={getTranslatedLabel(
                                        "projects.certificate.form.project",
                                        "Project *"
                                    )}
                                    dataItemKey="projectId"
                                    textField="ProjectName"
                                    validator={requiredValidator}
                                    disabled={editMode > 3}
                                />
                              </Grid>
                              <Grid item xs={6}>
                                <Field
                                    id="floorNumber"
                                    name="floorNumber"
                                    label={getTranslatedLabel(
                                        "product.products.form.floorNumber",
                                        "Floor Number *"
                                    )}
                                    component={MemoizedFormDropDownList}
                                    dataItemKey="floorNumber"
                                    textField="description"
                                    data={floorOptions}
                                    validator={requiredValidator}
                                />
                              </Grid>
                            </Grid>

                            {/* REFACTOR: Numeric fields using FormNumericTextBox */}
                            <Grid container spacing={2}>
                              <Grid item xs={3}>
                                <Field
                                    id="apartmentSpaceM2"
                                    name="apartmentSpaceM2"
                                    label={getTranslatedLabel(
                                        "product.products.form.apartmentSpaceM2",
                                        "Apartment Space (m²) *"
                                    )}
                                    format="n2"
                                    min={0}
                                    component={FormNumericTextBox}
                                    validator={requiredValidator}
                                />
                              </Grid>
                              <Grid item xs={3}>
                                <Field
                                    id="gardenSpaceM2"
                                    name="gardenSpaceM2"
                                    label={getTranslatedLabel(
                                        "product.products.form.gardenSpaceM2",
                                        "Garden Space (m²)"
                                    )}
                                    format="n2"
                                    min={0}
                                    component={FormNumericTextBox}
                                />
                              </Grid>
                              <Grid item xs={3}>
                                <Field
                                    id="apartmentPricePerM2"
                                    name="apartmentPricePerM2"
                                    label={getTranslatedLabel(
                                        "product.products.form.apartmentPricePerM2",
                                        "Apartment Price per m² *"
                                    )}
                                    format="n2"
                                    min={0}
                                    component={FormNumericTextBox}
                                    validator={requiredValidator}
                                />
                              </Grid>
                              <Grid item xs={3}>
                                <Field
                                    id="gardenPricePerM2"
                                    name="gardenPricePerM2"
                                    label={getTranslatedLabel(
                                        "product.products.form.gardenPricePerM2",
                                        "Garden Price per m²"
                                    )}
                                    format="n2"
                                    min={0}
                                    component={FormNumericTextBox}
                                />
                              </Grid>
                            </Grid>

                            <Grid container spacing={2}>
                              <Grid item xs={6}>
                                <Field
                                    id="apartmentStatusId"
                                    name="apartmentStatusId"
                                    label={getTranslatedLabel(
                                        "product.products.form.apartmentStatus",
                                        "Apartment Status *"
                                    )}
                                    component={MemoizedFormDropDownList}
                                    dataItemKey="apartmentStatusId"
                                    textField="name"
                                    data={[
                                      { apartmentStatusId: "APARTMENT_AVAILABLE", name: "متاح" },
                                      { apartmentStatusId: "APARTMENT_RESERVED", name: "محجوز" },
                                      { apartmentStatusId: "APARTMENT_SOLD", name: "مباع" },
                                    ]}
                                    validator={requiredValidator}
                                />
                              </Grid>
                              <Grid item xs={6}>
                                <Field
                                    id="buildingNumber"
                                    name="buildingNumber"
                                    label={getTranslatedLabel(
                                        "product.products.form.buildingNumber",
                                        "Land Number *"
                                    )}
                                    component={FormInput}
                                    autoComplete="off"
                                    validator={requiredValidator}
                                />
                              </Grid>
                            </Grid>
                          </>
                      )}

                      <Grid item xs={12} mt={2}>
                        <Field
                            id="comments"
                            name="comments"
                            label={getTranslatedLabel(
                                "product.products.form.comments",
                                "Comments"
                            )}
                            autoComplete="off"
                            rows={3}
                            component={FormTextArea}
                        />
                      </Grid>

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
                              {getTranslatedLabel(
                                  "product.products.form.createNew",
                                  "Clear Form"
                              )}
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
                          isProductCategoriesFlatFetching ||
                          isProductCategoriesFetchingRawMaterials) && (
                          <LoadingComponent message="Loading Data..." />
                      )}
                    </fieldset>
                  </FormElement>
              )}
          />
        </Paper>

        {editMode === 2 && product && (
            <ManageProductCategoryMembersModal
                productId={product.productId}
                productName={product.productName || ""}
                open={manageMembersOpen}
                onClose={() => setManageMembersOpen(false)}
            />
        )}
      </>
  );
}

const areProductFormPropsEqual = (prev: Props, next: Props) => {
  return (
      prev.product?.productId === next.product?.productId &&
      prev.product?.productName === next.product?.productName &&
      prev.product?.productTypeId === next.product?.productTypeId &&
      prev.editMode === next.editMode &&
      prev.cancelEdit === next.cancelEdit &&
      prev.setEditMode === next.setEditMode &&
      prev.onProductCreated === next.onProductCreated
  );
};

export default React.memo(ProductForm, areProductFormPropsEqual);