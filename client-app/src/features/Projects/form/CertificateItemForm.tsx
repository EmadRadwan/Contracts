import { Form, FormRenderProps } from "@progress/kendo-react-form";
import React, { useCallback, useMemo, useRef, useState } from "react";
import { useAppSelector, useFetchFacilitiesQuery } from "../../../app/store/configureStore";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import useCertificateItem from "../hook/useCertificateItem";
import { percentageValidator, requiredValidator } from "../../../app/common/form/Validators";
import SupplyProcurementForm from "./SupplyProcurementForm";
import WorkmanshipContractingForm from "./WorkmanshipContractingForm";
import ContractorPurchaseForm from "./ContractorPurchaseForm";
import CompanySupplyForm from "./CompanySupplyForm";

interface Props {
    certificateItem?: CertificateItem;
    editMode: number; // 1: add, 2: edit
    onClose: () => void;
    formEditMode: number; // 0: view, 1: create, 2: CREATED, 3: APPROVED, 4: COMPLETED
}

export default function CertificateItemForm({
                                                certificateItem,
                                                editMode,
                                                onClose,
                                                formEditMode,
                                            }: Props) {
    const { currentCertificateType } = useAppSelector((state) => state.certificateUi);
    const [discountMode, setDiscountMode] = useState<"value" | "percentage">("value");
    const [insuranceMode, setInsuranceMode] = useState<"value" | "percentage">("value");
    const [calculatedInsurance, setCalculatedInsurance] = useState<number>(0);
    const { getTranslatedLabel } = useTranslationHelper();

    console.log('certificateItem', certificateItem)
    const calculateTotals = useCallback(
        (valueGetter: FormRenderProps["valueGetter"]) => {
            const quantity = Number(valueGetter("quantity") || 0);
            const price = Number(valueGetter("unitPrice") || 0);
            const total = Math.round(quantity * price * 1000) / 1000;
            let finalTotal = total;
            let deserved = 0;
            let insurance = 0;
            let discount = 0;
            if (["SUPPLY_PROCUREMENT_CERTIFICATE", "EXTERNAL_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType)) {
                const discountInput = Number(valueGetter("discount") || 0);
                discount = discountMode === "value" ? discountInput : (discountInput / 100) * total;
                const transportationExpenses = Number(valueGetter("transportationExpenses") || 0);
                const gratuities = Number(valueGetter("gratuities") || 0);
                finalTotal = Math.max(0, Math.round((total - discount + transportationExpenses + gratuities) * 1000) / 1000);
            } else if (["COMPANY_SUPPLY_SALE_CERTIFICATE", "CONTRACTOR_PURCHASE_CERTIFICATE"].includes(currentCertificateType)) {
                const transportationExpenses = Number(valueGetter("transportationExpenses") || 0);
                const gratuities = Number(valueGetter("gratuities") || 0);
                finalTotal = Math.max(0, Math.round((total + transportationExpenses + gratuities) * 1000) / 1000);
            } else if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
                deserved = Math.max(0, Math.round((total - Number(valueGetter("deductions") || 0)) * 1000) / 1000);
                const insuranceInput = Number(valueGetter("insurance") || 0);
                insurance = insuranceMode === "value" ? insuranceInput : (insuranceInput / 100) * deserved;
                insurance = Math.round(insurance * 1000) / 1000;
                finalTotal = deserved;
            }
            const net = currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                ? Math.max(0, Math.round((deserved - insurance) * 1000) / 1000)
                : finalTotal;
            return { total, finalTotal, net, deserved, insurance, discount, transportationExpenses: Number(valueGetter("transportationExpenses") || 0), gratuities: Number(valueGetter("gratuities") || 0) };
        },
        [currentCertificateType, discountMode, insuranceMode]
    );



        // Set type-specific defaults
    // CertificateItemForm.tsx
    const deserializedInitValue = useMemo((): Partial<CertificateItem> => {
        const baseDefaultValues: Partial<CertificateItem> = {
            productId: "",
            productName: "",
            uomId: "",
            uomName: "",
            description: "",
            quantity: 0,
            unitPrice: 0,
            totalAmount: 0,
            discount: 0,
            insurance: 0,
            deductions: 0,
            deserved: 0,
            net: 0,
            procurementDate: new Date(),
            isContractorPurchased: false,
            isDeleted: false,
            achievementPercentage: 0,
            transportationExpenses: 0,
            gratuities: 0,
            completionPercentage: 0,
            notes: "",
            workEffortId: "",
            workEffortParentId: "",
        };

        // REFACTOR: Only include type-specific defaults that won't overwrite certificateItem values
        // Context: Previously, typeSpecificDefaults overwrote discount, transportationExpenses, and gratuities.
        // Improvement: Exclude these fields when certificateItem exists to preserve its values.
        const typeSpecificDefaults: Partial<CertificateItem> = {
            ...(["SUPPLY_PROCUREMENT_CERTIFICATE", "EXTERNAL_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType)
                ? certificateItem
                    ? {} // Skip defaults that could overwrite certificateItem values
                    : { discount: 0, transportationExpenses: 0, gratuities: 0, procurementDate: new Date() }
                : currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                    ? { insurance: 0, deductions: 0, achievementPercentage: 0 }
                    : ["COMPANY_SUPPLY_SALE_CERTIFICATE", "CONTRACTOR_PURCHASE_CERTIFICATE"].includes(currentCertificateType)
                        ? certificateItem
                            ? {} // Skip defaults for these types as well
                            : { transportationExpenses: 0, gratuities: 0, procurementDate: new Date(), discount: 0, insurance: 0 }
                        : {}),
        };

        return {
            ...baseDefaultValues,
            ...typeSpecificDefaults,
            ...(certificateItem || {}), // Ensure certificateItem takes precedence
            procurementDate: certificateItem?.procurementDate
                ? new Date(certificateItem.procurementDate)
                : new Date(),
        };
    }, [certificateItem, currentCertificateType]);

        const MyForm = useRef<Form>(null);
    const [formKey, setFormKey] = useState<number>(1);
    const [initValue, setInitValue] = useState<Partial<CertificateItem>>(deserializedInitValue);

    const { handleSubmitData } = useCertificateItem({
        certificateItem,
        editMode,
        setFormKey,
        setInitValue,
        discountMode,
        insuranceMode,
        calculateTotals,
    });

    // Purpose: Improve performance for WorkmanshipContractingForm
    // Context: Not used in ContractorPurchaseForm or CompanySupplyForm but kept for consistency
    const achievementPercentageValidator = useMemo(
        () => (value: number | undefined): string | undefined => {
            if (value === undefined || value === null) return "Achievement Percentage is required";
            if (value < 1 || value > 100) return "Achievement Percentage must be between 1 and 100";
            return undefined;
        },
        []
    );

    const handleInsuranceModeChange = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => {
            setInsuranceMode(event.target.value as "value" | "percentage");
            onChange("insurance", { value: 0 });
            setCalculatedInsurance(0);
        },
        []
    );

    const handleDiscountModeChange = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => {
            setDiscountMode(event.target.value as "value" | "percentage");
            onChange("discount", { value: 0 });
        },
        []
    );
    
    console.log('initValue', initValue)
    console.log("certificateItem in CertificateItemForm:", certificateItem);
    console.log("initValue in CertificateItemForm:", initValue);
     
    return (
        <Form
            ref={MyForm}
            initialValues={initValue}
            key={formKey}
            onSubmit={(values: Partial<CertificateItem>) => {
                // Purpose: Align with CertificateItem interface and prevent undefined values
                // Context: Handles COMPANY_SUPPLY_SALE_CERTIFICATE correctly
                const serializedValues: CertificateItem = {
                    ...values,
                    productId: values.productId || "",
                    productName: values.productName || "",
                    uomId: values.uomId || "",
                    uomName: values.uomName || "",
                    description: values.description || "",
                    quantity: Number(values.quantity) || 0,
                    unitPrice: Number(values.unitPrice) || 0,
                    totalAmount: Number(values.totalAmount) || 0,
                    discount: Number(values.discount) || 0,
                    insurance: Number(values.insurance) || 0,
                    deductions: Number(values.deductions) || 0,
                    deserved: Number(values.deserved) || 0,
                    net: Number(values.net) || 0,
                    procurementDate:
                        values.procurementDate instanceof Date
                            ? values.procurementDate.toISOString()
                            : new Date().toISOString(),
                    isContractorPurchased: false,
                    isDeleted: false,
                    achievementPercentage: Number(values.achievementPercentage) || 0,
                    transportationExpenses: Number(values.transportationExpenses) || 0,
                    gratuities: Number(values.gratuities) || 0,
                    completionPercentage: Number(values.completionPercentage) || 0,
                    notes: values.notes || "",
                    workEffortId: values.workEffortId || "",
                    workEffortParentId: values.workEffortParentId || "",
                };
                handleSubmitData(serializedValues, (name: string) => values[name] || "");
                onClose();
            }}
            render={(formRenderProps) => {
                if (currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE") {
                    return (
                        <SupplyProcurementForm
                            formRenderProps={formRenderProps}
                            editMode={editMode}
                            formEditMode={formEditMode}
                            discountMode={discountMode}
                            handleDiscountModeChange={handleDiscountModeChange}
                            calculateTotals={calculateTotals}
                            getTranslatedLabel={getTranslatedLabel}
                            onClose={onClose}
                            percentageValidator={percentageValidator}
                        />
                    );
                } else if (currentCertificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE") {
                    return (
                        <CompanySupplyForm
                            formRenderProps={formRenderProps}
                            editMode={editMode}
                            formEditMode={formEditMode}
                            calculateTotals={calculateTotals}
                            getTranslatedLabel={getTranslatedLabel}
                            onClose={onClose}
                        />
                    );
                } else if (currentCertificateType === "EXTERNAL_SUPPLY_SALE_CERTIFICATE") {
                    return (
                        <SupplyProcurementForm
                            formRenderProps={formRenderProps}
                            editMode={editMode}
                            formEditMode={formEditMode}
                            discountMode={discountMode}
                            handleDiscountModeChange={handleDiscountModeChange}
                            calculateTotals={calculateTotals}
                            getTranslatedLabel={getTranslatedLabel}
                            onClose={onClose}
                            percentageValidator={percentageValidator}
                        />
                    );
                } else if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
                    return (
                        <WorkmanshipContractingForm
                            formRenderProps={formRenderProps}
                            editMode={editMode}
                            formEditMode={formEditMode}
                            insuranceMode={insuranceMode}
                            handleInsuranceModeChange={handleInsuranceModeChange}
                            calculateTotals={calculateTotals}
                            getTranslatedLabel={getTranslatedLabel}
                            onClose={onClose}
                            achievementPercentageValidator={achievementPercentageValidator}
                        />
                    );
                } else if (currentCertificateType === "CONTRACTOR_PURCHASE_CERTIFICATE") {
                    return (
                        <ContractorPurchaseForm
                            formRenderProps={formRenderProps}
                            editMode={editMode}
                            formEditMode={formEditMode}
                            calculateTotals={calculateTotals}
                            getTranslatedLabel={getTranslatedLabel}
                            onClose={onClose}
                        />
                    );
                }
                return null; // Fallback for unsupported certificate types
            }}
        />
    );
}

export const CertificateItemFormMemo = React.memo(CertificateItemForm);