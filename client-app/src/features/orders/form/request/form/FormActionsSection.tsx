import React from "react";
import { Grid, Button } from "@mui/material";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import { FormRenderProps } from "@progress/kendo-react-form";

interface FormActionsSectionProps {
    formRenderProps: FormRenderProps;
    customInstallmentsLength: number;
    canOpenPaymentPlan: boolean;
    onOpenPaymentPlan: () => void;
    buttonFlag: boolean;
    isCreating: boolean;
    isUpdating: boolean;
    editMode: number;
    onCancel: () => void;
    getTranslatedLabel: (key: string, fallback: string) => string;
}

export const FormActionsSection: React.FC<FormActionsSectionProps> = React.memo(({
                                                                                     formRenderProps,
                                                                                     customInstallmentsLength,
                                                                                     canOpenPaymentPlan,
                                                                                     onOpenPaymentPlan,
                                                                                     buttonFlag,
                                                                                     isCreating,
                                                                                     isUpdating,
                                                                                     editMode,
                                                                                     onCancel,
                                                                                     getTranslatedLabel,
                                                                                 }) => {
    const { visited, errors, valueGetter } = formRenderProps;

    const viewPaymentPlanButton = canOpenPaymentPlan && (
        <Grid item>
            <Button
                variant="outlined"
                color="primary"
                onClick={onOpenPaymentPlan}
            >
                {customInstallmentsLength > 0
                    ? getTranslatedLabel("salesRequest.form.editPaymentPlan", "Edit Payment Plan")
                    : getTranslatedLabel("salesRequest.form.createPaymentPlan", "Create Payment Plan")
                }
            </Button>
        </Grid>
    );

    return (
        <>
            <div className="k-form-buttons" style={{ marginTop: 16 }}>
                <Grid container spacing={1}>
                    {visited && errors?.VALIDATION_SUMMARY && (
                        <Grid item xs={12}>
                            <div className="k-messagebox k-messagebox-error">
                                {errors.VALIDATION_SUMMARY}
                            </div>
                        </Grid>
                    )}

                    <Grid item>
                        <Button
                            size="small"
                            variant="contained"
                            type="submit"
                            color="success"
                            disabled={
                                buttonFlag ||
                                isCreating ||
                                isUpdating ||
                                editMode > 2 ||
                                (valueGetter("totalPrice") > 0 && customInstallmentsLength === 0)
                            }
                        >
                            {editMode === 1
                                ? getTranslatedLabel("general.create", "Create")
                                : getTranslatedLabel("general.update", "Update")}
                        </Button>
                    </Grid>

                    <Grid item>
                        <Button
                            size="small"
                            onClick={onCancel}
                            color="error"
                            variant="contained"
                        >
                            {getTranslatedLabel("general.cancel", "Cancel")}
                        </Button>
                    </Grid>

                    {viewPaymentPlanButton}
                </Grid>
            </div>

            {(buttonFlag || isCreating || isUpdating) && (
                <LoadingComponent
                    message={getTranslatedLabel("salesRequest.form.processing", "Processing...")}
                />
            )}
        </>
    );
});

FormActionsSection.displayName = "FormActionsSection";