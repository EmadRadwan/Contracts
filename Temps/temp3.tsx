import { Field, Form, FormElement, FormRenderProps } from "@progress/kendo-react-form";
import React, { useCallback, useRef, useState, useEffect } from "react";
import { Button, FormControlLabel, Grid, Radio, RadioGroup } from "@mui/material";
import useCertificateItem from "../hook/useCertificateItem";
import { percentageValidator, requiredValidator } from "../../../app/common/form/Validators";
import { useAppSelector, useFetchFacilitiesQuery } from "../../../app/store/configureStore";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { FormSimpleComboBoxVirtualProduct } from "../../../app/common/form/FormSimpleComboBoxVirtualProduct";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import { FormComboBoxVirtualUOM } from "../../../app/common/form/FormComboBoxVirtualUOM";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

interface Props {
  certificateItem?: CertificateItem;
  editMode: number; // 1: add, 2: edit
  onClose: () => void;
  formEditMode: number; // 0: view, 1: create, 2: CREATED, 3: APPROVED, 4: COMPLETED
  updateCertificateItems: (certificateItem: CertificateItem, editMode: number) => void;
}

export default function CertificateItemForm({
                                              certificateItem,
                                              editMode,
                                              onClose,
                                              formEditMode,
                                              updateCertificateItems,
                                            }: Props) {
  const MyForm = useRef<any>(null);
  const [formKey, setFormKey] = useState(1);
  const [initValue, setInitValue] = useState<CertificateItem | undefined>(certificateItem);
  const { handleSubmitData } = useCertificateItem({
    certificateItem,
    editMode,
    setFormKey,
    setInitValue,
    updateCertificateItems,
  });
  const { currentCertificateType } = useAppSelector((state) => state.certificateUi);
  const [discountMode, setDiscountMode] = useState<"value" | "percentage">("value");
  const { data: facilities, isFetching, isLoading } = useFetchFacilitiesQuery(undefined);
  const { getTranslatedLabel } = useTranslationHelper();

  const calculateTotals = (valueGetter: FormRenderProps["valueGetter"]) => {
    const quantity = valueGetter("quantity") || 0;
    const price = valueGetter("unitPrice") || 0;
    const total = Math.round(quantity * price * 100) / 100;
    let finalTotal = total;

    if (currentCertificateType === "PROCUREMENT_CERTIFICATE") {
      const discount = valueGetter("discount") || 0;
      finalTotal = discountMode === "value" ? total - discount : total * (1 - discount / 100);
    }

    const net =
        currentCertificateType === "CONTRACTING_CERTIFICATE"
            ? Math.round(
            (total - (valueGetter("deductions") || 0) - (valueGetter("insurance") || 0) + (valueGetter("deserved") || 0)) * 100
        ) / 100
            : finalTotal;

    return { total, finalTotal, net: net < 0 ? 0 : net };
  };

  const handleDiscountModeChange = useCallback(
      (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => {
        setDiscountMode(event.target.value as "value" | "percentage");
        onChange("discount", { value: 0 }); // Reset discount when mode changes
      },
      []
  );

  const ProcurementForm = ({ formRenderProps }: { formRenderProps: FormRenderProps }) => {
    const { valueGetter, onChange } = formRenderProps;
    const { finalTotal } = calculateTotals(valueGetter);

    useEffect(() => {
      onChange("total", { value: finalTotal });
    }, [finalTotal, onChange]);

    return (
        <FormElement>
          <fieldset className="k-form-fieldset">
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Field
                    id="productId"
                    name="productId"
                    label={getTranslatedLabel("certificate.items.form.product", "Product *")}
                    component={FormSimpleComboBoxVirtualProduct}
                    autoComplete="off"
                    validator={requiredValidator}
                    disabled={editMode === 2 || formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="uomId"
                    name="uomId"
                    label={getTranslatedLabel("certificate.items.form.uom", "Unit of Measure *")}
                    component={FormComboBoxVirtualUOM}
                    validator={requiredValidator}
                    disabled={editMode === 2 || formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="quantity"
                    name="quantity"
                    label={getTranslatedLabel("certificate.items.form.quantity", "Quantity *")}
                    component={FormNumericTextBox}
                    format="n0"
                    min={1}
                    validator={requiredValidator}
                    disabled={formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="unitPrice"
                    name="unitPrice"
                    label={getTranslatedLabel("certificate.items.form.price", "Price *")}
                    component={FormNumericTextBox}
                    format="n2"
                    min={0}
                    validator={requiredValidator}
                    disabled={formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="procurementDate"
                    name="procurementDate"
                    label={getTranslatedLabel("certificate.items.form.procurementDate", "Procurement Date *")}
                    component={FormDatePicker}
                    validator={requiredValidator}
                    disabled={formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="facilityId"
                    name="facilityId"
                    label={getTranslatedLabel("facility.items.form.facility", "Facility *")}
                    component={MemoizedFormDropDownList2}
                    data={facilities ?? []}
                    dataItemKey="facilityId"
                    textField="facilityName"
                    validator={requiredValidator}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="discount"
                    name="discount"
                    label={getTranslatedLabel("certificate.items.form.discount", `Discount (${discountMode})`)}
                    component={FormNumericTextBox}
                    format={discountMode === "percentage" ? "n0" : "n2"}
                    min={0}
                    max={discountMode === "percentage" ? 100 : undefined}
                    validator={discountMode === "percentage" ? percentageValidator : undefined}
                    disabled={formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="total"
                    name="total"
                    label={getTranslatedLabel("certificate.items.form.total", "Total")}
                    component={FormNumericTextBox}
                    format="n2"
                    value={finalTotal}
                    disabled
                />
              </Grid>
              <Grid item xs={12}>
                <RadioGroup
                    row
                    value={discountMode}
                    onChange={(e) => handleDiscountModeChange(e, formRenderProps.onChange)}
                >
                  <FormControlLabel
                      value="value"
                      control={<Radio disabled={formEditMode > 3} />}
                      label={getTranslatedLabel("certificate.items.form.discountValue", "Value")}
                  />
                  <FormControlLabel
                      value="percentage"
                      control={<Radio disabled={formEditMode > 3} />}
                      label={getTranslatedLabel("certificate.items.form.discountPercentage", "Percentage")}
                  />
                </RadioGroup>
              </Grid>
              <Grid item xs={12}>
                <div className="k-form-buttons">
                  <Grid container spacing={2}>
                    <Grid item xs={6}>
                      <Button
                          variant="contained"
                          type="submit"
                          color="success"
                          disabled={!formRenderProps.allowSubmit || formEditMode > 3}
                          fullWidth
                      >
                        {editMode === 2
                            ? getTranslatedLabel("certificate.items.form.update", "Update")
                            : getTranslatedLabel("certificate.items.form.add", "Add")}
                      </Button>
                    </Grid>
                    <Grid item xs={6}>
                      <Button onClick={onClose} variant="contained" color="error" fullWidth>
                        {getTranslatedLabel("certificate.items.form.cancel", "Cancel")}
                      </Button>
                    </Grid>
                  </Grid>
                </div>
              </Grid>
            </Grid>
          </fieldset>
        </FormElement>
    );
  };

  const ContractingForm = ({ formRenderProps }: { formRenderProps: FormRenderProps }) => {
    const { valueGetter, onChange } = formRenderProps;
    const { total, net } = calculateTotals(valueGetter);

    useEffect(() => {
      onChange("total", { value: total });
      onChange("net", { value: net });
    }, [total, net, onChange]);

    return (
        <FormElement>
          <fieldset className="k-form-fieldset">
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Field
                    id="productId"
                    name="productId"
                    label={getTranslatedLabel("certificate.items.form.product", "Product *")}
                    component={FormSimpleComboBoxVirtualProduct}
                    autoComplete="off"
                    validator={requiredValidator}
                    disabled={editMode === 2 || formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="uomId"
                    name="uomId"
                    label={getTranslatedLabel("certificate.items.form.uom", "Unit of Measure *")}
                    component={FormComboBoxVirtualUOM}
                    validator={requiredValidator}
                    disabled={editMode === 2 || formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="quantity"
                    name="quantity"
                    label={getTranslatedLabel("certificate.items.form.quantity", "Quantity *")}
                    component={FormNumericTextBox}
                    format="n0"
                    min={1}
                    validator={requiredValidator}
                    disabled={formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="unitPrice"
                    name="unitPrice"
                    label={getTranslatedLabel("certificate.items.form.price", "Price *")}
                    component={FormNumericTextBox}
                    format="n2"
                    min={0}
                    validator={requiredValidator}
                    disabled={formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="total"
                    name="total"
                    label={getTranslatedLabel("certificate.items.form.total", "Total")}
                    component={FormNumericTextBox}
                    format="n2"
                    value={total}
                    disabled
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="deductions"
                    name="deductions"
                    label={getTranslatedLabel("certificate.items.form.deductions", "Deductions")}
                    component={FormNumericTextBox}
                    format="n2"
                    min={0}
                    disabled={formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="deserved"
                    name="deserved"
                    label={getTranslatedLabel("certificate.items.form.deserved", "Deserved")}
                    component={FormNumericTextBox}
                    format="n2"
                    min={0}
                    disabled={formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="insurance"
                    name="insurance"
                    label={getTranslatedLabel("certificate.items.form.insurance", "Insurance")}
                    component={FormNumericTextBox}
                    format="n2"
                    min={0}
                    disabled={formEditMode > 3}
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="net"
                    name="net"
                    label={getTranslatedLabel("certificate.items.form.net", "Net")}
                    component={FormNumericTextBox}
                    format="n2"
                    value={net}
                    disabled
                />
              </Grid>
              <Grid item xs={12}>
                <div className="k-form-buttons">
                  <Grid container spacing={2}>
                    <Grid item xs={6}>
                      <Button
                          variant="contained"
                          type="submit"
                          color="success"
                          disabled={!formRenderProps.allowSubmit || formEditMode > 3}
                          fullWidth
                      >
                        {editMode === 2
                            ? getTranslatedLabel("certificate.items.form.update", "Update")
                            : getTranslatedLabel("certificate.items.form.add", "Add")}
                      </Button>
                    </Grid>
                    <Grid item xs={6}>
                      <Button onClick={onClose} variant="contained" color="error" fullWidth>
                        {getTranslatedLabel("certificate.items.form.cancel", "Cancel")}
                      </Button>
                    </Grid>
                  </Grid>
                </div>
              </Grid>
            </Grid>
          </fieldset>
        </FormElement>
    );
  };

  return (
      <Form
          ref={MyForm}
          initialValues={initValue}
          key={formKey}
          onSubmit={(values) => {
            const { total, finalTotal, net } = calculateTotals((name: string) => values[name]);
            handleSubmitData({
              ...values,
              total: currentCertificateType === "PROCUREMENT_CERTIFICATE" ? finalTotal : total,
              net,
            } as CertificateItem);
            onClose();
          }}
          render={(formRenderProps) =>
              currentCertificateType === "PROCUREMENT_CERTIFICATE" ? (
                  <ProcurementForm formRenderProps={formRenderProps} />
              ) : (
                  <ContractingForm formRenderProps={formRenderProps} />
              )
          }
      />
  );
}

export const CertificateItemFormMemo = React.memo(CertificateItemForm);