import { useCallback } from "react";
import { useSelector } from "react-redux";
import { toast } from "react-toastify";
import { useAppDispatch } from "../../../app/store/configureStore";
import { nonDeletedCertificateItemsSelector } from "../slice/certificateSelectors";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import {setProcessedCertificateItems, updateCertificateItem} from "../slice/certificateItemsUiSlice";

interface UseCertificateItemProps {
    certificateItem?: CertificateItem;
    editMode: number; // 1: add, 2: edit
    setFormKey: (key: number) => void;
    setInitValue: (value: CertificateItem | undefined) => void;
    discountMode: "value" | "percentage"; // REFACTOR: Added to track discount input mode
    insuranceMode: "value" | "percentage"; // REFACTOR: Added to track insurance input mode
}

export default function useCertificateItem({
                                               certificateItem,
                                               editMode,
                                               setFormKey,
                                               setInitValue,
                                               discountMode,
                                               insuranceMode,
                                           }: UseCertificateItemProps) {
    const dispatch = useAppDispatch();
    const certificateItemsFromUi: CertificateItem[] = useSelector(nonDeletedCertificateItemsSelector);

    console.log("useCertificateItem initialized with editMode:", editMode);


    const logError = (error: any, defaultMessage: string) => {
        const message = error?.data?.message || error?.message || defaultMessage;
        console.error("Error:", JSON.stringify(error, null, 2));
        toast.error(message);
    };


    const createOrUpdateCertificateItem = useCallback(
        (data: CertificateItem): CertificateItem => {

            console.log("createOrUpdateCertificateItem called with:", data);

            
            let newCertificateItem: CertificateItem;
            const itemSeqId = certificateItemsFromUi?.length ? certificateItemsFromUi.length + 1 : 1;

            const serializedProcurementDate =
                data.procurementDate instanceof Date ? data.procurementDate.toISOString() : data.procurementDate;

            const total = data.quantity * data.unitPrice;
            const discount =
                discountMode === "percentage" && data.discount ? (data.discount / 100) * total : data.discount || 0;

            const deserved = data.deserved || total;
            const insurance =
                insuranceMode === "percentage" && data.insurance ? (data.insurance / 100) * deserved : data.insurance || 0;
            const net = Math.max(0, deserved - insurance);

            const commonFields: CertificateItem = {
                productId: typeof data.productId === "object" ? data.productId.ProductId : data.productId,
                productName: typeof data.productId === "object" ? data.productId.ProductName : data.productName || "",
                uomId: typeof data.uomId === "object" ? data.uomId.UomId : data.uomId,
                uomName: typeof data.uomId === "object" ? data.uomId.Description : data.uomName || "",
                quantity: data.quantity,
                unitPrice: +data.unitPrice?.toFixed(2),
                totalAmount: +total.toFixed(2),
                discount: +discount.toFixed(2),
                insurance: +insurance.toFixed(2),
                deductions: data.deductions || 0,
                deserved: +deserved.toFixed(2),
                net: +net.toFixed(2), // REFACTOR: Include net field
                completionPercentage: data.completionPercentage,
                notes: data.notes,
                procurementDate: serializedProcurementDate,
                facilityId: data.facilityId,
                facilityName: data.facilityName || "",
                isContractorPurchased: data.isContractorPurchased || false,
                isDeleted: false,
                achievementPercentage: data.achievementPercentage,
            };

            if (editMode === 2) {
                // REFACTOR: Use calculated insurance and discount consistently in edit mode
                // Purpose: Prevent overriding calculated values with raw data
                // Context: Ensures backend receives correct absolute values
                newCertificateItem = {
                    ...commonFields,
                    workEffortId: certificateItem?.workEffortId || "",
                    workEffortParentId: certificateItem?.workEffortParentId || "",
                    deductions: data.deductions || 0,
                    deserved: data.deserved || 0,
                    achievementPercentage: data.achievementPercentage,
                };
            } else {
                // REFACTOR: Remove redundant discount and insurance assignments
                // Purpose: Use calculated values from commonFields for consistency
                // Context: Simplifies logic and avoids duplication
                newCertificateItem = {
                    ...commonFields,
                    workEffortId: `TEMP-${itemSeqId}`,
                    workEffortParentId: "",
                    deductions: data.deductions || 0,
                    deserved: data.deserved || 0,
                    achievementPercentage: data.achievementPercentage,
                };
            }

            return newCertificateItem;
        },
        [certificateItem, editMode, certificateItemsFromUi, discountMode, insuranceMode]
    );

    // Purpose: Streamline submission process with all fields
    // Context: Resets form after successful submission
const handleSubmitData = useCallback(
  async (data: CertificateItem) => {
    try {
      const newCertificateItem = createOrUpdateCertificateItem(data);
      dispatch(setProcessedCertificateItems([newCertificateItem]));
        dispatch(updateCertificateItem({ certificateItem: newCertificateItem, editMode }));
      setFormKey(Math.random());
      setInitValue(undefined);
    } catch (error: any) {
      // REFACTOR: Use utility function for error handling
      // Purpose: Avoid hook-related issues by moving toast.error to a non-hook function
      // Context: Ensures handleError logic is safe to call in async contexts
      logError(
        error,
        `Failed to ${editMode === 1 ? "add" : "update"} certificate item`
      );
    }
  },
  [
    dispatch,
    createOrUpdateCertificateItem,
    editMode,
    setFormKey,
    setInitValue,
  ]
);

    return { handleSubmitData };
}