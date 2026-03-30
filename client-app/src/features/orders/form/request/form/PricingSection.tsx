import React from "react";
import { Field, FormRenderProps } from "@progress/kendo-react-form";
import { Grid, Button } from "@mui/material";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import ApartmentPriceCalculatorModal from "../dashboard/ApartmentPriceCalculatorModal";
import ModalContainer from "../../../../../app/common/modals/ModalContainer";
import { SalesRequest } from "../../../../../app/models/order/SalesRequest";
import { MemoizedFormCheckBox } from "../../../../../app/common/form/FormCheckBox";

interface PricingSectionProps {
    formRenderProps: FormRenderProps;
    selectedApartment: SalesRequest | null;
    onPricePerM2Change: (
        form: FormRenderProps,
        field: "apartmentPricePerM2" | "gardenPricePerM2",
        value: number | null
    ) => void;
    onDiscountChange: (form: FormRenderProps, value: number | null) => void;
    autoSetDerivedFields: (form: FormRenderProps, finalTotal: number | null) => void;
    showCalculatorModal: boolean;
    setShowCalculatorModal: (open: boolean) => void;
    getTranslatedLabel: (key: string, fallback: string) => string;
}

export const PricingSection: React.FC<PricingSectionProps> = React.memo(
    ({
         formRenderProps,
         selectedApartment,
         onPricePerM2Change,
         onDiscountChange,
         autoSetDerivedFields,
         showCalculatorModal,
         setShowCalculatorModal,
         getTranslatedLabel,
     }) => {
        const formRenderPropsRef = React.useRef(formRenderProps);
        formRenderPropsRef.current = formRenderProps;

        const { valueGetter, onChange } = formRenderProps;

        // ────────────────────────────────────────────────
        // Determine if garden-related fields should be visible/editable
        // New logic: based on gardenSpaceM2 > 0 — not on floor number
        // ────────────────────────────────────────────────
        const gardenArea = selectedApartment?.gardenSpaceM2;
        const hasGarden =
            gardenArea != null &&
            !isNaN(Number(gardenArea)) &&
            Number(gardenArea) > 0;

        const totalPrice = valueGetter("totalPrice");

        const handlePricePerM2ChangeInternal = React.useCallback(
            (field: "apartmentPricePerM2" | "gardenPricePerM2", value: number | null) => {
                onPricePerM2Change(formRenderPropsRef.current, field, value);
            },
            [onPricePerM2Change]
        );

        const handleDiscountChangeInternal = React.useCallback(
            (value: number | null) => {
                onDiscountChange(formRenderPropsRef.current, value);
            },
            [onDiscountChange]
        );

        const aptPriceValidator = React.useCallback(
            () => (formRenderPropsRef.current.valueGetter("apartmentPricePerM2") ? undefined : "Required"),
            []
        );

        const totalPriceValidator = React.useCallback(
            () => (formRenderPropsRef.current.valueGetter("totalPrice") ? undefined : "Required"),
            []
        );

        const openCalculator = React.useCallback(() => {
            const currentPrice = formRenderPropsRef.current.valueGetter("totalPrice");
            if (currentPrice && currentPrice > 0) {
                setShowCalculatorModal(true);
            }
        }, [setShowCalculatorModal]);

        const handleCalculatorApply = React.useCallback((totalDiscount: number, finalPrice: number) => {
            const roundedDiscount = Math.round(totalDiscount);
            const roundedPrice = Math.round(finalPrice);
            formRenderPropsRef.current.onChange("discount", { value: roundedDiscount });
            formRenderPropsRef.current.onChange("totalPrice", { value: roundedPrice });
            autoSetDerivedFields(formRenderPropsRef.current, roundedPrice);
            setShowCalculatorModal(false);
        }, [autoSetDerivedFields, setShowCalculatorModal]);

        return (
            <>
                <Grid container spacing={1} alignItems="flex-end">
                    <Grid item xs={3}>
                        <Field
                            name="apartmentPricePerM2"
                            label={getTranslatedLabel("salesRequest.form.apartmentPriceM2", "Apt/m² *")}
                            format="n2"
                            min={0}
                            component={FormNumericTextBox}
                            validator={aptPriceValidator}
                            onChange={(e) => handlePricePerM2ChangeInternal("apartmentPricePerM2", e.value)}
                        />
                    </Grid>

                    <Grid item xs={3}>
                        {hasGarden ? (
                            <Field
                                name="gardenPricePerM2"
                                label={getTranslatedLabel("salesRequest.form.gardenPriceM2", "Garden/Terrace/m²")}
                                format="n2"
                                min={0}
                                component={FormNumericTextBox}
                                // No longer disabled based on floor
                                onChange={(e) => handlePricePerM2ChangeInternal("gardenPricePerM2", e.value)}
                            />
                        ) : (
                            <Field
                                name="gardenPricePerM2"
                                label={getTranslatedLabel("salesRequest.form.gardenPriceM2", "Garden/Terrace/m²")}
                                format="n2"
                                min={0}
                                component={FormNumericTextBox}
                                disabled={true}
                                hint="No garden area defined for this unit"
                            />
                        )}
                    </Grid>

                    <Grid item xs={2}>
                        <Field
                            name="discount"
                            label={getTranslatedLabel("salesRequest.form.discount", "Discount")}
                            format="n2"
                            min={0}
                            component={FormNumericTextBox}
                            onChange={(e) => handleDiscountChangeInternal(e.value)}
                        />
                    </Grid>

                    <Grid item xs={1}>
                        <Button
                            size="small"
                            variant="outlined"
                            color="secondary"
                            onClick={openCalculator}
                            disabled={!totalPrice || totalPrice <= 0}
                        >
                            +
                        </Button>
                    </Grid>

                    <Grid item xs={1}>
                        <Field
                            id="isChequesDelivered"
                            name="isChequesDelivered"
                            label={getTranslatedLabel("salesRequest.form.isChequesDelivered", "Cheques Delivered")}
                            component={MemoizedFormCheckBox}
                        />
                    </Grid>

                    <Grid item xs={2}>
                        <Field
                            name="totalPrice"
                            label={getTranslatedLabel("salesRequest.form.totalPrice", "Total")}
                            format="n2"
                            component={FormNumericTextBox}
                            disabled={true}
                            validator={totalPriceValidator}
                        />
                    </Grid>
                </Grid>
                

                {showCalculatorModal && totalPrice && (
                    <ModalContainer
                        show={showCalculatorModal}
                        onClose={() => setShowCalculatorModal(false)}
                        width={700}
                    >
                        <ApartmentPriceCalculatorModal
                            basePrice={totalPrice}
                            onClose={() => setShowCalculatorModal(false)}
                            onApply={handleCalculatorApply}
                        />
                    </ModalContainer>
                )}
            </>
        );
    }
);

PricingSection.displayName = "PricingSection";