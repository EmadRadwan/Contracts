import { Form, FormRenderProps } from "@progress/kendo-react-form";
import React, {useCallback, useEffect, useMemo, useRef, useState} from "react";
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

const parseAchievementPercentage = (value: string | number | null | undefined): number => {
    if (value == null || value === "") return 0;
    const str = String(value).trim().replace(/%/g, "");
    const num = parseFloat(str);
    return isNaN(num) ? 0 : num;
};

export default function CertificateItemForm({
                                                certificateItem,
                                                editMode,
                                                onClose,
                                                formEditMode,
                                            }: Props) {
    const { currentCertificateType } = useAppSelector((state) => state.certificateUi);
    const [discountMode, setDiscountMode] = useState<"value" | "percentage">("value");
    const { getTranslatedLabel } = useTranslationHelper();



    const calculateTotals = useCallback(
        (
            valueGetter: FormRenderProps["valueGetter"],
            insuranceModeParam?: "value" | "percentage",
            additionalInsuranceModeParam?: "value" | "percentage"
        ) => {
            const quantity = Number(valueGetter("quantity") || 0);
            const unitPrice = Number(valueGetter("unitPrice") || 0);
            const materialPrice = Number(valueGetter("materialPrice") || 0);
            const laborPrice = Number(valueGetter("laborPrice") || 0);

            let total = 0;
            // quantity × price
            let deserved = 0;       // This is the TRUE value of work done — never reduced by penalties
            let insurance = 0;
            let additionalInsurance = 0;
            let net = 0;
            let discount = 0;
            let transportationExpenses = 0;
            let gratuities = 0;

            // Use passed-in modes first (for live updates when switching radio), fallback to form value
            const insuranceMode = insuranceModeParam ?? (valueGetter("insuranceMode") as "value" | "percentage") ?? "value";
            const additionalInsuranceMode = additionalInsuranceModeParam ?? (valueGetter("additionalInsuranceMode") as "value" | "percentage") ?? "value";

            // ===================================================================
            // 1. WORKMANSHIP CONTRACTING CERTIFICATE
            // ===================================================================
            if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
                const pricePerUnit = materialPrice + laborPrice;
                total = Math.round(quantity * pricePerUnit * 1000) / 1000;

                const achievementPercentage = Number(valueGetter("achievementPercentage") || 0);
                const grossAchieved = Math.round(total * (achievementPercentage / 100) * 1000) / 1000;

                // Deserved = value of work actually performed (before any penalties)
                deserved = grossAchieved;

                const deductions = Number(valueGetter("deductions") || 0);

                // Insurance & Additional Insurance are ALWAYS calculated on the GROSS achieved value
                const insuranceInput = Number(valueGetter("insurance") || 0);
                insurance = insuranceMode === "percentage"
                    ? Math.round((insuranceInput / 100) * grossAchieved * 1000) / 1000
                    : insuranceInput;

                const addInsInput = Number(valueGetter("additionalInsurance") || 0);
                additionalInsurance = additionalInsuranceMode === "percentage"
                    ? Math.round((addInsInput / 100) * grossAchieved * 1000) / 1000
                    : addInsInput;

                // Net = deserved − deductions − insurance − additional insurance
                net = Math.max(0, Math.round((deserved - deductions - insurance - additionalInsurance) * 1000) / 1000);

                return {
                    total,
                    finalTotal: net,
                    net,
                    deserved,
                    insurance,
                    discount: 0,
                    additionalInsurance,
                    transportationExpenses: 0,
                    gratuities: 0,
                };
            }

            // ===================================================================
            // 2. SUPPLY PROCUREMENT CERTIFICATE
            // ===================================================================
            if (currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE") {
                total = Math.round(quantity * unitPrice * 1000) / 1000;
                deserved = total; // In supply, deserved = total

                const discountInput = Number(valueGetter("discount") || 0);
                discount = discountMode === "percentage"
                    ? Math.round((discountInput / 100) * total * 1000) / 1000
                    : discountInput;

                transportationExpenses = Number(valueGetter("transportationExpenses") || 0);
                gratuities = Number(valueGetter("gratuities") || 0);

                net = Math.max(0, Math.round((total - discount + transportationExpenses + gratuities) * 1000) / 1000);

                return {
                    total,
                    finalTotal: net,
                    net,
                    deserved,
                    insurance: 0,
                    discount,
                    additionalInsurance: 0,
                    transportationExpenses,
                    gratuities,
                };
            }

            // ===================================================================
            // 3. COMPANY SUPPLY SALE CERTIFICATE
            // ===================================================================
            if (currentCertificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE") {
                total = Math.round(quantity * unitPrice * 1000) / 1000;
                deserved = total;

                transportationExpenses = Number(valueGetter("transportationExpenses") || 0);
                gratuities = Number(valueGetter("gratuities") || 0);

                net = Math.max(0, Math.round((total + transportationExpenses + gratuities) * 1000) / 1000);

                return {
                    total,
                    finalTotal: net,
                    net,
                    deserved,
                    insurance: 0,
                    discount: 0,
                    additionalInsurance: 0,
                    transportationExpenses,
                    gratuities,
                };
            }

            // Fallback (should never happen)
            return {
                total: 0,
                finalTotal: 0,
                net: 0,
                deserved: 0,
                insurance: 0,
                discount: 0,
                additionalInsurance: 0,
                transportationExpenses: 0,
                gratuities: 0,
            };
        },
        [currentCertificateType, discountMode]
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
            transportationExpenses: 0,
            gratuities: 0,
            workEffortId: "",
            workEffortParentId: "",
            achievementPercentage: 0,
            insuranceMode: "value",
            additionalInsuranceMode: "value",
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

        return {
            ...baseDefaultValues,
            ...typeSpecificDefaults,
            ...(certificateItem || {}),
            procurementDate: certificateItem?.procurementDate ? new Date(certificateItem.procurementDate) : new Date(),
            materialPrice: certificateItem?.materialPrice || 0,
            laborPrice: certificateItem?.laborPrice || 0,
            // REFACTOR: Preserve mode from DB or default
            insuranceMode: certificateItem?.insuranceMode ?? "value",
            additionalInsuranceMode: certificateItem?.additionalInsuranceMode ?? "value",
            // REFACTOR: Parse string percentages safely
            achievementPercentage: certificateItem?.achievementPercentage != null
                ? parseAchievementPercentage(certificateItem.achievementPercentage)
                : 0,
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
        calculateTotals,
    });

    const achievementPercentageValidator = useMemo(
        () => (value: number | undefined): string | undefined => {
            if (value === undefined || value === null) return "Achievement Percentage is required";
            if (value < 1 || value > 100) return "Achievement Percentage must be between 1 and 100";
            return undefined;
        },
        []
    );

    const handleInsuranceModeChange = (
        e: React.ChangeEvent<HTMLInputElement>,
        onChange: FormRenderProps["onChange"]
    ) => {
        const newMode = e.target.value as "value" | "percentage";
        onChange("insuranceMode", { value: newMode });
        onChange("insurance", { value: 0 });
    };

    const handleAdditionalInsuranceModeChange = (
        e: React.ChangeEvent<HTMLInputElement>,
        onChange: FormRenderProps["onChange"]
    ) => {
        const newMode = e.target.value as "value" | "percentage";
        onChange("additionalInsuranceMode", { value: newMode });
        onChange("additionalInsurance", { value: 0 });
    };

    const handleDiscountModeChange = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>, onChange: FormRenderProps["onChange"]) => {
            setDiscountMode(event.target.value as "value" | "percentage");
            onChange("discount", { value: 0 });
        },
        []
    );


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
                    deserved: Number(values.deserved) || 0,
                    net: Number(values.net) || 0,
                    procurementDate: values.procurementDate instanceof Date ? values.procurementDate.toISOString() : new Date().toISOString(),
                    isDeleted: false,
                    transportationExpenses: Number(values.transportationExpenses) || 0,
                    gratuities: Number(values.gratuities) || 0,
                    workEffortId: values.workEffortId || "",
                    workEffortParentId: values.workEffortParentId || "",
                    insuranceMode: values.insuranceMode as "value" | "percentage",
                    additionalInsuranceMode: values.additionalInsuranceMode as "value" | "percentage",

                    // ---- safe numeric conversion (never NaN) -------------------------
                    achievementPercentage: Number(values.achievementPercentage) || 0,
                    insurance: Number(values.insurance) || 0,
                    additionalInsurance: Number(values.additionalInsurance) || 0,
                    deductions: Number(values.deductions) || 0,
                };
                //// console.log('onSubmit serializedValues:', serializedValues);
                handleSubmitData(serializedValues, (name: string) => values[name] || "");
                setFormKey(prev => prev + 1);
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