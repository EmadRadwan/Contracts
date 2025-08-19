import AccountingMenu from "../menu/AccountingMenu";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { Invoice } from "../../../../app/models/accounting/invoice";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { useFetchCurrenciesQuery } from "../../../../app/store/configureStore";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { useFetchInvoiceTypesQuery } from "../../../../app/store/apis/invoice/invoicesApi";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { useEffect, useState } from "react";
import { handleDatesObject } from "../../../../app/util/utils";
import FormInput from "../../../../app/common/form/FormInput";
import { FormComboBoxVirtualBillingAccount } from "../../../../app/common/form/FormComboBoxVirtualBillingAccount";
import useInvoice from "../hook/useInvoice";
import { toast } from "react-toastify";

interface Props {
  selectedInvoice: any;
  editMode: number;
  cancelEdit: () => void;
}

const InvoiceForm = ({ editMode, selectedInvoice, cancelEdit }: Props) => {
  // get data for billing account, inv item types and role id then construct the edit form with all fields
  // like ofbiz and set the save to the update mutation and the cancel to go back to the invoices list
  const {invoice, handleCreate} = useInvoice({editMode, selectedInvoice})
  console.log(invoice);
  const { data: currencies } = useFetchCurrenciesQuery(undefined);
  const { data: invoiceTypes } = useFetchInvoiceTypesQuery(undefined);
  const [formInvoice, setFormInvoice] = useState<Invoice>();
  const [formKey, setFormKey] = useState(Math.random());

  useEffect(() => {
    if (invoice) {
      let inv = handleDatesObject({
        ...invoice,
        partyIdFrom: invoice?.partyIdFrom?.fromPartyName ? invoice?.partyIdFrom?.fromPartyName : invoice?.partyIdFrom,
        partyId: invoice?.partyId?.fromPartyName ? invoice?.partyId?.fromPartyName :  invoice?.partyId
      });
      setFormInvoice(inv);
      setFormKey(Math.random());
    }
  }, [invoice]);

  function cleanObject(obj: any) {
    for (let key in obj) {
        if (obj[key] === null || obj[key] === '' || obj[key] === undefined) {
            delete obj[key];
        }
    }
    return obj;
  }

  const onFormSubmit = (values: any) => {
    let invoice = {...values}
    if (invoice["billingAccountId"]) {
      if (typeof invoice["billingAccountId"] === "object") {
        invoice.billingAccountId = values.billingAccountId.billingAccountId
      }
    }
    try {
      handleCreate(cleanObject(invoice))
      toast.success(`Invoice updated successfully`)
    } catch (e) {
      console.error(e)
      toast.error("Something went wrong")
    }
  }

  return (
    <>
      <AccountingMenu selectedMenuItem="invoices" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item xs={7}>
            <Box
              display="flex"
              flexDirection={"row"}
              justifyContent="space-between"
            >
              <Typography
                sx={{
                  fontWeight: "bold",
                  paddingLeft: 3,
                  fontSize: "18px",
                  color: "black",
                }}
                variant="h6"
              >
                {`#${invoice?.invoiceId} `}
              </Typography>
              <Typography
                sx={{
                  paddingLeft: 3,
                  fontSize: "18px",
                }}
                variant="h6"
              >
                <span style={{ color: "black" }}>{`Status: `}</span>
                <span style={{ color: "green" }}>
                  {invoice?.statusDescription}
                </span>
              </Typography>
            </Box>
          </Grid>
          {/* <Grid item xs={5}>
            <InvoiceMenu />
          </Grid> */}
          <Grid item xs={12}>
            <Form
              initialValues={formInvoice ? formInvoice : undefined}
              key={formKey}
              onSubmit={onFormSubmit}
              render={(formRenderProps) => (
                <FormElement>
                  <fieldset className={"k-form-fieldset"}>
                    <Grid container spacing={2}>
                      <Grid item xs={4}>
                        <Field
                          name="invoiceTypeId"
                          id="invoiceTypeId"
                          label="Invoice Type"
                          component={MemoizedFormDropDownList}
                          data={invoiceTypes ? invoiceTypes : []}
                          dataItemKey="invoiceTypeId"
                          textField="description"
                          disabled
                        />
                      </Grid>
                      <Grid item xs={4}>
                        <Field
                          name="currencyUomId"
                          id="currencyUomId"
                          label="Currency *"
                          component={MemoizedFormDropDownList}
                          data={currencies ? currencies : []}
                          dataItemKey="currencyUomId"
                          textField="description"
                          validator={requiredValidator}
                        />
                      </Grid>
                      <Grid item xs={4}>
                        <Field
                          name="invoiceDate"
                          id="invoiceDate"
                          label="Invoice Date"
                          component={FormDatePicker}
                          validator={requiredValidator}
                        />
                      </Grid>
                      {formInvoice?.paidDate === null && (
                        <Grid item xs={4}>
                          <Field
                            name="dueDate"
                            id="dueDate"
                            label="Due Date"
                            component={FormDatePicker}
                            validator={requiredValidator}
                          />
                        </Grid>
                      )}
                      <Grid item xs={4}>
                        <Field
                          name="paidDate"
                          id="paidDate"
                          label="Paid Date"
                          component={FormDatePicker}
                          validator={requiredValidator}
                        />
                      </Grid>
                      <Grid item xs={4}>
                        <Field
                          name="partyIdFrom"
                          id="partyIdFrom"
                          label="From Party"
                          component={FormInput}
                          disabled
                        />
                      </Grid>
                      <Grid item xs={4}>
                        <Field
                          name="partyId"
                          id="partyId"
                          label="To Party"
                          component={FormInput}
                          disabled
                        />
                      </Grid>
                      <Grid item xs={4}>
                        <Field
                          name="billingAccountId"
                          label="Billing Account"
                          id="billingAccountId"
                          component={FormComboBoxVirtualBillingAccount}
                        />
                      </Grid>
                    </Grid>
                  </fieldset>
                  <div className="k-form-buttons">
                    <Button
                      variant="contained"
                      type={"submit"}
                      color="success"
                      disabled={!formRenderProps.allowSubmit}
                    >
                      Save
                    </Button>

                    <Button
                      variant="contained"
                      type={"button"}
                      color="error"
                      onClick={cancelEdit}
                    >
                      Back
                    </Button>
                  </div>
                </FormElement>
              )}
            />
          </Grid>
        </Grid>
      </Paper>
    </>
  );
};

export default InvoiceForm;
