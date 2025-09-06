import { Field, FormElement, FormRenderProps } from "@progress/kendo-react-form";
import React, { useEffect } from "react";
import { Button, Grid, Radio, RadioGroup, FormControlLabel } from "@mui/material";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { FormSimpleComboBoxVirtualProduct } from "../../../app/common/form/FormSimpleComboBoxVirtualProduct";
import { FormComboBoxVirtualUOM } from "../../../app/common/form/FormComboBoxVirtualUOM";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import { requiredValidator } from "../../../app/common/form/Validators";
import { Facility } from "../../../app/models/facility";
import FormButtons from "./FormButtons"; // REFACTOR: Extracted button section to shared component

interface ProcurementFormProps {
    formRenderProps: FormRenderProps;
    editMode: number;
    formEditMode: number;
    discountMode: "value" | "percentage";
    handleDiscountModeChange: (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => void;
    calculateTotals: (valueGetter: FormRenderProps["valueGetter"]) => {
        total: number;
        finalTotal: number;
        net: number;
        deserved: number;
        insurance: number;
        discount: number;
    };
    facilities: Facility[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    onClose: () => void;
    percentageValidator: (value: number) => string | undefined; // REFACTOR: Added to avoid unused import
}

const SupplyProcurementForm = ({
                             formRenderProps,
                             editMode,
                             formEditMode,
                             discountMode,
                             handleDiscountModeChange,
                             calculateTotals,
                             facilities,
                             getTranslatedLabel,
                             onClose,
                             percentageValidator,
                         }: ProcurementFormProps) => {
    const { valueGetter, onChange } = formRenderProps;
    const { finalTotal } = calculateTotals(valueGetter);

    // REFACTOR: Optimized useEffect dependency array
    // Purpose: Prevent unnecessary re-renders by only depending on finalTotal and onChange
    // Context: Ensures total field updates only when finalTotal changes
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
                            data={facilities}
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
                        <FormButtons
                            editMode={editMode}
                            formEditMode={formEditMode}
                            allowSubmit={formRenderProps.allowSubmit}
                            getTranslatedLabel={getTranslatedLabel}
                            onClose={onClose}
                        />
                    </Grid>
                </Grid>
            </fieldset>
        </FormElement>
    );
};

export default SupplyProcurementForm;