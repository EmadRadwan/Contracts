import React, { useState, useEffect } from "react";
import { useAppSelector, useFetchProductUOMsQuery } from "../../../../app/store/configureStore";
import { Form, FormElement, Field } from "@progress/kendo-react-form";
import { Grid, Button, Box } from "@mui/material";
import { LoadingButton } from "@mui/lab";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import {
    FormMultiColumnComboBoxVirtualFacilityProduct
} from "../../../../app/common/form/FormMultiColumnComboBoxVirtualProduct";
import { FormDropDownTreeGlAccount2 } from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import {
    useFetchGlAccountOrganizationHierarchyLovQuery,
} from "../../../../app/store/apis";
import useInvoiceItem from "../hook/useInvoiceItem";
import { InvoiceItem } from "../../../../app/models/accounting/invoiceItem";
import {
    useFetchInvoiceItemTypesByInvoiceIdQuery
} from "../../../../app/store/apis/invoice/invoiceItemsApi";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";

interface Props {
    invoiceItem?: InvoiceItem;
    editMode: number; // 1 for create, 2 for edit
    onClose: () => void;
    invoiceId: string | undefined;
    refreshTotal?: () => Promise<void>;
}

const EditInvoiceItem: React.FC<Props> = ({ invoiceItem, editMode, onClose, invoiceId, refreshTotal }) => {
    const formRef = React.useRef<any>();
    const { user } = useAppSelector((state) => state.account);
    const [isLoading, setIsLoading] = useState(false);
    const [showProductField, setShowProductField] = useState(false);
    const companyId = user?.organizationPartyId || "";
    const { getTranslatedLabel } = useTranslationHelper();
    const loc = "accounting.invoices.edit.item";

    const { data: unitsOfMeasure, isFetching: isUoMsFetching } = useFetchProductUOMsQuery(undefined);
    const uomData = unitsOfMeasure?.map((uom: any) => ({
        uomId: uom.quantityUomId,
        description: uom.description
    })) || [];

    const { data: invoiceItemTypes, isLoading: isQueryLoading, error } = useFetchInvoiceItemTypesByInvoiceIdQuery(
        {
            invoiceId: invoiceId || "",
        },
        { skip: !invoiceId }
    );

    const { data: glAccounts } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, {
        skip: companyId === undefined,
    });

    const labels = {
        invoiceItemType: getTranslatedLabel(`${loc}.invoiceItemType`, "Invoice Item Type") + " *",
        product:        getTranslatedLabel(`${loc}.product`,        "Product"),
        description:    getTranslatedLabel(`${loc}.description`,    "Description"),
        quantity:       getTranslatedLabel(`${loc}.quantity`,       "Quantity") + " *",
        amount:         getTranslatedLabel(`${loc}.amount`,         "Amount") + " *",
        uom:            getTranslatedLabel(`${loc}.uom`,            "UOM"),
        overrideGlAccount: getTranslatedLabel(`${loc}.overrideGlAccount`, "Override GL Account"),
        add:            getTranslatedLabel(`${loc}.actions.add`,    "Add"),
        update:         getTranslatedLabel(`${loc}.actions.update`, "Update"),
        cancel:         getTranslatedLabel("general.cancel",        "Cancel"),
    };


    // REFACTOR: Added logic to determine if Product field should be shown
    // Purpose: Conditionally display Product field based on invoiceItemTypeId
    // Improvement: Reduces clutter for non-product-related items, aligns with business needs
    const productRelatedItemTypes = [
        "PINV_PROD_ITEM",
        "INV_PROD_ITEM",
        "PINV_DPROD_ITEM",
        "PINV_FDPROD_ITEM",
        "PINV_FPROD_ITEM",
        "PINV_INVPRD_ITEM",
        "PINV_SPROD_ITEM",
        "PINV_SUPLPRD_ITEM",
        "INV_LAND_ITEM",
        "INV_LAND_PARTNERSHIP_ITEM"
    ];

    // REFACTOR: Updated useInvoiceItem call to pass invoiceId prop
    // Purpose: Aligns with updated useInvoiceItem hook that uses invoiceId prop
    // Improvement: Ensures invoiceId is consistently used, removing Redux dependency
    const { handleCreate, isCreateItemLoading, isUpdateItemLoading } = useInvoiceItem({
        editMode,
        invoiceItem,
        invoiceId: invoiceId || '',
        setIsLoading,
    });

    // REFACTOR: Simplified handleSubmitData to avoid redundant data transformation
    // Purpose: Leverages useInvoiceItem's handleCreate to format data
    // Improvement: Reduces duplication and improves maintainability
    const handleSubmitData = async (data: any) => {
        try {
            setIsLoading(true);
            await handleCreate(data); // Let useInvoiceItem handle productId and invoiceId
            refreshTotal?.();
            onClose();
        } catch (e) {
            console.error(e);
        } finally {
            setIsLoading(false);
        }
    };

    // REFACTOR: Added effect to toggle Product field based on invoiceItemTypeId
    // Purpose: Dynamically show/hide Product field based on form state
    // Improvement: Enhances UX by only showing relevant fields
    useEffect(() => {
        if (formRef.current) {
            const invoiceItemTypeId = formRef.current.values?.invoiceItemTypeId;
            setShowProductField(productRelatedItemTypes.includes(invoiceItemTypeId));
        }
    }, [formRef.current?.values?.invoiceItemTypeId]);

    // REFACTOR: Removed dependency on selectedInvoiceTypeId
    // Purpose: Simplifies logic by relying on invoiceItemTypes query
    // Improvement: Reduces Redux dependency and avoids potential inconsistencies
    if (!invoiceItemTypes) {
        return (
            <Box p={2}>
                <p>Loading invoice item types...</p>
                <Button onClick={onClose} color="error" variant="outlined">
                    Cancel
                </Button>
            </Box>
        );
    }

    return (
        <Form
            ref={formRef}
            initialValues={invoiceItem || undefined}
            onSubmit={handleSubmitData}
            render={(formRenderProps) => (
                <FormElement>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <Field
                                id="invoiceItemTypeId"
                                name="invoiceItemTypeId"
                                label={labels.invoiceItemType}
                                component={MemoizedFormDropDownList}
                                autoComplete="off"
                                dataItemKey="invoiceItemTypeId"
                                textField="description"
                                data={invoiceItemTypes || []}
                                validator={requiredValidator}
                                //disabled={editMode === 2}
                            />
                        </Grid>

                        {showProductField && (
                            <Grid item xs={12}>
                                <Field
                                    id="productId"
                                    name="productId"
                                    label={labels.product}
                                    component={FormMultiColumnComboBoxVirtualFacilityProduct}
                                    autoComplete="off"
                                    columnWidth="500px"
                                />
                            </Grid>
                        )}

                        <Grid item xs={12}>
                            <Field
                                id="description"
                                name="description"
                                label={labels.description}
                                component={FormTextArea}
                                autoComplete="off"
                            />
                        </Grid>

                        <Grid item xs={6} sm={4}>
                            <Field
                                id="quantity"
                                name="quantity"
                                label={labels.quantity}
                                format="n0"
                                min={1}
                                component={FormNumericTextBox}
                                validator={requiredValidator}
                            />
                        </Grid>

                        <Grid item xs={6} sm={4}>
                            <Field
                                id="amount"
                                name="amount"
                                label={labels.amount}
                                format="n2"
                                min={0}
                                component={FormNumericTextBox}
                                validator={requiredValidator}
                            />
                        </Grid>

                        <Grid item xs={6} sm={4}>
                            <Field
                                id="uomId"
                                name="uomId"
                                label={labels.uom}
                                component={MemoizedFormDropDownList}
                                autoComplete="off"
                                dataItemKey="uomId"
                                textField="description"
                                data={uomData}
                                disabled={isUoMsFetching}
                            />
                        </Grid>

                        <Grid item xs={12}>
                            <Field
                                id="overrideGlAccountId"
                                name="overrideGlAccountId"
                                label={labels.overrideGlAccount}
                                data={glAccounts || []}
                                component={FormDropDownTreeGlAccount2}
                                dataItemKey="glAccountId"
                                textField="text"
                                selectField="selected"
                                expandField="expanded"
                            />
                        </Grid>

                        <Grid item xs={12}>
                            <Box display="flex" justifyContent="flex-end" gap={2} mt={2}>
                                <LoadingButton
                                    size="large"
                                    type="submit"
                                    loading={isLoading || isCreateItemLoading || isUpdateItemLoading}
                                    variant="outlined"
                                    disabled={!formRenderProps.allowSubmit}
                                    sx={{ minWidth: 120 }}
                                >
                                    {editMode === 2 ? labels.update : labels.add}
                                </LoadingButton>
                                <Button
                                    onClick={onClose}
                                    size="large"
                                    color="error"
                                    variant="outlined"
                                    sx={{ minWidth: 120 }}
                                >
                                    {labels.cancel}
                                </Button>
                            </Box>
                        </Grid>
                    </Grid>
                </FormElement>
            )}
        />
    );
};

export default EditInvoiceItem;