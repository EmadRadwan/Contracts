import { Field, FormElement, FormRenderProps } from "@progress/kendo-react-form";
import React, { useEffect } from "react";
import { Button, Grid, Radio, RadioGroup, FormControlLabel } from "@mui/material";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { FormSimpleComboBoxVirtualProduct } from "../../../app/common/form/FormSimpleComboBoxVirtualProduct";
import { FormComboBoxVirtualUOM } from "../../../app/common/form/FormComboBoxVirtualUOM";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import { requiredValidator } from "../../../app/common/form/Validators";
import FormButtons from "./FormButtons";
import FormInput from "../../../app/common/form/FormInput";

// Purpose: Ensure type safety for new fields used in calculations
// Context: Reflects the updated calculateTotals function to include new fields for procurement-like certificates
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
        transportationExpenses: number;
        gratuities: number;
    };
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    onClose: () => void;
    percentageValidator: (value: number) => string | undefined;
}

const SupplyProcurementForm = ({
                                   formRenderProps,
                                   editMode,
                                   formEditMode,
                                   discountMode,
                                   handleDiscountModeChange,
                                   calculateTotals,
                                   getTranslatedLabel,
                                   onClose,
                                   percentageValidator,
                               }: ProcurementFormProps) => {
    const { valueGetter, onChange } = formRenderProps;
    const { finalTotal } = calculateTotals(valueGetter);

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
                            label={getTranslatedLabel("projects.certificate.items.list.product", "Product *")}
                            component={FormSimpleComboBoxVirtualProduct}
                            autoComplete="off"
                            validator={requiredValidator}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        <Field
                            id="uomId"
                            name="uomId"
                            label={getTranslatedLabel("projects.certificate.items.list.unitOfMeasure", "Unit of Measure *")}
                            component={FormComboBoxVirtualUOM}
                            validator={requiredValidator}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        <Field
                            id="description"
                            name="description"
                            label={getTranslatedLabel("projects.certificate.items.list.description", "Description *")}
                            component={FormInput}
                            validator={requiredValidator}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        <Field
                            id="quantity"
                            name="quantity"
                            label={getTranslatedLabel("projects.certificate.items.list.quantity", "Quantity *")}
                            component={FormNumericTextBox}
                            format="n3"
                            min={1}
                            validator={requiredValidator}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        <Field
                            id="unitPrice"
                            name="unitPrice"
                            label={getTranslatedLabel("projects.certificate.items.list.unitPrice", "Unit Price *")}
                            component={FormNumericTextBox}
                            format="n3"
                            min={0}
                            validator={requiredValidator}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        <Field
                            id="procurementDate"
                            name="procurementDate"
                            label={getTranslatedLabel("projects.certificate.items.list.procurementDate", "Procurement Date *")}
                            component={FormDatePicker}
                            validator={requiredValidator}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    
                    <Grid item xs={6}>
                        {/* Purpose: Visually group discount and its mode selector for better UX */}
                        {/* Context: Ensures discount and RadioGroup are aligned together in the same column */}
                        <Grid container direction="column" spacing={1}>
                            <Grid item>
                                <Field
                                    id="discount"
                                    name="discount"
                                    label={getTranslatedLabel("projects.certificate.items.list.discount", `Discount (${discountMode})`)}
                                    component={FormNumericTextBox}
                                    format={discountMode === "percentage" ? "n0" : "n3"}
                                    min={0}
                                    max={discountMode === "percentage" ? 100 : undefined}
                                    validator={discountMode === "percentage" ? percentageValidator : undefined}
                                    disabled={formEditMode > 3}
                                />
                            </Grid>
                            <Grid item>
                                <RadioGroup
                                    row
                                    value={discountMode}
                                    onChange={(e) => handleDiscountModeChange(e, formRenderProps.onChange)}
                                >
                                    <FormControlLabel
                                        value="value"
                                        control={<Radio disabled={formEditMode > 3} />}
                                        label={getTranslatedLabel("projects.certificate.items.list.discountValue", "Value")}
                                    />
                                    <FormControlLabel
                                        value="percentage"
                                        control={<Radio disabled={formEditMode > 3} />}
                                        label={getTranslatedLabel("projects.certificate.items.list.discountPercentage", "Percentage")}
                                    />
                                </RadioGroup>
                            </Grid>
                        </Grid>
                    </Grid>
                    <Grid item xs={6}>
                        <Field
                            id="transportationExpenses"
                            name="transportationExpenses"
                            label={getTranslatedLabel("projects.certificate.items.list.transportationExpenses", "Transportation Expenses")}
                            component={FormNumericTextBox}
                            format="n3"
                            min={0}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        <Field
                            id="gratuities"
                            name="gratuities"
                            label={getTranslatedLabel("projects.certificate.items.list.gratuities", "Gratuities")}
                            component={FormNumericTextBox}
                            format="n3"
                            min={0}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        <Field
                            id="total"
                            name="total"
                            label={getTranslatedLabel("projects.certificate.items.list.totalAmount", "Total")}
                            component={FormNumericTextBox}
                            format="n3"
                            value={finalTotal}
                            disabled
                        />
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