import { Form, FormRenderProps } from "@progress/kendo-react-form";
import React, { useCallback, useMemo, useRef, useState } from "react";
import { useAppSelector, useFetchFacilitiesQuery } from "../../../app/store/configureStore";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import useCertificateItem from "../hook/useCertificateItem";
import { percentageValidator, requiredValidator } from "../../../app/common/form/Validators";
import SupplyProcurementForm from "./SupplyProcurementForm";
import WorkmanshipContractingForm from "./WorkmanshipContractingForm";
import CompanySupplyForm from "./CompanySupplyForm";
import { v4 as uuidv4 } from "uuid";
import {toast} from "react-toastify";

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
    const [additionalInsuranceMode, setAdditionalInsuranceMode] = useState<"value" | "percentage">("value");
    const [calculatedInsurance, setCalculatedInsurance] = useState<number>(0);
    const { getTranslatedLabel } = useTranslationHelper();

    const calculateTotals = useCallback(
        (valueGetter: FormRenderProps["valueGetter"]) => {
            const quantity = Number(valueGetter("quantity") || 0);
            const price =
                currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                    ? Number(valueGetter("materialPrice") || 0) + Number(valueGetter("laborPrice") || 0)
                    : Number(valueGetter("unitPrice") || 0);
            const total = Math.round(quantity * price * 1000) / 1000;
            let finalTotal = total;
            let deserved = 0;
            let insurance = 0;
            let additionalInsurance = 0;
            let discount = 0;

            if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
                deserved = Math.max(0, Math.round((total - Number(valueGetter("deductions") || 0)) * 1000) / 1000);
                const insuranceInput = Number(valueGetter("insurance") || 0);
                insurance = insuranceMode === "value" ? insuranceInput : (insuranceInput / 100) * deserved;
                insurance = Math.round(insurance * 1000) / 1000;
                const additionalInsuranceInput = Number(valueGetter("additionalInsurance") || 0);
                additionalInsurance =
                    additionalInsuranceMode === "value" ? additionalInsuranceInput : (additionalInsuranceInput / 100) * deserved;
                additionalInsurance = Math.round(additionalInsurance * 1000) / 1000;
                
              /*  // console.log('calculateTotals debug:', {
                    quantity,
                    price,
                    total,
                    deductions: Number(valueGetter("deductions") || 0),
                    deserved,
                    insuranceInput,
                    insuranceMode,
                    insurance,
                    additionalInsuranceInput,
                    additionalInsuranceMode,
                    additionalInsurance,
                    net: Math.max(0, Math.round((deserved - insurance - additionalInsurance) * 1000) / 1000),
                });*/
            } else if (currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE") {
                const discountInput = Number(valueGetter("discount") || 0);
                discount = discountMode === "value" ? discountInput : (discountInput / 100) * total;
                const transportationExpenses = Number(valueGetter("transportationExpenses") || 0);
                const gratuities = Number(valueGetter("gratuities") || 0);
                finalTotal = Math.max(0, Math.round((total - discount + transportationExpenses + gratuities) * 1000) / 1000);
            } else if (currentCertificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE") {
                const transportationExpenses = Number(valueGetter("transportationExpenses") || 0);
                const gratuities = Number(valueGetter("gratuities") || 0);
                finalTotal = Math.max(0, Math.round((total + transportationExpenses + gratuities) * 1000) / 1000);
            }

            const net =
                currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                    ? Math.max(0, Math.round((deserved - insurance - additionalInsurance) * 1000) / 1000)
                    : finalTotal;
            finalTotal = net;

            return {
                total,
                finalTotal,
                net,
                deserved,
                insurance,
                discount,
                additionalInsurance,
                transportationExpenses: Number(valueGetter("transportationExpenses") || 0),
                gratuities: Number(valueGetter("gratuities") || 0),
            };
        },
        [currentCertificateType, discountMode, insuranceMode, additionalInsuranceMode]
    );

    
    const deserializedInitValue = useMemo((): Partial<CertificateItem> => {
        const baseDefaultValues: Partial<CertificateItem> = {
            productId: "",
            productName: "",
            uomId: "",
            uomName: "",
            description: "",
            quantity: 0,
            unitPrice: 0,
            materialPrice: 0,
            laborPrice: 0,
            totalAmount: 0,
            discount: 0,
            insurance: 0,
            additionalInsurance: certificateItem?.additionalInsurance ?? 0,
            deductions: 0,
            deductionDescription: "",
            deserved: 0,
            net: 0,
            procurementDate: new Date(),
            isDeleted: false,
            achievementPercentage: 0,
            transportationExpenses: 0,
            gratuities: 0,
            workEffortId: "",
            workEffortParentId: "",
        };

        const typeSpecificDefaults: Partial<CertificateItem> = {
            ...(currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE"
                ? certificateItem
                    ? {}
                    : { discount: 0, transportationExpenses: 0, gratuities: 0, procurementDate: new Date() }
                : currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                    ? {
                        insurance: 0,
                        additionalInsurance: 0,
                        deductions: 0,
                        achievementPercentage: 0,
                        materialPrice: certificateItem?.materialPrice || 0,
                        laborPrice: certificateItem?.laborPrice || 0,
                    }
                    : {}),
        };

        // Use persisted additionalInsuranceMode, default to "value" if not set
        const additionalInsuranceModeDefault = certificateItem?.additionalInsuranceMode || "value";
        setAdditionalInsuranceMode(additionalInsuranceModeDefault);

        // Similarly for insuranceMode
        const insuranceModeDefault = certificateItem?.insuranceMode || "value";
        setInsuranceMode(insuranceModeDefault);

        return {
            ...baseDefaultValues,
            ...typeSpecificDefaults,
            ...(certificateItem || {}),
            procurementDate: certificateItem?.procurementDate ? new Date(certificateItem.procurementDate) : new Date(),
            materialPrice: certificateItem?.materialPrice || 0,
            laborPrice: certificateItem?.laborPrice || 0,
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

    const handleAdditionalInsuranceModeChange = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => {
            setAdditionalInsuranceMode(event.target.value as "value" | "percentage");
            onChange("additionalInsurance", { value: 0 });
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
/*
    // console.log('initValue', initValue)
    // console.log("certificateItem in CertificateItemForm:", certificateItem);
    // console.log("initValue in CertificateItemForm:", initValue);
    // console.log('deserializedInitValue', deserializedInitValue);
*/

    return (
        <Form
            ref={MyForm}
            initialValues={initValue}
            key={formKey}
            onSubmit={(values: Partial<CertificateItem>) => {
                const tempWorkEffortId = values.workEffortId || uuidv4();

                const description = values.description || "";
                const deductionDescription = values.deductionDescription || "";

                if (description.length > 3000) {
                    toast.error("Description cannot exceed 1000 characters.");
                    return; // Prevent form submission
                }
                if (deductionDescription.length > 1000) {
                    toast.error("Deduction Description cannot exceed 1000 characters.");
                    return; // Prevent form submission
                }
                
                const serializedValues: CertificateItem = {
                    ...values,
                    productId: values.productId || "",
                    productName: values.productName || "",
                    uomId: values.uomId || "",
                    uomName: values.uomName || "",
                    description: values.description || "",
                    deductionDescription: values.deductionDescription || "",
                    quantity: Number(values.quantity) || 0,
                    unitPrice:
                        currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                            ? Number(values.materialPrice || 0) + Number(values.laborPrice || 0)
                            : Number(values.unitPrice) || 0,
                    materialPrice: Number(values.materialPrice) || 0,
                    laborPrice: Number(values.laborPrice) || 0,
                    totalAmount: Number(values.totalAmount) || 0,
                    discount: Number(values.discount) || 0,
                    insurance: Number(values.insurance) || 0,
                    additionalInsurance: Number(values.additionalInsurance) || 0,
                    deductions: Number(values.deductions) || 0,
                    deserved: Number(values.deserved) || 0,
                    net: Number(values.net) || 0,
                    procurementDate: values.procurementDate instanceof Date ? values.procurementDate.toISOString() : new Date().toISOString(),
                    isDeleted: false,
                    achievementPercentage: Number(values.achievementPercentage) || 0,
                    transportationExpenses: Number(values.transportationExpenses) || 0,
                    gratuities: Number(values.gratuities) || 0,
                    workEffortId: values.workEffortId || "",
                    workEffortParentId: values.workEffortParentId || "",
                };
                //// console.log('onSubmit serializedValues:', serializedValues);
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
                } else if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
                    return (
                        <WorkmanshipContractingForm
                            formRenderProps={formRenderProps}
                            editMode={editMode}
                            formEditMode={formEditMode}
                            insuranceMode={insuranceMode}
                            additionalInsuranceMode={additionalInsuranceMode}
                            handleInsuranceModeChange={handleInsuranceModeChange}
                            handleAdditionalInsuranceModeChange={handleAdditionalInsuranceModeChange}
                            calculateTotals={calculateTotals}
                            getTranslatedLabel={getTranslatedLabel}
                            onClose={onClose}
                            achievementPercentageValidator={achievementPercentageValidator}
                        />
                    );
                } 
                return null; // Fallback for unsupported certificate types
            }}
        />
    );
}

export const CertificateItemFormMemo = React.memo(CertificateItemForm);