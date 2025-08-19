import {useAppDispatch} from "../../../../../app/store/configureStore";
import {useSelector} from "react-redux";
import {deletePayment, orderPaymentsEntities, setUiOrderPayments, updatePayment} from "../../../slice/orderPaymentsUiSlice";

const useOrderUiPayments = () => {
    const dispatch = useAppDispatch();
    // console.log('pymnt before useSelector: orderPaymentsEntities', orderPaymentsEntities);
    const uiOrderPayments = useSelector(orderPaymentsEntities);
    // console.log('pymnt after useSelector: uiOrderPayments', uiOrderPayments);

    const generateId = (data: any) =>
        data.reduce(
            (acc: any, current: any) => Math.max(acc, current.paymentId),
            0,
        ) + 1;

    const insertItem = (item: any) => {
        item.paymentId = generateId(uiOrderPayments).toString();
        item.inEdit = false;
        dispatch(setUiOrderPayments([item, ...uiOrderPayments]));
    };

    const getItems = () => {
        return uiOrderPayments;
    };

    const updateItem = (item: any) => {
        dispatch(updatePayment(item));
    };

    const deleteItem = (item: any) => {
        // dispatch(setUiOrderPayments([{...item, isPaymentDeleted: true}]));
        dispatch(deletePayment([item.paymentId]));
    };

    return {uiOrderPayments, insertItem, getItems, updateItem, deleteItem};
};

export default useOrderUiPayments;
