import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import {useAppDispatch, useAppSelector,} from "../../../../app/store/configureStore";

import Button from "@mui/material/Button";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import {Grid, Paper, Typography} from "@mui/material";
import {OrderAdjustment} from "../../../../app/models/order/orderAdjustment";

import {useSelector} from "react-redux";
import {
    jobOrderAdjustmentsSelector,
    jobOrderItemAdjustmentsSelector,
    setUiJobOrderAdjustments,
} from "../../slice/jobOrderUiSlice";
import OrderItemAdjustmentForm from "../../form/order/OrderItemAdjustmentForm";

interface Props {
    orderItem?: any;
    showItemAdjustmentList: boolean;
    onClose: () => void;
    orderFormEditMode: number;
    width?: number;
}

function JobOrderItemAdjustmentsList({
                                         orderItem,
                                         showItemAdjustmentList,
                                         onClose,
                                         orderFormEditMode,
                                         width,
                                     }: Props) {
    // state for the grid
    const initialSort: Array<SortDescriptor> = [
        {field: "partyId", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    // state for showing adjustment item form
    const [show, setShow] = useState(false);

    const [orderAdjustment, setOrderAdjusment] = useState<OrderAdjustment | undefined>(undefined);
    const [editMode, setEditMode] = useState(0);
    const dispatch = useAppDispatch();
    const uiJobOrderItemAdjustments: any = useSelector(
        jobOrderItemAdjustmentsSelector,
    );
    const uiJobOrderAdjustments: any = useSelector(jobOrderAdjustmentsSelector);
    const {user} = useAppSelector((state) => state.account);
    const roleWithPercentage = user!.roles!.find(
        (role) => role.Name === "AddAdjustments",
    );

    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, []);

    function handleSelectOrderAdjustment(orderAdjustmentId: string) {
        // select order adjustment from orderAdjustments list
        const orderAdjustment = uiJobOrderItemAdjustments!.find(
            (adjustment: OrderAdjustment) =>
                adjustment.orderAdjustmentId === orderAdjustmentId,
        );
        setOrderAdjusment(orderAdjustment);

        setEditMode(2);
        setShow(true);
    }

    const DeleteOrderItemAdjustmentCell = (props: any) => {
        const {dataItem} = props;

        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    onClick={() => props.remove(dataItem)}
                    disabled={dataItem.isManual === "N"}
                    color="error"
                >
                    Remove
                </Button>
            </td>
        );
    };
    const remove = (dataItem: OrderAdjustment) => {
        const newOrderAdjustments: OrderAdjustment[] | undefined =
            uiJobOrderAdjustments?.map((item: OrderAdjustment) => {
                if (item.orderAdjustmentId === dataItem?.orderAdjustmentId) {
                    return {...item, isAdjustmentDeleted: true};
                } else {
                    return item;
                }
            });
        // update order items in the slice
        dispatch(setUiJobOrderAdjustments(newOrderAdjustments));
    };

    const CommandCell = (props: GridCellProps) => (
        <DeleteOrderItemAdjustmentCell {...props} remove={remove}/>
    );

    const orderAdjustmentCell = (props: any) => {
        return props.dataItem.isManual === "Y" ? (
            <td>
                <Button
                    onClick={() =>
                        handleSelectOrderAdjustment(props.dataItem.orderAdjustmentId)
                    }
                >
                    {props.dataItem.orderAdjustmentTypeDescription}
                </Button>
            </td>
        ) : (
            <td>{props.dataItem.orderAdjustmentTypeDescription}</td>
        );
    };

    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };

    return ReactDOM.createPortal(
        <CSSTransition
            in={showItemAdjustmentList}
            unmountOnExit
            timeout={{enter: 0, exit: 300}}
        >
            <div className="modal">
                <div
                    className="modal-content"
                    style={{width: width}}
                    onClick={(e) => e.stopPropagation()}
                >
                    <Paper elevation={5} className={`div-container-withBorderCurved`}>
                        <OrderItemAdjustmentForm
                            orderItem={orderItem}
                            orderAdjustment={orderAdjustment}
                            editMode={editMode}
                            onClose={() => setShow(false)}
                            show={show}
                            width={400}
                        />
                        <Grid container columnSpacing={1}>
                            <Grid container alignItems="center">
                                <Grid item xs={8}>
                                    <Typography sx={{p: 2}} variant="h4">
                                        Adjustments for {orderItem?.productName} <br/>
                                    </Typography>
                                </Grid>

                                <Grid item xs={4}>
                                    {roleWithPercentage && (
                                        <Button
                                            color={"secondary"}
                                            onClick={() => {
                                                setEditMode(1);
                                                setShow(true);
                                            }}
                                            variant="outlined"
                                            disabled={orderFormEditMode > 2}
                                        >
                                            Add Adjustment
                                        </Button>
                                    )}
                                </Grid>
                            </Grid>
                            <Grid item xs={12}>
                                <div className="div-container">
                                    <KendoGrid
                                        className="main-grid"
                                        style={{height: "400px"}}
                                        data={orderBy(
                                            uiJobOrderItemAdjustments
                                                ? uiJobOrderItemAdjustments
                                                : [],
                                            sort,
                                        ).slice(page.skip, page.take + page.skip)}
                                        sortable={true}
                                        sort={sort}
                                        onSortChange={(e: GridSortChangeEvent) => {
                                            setSort(e.sort);
                                        }}
                                        skip={page.skip}
                                        take={page.take}
                                        total={
                                            uiJobOrderItemAdjustments
                                                ? uiJobOrderItemAdjustments.length
                                                : 0
                                        }
                                        pageable={true}
                                        onPageChange={pageChange}
                                    >
                                        <Column
                                            field="orderAdjustmentTypeDescription"
                                            title="Adjustment Type"
                                            cell={orderAdjustmentCell}
                                            width={200}
                                        />
                                        <Column field="orderId" title="orderId" width={0}/>
                                        <Column
                                            field="orderItemSeqId"
                                            title="orderItemSeqId"
                                            width={0}
                                        />
                                        <Column field="amount" title="Amount" width={100}/>
                                        <Column
                                            field="sourcePercentage"
                                            title="Percentage"
                                            width={150}
                                        />
                                        <Column field="isManual" title="User Entered" width={150}/>
                                        <Column cell={CommandCell} width="100px"/>
                                    </KendoGrid>
                                </div>
                            </Grid>
                        </Grid>
                        <Grid item xs={12}></Grid>
                        <Grid item xs={2}>
                            <Button onClick={() => onClose()} variant="contained" color="error">
                                Close
                            </Button>
                        </Grid>
                    </Paper>
                </div>
            </div>
        </CSSTransition>,
        document.getElementById("root")!,
    );
}

export const JobOrderItemAdjustmentsListMemo = React.memo(
    JobOrderItemAdjustmentsList,
);
