import { useCallback } from "react";
import { FormRenderProps } from "@progress/kendo-react-form";
import { toNumber } from "lodash";
import {SalesRequest} from "../../../../../app/models/order/SalesRequest";

const GROUND_FLOOR_ARABIC = "الطابق الأرضي";

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

        const apartmentTotal = aptM2 * aptPrice;

        if (gardenM2 != null && gardenPrice != null && gardenM2 > 0 && gardenPrice > 0) {
            return apartmentTotal + (gardenM2 * gardenPrice);
        }

        return apartmentTotal;
    }, []);

    const calculateFinalTotal = useCallback((
        baseTotal: number | null,
        discount: number | null
    ): number | null => {
        if (baseTotal == null) return null;
        return discount != null ? Math.max(0, baseTotal - discount) : baseTotal;
    }, []);

    const autoSetDerivedFields = useCallback((
        formRenderProps: FormRenderProps,
        finalTotal: number | null
    ) => {
        if (finalTotal === null) {
            formRenderProps.onChange("advancePayment", { value: null });
            formRenderProps.onChange("maintenanceDeposit", { value: null });
            return;
        }

        formRenderProps.onChange("advancePayment", {
            value: finalTotal * defaultAdvancePercent,
        });
        formRenderProps.onChange("maintenanceDeposit", {
            value: finalTotal * defaultMaintenancePercent,
        });
    }, [defaultAdvancePercent, defaultMaintenancePercent]);

    const handleProductChange = useCallback((
        formRenderProps: FormRenderProps,
        e: any,
        setSelectedApartment: (apt: SalesRequest | null) => void
    ) => {
        const apartment = e.value as any;
        setSelectedApartment(apartment);

        formRenderProps.onChange("apartmentPricePerM2", {
            value: apartment?.apartmentPricePerM2 ?? null,
        });

        const isGroundFloor = apartment?.floorNumber === GROUND_FLOOR_ARABIC;
        formRenderProps.onChange("gardenPricePerM2", {
            value: isGroundFloor ? (apartment?.gardenPricePerM2 ?? null) : null,
        });

        formRenderProps.onChange("discount", { value: null });

        const baseTotal = calculateBaseTotal(
            toNumber(apartment?.apartmentSpaceM2),
            toNumber(apartment?.apartmentPricePerM2),
            isGroundFloor ? toNumber(apartment?.gardenSpaceM2) : null,
            isGroundFloor ? toNumber(apartment?.gardenPricePerM2) : null
        );

        const finalTotal = calculateFinalTotal(baseTotal, null);
        formRenderProps.onChange("totalPrice", { value: finalTotal });
        autoSetDerivedFields(formRenderProps, finalTotal);
    }, [calculateBaseTotal, calculateFinalTotal, autoSetDerivedFields]);

    const handlePricePerM2Change = useCallback((
        formRenderProps: FormRenderProps,
        fieldName: "apartmentPricePerM2" | "gardenPricePerM2",
        value: number | null
    ) => {
        formRenderProps.onChange(fieldName, { value });

        const aptObj = formRenderProps.valueGetter("productId");
        if (!aptObj) return;

        const aptM2 = normalizeNumeric(aptObj.apartmentSpaceM2);
        const aptPrice = fieldName === "apartmentPricePerM2"
            ? value
            : normalizeNumeric(formRenderProps.valueGetter("apartmentPricePerM2"));

        const isGroundFloor = aptObj.floorNumber === GROUND_FLOOR_ARABIC;
        const gardenM2 = isGroundFloor ? normalizeNumeric(aptObj.gardenSpaceM2) : 0;
        const gardenPrice = fieldName === "gardenPricePerM2" && isGroundFloor
            ? value
            : normalizeNumeric(formRenderProps.valueGetter("gardenPricePerM2"));

        const discount = normalizeNumeric(formRenderProps.valueGetter("discount")) ?? 0;

        const baseTotal = calculateBaseTotal(aptM2, aptPrice, gardenM2, gardenPrice);
        const finalTotal = calculateFinalTotal(baseTotal, discount);

        formRenderProps.onChange("totalPrice", { value: finalTotal });
        autoSetDerivedFields(formRenderProps, finalTotal);
    }, [calculateBaseTotal, calculateFinalTotal, autoSetDerivedFields]);

    const handleDiscountChange = useCallback((
        formRenderProps: FormRenderProps,
        value: number | null
    ) => {
        formRenderProps.onChange("discount", { value });

        const aptObj = formRenderProps.valueGetter("productId");
        if (!aptObj) return;

        const isGroundFloor = aptObj.floorNumber === GROUND_FLOOR_ARABIC;

        const baseTotal = calculateBaseTotal(
            normalizeNumeric(aptObj.apartmentSpaceM2),
            normalizeNumeric(formRenderProps.valueGetter("apartmentPricePerM2")),
            isGroundFloor ? normalizeNumeric(aptObj.gardenSpaceM2) : 0,
            isGroundFloor ? normalizeNumeric(formRenderProps.valueGetter("gardenPricePerM2")) : null
        );

        const finalTotal = calculateFinalTotal(baseTotal, value);
        formRenderProps.onChange("totalPrice", { value: finalTotal });
        autoSetDerivedFields(formRenderProps, finalTotal);
    }, [calculateBaseTotal, calculateFinalTotal, autoSetDerivedFields]);

    const handleAdvanceChange = useCallback((
        formRenderProps: FormRenderProps,
        value: number | null
    ) => {
        formRenderProps.onChange("advancePayment", { value });

        if (value == null) return;

        const selectedApartmentObj = formRenderProps.valueGetter("productId");
        if (!selectedApartmentObj) return;

        const isGroundFloor = selectedApartmentObj.floorNumber === GROUND_FLOOR_ARABIC;

        const baseTotal = calculateBaseTotal(
            normalizeNumeric(selectedApartmentObj.apartmentSpaceM2),
            normalizeNumeric(formRenderProps.valueGetter("apartmentPricePerM2")),
            isGroundFloor ? normalizeNumeric(selectedApartmentObj.gardenSpaceM2) : null,
            isGroundFloor ? normalizeNumeric(formRenderProps.valueGetter("gardenPricePerM2")) : null
        );

        if (baseTotal == null) return;

        const discount = toNumber(formRenderProps.valueGetter("discount")) || 0;
        const currentTotal = Math.max(0, baseTotal - discount);

        if (value > currentTotal) {
            formRenderProps.onChange("totalPrice", { value });
        }
    }, [calculateBaseTotal]);

    return {
        autoSetDerivedFields,
        handleProductChange,
        handlePricePerM2Change,
        handleDiscountChange,
        handleAdvanceChange,
    };
};