import React, { useState, useEffect, useRef } from "react";
import { Button, Grid, Paper, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { requiredValidator } from "../../../app/common/form/Validators";
import { MemoizedFormDropDownList } from "../../../app/common/form/MemoizedFormDropDownList";
import {
  Grid as KendoGrid,
  GridColumn as Column,
} from "@progress/kendo-react-grid";
import { BillOfMaterial } from "../../../app/models/manufacturing/billOfMaterial";
import {
  useFetchCompanyBaseCurrencyQuery,
  useFetchCurrenciesQuery,
  useFetchProductQuantityUomQuery,
  useFetchSimulatedBomCostQuery,
} from "../../../app/store/configureStore";
import { ExcelExport } from "@progress/kendo-react-excel-export";
import { handleDatesArray } from "../../../app/util/utils";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";

interface BOMSimulationFormProps {
  selectedProduct: BillOfMaterial | undefined;
  onClose: () => void;
}


// Row render for custom styling
const rowRender = (trElement, props) => {
  const isMainProduct = props.dataItem.productLevel === 0;
  const trProps = {
    ...trElement.props,
    style: {
      ...trElement.props.style,
      backgroundColor: isMainProduct ? "#e0f7fa" : "inherit",
    },
  };
  // Clone children to apply cost styling
  const children = React.Children.map(trElement.props.children, (child) => {
    if (child.props.field === "totalCost" && isMainProduct){
      return React.cloneElement(child, {
        style: {
          ...child.props.style,
          fontWeight: "bold",
          color: "#ff0000",
        },
      });
    }
    return child;
  });
  return React.cloneElement(trElement, trProps, children);
};


const BOMSimulationForm = ({
  selectedProduct,
  onClose,
}: BOMSimulationFormProps) => {
  const [buttonFlag, setButtonFlag] = useState(false);
  const [formValues, setFormValues] = useState<{
    quantityToProduce?: number;
    currencyUomId?: string;
  }>({});
  const { getTranslatedLabel } = useTranslationHelper();
  const { data: currencies } = useFetchCurrenciesQuery(undefined);
  const { data: baseCurrency } = useFetchCompanyBaseCurrencyQuery(undefined);
  const [formKey, setFormKey] = useState(true);
  const _export = useRef(null);

  const { data, isSuccess, isFetching } = useFetchSimulatedBomCostQuery(
    {
      productId: selectedProduct?.productId!,
      quantityToProduce: formValues.quantityToProduce!,
      currencyUomId: formValues.currencyUomId!,
    },
    { skip: !formValues.quantityToProduce || !formValues.currencyUomId }
  );

  const { data: uomDescription } = useFetchProductQuantityUomQuery(
    { productId: selectedProduct?.productId! },
    {
      skip:
        !selectedProduct ||
        selectedProduct?.productId === undefined ||
        selectedProduct?.productId === null,
    }
  );

  // Filter out IsTemplateLink records and find template link for display
  const gridData = data
      ? data
          .filter(item => !item.isTemplateLink)
          .map(item => ({
            ...item,
            unitCost: item.productLevel === 0 ? item.cost / item.quantity : item.cost,
            totalCost: item.productLevel === 0 ? item.cost : item.cost * item.quantity,
          }))
      : [];
  
  
  
  const templateLink = data ? data.find(item => item.isTemplateLink) : null;


  const handleSubmit = (values: {
    quantityToProduce: number;
    currencyUomId: string;
  }) => {
    setFormValues(values);
    setButtonFlag(true);
  };

  useEffect(() => {
    if (data) {
      setButtonFlag(false);
      console.log(data);
    }
  }, [data]);

  useEffect(() => {
    if (baseCurrency) setFormKey(!formKey);
  }, [baseCurrency]);

  return (
    <>
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container padding={2} columnSpacing={1}>
          <Grid container alignItems="center">
            <Grid item xs={12} justifyContent={"center"}>
              <Typography sx={{ p: 2, textAlign: "center"}} variant="h5">
                <span>
                  {getTranslatedLabel(
                    "manufacturing.bom.simulation.bomSimulationFor",
                    "BOM Simulation for"
                  )}{" "}
                </span>
                <span>{selectedProduct?.productName}</span>
                <br />
              </Typography>
            </Grid>
          </Grid>
          <Grid item xs={12}>
            <Form
              onSubmit={handleSubmit}
              key={formKey.toString()}
              initialValues={
                baseCurrency?.length > 0
                  ? { currencyUomId: baseCurrency[0].currencyUomId! }
                  : { currencyUomId: "EGP" } 
              }
              render={(formRenderProps) => (
                <FormElement>
                  <fieldset className={"k-form-fieldset"}>
                    <Grid container spacing={1}>
                      <Grid item xs={6} direction={"column"}>
                        <Grid item xs={12}>
                          <Field
                              name="quantityToProduce"
                              id="quantityToProduce"
                              min={0} // Allow decimals ≥ 0
                              //defaultValue={1}
                              format="n2" // Display with 3 decimal places
                              component={FormNumericTextBox}
                              validator={(value) => {
                                // Combine requiredValidator and positive validator
                                const requiredError = requiredValidator(value);
                                if (requiredError) return requiredError;
                                if (value <= 0) return "Quantity must be greater than 0";
                                return undefined;
                              }}
                              label={getTranslatedLabel(
                                  "manufacturing.bom.simulation.quantity",
                                  "Quantity *"
                              )}
                          />
                        </Grid>
                        <Grid item xs={2}>
                          <Typography sx={{ p: 1, color: "blue" }} variant="h6">
                            {uomDescription}
                          </Typography>
                        </Grid>
                      </Grid>
                      <Grid item xs={6}>
                        <Field
                          name="currencyUomId"
                          id="currencyUomId"
                          component={MemoizedFormDropDownList}
                          data={currencies ? currencies : []}
                          label={getTranslatedLabel(
                            "manufacturing.bom.simulation.currencyUomId",
                            "Currency"
                          )}
                          dataItemKey="currencyUomId"
                          textField="description"
                        />
                      </Grid>
                    </Grid>

                    <div className="k-form-buttons">
                      <Grid container spacing={3}>
                        <Grid item>
                          <Button
                            variant="contained"
                            color="success"
                            type={"submit"}
                            disabled={
                              buttonFlag ||
                              !formRenderProps.valueGetter("quantityToProduce")
                            }
                          >
                            {getTranslatedLabel("general.submit", "Submit")}
                          </Button>
                        </Grid>
                        <Grid item>
                          <Button
                            onClick={onClose}
                            variant="contained"
                            color="error"
                          >
                            {getTranslatedLabel("general.cancel", "Cancel")}
                          </Button>
                        </Grid>
                      </Grid>
                    </div>
                  </fieldset>
                </FormElement>
              )}
            ></Form>
            {isFetching && (
                <LoadingComponent message="Calculating BOM..." />
            )}
          </Grid>
          {isSuccess && (
            <Grid container mt={2}>
              <Grid item xs={12}>
                <div className="div-container">
                  <KendoGrid style={{ height: "35vh" }}
                             data={gridData}
                             rowRender={rowRender}
                             resizable={true}
                  >
                    <Column
                      field="productLevel"
                      title={getTranslatedLabel(
                        "manufacturing.bom.simulation.productLevel",
                        "Product Level"
                      )}
                      width={70}
                    />
                    <Column
                      field="productName"
                      title={getTranslatedLabel(
                        "manufacturing.bom.simulation.product",
                        "Product"
                      )}
                      width={250}
                    />
                    <Column
                      field="qoh"
                      title={getTranslatedLabel(
                        "manufacturing.bom.simulation.qoh",
                        "Quantity on hand"
                      )}
                      format="{0:n2}" // 3 decimal places
                    />
                    <Column
                      field="unitCost"
                      title={getTranslatedLabel(
                        "manufacturing.bom.simulation.cost",
                        "Unit Cost"
                      )}
                      width={90}
                      format="{0:n2}" // 3 decimal places
                    />
                    <Column
                      field="quantity"
                      title={getTranslatedLabel(
                        "manufacturing.bom.simulation.quantity",
                        "BOM Quantity"
                      )}
                      width={90}
                      format="{0:n2}" // 3 decimal places
                    />
                    <Column
                      field="uomDescription"
                      title={getTranslatedLabel(
                        "manufacturing.bom.simulation.uom",
                        "UOM"
                      )}
                    />
                    <Column
                        field="totalCost"
                        title={getTranslatedLabel(
                            "manufacturing.bom.simulation.totalCost",
                            "Total Cost"
                        )}
                        format="{0:n2}" // 3 decimal places
                    />
                  </KendoGrid>
                </div>
              </Grid>
            </Grid>
          )}
        </Grid>
      </Paper>
    </>
  );
};

export default BOMSimulationForm;
