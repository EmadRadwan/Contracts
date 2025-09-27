import { Field, FormElement, FormRenderProps } from "@progress/kendo-react-form";
import React, { useEffect } from "react";
import { Grid } from "@mui/material";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { FormSimpleComboBoxVirtualProduct } from "../../../app/common/form/FormSimpleComboBoxVirtualProduct";
import { FormComboBoxVirtualUOM } from "../../../app/common/form/FormComboBoxVirtualUOM";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import { requiredValidator } from "../../../app/common/form/Validators";
import FormButtons from "./FormButtons";
import FormInput from "../../../app/common/form/FormInput";

// Purpose: Ensure type safety for COMPANY_SUPPLY_SALE_CERTIFICATE, excluding discount fields
// Context: Reflects the requirement that CompanySupplyForm is identical to ContractorPurchaseForm
interface CompanySupplyFormProps {
    formRenderProps: FormRenderProps;
    editMode: number;
    formEditMode: number;
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
}

const CompanySupplyForm = ({
                               formRenderProps,
                               editMode,
                               formEditMode,
                               calculateTotals,
                               getTranslatedLabel,
                               onClose,
                           }: CompanySupplyFormProps) => {
    const { valueGetter, onChange } = formRenderProps;
    const { finalTotal } = calculateTotals(valueGetter);

    // Purpose: Ensure total field reflects calculated finalTotal without discount
    // Context: Consistent with ContractorPurchaseForm, updates total when finalTotal changes
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

export default CompanySupplyForm;