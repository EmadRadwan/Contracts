import { Form, FormRenderProps } from "@progress/kendo-react-form";
import React, { useCallback, useMemo, useRef, useState } from "react";
import { useAppSelector, useFetchFacilitiesQuery } from "../../../app/store/configureStore";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import useCertificateItem from "../hook/useCertificateItem";
import { percentageValidator, requiredValidator } from "../../../app/common/form/Validators";
import SupplyProcurementForm from "./SupplyProcurementForm";
import WorkmanshipContractingForm from "./WorkmanshipContractingForm";

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
    const deserializedInitValue = certificateItem
        ? {
            ...certificateItem,
            procurementDate: certificateItem.procurementDate
                ? new Date(certificateItem.procurementDate)
                : undefined,
        }
        : undefined;

    const MyForm = useRef<Form>(null);
    const [formKey, setFormKey] = useState(1);
    const [initValue, setInitValue] = useState<CertificateItem | undefined>(deserializedInitValue);
    const { currentCertificateType } = useAppSelector((state) => state.certificateUi);
    const [discountMode, setDiscountMode] = useState<"value" | "percentage">("value");
    const [insuranceMode, setInsuranceMode] = useState<"value" | "percentage">("value");
    const [calculatedInsurance, setCalculatedInsurance] = useState(0);
    const { data: facilities = [] } = useFetchFacilitiesQuery(undefined); // REFACTOR: Default to empty array to simplify null checks
    const { getTranslatedLabel } = useTranslationHelper();
    const { handleSubmitData } = useCertificateItem({
        certificateItem,
        editMode,
        setFormKey,
        setInitValue,
        discountMode,
        insuranceMode,
    });

    // REFACTOR: Memoize achievementPercentageValidator to prevent unnecessary re-creation
    // Purpose: Improve performance by ensuring stable validator function
    // Context: Used in ContractingForm, stable reference reduces re-renders
    const achievementPercentageValidator = useMemo(
        () => (value: number) => {
            if (value === undefined || value === null) return "Achievement Percentage is required";
            if (value < 1 || value > 100) return "Achievement Percentage must be between 1 and 100";
            return undefined;
        },
        []
    );

    // REFACTOR: Memoize calculateTotals to avoid redundant calculations
    // Purpose: Optimize performance by caching results based on inputs
    // Context: Used frequently in form rendering, memoization reduces computation
    const calculateTotals = useCallback(
        (valueGetter: FormRenderProps["valueGetter"]) => {
            const quantity = valueGetter("quantity") || 0;
            const price = valueGetter("unitPrice") || 0;
            const total = Math.round(quantity * price * 100) / 100;
            let finalTotal = total;
            let deserved = 0;
            let insurance = 0;
            let discount = 0;

            if (currentCertificateType === "PROCUREMENT_CERTIFICATE") {
                const discountInput = valueGetter("discount") || 0;
                discount = discountMode === "value" ? discountInput : (discountInput / 100) * total;
                finalTotal = total - discount;
            } else if (currentCertificateType === "CONTRACTING_CERTIFICATE") {
                deserved = Math.max(0, Math.round((total - (valueGetter("deductions") || 0)) * 100) / 100);
                const insuranceInput = valueGetter("insurance") || 0;
                insurance = insuranceMode === "value" ? insuranceInput : (insuranceInput / 100) * deserved;
                insurance = Math.round(insurance * 100) / 100;
            }

            const net =
                currentCertificateType === "CONTRACTING_CERTIFICATE"
                    ? Math.max(0, Math.round((deserved - insurance) * 100) / 100)
                    : finalTotal;

            setCalculatedInsurance(insurance);
            return { total, finalTotal, net, deserved, insurance, discount };
        },
        [currentCertificateType, discountMode, insuranceMode]
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

    const getFacilityName = useCallback(
        (facilityId: string | undefined) => {
            if (!facilityId) return "";
            const facility = facilities.find((f) => f.facilityId === facilityId);
            return facility?.facilityName || "";
        },
        [facilities]
    );

    // REFACTOR: Removed console.log from onSubmit
    // Purpose: Avoid unnecessary logging in production for performance
    // Context: Debugging statement was redundant
    return (
        <Form
            ref={MyForm}
            initialValues={initValue}
            key={formKey}
            onSubmit={(values) => {
                const { total, finalTotal, net, deserved, discount, insurance } = calculateTotals(
                    (name: string) => values[name]
                );
                const serializedValues = {
                    ...values,
                    procurementDate:
                        values.procurementDate instanceof Date
                            ? values.procurementDate.toISOString()
                            : values.procurementDate,
                };
                handleSubmitData({
                    ...serializedValues,
                    total: currentCertificateType === "PROCUREMENT_CERTIFICATE" ? finalTotal : total,
                    net,
                    deserved,
                    discount: +discount.toFixed(2),
                    insurance: +insurance.toFixed(2),
                    isContractorPurchased: values.isContractorPurchased || false,
                    achievementPercentage: values.achievementPercentage,
                    facilityName: getFacilityName(values.facilityId),
                } as CertificateItem);
                onClose();
            }}
            render={(formRenderProps) =>
                currentCertificateType === "PROCUREMENT_CERTIFICATE" ? (
                    <SupplyProcurementForm
                        formRenderProps={formRenderProps}
                        editMode={editMode}
                        formEditMode={formEditMode}
                        discountMode={discountMode}
                        handleDiscountModeChange={handleDiscountModeChange}
                        calculateTotals={calculateTotals}
                        facilities={facilities}
                        getTranslatedLabel={getTranslatedLabel}
                        onClose={onClose}
                        percentageValidator={percentageValidator} // REFACTOR: Pass percentageValidator explicitly
                    />
                ) : (
                    <WorkmanshipContractingForm
                        formRenderProps={formRenderProps}
                        editMode={editMode}
                        formEditMode={formEditMode}
                        insuranceMode={insuranceMode}
                        handleInsuranceModeChange={handleInsuranceModeChange}
                        calculateTotals={calculateTotals}
                        facilities={facilities}
                        getTranslatedLabel={getTranslatedLabel}
                        onClose={onClose}
                        achievementPercentageValidator={achievementPercentageValidator}
                    />
                )
            }
        />
    );
}

export const CertificateItemFormMemo = React.memo(CertificateItemForm);