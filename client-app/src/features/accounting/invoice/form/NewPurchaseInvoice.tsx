import React, {useRef, useState} from "react";
import { Form, FormElement, Field } from "@progress/kendo-react-form";
import { Button, Grid } from "@mui/material";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { FormComboBoxVirtualParty } from "../../../../app/common/form/FormComboBoxVirtualParty";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {
     useFetchCompaniesQuery,
    useFetchInvoiceTypesQuery
} from "../../../../app/store/configureStore";
import {requiredValidator} from "../../../../app/common/form/Validators";
import useInvoice from "../hook/useInvoice";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import FormInput from "../../../../app/common/form/FormInput";
import FormTextArea from "../../../../app/common/form/FormTextArea";

interface Props {
    onClose: () => void;
}

const NewPurchaseInvoice = ({ onClose }: Props) => {
    const formRef = useRef<Form | null>(null);
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.invoices.display.form";
    const { data: invoiceTypes } = useFetchInvoiceTypesQuery(undefined);
    const { data: companies } = useFetchCompaniesQuery(undefined);
    const [isLoading, setIsLoading] = useState(false);


    const { handleCreate } = useInvoice(null);

    const purchaseInvoiceTypes = invoiceTypes?.filter(
        (type: any) => type.invoiceTypeId === "PURCHASE_INVOICE" || type.parentTypeId === "PURCHASE_INVOICE"
    ) || [];

    const handleSubmit = async (values) => {
        try {
            await handleCreate({
                ...values,
                statusId: "INVOICE_IN_PROCESS"
            });
            onClose();
        } catch (e) {
            console.error("Error creating purchase invoice:", e);
        }
    };

    return (
        <>
            {isLoading && (
                <div style={{
                    position: 'fixed',
                    top: 0,
                    left: 0,
                    width: '100%',
                    height: '100%',
                    backgroundColor: 'rgba(0, 0, 0, 0.5)', // Semi-transparent background
                    display: 'flex',
                    justifyContent: 'center',
                    alignItems: 'center',
                    zIndex: 1000
                }}>
                    <LoadingComponent
                        message={getTranslatedLabel(
                            `${localizationKey}.loading`,
                            "Processing Invoice..."
                        )}
                    />
                </div>
            )}
            <Form
                onSubmit={handleSubmit}
                ref={formRef}
                render={(formRenderProps) => (
                    <FormElement>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={6}>
                                <Field
                                    name="invoiceTypeId"
                                    id="invoiceTypeId"
                                    label={getTranslatedLabel(`${localizationKey}.invoice-type`, "Invoice Type")}
                                    component={MemoizedFormDropDownList}
                                    data={purchaseInvoiceTypes}
                                    dataItemKey="invoiceTypeId"
                                    textField="description"
                                    defaultValue="PURCHASE_INVOICE"
                                    validator={requiredValidator}
                                />
                            </Grid>
                            <Grid item xs={12} sm={6}>
                                <Field
                                    name="organizationPartyId"
                                    id="organizationPartyId"
                                    label={getTranslatedLabel(`${localizationKey}.organizationPartyId`, "Organization Party ID")}
                                    component={MemoizedFormDropDownList}
                                    data={companies || []}
                                    dataItemKey="organizationPartyId"
                                    textField="organizationPartyName"
                                    validator={requiredValidator}
                                />
                            </Grid>
                            <Grid item xs={12} sm={6}>
                                <Field
                                    name="partyIdFrom"
                                    id="partyIdFrom"
                                    label={getTranslatedLabel(`${localizationKey}.from-party`, "From Party ID")}
                                    component={FormComboBoxVirtualParty}
                                    validator={requiredValidator}
                                />
                            </Grid>
                            <Grid item xs={12} sm={6}>
                                <Field
                                    id="invoiceDate"
                                    name="invoiceDate"
                                    label={getTranslatedLabel(`${localizationKey}.invoice-date`, "Invoice Date")}
                                    component={FormDatePicker}
                                    format="dd MMM yyyy"
                                    validator={requiredValidator}
                                />
                            </Grid>

                            <Grid item xs={12} sm={6}>
                                <Field
                                    id="referenceNumber"
                                    name="referenceNumber"
                                    label={getTranslatedLabel(`${localizationKey}.referenceNumber`, "Reference / Invoice No")}
                                    component={FormInput}
                                />
                            </Grid>

                            <Grid item xs={12}>
                                <Field
                                    id="description"
                                    name="description"
                                    label={getTranslatedLabel(`${localizationKey}.description`, "Description / Notes")}
                                    component={FormTextArea}   // ← you need to implement or import this
                                    multiline
                                    rows={3}
                                />
                            </Grid>
                            <Field name="statusId" component="input" type="hidden" value="INVOICE_IN_PROCESS" />
                            <Field name="currencyUomId" component="input" type="hidden" value="USD" />
                            <Grid item xs={4}>
                                {/* REFACTOR: Added nested Grid container to control button spacing */}
                                <Grid container spacing={2} direction="row">
                                    <Grid item>
                                        <Button
                                            variant="contained"
                                            type="submit"
                                            color="success"
                                            disabled={!formRenderProps.allowSubmit}
                                        >
                                            {getTranslatedLabel(`${localizationKey}.create`, "Create")}
                                        </Button>
                                    </Grid>
                                    <Grid item>
                                        <Button
                                            variant="contained"
                                            color="error"
                                            onClick={onClose}
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

export default NewPurchaseInvoice;