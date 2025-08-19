import React, { useRef, useState } from "react";
import Grid from "@mui/material/Grid";
import { Paper, Typography, Button } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import FormInput from "../../../../app/common/form/FormInput";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import useAcctgTransEntry from "../hook/useAcctgTransEntry";
import {useSelector} from "react-redux";
import {RootState, useAppSelector} from "../../../../app/store/configureStore";
import {useFetchGlAccountOrganizationHierarchyLovQuery} from "../../../../app/store/apis";
import {FormDropDownTreeGlAccount2} from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import {requiredValidator} from "../../../../app/common/form/Validators";

interface AcctgTransEntryProps {
    selectedMenuItem: string;
    editMode: number;
    selectedEntry?: any; // Replace 'any' with AcctgTransEntry interface in production
    acctgTransId: string; // REFACTORED: Added acctgTransId as a required prop
}

export default function AcctgTransEntryForm({ selectedMenuItem, editMode, selectedEntry, acctgTransId }: AcctgTransEntryProps) {
  const [isLoading, setIsLoading] = useState(false);
  const formRef = useRef<boolean>(false);
  const { handleCreate, entry, formEditMode, isAddEntryLoading, isUpdateEntryLoading } = useAcctgTransEntry({
    selectedMenuItem,
    formRef,
    editMode,
    selectedEntry,
    setIsLoading,
  });
  const companyId = useAppSelector(state => state.accountingSharedUi.selectedAccountingCompanyId);
  const { data: glAccounts } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, { skip: companyId === undefined });

console.log('selectedEntry:', selectedEntry);

  const debitCreditOptions = [
    { debitCreditFlag: "C", description: "Credit" },
    { debitCreditFlag: "D", description: "Debit" },
  ];


  

  return (
    <Paper elevation={5} style={{ padding: "16px" }}>
      <Grid container spacing={2}>
        <Grid item xs={12}>
          <Typography variant="h5">{selectedMenuItem}</Typography>
        </Grid>
        <Grid item xs={12}>
          <Form
            initialValues={entry || {
              organizationPartyId: companyId,
              acctgTransId: acctgTransId,
              acctgTransEntrySeqId: "",
              acctgTransEntryTypeId: "_NA_",
              glAccountTypeId: "",
              glAccountId: "",
              debitCreditFlag: "D",
              partyId: "",
              origAmount: 0,
              origCurrencyUomId: "",
              purposeEnumId: "",
              voucherRef: "",
              productId: "",
              reconcileStatusId: "",
              settlementTermId: "",
              isSummary: "",
              description: "",
            }}
            onSubmit={handleCreate}
            render={(formRenderProps) => (
              <FormElement>
                <fieldset style={{ border: "none" }}>
                  <Grid container spacing={2}>
                    {/* Hidden fields */}
                    <input type="hidden" name="organizationPartyId" value={companyId} />
                    <input type="hidden" name="acctgTransId" value={acctgTransId} />
                    <input type="hidden" name="acctgTransEntrySeqId" />
                    <input type="hidden" name="acctgTransEntryTypeId" value="_NA_" />


                    <Grid item xs={6}>
                      <Field
                          id={"glAccountId"}
                          name={"glAccountId"}
                          label={"GL Account Id"}
                          data={glAccounts}
                          component={FormDropDownTreeGlAccount2}
                          dataItemKey={"glAccountId"}
                          textField={"text"}
                          selectField={"selected"}
                          expandField={"expanded"}
                          validator={requiredValidator}
                      />
                    </Grid>

                    {/* Row 2: Debit/Credit Flag & Party ID */}
                    <Grid item xs={6}>
                      <Field
                        name="debitCreditFlag"
                        label="Debit Credit Flag"
                        component={MemoizedFormDropDownList}
                        data={debitCreditOptions}
                        dataItemKey="debitCreditFlag"
                        textField="description"
                        validator={requiredValidator}
                      />
                    </Grid>
                   

                    {/* Row 3: Orig Amount & Orig Currency Uom ID */}
                    <Grid item xs={6}>
                      <Field
                        name="origAmount"
                        label="Orig Amount"
                        component={FormNumericTextBox}
                        format="n2"
                        min={0}
                      />
                    </Grid>

                    
                    <Grid item xs={6}>
                      <Field name="voucherRef" label="Voucher Ref" component={FormInput} />
                    </Grid>

                   

                    {/* Row 7: Description */}
                    <Grid item xs={12}>
                      <Field name="description" label="Description" component={FormInput} />
                    </Grid>

                    {/* Submit Button */}
                    <Grid item xs={12}>
                      <Button
                        variant="contained"
                        type="submit"
                        color="success"
                        disabled={!formRenderProps.allowSubmit || isAddEntryLoading || isUpdateEntryLoading}
                      >
                        {selectedMenuItem}
                      </Button>
                    </Grid>
                  </Grid>
                </fieldset>
                {(isAddEntryLoading || isUpdateEntryLoading) && (
                  <LoadingComponent message="Processing Transaction Entry..." />
                )}
              </FormElement>
            )}
          />
        </Grid>
      </Grid>
    </Paper>
  );
}