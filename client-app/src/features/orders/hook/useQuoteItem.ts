import React, {useState} from "react";
import {useAppDispatch} from "../../../app/store/configureStore";

import {toast} from "react-toastify";
import {useSelector} from "react-redux";
import {nonDeletedQuoteItemsSelector} from "../slice/quoteSelectors";
import {useCustomerTaxStatus} from "./useCustomerTaxStatus";
import {useProductPromotionsData} from "./useProductPromotionsData";
import {setRelatedRecords} from "../slice/quoteItemsUiSlice";
import {QuoteItem} from "../../../app/models/order/quoteItem";
import {useProcessQuoteItemQuery} from "../../../app/store/apis";


type UseQuoteItemProps = {
    quoteItem: any;
    editMode: number;
    setFormKey: any;
};
export default function useQuoteItem({
                                         quoteItem,
                                         editMode, setFormKey
                                     }: UseQuoteItemProps) {
    const {customerTaxStatus} = useCustomerTaxStatus();
    console.log('customerTaxStatus', customerTaxStatus);
    const productPromotions = useProductPromotionsData().productPromotions;

    const [updatedQuoteItem, setUpdatedQuoteItem] = useState<QuoteItem | undefined>(undefined);

    const quoteItemsFromUi: any = useSelector(nonDeletedQuoteItemsSelector);


    const dispatch = useAppDispatch();

    const {
        data: processQuoteItemData,
        error: processQuoteItemError,
        isLoading: processQuoteItemLoading,
        isFetching: processQuoteItemFetching
    }
        = useProcessQuoteItemQuery(updatedQuoteItem, {
        skip: updatedQuoteItem === undefined,
    });

    React.useEffect(() => {
        if (processQuoteItemData && processQuoteItemData!.status === 'Success') {
            setFormKey(Math.random());
        }

    }, [processQuoteItemData, setFormKey]);

    const emptyPromotion = {productPromoId: "", promoText: ""};
    const productPromotionsWithEmpty = productPromotions
        ? [emptyPromotion, ...productPromotions]
        : [emptyPromotion];

    console.log('productPromotions', productPromotions);


    async function handleSubmitData(data: any) {
        try {
            console.log('data from submit', data);
            const newQuoteItem = await createOrUpdateQuoteItem(data);
            setUpdatedQuoteItem({...newQuoteItem});

        } catch
            (error) {
            toast.error(error.message);
            handleError(error);
        }
    }


    function handleError(error: any) {
        console.log(error);
        // Additional error handling logic
    }

    async function createOrUpdateQuoteItem(data: any): Promise<QuoteItem> {
        let newQuoteItem: QuoteItem;
        if (editMode === 2) {
            newQuoteItem = {
                ...data,
                quantity: data.quantity,
                productId: data.productId.productId,
                collectTax: customerTaxStatus?.isExempt !== "Y",
            };
        } else {
            const quoteItemSeqId = quoteItemsFromUi?.length
                ? quoteItemsFromUi?.length + 1
                : 1;

            newQuoteItem = {
                inventoryItemId: data.productId.inventoryItem,
                productTypeId: data.productId.productTypeId,
                isProductDeleted: false,
                quoteItemSeqId: quoteItemSeqId.toString().padStart(2, "0"),
                productId: data.productId.productId,
                productLov: data.productId,
                productName: data.productId.productName,
                quantity: data.quantity,
                unitPrice: +data.productId.price.toFixed(2), // Limit to two decimal places
                unitListPrice: +data.productId.listPrice.toFixed(2), // Limit to two decimal places
                productPromoId: data.productPromoId,
                subTotal: +data.productId.price.toFixed(2) * data.quantity,
                collectTax: customerTaxStatus?.isExempt !== "Y",
            };

            if (quoteItem) {
                newQuoteItem.quoteId = quoteItem.quoteId;
            } else {
                newQuoteItem.quoteId = "QUOTE-DUMMY";
            }

            if (data.productId.productTypeId === "MARKETING_PKG") {
                dispatch(setRelatedRecords(data.productId.relatedRecords));
            }
        }
        return newQuoteItem;
    }

    return {
        productPromotionsWithEmpty,
        handleSubmitData,
        productPromotions,
        processQuoteItemData,
        processQuoteItemError,
        processQuoteItemLoading,
        processQuoteItemFetching,
    };
}
