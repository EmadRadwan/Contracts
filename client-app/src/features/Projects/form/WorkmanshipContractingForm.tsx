import { Field, FormElement, FormRenderProps } from "@progress/kendo-react-form";
import React, { useEffect } from "react";
import { Button, Grid, Radio, RadioGroup, FormControlLabel } from "@mui/material";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { FormSimpleComboBoxVirtualProduct } from "../../../app/common/form/FormSimpleComboBoxVirtualProduct";
import { FormComboBoxVirtualUOM } from "../../../app/common/form/FormComboBoxVirtualUOM";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import { percentageValidator, requiredValidator } from "../../../app/common/form/Validators";
import { Facility } from "../../../app/models/facility";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import FormButtons from "./FormButtons";

interface ContractingFormProps {
    formRenderProps: FormRenderProps;
    editMode: number;
    formEditMode: number;
    insuranceMode: "value" | "percentage";
    handleInsuranceModeChange: (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => void;
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
    achievementPercentageValidator: (value: number) => string | undefined;
}

const WorkmanshipContractingForm = ({
                                        formRenderProps,
                                        editMode,
                                        formEditMode,
                                        insuranceMode,
                                        handleInsuranceModeChange,
                                        calculateTotals,
                                        facilities,
                                        getTranslatedLabel,
                                        onClose,
                                        achievementPercentageValidator,
                                    }: ContractingFormProps) => {
    const { valueGetter, onChange } = formRenderProps;
    const { total, net, deserved } = calculateTotals(valueGetter);

    // REFACTOR: Optimized useEffect dependency array
    // Purpose: Prevent unnecessary re-renders by only depending on changed values
    // Context: Ensures total, deserved, and net fields update only when necessary
    useEffect(() => {
        onChange("total", { value: total });
        onChange("deserved", { value: deserved });
        onChange("net", { value: net });
    }, [total, deserved, net, onChange]);

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
                            value={deserved}
                            disabled
                        />
                    </Grid>
                    <Grid item xs={6}>
                        {/* REFACTOR: Wrapped insurance field and RadioGroup in a nested Grid container */}
                        {/* Purpose: Visually group insurance and its mode selector for better UX */}
                        {/* Context: Places RadioGroup directly below insurance field in the same column */}
                        <Grid container direction="column" spacing={1}>
                            <Grid item>
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
                            <Grid item>
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
                        </Grid>
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
                    <Grid item xs={6}>
                        <Field
                            id="achievementPercentage"
                            name="achievementPercentage"
                            label={getTranslatedLabel("certificate.items.form.achievementPercentage", "Achievement Percentage *")}
                            component={FormNumericTextBox}
                            format="n0"
                            min={1}
                            max={100}
                            validator={achievementPercentageValidator}
                            disabled={formEditMode > 3}
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

export default WorkmanshipContractingForm;