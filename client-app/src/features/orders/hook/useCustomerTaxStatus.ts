import {useAppSelector, useFetchCustomerTaxStatusQuery} from '../../../app/store/configureStore';

export function useCustomerTaxStatus() {

    const customerId = useAppSelector(
        (state) => state.sharedOrderUi.selectedCustomerId
    );

    const {data: customerTaxStatus} = useFetchCustomerTaxStatusQuery(
        customerId,
        {skip: customerId === undefined}
    );

    return {
        customerTaxStatus,
    };
}

