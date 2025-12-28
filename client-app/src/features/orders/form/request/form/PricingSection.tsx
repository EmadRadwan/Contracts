import React from "react";
import { Field, FormRenderProps } from "@progress/kendo-react-form";
import { Grid, Button } from "@mui/material";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import ApartmentPriceCalculatorModal from "../dashboard/ApartmentPriceCalculatorModal";
import ModalContainer from "../../../../../app/common/modals/ModalContainer";
import { SalesRequest } from "../../../../../app/models/order/SalesRequest";

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

export const PricingSection: React.FC<PricingSectionProps> = React.memo(({
                                                                             formRenderProps,
                                                                             selectedApartment,
                                                                             onPricePerM2Change,
                                                                             onDiscountChange,
                                                                             autoSetDerivedFields,
                                                                             showCalculatorModal,
                                                                             setShowCalculatorModal,
                                                                             getTranslatedLabel,
                                                                         }) => {
    const { valueGetter, onChange } = formRenderProps;
    const isGroundFloor = selectedApartment?.floorNumber === "الطابق الأرضي";
    const totalPrice = valueGetter("totalPrice");

    const openCalculator = () => {
        if (totalPrice && totalPrice > 0) {
            setShowCalculatorModal(true);
        }
    };

    const handleCalculatorApply = (totalDiscount: number, finalPrice: number) => {
        onChange("discount", { value: totalDiscount });
        onChange("totalPrice", { value: finalPrice });
        autoSetDerivedFields(formRenderProps, finalPrice);
        setShowCalculatorModal(false);
    };

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
                        validator={() => (valueGetter("apartmentPricePerM2") ? undefined : "Required")}
                        onChange={(e) => onPricePerM2Change(formRenderProps, "apartmentPricePerM2", e.value)}
                    />
                </Grid>
                <Grid item xs={3}>
                    <Field
                        name="gardenPricePerM2"
                        label={getTranslatedLabel("salesRequest.form.gardenPriceM2", "Garden/m²")}
                        format="n2"
                        min={0}
                        component={FormNumericTextBox}
                        disabled={!isGroundFloor}
                        onChange={(e) => onPricePerM2Change(formRenderProps, "gardenPricePerM2", e.value)}
                    />
                </Grid>
                <Grid item xs={2}>
                    <Field
                        name="discount"
                        label={getTranslatedLabel("salesRequest.form.discount", "Discount")}
                        format="n2"
                        min={0}
                        component={FormNumericTextBox}
                        onChange={(e) => onDiscountChange(formRenderProps, e.value)}
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
                <Grid item xs={1} />
                <Grid item xs={2}>
                    <Field
                        name="totalPrice"
                        label={getTranslatedLabel("salesRequest.form.totalPrice", "Total")}
                        format="n2"
                        component={FormNumericTextBox}
                        disabled={true}
                        validator={() => (totalPrice ? undefined : "Required")}
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
});

PricingSection.displayName = "PricingSection";