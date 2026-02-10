import { toast } from "react-toastify";
import { useNavigate } from "react-router-dom";
import {useDuplicateAcctgTransMutation} from "../../../../app/store/apis";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";

export default function useDuplicateAcctgTrans() {
    const [duplicateTrigger, { isLoading }] = useDuplicateAcctgTransMutation();
    const { getTranslatedLabel } = useTranslationHelper();
    const navigate = useNavigate();

    const duplicate = async (acctgTransId: string | null | undefined) => {
        if (!acctgTransId) {
            toast.error("Cannot duplicate — no transaction ID");
            return;
        }

        try {
            const response = await duplicateTrigger(acctgTransId).unwrap();
            toast.success(
                `${getTranslatedLabel("general.duplicated", "Transaction duplicated successfully")} — New ID: #${response.newAcctgTransId}`
            );
            // Navigate to edit view of the new transaction
            navigate(`/editAcctgTrans/${response.newAcctgTransId}`);
        } catch (err: any) {
            toast.error(
                getTranslatedLabel("general.error", "Failed to duplicate transaction") +
                (err?.data?.message ? `: ${err.data.message}` : "")
            );
        }
    };

    return {
        duplicate,
        isDuplicating: isLoading,
    };
}