import React, {useState} from "react";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import {OrderItem} from "../../../../../app/models/order/orderItem";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import {
    FormMultiColumnComboBoxVirtualPurchaseProduct
} from "../../../../../app/common/form/FormMultiColumnComboBoxVirtualPurchaseProduct";
import {ComboBoxChangeEvent} from "@progress/kendo-react-dropdowns";
import usePurchaseOrderItem from "../../../hook/usePurchaseOrderItem";
import { Typography } from "@mui/material";

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
    const [lastPrice, setLastPrice] = useState('');
    const [selectedProduct, setSelectedProduct] = React.useState(undefined)
    const MyForm = React.useRef<any>()
    const [formKey, setFormKey] = React.useState(1);
    const [initValue, setInitValue] = React.useState<OrderItem | undefined>(orderItem);

    const {
        handleSubmitData
    } = usePurchaseOrderItem({orderItem, editMode, setFormKey, setInitValue});


    const onCloseCombo = (event: ComboBoxChangeEvent) => {
        if (event!.target!.value!) {
            setLastPrice(event.target.value.lastPrice)
        }
    };

    return (
        <React.Fragment>
            <Form
                ref={MyForm}
                initialValues={initValue === undefined ? undefined : orderItem}
                key={formKey}
                onSubmit={(values: any) => {
                    handleSubmitData(values as OrderItem)
                    setLastPrice('')
                    setSelectedProduct(undefined)
                }}
                render={(formRenderProps) => (

                    <FormElement>

                        <fieldset className={'k-form-fieldset'}>

                                <Grid item xs={8}>
                                    <Field
                                        id={"productId"}
                                        name={"productId"}
                                        label={"Product"}
                                        component={FormMultiColumnComboBoxVirtualPurchaseProduct}
                                        autoComplete={"off"}
                                        validator={requiredValidator}
                                        onClose={onCloseCombo}
                                        onChange={(e) => {
                                            if (e.value === null || e.value === undefined) {
                                                setLastPrice("")
                                            }
                                            setSelectedProduct(e.value)
                                        }}
                                        disabled={editMode === 2}
                                    />
                                </Grid>
    
                                <Grid item container xs={12} spacing={2} alignItems={"flex-end"}>
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
                                    {selectedProduct && (
                                        <Grid item xs={4}>
                                            <Typography variant="h6" color={"blue"} fontWeight={"bold"}>{selectedProduct?.uomDescription!}</Typography>
                                        </Grid>
                                    )}
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
                                <Grid item xs={3}>{lastPrice && <div>Last Price: {lastPrice}</div>}</Grid>
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
                                            setLastPrice('');
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
