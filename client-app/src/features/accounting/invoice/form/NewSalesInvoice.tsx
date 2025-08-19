import React, {useRef, useState} from "react";
import { Form, FormElement, Field } from "@progress/kendo-react-form";
import { Button, Grid } from "@mui/material";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { FormComboBoxVirtualCustomer } from "../../../../app/common/form/FormComboBoxVirtualCustomer";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {
    useFetchCompaniesQuery,
    useFetchInvoiceTypesQuery
} from "../../../../app/store/configureStore";
import {requiredValidator} from "../../../../app/common/form/Validators";
import useInvoice from "../hook/useInvoice";
import LoadingComponent from "../../../../app/layout/LoadingComponent";

interface Props {
    onClose: () => void;
}

const NewSalesInvoice = ({ onClose }: Props) => {
    const formRef = useRef<Form | null>(null);
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.invoices.form";
    const { data: invoiceTypes } = useFetchInvoiceTypesQuery(undefined);
    const { data: companies } = useFetchCompaniesQuery(undefined);
    const [isLoading, setIsLoading] = useState(false);

    const { handleCreate } = useInvoice({
        editMode: 0,
        setIsLoading
    });
    
    const salesInvoiceTypes = invoiceTypes?.filter(
        (type: any) => type.invoiceTypeId === "SALES_INVOICE" || type.parentTypeId === "SALES_INVOICE"
    ) || [];

    const handleSubmit = async (values) => {
        try {
            await handleCreate({
                ...values,
                statusId: "INVOICE_IN_PROCESS"
            });
            onClose();
        } catch (e) {
            console.error("Error creating sales invoice:", e);
        }
    };
    
    console.log('companies:', companies);

    return (
        <Form
            onSubmit={handleSubmit}
            ref={formRef}
            render={(formRenderProps) => (
                <FormElement>
                    <Grid container spacing={2} flexDirection="column">
                        <Grid item xs={4}>
                            <Field
                                name="invoiceTypeId"
                                id="invoiceTypeId"
                                label={getTranslatedLabel(`${localizationKey}.invoiceType`, "Invoice Type")}
                                component={MemoizedFormDropDownList}
                                data={salesInvoiceTypes}
                                dataItemKey="invoiceTypeId"
                                textField="description"
                                defaultValue="SALES_INVOICE"
                                validator={requiredValidator}
                            />
                        </Grid>
                        <Grid item xs={4}>
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
                        <Grid item xs={4}>
                            <Field
                                name="partyId"
                                id="partyIdTo"
                                label={getTranslatedLabel(`${localizationKey}.partyIdTo`, "To Party ID")}
                                component={FormComboBoxVirtualCustomer}
                                validator={requiredValidator}
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
                    {isLoading && (
                        <Grid item xs={12}>
                            <LoadingComponent
                                message={getTranslatedLabel(
                                    `${localizationKey}.loading`,
                                    "Processing Invoice..."
                                )}
                            />
                        </Grid>
                    )}
                </FormElement>
            )}
        />
    );
};

export default NewSalesInvoice;