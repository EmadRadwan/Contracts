import React, { useState } from "react";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import { ComboBoxChangeEvent } from "@progress/kendo-react-dropdowns";
import { Typography } from "@mui/material";
import {CertificateItem} from "../../../app/models/project/certificateItem";
import FormTextArea from "../../../app/common/form/FormTextArea";
import {percentageValidator, requiredValidator} from "../../../app/common/form/Validators";
import {
    FormMultiColumnComboBoxVirtualPurchaseProduct
} from "../../../app/common/form/FormMultiColumnComboBoxVirtualPurchaseProduct";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";

// REFACTOR: Define props
// Purpose: Type safety for form props
// Context: Adapted from PurchaseOrderItemForm, uses CertificateItem and formEditMode
interface Props {
  certificateItem?: CertificateItem;
  editMode: number; // 1: add, 2: edit
  onClose: () => void;
  formEditMode: number; // 0: view, 1: create, 2: CREATED, 3: APPROVED, 4: COMPLETED
  updateCertificateItems: (certificateItem: CertificateItem, editMode: number) => void;
}

// REFACTOR: Main form component
// Purpose: Form for adding/editing certificate items
// Context: Modeled after PurchaseOrderItemForm, added description, completionPercentage, notes
export default function CertificateItemForm({ certificateItem, editMode, onClose, formEditMode, updateCertificateItems }: Props) {
  const [lastPrice, setLastPrice] = useState("");
  const [selectedProduct, setSelectedProduct] = useState<any>(undefined);
  const MyForm = React.useRef<any>();
  const [formKey, setFormKey] = useState(1);
  const [initValue, setInitValue] = useState<CertificateItem | undefined>(certificateItem);

  // REFACTOR: Initialize form handling
  // Purpose: Handle submission and reset form state
  // Context: Uses custom hook for API integration
  const { handleSubmitData } = useCertificateItem({ certificateItem, editMode, setFormKey, setInitValue, updateCertificateItems });

  // REFACTOR: Handle product selection
  // Purpose: Update lastPrice on product change
  // Context: Matches PurchaseOrderItemForm's onCloseCombo
  const onCloseCombo = (event: ComboBoxChangeEvent) => {
    if (event?.target?.value) {
      setLastPrice(event.target.value.lastPrice);
    }
  };

  // REFACTOR: Calculate totalAmount
  // Purpose: Compute totalAmount from quantity and unitPrice
  // Context: Added to display calculated total
  const calculateTotalAmount = (values: Partial<CertificateItem>) => {
    const quantity = values.quantity || 0;
    const unitPrice = values.unitPrice || 0;
    return Math.round(quantity * unitPrice * 100) / 100;
  };

  return (
    <Form
      ref={MyForm}
      initialValues={initValue}
      key={formKey}
      onSubmit={(values: any) => {
        const totalAmount = calculateTotalAmount(values);
        handleSubmitData({ ...values, totalAmount } as CertificateItem);
        setLastPrice("");
        setSelectedProduct(undefined);
      }}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className="k-form-fieldset">
            <Grid container spacing={2} direction="column">
              {/* REFACTOR: Description field */}
              {/* Purpose: Input item description */}
              {/* Context: Added for CertificateItem */}
              <Grid item xs={12}>
                <Field
                  id="description"
                  name="description"
                  label="Description *"
                  component={FormTextArea}
                  validator={requiredValidator}
                  disabled={formEditMode > 3}
                />
              </Grid>
              {/* REFACTOR: Product dropdown */}
              {/* Purpose: Select optional product */}
              {/* Context: Reuses FormMultiColumnComboBoxVirtualPurchaseProduct */}
              <Grid item xs={12}>
                <Field
                  id="productId"
                  name="productId"
                  label="Product"
                  component={FormMultiColumnComboBoxVirtualPurchaseProduct}
                  autoComplete="off"
                  onClose={onCloseCombo}
                  onChange={(e: ComboBoxChangeEvent) => {
                    if (e.value === null || e.value === undefined) {
                      setLastPrice("");
                    }
                    setSelectedProduct(e.value);
                  }}
                  disabled={editMode === 2}
                />
              </Grid>
              {/* REFACTOR: Quantity and UOM */}
              {/* Purpose: Input quantity with UOM display */}
              {/* Context: Matches PurchaseOrderItemForm */}
              <Grid item container xs={12} spacing={2} alignItems="flex-end">
                <Grid item xs={8}>
                  <Field
                    id="quantity"
                    format="n0"
                    min={1}
                    name="quantity"
                    label="Quantity *"
                    component={FormNumericTextBox}
                    validator={requiredValidator}
                    disabled={formEditMode > 3}
                  />
                </Grid>
                {selectedProduct && (
                  <Grid item xs={4}>
                    <Typography variant="h6" color="blue" fontWeight="bold">
                      {selectedProduct?.uomDescription || ""}
                    </Typography>
                  </Grid>
                )}
              </Grid>
              {/* REFACTOR: Unit price and last price */}
              {/* Purpose: Input unit price and show last price */}
              {/* Context: Matches PurchaseOrderItemForm */}
              <Grid item container xs={12} spacing={2}>
                <Grid item xs={8}>
                  <Field
                    id="unitPrice"
                    format="n2"
                    min={0.1}
                    name="unitPrice"
                    label="Unit Price *"
                    component={FormNumericTextBox}
                    validator={requiredValidator}
                    disabled={formEditMode > 3}
                  />
                </Grid>
                {lastPrice && (
                  <Grid item xs={4}>
                    <Typography>Last Price: {lastPrice}</Typography>
                  </Grid>
                )}
              </Grid>
              {/* REFACTOR: Total amount */}
              {/* Purpose: Display calculated total */}
              {/* Context: Read-only, calculated from quantity * unitPrice */}
              <Grid item xs={12}>
                <Field
                  id="totalAmount"
                  name="totalAmount"
                  label="Total Amount"
                  component={FormNumericTextBox}
                  format="n2"
                  value={calculateTotalAmount(formRenderProps.valueGetter)}
                  disabled
                />
              </Grid>
              {/* REFACTOR: Completion percentage */}
              {/* Purpose: Input completion percentage */}
              {/* Context: Added for CertificateItem, 0-100 */}
              <Grid item xs={12}>
                <Field
                  id="completionPercentage"
                  name="completionPercentage"
                  label="Completion Percentage *"
                  component={FormNumericTextBox}
                  format="n0"
                  min={0}
                  max={100}
                  validator={percentageValidator}
                  disabled={formEditMode > 3}
                />
              </Grid>
              {/* REFACTOR: Notes field */}
              {/* Purpose: Optional notes input */}
              {/* Context: Added for CertificateItem */}
              <Grid item xs={12}>
                <Field
                  id="notes"
                  name="notes"
                  label="Notes"
                  component={FormTextArea}
                  disabled={formEditMode > 3}
                />
              </Grid>
              {/* REFACTOR: Form buttons */}
              {/* Purpose: Submit or cancel form */}
              {/* Context: Matches PurchaseOrderItemForm */}
              <Grid item xs={12}>
                <div className="k-form-buttons">
                  <Grid container spacing={2}>
                    <Grid item xs={5}>
                      <Button
                        variant="contained"
                        type="submit"
                        color="success"
                        disabled={!formRenderProps.allowSubmit || formEditMode > 3}
                      >
                        {editMode === 2 ? "Update" : "Add"}
                      </Button>
                    </Grid>
                    <Grid item xs={2}>
                      <Button
                        onClick={() => {
                          setLastPrice("");
                          setSelectedProduct(undefined);
                          onClose();
                        }}
                        variant="contained"
                        color="error"
                      >
                        Cancel
                      </Button>
                    </Grid>
                  </Grid>
                </div>
              </Grid>
            </Grid>
          </fieldset>
        </FormElement>
      )}
    />
  );
}

// REFACTOR: Memoize component
// Purpose: Optimize performance by preventing unnecessary re-renders
// Context: Matches PurchaseOrderItemFormMemo
export const CertificateItemFormMemo = React.memo(CertificateItemForm);