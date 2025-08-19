import React from "react";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { Button, Grid, Typography } from "@mui/material";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import { requiredValidator } from "../../../app/common/form/Validators";
import { toast } from "react-toastify";
import { useFetchInventoryItemLotsQuery } from "../../../app/store/apis";
import {MemoizedFormCheckBox} from "../../../app/common/form/FormCheckBox"; // To be created

interface ProductionRunComponentDto {
  productId: string;
  productName: string;
  estimatedQuantity: number;
  workEffortId: string;
  workEffortName: string;
  fromDate: string | null;
  lotId: string | null;
  issuedQuantity: number;
  returnedQuantity: number;
  quantityToReturn?: number;
  includeThisItem?: boolean;
}

export const numberValidator = (value: any, label?: string): string | undefined => {
  const fieldName = label || "This field";

  // Check if value is undefined, null, or empty string
  if (value === undefined || value === null || value === "") {
    return `${fieldName} must be a number.`;
  }

  // Convert to number and check if it's a valid number
  const numValue = Number(value);
  if (isNaN(numValue)) {
    return `${fieldName} must be a valid number.`;
  }

  // Optional: Enforce non-negative numbers (common for quantities)
  if (numValue < 0) {
    return `${fieldName} cannot be negative.`;
  }

  return undefined; // Valid
};

interface ReturnInventoryItemFormProps {
  component: ProductionRunComponentDto;
  handleSubmit: (component: ProductionRunComponentDto) => void;
  onClose: () => void;
}

const ReturnInventoryItemForm = ({
  component,
  handleSubmit,
  onClose,
}: ReturnInventoryItemFormProps) => {
  const { getTranslatedLabel } = useTranslationHelper();
  const formRef = React.useRef<any>();
  const { data: availableLots } = useFetchInventoryItemLotsQuery({
    productId: component.productId,
    workEffortId: component.workEffortId,
  });

  const onSubmit = async (data: any) => {
    const item = { ...data };
    const quantityToReturn = item.quantityToReturn || 0;
    const lotId = item.lotId;
    const maxReturnable = component.issuedQuantity - component.returnedQuantity;

    const errors = [];
    if (quantityToReturn <= 0) {
      errors.push("Quantity to return must be greater than 0.");
    }
    if (quantityToReturn > maxReturnable) {
      errors.push(`Quantity to return cannot exceed ${maxReturnable}.`);
    }
    console.log("availableLots", availableLots);
   
    if (availableLots.length > 0 && !availableLots.some((lot: any) => lot.lotId === lotId)) {
      errors.push("Invalid Lot ID.");
    }

    if (errors.length > 0) {
      toast.error(errors.map((e) => `• ${e}`).join("\n"), {
        style: { whiteSpace: "pre-line" },
      });
      return;
    }

    handleSubmit({
      ...component,
      lotId,
      quantityToReturn,
      includeThisItem: item.includeThisItem,
    });
    onClose();
  };

  return (
    <>
      <Typography variant="h4" color="black">
        {`${component.productName} `}
      </Typography>
      
      <Form
        initialValues={{
          lotId: component.lotId,
          quantityToReturn: component.quantityToReturn || 0,
          includeThisItem: component.includeThisItem || false,
          issuedQuantity: component.issuedQuantity,
          returnedQuantity: component.returnedQuantity,
        }}
        ref={formRef}
        onSubmit={onSubmit}
        render={(formRenderProps) => (
          <FormElement>
            <fieldset className="k-form-fieldset">
              <Grid container spacing={2} alignItems="center" justifyContent="center">
                <Grid item xs={2}>
                  <Field
                    id="issuedQuantity"
                    name="issuedQuantity"
                    label={getTranslatedLabel(
                      "manufacturing.return.issuedQuantity",
                      "Issued Quantity"
                    )}
                    component={FormNumericTextBox}
                    disabled
                    autoComplete="off"
                  />
                </Grid>
                <Grid item xs={2}>
                  <Field
                    id="returnedQuantity"
                    name="returnedQuantity"
                    label={getTranslatedLabel(
                      "manufacturing.return.returnedQuantity",
                      "Returned Quantity"
                    )}
                    component={FormNumericTextBox}
                    disabled
                    autoComplete="off"
                  />
                </Grid>
                <Grid item xs={2}>
                  <Field
                    id="quantityToReturn"
                    name="quantityToReturn"
                    label={getTranslatedLabel(
                      "manufacturing.return.quantityToReturn",
                      "Quantity to Return"
                    )}
                    component={FormNumericTextBox}
                    min={0}
                    max={component.issuedQuantity - component.returnedQuantity}
                    autoComplete="off"
                    validator={[requiredValidator, numberValidator]}
                  />
                </Grid>
                <Grid item xs={3}>
                  <Field
                    id="lotId"
                    name="lotId"
                    label={getTranslatedLabel("manufacturing.return.lotId", "Lot ID")}
                    component={MemoizedFormDropDownList2}
                    data={availableLots || []}
                    dataItemKey="lotId"
                    textField="lotId"
                    autoComplete="off"
                  />
                </Grid>
                <Grid item xs={3} mt={2}>
                  <Field
                    id="includeThisItem"
                    name="includeThisItem"
                    label={getTranslatedLabel("manufacturing.return.include", "Include")}
                    hint="Hint: Include this item in return"
                    component={MemoizedFormCheckBox}
                  />
                </Grid>
              </Grid>
              <div className="k-form-buttons">
                <Grid container spacing={1}>
                  <Grid item>
                    <Button
                      type="submit"
                      color="success"
                      variant="contained"
                      disabled={!formRenderProps.allowSubmit}
                    >
                      Save
                    </Button>
                  </Grid>
                  <Grid item>
                    <Button onClick={onClose} variant="contained" color="error">
                      Cancel
                    </Button>
                  </Grid>
                </Grid>
              </div>
            </fieldset>
          </FormElement>
        )}
      />
    </>
  );
};

export default ReturnInventoryItemForm;