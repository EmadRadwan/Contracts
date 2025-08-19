import React, {  useRef, useState } from "react";
import AccountingMenu from "../menu/AccountingMenu";
import { Box, Button, Grid, Paper, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { useFetchInvoiceTypesQuery } from "../../../../app/store/apis/invoice/invoicesApi";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { FormComboBoxVirtualCustomer } from "../../../../app/common/form/FormComboBoxVirtualCustomer";
import { FormComboBoxVirtualParty } from "../../../../app/common/form/FormComboBoxVirtualParty";
import { useFetchCompaniesQuery } from "../../../../app/store/apis";
import { Invoice } from "../../../../app/models/accounting/invoice";
import useInvoice from "../hook/useInvoice";
import { useAppDispatch } from "../../../../app/store/configureStore";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";

interface Props {
  editMode: number;
  onClose: () => void;
}

const CreateInvoiceForm = ({ editMode, onClose }: Props) => {
  const { data: invoiceTypes } = useFetchInvoiceTypesQuery(undefined);
  const { data: companies } = useFetchCompaniesQuery(undefined);
  const formRef = useRef<Form | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.invoices.form";

  console.log(invoiceTypes);
  // used to differenciate btwn sales (1) and purchase (2)
  const [invoiceType, setInvoiceType] = useState(0);
  const dispatch = useAppDispatch();

  const { invoice, handleCreate } = useInvoice({
    editMode, setIsLoading
  });

  const handleSelectInvoiceType = (e: { value: string }) => {
    console.log(e);
    if (formRef?.current) {
      if (formRef.current.values.partyId)
        formRef.current.onChange("partyId", { value: null });
      if (formRef.current.values.partyIdFrom)
        formRef.current.onChange("partyIdFrom", { value: null });
    }
    let selectedInvoiceType = invoiceTypes?.find(
      (i: any) => i.invoiceTypeId === e.value
    );
    if (
      [
        selectedInvoiceType!.invoiceTypeId,
        selectedInvoiceType!.parentTypeId,
      ].includes("SALES_INVOICE")
    ) {
      setInvoiceType(1);
    } else {
      setInvoiceType(2);
    }
  };

  const onSubmitInvoice = async (values: Invoice) => {
    try {
      setIsLoading(true);

      await handleCreate(values);
      onClose()

    } catch (e) {
      console.error(e);
    }
    // setLocalInvoiceData(data)
  };
  return (
    <>
      <AccountingMenu selectedMenuItem={"/invoices"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container spacing={2}>
          <Grid item xs={5}>
            {
              <Box display="flex" justifyContent="space-between">
                <Typography sx={{ p: 2 }} color={"green"} variant="h4">
                  {" "}
                  {"New Invoice"}{" "}
                </Typography>
              </Box>
            }
          </Grid>
        </Grid>
        <Form
          onSubmit={(values) => onSubmitInvoice(values as Invoice)}
          ref={formRef}
          render={(formRenderProps) => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid
                  container
                  spacing={2}
                  flexDirection={"column"}
                >
                  <Grid item xs={3}>
                    <Field
                      name="invoiceTypeId"
                      id="invoiceTypeId"
                      label="Invoice Type"
                      component={MemoizedFormDropDownList}
                      data={invoiceTypes ? invoiceTypes : []}
                      dataItemKey="invoiceTypeId"
                      textField="description"
                      validator={requiredValidator}
                      onChange={handleSelectInvoiceType}
                    />
                  </Grid>
                  <Grid item container spacing={2} flexDirection={"row"}>
                    <Grid item xs={3}>
                      <Field
                        name="organizationPartyId"
                        id="organizationPartyId"
                        label="Organization Party *"
                        component={MemoizedFormDropDownList}
                        data={companies ? companies : []}
                        dataItemKey="organizationPartyId"
                        textField="organizationPartyName"
                        validator={requiredValidator}
                      />
                    </Grid>
                    {invoiceType === 1 && (
                      <Grid item xs={3}>
                        <Field
                          name="partyIdFrom"
                          id="partyIdFrom"
                          label="From Party"
                          component={FormComboBoxVirtualCustomer}
                          validator={requiredValidator}
                        />
                      </Grid>
                    )}
                    {invoiceType === 2 && (
                      <Grid item xs={3}>
                        <Field
                          name="partyId"
                          id="partyId"
                          label="To Party"
                          component={FormComboBoxVirtualParty}
                          validator={requiredValidator}
                        />
                      </Grid>
                    )}
                  </Grid>
                </Grid>
                {(isLoading) && (
                    <LoadingComponent
                        message={getTranslatedLabel(
                            `${localizationKey}.loading`,
                            "Processing Invoice..."
                        )}
                    />
                )}
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
                  onClick={onClose}
                >
                  Back
                </Button>
              </div>
            </FormElement>
          )}
        />
      </Paper>
    </>
  );
};

export default CreateInvoiceForm;
