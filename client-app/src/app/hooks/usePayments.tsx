import {useEffect} from "react";

import {useAppDispatch, useAppSelector} from "../store/configureStore";
import {useSelector} from "react-redux";
import {
    fetchPaymentMethodTypesAsync,
    fetchPaymentsAsync,
    fetchPaymentTypesAsync,
    getSelectedPaymentIdEntity,
    paymentSelectors,
    selectPaymentById
} from "../../features/accounting/payment/slice/paymentSlice";
import {
    getSelectedSinglePaymentIdEntity,
    selectSinglePaymentById
} from "../../features/accounting/payment/slice/singlePaymentSlice";

export default function usePayments() {
    const payments = useAppSelector(paymentSelectors.selectAll);


    const {paymentsLoaded, metaData, paymentTypes, paymentTypesLoaded} = useAppSelector(state => state.payment);
    const {paymentMethodTypesLoaded} = useAppSelector(state => state.payment);
    const dispatch = useAppDispatch();

    const selectPayment = (paymentId: string) => dispatch(selectPaymentById(paymentId))
    const selectedPayment: any = useSelector(getSelectedPaymentIdEntity)

    const selectSinglePayment = (paymentId: string) => dispatch(selectSinglePaymentById(paymentId))
    const selectedSinglePayment: any = useSelector(getSelectedSinglePaymentIdEntity)


    useEffect(() => {
        if (!paymentsLoaded) dispatch(fetchPaymentsAsync());
    }, [paymentsLoaded, dispatch])


    useEffect(() => {
        if (!paymentMethodTypesLoaded) dispatch(fetchPaymentMethodTypesAsync());
    }, [paymentMethodTypesLoaded, dispatch]);

    useEffect(() => {
        if (!paymentTypesLoaded) dispatch(fetchPaymentTypesAsync());
    }, [paymentTypesLoaded, dispatch]);


    const types = paymentTypes.map(type => type.description)


    return {
        payments,
        paymentsLoaded,
        selectPayment,
        selectSinglePayment,
        selectedSinglePayment,
        selectedPayment, types,
        metaData
    }
}