import { useCallback } from "react";
import { addUiMultiPaymentItem, updateUiMultiPaymentItem } from "../slice/multiPaymentItemsUiSlice";
import {useAppDispatch} from "../../../app/store/configureStore";
import {MultiPaymentItem} from "../../../app/models/project/MultiPaymentItem";

export default function useMultiPaymentItem({
                                                multiPaymentItem,
                                                editMode,
                                                certificateId,
                                                setFormKey,
                                            }: {
    multiPaymentItem?: MultiPaymentItem;
    editMode: number;
    certificateId: string;
    setFormKey: (key: number) => void;
}) {
    const dispatch = useAppDispatch();

    // REFACTOR: Update handleSubmitData to include new fields, maintaining certificateId association.
    const handleSubmitData = useCallback(
        (values: MultiPaymentItem) => {
            const itemWithCertificateId = { ...values, certificateId };
            if (editMode === 1) {
                dispatch(addUiMultiPaymentItem(itemWithCertificateId));
            } else {
                dispatch(updateUiMultiPaymentItem(itemWithCertificateId));
            }
            setFormKey((prev) => prev + 1);
        },
        [dispatch, editMode, certificateId, setFormKey]
    );

    return { handleSubmitData };
}