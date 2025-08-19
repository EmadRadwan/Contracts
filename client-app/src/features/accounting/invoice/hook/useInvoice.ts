import React, {useEffect, useState} from "react";
import { Invoice } from "../../../../app/models/accounting/invoice";
import {
  useCreateInvoiceMutation, useFetchInvoiceByIdQuery,
  useUpdateInvoiceMutation,
} from "../../../../app/store/apis/invoice/invoicesApi";
import { useAppDispatch } from "../../../../app/store/configureStore";
import { toast } from "react-toastify";
import { useNavigate } from "react-router";
import { setSelectedInvoice } from "../../slice/accountingSharedUiSlice";

type UseInvoiceProps = {
  invoiceId?: string;
};

const useInvoice = (invoiceId: UseInvoiceProps) => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const [
    updateInvoice,
    { data: invoiceUpdate, isLoading: isInvoiceUpdateLoading, error: invoiceUpdateError },
  ] = useUpdateInvoiceMutation();
  const [
    createAnInvoice,
    {
      data: invoiceCreate,
      error: invoiceCreateError,
      isLoading: isInvoiceCreateLoading,
    },
  ] = useCreateInvoiceMutation();


  const [invoice, setInvoice] = useState<Invoice | undefined>(undefined);
console.log("useInvoice hook initialized with invoiceId:", invoiceId);
  // Purpose: Retrieves invoice for display in InvoiceDisplayForm
  // Improvement: Ensures data is fetched based on invoiceId from route
  const {
    data: fetchedInvoice,
    isLoading: isInvoiceFetchLoading,
    error: invoiceFetchError,
  } = useFetchInvoiceByIdQuery(invoiceId, { skip: !invoiceId });

  // Purpose: Updates local invoice state with fetched data
  // Improvement: Ensures InvoiceDisplayForm receives correct invoice
  useEffect(() => {
    if (fetchedInvoice) {
      setInvoice(fetchedInvoice);
      dispatch(setSelectedInvoice(fetchedInvoice));
    }
  }, [fetchedInvoice, dispatch]);
  
  async function createStandaloneInvoice(values: any) {
    const isSalesInvoiceType =
        values.invoiceTypeId === "SALES_INVOICE" ||
        values.parentTypeId === "SALES_INVOICE" ||
        ["INTEREST_INVOICE", "PURC_RTN_INVOICE"].includes(values.invoiceTypeId);
    const isPurchaseInvoiceType =
        values.invoiceTypeId === "PURCHASE_INVOICE" ||
        values.parentTypeId === "PURCHASE_INVOICE" ||
        ["COMMISSION_INVOICE", "CUST_RTN_INVOICE", "PAYROL_INVOICE"].includes(values.invoiceTypeId);
    const isGenericInvoice = values.invoiceTypeId === "INVOICE";
    const isTemplate = values.invoiceTypeId === "PUR_INV_TEMPLATE";

    if (isTemplate) {
      throw new Error("Cannot create an invoice from a template (PUR_INV_TEMPLATE).");
    }

    if (values.invoiceTypeId === "PURC_RTN_INVOICE" && values.partyId === values.organizationPartyId) {
      throw new Error("For purchase return invoices, partyId must differ from organizationPartyId.");
    }

    let partyId: string;
    let partyIdFrom: string;

    if (isSalesInvoiceType) {
      partyIdFrom = values.organizationPartyId;
      partyId = values.partyId?.fromPartyId || values.partyId || values.organizationPartyId;
    } else if (isPurchaseInvoiceType) {
      partyId = values.organizationPartyId;
      partyIdFrom = values.partyIdFrom?.fromPartyId || values.partyId || values.organizationPartyId;
    } else if (isGenericInvoice) {
      partyId = values.partyId?.fromPartyId || values.partyId || values.organizationPartyId;
      partyIdFrom = values.partyIdFrom?.fromPartyId || values.organizationPartyId;
    } else {
      throw new Error(`Unsupported invoice type: ${values.invoiceTypeId}`);
    }

    if (!partyId || !partyIdFrom || !values.invoiceTypeId) {
      throw new Error("Missing required fields: invoiceTypeId, partyId, or partyIdFrom.");
    }

    const invoiceData: Invoice = {
      invoiceTypeId: values.invoiceTypeId,
      partyId,
      partyIdFrom,
      statusId: "INVOICE_IN_PROCESS",
      currencyUomId: values.currencyUomId || "EGP",
      invoiceDate: new Date().toISOString(),
      createdStamp: new Date().toISOString(),
    };

    try {
      const newInvoice = await createAnInvoice(invoiceData).unwrap();
      setInvoice(newInvoice);
      dispatch(setSelectedInvoice(newInvoice));
      toast.success("Invoice created successfully");
      
      // Purpose: Shows invoice details after creation
      // Improvement: Better UX by displaying result before editing
      navigate(`/invoices/${newInvoice.invoiceId}`);
    } catch (e) {
      console.error(e);
      toast.error("Something went wrong");
      throw e; // Re-throw to allow caller to handle
    }
  }

  async function handleCreate(data: any) {
    await createStandaloneInvoice(data);
  }

  async function handleUpdate(data: any) {
    if (!invoiceId) {
      throw new Error("No invoice ID provided for update");
    }

    try {
      const updatedInvoice = await updateInvoice({ invoiceId, ...data }).unwrap();
      setInvoice(updatedInvoice);
      dispatch(setSelectedInvoice(updatedInvoice));
      toast.success("Invoice updated successfully");
      // Purpose: Shows updated invoice details
      // Improvement: Consistent navigation flow
      navigate(`/invoices/${invoiceId}`);
    } catch (e) {
      console.error(e);
      toast.error("Something went wrong");
      throw e;
    }
  }


  return {
    invoice,
    setInvoice,
    handleCreate,
    handleUpdate,
    isLoading: isInvoiceCreateLoading || isInvoiceUpdateLoading,
  };
};
export default useInvoice;
