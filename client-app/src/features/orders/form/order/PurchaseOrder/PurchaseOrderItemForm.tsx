import React, {useState} from "react";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import {OrderItem} from "../../../../../app/models/order/orderItem";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import usePurchaseOrderItem from "../../../hook/usePurchaseOrderItem";
import {
    FormMultiColumnComboBoxVirtualSimplePurchaseProduct
} from "../../../../../app/common/form/FormMultiColumnComboBoxVirtualSimplePurchaseProduct";
import {FormComboBoxVirtualUOM} from "../../../../../app/common/form/FormComboBoxVirtualUOM";
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";

interface Props {
    orderItem?: any;
    editMode: number;
    onClose: () => void;
    orderFormEditMode: number
}

export default function PurchaseOrderItemForm({
                                                  orderItem,
                                                  editMode,
                                                  onClose,
                                                  orderFormEditMode,
                                              }: Props) {


    // const [buttonFlag, setButtonFlag] = useState(false);
    const MyForm = React.useRef<any>()
    const [formKey, setFormKey] = React.useState(1);
    const {getTranslatedLabel} = useTranslationHelper();

    const transformedInitialValues = orderItem
        ? {
            ...orderItem,
            uomId: orderItem.uomId && orderItem.uomName
                ? { UomId: orderItem.uomId, Description: orderItem.uomName }
                : null,
        }
        : undefined;
    
    const [initValue, setInitValue] = React.useState<OrderItem | undefined>(transformedInitialValues);

    const {
        handleSubmitData
    } = usePurchaseOrderItem({orderItem, editMode, setFormKey, setInitValue});

    console.log('initValue', initValue)
   
    return (
        <React.Fragment>
            <Form
                ref={MyForm}
                initialValues={initValue}
                key={formKey}
                onSubmit={(values: any) => {
                    handleSubmitData(values as OrderItem)
                }}
                render={(formRenderProps) => (

                    <FormElement>

                        <fieldset className={'k-form-fieldset'}>

                                <Grid item xs={8}>
                                    <Field
                                        id={"productId"}
                                        name={"productId"}
                                        label={"Product"}
                                        component={FormMultiColumnComboBoxVirtualSimplePurchaseProduct}
                                        autoComplete={"off"}
                                        validator={requiredValidator}
                                        disabled={editMode === 2}
                                    />
                                </Grid>

                            <Grid item xs={8}>
                                <Field
                                    id="uomId"
                                    name="uomId"
                                    label={getTranslatedLabel("projects.certificate.items.list.unitOfMeasure", "Unit of Measure *")}
                                    component={FormComboBoxVirtualUOM}
                                    validator={requiredValidator}
                                />
                            </Grid>

                            <Grid item xs={8}>
                                <Field
                                    id={'quantity'}
                                    format="n0"
                                    min={1}
                                    name={'quantity'}
                                    label={'Quantity *'}
                                    component={FormNumericTextBox}
                                    validator={requiredValidator}
                                    disabled={orderFormEditMode > 2}
                                />
                            </Grid>
    
                                <Grid item xs={8}>
                                    <Field
                                        id={'unitPrice'}
                                        format="n2"
                                        min={0.1}
                                        name={'unitPrice'}
                                        label={'Unit Price *'}
                                        component={FormNumericTextBox}
                                        validator={requiredValidator}
                                        disabled={orderFormEditMode > 2}
                                    />
                                </Grid>
                            <div className="k-form-buttons">
                                <Grid container>
                                    <Grid item xs={5}>
                                        <Button
                                            variant="contained"
                                            type={'submit'}
                                            color="success"
                                            disabled={!formRenderProps.allowSubmit}
                                        >
                                            {editMode === 2 ? 'Update' : 'Add'}
                                        </Button>
                                    </Grid>
                                    <Grid item xs={2}>
                                        <Button onClick={() => {
                                            onClose()
                                        }} variant="contained" color="error">
                                            Cancel
                                        </Button>
                                    </Grid>


                                </Grid>
                            </div>

                        </fieldset>

                    </FormElement>

                )}
            />
        </React.Fragment>
    );
}

export const PurchaseOrderItemFormMemo = React.memo(PurchaseOrderItemForm);
