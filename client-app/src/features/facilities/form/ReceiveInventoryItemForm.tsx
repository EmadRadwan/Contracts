import React from "react";
import { OrderItem } from "../../../app/models/order/orderItem";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { Button, Grid, Typography } from "@mui/material";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import FormInput from "../../../app/common/form/FormInput";
import { requiredValidator } from "../../../app/common/form/Validators";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import {
  useFetchFacilitiesQuery,
  useFetchRejectionReasonsQuery,
} from "../../../app/store/apis";
import { GridItemChangeEvent } from "@progress/kendo-react-grid";
import { useFetchFacilityLocationsLovQuery } from "../../../app/store/configureStore";
import { toast } from "react-toastify";
import { MemoizedFormCheckBox } from "../../../app/common/form/FormCheckBox";

interface ReceiveInventoryItemFormProps {
  orderItem?: OrderItem;
  handleSubmit: (orderItem: OrderItem) => void;
  onClose: () => void;
}

const ReceiveInventoryItemForm = ({
  orderItem,
  handleSubmit,
  onClose,
}: ReceiveInventoryItemFormProps) => {
  const { getTranslatedLabel } = useTranslationHelper();
  const { data: rejectionReasonsData } =
    useFetchRejectionReasonsQuery(undefined);
  const { data: facilityList } = useFetchFacilitiesQuery(undefined);
  const { data: facilityLocations } =
    useFetchFacilityLocationsLovQuery(undefined);
  const formRef = React.useRef<any>();
  console.log(orderItem)

  const handleAmountsChange = (e: GridItemChangeEvent) => {
    const currentAccepted = formRef?.current.valueGetter("quantityAccepted");
    const currentRejected = formRef?.current.valueGetter("quantityRejected");
    const field = e.target.element!.id;
    const value = e.value;
    const max = orderItem?.quantity;
    if (field === "quantityAccepted" && value + currentRejected > max!) {
      formRef?.current.onChange("quantityRejected", {
        value: max! - value,
      });
    }
    if (field === "quantityRejected" && value + currentAccepted > max!) {
      formRef?.current.onChange("quantityAccepted", {
        value: max! - value,
      });
    }
  };

  const onSubmit = async (data: any) => {
    const item = { ...data };
    let quantityReceived = item["defaultQuantityToReceive"] || 0;
    let quantityRejected = item["quantityRejected"] || 0;

    const quantityOrdered = item["quantity"];
    const rejectionId = item["rejectionReasonId"];

    const noneAccepted = quantityReceived === 0;
    const receivedAndRejectedMoreThanOrdered =
      quantityReceived + quantityRejected > quantityOrdered;
    const rejectedWithoutReason =
      quantityRejected > 0 && (!rejectionId || rejectionId === "");

    const isValid =
      quantityReceived > 0 &&
      quantityReceived <= quantityOrdered - quantityRejected &&
      ((quantityRejected > 0 && rejectionId !== "") ||
        (!quantityRejected && (!rejectionId || rejectionId === "")));

    // Return a copy of the item with 'validItem' updated
    //console.log('order item in validation', item);
    console.log(isValid);
    let errors = [];
    if (noneAccepted) {
      errors.push("Must accept at least one item.");
    }
    if (receivedAndRejectedMoreThanOrdered) {
      errors.push(
        "Sum of accepted and rejected can't be more that total quanity."
      );
    }
    if (rejectedWithoutReason) {
      errors.push("Must add rejection reason for rejected items.");
    }
    if (errors.length > 0) {
      toast.error(errors.map((e) => `• ${e}`).join("\n"), {
        style: { whiteSpace: "pre-line" },
      });
      return;
    }
    if (isValid) {
      // try {
      //   let r = await receiveInventoryProduct({...item})
      //   console.log(r)
      //   onClose()
      // } catch (e) {
      //   console.log(e)
      // }
      handleSubmit({
        ...item,
        rejectionDescription: rejectionReasonsData?.find(
          (r) => r.rejectionId === rejectionId
        )?.description,
      });
      onClose();
    }
  };
  return (
    <>
      {orderItem && (
        <Typography variant="h4" color={"black"}>
          {orderItem?.productName}
        </Typography>
      )}
      <Form
        initialValues={orderItem}
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
                    id={"quantity"}
                    name={"quantity"}
                    label={getTranslatedLabel(
                      "facility.receive.form.ordered",
                      "Quantity Ordered"
                    )}
                    component={FormNumericTextBox}
                    disabled
                    autoComplete={"off"}
                    validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={3}>
                  <Field
                    id={"defaultQuantityToReceive"}
                    name={"defaultQuantityToReceive"}
                    label={getTranslatedLabel(
                      "facility.receive.form.accepted",
                      "Quantity Accepted"
                    )}
                    component={FormNumericTextBox}
                    min={1}
                    max={orderItem && orderItem.quantity}
                    autoComplete={"off"}
                    onChange={handleAmountsChange}
                  />
                </Grid>
                <Grid item xs={3}>
                  <Field
                    id={"quantityRejected"}
                    name={"quantityRejected"}
                    label={getTranslatedLabel(
                      "facility.receive.form.rejected",
                      "Quantity Rejected"
                    )}
                    component={FormNumericTextBox}
                    min={0}
                    max={orderItem && orderItem.quantity}
                    autoComplete={"off"}
                    onChange={handleAmountsChange}
                    // validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={3}>
                  <Field
                    id={"unitPrice"}
                    name={"unitPrice"}
                    label={getTranslatedLabel(
                      "facility.receive.form.unitPrice",
                      "Per Unit Price"
                    )}
                    component={FormNumericTextBox}
                    min={0}
                    autoComplete={"off"}
                    // validator={requiredValidator}
                  />
                </Grid>

                <Grid item xs={3}>
                  <Field
                    id={"lotId"}
                    name={"lotId"}
                    label={getTranslatedLabel(
                      "facility.receive.form.lot",
                      "Lot"
                    )}
                    component={FormInput}
                    autoComplete={"off"}
                    // validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={6}>
                  <Field
                    id={"facilityLocation"}
                    name={"facilityLocation"}
                    label={getTranslatedLabel(
                      "facility.receive.form.location",
                      "Facility Location"
                    )}
                    component={MemoizedFormDropDownList2}
                    data={facilityLocations ?? []}
                    autoComplete={"off"}
                    dataItemKey="locationSeqId"
                    textField="description"
                    // validator={requiredValidator}
                  />
                </Grid>
                <Grid item xs={3} mt={2}>
                  <Field
                    id={"includeThisItem"}
                    name={"includeThisItem"}
                    label={"Include"}
                    hint={"Hint: Include this item in order receive"}
                    component={MemoizedFormCheckBox}
                  />
                </Grid>
                <Grid item xs={3}>
                  <Field
                    id={"rejectionReasonId"}
                    name={"rejectionReasonId"}
                    label={getTranslatedLabel(
                      "facility.receive.form.reason",
                      "Rejection Reason"
                    )}
                    component={MemoizedFormDropDownList2}
                    data={rejectionReasonsData ?? []}
                    dataItemKey="rejectionId"
                    textField="description"
                    // validator={requiredValidator}
                  />
                </Grid>
              </Grid>
              <div className="k-form-buttons">
                <Grid container spacing={1}>
                  <Grid item>
                    <Button
                      type={"submit"}
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

export default ReceiveInventoryItemForm;
