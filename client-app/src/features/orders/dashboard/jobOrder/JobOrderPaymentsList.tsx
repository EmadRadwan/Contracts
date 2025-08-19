import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridItemChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import {Grid} from "@mui/material";
import {useFetchPaymentsQuery} from "../../../../app/store/apis";
import {Payment} from "../../../../app/models/accounting/payment";
import {toast} from "react-toastify";
import {JobMyCommandCell} from "./JobOrderPayments/JobMyCommandCell";
import {JobDropDownCell} from "./JobOrderPayments/JobDropDownCell";
import useJobOrderUiPayments from "./JobOrderPayments/useJobOrderUiPayments";


interface Props {
    showPaymentList: boolean;
    onClose: () => void;
    orderId?: string;
    width?: number;
}


export default function JobOrderPaymentsList({showPaymentList, onClose, orderId, width}: Props) {

    // get deleteItem, insertItem, getItems, updateItem from useOrderUiPayments
    const {uiOrderPayments, insertItem, getItems, updateItem, deleteItem} = useJobOrderUiPayments();


    console.log('uiOrderPayments', uiOrderPayments);

    const {data: orderPaymentsData} = useFetchPaymentsQuery(orderId,
        {skip: orderId === undefined});


    const editField = "inEdit";

    const [data, setData] = useState<Array<Payment | undefined>>(orderPaymentsData ? orderPaymentsData : []);

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

    const addNew = () => {
        const newDataItem: Payment = {
            inEdit: true,
            paymentMethodTypeId: 'CASH',
            paymentId: '0',
        };
        setData([newDataItem, ...data]);
    };

    const CommandCell = (props: GridCellProps) => (
        <JobMyCommandCell
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


    console.log("uiOrderPayments:", uiOrderPayments);


    return ReactDOM.createPortal(
        <CSSTransition
            in={showPaymentList}
            unmountOnExit
            timeout={{enter: 0, exit: 300}}
        >
            <div className="modal">
                <div className="modal-content" style={{width: width}} onClick={e => e.stopPropagation()}>
                    <div className="div-container-withBorderBoxBlue">
                        <Grid container columnSpacing={1}>
                            <Grid container>
                                <div className="div-container">
                                    <KendoGrid className="main-grid" style={{height: "300px"}}
                                               data={data}
                                               onItemChange={itemChange}
                                               editField={editField}
                                               dataItemKey={"paymentId"}

                                    >
                                        <GridToolbar>
                                            <button
                                                title="Add new"
                                                className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-primary"
                                                onClick={addNew}
                                            >
                                                Add new
                                            </button>
                                        </GridToolbar>
                                        <Column field="paymentId" title="Id" editable={false} width={0}/>
                                        <Column field="amount" title="Amount" editor="numeric" width={150}/>
                                        <Column field="paymentMethodTypeId" title="Payment Method"
                                                cell={JobDropDownCell} width={200}/>
                                        <Column cell={CommandCell} width="240px"/>
                                    </KendoGrid>
                                </div>
                            </Grid>


                        </Grid>
                        <Grid item xs={2}>
                            <Button onClick={() => onClose()} variant="contained">
                                Close
                            </Button>
                        </Grid>
                    </div>

                </div>
            </div>
        </CSSTransition>,
        document.getElementById("root")!
    );
}