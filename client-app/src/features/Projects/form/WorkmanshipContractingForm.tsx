import { Field, FormElement, FormRenderProps } from "@progress/kendo-react-form";
import React, { useEffect } from "react";
import { Button, Grid, Radio, RadioGroup, FormControlLabel } from "@mui/material";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { FormSimpleComboBoxVirtualProduct } from "../../../app/common/form/FormSimpleComboBoxVirtualProduct";
import { FormComboBoxVirtualUOM } from "../../../app/common/form/FormComboBoxVirtualUOM";
import { percentageValidator, requiredValidator } from "../../../app/common/form/Validators";
import FormButtons from "./FormButtons";
import FormTextArea from "../../../app/common/form/FormTextArea";
import FormInput from "../../../app/common/form/FormInput";


interface ContractingFormProps {
    formRenderProps: FormRenderProps;
    editMode: number;
    formEditMode: number;
    handleInsuranceModeChange: (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => void;
    handleAdditionalInsuranceModeChange: (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => void;
    calculateTotals: (
        valueGetter: FormRenderProps["valueGetter"],
        insuranceMode?: "value" | "percentage",
        additionalInsuranceMode?: "value" | "percentage"
    ) => {
        total: number;
        finalTotal: number;
        net: number;
        deserved: number;
        insurance: number;
        discount: number;
        additionalInsurance: number;
        transportationExpenses: number;
        gratuities: number;
    };
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    onClose: () => void;
    achievementPercentageValidator: (value: number) => string | undefined;
}

const priceValidator = (laborPrice: number | undefined, materialPrice: number | undefined): string | undefined => {
    if ((laborPrice === undefined || laborPrice === null) && (materialPrice === undefined || materialPrice === null)) {
        return "At least one of Labor Price or Material Price must be provided.";
    }
    return undefined;
};

const WorkmanshipContractingForm = ({
                                        formRenderProps,
                                        editMode,
                                        formEditMode,
                                        handleInsuranceModeChange,
                                        handleAdditionalInsuranceModeChange,
                                        calculateTotals,
                                        getTranslatedLabel,
                                        onClose,
                                        achievementPercentageValidator,
                                    }: ContractingFormProps) => {
    const { valueGetter, onChange } = formRenderProps;

    const calculated = React.useMemo(() => {
        const result = calculateTotals(
            valueGetter,
            (valueGetter("insuranceMode") as "value" | "percentage") ?? "value",
        (valueGetter("additionalInsuranceMode") as "value" | "percentage") ?? "value"
    );

        // DEBUG TABLE — DELETE WHEN DONE
        console.table({
            qty:               valueGetter("quantity"),
            matPrice:          valueGetter("materialPrice"),
            labPrice:          valueGetter("laborPrice"),
            pricePerUnit:      Number(valueGetter("materialPrice") || 0) + Number(valueGetter("laborPrice") || 0),
            total:             result.total,
            achPctRaw:         valueGetter("achievementPercentage"),
            achPctUsed:        typeof valueGetter("achievementPercentage") === "string"
                ? parseFloat(String(valueGetter("achievementPercentage")).replace(/[^\\d.-]/g, "")) || 0
                : Number(valueGetter("achievementPercentage") || 0),
            deservedBeforeDed: result.total * (Number(valueGetter("achievementPercentage") || 0) / 100),
            deductions:        valueGetter("deductions"),
            deserved:          result.deserved,
            insMode:           valueGetter("insuranceMode"),
            insuranceInput:    valueGetter("insurance"),
            insuranceCalc:     result.insurance,
            addInsMode:        valueGetter("additionalInsuranceMode"),
            addInsInput:       valueGetter("additionalInsurance"),
            addInsCalc:        result.additionalInsurance,
            net:               result.net,
        });

        return result;
    }, [
        valueGetter("quantity"),
        valueGetter("materialPrice"),
        valueGetter("laborPrice"),
        valueGetter("achievementPercentage"),
        valueGetter("deductions"),
        valueGetter("insurance"),
        valueGetter("additionalInsurance"),
        valueGetter("insuranceMode"),
        valueGetter("additionalInsuranceMode"),
        calculateTotals,
    ]);

// Update form fields when calculations change
    useEffect(() => {
        onChange("total",    { value: calculated.total });
        onChange("deserved", { value: calculated.deserved });
        onChange("net",      { value: calculated.net });
    }, [calculated, onChange]);

// Initial calculation on mount
    useEffect(() => {
        const init = calculateTotals(valueGetter);
        onChange("total",    { value: init.total });
        onChange("deserved", { value: init.deserved });
        onChange("net",      { value: init.net });
    }, []);

// Auto-set 100% achievement for new items
    useEffect(() => {
        const curAch = valueGetter("achievementPercentage");
        const hasPrice = Number(valueGetter("materialPrice") || 0) + Number(valueGetter("laborPrice") || 0) > 0;
        const isNewItem = editMode === 1;

        if ((curAch == null || curAch === "" || isNaN(Number(curAch))) && hasPrice && isNewItem) {
            onChange("achievementPercentage", { value: 100 });
        }
    }, [valueGetter, onChange, editMode]);

// Extract values for JSX
    const { total, deserved, net } = calculated;


const insuranceMode = (valueGetter("insuranceMode") as "value" | "percentage") ?? "value";
    const additionalInsuranceMode = (valueGetter("additionalInsuranceMode") as "value" | "percentage") ?? "value";




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
                <Field name="insuranceMode" component={() => null} />
                <Field name="additionalInsuranceMode" component={() => null} />
                    
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
                            format="n3"
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
                            format="n3"
                            min={0}
                            validator={() => priceValidator(valueGetter("laborPrice"), valueGetter("materialPrice"))}
                            disabled={formEditMode > 3}
                        />
                    </Grid>
                    <Grid item xs={2}>
                        <Field
                            id="laborPrice"
                            name="laborPrice"
                            label={getTranslatedLabel("projects.certificate.items.list.laborPrice", "Labor Price *")}
                            component={FormNumericTextBox}
                            format="n3"
                            min={0}
                            validator={() => priceValidator(valueGetter("laborPrice"), valueGetter("materialPrice"))}
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
                    <Grid item xs={2}>
                        <Field
                            id="achievementPercentage"
                            name="achievementPercentage"
                            label={getTranslatedLabel("projects.certificate.items.list.achievementPercentage", "Achievement % *")}
                            component={FormNumericTextBox}
                            format="n9"
                            min={1}
                            max={100}
                            validator={achievementPercentageValidator}
                            disabled={formEditMode > 3}
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
                    
                    <Grid item xs={12}>
                        <Grid container spacing={1}>
                            <Grid item xs={2}>
                                <Field
                                    id="deductions"
                                    name="deductions"
                                    label={getTranslatedLabel(
                                        "projects.certificate.items.list.deductions",
                                        "Deductions"
                                    )}
                                    component={FormNumericTextBox}
                                    format="n2"
                                    min={0}
                                    disabled={formEditMode > 3}
                                />
                            </Grid>
                            <Grid item xs={8}>
                                <Field
                                    id="deductionDescription"
                                    name="deductionDescription"
                                    label={getTranslatedLabel(
                                        "projects.certificate.items.list.deductionDescription",
                                        "Deduction Description"
                                    )}
                                    component={FormInput}
                                    disabled={formEditMode > 3}
                                    validator={deductionDescriptionLengthValidator}
                                />
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
                        </Grid>
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