import React from "react";
import { ProductInventoryItem } from "../../../app/models/facility/productInventoryItem";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { Button, Grid, Typography } from "@mui/material";
import { MemoizedFormDropDownList } from "../../../app/common/form/MemoizedFormDropDownList";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import {
  useFetchVarianceReasonsQuery,
} from "../../../app/store/configureStore";
import { requiredValidator } from "../../../app/common/form/Validators";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { useCreatePhysicalInventoryAndVarianceMutation } from "../../../app/store/apis";
import LoadingComponent from "../../../app/layout/LoadingComponent";

interface TransferPhysicalInventoryFormProps {
  inventoryItem: ProductInventoryItem;
  onClose: () => void;
}

const TransferPhysicalInventoryForm = ({
                                         inventoryItem,
                                         onClose,
                                       }: TransferPhysicalInventoryFormProps) => {
  const { getTranslatedLabel } = useTranslationHelper();
  const { data: varianceReasons } = useFetchVarianceReasonsQuery(undefined);
  const [createPhysicalInventoryAndVariance, { isLoading, error }] =
      useCreatePhysicalInventoryAndVarianceMutation();

  const initialValues = {
    inventoryItemId: inventoryItem?.inventoryItemId ?? '',
    productName: inventoryItem?.productName ?? '',
    itemQOH: inventoryItem?.itemQOH ?? '',
    itemATP: inventoryItem?.itemATP ?? '',
    productATP: inventoryItem?.productATP ?? '',
    productQOH: inventoryItem?.productQOH ?? '',
    varianceReasonId: inventoryItem?.varianceReasonId ?? '',
    ATPVariance: inventoryItem?.ATPVariance ?? 0,
    QOHVariance: inventoryItem?.QOHVariance ?? 0,
  };

  const handleSubmit = async (values: any) => {
   

    // REFACTOR: Simplified DTO mapping and included optional fields if needed.
    const dto = {
      inventoryItemId: values.inventoryItemId,
      varianceReasonId: values.varianceReasonId,
      quantityOnHandVar: values.QOHVariance,
      availableToPromiseVar: values.QOHVariance,
      comments: values.comments || undefined,
    };

    try {
      const result = await createPhysicalInventoryAndVariance(dto).unwrap();
      // REFACTOR: Simplified success handling; use your notification system.
      console.log(`Success: Physical Inventory ID ${result.value}`);
      onClose();
    } catch (err: any) {
      // REFACTOR: Improved error handling to extract message from RTK Query error.
      const errorMessage =
          (err.data?.error as string) || 'Failed to create Physical Inventory and Variance';
      console.error('Submission error:', errorMessage);
    }
    onClose();
  };

  return (
      <Form
          initialValues={initialValues}
          onSubmit={handleSubmit}
          render={() => (
              <FormElement>
                <fieldset className={"k-form-fieldset"}>
                  <Grid container spacing={2}>
                    <Grid container item xs={12} spacing={2}>
                      {/* REFACTOR: Replaced Field with Typography for inventoryItemId to render as static text.
                    Improves UX by displaying non-editable fields as plain text, reducing form clutter. */}
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          <strong>{getTranslatedLabel("facility.physical.form.invItem", "Inventory Item")}:</strong> {initialValues.inventoryItemId}
                        </Typography>
                      </Grid>
                      {/* REFACTOR: Replaced Field with Typography for productName to render as static text.
                    Maintains consistency with other non-editable fields. */}
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          <strong>{getTranslatedLabel("facility.physical.form.product", "Product Name")}:</strong> {initialValues.productName}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid container item xs={12} spacing={2}>
                      {/* REFACTOR: Replaced Field with Typography for itemQOH to render as static text.
                    Ensures non-editable fields are visually distinct from input fields. */}
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          <strong>{getTranslatedLabel("facility.physical.form.itemQOH", "Item QOH")}:</strong> {initialValues.itemQOH}
                        </Typography>
                      </Grid>
                      {/* REFACTOR: Replaced Field with Typography for itemATP to render as static text.
                    Keeps form focused on editable fields only. */}
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          <strong>{getTranslatedLabel("facility.physical.form.itemATP", "Item ATP")}:</strong> {initialValues.itemATP}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid container item xs={12} spacing={2}>
                      {/* REFACTOR: Replaced Field with Typography for productATP to render as static text.
                    Aligns with requirement to display specified fields as text. */}
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          <strong>{getTranslatedLabel("facility.physical.form.productATP", "Product ATP")}:</strong> {initialValues.productATP}
                        </Typography>
                      </Grid>
                      {/* REFACTOR: Replaced Field with Typography for productQOH to render as static text.
                    Completes the transition of specified fields to non-editable text display. */}
                      <Grid item xs={6}>
                        <Typography variant="body1">
                          <strong>{getTranslatedLabel("facility.physical.form.productQOH", "Product QOH")}:</strong> {initialValues.productQOH}
                        </Typography>
                      </Grid>
                    </Grid>
                    <Grid container item xs={12} spacing={2}>
                      {/*<Grid item xs={6}>
                        <Field
                            id={"ATPVariance"}
                            name={"ATPVariance"}
                            label={getTranslatedLabel("facility.physical.form.atpVar", "ATP Variance")}
                            component={FormNumericTextBox}
                            autoComplete={"off"}
                        />
                      </Grid>*/}
                      <Grid item xs={6}>
                        <Field
                            id={"QOHVariance"}
                            name={"QOHVariance"}
                            label={getTranslatedLabel("facility.physical.form.qohVar", "QOH Variance")}
                            component={FormNumericTextBox}
                            autoComplete={"off"}
                        />
                      </Grid>
                      <Grid item xs={6}>
                        <Field
                            id={"varianceReasonId"}
                            name={"varianceReasonId"}
                            label={getTranslatedLabel("facility.physical.form.varianceReason", "Variance Reason")}
                            component={MemoizedFormDropDownList}
                            data={varianceReasons ?? []}
                            dataItemKey={"varianceReasonId"}
                            textField={"description"}
                            autoComplete={"off"}
                            validator={requiredValidator}
                        />
                      </Grid>
                    </Grid>
                  </Grid>
                  <Grid container item xs={12} spacing={2} mt={1}>
                    <Grid item>
                      <Button variant="contained" type="submit" color="success">
                        {getTranslatedLabel("general.save", "Submit")}
                      </Button>
                    </Grid>
                    <Grid item>
                      <Button
                          variant="contained"
                          type="button"
                          color="error"
                          onClick={onClose}
                      >
                        {getTranslatedLabel("general.cancel", "Cancel")}
                      </Button>
                    </Grid>
                  </Grid>
                  {isLoading && (
                      <LoadingComponent message="Adjusting Inventory Item..." />
                  )}
                </fieldset>
              </FormElement>
          )}
      />
  );
};

export default TransferPhysicalInventoryForm;