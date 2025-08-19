import React from "react";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { Button, Grid, Typography } from "@mui/material";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { FormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { toast } from "react-toastify";
import { ReturnItem, ReturnAdjustment } from "../../../../app/models/order/return";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";

interface ReturnItemFormProps {
    item?: ReturnItem | ReturnAdjustment;
    isNew?: boolean;
    type: "item" | "adjustment";
    returnTypes: Array<{ returnTypeId: string; description: string }>;
    returnReasons: Array<{ returnReasonId: string; description: string }>;
    handleSubmit: (data: ReturnItem | ReturnAdjustment) => void;
    onClose: () => void;
}

const ReturnItemForm = ({ item, isNew, type, returnTypes, returnReasons, handleSubmit, onClose }: ReturnItemFormProps) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const formRef = React.useRef<Form>(null);

    // REFACTOR: Add quantity validation for return items
    // Purpose: Ensure returnQuantity is positive and valid
    // Why: Prevents invalid submissions
    const quantityValidator = (value: number) => {
        if (type === "item" && (!value || value < 1)) {
            toast.error("Quantity must be greater than zero.");
            return "Quantity must be greater than zero.";
        }
        return undefined;
    };

    // REFACTOR: Add price/amount validation
    // Purpose: Ensure returnPrice or amount is valid
    // Why: Ensures financial accuracy
    const priceValidator = (value: number) => {
        if ((type === "item" && !value) || (type === "adjustment" && value === undefined)) {
            toast.error(`${type === "item" ? "Price" : "Amount"} is required.`);
            return `${type === "item" ? "Price" : "Amount"} is required.`;
        }
        return undefined;
    };

    const onSubmit = (data: any) => {
        const errors: string[] = [];
        if (type === "item" && (!data.returnQuantity || data.returnQuantity < 1)) {
            errors.push("Quantity must be greater than zero.");
        }
        if (type === "item" && !data.returnPrice) {
            errors.push("Price is required.");
        }
        if (type === "adjustment" && data.amount === undefined) {
            errors.push("Amount is required.");
        }
        if (errors.length > 0) {
            toast.error(errors.join("\n"), { style: { whiteSpace: "pre-line" } });
            return;
        }
        handleSubmit(data);
    };

    // REFACTOR: Set initial values for new or existing rows
    // Purpose: Provide defaults for new rows, populate for existing
    // Why: Ensures form is pre-filled correctly
    const initialValues = isNew
        ? type === "item"
            ? {
                returnId: (item as ReturnItem).returnId,
                returnItemSeqId: `new_${Date.now()}`,
                orderId: "",
                productId: "",
                description: "",
                returnQuantity: 1,
                returnPrice: 0,
                returnReasonId: "",
                returnTypeId: "",
            }
            : {
                returnId: (item as ReturnAdjustment).returnId,
                returnAdjustmentId: `new_${Date.now()}`,
                description: "",
                amount: 0,
                returnTypeId: "",
            }
        : { ...item };

    return (
        <>
            <Typography variant="h4" color="black">
                {type === "item"
                    ? (item as ReturnItem)?.description || "New Return Item"
                    : (item as ReturnAdjustment)?.description || "New Adjustment"}
            </Typography>
            <Form
                initialValues={initialValues}
                ref={formRef}
                onSubmit={onSubmit}
                render={(formRenderProps) => (
                    <FormElement>
                        <fieldset className="k-form-fieldset">
                            <Grid container spacing={2} alignItems="center" justifyContent="center">
                                {type === "item" && (
                                    <>
                                        <Grid item xs={3}>
                                            <Field
                                                id="orderId"
                                                name="orderId"
                                                label={getTranslatedLabel("Order", "OrderId")}
                                                component="input"
                                                type="text"
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="productId"
                                                name="productId"
                                                label={getTranslatedLabel("Product", "ProductId")}
                                                component="input"
                                                type="text"
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="returnQuantity"
                                                name="returnQuantity"
                                                label={getTranslatedLabel("Order", "Quantity")}
                                                component={FormNumericTextBox}
                                                min={1}
                                                validator={quantityValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="returnPrice"
                                                name="returnPrice"
                                                label={getTranslatedLabel("Order", "Price")}
                                                component={FormNumericTextBox}
                                                validator={priceValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="returnReasonId"
                                                name="returnReasonId"
                                                label={getTranslatedLabel("Order", "ReturnReason")}
                                                component={FormDropDownList}
                                                data={returnReasons}
                                                dataItemKey="returnReasonId"
                                                textField="description"
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                    </>
                                )}
                                <Grid item xs={3}>
                                    <Field
                                        id="description"
                                        name="description"
                                        label={getTranslatedLabel("Common", "Description")}
                                        component="input"
                                        type="text"
                                        validator={requiredValidator}
                                    />
                                </Grid>
                                {type === "adjustment" && (
                                    <Grid item xs={3}>
                                        <Field
                                            id="amount"
                                            name="amount"
                                            label={getTranslatedLabel("Order", "Amount")}
                                            component={FormNumericTextBox}
                                            validator={priceValidator}
                                        />
                                    </Grid>
                                )}
                                <Grid item xs={3}>
                                    <Field
                                        id="returnTypeId"
                                        name="returnTypeId"
                                        label={getTranslatedLabel("Common", "Type")}
                                        component={FormDropDownList}
                                        data={returnTypes}
                                        dataItemKey="returnTypeId"
                                        textField="description"
                                        validator={requiredValidator}
                                    />
                                </Grid>
                            </Grid>
                            <div className="k-form-buttons">
                                <Grid container spacing={1}>
                                    <Grid item>
                                        <Button
                                            type="submit"
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

export default ReturnItemForm;