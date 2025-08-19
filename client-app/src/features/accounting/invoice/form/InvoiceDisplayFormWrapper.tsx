import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {useAppDispatch, useAppSelector, useFetchInvoiceByIdQuery} from "../../../../app/store/configureStore";
import { setSelectedInvoice } from "../../slice/accountingSharedUiSlice";
import InvoiceDisplayForm from "./InvoiceDisplayForm";
import LoadingComponent from "../../../../app/layout/LoadingComponent";

function InvoiceDisplayFormWrapper() {
    const { invoiceId } = useParams<{ invoiceId: string }>();
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const { selectedInvoice } = useAppSelector((state) => state.accountingSharedUi);
    const [editMode, setEditMode] = useState(0);
    const { data: invoice, isLoading } = useFetchInvoiceByIdQuery(invoiceId!, {
        skip: !invoiceId,
    });

    // Refactored: Set editMode and selectedInvoice based on fetched invoice
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
                    setEditMode(0);
            }
        }
    }, [invoice, dispatch]);

    // Refactored: Cleanup selectedInvoice on unmount
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
    
    console.log('invoice', invoice);

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
}

export default InvoiceDisplayFormWrapper;