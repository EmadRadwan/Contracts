import React, {createContext, useState} from 'react';
import {OrderItem} from "../models/order/orderItem";
import {OrderAdjustment} from "../models/order/orderAdjustment";
import {Order} from "../models/order/order";


interface ContextState {
    // set the type of state you want to handle with context e.g.
    selectedOrder: Order | undefined;
    orderItems: OrderItem[] | undefined;
    orderItemsWithAdjustments: OrderItem[] | undefined;
    setSelectedOrder: (order: Order) => void;
    setOrderItems: (orderItems: OrderItem[]) => void;
    orderAdjustments: OrderAdjustment[] | undefined;
    setOrderAdjustments: (orderAdjustments: OrderAdjustment[]) => void;
    subTotal: number;
    totalOrderLevelAdjustments: number;
    grandTotal: number;
    orderChanged: boolean;
    setOrderChanged: (orderChanged: boolean) => void;

}

const OrdersContext = createContext({} as ContextState);

function OrdersContextProvider({children}: any) {

    const [orderItemsWithAdjustments, setOrderItemsWithAdjustments] = useState<OrderItem[] | undefined>(undefined);
    const [orderItems, setOrderItems] = useState<OrderItem[] | undefined>(undefined);
    const [orderAdjustments, setOrderAdjustments] = useState<OrderAdjustment[] | undefined>(undefined);
    const [selectedOrder, setSelectedOrder] = useState<Order | undefined>(undefined);
    const [subTotal, setSubTotal] = useState<number>(0);
    const [totalOrderLevelAdjustments, setTotalOrderLevelAdjustments] = useState<number>(0);
    const [grandTotal, setGrandTotal] = useState<number>(0);
    const [orderChanged, setOrderChanged] = useState<boolean>(false);

    console.count('orderAdjustments from context')
    // console.log('orderAdjustments from context', orderAdjustments)


    const value: ContextState = {
        orderItems,
        setOrderItems,
        orderAdjustments,
        setOrderAdjustments,
        orderItemsWithAdjustments,
        selectedOrder,
        setSelectedOrder,
        subTotal,
        totalOrderLevelAdjustments,
        grandTotal,
        orderChanged,
        setOrderChanged
    };

    return (
        <OrdersContext.Provider value={value}>
            {children}
        </OrdersContext.Provider>
    );
}

export {OrdersContextProvider};
export default OrdersContext;