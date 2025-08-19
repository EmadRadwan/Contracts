import React, {useEffect, useState} from "react";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {Paper, Typography} from "@mui/material";
import RadioButtonGroup from "../../../../app/components/RadioButtonGroup";
import CheckboxButtons from "../../../../app/components/CheckboxButtons";
import {OrderParams} from "../../../../app/models/order/order";
import FormInput from "../../../../app/common/form/FormInput";

//todo: search needs to add search text for customer name and phone number

interface Props {
    params: OrderParams;
    onClose: () => void;
    onSubmit: (orderParam: OrderParams) => void;
}

export default function OrderSearchForm({params, onSubmit, onClose}: Props) {
    const [orderTypeDesc, setOrderTypeDesc] = useState<string[]>()

    const orderTypes = [
        {orderTypeId: "SALES_ORDER", description: 'Sales'},
        {orderTypeId: "PURCHASE_ORDER", description: 'Purchase'}
    ]

    console.count("OrderSearch Rendered");

    const [orderBy, setOrderBy] = useState(params.orderBy)
    const [orderTypeArray, setOrderTypeArray] = useState<string[]>()
    const [customerName, setCustomerName] = useState(params.customerName)
    const [customerPhone, setCustomerPhone] = useState(params.customerPhone)
    // eslint-disable-next-line react-hooks/exhaustive-deps
    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };


    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, [closeOnEscapeKeyDown]);


    const getOrderTypes = (): string[] => {
        return orderTypes.map(type => type.description)
    }


    const handleChangedCheckBoxButtons = (items: string[]) => {
        const filteredOrderTypes = orderTypes.filter(type => items.includes(type.description))
        const values: any[] = filteredOrderTypes.map(type => type.orderTypeId)

        const descArray = filteredOrderTypes.map(type => type.description)
        setOrderTypeDesc(descArray.length > 0 ? descArray : [''])
        setOrderTypeArray(values)
    }


    const sortOptions = [
        {value: 'orderIdAsc', label: 'Order ID Asc'},
        {value: 'orderIdDesc', label: 'Order ID Desc'},
        {value: 'createdStampAsc', label: 'Order Date Asc'},
        {value: 'createdStampDesc', label: 'Order Date Desc'},
    ]

    async function handleSubmitData(data: any) {
        const orderParam: OrderParams = {orderBy: orderBy, orderTypes: orderTypeArray, customerName, customerPhone}
        onSubmit(orderParam)
        setCustomerName("")
        setCustomerPhone("")
        setOrderTypeArray([])
        setOrderTypeDesc([])
        onClose();
    }


    return <React.Fragment>
        <Form
            initialValues={params}
            onSubmitClick={values => handleSubmitData(values)}
            render={(formRenderProps) => (

                <FormElement>
                    <fieldset className={'k-form-fieldset'}>
                        <Grid container spacing={1} p={1}>
                            <Grid item xs={5}>
                                <Paper sx={{mb: 2, p: 2}} style={{height: 200}}>
                                    <Typography sx={{fontWeight: "bold"}}>
                                        Sort Options:
                                    </Typography>
                                    <RadioButtonGroup
                                        selectedValue={orderBy}
                                        options={sortOptions}
                                        onChange={(e) => setOrderBy(e.target.value)}
                                    />
                                </Paper>

                            </Grid>
                            <Grid item xs={5}>
                                <Paper sx={{mb: 2, p: 2}} style={{height: 200}}>
                                    <CheckboxButtons
                                        items={getOrderTypes()}
                                        checked={orderTypeDesc}
                                        onChange={handleChangedCheckBoxButtons}

                                    />
                                </Paper>
                            </Grid>


                        </Grid>
                        <Grid container spacing={1} p={1}>
                            <Grid item xs={5}>
                                <Field
                                    id={'orderSearchByCustomerPhoneNum'}
                                    name={'orderSearchByCustomerPhoneNum'}
                                    label={'Customer Phone Number'}
                                    component={FormInput}
                                    value={customerPhone}
                                    onChange={(e) => setCustomerPhone(e.target.value)}
                                    autoComplete={"off"}
                                />
                            </Grid>
                            <Grid item xs={5}>
                                <Field
                                    id={'orderSearchByCustomerName'}
                                    name={'orderSearchByCustomerName'}
                                    label={'Customer'}
                                    component={FormInput}
                                    value={customerName}
                                    onChange={(e) => setCustomerName(e.target.value)}
                                    autoComplete={"off"}
                                />
                            </Grid>

                        </Grid>

                        <div className="k-form-buttons">

                            <Grid container p={1}>
                                <Grid item xs={2}>
                                    <Button
                                        variant="contained"
                                        type={'submit'}
                                        color="success"
                                    >
                                        Find
                                    </Button>
                                </Grid>
                                <Grid item xs={2}>
                                    <Button onClick={() => onClose()} color="error" variant="contained">
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


}
export const OrderSearchFormMemo = React.memo(OrderSearchForm)
