import { useCallback } from "react";
import { FormRenderProps } from "@progress/kendo-react-form";
import { toNumber } from "lodash";
import {SalesRequest} from "../../../../../app/models/order/SalesRequest";


interface UseSalesRequestCalculationsProps {
    defaultAdvancePercent: number;
    defaultMaintenancePercent: number;
}

export const useSalesRequestCalculations = ({
                                                defaultAdvancePercent,
                                                defaultMaintenancePercent,
                                            }: UseSalesRequestCalculationsProps) => {
    const normalizeNumeric = (value: any): number | null => {
        if (value === null || value === undefined || value === "") return null;
        const num = Number(value);
        return isNaN(num) ? null : num;
    };

    const calculateBaseTotal = useCallback((
        aptM2: number | null,
        aptPrice: number | null,
        gardenM2: number | null,
        gardenPrice: number | null
    ): number | null => {
        if (aptM2 == null || aptPrice == null) return null;

            let total = aptM2 * aptPrice;

            // Garden is included only if both values are present and positive
            if (
                gardenM2 != null &&
                gardenM2 > 0 &&
                gardenPrice != null &&
                gardenPrice > 0
            ) {
                total += gardenM2 * gardenPrice;
            }

            return total;
        },
        []
    );

    const calculateFinalTotal = useCallback(
        (baseTotal: number | null, discount: number | null): number | null => {
            if (baseTotal == null) return null;
            return discount != null ? Math.max(0, baseTotal - discount) : baseTotal;
        },
        []
    );

    const autoSetDerivedFields = useCallback(
        (formRenderProps: FormRenderProps, finalTotal: number | null) => {
            if (finalTotal === null) {
                formRenderProps.onChange("advancePayment", { value: null });
                formRenderProps.onChange("maintenanceDeposit", { value: null });
                return;
            }

            formRenderProps.onChange("advancePayment", {
                value: Math.round(finalTotal * defaultAdvancePercent),
            });
            formRenderProps.onChange("maintenanceDeposit", {
                value: Math.round(finalTotal * defaultMaintenancePercent),
            });
        },
        [defaultAdvancePercent, defaultMaintenancePercent]
    );

    const handleProductChange = useCallback(
        (
            formRenderProps: FormRenderProps,
            e: any,
            setSelectedApartment: (apt: SalesRequest | null) => void
        ) => {
            const apartment = e.value as any;
            setSelectedApartment(apartment);

            formRenderProps.onChange("apartmentPricePerM2", {
                value: apartment?.apartmentPricePerM2 ?? null,
            });

            // No more floor-based restriction — we take whatever came from the apartment
            formRenderProps.onChange("gardenPricePerM2", {
                value: apartment?.gardenPricePerM2 ?? null,
            });

            formRenderProps.onChange("discount", { value: null });

            const baseTotal = calculateBaseTotal(
                toNumber(apartment?.apartmentSpaceM2),
                toNumber(apartment?.apartmentPricePerM2),
                toNumber(apartment?.gardenSpaceM2),
                toNumber(apartment?.gardenPricePerM2)
            );

            const finalTotal = calculateFinalTotal(baseTotal, null);
            const roundedFinalTotal = finalTotal != null ? Math.round(finalTotal) : null;
            formRenderProps.onChange("totalPrice", { value: roundedFinalTotal });
            autoSetDerivedFields(formRenderProps, roundedFinalTotal);
        },
        [calculateBaseTotal, calculateFinalTotal, autoSetDerivedFields]
    );

    const handlePricePerM2Change = useCallback(
        (
            formRenderProps: FormRenderProps,
            fieldName: "apartmentPricePerM2" | "gardenPricePerM2",
            value: number | null
        ) => {
            formRenderProps.onChange(fieldName, { value });

            const aptObj = formRenderProps.valueGetter("productId");
            if (!aptObj) return;

            const aptM2 = normalizeNumeric(aptObj.apartmentSpaceM2);
            const aptPrice =
                fieldName === "apartmentPricePerM2"
                    ? value
                    : normalizeNumeric(formRenderProps.valueGetter("apartmentPricePerM2"));

            const gardenM2 = normalizeNumeric(aptObj.gardenSpaceM2);
            const gardenPrice =
                fieldName === "gardenPricePerM2"
                    ? value
                    : normalizeNumeric(formRenderProps.valueGetter("gardenPricePerM2"));

            const discount = normalizeNumeric(formRenderProps.valueGetter("discount")) ?? 0;

            const baseTotal = calculateBaseTotal(aptM2, aptPrice, gardenM2, gardenPrice);
            const finalTotal = calculateFinalTotal(baseTotal, discount);
            const roundedFinalTotal = finalTotal != null ? Math.round(finalTotal) : null;

            formRenderProps.onChange("totalPrice", { value: roundedFinalTotal });
            autoSetDerivedFields(formRenderProps, roundedFinalTotal);
        },
        [calculateBaseTotal, calculateFinalTotal, autoSetDerivedFields]
    );

    const handleDiscountChange = useCallback(
        (formRenderProps: FormRenderProps, value: number | null) => {
            formRenderProps.onChange("discount", { value });

            const aptObj = formRenderProps.valueGetter("productId");
            if (!aptObj) return;

            const baseTotal = calculateBaseTotal(
                normalizeNumeric(aptObj.apartmentSpaceM2),
                normalizeNumeric(formRenderProps.valueGetter("apartmentPricePerM2")),
                normalizeNumeric(aptObj.gardenSpaceM2),
                normalizeNumeric(formRenderProps.valueGetter("gardenPricePerM2"))
            );

            const finalTotal = calculateFinalTotal(baseTotal, value);
            const roundedFinalTotal = finalTotal != null ? Math.round(finalTotal) : null;
            formRenderProps.onChange("totalPrice", { value: roundedFinalTotal });
            autoSetDerivedFields(formRenderProps, roundedFinalTotal);
        },
        [calculateBaseTotal, calculateFinalTotal, autoSetDerivedFields]
    );

    const handleAdvanceChange = useCallback(
        (formRenderProps: FormRenderProps, value: number | null) => {
            const roundedValue = value != null ? Math.round(value) : null;
            formRenderProps.onChange("advancePayment", { value: roundedValue });

            if (roundedValue == null) return;

            const selectedApartmentObj = formRenderProps.valueGetter("productId");
            if (!selectedApartmentObj) return;

            const baseTotal = calculateBaseTotal(
                normalizeNumeric(selectedApartmentObj.apartmentSpaceM2),
                normalizeNumeric(formRenderProps.valueGetter("apartmentPricePerM2")),
                normalizeNumeric(selectedApartmentObj.gardenSpaceM2),
                normalizeNumeric(formRenderProps.valueGetter("gardenPricePerM2"))
            );

            if (baseTotal == null) return;

            const discount = toNumber(formRenderProps.valueGetter("discount")) || 0;
            const currentTotal = Math.max(0, Math.round(baseTotal - discount));

            // Optional: prevent advance > total (you may want to show a warning instead)
            if (roundedValue > currentTotal) {
                formRenderProps.onChange("advancePayment", { value: currentTotal });
                // or show toast/warning: "Advance cannot exceed total price"
            }
        },
        [calculateBaseTotal]
    );

    return {
        autoSetDerivedFields,
        handleProductChange,
        handlePricePerM2Change,
        handleDiscountChange,
        handleAdvanceChange,
    };
};