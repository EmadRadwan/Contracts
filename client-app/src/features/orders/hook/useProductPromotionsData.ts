import {useAppSelector} from '../../../app/store/configureStore';
import {useFetchAvailableProductPromotionsQuery} from '../../../app/store/apis';

export function useProductPromotionsData() {
    const productId = useAppSelector((state) => state.sharedOrderUi.selectedProductId);

    const {data: productPromotions} = useFetchAvailableProductPromotionsQuery(
        productId,
        {
            skip: productId === undefined,
        }
    );

    return {
        productPromotions,
    };
}
