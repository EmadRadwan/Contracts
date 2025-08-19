import { useState } from "react";
import { toast } from "react-toastify";
import { Payment } from "../../../../app/models/accounting/payment";
import { setPaymentFormEditMode } from "../slice/paymentsUiSlice";
import {
    useCreatePaymentAndFinAccountTransMutation,
    useSetPaymentStatusToReceivedMutation,
    useUpdatePaymentMutation,
} from "../../../../app/store/apis";
import { useAppDispatch, useAppSelector } from "../../../../app/store/configureStore"; // REFACTOR: Import useAppSelector for language
import { acctTransApi } from "../../../../app/store/apis";

interface Company {
    organizationPartyId: string;
    organizationPartyName: string;
}

interface UsePaymentProps {
    selectedMenuItem: string;
    editMode: number;
    selectedPayment?: Payment;
    formFieldsUpdated: boolean;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
    companies: Company[];
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

// REFACTOR: Add localized error messages for English and Arabic
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
                                       selectedMenuItem,
                                       editMode,
                                       selectedPayment,
                                       formFieldsUpdated,
                                       setIsLoading,
                                       companies,
                                   }: UsePaymentProps) {
    const dispatch = useAppDispatch();
    const language = useAppSelector((state) => state.localization.language || "en"); // REFACTOR: Get language from Redux store
    const [formEditMode, setFormEditMode] = useState(editMode);
    const [payment, setPayment] = useState<Payment | undefined>(() => {
        if (selectedPayment) {
            return selectedPayment;
        }
        return {
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
            effectiveDate: "",
            paymentRefNum: "",
            organizationPartyId: "",
            isDepositWithDrawPayment: "Y",
            finAccountTransTypeId: "",
            isDisbursement: false,
        };
    });

    const [createPaymentAndFinAccountTrans, { isLoading: isCreateLoading }] =
        useCreatePaymentAndFinAccountTransMutation();
    const [updatePayment, { isLoading: isUpdateLoading }] = useUpdatePaymentMutation();
    const [setPaymentStatus, { isLoading: isStatusLoading }] =
        useSetPaymentStatusToReceivedMutation();

    const isLoading = isCreateLoading || isUpdateLoading || isStatusLoading;

    /**
     * Updates the payment state with new data and optionally sets a new edit mode.
     */
    const updatePaymentState = (
        updatedPayment: Partial<Payment>,
        customerId: string,
        organizationId: string,
        newStatusId?: string
    ) => {
        setPayment({
            ...payment,
            paymentId: updatedPayment.paymentId ?? payment?.paymentId ?? "",
            paymentTypeId: updatedPayment.paymentTypeId ?? payment?.paymentTypeId ?? "",
            paymentMethodId: updatedPayment.paymentMethodId ?? payment?.paymentMethodId ?? "",
            statusId: newStatusId ?? updatedPayment.statusId ?? payment?.statusId ?? PAYMENT_STATUSES.NOT_PAID,
            statusDescription: updatedPayment.statusDescription ?? payment?.statusDescription ?? "Not Paid",
            partyIdFrom: updatedPayment.isDisbursement ? organizationId : customerId,
            partyIdFromName: updatedPayment.partyIdFromName ?? payment?.partyIdFromName ?? "",
            partyIdTo: updatedPayment.isDisbursement ? customerId : organizationId,
            partyIdToName: updatedPayment.partyIdToName ?? payment?.partyIdToName ?? "",
            amount: updatedPayment.amount ?? payment?.amount ?? 0,
            effectiveDate: updatedPayment.effectiveDate ?? payment?.effectiveDate ?? "",
            paymentRefNum: updatedPayment.paymentRefNum ?? payment?.paymentRefNum ?? "",
            organizationPartyId: organizationId,
            isDepositWithDrawPayment: updatedPayment.isDepositWithDrawPayment ?? payment?.isDepositWithDrawPayment ?? "Y",
            finAccountTransTypeId: updatedPayment.finAccountTransTypeId ?? payment?.finAccountTransTypeId ?? "",
            isDisbursement: updatedPayment.isDisbursement ?? payment?.isDisbursement ?? false,
            currencyUomId: updatedPayment.currencyUomId ?? payment?.currencyUomId ?? "",
            finAccountTransId: updatedPayment.finAccountTransId ?? payment?.finAccountTransId ?? "",
        });
        if (newStatusId && statusToEditMode[newStatusId]) {
            const newEditMode = statusToEditMode[newStatusId];
            setFormEditMode(newEditMode);
            dispatch(setPaymentFormEditMode(newEditMode));
        }
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

    /**
     * Creates a new payment and updates the state with the response.
     */
    const createPayment = async (
        newPayment: Payment,
        customerId: string,
        organizationId: string
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
            };

            const createdPayment = await createPaymentAndFinAccountTrans(request).unwrap();
            const updatedPayment = {
                ...newPayment,
                paymentId: createdPayment.paymentId,
                statusDescription: "Not Paid",
                currencyUomId: createdPayment.currencyUomId,
                finAccountTransId: createdPayment.finAccountTransId,
            };
            if (process.env.NODE_ENV !== "production") {
                console.log("updated payment", updatedPayment);
            }
            updatePaymentState(updatedPayment, customerId, organizationId, PAYMENT_STATUSES.NOT_PAID);
            toast.success(language === "ar" ? "تم إنشاء الدفع بنجاح" : "Payment Created Successfully");
            dispatch(acctTransApi.util.invalidateTags(["Payments", "PTransactions"]));
            setIsLoading(false);
        } catch (error) {
            handleApiError(error, language === "ar" ? "فشل في إنشاء الدفع" : "Failed to create payment");
        }
    };

    /**
     * Updates an existing payment and refreshes the state.
     */
    const updatePaymentData = async (
        newPayment: Payment,
        customerId: string,
        organizationId: string
    ) => {
        try {
            setIsLoading(true);
            const updatedPayment = await updatePayment(newPayment).unwrap();
            updatePaymentState(updatedPayment, customerId, organizationId);
            toast.success(language === "ar" ? "تم تحديث الدفع بنجاح" : "Payment Updated Successfully");
            dispatch(acctTransApi.util.invalidateTags(["Payments"]));
            setIsLoading(false);
        } catch (error) {
            handleApiError(error, language === "ar" ? "فشل في تحديث الدفع" : "Failed to update payment");
            throw error;
        }
    };

    /**
     * Changes the status of a payment and updates the state.
     */
    const changePaymentStatus = async (
        paymentId: string,
        statusId: string,
        customerId: string,
        organizationId: string
    ) => {
        try {
            setIsLoading(true);
            if (formFieldsUpdated && payment) {
                await updatePaymentData(payment, customerId, organizationId);
            }
            const updatedPayment = await setPaymentStatus({ paymentId, statusId }).unwrap();
            updatePaymentState(updatedPayment, customerId, organizationId, statusId);
            toast.success(
                language === "ar"
                    ? `تم تغيير حالة الدفع إلى ${updatedPayment.statusDescription} بنجاح`
                    : `Payment Status Changed to ${updatedPayment.statusDescription} Successfully`
            );
            dispatch(acctTransApi.util.invalidateTags(["Payments", "PTransactions"]));
            setIsLoading(false);
        } catch (error) {
            handleApiError(
                error,
                language === "ar"
                    ? `فشل في تغيير حالة الدفع إلى ${statusId}`
                    : `Failed to change payment status to ${statusId}`
            );
        }
    };

    /**
     * Handles payment creation.
     */
    const handleCreate = async (data: any) => {
        if (!data.isValid) {
            toast.error(language === "ar" ? "يرجى تصحيح أخطاء النموذج قبل الإرسال" : "Please fix form errors before submitting");
            setIsLoading(false);
            return;
        }

        if (process.env.NODE_ENV !== "production") {
            console.log("data", data);
        }

        const customerId = data.values.isDisbursement ? data.values.partyIdTo : data.values.partyIdFrom;
        const organizationId = data.values.organizationPartyId;

        const organization = companies.find(
            (company) => company.organizationPartyId === organizationId
        );
        const organizationName = organization?.organizationPartyName || "Unknown Organization";

        const partyIdFromName = data.values.isDisbursement
            ? organizationName
            : data.values.partyIdFrom?.fromPartyName || "";
        const partyIdToName = data.values.isDisbursement
            ? data.values.partyIdTo?.fromPartyName || ""
            : organizationName;

        const newPayment: Payment = {
            paymentId: formEditMode > 1 ? payment?.paymentId || "" : "",
            paymentTypeId: data.values.paymentTypeId,
            paymentMethodId: data.values.paymentMethodId,
            statusId: data.values.statusId || PAYMENT_STATUSES.NOT_PAID,
            partyIdFrom: data.values.isDisbursement
                ? organizationId
                : data.values.partyIdFrom.fromPartyId,
            partyIdFromName,
            partyIdTo: data.values.isDisbursement
                ? data.values.partyIdTo.fromPartyId
                : organizationId,
            partyIdToName,
            amount: data.values.amount,
            effectiveDate: data.values.effectiveDate || new Date().toISOString(),
            paymentRefNum: data.values.paymentRefNum,
            organizationPartyId: organizationId,
            isDepositWithDrawPayment:
                data.values.paymentMethodId && data.values.isDepositWithDrawPayment ? "Y" : "N",
            finAccountTransTypeId: data.values.isDisbursement ? "WITHDRAWAL" : "DEPOSIT",
            isDisbursement: data.values.isDisbursement || selectedMenuItem === "outgoing",
        };

        if (process.env.NODE_ENV !== "production") {
            console.log("new payment", newPayment);
        }

        if (data.menuItem === "Create Payment") {
            await createPayment(newPayment, customerId, organizationId);
        } else {
            setIsLoading(false);
        }
    };

    /**
     * Handles payment updates based on form data.
     */
    const handleUpdate = async (data: any) => {
        if (!data.isValid) {
            toast.error(language === "ar" ? "يرجى تصحيح أخطاء النموذج قبل الإرسال" : "Please fix form errors before submitting");
            setIsLoading(false);
            return;
        }

        if (!payment) {
            toast.error(language === "ar" ? "لم يتم اختيار دفع للتحديث" : "No payment selected for updating");
            setIsLoading(false);
            return;
        }

        if (!data.values.paymentMethodId) {
            toast.error(language === "ar" ? "طريقة الدفع مطلوبة" : "Payment method is required");
            setIsLoading(false);
            return;
        }

        const customerId = data.values.isDisbursement ? data.values.partyIdTo : data.values.partyIdFrom;
        const organizationId = data.values.organizationPartyId;

        const organization = companies.find(
            (company) => company.organizationPartyId === organizationId
        );
        const organizationName = organization?.organizationPartyName || "Unknown Organization";

        const partyIdFromName = data.values.isDisbursement
            ? organizationName
            : data.values.partyIdFromName || payment.partyIdFromName || "";
        const partyIdToName = data.values.isDisbursement
            ? data.values.partyIdToName || payment.partyIdToName || ""
            : organizationName;

        const updatedPayment: Payment = {
            ...payment,
            paymentMethodId: data.values.paymentMethodId,
            partyIdFromName,
            partyIdToName,
            amount: data.values.amount || payment.amount,
            effectiveDate:
                data.values.effectiveDate && !isNaN(new Date(data.values.effectiveDate).getTime())
                    ? new Date(data.values.effectiveDate).toISOString()
                    : payment.effectiveDate,
            paymentRefNum: data.values.paymentRefNum || payment.paymentRefNum || "",
            organizationPartyId: organizationId,
            isDepositWithDrawPayment:
                data.values.paymentMethodId && data.values.isDepositWithDrawPayment
                    ? "Y"
                    : payment.isDepositWithDrawPayment || "N",
            comments: data.values.comments || payment.comments || "",
            actualCurrencyUomId: data.values.actualCurrencyUomId || payment.actualCurrencyUomId || "",
            actualCurrencyAmount: data.values.actualCurrencyAmount || payment.actualCurrencyAmount || 0,
        };

        try {
            if (data.menuItem === "Update Payment") {
                await updatePaymentData(updatedPayment, customerId, organizationId);
            } else {
                toast.error(language === "ar" ? "تم اختيار إجراء تحديث غير صالح" : "Invalid update action selected");
                setIsLoading(false);
            }
        } catch (error) {
            handleApiError(error, language === "ar" ? "فشل في تحديث الدفع" : "Failed to update payment");
        }
    };

    /**
     * Handles payment status changes based on form data.
     */
    const handleStatusChange = async (data: any) => {
        if (!data.isValid) {
            toast.error(language === "ar" ? "يرجى تصحيح أخطاء النموذج قبل الإرسال" : "Please fix form errors before submitting");
            setIsLoading(false);
            return;
        }

        if (!payment || !payment.paymentId) {
            toast.error(language === "ar" ? "لم يتم اختيار دفع لتغيير الحالة" : "No payment selected for status change");
            setIsLoading(false);
            return;
        }

        const customerId = data.values.isDisbursement ? data.values.partyIdTo : data.values.partyIdFrom;
        const organizationId = data.values.organizationPartyId;

        try {
            switch (data.menuItem) {
                case "Status to Received":
                    await changePaymentStatus(payment.paymentId, PAYMENT_STATUSES.RECEIVED, customerId, organizationId);
                    break;
                case "Status to Sent":
                    await changePaymentStatus(payment.paymentId, PAYMENT_STATUSES.SENT, customerId, organizationId);
                    break;
                case "Status to Cancelled":
                    await changePaymentStatus(payment.paymentId, PAYMENT_STATUSES.CANCELLED, customerId, organizationId);
                    break;
                case "Status to Confirmed":
                    await changePaymentStatus(payment.paymentId, PAYMENT_STATUSES.CONFIRMED, customerId, organizationId);
                    break;
                default:
                    toast.error(language === "ar" ? "تم اختيار إجراء تغيير حالة غير صالح" : "Invalid status change action selected");
                    setIsLoading(false);
            }
        } catch (error) {
            handleApiError(error, language === "ar" ? `فشل في تغيير حالة الدفع` : `Failed to change payment status`);
        }
    };

    return {
        formEditMode,
        setFormEditMode,
        payment,
        setPayment,
        handleCreate,
        handleUpdate,
        handleStatusChange,
        isLoading,
    };
}