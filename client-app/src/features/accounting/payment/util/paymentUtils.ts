// src/app/utils/paymentUtils.ts

export const ADVANCE_TO_VENDOR_CONTRACTOR = "ADVANCE_TO_VENDOR_CONTRACTOR";
export const VENDOR_PAYMENT = "VENDOR_PAYMENT";

export const BALANCE_CHECK_PAYMENT_TYPES = [
    ADVANCE_TO_VENDOR_CONTRACTOR,
    VENDOR_PAYMENT,
] as const;

/**
 * Checks if a payment type requires balance validation (advance or vendor payment)
 */
export const needsBalanceCheck = (paymentTypeId?: string | null): boolean => {
    if (!paymentTypeId) return false;
    return BALANCE_CHECK_PAYMENT_TYPES.includes(
        paymentTypeId as typeof BALANCE_CHECK_PAYMENT_TYPES[number]
    );
};

/**
 * Returns a user-friendly message for when balance check is not available
 */
export const getBalanceErrorMessage = (): string => {
    return "جاري التحقق من الرصيد...";
};

/**
 * Validates the payment amount against available balance
 */
export const validatePaymentAmount = (
    value: number,
    paymentTypeId: string | null | undefined,
    balanceData: { initialBalance: number; remainingBalance: number } | undefined
): string | undefined => {
    if (!value || value <= 0) return "الرجاء إدخال مبلغ صحيح";
    if (!needsBalanceCheck(paymentTypeId)) return undefined;

    if (!balanceData) return getBalanceErrorMessage();

    if (balanceData.initialBalance === 0) {
        return undefined;
    }

    if (value > balanceData.remainingBalance) {
        return `المبلغ المُدخل (${value.toLocaleString("ar-EG")}) يتجاوز الرصيد المتاح (${balanceData.remainingBalance.toLocaleString("ar-EG")})`;
    }

    return undefined;
};