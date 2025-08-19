import React, { useEffect, useState } from "react";
import { useAppSelector } from "../../../../app/store/configureStore";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { Button, Grid } from "@mui/material";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import useInvoiceItem from "../hook/useInvoiceItem";
import { LoadingButton } from "@mui/lab";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import { FormMultiColumnComboBoxVirtualFacilityProduct } from "../../../../app/common/form/FormMultiColumnComboBoxVirtualProduct";
import { useFetchGlAccountOrganizationHierarchyLovQuery, useFetchInvoiceItemDataQuery } from "../../../../app/store/apis";
import { FormDropDownTreeGlAccount2 } from "../../../../app/common/form/FormDropDownTreeGlAccount2";

interface Props {
  invoiceItem?: any;
  editMode: number;
  onClose: () => void;
}

const InvoiceItemForm = ({ invoiceItem, editMode, onClose }: Props) => {
  const MyForm = React.useRef<any>();

  // Retrieve selected invoice from shared state.
  const selectedInvoice = useAppSelector(state => state.accountingSharedUi.selectedInvoice);
  const { user } = useAppSelector(state => state.account);
  const selectedInvoiceTypeId = selectedInvoice?.invoiceTypeId;
  const partyId = selectedInvoice?.partyId.fromPartyId;
  const partyIdFrom = selectedInvoice?.partyIdFrom.fromPartyId;
  const [isLoading, setIsLoading] = useState(false);

  const companyId = user?.organizationPartyId || "";
  const effectivePartyId = selectedInvoiceTypeId === "SALES_INVOICE" ? partyIdFrom : partyId;

  // Fetch LOVs for the form.
  const { data: invoiceData } = useFetchInvoiceItemDataQuery({
    invoiceTypeId: selectedInvoiceTypeId || "",
    partyId: effectivePartyId || "",
    partyIdFrom: partyIdFrom || ""
  });

  const {
    handleCreate,
    isCreateItemLoading,
    isUpdateItemLoading,
  } = useInvoiceItem({ editMode, invoiceItem, setIsLoading });

  const { data: glAccounts } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, { skip: companyId === undefined });


  const handleSubmitData = async (data: any) => {
    try {
      setIsLoading(true);

      await handleCreate(data);
    } catch (e) {
      console.error(e);
    }
  };

  return (
      <React.Fragment>
        <Form
            ref={MyForm}
            initialValues={invoiceItem || undefined}
            onSubmit={(values) => handleSubmitData(values)}
            render={(formRenderProps) => (
                <FormElement>
                  <Grid container spacing={2}>
                    {/* Invoice Item Type */}
                    <Grid item xs={12}>
                      <Field
                          id="invoiceItemTypeId"
                          name="invoiceItemTypeId"
                          label="Invoice Item Type *"
                          component={MemoizedFormDropDownList}
                          autoComplete="off"
                          dataItemKey="invoiceItemTypeId"
                          textField="description"
                          data={invoiceData?.invoiceItemTypes || []}
                          validator={requiredValidator}
                          disabled={editMode === 2}
                      />
                    </Grid>
                    {/* Product */}
                    <Grid item xs={12}>
                      <Field
                          id="productId"
                          name="productId"
                          label="Product"
                          component={FormMultiColumnComboBoxVirtualFacilityProduct}
                          autoComplete="off"
                          columnWidth="500px"
                          //validator={requiredValidator}
                          //disabled={editMode === 2}
                      />
                    </Grid>
                    {/* Quantity and Unit Price side-by-side */}
                    <Grid item xs={6}>
                      <Field
                          id="quantity"
                          name="quantity"
                          label="Quantity *"
                          format="n0"
                          min={1}
                          component={FormNumericTextBox}
                          validator={requiredValidator}
                      />
                    </Grid>
                    <Grid item xs={6}>
                      <Field
                          id="amount"
                          name="amount"
                          label="Amount *"
                          format="n0"
                          min={1}
                          component={FormNumericTextBox}
                          validator={requiredValidator}
                      />
                    </Grid>
                    {/* Override GL Account Id and Taxable Flag */}
                    <Grid item xs={12}>
                      <Field
                          id="glAccountId"
                          name="glAccountId"
                          label="Override GL Account Id"
                          data={glAccounts || []}
                          component={FormDropDownTreeGlAccount2}
                          dataItemKey="glAccountId"
                          textField="text"
                          selectField="selected"
                          expandField="expanded"
                      />
                    </Grid>
                    {/*<Grid item xs={6}>
                      <Field
                          id="taxableFlag"
                          name="taxableFlag"
                          label="Taxable Flag"
                          component={MemoizedFormDropDownList}
                          data={[
                            { value: "yes", text: "Yes" },
                            { value: "no", text: "No" }
                          ]}
                          dataItemKey="value"
                          textField="text"
                      />
                    </Grid>*/}
                    {/* Description */}
                    <Grid item xs={12}>
                      <Field
                          id="description"
                          name="description"
                          label="Description"
                          component={FormTextArea}
                          autoComplete="off"
                      />
                    </Grid>
                  </Grid>
                  <div className="k-form-buttons" style={{ marginTop: '16px' }}>
                    <Grid container spacing={2}>
                      <Grid item xs={5}>
                        <LoadingButton
                            size="large"
                            type="submit"
                            loading={isLoading}
                            variant="outlined"
                            disabled={!formRenderProps.allowSubmit}
                        >
                          {isLoading ? "Processing" : editMode === 2 ? "Update" : "Add"}
                        </LoadingButton>
                      </Grid>
                      <Grid item xs={2}>
                        <Button
                            onClick={onClose}
                            size="large"
                            color="error"
                            variant="outlined"
                        >
                          Cancel
                        </Button>
                      </Grid>
                    </Grid>
                  </div>
                </FormElement>
            )}
        />
      </React.Fragment>
  );
};

export const InvoiceItemFormMemo = React.memo(InvoiceItemForm);
