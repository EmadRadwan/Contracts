import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import {store} from "../configureStore";
// ProcessOrderItemResult can be used with Quotes and Orders
import {QuotePromoResult} from "../../models/order/QuotePromoResult";
import {
    generateNewPromoQuoteItem,
    generateNewPromoQuoteItemAdjustment,
    getDiscountAdjustments,
    getExistingPromoQuoteAdjustments,
    getExistingPromoQuoteItems
} from "../../../features/orders/utility functions/productPromotionsForQuotes";
import {QuoteTaxResult} from "../../models/order/QuoteTaxResult";
import {getExistingTaxAdjustments} from "../../../features/orders/utility functions/quoteTax";
import {CalculateItem} from "../../../features/orders/utility functions/calculateAdjustmentsForQuote";
import {QuoteItem} from "../../models/order/quoteItem";
import {QuoteAdjustment} from "../../models/order/quoteAdjustment";
import {ProcessQuoteItemResult} from "../../models/order/ProcessQuoteItemResult";
import {setUiQuoteAdjustments} from "../../../features/orders/slice/quoteAdjustmentsUiSlice";
import {setProcessedQuoteItems} from "../../../features/orders/slice/quoteItemsUiSlice";

interface ApplyQuoteItemPromoResult {
    data: QuotePromoResult;
    error: any;
}

interface ApplyQuoteItemTaxResult {
    data: QuoteTaxResult;
    error: any;
}

// External function to apply promotions
async function applyPromotions(quoteItem: QuoteItem, fetchWithBQ: any): Promise<ApplyQuoteItemPromoResult> {
    return await fetchWithBQ({
        url: '/products/applyQuoteItemPromo',
        method: 'POST',
        body: {...quoteItem},
    });
}

// External function to calculate tax adjustments
async function calculateTaxAdjustments(adjustedQuoteItems: QuoteItem[], fetchWithBQ: any): Promise<ApplyQuoteItemTaxResult> {
    return await fetchWithBQ({
        url: '/taxes/calculateTaxAdjustments',
        method: 'POST',
        body: {quoteItems: adjustedQuoteItems},
    });
}

// External function to handle promo results
function handlePromoResults(
    promoResult: ApplyQuoteItemPromoResult,
    tempQuoteItems: QuoteItem[],
    tempQuoteItemAdjustments: QuoteAdjustment[]
) {
    if (promoResult.data && promoResult.data.quoteItems) {
        let quoteItemSeqId = parseInt(tempQuoteItems[tempQuoteItems.length - 1].quoteItemSeqId) + 1;
        promoResult.data.quoteItems.forEach((oi: any) => {
            tempQuoteItems.push(generateNewPromoQuoteItem(oi, quoteItemSeqId, tempQuoteItems[tempQuoteItems.length - 1]));
            promoResult.data.quoteItemAdjustments?.forEach((oia: any) => {
                tempQuoteItemAdjustments.push(generateNewPromoQuoteItemAdjustment(oia, quoteItemSeqId));
            });
            quoteItemSeqId++;
        });
    }
}


const processQuoteItemApi = createApi({
    reducerPath: "processQuoteItem",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers, {getState}) => {
            // By default, if we have a token in the store, let's use that for authenticated requests
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),
    endpoints: (build) => ({
        processQuoteItem: build.query<ProcessQuoteItemResult, QuoteItem | undefined>({
            async queryFn(quoteItem, _queryApi, _extraOptions, fetchWithBQ) {
                let tempQuoteItems: QuoteItem[] = [];
                let existingPromoQuoteItems: QuoteItem[] = [];
                let tempQuoteItemAdjustments: QuoteAdjustment[] = [];
                let existingTaxAdjustments: QuoteAdjustment[] = [];
                let existingPromoQuoteItemAdjustments: QuoteAdjustment[] = [];
                let promoResult: ApplyQuoteItemPromoResult;
                let taxResult: ApplyQuoteItemTaxResult;
                let processQuoteItemResult: ProcessQuoteItemResult = {status: 'Success'};
                let customError = undefined;

                console.log("ProcessQuoteItem Rendered");
                // get all available adjustments from the state
                const allAvailableAdjustments: QuoteAdjustment[] = Object.values(_queryApi.getState().quoteAdjustmentsUi.quoteAdjustments.entities);
                //console.log(allAvailableAdjustments);

                // get the quote item from the state
                const allAvailableQuoteItems: QuoteItem[] = Object.values(_queryApi.getState().quoteItemsUi.quoteItems.entities);
                //console.log(allAvailableQuoteItems);


                if (quoteItem) {
                    tempQuoteItems.push(quoteItem);
                }

                console.log('quoteItem from processQuoteItem', quoteItem);


                // check if we need to run the promo api
                if (quoteItem && typeof quoteItem.productPromoId === 'string' && quoteItem.productPromoId!.trim() !== '') {
                    promoResult = await applyPromotions(quoteItem, fetchWithBQ);
                    if (promoResult.data.resultMessage !== 'Success') {
                        customError = promoResult.data.resultMessage;
                        processQuoteItemResult.status = 'Failed';
                        return {data: processQuoteItemResult, error: customError};
                    } else {
                        handlePromoResults(promoResult, tempQuoteItems, tempQuoteItemAdjustments);

                        // get existing promo quote items
                        try {
                            if (allAvailableQuoteItems && allAvailableQuoteItems.length > 0) {
                                existingPromoQuoteItems = getExistingPromoQuoteItems(
                                    allAvailableQuoteItems,
                                    quoteItem
                                );
                            }
                        } catch (error) {
                            console.log(error);
                        }


                        //console.log('allAvailableAdjustments', allAvailableAdjustments);
                        try {

                            if (
                                allAvailableAdjustments && allAvailableAdjustments.length > 0
                            ) {
                                existingPromoQuoteItemAdjustments =
                                    getExistingPromoQuoteAdjustments(
                                        allAvailableAdjustments,
                                        quoteItem
                                    );
                            }
                        } catch (error) {
                            console.log(error);
                        }
                    }

                }

                // check for discount adjustments available in the state in an ADD phase
                if (allAvailableAdjustments && allAvailableAdjustments.length > 0) {
                    const discountAdjustments = getDiscountAdjustments(allAvailableAdjustments, quoteItem!.quoteItemSeqId);

                    if (discountAdjustments.length > 0) {
                        tempQuoteItemAdjustments.push(...discountAdjustments);
                    }
                }


                // calculate the adjusted quote items
                const adjustedQuoteItems = CalculateItem(tempQuoteItems, tempQuoteItemAdjustments);
                console.log(adjustedQuoteItems);

                // check if we'll need to call the tax API
                if (quoteItem!.collectTax) {
                    // call the tax API
                    taxResult = await calculateTaxAdjustments(adjustedQuoteItems, fetchWithBQ);
                    if (taxResult.data) {
                        tempQuoteItemAdjustments.push(...taxResult.data);
                    }

                    // get existing tax adjustments
                    if (allAvailableAdjustments && allAvailableAdjustments.length > 0) {
                        existingTaxAdjustments = getExistingTaxAdjustments(tempQuoteItems, allAvailableAdjustments);
                    }


                }

                // if we have existing tax adjustments, add them to the tempQuoteItemAdjustments
                if (existingTaxAdjustments.length > 0) {
                    tempQuoteItemAdjustments.push(...existingTaxAdjustments);
                }

                // if we have existing promo quote items, add them to the tempQuoteItems
                if (existingPromoQuoteItems.length > 0) {
                    adjustedQuoteItems.push(...existingPromoQuoteItems);
                }

                // if we have existing promo quote item adjustments, add them to the tempQuoteItemAdjustments
                if (existingPromoQuoteItemAdjustments.length > 0) {
                    tempQuoteItemAdjustments.push(...existingPromoQuoteItemAdjustments);
                }

                // update the quote items in the state
                _queryApi.dispatch(setProcessedQuoteItems(adjustedQuoteItems));

                // update the quote adjustments in the state
                _queryApi.dispatch(setUiQuoteAdjustments(tempQuoteItemAdjustments));

                processQuoteItemResult.status = 'Success';
                return {data: processQuoteItemResult, error: customError};
            },
        }),
    }),
});

export const {
    useProcessQuoteItemQuery,
} = processQuoteItemApi;
export {processQuoteItemApi};
