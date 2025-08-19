import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {useCallback, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent
} from "@progress/kendo-react-grid";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";

import Button from "@mui/material/Button";
import {Grid, Typography} from "@mui/material";
import {OrderAdjustment} from "../../../../app/models/order/orderAdjustment";

import {useSelector} from "react-redux";
import OrderItemAdjustmentForm from "../../form/order/OrderItemAdjustmentForm";
import {orderAdjustmentsSelector, orderItemAdjustments} from "../../slice/orderSelectors";
import {setUiOrderAdjustments} from "../../slice/orderAdjustmentsUiSlice";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {setNeedsTaxRecalculation} from "../../slice/sharedOrderUiSlice";

//todo: product name font size to be adjusted based on text length
//todo: Add ajustment button to be displayed only if user has permission to add adjustment
interface Props {
    orderItem?: any;
    onClose: () => void;
    orderFormEditMode: number
}

function OrderItemAdjustmentsList({
                                      orderItem,
                                      onClose,
                                      orderFormEditMode

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
    const addTax = useSelector((state: any) => state.sharedOrderUi.addTax);

    console.log('orderFormEditMode', orderFormEditMode);

    // state for showing adjustment item form
    const [show, setShow] = useState(false);


    const [orderAdjustment, setOrderAdjusment] = useState<OrderAdjustment | undefined>(undefined);
    const [editMode, setEditMode] = useState(0);
    const dispatch = useAppDispatch();
    const uiOrderItemAdjustments: any = useSelector(orderItemAdjustments)
    const uiOrderAdjustments: any = useSelector(orderAdjustmentsSelector)
    const {user} = useAppSelector(state => state.account);
    const roleWithPercentage = (user!.roles || []).find(role => role.Name === 'AddAdjustments');
    const {getTranslatedLabel} = useTranslationHelper()
    const localizationKey = 'order.so.items.adj.list'

    console.count('OrderItemAdjustmentsList.tsx Rendered');


    function handleSelectOrderAdjustment(orderAdjustmentId: string) {
        // select order adjustment from orderAdjustments list
        const orderAdjustment = uiOrderItemAdjustments!.find((adjustment: OrderAdjustment) => adjustment.orderAdjustmentId === orderAdjustmentId)
        setOrderAdjusment(orderAdjustment)

        setEditMode(2);
        setShow(true);
    }
    
    console.log('uiOrderItemAdjustments', uiOrderItemAdjustments);


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
                    {getTranslatedLabel(`general.remove`, "Remove")}
                </Button>
            </td>
        )
    };
    const remove = (dataItem: OrderAdjustment) => {
        const newOrderAdjustments: OrderAdjustment[] | undefined = uiOrderAdjustments?.map((item: OrderAdjustment) => {
            if (item.orderAdjustmentId === dataItem?.orderAdjustmentId) {
                return {...item, isAdjustmentDeleted: true};
            } else {
                return item;
            }
        });
        // update order items in the slice
        if (addTax) {
            dispatch(setNeedsTaxRecalculation(true));
        }
        dispatch(setUiOrderAdjustments(newOrderAdjustments));
    };

    const CommandCell = (props: GridCellProps) => (
        <DeleteOrderItemAdjustmentCell
            {...props}
            remove={remove}
        />
    )

    const orderAdjustmentCell = (props: any) => {

        return (props.dataItem.isManual === 'Y' ?
                <td>
                    <Button
                        onClick={() => handleSelectOrderAdjustment(props.dataItem.orderAdjustmentId)}
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

    const memoizedOnClose = useCallback(
        () => {
            setShow(false)
        },
        [],
    );

    return <React.Fragment>
        {show && (<ModalContainer show={show} onClose={memoizedOnClose} width={400}>
            <OrderItemAdjustmentForm orderItem={orderItem} orderAdjustment={orderAdjustment}
                                     editMode={editMode} onClose={memoizedOnClose}
            />
        </ModalContainer>)}

        <Grid container columnSpacing={1}>
            <Grid container alignItems="center">
                <Grid item xs={4}>
                    <Button color={"secondary"} onClick={() => {
                        setEditMode(1);
                        setShow(true);
                    }}
                            variant="outlined"
                            disabled={orderFormEditMode == 4}
                    >
                        {getTranslatedLabel(`${localizationKey}.add`, "Add Adjustment")}
                    </Button>
                </Grid>
                <Grid item xs={9}>
                    <Typography sx={{p: 2}} variant='h4'>
                        {getTranslatedLabel(`${localizationKey}.title`, "Adjustments for ")} {orderItem?.productName} <br/>
                    </Typography>
                </Grid>

                
            </Grid>
            <Grid item xs={12}>
                <KendoGrid className="main-grid" style={{height: "400px"}}
                           data={orderBy(uiOrderItemAdjustments ? uiOrderItemAdjustments : [], sort).slice(page.skip, page.take + page.skip)}
                           sortable={true}
                           sort={sort}
                           onSortChange={(e: GridSortChangeEvent) => {
                               setSort(e.sort);
                           }}
                           skip={page.skip}
                           take={page.take}
                           total={uiOrderItemAdjustments ? uiOrderItemAdjustments.length : 0}
                           pageable={true}
                           onPageChange={pageChange}

                >

                    <Column field="orderAdjustmentTypeDescription" title={getTranslatedLabel(`${localizationKey}.type`, "Adjustment Type")}
                            cell={orderAdjustmentCell} width={180}/>
                    <Column field="amount" title={getTranslatedLabel(`${localizationKey}.amount`, "Amount")} width={120}/>
                    <Column field="sourcePercentage" title={getTranslatedLabel(`${localizationKey}.percentage`, "Percentage")} width={140}/>
                    <Column field="isManual" title={getTranslatedLabel(`${localizationKey}.userEntered`, "User Entered")} width={150}/>
                    <Column cell={CommandCell} width="100px"/>


                </KendoGrid>
            </Grid>


        </Grid>
        <Grid item xs={12}>

        </Grid>
        <Grid item xs={2}>
            <Button onClick={() => onClose()} color="error" variant="contained">
                {getTranslatedLabel(`general.close`, "Close")}
            </Button>
        </Grid>

    </React.Fragment>

}

export const OrderItemAdjustmentsListMemo = React.memo(OrderItemAdjustmentsList)
