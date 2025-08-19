import React, {useEffect, useState} from "react";
import {useSelector} from "react-redux";

import {toast} from "react-toastify";


import {Quote} from "../../../app/models/order/quote";
import {useNavigate} from "react-router";
import {
    useAddOrderFromQuoteMutation,
    useAppDispatch,
    useCreateQuoteMutation,
    useUpdateQuoteMutation
} from "../../../app/store/configureStore";
import {quoteAdjustmentsEntities} from "../../orders/slice/quoteAdjustmentsUiSlice";
import {quoteItemsEntities} from "../../orders/slice/quoteItemsUiSlice";
import {
    quoteLevelAdjustmentsTotal,
    quoteSubTotal,
    selectAdjustedQuoteItemsWithMarkedForDeletionItems
} from "../../orders/slice/quoteSelectors";
import {setQuoteFormEditMode} from "../../orders/slice/quotesUiSlice";
import { allItemsAreDeletedOrNone } from "../../orders/slice/quoteSelectors";

type UseQuoteProps = {
    selectedMenuItem: string;
    formRef2: any;
    editMode: number;
    selectedQuote: Quote;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
};
const useQuote = ({
                      selectedMenuItem,
                      formRef2,
                      editMode,
                      selectedQuote,
                      setIsLoading,
                  }: UseQuoteProps) => {
    const [
        addQuoteTrigger,
        {data: quoteResults, error, isLoading: isAddQuoteLoading},
    ] = useCreateQuoteMutation();
    const [
        addOrderFromQuoteTrigger,
        // {
        //     data: orderFromQuoteResults,
        //     error: orderFromQuoteError,
        //     isLoading: isAddOrderFromQuoteLoading,
        // },
    ] = useAddOrderFromQuoteMutation();
    const [
        updateQuoteTrigger,
        {data: quoteResults2, error: error2, isLoading: isUpdateQuoteLoading},
    ] = useUpdateQuoteMutation();

    const [formEditMode, setFormEditMode] = useState(editMode);
    const [quote, setQuote] = useState<Quote | undefined>(selectedQuote);
    useEffect(() => {
        console.log("quote", quote)
        console.log("selectedQuote", selectedQuote)
    }, [quote, selectedQuote])
    const quoteAdjustmentsFromUi: any = useSelector(quoteAdjustmentsEntities);
    const quoteItemsFromUi: any = useSelector(quoteItemsEntities);
    const sTotal: any = useSelector(quoteSubTotal);
    const aTotal: any = useSelector(quoteLevelAdjustmentsTotal);
    const dispatch = useAppDispatch();
    const adjustedQuoteItemsWithMarkedForDeletionItems = useSelector(
        selectAdjustedQuoteItemsWithMarkedForDeletionItems,
    );
    const allItemsAreDeletedOrNot: any = useSelector(allItemsAreDeletedOrNone);
    const quoteItemFlat = quoteItemsFromUi.map((item: any) => {
        if (typeof item.productId === "object") {
            return {...item, productId: item.productId.productId}
        } else {
            return item
        }
    });

    const updatedQuoteItemFlat = adjustedQuoteItemsWithMarkedForDeletionItems.map((item: any) => {
        if (typeof item.productId === "object") {
            return {...item, productId: item.productId.productId}
        } else {
            return item
        }
    });
    const navigate = useNavigate();

    async function createQuote(newQuote: Quote, customer: any, vehicle?: any) {
        console.log(newQuote)
        try {
            let createdQuote: any;
            try {
                newQuote.quoteItems = quoteItemFlat;
                newQuote.quoteAdjustments = quoteAdjustmentsFromUi;
                createdQuote = await addQuoteTrigger(newQuote).unwrap();
            } catch (error) {
                toast.error("Failed to create quote");
            }

            if (createdQuote) {
                setQuote({
                    quoteId: createdQuote.quoteId,
                    fromPartyId: customer,
                    vehicleId: vehicle,
                    customerRemarks: createdQuote.customerRemarks,
                    internalRemarks: createdQuote.internalRemarks,
                    statusDescription: createdQuote.statusDescription,
                    currentMileage: createdQuote.currentMileage,
                });
                setFormEditMode(2);
                dispatch(setQuoteFormEditMode(2));
                formRef2.current = !formRef2.current;
                toast.success("Quote Created Successfully");
            }

        } catch (error: any) {
            console.log(error);
        }
        setIsLoading(false);
    }

    async function createOrder(newQuote: Quote, customer: any, vehicle: any) {
        try {
            let createdJobOrder: any;
            try {
                //todo: check if anything has changed in the quote and update before creating the job order
                newQuote.quoteItems = undefined;
                newQuote.quoteAdjustments = quoteAdjustmentsFromUi;
                createdJobOrder = await addOrderFromQuoteTrigger(newQuote).unwrap();
                createdJobOrder = {
                    ...createdJobOrder,
                    currentMileage: newQuote.currentMileage,
                    customerRemarks: newQuote.customerRemarks,
                    internalRemarks: newQuote.internalRemarks,
                    fromPartyId: customer,
                    vehicleId: vehicle,
                };
                setIsLoading(false);
                setFormEditMode(4);
                formRef2.current = !formRef2.current;
                toast.success("Order Job Created Successfully");

                navigate("/jobOrders", {state: {order: createdJobOrder}});
            } catch (error) {
                toast.error("Failed to create job order");
            }
        } catch (error: any) {
            setIsLoading(false);
            console.log(error);
            toast.error("Error in create job order");
        }
    }

    async function updateQuote(newQuote: Quote, customer: any, vehicle: any) {
        try {
            let updatedQuote: any;

            try {
                // update modification type flag in newQuote to 'Update'
                newQuote = {...newQuote, modificationType: "UPDATE"};
                newQuote.quoteItems = adjustedQuoteItemsWithMarkedForDeletionItems;
                newQuote.quoteAdjustments = quoteAdjustmentsFromUi;

                // check if productId in quoteItems is Object and if it is, then remove it
                // and replace it with productId.productId
                // this is to prevent error in the backend

                newQuote.quoteItems = updatedQuoteItemFlat;

                updatedQuote = await updateQuoteTrigger(newQuote).unwrap();
            } catch (error) {
                toast.error("Failed to update quote");
            }

            setQuote({
                quoteId: updatedQuote.quoteId,
                fromPartyId: customer,
                vehicleId: vehicle,
                customerRemarks: updatedQuote.customerRemarks,
                internalRemarks: updatedQuote.internalRemarks,
                statusDescription: updatedQuote.statusDescription,
                currentMileage: updatedQuote.currentMileage,
            });
            setIsLoading(false);
            formRef2.current = !formRef2.current;
            toast.success("Quote Updated Successfully");
        } catch (error: any) {
            console.log(error);
            toast.error("Error in update quote");
        }
    }

    async function handleCreate(data: any) {
        const customer = data.values.fromPartyId;

        const newQuote: Quote = {
            quoteId: formEditMode > 1 ? data.values.quoteId : "QUOTE-DUMMY",
            fromPartyId: data.values.fromPartyId.fromPartyId,
            fromPartyName: data.values.fromPartyId.fromPartyName,
            vehicleId: data.values.vehicleId?.vehicleId || null,
            customerRemarks: data.values.customerRemarks || null,
            internalRemarks: data.values.internalRemarks || null,
            currentMileage: data.values.currentMileage || null,
            grandTotal: sTotal + aTotal,
            currencyUomId: data.values.currencyUomId,
            agreementId: data.values.agreementId
        };

        // pre submit validation
        // check if quote items are all deleted, that's also considered as empty quote
        if (allItemsAreDeletedOrNot) {
            toast.error('Quote Items cannot be empty');
            setIsLoading(false);
            return;
        }

        if (selectedMenuItem === "Create Job Quote") {
            await createQuote(newQuote, customer, data.values.vehicleId);
        }

        if (selectedMenuItem === "Create Quote") {
            await createQuote(newQuote, customer);
        }

        if (selectedMenuItem === "Update Job Quote") {
            await updateQuote(newQuote, customer, data.values.vehicleId);
        }

        if (selectedMenuItem === "Create Job Order") {
            await createOrder(newQuote, customer, data.values.vehicleId);
        }
    }

    return {
        quoteResults,
        error,
        isAddQuoteLoading,
        isUpdateQuoteLoading,
        formEditMode,
        setFormEditMode,
        quote,
        setQuote,
        handleCreate,
    };
};
export default useQuote;
