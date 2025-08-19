import React, { useEffect, useState } from "react";
import Grid from "@mui/material/Grid";
import { Paper, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import Button from "@mui/material/Button";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";


import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { requiredValidator } from "../../../../app/common/form/Validators";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { useFetchAcctgTransTypesQuery } from "../../../../app/store/apis";
import { AcctgTrans } from "../../../../app/models/accounting/acctgTrans";
import useEditAcctgTrans from "../hook/useEditAcctgTrans";
import { useLocation } from "react-router";
import SetupAccountingMenu from "../../organizationGlSettings/menu/SetupAccountingMenu";
import AccountingSummaryMenu from "../../organizationGlSettings/menu/AccountingSummaryMenu";
import { FormComboBoxVirtualParty } from "../../../../app/common/form/FormComboBoxVirtualParty";
import FormInput from "../../../../app/common/form/FormInput";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import AcctgTransEntryList from "../dashboard/AcctgTransEntryList";
import {useAppSelector} from "../../../../app/store/configureStore";
import {router} from "../../../../app/router/Routes";

interface Props {
  selectedAcctgTrans?: AcctgTrans;
}

export default function EditAcctgTrans({ selectedAcctgTrans }: Props) {
  const formRef = React.useRef<any>();
  const [selectedMenuItem, setSelectedMenuItem] = React.useState("");

  const [isLoading, setIsLoading] = useState(false);
  const { data: acctgTransTypes } = useFetchAcctgTransTypesQuery(undefined);
  const { getTranslatedLabel } = useTranslationHelper();
  const companyId = useAppSelector(state => state.accountingSharedUi.selectedAccountingCompanyId);

  const { acctgTrans, setAcctgTrans, handleUpdate, handlePostTransaction } = useEditAcctgTrans({
    selectedMenuItem,
    selectedAcctgTrans,
    setIsLoading,
  });

  const location = useLocation();
  const acctgTransToBeUsed =
      selectedAcctgTrans || location.state?.selectedAcctgTrans;
  

  useEffect(() => {
    if (!companyId) {
      router.navigate("/orgGl");
    }
  }, [companyId]);


  useEffect(() => {
    if (acctgTransToBeUsed) {
      const sanitizedAcctgTrans = {
        ...acctgTransToBeUsed,
        transactionDate: new Date(acctgTransToBeUsed.transactionDate),
        paymentId: acctgTransToBeUsed.paymentId ?? '', // Convert null to empty string
        invoiceId: acctgTransToBeUsed.invoiceId ?? '', // Convert null to empty string
        fixedAssetId: acctgTransToBeUsed.fixedAssetId ?? '', // Convert null to empty string
        description: acctgTransToBeUsed.description ?? '', // Convert null to empty string
        acctgTransId: acctgTransToBeUsed.acctgTransId || '', // REFACTORED: Ensure acctgTransId is always a string
      };
      setAcctgTrans(sanitizedAcctgTrans);
    }
  }, [acctgTransToBeUsed, setAcctgTrans]);
  console.log("AcctgTrans", acctgTrans);

  // REFACTORED: Modified to trigger post transaction backend function
  async function handleMenuSelect(e: MenuSelectEvent) {
    if (e.item.text === "Post Transaction") {
      if (acctgTrans?.acctgTransId) {
        handlePostTransaction(acctgTrans.acctgTransId);
      }
    }
  }


  const handleSubmit = (data: any) => {
    if (!data.isValid) {
      return false;
    }
    setIsLoading(true);
    handleUpdate(data);
  };

 
  return (
      <>
        <AccountingMenu selectedMenuItem={"/orgGl"} />
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
          <SetupAccountingMenu selectedMenuItem="orgChartOfAccount" />
          <AccountingSummaryMenu selectedMenuItem="accountingTransactionEntries" />
          <Form
              ref={formRef}
              initialValues={acctgTrans}
              key={acctgTrans?.acctgTransId}
              onSubmitClick={(values) => handleSubmit(values)}
              render={(formRenderProps) => (
                  <FormElement>
                    <fieldset className="k-form-fieldset">
                      <Grid container spacing={2}>
                        {/* Header Section */}
                        <Grid item xs={12}>
                          <Grid container spacing={2} alignItems="center">
                            <Grid item xs={4}>
                              <Typography
                                  sx={{
                                    fontWeight: "bold",
                                    fontSize: "16px",
                                    color: "black",
                                  }}
                                  variant="h6"
                              >
                                Accounting Transaction No:{" "}
                                <span style={{ color: "blue" }}>
                            {acctgTrans ? `${acctgTrans.acctgTransId}` : ""}
                          </span>
                              </Typography>
                            </Grid>
                            <Grid item xs={8} container justifyContent="flex-end">
                              <Menu onSelect={handleMenuSelect}>
                                <MenuItem
                                    text={getTranslatedLabel(
                                        "general.actions",
                                        "Actions"
                                    )}
                                >
                                  <MenuItem text="Post Transaction" />
                                </MenuItem>
                              </Menu>
                            </Grid>
                          </Grid>
                        </Grid>

                        {/* Form Fields Section - First Row */}
                        <Grid item xs={12} >
                          <Grid container spacing={2}>
                            <Grid item xs={2}>
                              <Field
                                  id={"acctgTransTypeId"}
                                  name={"acctgTransTypeId"}
                                  label={"Acctg Trans Type *"}
                                  component={MemoizedFormDropDownList}
                                  dataItemKey={"acctgTransTypeId"}
                                  textField={"description"}
                                  disabled={acctgTransToBeUsed?.isPosted === "Y"}
                                  data={acctgTransTypes ? acctgTransTypes : []}
                                  validator={requiredValidator}
                              />
                            </Grid>
                            <Grid item xs={2}>
                              <Field
                                  id="transactionDate"
                                  name="transactionDate"
                                  disabled={acctgTransToBeUsed?.isPosted === "Y"}
                                  label="Transaction Date *"
                                  component={FormDatePicker}
                                  validator={requiredValidator}
                              />
                            </Grid>
                            <Grid item xs={2}>
                              <Field
                                  id="fromPartyId"
                                  name="fromPartyId"
                                  label="Party Id"
                                  disabled={acctgTransToBeUsed?.isPosted === "Y"}
                                  component={FormComboBoxVirtualParty}
                                  autoComplete="off"
                              />
                            </Grid>
                            <Grid item xs={2}>
                              <Field
                                  id="paymentId"
                                  name="paymentId"
                                  label="Payment Id"
                                  disabled={acctgTransToBeUsed?.isPosted === "Y"}
                                  component={FormInput}
                                  autoComplete="off"
                              />
                            </Grid>
                            <Grid item xs={2}>
                              <Field
                                  id="invoiceId"
                                  name="invoiceId"
                                  label="Invoice Id"
                                  disabled={acctgTransToBeUsed?.isPosted === "Y"}
                                  component={FormInput}
                                  autoComplete="off"
                              />
                            </Grid>
                            <Grid item xs={2}>
                              <Field
                                  id="fixedAssetId"
                                  name="fixedAssetId"
                                  label="Fixed Asset"
                                  disabled={acctgTransToBeUsed?.isPosted === "Y"}
                                  component={FormInput}
                                  autoComplete="off"
                              />
                            </Grid>
                          </Grid>
                        </Grid>
                        
                        <Grid item xs={12}>
                          <Grid container spacing={2} alignItems="flex-end">
                            <Grid item xs={6}>
                              <Field
                                  id="description"
                                  name="description"
                                  label="Description"
                                  disabled={acctgTransToBeUsed?.isPosted === "Y"}
                                  component={FormInput}
                                  autoComplete="off"
                              />
                            </Grid>
                            <Grid item xs={2} >
                              <Button
                                  variant="contained"
                                  type="submit"
                                  color="success"
                                  disabled={!formRenderProps.allowSubmit}
                              >
                                Edit
                              </Button>
                            </Grid>
                          </Grid>
                          </Grid>
                        </Grid>

                       <Grid item xs={12}>
                          <Grid container spacing={1} >
                            <Grid item xs={11}>
                              {acctgTrans && acctgTrans.acctgTransId && (
                                  <AcctgTransEntryList acctgTrans={acctgTrans} />
                              )}
                            </Grid>
                          </Grid>
                        </Grid>

                       

                        {/* Submit Button */}
                        

                      {/* Loading Indicator */}
                      {isLoading && (
                          <LoadingComponent message="Processing Accounting Transaction..." />
                      )}
                    </fieldset>
                  </FormElement>
              )}
          />
        </Paper>
      </>
  );
}