import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import {Grid} from "@mui/material";
import {OrderAdjustment} from "../../../../app/models/order/orderAdjustment";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import {useSelector} from "react-redux";
import {
    jobOrderAdjustmentsSelector,
    jobOrderLevelAdjustments,
    setUiJobOrderAdjustments
} from "../../slice/jobOrderUiSlice";
import JobOrderAdjustmentForm from "../../form/order/JobOrder/JobOrderAdjustmentForm";

interface Props {
    showList: boolean;
    onClose: () => void;
    orderId?: string;
    width?: number;
}


export default function JobOrderAdjustmentsList({showList, onClose, orderId, width}: Props) {
    const initialSort: Array<SortDescriptor> = [
        {field: "partyId", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    const [show, setShow] = useState(false);
    const [jobOrderAdjustment, setJobOrderAdjustment] = useState<OrderAdjustment | undefined>(undefined);
    const uiJobOrderLevelAdjustments: any = useSelector(jobOrderLevelAdjustments)
    const uiJobOrderAdjustments: any = useSelector(jobOrderAdjustmentsSelector)

    const dispatch = useAppDispatch();


    const [editMode, setEditMode] = useState(0);
    const {user} = useAppSelector(state => state.account);
    const roleWithPercentage = user!.roles!.find(role => role.Name === 'AddAdjustments');


    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, []);


    function handleSelectJobOrderAdjustment(orderAdjustmentId: string) {
        // select order adjustment from orderAdjustments list
        const orderAdjustment = uiJobOrderLevelAdjustments!.find((adjustment: OrderAdjustment) => adjustment.orderAdjustmentId === orderAdjustmentId)


        setJobOrderAdjustment(orderAdjustment)


        setEditMode(2);
        setShow(true);
    }


    const DeleteOrderItemAdjustmentCell = (props: any) => {
        const {dataItem} = props;

        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    onClick={() =>
                        props.remove(dataItem)
                    }
                    disabled={dataItem.isManual === 'N'}
                    color="error"
                >
                    Remove
                </Button>
            </td>
        )
    };
    const remove = (dataItem: OrderAdjustment) => {
        const newNonDeletedOrderAdjustments = uiJobOrderAdjustments?.map((item: OrderAdjustment) => {
            if (item.orderAdjustmentId === dataItem?.orderAdjustmentId) {
                return {...item, isAdjustmentDeleted: true};
            } else {
                return item;
            }
        });
        dispatch(setUiJobOrderAdjustments(newNonDeletedOrderAdjustments));
    };

    const CommandCell = (props: GridCellProps) => (
        <DeleteOrderItemAdjustmentCell
            {...props}
            remove={remove}
        />
    )

    const JobOrderAdjustmentCell = (props: any) => {

        return (props.dataItem.isManual === 'Y' ?
                <td>
                    <Button
                        onClick={() => handleSelectJobOrderAdjustment(props.dataItem.orderAdjustmentId)}
                    >
                        {props.dataItem.orderAdjustmentTypeDescription}
                    </Button>
                </td>
                :
                <td>
                    {props.dataItem.orderAdjustmentTypeDescription}
                </td>
        )
    };

    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };

    ////console.log('orderItems', orderAdjustments);
    ////console.log('nonDeletedOrderAdjustments', nonDeletedOrderAdjustments);

    return ReactDOM.createPortal(
        <CSSTransition
            in={showList}
            unmountOnExit
            timeout={{enter: 0, exit: 300}}
        >
            <div className="modal">
                <div className="modal-content" style={{width: width}} onClick={e => e.stopPropagation()}>
                    <div className="div-container-withBorderBoxBlue">
                        <JobOrderAdjustmentForm jobOrderAdjustment={jobOrderAdjustment} editMode={editMode}
                                                onClose={() => setShow(false)}
                                                show={show}/>
                        <Grid container columnSpacing={1}>
                            <Grid container alignItems="center">
                                <Grid item xs={6}>
                                    {roleWithPercentage && <Button color={"secondary"} onClick={() => {
                                        setEditMode(1);
                                        setShow(true);
                                    }} variant="contained">
                                        Add Order Adjustment
                                    </Button>}
                                </Grid>
                            </Grid>
                            <Grid container>
                                <div className="div-container">
                                    <KendoGrid className="main-grid" style={{height: "300px"}}
                                               data={orderBy(uiJobOrderLevelAdjustments ? uiJobOrderLevelAdjustments : [], sort).slice(page.skip, page.take + page.skip)}
                                               sortable={true}
                                               sort={sort}
                                               onSortChange={(e: GridSortChangeEvent) => {
                                                   setSort(e.sort);
                                               }}
                                               skip={page.skip}
                                               take={page.take}
                                               total={uiJobOrderLevelAdjustments ? uiJobOrderLevelAdjustments.length : 0}
                                               pageable={true}
                                               onPageChange={pageChange}

                                    >

                                        <Column field="orderAdjustmentTypeDescription" title="Adjustment Type"
                                                cell={JobOrderAdjustmentCell} width={150}/>
                                        <Column field="orderId" title="orderId" width={0}/>
                                        <Column field="orderItemSeqId" title="orderItemSeqId" width={0}/>
                                        <Column field="amount" title="Amount" width={100}/>
                                        <Column field="description" title="Description" width={100}/>
                                        <Column field="sourcePercentage" title="Percentage" width={120}/>
                                        <Column field="isManual" title="User Entered" width={100}/>
                                        <Column cell={CommandCell} width="60px"/>

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
