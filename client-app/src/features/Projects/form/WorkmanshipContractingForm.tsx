import { Field, FormElement, FormRenderProps } from "@progress/kendo-react-form";
import React, { useEffect } from "react";
import { Button, Grid, Radio, RadioGroup, FormControlLabel } from "@mui/material";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { FormSimpleComboBoxVirtualProduct } from "../../../app/common/form/FormSimpleComboBoxVirtualProduct";
import { FormComboBoxVirtualUOM } from "../../../app/common/form/FormComboBoxVirtualUOM";
import { percentageValidator, requiredValidator } from "../../../app/common/form/Validators";
import FormButtons from "./FormButtons";
import FormTextArea from "../../../app/common/form/FormTextArea";
import { toast } from "react-toastify";
import FormInput from "../../../app/common/form/FormInput";


interface ContractingFormProps {
    formRenderProps: FormRenderProps;
    editMode: number;
    formEditMode: number;
    insuranceMode: "value" | "percentage";
    additionalInsuranceMode: "value" | "percentage";
    handleInsuranceModeChange: (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => void;
    handleAdditionalInsuranceModeChange: (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => void;
    calculateTotals: (valueGetter: FormRenderProps["valueGetter"]) => {
        total: number;
        finalTotal: number;
        net: number;
        deserved: number;
        insurance: number;
        discount: number;
        additionalInsurance: number;
    };
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    onClose: () => void;
    achievementPercentageValidator: (value: number) => string | undefined;
}

const WorkmanshipContractingForm = ({
                                        formRenderProps,
                                        editMode,
                                        formEditMode,
                                        insuranceMode,
                                        additionalInsuranceMode,
                                        handleInsuranceModeChange,
                                        handleAdditionalInsuranceModeChange,
                                        calculateTotals,
                                        getTranslatedLabel,
                                        onClose,
                                        additionalInsurance,
                                        achievementPercentageValidator,
                                    }: ContractingFormProps) => {
    const { valueGetter, onChange } = formRenderProps;
    const { total, net, deserved } = calculateTotals(valueGetter);

    // Purpose: Prevent unnecessary re-renders by only depending on changed values
    // Context: Ensures total, deserved, and net fields update only when necessary
    useEffect(() => {
        onChange("total", { value: total });
        onChange("deserved", { value: deserved });
        onChange("net", { value: net });
    }, [total, deserved, net, onChange]);

    const descriptionLengthValidator = (value: string | undefined): string | undefined => {
        if (value && value.length > 3000) {
            return "Description cannot exceed 1000 characters.";
        }
        return undefined;
    };

    const deductionDescriptionLengthValidator = (value: string | undefined): string | undefined => {
        if (value && value.length > 1000) {
            return "Deduction description cannot exceed 1000 characters.";
        }
        return undefined;
    };
    
    return (
        <FormElement>
            <fieldset className="k-form-fieldset">
                <Grid container spacing={1}>
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
                    <Grid item xs={12}>
                        <Field
                            id="description"
                            name="description"
                            label={getTranslatedLabel("projects.certificate.items.list.description", "Description *")}
                            component={FormTextArea}
                            disabled={formEditMode > 3}
                            rows={4} // Set rows for better visibility
                            validator={(value: string | undefined) => {
                                const requiredError = requiredValidator(value);
                                if (requiredError) return requiredError;
                                return descriptionLengthValidator(value);
                            }}
                        />
                    </Grid>
                    <Grid item xs={2}>
                        <Field
                            id="quantity"
                            name="quantity"
                            label={getTranslatedLabel("projects.certificate.items.list.quantity", "Quantity *")}
                            component={FormNumericTextBox}
                            format="n0"
                            min={1}
                            validator={requiredValidator}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={2}>
                        <Field
                            id="materialPrice"
                            name="materialPrice"
                            label={getTranslatedLabel("projects.certificate.items.list.materialPrice", "Material Price *")}
                            component={FormNumericTextBox}
                            format="n2"
                            min={0}
                            validator={requiredValidator}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={2}>
                        <Field
                            id="laborPrice"
                            name="laborPrice"
                            label={getTranslatedLabel("projects.certificate.items.list.laborPrice", "Labor Price *")}
                            component={FormNumericTextBox}
                            format="n2"
                            min={0}
                            validator={requiredValidator}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={2}>
                        <Field
                            id="total"
                            name="total"
                            label={getTranslatedLabel("projects.certificate.items.list.totalAmount", "Total")}
                            component={FormNumericTextBox}
                            format="n2"
                            value={total}
                            disabled
                        />
                    </Grid>
                    <Grid item  xs={2}>
                        <Field
                            id="deductions"
                            name="deductions"
                            label={getTranslatedLabel("projects.certificate.items.list.deductions", "Deductions")}
                            component={FormNumericTextBox}
                            format="n2"
                            min={0}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={12}>
                        <Field
                            id="deductionDescription"
                            name="deductionDescription"
                            label={getTranslatedLabel("projects.certificate.items.list.deductionDescription", "Deduction Description")}
                            component={FormInput}
                            disabled={formEditMode > 3}
                            validator={deductionDescriptionLengthValidator}
                        />
                    </Grid>
                    <Grid item xs={2}>
                        <Field
                            id="deserved"
                            name="deserved"
                            label={getTranslatedLabel("projects.certificate.items.list.deserved", "Deserved")}
                            component={FormNumericTextBox}
                            format="n2"
                            value={deserved}
                            disabled
                        />
                    </Grid>
                    <Grid item xs={3}>
                        {/* Purpose: Visually group insurance and its mode selector for better UX */}
                        {/* Context: Places RadioGroup directly below insurance field in the same column */}
                        <Grid container direction="column" spacing={1}>
                            <Grid item>
                                <Field
                                    id="insurance"
                                    name="insurance"
                                    label={getTranslatedLabel("projects.certificate.items.list.insurance", `Insurance (${insuranceMode})`)}
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
                                        label={getTranslatedLabel("projects.certificate.items.list.insuranceValue", "Value")}
                                    />
                                    <FormControlLabel
                                        value="percentage"
                                        control={<Radio disabled={formEditMode > 3} />}
                                        label={getTranslatedLabel("projects.certificate.items.list.insurancePercentage", "Percentage")}
                                    />
                                </RadioGroup>
                            </Grid>
                        </Grid>
                    </Grid>
                    <Grid item xs={3}>
                        <Grid container direction="column" spacing={1}>
                            <Grid item>
                                <Field
                                    id="additionalInsurance"
                                    name="additionalInsurance"
                                    label={getTranslatedLabel("projects.certificate.items.list.additionalInsurance", `Additional Insurance (${additionalInsuranceMode})`)}
                                    component={FormNumericTextBox}
                                    format={additionalInsuranceMode === "percentage" ? "n0" : "n2"}
                                    min={0}
                                    max={additionalInsuranceMode === "percentage" ? 100 : undefined}
                                    validator={
                                        additionalInsuranceMode === "percentage" &&
                                        valueGetter("additionalInsurance") !== undefined &&
                                        valueGetter("additionalInsurance") !== null
                                            ? percentageValidator
                                            : undefined
                                    }
                                    disabled={formEditMode > 3}
                                />
                            </Grid>
                            <Grid item>
                                <RadioGroup
                                    row
                                    value={additionalInsuranceMode}
                                    onChange={(e) => handleAdditionalInsuranceModeChange(e, formRenderProps.onChange)}
                                >
                                    <FormControlLabel
                                        value="value"
                                        control={<Radio disabled={formEditMode > 3} />}
                                        label={getTranslatedLabel("projects.certificate.items.list.additionalInsuranceValue", "Value")}
                                    />
                                    <FormControlLabel
                                        value="percentage"
                                        control={<Radio disabled={formEditMode > 3} />}
                                        label={getTranslatedLabel("projects.certificate.items.list.additionalInsurancePercentage", "Percentage")}
                                    />
                                </RadioGroup>
                            </Grid>
                        </Grid>
                    </Grid>
                    <Grid item xs={2}>
                        <Field
                            id="net"
                            name="net"
                            label={getTranslatedLabel("projects.certificate.items.list.net", "Net")}
                            component={FormNumericTextBox}
                            format="n2"
                            value={net}
                            disabled
                        />
                    </Grid>
                    <Grid item xs={2}>
                        <Field
                            id="achievementPercentage"
                            name="achievementPercentage"
                            label={getTranslatedLabel("projects.certificate.items.list.achievementPercentage", "Achievement Percentage *")}
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