import { APARTMENT_AVAILABLE } from "../../SalesRequestForm"; // Adjust path if needed

const apartmentSelectionValidator = (value: any, getTranslatedLabel: (key: string, fallback: string) => string) => {
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

    // Optional: also block other non-available statuses if desired
    // if (apt?.apartmentStatusId && apt?.apartmentStatusId !== APARTMENT_AVAILABLE) {
    //     return getTranslatedLabel(
    //         "salesRequest.form.validation.apartmentNotAvailable",
    //         "This apartment is not available for sale."
    //     );
    // }

    return undefined; // valid
};