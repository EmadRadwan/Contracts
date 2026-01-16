/* jshint esversion: 6 */

import * as React from 'react';
import {getter} from '@progress/kendo-react-common';
import {FieldValidatorType} from "@progress/kendo-react-form";


const emailRegex = new RegExp(/\S+@\S+\.\S+/);
const phoneRegex = new RegExp(/^01[0-2]{1}[0-9]{8}$/);
const phoneRegexWe = new RegExp(/^015{1}[0-9]{8}$/);
const ccardRegex = new RegExp(/^[0-9-]+$/);
const cvcRegex = new RegExp(/^[0-9]+$/);

export const termsValidator = (value) => value ? "" : "It's required to agree with Terms and Conditions.";
export const emailValidator = (value) => !value ?
    "Email field is required." :
    (emailRegex.test(value) ? "" : "Email is not valid format.");
export const nameValidator = (value) => !value ?
    "Full Name is required" :
    value.length < 7 ? "Full Name should be at least 7 characters long." : "";
export const titleValidator = (value) => !value ?
    "Idea Title is Required" : "";
export const userNameValidator = (value) => !value ?
    "User Name is required" :
    value.length < 5 ? "User name should be at least 3 characters long." : "";
export const phoneValidator = (value) => !value ?
    "Phone number is required." :
    phoneRegex.test(value) || phoneRegexWe.test(value) ? "" : "Not a valid phone number.";
export const cardValidator = (value) => !value ?
    "Credit card number is required. " :
    ccardRegex.test(value) ? "" : "Not a valid credit card number format.";
export const cvcValidator = (value) => !value ?
    "CVC code is required," :
    cvcRegex.test(value) || value.length !== 3 ? "" : "Not a valid CVC code format.";
export const guestsValidator = (value) => !value ?
    "Number of guests is required" :
    value < 5 ? "" : "Maximum 5 guests";
export const nightsValidator = (value) => value ? "" : "Number of Nights is required";
export const savingValidator = (value) => value >= 1 ? "" : "Valid estimated saving is required";
export const completedDateValidator = (value) => {
    if (!value) {
        return ""
    } else {
        if (value >= Date.now()) {
            return ""
        } else {
            return "Future date is not valid";

        }
    }
}
export const dateToValidator = (value) => value >= Date.now() ? "" : "Future date is not valid";
export const arrivalDateValidator = (value) => value ? "" : "Arriaval Date is required";
export const colorValidator = (value) => value ? "" : "Color is required.";
export const requiredValidator = (value) => value ? "" : "Error: This field is required.";
export const radioGroupValidator = (value) =>
    !value ? "Type of Confirmation is required" : "";
export const radioGroupValidatorReturnHeader = (value) =>
    !value ? "Type of Return is required" : "";
export const radioGroupValidatorNeedsInventoryReceive = (value: string | undefined): string =>
    !value ? "Needs Inventory Receive selection is required" : "";
export const radioGroupValidatorSalesOrder = (value) =>
    !value ? "Payment Method Type is required" : "";


export const passwordValidator = (value) => value && value.length > 8 ? '' : 'Password must be at least 8 symbols.';
export const Validator = (value) => value && value.length > 8 ? '' : 'Password must be at least 8 symbols.';
export const addressValidator = (value) => value ? "" : "Address is required.";

const userNameGetter = getter('username');
const emailGetter = getter('email');

export const formValidator = (values) => {
    const userName = userNameGetter(values);
    const emailValue = emailGetter(values);

    if (userName && emailValue && emailRegex.test(emailValue)) {
        return {};
    }

    return {
        VALIDATION_SUMMARY: 'Please fill the following fields.',
        ['username']: !userName ? 'User Name is required.' : '',
        ['email']: emailValue && emailRegex.test(emailValue) ? '' : 'Email is required and should be in valid format.'
    };
};

// create a percentage validator that accepts a number between 0 and 100 as a valid value and returns an error message otherwise
export const percentageValidator = (value) => {
    if (value === null || value === undefined || value === "") {
        return "Percentage is required";
    } else if (value < 0 || value > 100) {
        return "Percentage should be between 0 and 100";
    } else {
        return "";
    }
};

// Validator for the "Bank Transfer" checkbox
// Only allows checking it when the selected payment method has paymentMethodTypeId === 'COMPANY_CHECK'
export const createBankTransferAllowedValidator = (paymentMethods: any[] = []) => {
    return (value: boolean | undefined, getter: (field: string) => any): string | undefined => {
        // Only validate when someone tries to check it
        if (value !== true) {
            return undefined;
        }

        const paymentMethodId = getter('paymentMethodId') as string | undefined;

        if (!paymentMethodId) {
            return "Please select a payment method first";
        }

        if (paymentMethods.length === 0) {
            return "Payment methods data not available";
        }

        const selectedMethod = paymentMethods.find(
            (m) => m.paymentMethodId === paymentMethodId
        );

        if (!selectedMethod) {
            return "Selected payment method not found";
        }

        if (selectedMethod.paymentMethodTypeId !== 'COMPANY_CHECK') {
            return "Bank Transfer is only allowed for bank cheque payment methods (COMPANY_CHECK)";
        }

        return undefined;
    };
};

// Validator for chequeNumber and chequeDate fields
// Enforces:
// - Must be empty when isBankTransfer = true (and it's a COMPANY_CHECK method)
// - Must be filled when isBankTransfer = false (for COMPANY_CHECK methods)
// - Not applicable / should be empty for other methods
export const createChequeFieldsValidator = (paymentMethods: any[] = []) => {
    return (value: any, getter: (field: string) => any): string | undefined => {
        const isBankTransfer = !!getter('isBankTransfer');
        const paymentMethodId = getter('paymentMethodId') as string | undefined;

        // If no method selected yet → no error here (let requiredValidator handle it)
        if (!paymentMethodId) {
            return undefined;
        }

        // If no payment methods loaded → skip for now
        if (paymentMethods.length === 0) {
            return undefined;
        }

        const method = paymentMethods.find((m) => m.paymentMethodId === paymentMethodId);

        // Invalid method → no strong opinion here
        if (!method) {
            return undefined;
        }

        const isCompanyCheck = method.paymentMethodTypeId === 'COMPANY_CHECK';
        const isCash = paymentMethodId === 'CASH';

        // For non-COMPANY_CHECK methods → cheque fields should not be used
        if (!isCompanyCheck) {
            if (value) {
                return "Cheque number/date not applicable for this payment method";
            }
            return undefined;
        }

        // ────────────────────────
        // COMPANY_CHECK method cases
        // ────────────────────────

        if (isBankTransfer) {
            // Bank transfer → cheque should be EMPTY
            if (value) {
                return "Cheque number/date must be empty when Bank Transfer is selected";
            }
            return undefined;
        }

        // Classic cheque payment (not bank transfer) → required
        if (!value) {
            return "Cheque number/date is required for COMPANY_CHECK payments unless using Bank Transfer";
        }

        return undefined;
    };
};

