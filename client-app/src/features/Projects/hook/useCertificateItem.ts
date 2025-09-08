import { useCallback } from "react";
import { useSelector } from "react-redux";
import { toast } from "react-toastify";
import { useAppDispatch } from "../../../app/store/configureStore";
import { nonDeletedCertificateItemsSelector } from "../slice/certificateSelectors";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import { setProcessedCertificateItems, updateCertificateItem } from "../slice/certificateItemsUiSlice";
import { FormRenderProps } from "@progress/kendo-react-form";

// Purpose: Ensure consistency with CertificateItemForm calculations including transportationExpenses and gratuities
// Context: Avoids duplicating calculation logic and ensures correct handling of new fields
interface UseCertificateItemProps {
    certificateItem?: CertificateItem;
    editMode: number; // 1: add, 2: edit
    setFormKey: (key: number) => void;
    setInitValue: (value: CertificateItem | undefined) => void;
    discountMode: "value" | "percentage";
    insuranceMode: "value" | "percentage";
    calculateTotals: (valueGetter: FormRenderProps["valueGetter"]) => {
        total: number;
        finalTotal: number;
        net: number;
        deserved: number;
        insurance: number;
        discount: number;
        transportationExpenses: number;
        gratuities: number;
    };
}

export default function useCertificateItem({
                                               certificateItem,
                                               editMode,
                                               setFormKey,
                                               setInitValue,
                                               discountMode,
                                               insuranceMode,
                                               calculateTotals,
                                           }: UseCertificateItemProps) {
    const dispatch = useAppDispatch();
    const certificateItemsFromUi: CertificateItem[] = useSelector(nonDeletedCertificateItemsSelector);

    const logError = (error: any, defaultMessage: string) => {
        const message = error?.data?.message || error?.message || defaultMessage;
        console.error("Error:", JSON.stringify(error, null, 2));
        toast.error(message);
    };

    // REFACTOR: Removed certificate type dependency for transportationExpenses and gratuities
    // Purpose: Allow transportationExpenses and gratuities for all certificate types, including CONTRACTOR_PURCHASE_CERTIFICATE
    // Context: Ensures values from calculateTotals are used consistently without being forced to 0
    const createOrUpdateCertificateItem = useCallback(
        (data: CertificateItem, valueGetter: FormRenderProps["valueGetter"]): CertificateItem => {
            console.log("createOrUpdateCertificateItem called with:", data);
            const itemSeqId = certificateItemsFromUi?.length ? certificateItemsFromUi.length + 1 : 1;
            const serializedProcurementDate = data.procurementDate instanceof Date
                ? data.procurementDate.toISOString()
                : data.procurementDate;

            // Use calculateTotals to get consistent values
            const { total, finalTotal, net, deserved, insurance, discount, transportationExpenses, gratuities } =
                calculateTotals(valueGetter);

            const commonFields: CertificateItem = {
                productId: typeof data.productId === "object" ? data.productId.ProductId : data.productId,
                productName: typeof data.productId === "object" ? data.productId.ProductName : data.productName || "",
                uomId: typeof data.uomId === "object" ? data.uomId.UomId : data.uomId,
                uomName: typeof data.uomId === "object" ? data.uomId.Description : data.uomName || "",
                quantity: data.quantity,
                unitPrice: +data.unitPrice?.toFixed(3),
                totalAmount: +total.toFixed(3),
                discount: +discount.toFixed(3),
                insurance: +insurance.toFixed(3),
                deductions: data.deductions || 0,
                deserved: +deserved.toFixed(3),
                net: +net.toFixed(3),
                completionPercentage: data.completionPercentage,
                notes: data.notes,
                procurementDate: serializedProcurementDate,
                facilityId: data.facilityId,
                facilityName: data.facilityName || "",
                isDeleted: false,
                achievementPercentage: data.achievementPercentage,
                transportationExpenses: +transportationExpenses.toFixed(3),
                gratuities: +gratuities.toFixed(3),
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