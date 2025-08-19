import React, {useEffect, useRef, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridItemChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import {Grid} from "@mui/material";
import {useFetchBillingAccountsByPartyIdQuery, useFetchPaymentsQuery} from "../../../../app/store/apis";
import {Payment} from "../../../../app/models/accounting/payment";
import useOrderUiPayments from "./OrderPayments/useOrderUiPayments";
import {MyCommandCell} from "./OrderPayments/MyCommandCell";
import {DropDownCell} from "./OrderPayments/DropDownCell";
import {toast} from "react-toastify";
import {useAppSelector} from "../../../../app/store/configureStore";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import {useDispatch, useSelector} from "react-redux";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import Checkbox from '@mui/material/Checkbox'
import {setBillingAccountPayment} from "../../slice/orderPaymentsUiSlice";
import { orderLevelAdjustmentsTotal, orderLevelTaxTotal, orderSubTotal } from "../../slice/orderSelectors";


interface Props {
    onClose: () => void;
    orderId?: string;
    partyId?: string
}


export default function OrderPaymentsList({onClose, orderId, partyId}: Props) {

    // get deleteItem, insertItem, getItems, updateItem from useOrderUiPayments
    const {
        uiOrderPayments,
        insertItem,
        getItems,
        updateItem,
        deleteItem
    } = useOrderUiPayments();
    const {data: billingAccounts} = useFetchBillingAccountsByPartyIdQuery(partyId);
    const formRef = React.useRef<Form>(null);
    const dispatch = useDispatch()
    const {data: orderPaymentsData} = useFetchPaymentsQuery(orderId,
        {skip: orderId === undefined});
    const editField = "inEdit";
    const [data, setData] = useState<Payment[] | undefined>(orderPaymentsData ? orderPaymentsData : []);
    const [billingAccountAmount, setBillingAccountAmount] = useState<number | null>(null)
    const [customerBillingAccount, setCustomerBillingAccount] = useState<any>(undefined)
    const [checked, setChecked] = useState<boolean>(false);
    const [isBillingAccount, setIsBillingAccount] = useState<boolean>(checked);
    const billingAccountPaymentFromUi = useAppSelector(state => state.orderPaymentsUi.billingAccountPayment)
    const sTotal: any = useSelector(orderSubTotal);
    const aTotal: any = useSelector(orderLevelAdjustmentsTotal)
    const taxTotal: any = useAppSelector(orderLevelTaxTotal)
    const initialValues = billingAccountPaymentFromUi ?
        {
            billingAccountId: billingAccountPaymentFromUi.billingAccountId.billingAccountId,
            useUpToFromBillingAccount: billingAccountPaymentFromUi.useUpToFromBillingAccount
        } : {
            billingAccountAmount: customerBillingAccount?.billingAccountId,
            useUpToFromBillingAccount: billingAccountAmount
        };

    // get existig payments from slice
    useEffect(() => {
        if (uiOrderPayments) {
            setData(uiOrderPayments);
        }
    }, [uiOrderPayments]);

    useEffect(() => {
        if (data && data.length === 0) {
            addNew(sTotal + aTotal + taxTotal)
        }
    }, [data]);


    console.count('OrderPaymentsList Rendered');
    console.log('payment  data', data);
    const formRef2 = useRef<boolean>(false);

    useEffect(() => {
        setIsBillingAccount(checked)
    }, [checked]);


    const remove = (dataItem: Payment) => {
        deleteItem(dataItem);
        setData([...uiOrderPayments]);
    };

    const add = (dataItem: Payment) => {
        dataItem.inEdit = true;
        // check if dataItem.amount is undefined or null or less than .1 and toast error
        // if not then insertItem
        if (dataItem.amount === undefined || dataItem.amount === null || dataItem.amount < .1) {
            toast.error('Payment amount must be greater than 0.00');
        } else {
            insertItem(dataItem);
        }
    };

    const handleSubmit = (data: any) => {
        if (isBillingAccount) {
            if (data.values) {
                const {billingAccountId, useUpToFromBillingAccount} = data.values

                if (billingAccountId !== "" && useUpToFromBillingAccount && useUpToFromBillingAccount > 0) {
                    dispatch(setBillingAccountPayment({
                        billingAccountId: customerBillingAccount,
                        useUpToFromBillingAccount
                    }))
                }
            }

        } else {
            return
        }

    };

    const update = (dataItem: Payment) => {
        dataItem.inEdit = false;
        if (dataItem.amount === undefined || dataItem.amount === null || dataItem.amount < .1) {
            toast.error('Payment amount must be greater than 0.00');
        } else {
            updateItem(dataItem);
            setData(uiOrderPayments);
        }
    };

    // Local state operations
    const discard = (dataItem: Payment) => {
        const newData = [...data];
        newData.splice(0, 1);
        setData(newData);
    };

    const cancel = (dataItem: Payment) => {
        const originalItem = getItems().find(
            (p: any) => p.paymentId === dataItem.paymentId
        );
        const newData = data.map((item: any) =>
            item.paymentId === originalItem?.paymentId ? originalItem : item
        );
        setData(newData);
    };

    const enterEdit = (dataItem: Payment) => {
        const newData = data.map((item: any) =>
            item.paymentId === dataItem.paymentId ? {...item, inEdit: true} : item
        );
        setData(newData);
    };

    const itemChange = (event: GridItemChangeEvent) => {
        const field = event.field || "";
        const newData = data.map((item: any) =>
            item.paymentId === event.dataItem.paymentId
                ? {...item, [field]: event.value}
                : item
        );
        setData(newData);
    };

    const addNew = (amount = 0) => {
        console.log(amount)
        const newDataItem: Payment = {
            inEdit: true,
            paymentMethodTypeId: 'CASH',
            paymentId: '0',
            amount: Number(amount)
        };
        setData([newDataItem, ...data]);
    };

    const CommandCell = (props: GridCellProps) => (
        <MyCommandCell
            {...props}
            edit={enterEdit}
            remove={remove}
            add={add}
            discard={discard}
            update={update}
            cancel={cancel}
            editField={editField}
        />
    );

    useEffect(() => {
        setData(uiOrderPayments);
    }, [uiOrderPayments]);

    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, []);

    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };

    const onClearForm = () => {

        formRef.current!.onChange("billingAccountId", {
            value: undefined
        })
        setCustomerBillingAccount(undefined)
        formRef.current!.onChange("useUpToFromBillingAccount", {
            value: null
        })
        setBillingAccountAmount(null)

        dispatch(setBillingAccountPayment(undefined))
    }

    const onModalClose = () => {
        if (isBillingAccount) {
            if (customerBillingAccount) {
                if (!billingAccountAmount || billingAccountAmount === 0) {
                    toast.error("Enter Amount or Remove Billing Account")
                } else {
                    formRef.current?.onSubmit()
                    onClose()
                }
            } else {
                setChecked(false)
                onClose()
            }
        } else {
            onClose()
        }


    }

    const onBillingAccountChange = (e: any) => {
        setCustomerBillingAccount({
            billingAccountId: e.value,
            description: billingAccounts?.find(account => account.billingAccountId === e.value)?.description
        })
    }


    return <React.Fragment>
        <Grid container columnSpacing={1}>
            <Grid container paddingLeft={1}>
                <Grid container>
                    <div className="div-container">
                        <KendoGrid className="main-grid" style={{height: "350px"}}
                                   data={data}
                                   onItemChange={itemChange}
                                   editField={editField}
                                   dataItemKey={"paymentId"}

                        >
                            <GridToolbar>
                                <Grid item xs={6} paddingLeft={1} alignItems={'center'}>
                                    Use Billing Account:&nbsp;&nbsp;

                                    <Checkbox checked={checked} onChange={() => setChecked(!checked)}/>
                                </Grid>
                                <Grid item xs={2}/>
                                <Grid item xs={3} paddingLeft={2}>
                                    <Button
                                        className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-primary"
                                        color="success"
                                        onClick={() => addNew(0)}
                                    >
                                        Add Payment
                                    </Button>
                                </Grid>
                            </GridToolbar>
                            <Column field="paymentId" title="Id" editable={false} width={0}/>
                            <Column field="amount" title="Amount" editor="numeric" format="{0:n}"/>
                            <Column field="paymentMethodTypeId" title="Payment Method" cell={DropDownCell} width={180}/>
                            <Column cell={CommandCell} width="240px"/>
                        </KendoGrid>
                    </div>
                </Grid>
                {isBillingAccount && <Grid item xs={12} height={"150px"} alignItems={"center"} paddingLeft={2}>
                    <Form
                        ref={formRef}
                        initialValues={initialValues}
                        key={JSON.stringify(formRef2)}
                        onSubmitClick={values => handleSubmit(values)}
                        render={(formRenderProps) => (
                            <FormElement>
                                <fieldset className={'k-form-fieldset'}>
                                    <Grid container spacing={2} alignItems="flex-end">
                                        <Grid item xs={4}>
                                            <Field
                                                name={"billingAccountId"}
                                                id={"billingAccountId"}
                                                label={"Billing Account"}
                                                component={MemoizedFormDropDownList}
                                                value={customerBillingAccount}
                                                data={billingAccounts ? billingAccounts : []}
                                                dataItemKey={"billingAccountId"}
                                                textField={"description"}
                                                onChange={onBillingAccountChange}

                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                name={"useUpToFromBillingAccount"}
                                                id={"useUpToFromBillingAccount"}
                                                label={"Amount *"}
                                                format="n2"
                                                min={.01}
                                                value={billingAccountAmount}
                                                component={FormNumericTextBox}
                                                onChange={e => setBillingAccountAmount(e.value)}
                                            />
                                        </Grid>
                                        <Grid item xs={2}>
                                            <Button color="warning" onClick={onClearForm}
                                                    variant="contained">
                                                Clear
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </fieldset>

                            </FormElement>
                        )}
                    />

                </Grid>}
            </Grid>


        </Grid>
        <Grid item xs={2}>
            <Button onClick={onModalClose} color="error" variant="contained">
                Close
            </Button>
        </Grid>
    </React.Fragment>


}