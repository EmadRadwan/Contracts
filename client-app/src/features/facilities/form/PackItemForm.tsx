import React from "react";
import { OrderItem } from "../../../app/models/order/orderItem";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { Button, Grid, Typography } from "@mui/material";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { requiredValidator } from "../../../app/common/form/Validators";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { GridItemChangeEvent } from "@progress/kendo-react-grid";
import { toast } from "react-toastify";
import { MemoizedFormCheckBox } from "../../../app/common/form/FormCheckBox";

interface PackItemFormProps {
  orderItem?: OrderItem;
  handleSubmit: (orderItem: OrderItem) => void;
  onClose: () => void;
}

const PackItemForm = ({ orderItem, handleSubmit, onClose }: PackItemFormProps) => {
  const { getTranslatedLabel } = useTranslationHelper();
  const formRef = React.useRef<any>();

  // Calculate remaining quantity: ordered minus shipped.
  const remainingQty = (orderItem?.quantity ?? 0) - (orderItem?.shippedQuantity ?? 0);

  // Ensure the value for quantityToShip does not exceed the remaining quantity.
  const handleAmountsChange = (e: GridItemChangeEvent) => {
    const field = e.target.element!.id;
    const value = e.value;
    if (field === "quantityToShip" && value > remainingQty) {
      formRef?.current.onChange("quantityToShip", { value: remainingQty });
    }
  };

  // Custom validator for quantityToShip: must be at least 1 and no greater than available.
  const quantityToShipValidator = (value: number) => {
    const orderedQty = formRef?.current?.valueGetter("quantity") || 0;
    const shippedQty =
        formRef?.current?.valueGetter("shippedQuantity") ||
        formRef?.current?.valueGetter("shippedQty") ||
        0;
    const availableQty = orderedQty - shippedQty;

    if (!value || value < 1) {
      toast.error("Must enter a quantity to ship greater than zero.", {
        style: { whiteSpace: "pre-line" },
      });
      return "Must enter a quantity to ship greater than zero.";
    }
    if (value > availableQty) {
      toast.error(
          `Quantity to ship cannot exceed remaining quantity (${availableQty}).`,
          { style: { whiteSpace: "pre-line" } }
      );
      return `Quantity to ship cannot exceed remaining quantity (${availableQty}).`;
    }
    return undefined;
  };

  const onSubmit = async (data: any) => {
    const orderedQty = data["quantity"] || 0;
    const shippedQty =
        data["shippedQuantity"] || data["shippedQty"] || 0;
    const toShipQty = data["quantityToShip"] || 0;
    const availableQty = orderedQty - shippedQty;

    const errors: string[] = [];

    if (toShipQty <= 0) {
      errors.push("Must enter a quantity to ship greater than zero.");
    }

    if (toShipQty > availableQty) {
      errors.push(
          `Quantity to ship cannot exceed remaining quantity (${availableQty}).`
      );
    }

    if (errors.length > 0) {
      toast.error(errors.map((e) => `• ${e}`).join("\n"), {
        style: { whiteSpace: "pre-line" },
      });
      return;
    }

    handleSubmit({ ...data });
  };

  // Set default initial values
  const initialValues = {
    ...orderItem,
    quantityToShip: remainingQty > 0 ? remainingQty : 1,
  };

  return (
      <>
        {orderItem && (
            <Typography variant="h4" color={"black"}>
              {orderItem.productName}
            </Typography>
        )}
        <Form
            initialValues={initialValues}
            ref={formRef}
            onSubmit={(values) => onSubmit(values as OrderItem)}
            render={(formRenderProps) => (
                <FormElement>
                  <fieldset className={"k-form-fieldset"}>
                    <Grid
                        container
                        spacing={2}
                        alignItems={"center"}
                        justifyContent={"center"}
                    >
                      <Grid item xs={3}>
                        <Field
                            id="quantity"
                            name="quantity"
                            label={getTranslatedLabel(
                                "facility.pack.form.quantityOrdered",
                                "Quantity Ordered"
                            )}
                            component={FormNumericTextBox}
                            disabled
                            autoComplete="off"
                            validator={requiredValidator}
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <Field
                            id="shippedQuantity"
                            name="shippedQuantity"
                            label={getTranslatedLabel(
                                "facility.pack.form.shippedQty",
                                "Quantity Already Shipped"
                            )}
                            component={FormNumericTextBox}
                            disabled
                            autoComplete="off"
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <Field
                            id="quantityToShip"
                            name="quantityToShip"
                            label={getTranslatedLabel(
                                "facility.receive.form.qtyToShip",
                                "Quantity To Ship"
                            )}
                            component={FormNumericTextBox}
                            min={1}
                            max={orderItem?.quantity}
                            autoComplete="off"
                            onChange={handleAmountsChange}
                            validator={quantityToShipValidator}
                        />
                      </Grid>
                      <Grid item xs={3} mt={2}>
                        <Field
                            id="includeThisItem"
                            name="includeThisItem"
                            label="Include"
                            hint="Hint: Include this item in order shipment"
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
                          <Button
                              onClick={onClose}
                              variant="contained"
                              color="error"
                          >
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

export default PackItemForm;