import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {useCallback, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import {Grid, Skeleton, Checkbox, FormControlLabel, Typography, Chip} from "@mui/material";
import {OrderItem} from "../../../../app/models/order/orderItem";
import {useFetchOrderAdjustmentsQuery, useFetchSalesOrderItemsQuery} from "../../../../app/store/apis";

import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";

import {OrderItemAdjustmentsListMemo} from "./OrderItemAdjustmentsList";
import {useSelector} from "react-redux";
import {OrderAdjustment} from "../../../../app/models/order/orderAdjustment";
import {SalesOrderItemFormMemo} from "../../form/order/SalesOrder/SalesOrderItemForm";
import {
    nonDeletedOrderItemsSelector,
    orderAdjustmentsSelector,
    orderSubTotal,
    selectAdjustedOrderItems
} from "../../slice/orderSelectors";
import {setSelectedOrderItem, setUiOrderItems} from "../../slice/orderItemsUiSlice";
import {setUiOrderAdjustments} from "../../slice/orderAdjustmentsUiSlice";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import OrderAdjustmentsList from "./OrderAdjustmentsList";
import OrderTermsList from "../../form/order/SalesOrder/OrderTermsList";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import store from "../../../../app/store/store";
import {SalesOrderItemFormBarcodeMemo} from "../../form/order/SalesOrder/SalesOrderItemFormBarcode";

interface Props {
    orderFormEditMode: number
    orderId?: string
}


export default function SalesOrderItemsList({orderFormEditMode, orderId}: Props) {
    const orderSTotal: any = useSelector(orderSubTotal);
    // state for the grid
    const initialSort: Array<SortDescriptor> = [
        {field: "orderItemSeqId", dir: "asc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const [showList, setShowList] = useState(false);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    const [useBarcode, setUseBarcode] = useState(false);

    // state for the order items component
    const [show, setShow] = useState(false);

    // state for the order item adjustments component
    const [showItemAdjustmentList, setShowItemAdjustmentList] = useState(false);
    const [showTermsModal, setShowTermsModal] = useState(false);


    // state for the order items component edit mode
    const [editMode, setEditMode] = useState(0);

    // state for selected order item for editing
    const [orderItem, setOrderItem] = useState<OrderItem | undefined>(undefined);

    const uiOrderItems: any = useSelector(nonDeletedOrderItemsSelector)
    const uiOrderAdjustments: any = useSelector(orderAdjustmentsSelector)
    const customerId = useAppSelector(state => state.sharedOrderUi.selectedCustomerId);
    const adjustedOrderItems = useSelector(selectAdjustedOrderItems);
    const localizationKey = 'order.so.items.list'
    const {getTranslatedLabel} = useTranslationHelper()

    // get the order items from the store
    const { data: orderItemsData, isLoading: isLoadingOrderItems } = useFetchSalesOrderItemsQuery(orderId, { skip: orderId === undefined });
    const { data: orderAdjustmentsData, isLoading: isLoadingAdjustments } = useFetchOrderAdjustmentsQuery(orderId, { skip: orderId === undefined });



    const dispatch = useAppDispatch();


    // handle order item selection from grid for editing
    function handleSelectOrderItem(orderItemId: string) {
        // get the order item from the orderItemsAndAdjustments state
        const selectedOrderItem: OrderItem | undefined = uiOrderItems!.find((orderItem: any) =>
            orderItem.orderId.concat(orderItem.orderItemSeqId) === orderItemId);

        // change string based productId with the actual product object
        // if this orderItem is a saved one then we'll use the one from orderItemProduct, else we'll use the one from productLov
        let orderItemToDisplay;
        if (selectedOrderItem?.orderItemProduct) {
            orderItemToDisplay = {...selectedOrderItem, productId: {
                    productId: selectedOrderItem?.productId,
                    productName: selectedOrderItem?.productName,
                    inventoryItem: selectedOrderItem?.inventoryItem,
                    availableToPromiseTotal: selectedOrderItem?.availableToPromiseTotal,
                    quantityOnHandTotal: selectedOrderItem?.quantityOnHandTotal,
                    uomDescription: selectedOrderItem?.uomDescription,
                    weightUom: selectedOrderItem?.weightUom || null // Include weightUom, fallback to null
                }};
        } else {
            orderItemToDisplay = {...selectedOrderItem, productId: selectedOrderItem?.productLov};
        }

        console.log('orderItemToDisplay', orderItemToDisplay);
        // set the selected order item in state and start edit mode
        setOrderItem(orderItemToDisplay);
        setEditMode(2);
        setShow(true);
    }


    const ItemDiscountCommandCell = (props: any) => {
        const item = props.dataItem;
        const discountAdjs = uiOrderAdjustments.filter(
            (a: OrderAdjustment) =>
                a.orderAdjustmentTypeId === "DISCOUNT_ADJUSTMENT" &&
                a.correspondingProductId === props.dataItem.productId && a.orderItemSeqId === props.dataItem.orderItemSeqId
        );

        const salesTaxAdjs = uiOrderAdjustments.filter(
          (a: OrderAdjustment) =>
            a.orderAdjustmentTypeId === "VAT_TAX" &&
            a.correspondingProductId === props.dataItem.productId &&
            a.orderItemSeqId === props.dataItem.orderItemSeqId
        );

        const discountsNotProcessedYet =
            (discountAdjs.length > 0 || salesTaxAdjs.length > 0) &&
            props.dataItem.discountAndPromotionAdjustments === 0;

        const discountValue = discountAdjs.reduce(
            (a: number, b: OrderAdjustment) => a + (b.amount || 0),
            0
        );

        const salesTaxValue = salesTaxAdjs.reduce(
            (a: number, b: OrderAdjustment) => a + (b.amount || 0),
            0
        );

        const taxNotProcessedYet = props.dataItem.totalItemTaxAdjustments !== salesTaxValue;

        const finalDiscount = discountsNotProcessedYet
            ? discountValue
            : props.dataItem.discountAndPromotionAdjustments;

        const finalTax = taxNotProcessedYet
            ? salesTaxValue
            : props.dataItem.totalItemTaxAdjustments;

        const totalAdjustments = finalDiscount + finalTax;

        const showBreakdown = finalDiscount !== 0 && finalTax > 0;
        
        console.log('showBreakdown', showBreakdown);

        return (
          <td>
            <Button
              onClick={() => {
                const selectedOrderItem: OrderItem | undefined =
                  uiOrderItems!.find(
                    (orderItem: any) =>
                      orderItem.orderId.concat(orderItem.orderItemSeqId) ===
                      props.dataItem.orderId.concat(
                        props.dataItem.orderItemSeqId
                      )
                  );

                dispatch(setSelectedOrderItem(selectedOrderItem));
                setOrderItem(selectedOrderItem);
                setShowItemAdjustmentList(true);
              }}
              title={
                showBreakdown
                  ? `Discount: ${finalDiscount.toFixed(
                      2
                    )}\nTax: ${finalTax.toFixed(2)}`
                  : undefined
              }
            >
              {totalAdjustments.toFixed(2)}
              {showBreakdown && (
                <span
                  style={{
                    fontSize: "1em", // Increase font size
                    color: "#333", // Darker color for better visibility
                    marginLeft: 6, // Add more space between the main value and breakdown
                    fontWeight: "bold", // Make it bold
                    backgroundColor: "#f0f0f0", // Light background for contrast
                    padding: "2px 6px", // Add padding for a better box effect
                    borderRadius: "4px", // Rounded corners
                  }}
                >
                  (D: {finalDiscount.toFixed(2)}, T: {finalTax.toFixed(2)})
                </span>
              )}
            </Button>
          </td>
        );
    };

    const memoizedOnClose2 = useCallback(() => {
        setShowList(false);
    }, [])
    const memoizedOnClose3 = useCallback(() => {
        setShowTermsModal(false);
      }, []);



    // custom grid cell for order item selection
    const orderItemCell = (props: any) => {

        return (
            <td>
                <Button
                    onClick={() => handleSelectOrderItem(props.dataItem.orderId.concat(props.dataItem.orderItemSeqId))}
                    disabled={props.dataItem.isPromo === 'Y'}
                >
                    {props.dataItem.productName}
                </Button>


            </td>
        )
    };

    const SubtotalDisplayCell = (props: any) => {
        const discountAdjs = uiOrderAdjustments.filter((a: OrderAdjustment) =>  (a.orderAdjustmentTypeId === "DISCOUNT_ADJUSTMENT") && a.correspondingProductId === props.dataItem.productId)
        const salesTaxAdjs = uiOrderAdjustments.filter((a: OrderAdjustment) =>  (a.orderAdjustmentTypeId === "VAT_TAX") && a.correspondingProductId === props.dataItem.productId)
        const discountsNotProcessedYet = (discountAdjs.length > 0 || salesTaxAdjs.length > 0) && props.dataItem.discountAndPromotionAdjustments === 0
        const discountValue = discountAdjs.reduce((a: number,b: OrderAdjustment) => a + b.amount!, 0)
        const salesTaxValue = salesTaxAdjs.reduce((a: number,b: OrderAdjustment) => a + b.amount!, 0)
        const taxNotProcessedYet = props.dataItem.totalItemTaxAdjustments !== salesTaxValue
        const finalDiscount = discountsNotProcessedYet ? discountValue : props.dataItem.discountAndPromotionAdjustments
        const finalTax = taxNotProcessedYet ? salesTaxValue : props.dataItem.totalItemTaxAdjustments
        
        return (
            <td>
                {(props.dataItem.subTotal + finalTax + finalDiscount).toFixed(2)}
            </td>
        )
    }

    // showing a Material-UI Chip when isBackOrdered is true to align with the UI style of ItemDiscountCommandCell.
    const QuantityCell = (props: GridCellProps) => {
        const { dataItem } = props;
        const { getTranslatedLabel } = useTranslationHelper();

        return (
            <td>
                <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                    <Typography>{dataItem.quantity.toFixed(2)}</Typography>
                    {dataItem.isBackOrdered && (
                        <Chip
                            label={getTranslatedLabel(`${localizationKey}.backordered`, "Backordered")}
                            size="small"
                            color="warning"
                            sx={{
                                fontSize: '0.75em',
                                height: '20px',
                                backgroundColor: '#ff9800',
                                color: '#fff',
                                fontWeight: 'bold',
                            }}
                        />
                    )}
                </div>
            </td>
        );
    };

    // close the order item form
    const memoizedOnClose = useCallback(
        () => {
            setShow(false)
        },
        [],
    );
    // close the order item Adjustment list
    const onCloseItemAdjustmentList = useCallback(
        () => {
            setShowItemAdjustmentList(false)
        },
        [],
    );

    // custom grid button function to delete an order item
    const DeleteOrderItemCell = (props: any) => {
        const {dataItem} = props;

        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    onClick={() =>
                        props.remove(dataItem)
                    }
                    disabled={props.dataItem.isPromo === 'Y' || orderFormEditMode > 3}
                    color="error"
                >
                    {getTranslatedLabel("general.remove", "Remove")}
                </Button>
            </td>
        )
    };


    // the actual deletion logic for an order item
    const remove = (dataItem: OrderItem) => {
        // get a local copy of the order items
        let localOrderItems: OrderItem[] | undefined = uiOrderItems;

        // check if the order item has a related promo item(s) and delete them as well
        // start by checking if the other order item is a promo item and by using 
        // parentOrderItemSeqId to find the related order items

        const relatedOrderItems = uiOrderItems?.filter((item: OrderItem) => {
            return item.parentOrderItemSeqId === dataItem?.orderItemSeqId;
        });
        // mark related order items as deleted
        const newRelatedOrderItems = relatedOrderItems?.map((item: OrderItem) => {
            return {...item, isProductDeleted: true};
        });

        // update local order items with the deleted order items in newRelatedOrderItems
        localOrderItems = localOrderItems?.map((item: OrderItem) => {
            if (newRelatedOrderItems?.some((orderItem: OrderItem) => orderItem.orderItemSeqId === item.orderItemSeqId)) {
                return newRelatedOrderItems?.find((orderItem: OrderItem) => orderItem.orderItemSeqId === item.orderItemSeqId);
            } else {
                return item;
            }

        });

        // mark the original order item as deleted in local order items
        localOrderItems = localOrderItems?.map((item: OrderItem) => {
            if (item.orderItemSeqId === dataItem?.orderItemSeqId) {
                return {...item, isProductDeleted: true};
            } else {
                return item;
            }
        });

        // set the order items in state
        dispatch(setUiOrderItems(localOrderItems!));


        // also mark adjustments that are related to all the order items 
        // we are deleting that are defined in relatedOrderItems
        const relatedOrderAdjustments = uiOrderAdjustments?.filter((item: OrderAdjustment) => {
            return localOrderItems?.some(
                (orderItem: OrderItem) =>
                    orderItem.orderItemSeqId === item.orderItemSeqId && orderItem.isProductDeleted
            );
        });
        // mark related order adjustments as deleted
        const newRelatedOrderAdjustments = relatedOrderAdjustments?.map((item: OrderAdjustment) => {
            return {...item, isAdjustmentDeleted: true};
        });
        // set the order adjustments in state
        dispatch(setUiOrderAdjustments(newRelatedOrderAdjustments!));

    };

    // support for order item deletion cell
    const CommandCell = (props: GridCellProps) => (
        <DeleteOrderItemCell
            {...props}
            remove={remove}
        />
    );
    const isAddingWithBarcode = editMode === 1 && useBarcode;

    console.log('orderItem from list', orderItem);
    return (
        <>
            <ModalContainer show={show} onClose={memoizedOnClose} width={700}>
                {isAddingWithBarcode ? (
                    <SalesOrderItemFormBarcodeMemo
                        orderItem={orderItem}
                        editMode={editMode}
                        onClose={memoizedOnClose}
                        orderFormEditMode={orderFormEditMode}
                    />
                ) : (
                    <SalesOrderItemFormMemo
                        orderItem={orderItem}
                        editMode={editMode}
                        onClose={memoizedOnClose}
                        orderFormEditMode={orderFormEditMode}
                    />
                )}
            </ModalContainer>
            
            
            {showList && (
                <ModalContainer show={showList} onClose={memoizedOnClose2} width={900}>
                  <OrderAdjustmentsList onClose={memoizedOnClose2} />
                </ModalContainer>
            )}
            {showTermsModal && (
                <ModalContainer
                    show={showTermsModal}
                    onClose={memoizedOnClose3}
                    width={900}
                >
                    <OrderTermsList
                        onClose={memoizedOnClose3}
                        orderId={orderId!}
                    />
                </ModalContainer>
            )}

            {showItemAdjustmentList && (
                <ModalContainer show={showItemAdjustmentList} onClose={onCloseItemAdjustmentList} width={800}>
                    <OrderItemAdjustmentsListMemo orderItem={orderItem} onClose={onCloseItemAdjustmentList}
                                                  orderFormEditMode={orderFormEditMode}/>
                </ModalContainer>)}

            {isLoadingOrderItems || isLoadingAdjustments ? (
                <Grid container spacing={2} direction="column">
                    <Grid item>
                        <Skeleton animation="wave" variant="rounded" height={40} sx={{ width: '70%' }} />
                    </Grid>
                    <Grid item>
                        <Skeleton animation="wave" variant="rounded" height={40} sx={{ width: '70%' }} />
                    </Grid>
                    <Grid item>
                        <Skeleton animation="wave" variant="rounded" height={40} sx={{ width: '70%' }} />
                    </Grid>
                </Grid>
            ) : (
            <Grid container columnSpacing={1} direction={"column"} alignItems="flex-start" sx={{mt: 1}}>
                
                    <KendoGrid className="main-grid" style={{height: "40vh"}}
                               data={orderBy(adjustedOrderItems ? adjustedOrderItems : [], sort).slice(page.skip, page.take + page.skip)}
                               sortable={true}
                               sort={sort}
                               onSortChange={(e: GridSortChangeEvent) => {
                                   setSort(e.sort);
                               }}
                               skip={page.skip}
                               take={page.take}
                               total={adjustedOrderItems ? adjustedOrderItems.length : 0}
                               pageable={true}
                               onPageChange={pageChange}
                    >
                        <GridToolbar>
                            <Grid container direction={"row"} justifyContent={"space-between"} alignItems="center">
                                <Grid item>
                                    <Grid container direction="row" alignItems="center" spacing={2}>
                                        <Grid item>
                                            <FormControlLabel
                                                control={
                                                    <Checkbox
                                                        checked={useBarcode}
                                                        onChange={(e) => setUseBarcode(e.target.checked)}
                                                        disabled={editMode === 2 || customerId === undefined || orderFormEditMode > 3}
                                                    />
                                                }
                                                label={getTranslatedLabel(`${localizationKey}.useBarcode`, "Use Barcode")}
                                            />
                                        </Grid>
                                        <Grid item>
                                            <Button
                                                color={"secondary"}
                                                onClick={() => {
                                                    setOrderItem(undefined);
                                                    setEditMode(1);
                                                    setShow(true);
                                                }}
                                                variant="outlined"
                                                disabled={customerId === undefined || orderFormEditMode > 3}
                                            >
                                                {getTranslatedLabel(`${localizationKey}.add`, "Add Product")}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </Grid>
                                <Grid item rowSpacing={3}>
                                    <Button
                                        color="secondary"
                                        sx={{ marginX: 2 }}
                                        onClick={() => setShowList(true)}
                                        variant="outlined"
                                        disabled={orderSTotal === 0}
                                    >
                                        {getTranslatedLabel(`${localizationKey}.adj`, "Order Adjustments")}
                                    </Button>
                                    <Button
                                        onClick={() => setShowTermsModal(true)}
                                        variant="outlined"
                                    >
                                        {getTranslatedLabel(`${localizationKey}.terms`, "Order Terms")}
                                    </Button>
                                </Grid>
                            </Grid>
                        </GridToolbar>
                        <Column field="productName" title={getTranslatedLabel(`${localizationKey}.product`, "Product")} cell={orderItemCell} width={300}/>
                        <Column field="productId" title={getTranslatedLabel(`${localizationKey}.orderItem`,"ProductId")} width={110}/>
                        <Column field="orderItemSeqId" title={getTranslatedLabel(`${localizationKey}.orderItem`,"orderItemSeqId")} width={0}/>
                        <Column
                            field="quantity"
                            title={getTranslatedLabel(`${localizationKey}.quantity`, "Quantity")}
                            width={120}
                            cell={QuantityCell}
                        /> <Column field="quantity" title={getTranslatedLabel(`${localizationKey}.quantity`,"Quantity")} width={120}/>
                        <Column cell={ItemDiscountCommandCell} title={getTranslatedLabel(`${localizationKey}.discount`,"Discounts/Tax")} width={170}/>
                        <Column field="subTotal" cell={SubtotalDisplayCell} title={getTranslatedLabel(`${localizationKey}.total`,"Item Total")} width={120} format="{0:c}"/>
                        <Column cell={CommandCell} width="100px"/>

                    </KendoGrid>
            </Grid>
            )}
        </>

    )
}