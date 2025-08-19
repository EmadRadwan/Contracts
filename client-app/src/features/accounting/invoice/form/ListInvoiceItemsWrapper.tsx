import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAppDispatch, useAppSelector, useFetchInvoiceByIdQuery } from "../../../../app/store/configureStore";
import { setSelectedInvoice } from "../../slice/accountingSharedUiSlice";
import InvoiceDisplayForm from "./InvoiceDisplayForm"; // Adjust path as needed
import LoadingComponent from "../../../../app/layout/LoadingComponent";

const ListInvoiceItemsWrapper: React.FC = () => {
    // Purpose: Directly renders InvoiceDisplayForm with InvoiceItemsList, allowing customization of editMode and status
    // Improvement: Consolidates logic in a single wrapper, enabling active "Add Invoice Item" button and supporting empty invoices
    const { invoiceId } = useParams<{ invoiceId: string }>();
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const { selectedInvoice } = useAppSelector((state) => state.accountingSharedUi);
    const [editMode, setEditMode] = useState(0);
    const { data: invoice, isLoading } = useFetchInvoiceByIdQuery(invoiceId!, {
        skip: !invoiceId,
    });

    // Purpose: Replicates InvoiceDisplayFormWrapper logic to determine editMode based on invoice status
    // Improvement: Allows customization in InvoiceDisplayForm while maintaining consistent state
    useEffect(() => {
        if (invoice) {
            dispatch(setSelectedInvoice(invoice));
            switch (invoice.statusDescription) {
                case "In-Process":
                    setEditMode(2);
                    break;
                case "Approved":
                    setEditMode(3);
                    break;
                case "Ready for Posting":
                    setEditMode(4);
                    break;
                case "Received":
                    setEditMode(5);
                    break;
                case "Paid":
                    setEditMode(6);
                    break;
                case "Cancelled":
                    setEditMode(7);
                    break;
                default:
                    setEditMode(0); // Default for new or undefined status
            }
        }
    }, [invoice, dispatch]);

    // Purpose: Prevents stale invoice data in Redux store
    // Improvement: Ensures clean state transitions when navigating away
    useEffect(() => {
        return () => {
            dispatch(setSelectedInvoice(undefined));
        };
    }, [dispatch]);

    if (isLoading) {
        return <LoadingComponent message="Loading Invoice..." />;
    }

    if (!invoice) {
        navigate("/invoices");
        return null;
    }

    return (
        <InvoiceDisplayForm
            editMode={editMode}
            cancelEdit={() => {
                setEditMode(0);
                dispatch(setSelectedInvoice(undefined));
                navigate("/invoices");
            }}
        />
    );
};

export default ListInvoiceItemsWrapper;