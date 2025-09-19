import { useCallback } from "react";
import { useSelector } from "react-redux";
import { toast } from "react-toastify";
import { useAppDispatch } from "../../../app/store/configureStore";
import { nonDeletedCertificateItemsSelector } from "../slice/certificateSelectors";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import { setProcessedCertificateItems, updateCertificateItem } from "../slice/certificateItemsUiSlice";
import { FormRenderProps } from "@progress/kendo-react-form";

interface UseCertificateItemProps {
    certificateItem?: CertificateItem;
    editMode: number; // 1: add, 2: edit
    setFormKey: (key: number) => void;
    setInitValue: (value: CertificateItem | undefined) => void;
    discountMode: "value" | "percentage";
    insuranceMode: "value" | "percentage";
    additionalInsuranceMode: "value" | "percentage";
    calculateTotals: (valueGetter: FormRenderProps["valueGetter"]) => {
        total: number;
        finalTotal: number;
        net: number;
        deserved: number;
        insurance: number;
        discount: number;
        transportationExpenses: number;
        gratuities: number;
        additionalInsurance: number;
    };
}

export default function useCertificateItem({
                                               certificateItem,
                                               editMode,
                                               setFormKey,
                                               setInitValue,
                                               discountMode,
                                               insuranceMode,
                                               additionalInsuranceMode,
                                               calculateTotals,
                                           }: UseCertificateItemProps) {
    const dispatch = useAppDispatch();
    const certificateItemsFromUi: CertificateItem[] = useSelector(nonDeletedCertificateItemsSelector);
    const currentCertificateType = useSelector((state: any) => state.certificateUi.currentCertificateType);

    const logError = (error: any, defaultMessage: string) => {
        const message = error?.data?.message || error?.message || defaultMessage;
        console.error("Error:", JSON.stringify(error, null, 2));
        toast.error(message);
    };

    // Purpose: Allow transportationExpenses and gratuities for all certificate types, including CONTRACTOR_PURCHASE_CERTIFICATE
    // Context: Ensures values from calculateTotals are used consistently without being forced to 
    const createOrUpdateCertificateItem = useCallback(
        (data: CertificateItem, valueGetter: FormRenderProps["valueGetter"]): CertificateItem => {
            const itemSeqId = certificateItemsFromUi?.length ? certificateItemsFromUi.length + 1 : 1;
            const serializedProcurementDate = data.procurementDate instanceof Date ? data.procurementDate.toISOString() : data.procurementDate;
            const { total, finalTotal, net, deserved, insurance, discount, transportationExpenses, gratuities, additionalInsurance } = calculateTotals(valueGetter);
            const isContractingType = currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE";

            const commonFields: CertificateItem = {
              productId:
                typeof data.productId === "object"
                  ? data.productId.ProductId
                  : data.productId,
              productName:
                typeof data.productId === "object"
                  ? data.productId.ProductName
                  : data.productName || "",
              uomId:
                typeof data.uomId === "object" ? data.uomId.UomId : data.uomId,
              uomName:
                typeof data.uomId === "object"
                  ? data.uomId.Description
                  : data.uomName || "",
              description: data.description || "",
              quantity: data.quantity || 0,
              unitPrice: isContractingType
                ? (Number(data.materialPrice) || 0) +
                  (Number(data.laborPrice) || 0)
                : data.unitPrice || 0,
              materialPrice: isContractingType
                ? Number(data.materialPrice) || 0
                : 0,
              laborPrice: isContractingType ? Number(data.laborPrice) || 0 : 0,
              totalAmount: isContractingType
                ? +total.toFixed(3)
                : +net.toFixed(3),
              discount: +discount.toFixed(3),
              insurance: +insurance.toFixed(3),
              additionalInsurance: +additionalInsurance.toFixed(3),
              deductions: data.deductions || 0,
              deserved: +deserved.toFixed(3),
              net: +net.toFixed(3),
              completionPercentage: data.completionPercentage,
              notes: data.notes,
              procurementDate: serializedProcurementDate,
              isDeleted: false,
              achievementPercentage: data.achievementPercentage || 0,
              transportationExpenses: +transportationExpenses.toFixed(3),
              gratuities: +gratuities.toFixed(3),
              insuranceMode,
              additionalInsuranceMode,
              finalTotal: +finalTotal.toFixed(3),
            };

            let newCertificateItem: CertificateItem;
            if (editMode === 2) {
                newCertificateItem = {
                    ...commonFields,
                    workEffortId: certificateItem?.workEffortId || "",
                    workEffortParentId: certificateItem?.workEffortParentId || "",
                };
            } else {
                newCertificateItem = {
                    ...commonFields,
                    workEffortId: `TEMP-${itemSeqId}`,
                    workEffortParentId: "",
                };
            }
            console.log('createOrUpdateCertificateItem debug:', {
                inputData: data,
                calculated: { total, finalTotal, net, deserved, insurance, additionalInsurance },
                newCertificateItem,
            });
            return newCertificateItem;
        },
        [certificateItem, editMode, certificateItemsFromUi, calculateTotals]
    );

    // Purpose: Allow createOrUpdateCertificateItem to use calculateTotals for accurate calculations
    // Context: Ensures transportationExpenses and gratuities are handled consistently
    const handleSubmitData = useCallback(
        async (data: CertificateItem, valueGetter: FormRenderProps["valueGetter"]) => {
            try {
                const newCertificateItem = createOrUpdateCertificateItem(data, valueGetter);
                dispatch(setProcessedCertificateItems([newCertificateItem]));
                dispatch(updateCertificateItem({ certificateItem: newCertificateItem, editMode }));
                setFormKey(Math.random());
                setInitValue(undefined);
            } catch (error: any) {
                logError(error, `Failed to ${editMode === 1 ? "add" : "update"} certificate item`);
            }
        },
        [dispatch, createOrUpdateCertificateItem, editMode, setFormKey, setInitValue]
    );

    return { handleSubmitData };
}