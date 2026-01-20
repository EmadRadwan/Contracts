import React from "react";
import { Field, FormRenderProps } from "@progress/kendo-react-form";
import { Grid, Button, Typography } from "@mui/material";
import { FormSimpleComboBoxVirtualApartment } from "../../../../../app/common/form/FormSimpleComboBoxVirtualApartment";
import FormDatePicker from "../../../../../app/common/form/FormDatePicker";
import { FormComboBoxVirtualCustomer } from "../../../../../app/common/form/FormComboBoxVirtualCustomer";
import { FormComboBoxVirtualPartyEmployee } from "../../../../../app/common/form/FormComboBoxVirtualPartyEmployee";
import { requiredValidator } from "../../../../../app/common/form/Validators";
import { SalesRequest } from "../../../../../app/models/order/SalesRequest";

interface ApartmentHeaderSectionProps {
    formRenderProps: FormRenderProps;
    selectedApartment: SalesRequest | null;
    onProductChange: (form: FormRenderProps, e: any) => void;
    showNewCustomer: boolean;
    setShowNewCustomer: (show: boolean) => void;
    getTranslatedLabel: (key: string, fallback: string) => string;
    partyInputRef: React.RefObject<HTMLInputElement>;
    editMode: number;
}

export const ApartmentHeaderSection: React.FC<ApartmentHeaderSectionProps> = React.memo(
    ({
         formRenderProps,
         selectedApartment,
         onProductChange,
         setShowNewCustomer,
         getTranslatedLabel,
         partyInputRef,
         editMode,
     }) => {
        const apt = formRenderProps.valueGetter("productId");

        // Helper: should we show garden area?
        const hasGardenArea =
            selectedApartment?.gardenSpaceM2 != null &&
            !isNaN(Number(selectedApartment.gardenSpaceM2)) &&
            Number(selectedApartment.gardenSpaceM2) > 0;

        const apartmentSelectionValidator = (
            value: any,
            getTranslatedLabel: (key: string, fallback: string) => string
        ) => {
            // Required check
            if (!value) {
                return getTranslatedLabel("validation.required", "This field is required.");
            }

            // Value is the selected apartment object from the ComboBox
            const apt = value as any;

            // If status is SOLD and it's not reserved by the current sales request (in edit mode), reject
            if (apt?.apartmentStatusId === "APARTMENT_SOLD") {
                return getTranslatedLabel(
                    "salesRequest.form.validation.apartmentSold",
                    "This apartment is already sold and cannot be selected."
                );
            }

            return undefined; // valid
        };

        return (
            <>
                {/* Main Selection Row */}
                <Grid
                    container
                    spacing={1}
                    alignItems="flex-end"
                    className={editMode > 2 ? "grid-disabled" : "grid-normal"}
                >
                    <Grid item xs={4}>
                        <Field
                            id="productId"
                            name="productId"
                            label={getTranslatedLabel("salesRequest.form.product", "Product *")}
                            component={FormSimpleComboBoxVirtualApartment}
                            autoComplete="off"
                            validator={(value) =>
                                requiredValidator(value) || apartmentSelectionValidator(value, getTranslatedLabel)
                            }
                            onChange={(e) => onProductChange(formRenderProps, e)}
                        />
                        {formRenderProps.visited?.productId && formRenderProps.errors?.productId && (
                            <Typography variant="caption" color="error" sx={{ mt: 0.5, display: "block" }}>
                                {formRenderProps.errors.productId}
                            </Typography>
                        )}
                    </Grid>
                    <Grid item xs={3}>
                        <Field
                            id="saleDate"
                            name="saleDate"
                            label={getTranslatedLabel("salesRequest.form.saleDate", "Sale Date *")}
                            component={FormDatePicker}
                            validator={requiredValidator}
                        />
                    </Grid>
                    <Grid item xs={2.5}>
                        <Field
                            id="fromPartyId"
                            name="fromPartyId"
                            label={getTranslatedLabel("salesRequest.form.from", "From *")}
                            component={FormComboBoxVirtualCustomer}
                            autoComplete="off"
                            validator={requiredValidator}
                            inputRef={partyInputRef}
                        />
                    </Grid>
                    <Grid item xs={0.5}>
                        <Button
                            size="small"
                            color="secondary"
                            onClick={() => setShowNewCustomer(true)}
                            variant="outlined"
                            sx={{ height: "100%", minWidth: 32, p: 0 }}
                        >
                            +
                        </Button>
                    </Grid>
                    <Grid item xs={2}>
                        <Field
                            id="employeePartyId"
                            name="employeePartyId"
                            component={FormComboBoxVirtualPartyEmployee}
                            label={getTranslatedLabel("salesRequest.form.employee", "Employee")}
                            valueField="fromPartyId"
                            textField="fromPartyName"
                            validator={requiredValidator}
                        />
                    </Grid>
                </Grid>

                {/* Read-only Info Row */}
                <Grid container spacing={1} mt={0.5}>
                    <Grid item xs={3}>
                        <Typography variant="caption" color="textSecondary">
                            {getTranslatedLabel("salesRequest.form.project", "Project")}
                        </Typography>
                        <Typography>{apt?.projectName ?? "-"}</Typography>
                    </Grid>

                    <Grid item xs={2}>
                        <Typography variant="caption" color="textSecondary">
                            {getTranslatedLabel("salesRequest.form.apartmentM2", "Apt m²")}
                        </Typography>
                        <Typography>{apt?.apartmentSpaceM2 ?? "-"}</Typography>
                    </Grid>

                    <Grid item xs={2}>
                        <Typography variant="caption" color="textSecondary">
                            {getTranslatedLabel("salesRequest.form.gardenM2", "Garden / Terrace m²")}
                        </Typography>
                        <Typography>
                            {hasGardenArea ? Number(selectedApartment?.gardenSpaceM2).toFixed(2) : "-"}
                        </Typography>
                    </Grid>

                    <Grid item xs={3}>
                        <Typography variant="caption" color="textSecondary">
                            {getTranslatedLabel("salesRequest.form.status", "Status")}
                        </Typography>
                        <Typography>{apt?.apartmentStatusDescription ?? "-"}</Typography>
                    </Grid>

                    <Grid item xs={2} /> {/* spacer */}
                </Grid>

                {/* Optional: small hint when garden exists but price not set yet */}
                {hasGardenArea && !apt?.gardenPricePerM2 && (
                    <Typography
                        variant="caption"
                        color="textSecondary"
                        sx={{ mt: 0.5, display: "block", fontStyle: "italic" }}
                    >
                        Garden area detected — please enter Garden/m² price if applicable
                    </Typography>
                )}
            </>
        );
    }
);

ApartmentHeaderSection.displayName = "ApartmentHeaderSection";