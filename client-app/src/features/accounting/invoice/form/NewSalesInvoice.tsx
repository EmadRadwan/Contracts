import React, { useRef, useState } from "react";
import { Form, FormElement, Field } from "@progress/kendo-react-form";
import { Button, Grid } from "@mui/material";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { FormComboBoxVirtualCustomer } from "../../../../app/common/form/FormComboBoxVirtualCustomer";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useFetchCompaniesQuery, useFetchInvoiceTypesQuery } from "../../../../app/store/configureStore";
import { requiredValidator } from "../../../../app/common/form/Validators";
import useInvoice from "../hook/useInvoice";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import FormInput from "../../../../app/common/form/FormInput";
import FormTextArea from "../../../../app/common/form/FormTextArea";

interface Props {
    onClose: () => void;
}

const NewSalesInvoice = ({ onClose }: Props) => {
    const formRef = useRef<Form | null>(null);
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.invoices.display.form";

    const { data: invoiceTypes } = useFetchInvoiceTypesQuery(undefined);
    const { data: companies } = useFetchCompaniesQuery(undefined);
    const [isLoading, setIsLoading] = useState(false);

    const { handleCreate } = useInvoice(null);

    const salesInvoiceTypes =
        invoiceTypes?.filter(
            (type: any) => type.invoiceTypeId === "SALES_INVOICE" || type.parentTypeId === "SALES_INVOICE"
        ) || [];

    const handleSubmit = async (values: any) => {
        try {
            await handleCreate({ ...values, statusId: "INVOICE_IN_PROCESS" });
            onClose();
        } catch (e) {
            console.error("Error creating sales invoice:", e);
        }
    };

    return (
        <>
            {isLoading && (
                <div
                    style={{
                        position: "fixed",
                        top: 0,
                        left: 0,
                        width: "100%",
                        height: "100%",
                        backgroundColor: "rgba(0, 0, 0, 0.5)",
                        display: "flex",
                        justifyContent: "center",
                        alignItems: "center",
                        zIndex: 1000,
                    }}
                >
                    <LoadingComponent
                        message={getTranslatedLabel(`${localizationKey}.loading`, "Processing Invoice...")}
                    />
                </div>
            )}

            <Form
                onSubmit={handleSubmit}
                ref={formRef}
                render={(formRenderProps) => (
                    <FormElement>
                        <Grid container spacing={2}>
                            {/* Invoice Type */}
                            <Grid item xs={12} sm={6}>
                                <Field
                                    name="invoiceTypeId"
                                    label={getTranslatedLabel(`${localizationKey}.invoice-type`, "Invoice Type")}
                                    component={MemoizedFormDropDownList}
                                    data={salesInvoiceTypes}
                                    dataItemKey="invoiceTypeId"
                                    textField="description"
                                    defaultValue="SALES_INVOICE"
                                    validator={requiredValidator}
                                />
                            </Grid>

                            {/* Our Company */}
                            <Grid item xs={12} sm={6}>
                                <Field
                                    name="organizationPartyId"
                                    label={getTranslatedLabel(`${localizationKey}.organizationPartyId`, "Our Company")}
                                    component={MemoizedFormDropDownList}
                                    data={companies || []}
                                    dataItemKey="organizationPartyId"
                                    textField="organizationPartyName"
                                    validator={requiredValidator}
                                />
                            </Grid>

                            {/* Customer */}
                            <Grid item xs={12} sm={6}>
                                <Field
                                    name="partyId"
                                    label={getTranslatedLabel(`${localizationKey}.to-party`, "Customer")}
                                    component={FormComboBoxVirtualCustomer}
                                    validator={requiredValidator}
                                />
                            </Grid>

                            {/* Invoice Date */}
                            <Grid item xs={12} sm={6}>
                                <Field
                                    name="invoiceDate"
                                    label={getTranslatedLabel(`${localizationKey}.invoice-date`, "Invoice Date")}
                                    component={FormDatePicker}
                                    format="dd MMM yyyy"
                                    validator={requiredValidator}
                                />
                            </Grid>

                            {/* Customer PO / Reference (optional) */}
                            <Grid item xs={12} sm={6}>
                                <Field
                                    name="referenceNumber"
                                    label={getTranslatedLabel(`${localizationKey}.referenceNumber`, "Customer PO / Ref No")}
                                    component={FormInput}
                                    // Optional field – no validator
                                />
                            </Grid>

                            {/* Description / Notes */}
                            <Grid item xs={12}>
                                <Field
                                    name="description"
                                    label={getTranslatedLabel(`${localizationKey}.description`, "Description / Notes")}
                                    component={FormTextArea}
                                    rows={3}
                                />
                            </Grid>

                            {/* Hidden fields */}
                            <Field name="statusId" component="input" type="hidden" value="INVOICE_IN_PROCESS" />
                            <Field name="currencyUomId" component="input" type="hidden" value="EGP" />

                            {/* Buttons */}
                            <Grid item xs={12}>
                                <Grid container spacing={2} justifyContent="flex-start">
                                    <Grid item>
                                        <Button
                                            variant="contained"
                                            type="submit"
                                            color="success"
                                            disabled={!formRenderProps.allowSubmit || isLoading}
                                        >
                                            {getTranslatedLabel(`${localizationKey}.create`, "Create")}
                                        </Button>
                                    </Grid>
                                    <Grid item>
                                        <Button
                                            variant="contained"
                                            color="error"
                                            onClick={onClose}
                                            disabled={isLoading}
                                        >
                                            {getTranslatedLabel(`${localizationKey}.back`, "Back")}
                                        </Button>
                                    </Grid>
                                </Grid>
                            </Grid>
                        </Grid>
                    </FormElement>
                )}
            />
        </>
    );
};

export default NewSalesInvoice;