import { Field, Form, FormElement, FormRenderProps } from "@progress/kendo-react-form";
import React, { useCallback, useEffect, useRef, useState } from "react";
import { Button, Checkbox, FormControlLabel, Grid, Radio, RadioGroup } from "@mui/material";
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

// REFACTOR: Added explicit type for FormCheckBox props
// Purpose: Improve type safety and maintainability
// Context: Defines expected props for FormCheckBox component
interface FormCheckBoxProps {
  value: boolean;
  onChange: (event: { value: boolean }) => void;
  disabled: boolean;
  label: string;
  [key: string]: any;
}

const FormCheckBox = ({ value, onChange, disabled, label, ...others }: FormCheckBoxProps) => {
  const { getTranslatedLabel } = useTranslationHelper();
  return (
      <FormControlLabel
          control={
            <Checkbox
                checked={value || false}
                onChange={(e) => onChange({ value: e.target.checked })}
                disabled={disabled}
                {...others}
            />
          }
          label={getTranslatedLabel("certificate.items.form.isContractorPurchased", label)}
      />
  );
};

interface Props {
  certificateItem?: CertificateItem;
  editMode: number; // 1: add, 2: edit
  onClose: () => void;
  formEditMode: number; // 0: view, 1: create, 2: CREATED, 3: APPROVED, 4: COMPLETED
  updateCertificateItems: (certificateItem: CertificateItem, editMode: number) => void;
}

// REFACTOR: Added assumed CertificateItem interface
// Purpose: Clarify expected properties for type safety
// Context: Based on form fields and submission data
interface CertificateItem {
  productId: string;
  uomId: string;
  quantity: number;
  unitPrice: number;
  total: number;
  net: number;
  deserved?: number;
  insurance?: number;
  deductions?: number;
  isContractorPurchased?: boolean;
  procurementDate?: Date;
  facilityId?: string;
}

export default function CertificateItemForm({
                                              certificateItem,
                                              editMode,
                                              onClose,
                                              formEditMode,
                                              updateCertificateItems,
                                            }: Props) {
  const MyForm = useRef<Form>(null);
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
  const [insuranceMode, setInsuranceMode] = useState<"value" | "percentage">("value");
  const { data: facilities } = useFetchFacilitiesQuery(undefined);
  const { getTranslatedLabel } = useTranslationHelper();

  // REFACTOR: Memoized calculateTotals to optimize performance
  // Purpose: Compute totals, with deserved as total - deductions and insurance as value/percentage
  // Context: Ensures correct calculations for both certificate types
  const calculateTotals = useCallback(
      (valueGetter: FormRenderProps["valueGetter"]) => {
        const quantity = valueGetter("quantity") || 0;
        const price = valueGetter("unitPrice") || 0;
        const total = Math.round(quantity * price * 100) / 100;
        let finalTotal = total;
        let deserved = 0;
        let insurance = 0;

        if (currentCertificateType === "PROCUREMENT_CERTIFICATE") {
          const discount = valueGetter("discount") || 0;
          finalTotal = discountMode === "value" ? total - discount : total * (1 - discount / 100);
        } else if (currentCertificateType === "CONTRACTING_CERTIFICATE") {
          deserved = Math.max(0, Math.round((total - (valueGetter("deductions") || 0)) * 100) / 100);
          const insuranceValue = valueGetter("insurance") || 0;
          insurance = insuranceMode === "value" ? insuranceValue : deserved * (insuranceValue / 100);
          insurance = Math.round(insurance * 100) / 100;
        }

        const net = currentCertificateType === "CONTRACTING_CERTIFICATE"
            ? Math.max(0, Math.round((deserved - insurance) * 100) / 100)
            : finalTotal;

        return { total, finalTotal, net, deserved, insurance };
      },
      [currentCertificateType, discountMode, insuranceMode]
  );

  // REFACTOR: Updated handleInsuranceModeChange
  // Purpose: Toggle insurance between value and percentage modes
  // Context: Resets insurance value on mode change for CONTRACTING_CERTIFICATE
  const handleInsuranceModeChange = useCallback(
      (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => {
        setInsuranceMode(event.target.value as "value" | "percentage");
        onChange("insurance", { value: 0 });
      },
      []
  );

  // REFACTOR: Updated handleDiscountModeChange
  // Purpose: Toggle discount between value and percentage modes
  // Context: Resets discount value on mode change for PROCUREMENT_CERTIFICATE
  const handleDiscountModeChange = useCallback(
      (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => {
        setDiscountMode(event.target.value as "value" | "percentage");
        onChange("discount", { value: 0 });
      },
      []
  );

  const ProcurementForm = ({ formRenderProps }: { formRenderProps: FormRenderProps }) => {
    const { valueGetter, onChange } = formRenderProps;
    const { finalTotal } = calculateTotals(valueGetter);

    // REFACTOR: Optimized useEffect for total field
    // Purpose: Ensure total reflects discounted value
    // Context: Single onChange call for efficiency
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
                      <Button
                          onClick={onClose}
                          variant="contained"
                          color="error"
                          fullWidth
                      >
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
    const { total, net, deserved, insurance } = calculateTotals(valueGetter);
    const productType = valueGetter("productId")?.ProductType || "";

    // REFACTOR: Optimized useEffect to batch onChange calls
    // Purpose: Update total, deserved, insurance, and net efficiently
    // Context: Ensures deserved displays total - deductions, net displays deserved - insurance
    useEffect(() => {
      onChange({
        total: { value: total },
        deserved: { value: deserved },
        insurance: { value: insurance },
        net: { value: net },
      });
    }, [total, deserved, insurance, net, onChange]);

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
                    id="isContractorPurchased"
                    name="isContractorPurchased"
                    label="Contractor Purchased"
                    component={FormCheckBox}
                    disabled={formEditMode > 3 || productType !== "RAW_MATERIAL"}
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
                    value={deserved}
                    disabled
                />
              </Grid>
              <Grid item xs={6}>
                <Field
                    id="insurance"
                    name="insurance"
                    label={getTranslatedLabel("certificate.items.form.insurance", `Insurance (${insuranceMode})`)}
                    component={FormNumericTextBox}
                    format={insuranceMode === "percentage" ? "n0" : "n2"}
                    min={0}
                    max={insuranceMode === "percentage" ? 100 : undefined}
                    validator={insuranceMode === "percentage" ? percentageValidator : undefined}
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
                <RadioGroup
                    row
                    value={insuranceMode}
                    onChange={(e) => handleInsuranceModeChange(e, formRenderProps.onChange)}
                >
                  <FormControlLabel
                      value="value"
                      control={<Radio disabled={formEditMode > 3} />}
                      label={getTranslatedLabel("certificate.items.form.insuranceValue", "Value")}
                  />
                  <FormControlLabel
                      value="percentage"
                      control={<Radio disabled={formEditMode > 3} />}
                      label={getTranslatedLabel("certificate.items.form.insurancePercentage", "Percentage")}
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
                      <Button
                          onClick={onClose}
                          variant="contained"
                          color="error"
                          fullWidth
                      >
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

  // REFACTOR: Updated onSubmit to include calculated fields
  // Purpose: Ensure total, net, deserved, and insurance are included in submission
  // Context: Handles both certificate types, with deserved as total - deductions
  return (
      <Form
          ref={MyForm}
          initialValues={initValue}
          key={formKey}
          onSubmit={(values) => {
            const { total, finalTotal, net, deserved, insurance } = calculateTotals((name: string) => values[name]);
            handleSubmitData({
              ...values,
              total: currentCertificateType === "PROCUREMENT_CERTIFICATE" ? finalTotal : total,
              net,
              deserved,
              insurance,
              isContractorPurchased: values.isContractorPurchased || false,
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