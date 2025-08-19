import {useAppDispatch,} from "../../../../../app/store/configureStore";
import {useSelector} from "react-redux";
import {orderPaymentsEntities, orderPaymentsSelector, setUiJobOrderPayments} from "../../../slice/jobOrderUiSlice";

const useJobOrderUiPayments = () => {
    const dispatch = useAppDispatch();
    console.log('pymnt before useSelector: orderPaymentsEntities', orderPaymentsEntities);
    const uiOrderPayments = useSelector(orderPaymentsSelector)

    const generateId = (data: any) =>
        data.reduce((acc: any, current: any) => Math.max(acc, current.paymentId), 0) + 1;

    const insertItem = (item: any) => {
        item.paymentId = generateId(uiOrderPayments);
        item.inEdit = false;
        dispatch(setUiJobOrderPayments([item, ...uiOrderPayments]));
    };

    const getItems = () => {
        return uiOrderPayments;
    };

    const updateItem = (item: any) => {
        dispatch(setUiJobOrderPayments([item]));
    };

    const deleteItem = (item: any) => {
        dispatch(setUiJobOrderPayments([{...item, isPaymentDeleted: true}]));
        //dispatch(deletePayment([item.paymentId]));
    };

    return {uiOrderPayments, insertItem, getItems, updateItem, deleteItem};
};

export default useJobOrderUiPayments;

