import {useEffect, useMemo, useRef, useState} from "react";
import { toast } from "react-toastify";
import { Payment } from "../../../../app/models/accounting/payment";
import {
  useCreatePaymentAndFinAccountTransMutation,
  useSetPaymentStatusToReceivedMutation,
  useUpdatePaymentMutation,
} from "../../../../app/store/apis";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import { acctTransApi } from "../../../../app/store/apis";
import {
  setFormEditMode,
} from "../slice/paymentsUiSlice";
import {setSelectedPayment} from "../../slice/accountingSharedUiSlice";

interface Company {
  organizationPartyId: string;
  organizationPartyName: string;
}

interface UsePaymentProps {
  editMode: number;              
  selectedPayment?: Payment;  
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
  companies: { organizationPartyId: string; organizationPartyName: string }[];
}

const PAYMENT_STATUSES = {
  NOT_PAID: "PMNT_NOT_PAID",
  RECEIVED: "PMNT_RECEIVED",
  SENT: "PMNT_SENT",
  CANCELLED: "PMNT_CANCELLED",
  CONFIRMED: "PMNT_CONFIRMED",
};

const statusToEditMode: Record<string, number> = {
  [PAYMENT_STATUSES.NOT_PAID]: 2,
  [PAYMENT_STATUSES.RECEIVED]: 3,
  [PAYMENT_STATUSES.SENT]: 4,
  [PAYMENT_STATUSES.CONFIRMED]: 5,
  [PAYMENT_STATUSES.CANCELLED]: 6,
};

// REFACTOR: Map backend error codes to user-friendly messages
const errorMessages: Record<string, Record<string, string>> = {
  en: {
    PAYMENT_NOT_FOUND: "The specified payment could not be found.",
    INVALID_STATUS: "The selected status is invalid.",
    INVALID_STATUS_TRANSITION: "This status change is not allowed.",
    MISSING_PAYMENT_METHOD: "A payment method is required for this status.",
    PAYMENT_NOT_APPLIED: "The payment must be fully applied before confirming.",
    INVOICE_CHECK_FAILED: "Invoice validation failed. Please check the invoices.",
    ECA_LOGIC_FAILED: "An error occurred while processing accounting transactions.",
    STATUS_DESCRIPTION_NOT_FOUND: "The status description could not be found.",
    UNEXPECTED_ERROR: "An unexpected error occurred. Please try again.",
    DEFAULT: "An unexpected error occurred. Please try again.",
  },
  ar: {
    PAYMENT_NOT_FOUND: "الدفع المحدد غير موجود.",
    INVALID_STATUS: "الحالة المختارة غير صالحة.",
    INVALID_STATUS_TRANSITION: "تغيير الحالة هذا غير مسموح به.",
    MISSING_PAYMENT_METHOD: "طريقة الدفع مطلوبة لهذه الحالة.",
    PAYMENT_NOT_APPLIED: "يجب تطبيق الدفع بالكامل قبل التأكيد.",
    INVOICE_CHECK_FAILED: "فشل التحقق من الفواتير. يرجى التحقق من الفواتير.",
    ECA_LOGIC_FAILED: "حدث خطأ أثناء معالجة المعاملات المحاسبية.",
    STATUS_DESCRIPTION_NOT_FOUND: "وصف الحالة غير موجود.",
    UNEXPECTED_ERROR: "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.",
    DEFAULT: "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.",
  },
};


/**
 * Custom hook to manage payment operations (create, update, status change) for payment forms.
 * @param props - Configuration for the payment hook.
 * @returns Object containing state and handlers for payment operations.
 */
export default function usePayment({
                                     editMode,
                                     selectedPayment: propSelectedPayment,
                                     setIsLoading,
                                     companies,
                                   }: UsePaymentProps) {
  const dispatch = useAppDispatch();
  const language = useAppSelector((state) => state.localization.language || "en"); // REFACTOR: Get language from Redux store
  const reduxSelectedPayment = useAppSelector(
      (s) => s.accountingSharedUi.selectedPayment
  );
  const [payment, setPayment] = useState<Payment | undefined>(undefined);

  const [createPaymentAndFinAccountTrans, { isLoading: isCreateLoading }] =
      useCreatePaymentAndFinAccountTransMutation();
  const [updatePayment, { isLoading: isUpdateLoading }] = useUpdatePaymentMutation();
  const [setPaymentStatus, { isLoading: isStatusLoading }] =
      useSetPaymentStatusToReceivedMutation();

  const isLoading = isCreateLoading || isUpdateLoading || isStatusLoading;

  
  const defaultNewPayment = useMemo(() => ({
    paymentId: "",
    paymentTypeId: "",
    paymentMethodId: "",
    statusId: PAYMENT_STATUSES.NOT_PAID,
    statusDescription: "Not Paid",
    partyIdFrom: "",
    partyIdFromName: "",
    partyIdTo: "",
    partyIdToName: "",
    amount: 0,
    effectiveDate: new Date().toISOString(),
    paymentRefNum: "",
    organizationPartyId:  "",
    isDepositWithDrawPayment: "Y",
    finAccountTransTypeId: "",
    isDisbursement: false,
    chequeNumber: "",
    chequeDate: null,
    comments: "",
    overrideGlAccountId: undefined,
    projectId: null,
    costCenterId: null,
  }), [companies]);

  // Replace the entire useEffect with this:
  useEffect(() => {
    // 1. Redux has a new selected payment → use it instantly
    if (reduxSelectedPayment) {
      setPayment(reduxSelectedPayment);
      return;
    }

    // 2. We are in CREATE mode (editMode === 1) and we have no payment yet
    if (editMode === 1 && !payment?.paymentId) {
      setPayment(defaultNewPayment);
    }
  }, [reduxSelectedPayment, editMode, defaultNewPayment]);
  
  const finalizePayment = (
      updated: Partial<Payment>,
      newStatusId?: string
  ) => {
    const full = { ...payment, ...updated } as Payment;
    dispatch(setSelectedPayment(full));
    if (newStatusId) {
      const mode = statusToEditMode[newStatusId] ?? 2;
      dispatch(setFormEditMode(mode));
    }
    setPayment(full);
  };
  
  

  /**
   * Handles API errors by displaying a toast message based on error code and logging the error.
   */
  const handleApiError = (error: any, defaultMessage: string) => {
    // REFACTOR: Select localized message based on language from Redux store
    const errorCode = error?.data?.errorCode || "DEFAULT";
    const errorMessage = error?.data?.title || defaultMessage;
    const localizedMessage = errorMessages[language][errorCode] || errorMessage || defaultMessage;
    toast.error(localizedMessage);
    console.error(error);
    setIsLoading(false);
  };

  
  const createPayment = async (
      newPayment: Payment
  ) => {
    try {
      setIsLoading(true);
      const request = {
        paymentMethodId: newPayment.paymentMethodId,
        isDepositWithDrawPayment: newPayment.isDepositWithDrawPayment,
        finAccountTransTypeId: newPayment.isDisbursement ? "WITHDRAWAL" : "DEPOSIT",
        paymentTypeId: newPayment.paymentTypeId,
        partyIdFrom: newPayment.partyIdFrom,
        partyIdTo: newPayment.partyIdTo,
        amount: newPayment.amount,
        statusId: PAYMENT_STATUSES.NOT_PAID,
        paymentDate: newPayment.effectiveDate || new Date().toISOString(),
        chequeNumber: newPayment.chequeNumber,
        chequeDate: newPayment.chequeDate,
        comments: newPayment.comments || "",
        overrideGlAccountId: newPayment.overrideGlAccountId,
        projectId: newPayment.projectId || null,
        costCenterId: newPayment.costCenterId || null,
        paymentRefNum: newPayment.paymentRefNum || "",
      };

      const created = await createPaymentAndFinAccountTrans(request).unwrap();

      const merged: Payment = {
        ...newPayment,
        paymentId: created.paymentId,
        statusId: PAYMENT_STATUSES.NOT_PAID,
        statusDescription: "Not Paid",
        currencyUomId: created.currencyUomId ?? "EGP",
        finAccountTransId: created.finAccountTransId,
        partyIdFromName : created.partyIdFromName,
        partyIdToName : created.partyIdToName
      };

      finalizePayment(merged, PAYMENT_STATUSES.NOT_PAID);
      toast.success("Payment created");
      dispatch(acctTransApi.util.invalidateTags(["Payments", "PTransactions"]));

    } catch (error) {
      handleApiError(error, "Failed to create payment");
    } finally {
      setIsLoading(false);
    }
  };

  
  const updatePaymentData = async (
      updated: Payment
  ) => {
    try {
      setIsLoading(true);
      const result = await updatePayment(updated).unwrap();
      finalizePayment(result);
      toast.success("Payment updated");
      dispatch(acctTransApi.util.invalidateTags(["Payments"]));
    } catch (error) {
      handleApiError(error, "Failed to update payment");
      throw error;
    } finally {
      setIsLoading(false);
    }
  };


  const changeStatus = async (paymentId: string, statusId: string) => {
    try {
      setIsLoading(true);
      const result = await setPaymentStatus({ paymentId, statusId }).unwrap();
      finalizePayment(result, statusId);
      toast.success(`Status → ${result.statusDescription}`);
      dispatch(acctTransApi.util.invalidateTags(["Payments", "PTransactions"]));
    } catch (error) {
      handleApiError(error, `Failed to change payment status to ${statusId}`);
    } finally {
    setIsLoading(false);
  }
  };

const handleCreate = async (data: {
  values: any;
  menuItem: string;
}) => {

  const { values } = data;
  const isDisbursement = values.isDisbursement ?? false;
  const customerId = isDisbursement ? values.partyIdTo?.fromPartyId : values.partyIdFrom?.fromPartyId;
  const organizationId = values.organizationPartyId;

  const org = companies.find((c) => c.organizationPartyId === organizationId);
  const orgName = org?.organizationPartyName ?? "";

  const newPayment: Payment = {
    paymentId: "",
    paymentTypeId: values.paymentTypeId,
    paymentMethodId: values.paymentMethodId,
    statusId: PAYMENT_STATUSES.NOT_PAID,
    partyIdFrom: isDisbursement ? organizationId : customerId,
    partyIdFromName: isDisbursement ? orgName : values.partyIdFrom?.fromPartyName ?? "",
    partyIdTo: isDisbursement ? customerId : organizationId,
    partyIdToName: isDisbursement ? values.partyIdTo?.fromPartyName ?? "" : orgName,
    amount: values.amount,
    effectiveDate: values.effectiveDate ?? new Date().toISOString(),
    comments: values.comments ?? "",
    organizationPartyId: organizationId,
    isDepositWithDrawPayment: values.isDepositWithDrawPayment ? "Y" : "N",
    finAccountTransTypeId: isDisbursement ? "WITHDRAWAL" : "DEPOSIT",
    isDisbursement,
    chequeNumber: values.chequeNumber ?? "",
    chequeDate: values.chequeDate ? new Date(values.chequeDate).toISOString() : null,
    overrideGlAccountId: values.overrideGlAccountId,
    projectId: values.projectId?.projectId || null,
    projectName: values.projectId?.projectName || null,
    costCenterId: values.costCenterId || null,
    paymentRefNum: values.paymentRefNum || "",
  };

  await createPayment(newPayment);
};

  const handleUpdate = async (data: {
    values: any;
    menuItem: string;
  }) => {

    const updated: Payment = {
      ...payment,
      paymentMethodId: data.values.paymentMethodId,
      amount: data.values.amount ?? payment.amount,
      effectiveDate:
          data.values.effectiveDate && !isNaN(new Date(data.values.effectiveDate).getTime())
              ? new Date(data.values.effectiveDate).toISOString()
              : payment.effectiveDate,
      comments: data.values.comments ?? payment.comments ?? "",
      chequeNumber: data.values.chequeNumber ?? payment.chequeNumber ?? "",
      chequeDate: data.values.chequeDate
          ? new Date(data.values.chequeDate).toISOString()
          : payment.chequeDate ?? null,
      overrideGlAccountId: data.values.overrideGlAccountId,
        projectId: data.values.projectId?.projectId,
        costCenterId: data.values.costCenterId || null,
      paymentRefNum: data.values.paymentRefNum || payment.paymentRefNum || "",
    };

    await updatePaymentData(updated);
  };

  const handleStatusChange = async (data: {
    menuItem: string;
  }) => {

    const map: Record<string, string> = {
      "Status to Received": PAYMENT_STATUSES.RECEIVED,
      "Status to Sent": PAYMENT_STATUSES.SENT,
      "Status to Cancelled": PAYMENT_STATUSES.CANCELLED,
      "Status to Confirmed": PAYMENT_STATUSES.CONFIRMED,
    };
    const target = map[data.menuItem];
    if (!target) return toast.error("Unknown status");

    await changeStatus(payment.paymentId, target);
  };

  return {
    payment,
    handleCreate,
    handleUpdate,
    handleStatusChange,
    isLoading
  };
}