import {useEffect, useState} from "react";
import {toast} from "react-toastify";
import {useSelector} from "react-redux";
import {QuoteItem} from "../../../app/models/order/quoteItem";
import {
    useAppDispatch,
    useAppSelector,
    useFetchAvailableProductPromotionsQuery,
    useFetchCustomerTaxStatusQuery,
} from "../../../app/store/configureStore";
import {
    deletePromoQuoteAdjustments,
    deletePromoQuoteItem,
    quoteAdjustmentsSelector,
    quoteItemsSelector,
    setRelatedRecords,
    setUiQuoteAdjustments,
    setUiQuoteItems,
} from "../../services/slice/quoteUiSlice";
import {quotePromoProductDiscountEndpoints} from "../../../app/store/apis/quote/quotePromoProductDiscountApi";
import {QuoteAdjustment} from "../../../app/models/order/quoteAdjustment";
import {quoteTaxAdjustmentsEndpoints,} from "../../../app/store/apis";
import {Quote} from "../../../app/models/order/quote";

type UseQuoteItem = {
    quoteItem: any;
    editMode: number;
};
export default function useQuoteItem({
                                         quoteItem,
                                         editMode,
                                     }: UseQuoteItem) {
    const productId = useAppSelector((state) => state.quoteUi.selectedProductId);
    const [oItem, setOItem] = useState(quoteItem);

    const quoteItemsFromUi: any = useSelector(quoteItemsSelector);
    const quoteAdjustmentsFromUi: any = useSelector(quoteAdjustmentsSelector);

    const customerId = useAppSelector(
        (state) => state.quoteUi.selectedCustomerId,
    );
    const {data: customerTaxStatus} = useFetchCustomerTaxStatusQuery(
        customerId,
        {skip: customerId === undefined},
    );

    const dispatch = useAppDispatch();

    const {data: productPromotions} = useFetchAvailableProductPromotionsQuery(
        productId,
        {
            skip: productId === undefined,
        },
    );

    const emptyPromotion = {productPromoId: "", promoText: ""};
    const productPromotionsWithEmpty = productPromotions
        ? [emptyPromotion, ...productPromotions]
        : [emptyPromotion];

    useEffect(() => {
        // new quoteItem already created, no need to fetch product
        if (quoteItem !== undefined && quoteItem.quoteId === "ORDER-DUMMY") {
            setOItem({...quoteItem, productId: quoteItem.productLov});
        }
    }, [quoteItem]);

    useEffect(() => {
        // existing quoteItem, need to fetch product
        if (quoteItem !== undefined && quoteItem.quoteId !== "ORDER-DUMMY") {
            setOItem({...quoteItem});
        }
    }, [quoteItem]);

    useEffect(() => {
        if (quoteItem === undefined) {
            setOItem(undefined);
        }
    }, [oItem, quoteItem]);

    async function handleSubmitData(data: any) {
        let allQuoteItems: any[] = []; // Array to store both promo and original quote items
        let allQuoteAdjustments: any[] = []; // Similarly for quote adjustments

        try {
            const newQuoteItem = await createOrUpdateQuoteItem(data);
            // update allQuoteItems with newQuoteItem
            allQuoteItems.push(newQuoteItem);

            // if there's a promotion associated, call the promo API
            if (
                newQuoteItem.productPromoId !== undefined &&
                newQuoteItem.productPromoId !== "" &&
                newQuoteItem.productPromoId !== null
            ) {
                const promoResult = await applyProductPromotions(newQuoteItem);
                // if promoResult is not null, update allQuoteItems and allQuoteAdjustments with promoResult
                if (promoResult.promoResult === "Success") {
                    allQuoteItems = [...allQuoteItems, ...promoResult.promoQuoteItems];
                    allQuoteAdjustments = [
                        ...allQuoteAdjustments,
                        ...promoResult.promoQuoteAdjustments,
                    ];
                } else {
                    return;
                }
            }

            if (customerTaxStatus?.isExempt !== "Y") {
                const taxAdjustments = await fetchAndSetTaxAdjustments({
                    ...newQuoteItem,
                    productId: data.productId,
                });
                allQuoteAdjustments = [...allQuoteAdjustments, ...taxAdjustments];
            }

            // update quoteItems and quoteAdjustments in the ui
            dispatch(setUiQuoteAdjustments(allQuoteAdjustments));
            dispatch(setUiQuoteItems(allQuoteItems));

            toggleOItemState();
        } catch (error) {
            handleError(error);
        }
    }

    const fetchAndSetTaxAdjustments = async (
        quoteItem: QuoteItem,
    ): Promise<QuoteAdjustment[]> => {
        const currentQuoteItemAdjustments = quoteAdjustmentsFromUi?.filter(
            (quoteAdjustment: QuoteAdjustment) =>
                quoteAdjustment.quoteItemSeqId === quoteItem.quoteItemSeqId,
        );

        const result = await dispatch(
            quoteTaxAdjustmentsEndpoints.fetchQuoteTaxAdjustments.initiate({
                quoteItems: [quoteItem],
                quoteAdjustments: currentQuoteItemAdjustments || [],
            }),
        );

        if (result.status === "fulfilled") {
            return result.data || [];
        }

        return [];
    };

    async function createOrUpdateQuoteItem(data: any): Promise<QuoteItem> {
        // logic to create or update quote item
        let newQuoteItem: QuoteItem;
        if (editMode === 2) {
            newQuoteItem = {...data, quantity: data.quantity};
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
                quoteUnitListPrice: +data.productId.price.toFixed(2), // Limit to two decimal places
                productPromoId: data.productPromoId,
            };
            if (quoteItem) {
                newQuoteItem.quoteId = quoteItem.quoteId;
            } else {
                newQuoteItem.quoteId = "ORDER-DUMMY";
            }

            if (data.productId.productTypeId === "MARKETING_PKG") {
                dispatch(setRelatedRecords(data.productId.relatedRecords));
            }
        }
        return newQuoteItem;
    }

    // This function handles the logic when there's a promotion associated.
    // It will return the quote items and adjustments from the promo API.
    async function applyProductPromotions(newQuoteItem: QuoteItem): Promise<{
        promoQuoteItems: Quote[];
        promoQuoteAdjustments: QuoteAdjustment[];
        promoResult: string;
    }> {
        const promoQuoteItems: any[] = [];
        const promoQuoteAdjustments: any[] = [];
        let result: any | undefined = undefined;
        let promoResult = "";

        if (
            newQuoteItem.productPromoId !== undefined &&
            newQuoteItem.productPromoId !== "" &&
            newQuoteItem.productPromoId !== null
        ) {
            // using newQuoteItem.productPromoId, get promoActionEnumId from productPromotions
            const promoActionEnumId = productPromotions?.find(
                (p: any) => p.productPromoId === newQuoteItem.productPromoId,
            )?.productPromoActionEnumId;
            // switch case for promoActionEnumId
            switch (promoActionEnumId) {
                case "PROMO_PROD_DISC":
                    result = await dispatch(
                        quotePromoProductDiscountEndpoints.fetchQuotePromoProductDiscount.initiate(
                            newQuoteItem,
                        ),
                    );
                    break;
            }
            if (result !== undefined && result !== null) {
                console.log("result", result);
                if (result.data?.resultMessage === "Success") {
                    promoResult = result.data?.resultMessage;
                    // based on editMode, look for the order items in the promo
                    // and delete any similar order items and order item adjustments
                    // then add the new order item and order item adjustments
                    // then update the order items and order item adjustments in the ui
                    if (editMode === 2) {
                        // get from the result the productPromoId from the orderAdjustments
                        // as orderAdjustment.productPromoId should be the same as newQuoteItem.productPromoId
                        // and based on that, get the promo quoteItem that was added
                        // and delete it

                        const existingQuoteAdjustments = quoteAdjustmentsFromUi.filter(
                            (uiOa: any) =>
                                uiOa.productPromoId === newQuoteItem.productPromoId,
                        );
                        // get existing order items from the ui that are related to existingOrderAdjustments
                        const existingQuoteItems = quoteItemsFromUi.filter((oi: any) =>
                            existingQuoteAdjustments.find(
                                (uiOa: any) =>
                                    uiOa.quoteId === oi.quoteId &&
                                    uiOa.quoteItemSeqId === oi.quoteItemSeqId,
                            ),
                        );

                        if (existingQuoteItems !== undefined) {
                            // before deleting, delete the order item adjustments

                            dispatch(deletePromoQuoteAdjustments(existingQuoteAdjustments));

                            dispatch(deletePromoQuoteItem(existingQuoteItems));
                        }
                    }
                    // get quoteItemSeq from the newQuoteItem
                    let quoteItemSeqId = parseInt(newQuoteItem.quoteItemSeqId) + 1;
                    // before adding the new order item and order item adjustments
                    // loop through the order items in the promo - result.data.quoteItems
                    // and results.data.quoteItemAdjustments
                    // and assign the new quoteItemSeqId to the order items and order item adjustments
                    // then add the new order item and order item adjustments
                    // then update the order items and order item adjustments in the ui
                    const quoteItems = result.data?.quoteItems;
                    const quoteItemAdjustments = result.data?.quoteItemAdjustments;
                    // loop through quoteItems
                    quoteItems?.forEach((oi: any) => {
                        // assign the new quoteItemSeqId to the order items
                        const newPromoQuoteItem = {
                            ...oi,
                            quoteItemSeqId: quoteItemSeqId.toString().padStart(2, "0"),
                        };
                        // link newPromotionQuoteItem to the newQuoteItem
                        newPromoQuoteItem.parentQuoteItemSeqId =
                            newQuoteItem.quoteItemSeqId;
                        // add the new quote item
                        promoQuoteItems.push(newPromoQuoteItem);
                        // loop through quoteItemAdjustments
                        quoteItemAdjustments?.forEach((oia: any) => {
                            const newPromoQuoteItemAdjustment = {
                                ...oia,
                                quoteItemSeqId: quoteItemSeqId.toString().padStart(2, "0"),
                            };
                            // add the new quote item adjustments
                            promoQuoteAdjustments.push(newPromoQuoteItemAdjustment);
                        });
                        // increment the quoteItemSeqId
                        quoteItemSeqId++;
                    });
                } else {
                    promoResult = "Failed";
                    toast.error(result!.data?.resultMessage);
                }
            }
        }

        return {promoQuoteItems, promoQuoteAdjustments, promoResult};
    }

    function toggleOItemState() {
        if (oItem === undefined) {
            setOItem({});
        } else {
            setOItem(undefined);
        }
    }

    function handleError(error: any) {
        console.log(error);
        // Additional error handling logic
    }

    return {
        //quoteItemProduct,
        productPromotionsWithEmpty,
        oItem,
        setOItem,
        handleSubmitData,
        productPromotions,
    };
}
