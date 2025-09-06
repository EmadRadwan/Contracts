import React from "react";
import { Button, Grid } from "@mui/material";

interface FormButtonsProps {
    editMode: number; // 1: add, 2: edit
    formEditMode: number; // 0: view, 1: create, 2: CREATED, 3: APPROVED, 4: COMPLETED
    allowSubmit: boolean;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    onClose: () => void;
}

const FormButtons = ({
                         editMode,
                         formEditMode,
                         allowSubmit,
                         getTranslatedLabel,
                         onClose,
                     }: FormButtonsProps) => {
    // REFACTOR: Extracted button section into a shared component
    // Purpose: Reduce duplication in ProcurementForm and ContractingForm
    // Context: Centralizes button logic for consistency across forms
    return (
        <div className="k-form-buttons">
            <Grid container spacing={2}>
                <Grid item xs={6}>
                    <Button
                        variant="contained"
                        type="submit"
                        color="success"
                        disabled={!allowSubmit || formEditMode > 3}
                        fullWidth
                    >
                        {editMode === 2
                            ? getTranslatedLabel("certificate.items.form.update", "Update")
                            : getTranslatedLabel("certificate.items.form.add", "Add")}
                    </Button>
                </Grid>
                <Grid item xs={6}>
                    <Button onClick={onClose} variant="contained" color="error" fullWidth>
                        {getTranslatedLabel("certificate.items.form.cancel", "Cancel")}
                    </Button>
                </Grid>
            </Grid>
        </div>
    );
};

export default FormButtons;