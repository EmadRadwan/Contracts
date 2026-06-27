import { useState } from "react";
import { toast } from "react-toastify";
import { useAppDispatch } from "../../../../app/store/configureStore";
import { useUpdateMultiAcctgTransWithEntriesMutation } from "../../../../app/store/apis";

// REFACTOR: Define response interface to match backend
// Purpose: Ensure type safety for the mutation response
// Improvement: Clarifies expected response structure
export interface UpdateMultiAcctgTransResponse {
  acctgTransId: string;
}

export interface UpdateMultiAcctgTransWithEntriesParams {
  acctgTransId: string;
  updateMultiAcctgTransParams: {
    acctgTransId: string;
    acctgTransTypeId?: string;
    transactionDate: Date | string;
    organizationPartyId: string;
    headerDescription: string;
    description: string;
    isPosted?: string;
    partyId?: string;
    costCenterId?: string;
    glFiscalTypeId?: string;
  };
  entries: MultiAcctgTransEntryParams[];
}

export interface MultiAcctgTransEntryParams {
  acctgTransEntrySeqId?: string;
  debitGlAccountId?: string;
  creditGlAccountId?: string;
  amount: number;
  description: string;
  debitCreditFlag: "D" | "C";
}

const useEditMultiAcctgTrans = () => {
  const dispatch = useAppDispatch();
  const [updateMultiAcctgTransWithEntries] = useUpdateMultiAcctgTransWithEntriesMutation();
  const [isLoading, setIsLoading] = useState(false);

  // REFACTOR: Handle update operation with proper error handling and cache invalidation
  // Purpose: Ensure robust update process and UI refresh
  // Improvement: Uses correct tag invalidation and type-safe response
  async function handleUpdateMultiAcctgTransWithEntries(params: UpdateMultiAcctgTransWithEntriesParams) {
    setIsLoading(true);
    try {
      const result = await updateMultiAcctgTransWithEntries(params).unwrap() as UpdateMultiAcctgTransResponse;
      toast.success("Transaction and entries updated successfully");
      // REFACTOR: Use RTK Query's tag invalidation directly
      // Purpose: Ensure transaction list is refreshed without manual dispatch
      // Improvement: Leverages RTK Query's built-in cache management
      return result;
    } catch (error: any) {
      toast.error(`Failed to update transaction: ${error?.message || "Unknown error"}`);
      throw error;
    } finally {
      setIsLoading(false);
    }
  }

  return { isLoading, handleUpdateMultiAcctgTransWithEntries };
};

export default useEditMultiAcctgTrans;