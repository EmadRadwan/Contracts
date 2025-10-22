import {useCallback, useEffect, useMemo, useState} from "react";
import { Field, Form, FormElement, FormRenderProps } from "@progress/kendo-react-form";
import { Button, FormControlLabel, Grid, Radio, RadioGroup } from "@mui/material";
import { v4 as uuidv4 } from "uuid";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { FormComboBoxVirtualProject } from "../../../app/common/form/FormComboBoxVirtualProject";
import { MultiPaymentItem } from "../../../app/models/project/MultiPaymentItem";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { requiredValidator, percentageValidator } from "../../../app/common/form/Validators";
import { MemoizedFormDropDownList } from "../../../app/common/form/MemoizedFormDropDownList";
import { useFetchSubProjectsQuery } from "../../../app/store/apis/multiPaymentCertificateApi";
import FormInput from "../../../app/common/form/FormInput";
import { FormSimpleComboBoxRawMaterialVirtual } from "../../../app/common/form/FormSimpleComboBoxRawMaterialVirtual";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import { ComboBoxChangeEvent } from "@progress/kendo-react-dropdowns";
import {FormSimpleComboBoxServiceVirtual} from "../../../app/common/form/FormSimpleComboBoxServiceVirtual";
import {FormComboBoxVirtualSupplierMultiColumn} from "../../../app/common/form/FormComboBoxVirtualSupplierMultiColumn";
import {FormComboBoxVirtualContractor} from "../../../app/common/form/FormComboBoxVirtualContractor";

interface Props {
    multiPaymentItem?: MultiPaymentItem;
    editMode: number; // 1: add, 2: edit
    onClose: () => void;
    workEffortId: string;
    formEditMode: number;
    addItem: (item: MultiPaymentItem) => void;
    updateItem: (item: MultiPaymentItem) => void;
}

export default function MultiPaymentItemForm({ multiPaymentItem, editMode, onClose, workEffortId, formEditMode, addItem, updateItem }: Props) {

    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "projects.multiPaymentCertificate.itemForm";
    const [selectedProjectId, setSelectedProjectId] = useState<string>(
        multiPaymentItem?.projectId && multiPaymentItem.projectId !== "" ? multiPaymentItem.projectId : ""
    );
    const [formKey, setFormKey] = useState<number>(1);
    const [discountMode, setDiscountMode] = useState<"value" | "percentage">(multiPaymentItem?.discountMode || "value");

    const { data: subProjects, isLoading: subProjectsLoading } = useFetchSubProjectsQuery(
        selectedProjectId || "",
        { skip: !selectedProjectId }
    );
    
    console.log('subProjects', subProjects)

    const itemTypes = useMemo(
        () => [
            { itemType: "MATERIALS", description: "المواد" },
            { itemType: "LABOR", description: "العمالة" },
            { itemType: "EQUIPMENT", description: "المعدات" },
            { itemType: "EXPENSES", description: "المصروفات" },
        ],
        []
    );

    const initialValues = useMemo((): Partial<MultiPaymentItem> => ({
        itemId: multiPaymentItem?.itemId || "",
        workEffortId: workEffortId || "",
        projectId: multiPaymentItem?.projectId
            ? { projectId: multiPaymentItem.projectId, projectName: multiPaymentItem.projectName || "" }
            : null,
        subProjectId: multiPaymentItem?.subProjectId || "",
        subProjectName: multiPaymentItem?.subProjectName || "",
        itemType: multiPaymentItem?.itemType || "",
        serviceId: multiPaymentItem?.serviceId || "",
        serviceName: multiPaymentItem?.serviceName || "",
        productId: multiPaymentItem?.productId || "",
        productName: multiPaymentItem?.productName || "",
        description: multiPaymentItem?.description || "",
        amount: multiPaymentItem?.amount || 0,
        discount: multiPaymentItem?.discount || 0,
        discountMode: multiPaymentItem?.discountMode || "value",
        transportationExpenses: multiPaymentItem?.transportationExpenses || 0,
        gratuities: multiPaymentItem?.gratuities || 0,
        total: multiPaymentItem?.total || 0,
        partyIdSupplier: multiPaymentItem?.partyIdSupplier || "",
        partyIdContractor: multiPaymentItem?.partyIdContractor || "",
    }), [multiPaymentItem, workEffortId]);

    const partyValidator = (values: Partial<MultiPaymentItem>): KeyValue<string> | undefined => {
        const hasSupplier = values.partyIdSupplier && values.partyIdSupplier !== "";
        const hasContractor = values.partyIdContractor && values.partyIdContractor !== "";

        if (hasSupplier && hasContractor) {
            return {
                VALIDATION_SUMMARY: getTranslatedLabel(
                    `${localizationKey}.validation.partyExclusive`,
                    "Please select either a Supplier or a Contractor, not both."
                ),
            };
        }
        // Optionally, require at least one to be filled
        if (!hasSupplier && !hasContractor) {
            return {
                VALIDATION_SUMMARY: getTranslatedLabel(
                    `${localizationKey}.validation.partyRequired`,
                    "At least one of Supplier or Contractor must be selected."
                ),
            };
        }
        return undefined;
    };

    const calculateTotals = useCallback(
        (valueGetter: FormRenderProps["valueGetter"], onChange: FormRenderProps["onChange"]) => {
            const amount = Number(valueGetter("amount") || 0);
            const discountInput = Number(valueGetter("discount") || 0);
            const discount = discountMode === "value" ? discountInput : (discountInput / 100) * amount;
            const transportationExpenses = Number(valueGetter("transportationExpenses") || 0);
            const gratuities = Number(valueGetter("gratuities") || 0);
            const total = Math.max(0, Math.round((amount - discount + transportationExpenses + gratuities) * 1000) / 1000);

            // Only update total if it has changed to avoid unnecessary re-renders
            if (valueGetter("total") !== total) {
                onChange("total", { value: total });
            }
        },
        [discountMode]
    );


    const handleDiscountModeChange = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => {
            const newMode = event.target.value as "value" | "percentage";
            setDiscountMode(newMode);
            onChange("discount", { value: 0 });
        },
        []
    );

    const handleSubmit = useCallback(
        (values: Partial<MultiPaymentItem>) => {

            const selectedSubProject = subProjects?.find(
                (subProject) => subProject.workEffortId === values.subProjectId
            );
            const subProjectName = selectedSubProject?.subProjectName || values.subProjectName || "";

            const selectedItemType = itemTypes.find(
                (type) => type.itemType === values.itemType
            );
            const itemTypeDescription = selectedItemType?.description || values.description || "";


            const serializedValues: MultiPaymentItem = {
                itemId: values.itemId || uuidv4(),
                workEffortId: workEffortId || "",
                projectId: (values.projectId as any)?.projectId || "",
                projectName: (values.projectId as any)?.projectName || "",
                subProjectId: values.subProjectId || "",
                subProjectName: subProjectName || "",
                itemType: values.itemType || "",
                itemTypeDescription: itemTypeDescription,
                serviceId: values.serviceId.ProductId || "",
                serviceName: values.serviceId.ProductName || "",
                productId: values.productId.ProductId || "",
                productName: values.productId.ProductName || "",
                description: values.description || "",
                amount: Number(values.amount) || 0,
                discount: Number(values.discount) || 0,
                discountMode: discountMode,
                transportationExpenses: Number(values.transportationExpenses) || 0,
                gratuities: Number(values.gratuities) || 0,
                total: Number(values.total) || 0,
                partyIdSupplier: values.partyIdSupplier?.fromPartyId || "",
                partyIdSupplierName: values.partyIdSupplier?.fromPartyName || "",
                partyIdContractor: values.partyIdContractor?.fromPartyId || "",
                partyIdContractorName: values.partyIdContractor?.fromPartyName || "",
            };
            if (editMode === 1) {
                addItem(serializedValues);
            } else {
                updateItem(serializedValues);
            }
            setFormKey((prev) => prev + 1);
            onClose();
        },
        [addItem, updateItem, editMode, workEffortId, discountMode, onClose, subProjects]
    );

    const handleProjectChange = useCallback(
        (e: ComboBoxChangeEvent, onChange: FormRenderProps["onChange"]) => {
            const newProjectId = e.value?.projectId || "";
            setSelectedProjectId(newProjectId);
            onChange("projectId", { value: e.value });
            onChange("subProjectId", { value: null });
        },
        []
    );

    const TotalUpdater = ({ formRenderProps }: { formRenderProps: FormRenderProps }) => {
        const { valueGetter, onChange } = formRenderProps;

        const handleFieldChange = useCallback(
            (field: string) => (event: any) => {
                onChange(field, { value: event.value });
                calculateTotals(valueGetter, onChange);
            },
            [valueGetter, onChange, calculateTotals]
        );

        return (
            <>
                <Grid container spacing={2}>
                    <Grid item xs={6}>
                        <Field
                            id="amount"
                            name="amount"
                            label={getTranslatedLabel(`${localizationKey}.amount`, "Amount *")}
                            component={FormNumericTextBox}
                            format="n2"
                            min={0}
                            validator={requiredValidator}
                            disabled={formEditMode > 3 || subProjectsLoading}
                            onChange={handleFieldChange("amount")}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        <Field
                            id="discount"
                            name="discount"
                            label={getTranslatedLabel(`${localizationKey}.discount`, `Discount (${discountMode})`)}
                            component={FormNumericTextBox}
                            format={discountMode === "percentage" ? "n0" : "n2"}
                            min={0}
                            max={discountMode === "percentage" ? 100 : undefined}
                            validator={discountMode === "percentage" ? percentageValidator : undefined}
                            disabled={formEditMode > 3 || subProjectsLoading}
                            onChange={handleFieldChange("discount")}
                        />
                        <Grid
                            container
                            sx={{
                                display: 'flex',
                                flexDirection: 'row',
                                flexWrap: 'nowrap', // Prevent wrapping
                                gap: 2,
                                mt: 1,
                                alignItems: 'center', // Vertically align radio buttons
                            }}
                            className="horizontal-radio-group" // Fallback for custom CSS
                        >
                            <RadioGroup
                                row
                                value={discountMode}
                                onChange={(e) => handleDiscountModeChange(e, formRenderProps.onChange)}
                                sx={{
                                    display: 'flex',
                                    flexDirection: 'row',
                                    flexWrap: 'nowrap', // Reinforce no wrapping
                                }}
                            >
                                <FormControlLabel
                                    value="value"
                                    control={<Radio disabled={formEditMode > 3 || subProjectsLoading} />}
                                    label={getTranslatedLabel(`${localizationKey}.discountValue`, "Value")}
                                    sx={{ minWidth: '100px' }} // Ensure label has enough space
                                />
                                <FormControlLabel
                                    value="percentage"
                                    control={<Radio disabled={formEditMode > 3 || subProjectsLoading} />}
                                    label={getTranslatedLabel(`${localizationKey}.discountPercentage`, "Percentage")}
                                    sx={{ minWidth: '100px' }} // Ensure label has enough space
                                />
                            </RadioGroup>
                        </Grid>
                    </Grid>
                    <Grid item xs={6}>
                        <Field
                            id="transportationExpenses"
                            name="transportationExpenses"
                            label={getTranslatedLabel(`${localizationKey}.transportationExpenses`, "Transportation Expenses")}
                            component={FormNumericTextBox}
                            format="n2"
                            min={0}
                            disabled={formEditMode > 3 || subProjectsLoading}
                            onChange={handleFieldChange("transportationExpenses")}
                        />
                    </Grid>
                    <Grid item xs={6}>
                        <Field
                            id="gratuities"
                            name="gratuities"
                            label={getTranslatedLabel(`${localizationKey}.gratuities`, "Gratuities")}
                            component={FormNumericTextBox}
                            format="n2"
                            min={0}
                            disabled={formEditMode > 3 || subProjectsLoading}
                            onChange={handleFieldChange("gratuities")}
                        />
                    </Grid>
                </Grid>
            </>
        );
    };

    return (
        <Form
            initialValues={initialValues}
            key={formKey}
            onSubmit={handleSubmit}
            validator={partyValidator}
            render={(formRenderProps: FormRenderProps) => {
                const isMaterials = formRenderProps.valueGetter("itemType") === "MATERIALS";
                
                return (
                    <FormElement>
                        {formRenderProps.visited &&
                            formRenderProps.errors &&
                            formRenderProps.errors.VALIDATION_SUMMARY && (
                                <div className={"k-messagebox k-messagebox-error"}>
                                    {formRenderProps.errors.VALIDATION_SUMMARY}
                                </div>
                            )}
                        <fieldset className="k-form-fieldset" disabled={subProjectsLoading}>
                            <Grid container spacing={2}>
                                <Field name="itemId" component="input" type="hidden" />
                                <Field name="workEffortId" component="input" type="hidden" />
                                <Grid item xs={4}>
                                    <Field
                                        id="projectId"
                                        name="projectId"
                                        label={getTranslatedLabel(`${localizationKey}.project`, "Project *")}
                                        component={FormComboBoxVirtualProject}
                                        validator={requiredValidator}
                                        textField="projectName"
                                        dataItemKey="projectId"
                                        disabled={formEditMode > 3 || subProjectsLoading}
                                        value={formRenderProps.valueGetter("projectId")}
                                        onChange={(e: ComboBoxChangeEvent) => handleProjectChange(e, formRenderProps.onChange, formRenderProps.valueGetter)}
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <Field
                                        id="subProjectId"
                                        name="subProjectId"
                                        label={getTranslatedLabel(`${localizationKey}.subProject`, "Sub-Project *")}
                                        component={MemoizedFormDropDownList2}
                                        data={subProjects || []}
                                        dataItemKey="workEffortId"
                                        textField="subProjectName"
                                        disabled={formEditMode > 3 || subProjectsLoading}
                                        value={
                                            formRenderProps.valueGetter("subProjectId")
                                                ? {
                                                    workEffortId: formRenderProps.valueGetter("subProjectId"),
                                                    subProjectName: formRenderProps.valueGetter("subProjectName") || "",
                                                }
                                                : null
                                        }
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <Field
                                        id="itemType"
                                        name="itemType"
                                        label={getTranslatedLabel(`${localizationKey}.itemType`, "Item Type *")}
                                        component={MemoizedFormDropDownList}
                                        dataItemKey="itemType"
                                        textField="description"
                                        data={itemTypes}
                                        validator={requiredValidator}
                                        disabled={formEditMode > 3 || subProjectsLoading}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    <Field
                                        id="serviceId"
                                        name="serviceId"
                                        label={getTranslatedLabel(`${localizationKey}.service`, "Service *")}
                                        component={FormSimpleComboBoxServiceVirtual}
                                        validator={requiredValidator}
                                        textField="productName"
                                        dataItemKey="productId"
                                        disabled={formEditMode > 3 || subProjectsLoading}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    {isMaterials && (
                                        <Field
                                            id="productId"
                                            name="productId"
                                            label={getTranslatedLabel(`${localizationKey}.product`, "Product *")}
                                            component={FormSimpleComboBoxRawMaterialVirtual}
                                            validator={requiredValidator}
                                            textField="productName"
                                            dataItemKey="productId"
                                            disabled={formEditMode > 3 || subProjectsLoading}
                                        />
                                    )}
                                </Grid>
                                <Grid container item xs={12} spacing={2}>
                                    <Grid item xs={6}>
                                        <Field
                                            id="partyIdSupplier"
                                            name="partyIdSupplier"
                                            component={FormComboBoxVirtualSupplierMultiColumn}
                                            label={getTranslatedLabel("projects.certificate.form.supplier", "Supplier")}
                                            valueField="fromPartyId"
                                            textField="fromPartyName"
                                        />
                                    </Grid>
                                    <Grid item xs={6}>
                                        <Field
                                            id="partyIdContractor"
                                            name="partyIdContractor"
                                            component={FormComboBoxVirtualContractor}
                                            label={getTranslatedLabel("projects.certificate.form.contractor", "Contractor")}
                                            valueField="fromPartyId"
                                            textField="fromPartyName"
                                        />
                                    </Grid>
                                </Grid>
                                
                                <Grid item xs={12}>
                                    <Field
                                        id="description"
                                        name="description"
                                        label={getTranslatedLabel(`${localizationKey}.description`, "Description")}
                                        component={FormInput}
                                        disabled={formEditMode > 3 || subProjectsLoading}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    <TotalUpdater formRenderProps={formRenderProps} />
                                </Grid>
                                <Grid item xs={6}>
                                    <Field
                                        id="total"
                                        name="total"
                                        label={getTranslatedLabel(`${localizationKey}.total`, "Total")}
                                        component={FormNumericTextBox}
                                        format="n2"
                                        disabled
                                    />
                                </Grid>
                                <Grid container spacing={2} justifyContent="flex-start">
                                    <Grid item xs={2}>
                                        <Button
                                            type="submit"
                                            variant="contained"
                                            disabled={!formRenderProps.valid || subProjectsLoading}
                                            sx={{ mt: 2, ml: 2 }}
                                        >
                                            {getTranslatedLabel(
                                                `${localizationKey}.${editMode === 1 ? "create" : "update"}`,
                                                editMode === 1 ? "Create Item" : "Update Item"
                                            )}
                                        </Button>
                                    </Grid>
                                    <Grid item xs={2}>
                                        <Button
                                            sx={{ mt: 2 }}
                                            onClick={onClose}
                                            color="error"
                                            variant="contained"
                                            disabled={subProjectsLoading}
                                        >
                                            {getTranslatedLabel("general.cancel", "Cancel")}
                                        </Button>
                                    </Grid>
                                </Grid>
                            </Grid>
                        </fieldset>
                    </FormElement>
                );
            }}
        />
    );
}