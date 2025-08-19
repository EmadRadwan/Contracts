import React, { useCallback, useEffect, useMemo, useState } from "react";

import Grid from "@mui/material/Grid";
import { Box, Paper, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import Button from "@mui/material/Button";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";

import {
  useAppDispatch,
  useAppSelector,
  useFetchCompanyBaseCurrencyQuery,
  useFetchCurrenciesQuery,
  useFetchCustomerTaxStatusQuery,
} from "../../../../app/store/configureStore";
import useQuote from "../../../services/hook/useQuote";
import {
  resetUiQuoteItems,
  setUiQuoteItems,
} from "../../slice/quoteItemsUiSlice";
import {
  resetUiQuoteAdjustments,
  setUiQuoteAdjustments,
} from "../../slice/quoteAdjustmentsUiSlice";
import { setCustomerId } from "../../slice/sharedOrderUiSlice";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import CreateCustomerModalForm from "../../../parties/form/CreateCustomerModalForm";
import { FormComboBoxVirtualCustomer } from "../../../../app/common/form/FormComboBoxVirtualCustomer";
import { requiredValidator } from "../../../../app/common/form/Validators";
import QuoteTotals from "../../../services/form/QuoteTotals";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import OrderMenu from "../../menu/OrderMenu";
import QuoteItemsList from "../../dashboard/quote/QuoteItemsList";
import { RibbonContainer, Ribbon } from "react-ribbons";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import { useFetchAgreementsByPartyIdQuery } from "../../../../app/store/apis";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { Quote } from "../../../../app/models/order/quote";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

interface Props {
  selectedQuote?: any;
  editMode: number;
  cancelEdit: () => void;
}

export default function QuoteForm({
  selectedQuote,
  cancelEdit,
  editMode,
}: Props) {
  const [showNewCustomer, setShowNewCustomer] = useState(false);
  const formRef = React.useRef<any>();
  const formRef2 = React.useRef<boolean>(false);
  const [selectedMenuItem, setSelectedMenuItem] = React.useState("");
  const { getTranslatedLabel } = useTranslationHelper();
  const [isLoading, setIsLoading] = useState(false);
  const language = useAppSelector((state) => state.localization.language);
  const customerId = useAppSelector(
    (state) => state.sharedOrderUi.selectedCustomerId
  );
  const { data: customerTaxStatus } = useFetchCustomerTaxStatusQuery(
    customerId,
    { skip: customerId === undefined }
  );
  const { data: currencies } = useFetchCurrenciesQuery(undefined);
  const { data: agreements } = useFetchAgreementsByPartyIdQuery(
    { partyId: customerId!, orderType: "SALES_ORDER" },
    {
      skip: !customerId,
    }
  );
  const { data: baseCurrency } = useFetchCompanyBaseCurrencyQuery(undefined);

  const { quote, setQuote, formEditMode, setFormEditMode, handleCreate } =
    useQuote({
      selectedMenuItem,
      editMode,
      formRef2,
      selectedQuote,
      setIsLoading,
    });

  const dispatch = useAppDispatch();
  const memoizedQuoteTotals = useMemo(() => <QuoteTotals />, [quote]);

  const quoteBaseValues: Quote = {
    quoteId: undefined,
    currencyUomId: baseCurrency?.currencyUomId,
    validFromDate: new Date(),
    issueDate: new Date(),
    validThruDate: new Date(new Date().setDate(new Date().getDate() + 30)),
  };

  useEffect(() => {
    if (selectedQuote) {
      console.log("selectedQuote", selectedQuote);
      setQuote(selectedQuote);
    } else {
      // setQuote({quoteId: undefined, currencyUomId: baseCurrency?.currencyUomId})
      setQuote(quoteBaseValues);
      formRef2.current = !formRef2.current;
    }
  }, [selectedQuote, setQuote, baseCurrency?.currencyUomId]);

  useEffect(() => {
    if (formEditMode < 2) {
      setQuote(quoteBaseValues);
    }
  }, [editMode, formEditMode, setQuote]);

  useEffect(() => {
    console.log(quote);
  }, [quote]);

  // menu select event handler
  async function handleMenuSelect(e: MenuSelectEvent) {
    if (e.item.text === "Create Quote") {
      setSelectedMenuItem("Create Quote");
      setTimeout(() => {
        formRef.current.onSubmit();
      });
    }
    if (e.item.text === "Update Quote") {
      setSelectedMenuItem("Update Quote");
      setTimeout(() => {
        formRef.current.onSubmit();
      });
    }
    if (e.item.text === "Create Order From Quote") {
      setSelectedMenuItem("Create Order From Quote");
      setTimeout(() => {
        formRef.current.onSubmit();
      });
    }
    if (e.item.text === "New Quote") {
      handleNewQuote();
    }
  }

  const handleSubmit = (data: any) => {
    if (!data.isValid) {
      return false;
    }
    setIsLoading(true);
    handleCreate(data);
  };

  const handleNewQuote = () => {
    setQuote(quoteBaseValues);
    setFormEditMode(1);
    dispatch(setUiQuoteItems([]));
    dispatch(resetUiQuoteAdjustments());
    dispatch(setUiQuoteAdjustments([]));
    formRef2.current = !formRef2.current;
  };

  const handleCancelForm = () => {
    dispatch(setUiQuoteItems([]));
    dispatch(resetUiQuoteItems());
    dispatch(resetUiQuoteAdjustments());
    dispatch(setUiQuoteAdjustments([]));
    formRef2.current = !formRef2.current;
    cancelEdit();
  };

  const updateCustomerDropDown = (newCustomer: any) => {
    // Logic to update the DropDown in the parent with this new customer.
    formRef?.current.onChange("fromPartyId", {
      value: newCustomer.fromPartyId,
      valid: true,
    });
    dispatch(setCustomerId(newCustomer.fromPartyId.fromPartyId));
  };

  const renderSwitchStatus = () => {
    switch (formEditMode) {
      case 1:
        return { label: "New", backgroundColor: "green", foreColor: "#ffffff" };
      case 2:
        return {
          label: "Created",
          backgroundColor: "green",
          foreColor: "#ffffff",
        };
      case 3:
        return {
          label: "Approved",
          backgroundColor: "yellow",
          foreColor: "#000000",
        }; // Black text on yellow
      case 4:
        return {
          label: "Completed",
          backgroundColor: "blue",
          foreColor: "#ffffff",
        };
      default:
        return {
          label: "Unknown",
          backgroundColor: "gray",
          foreColor: "#ffffff",
        };
    }
  };

  useEffect(() => {
    renderSwitchStatus();
  }, [formEditMode, renderSwitchStatus]);

  // const updateVehicleDropDown = (newVehicle: any, customer: any) => {
  //     formRef?.current.onChange("vehicleId", {value: newVehicle, valid: true});
  //     formRef?.current.onChange("fromPartyId", {value: customer, valid: true});
  //     dispatch(setCustomerId(customer.fromPartyId));
  //     dispatch(dispatch(setVehicleId(newVehicle.vehicleId)));
  //     dispatch(setSelectedVehicle(newVehicle));
  //     setVehicle(newVehicle);
  // };

  const onCustomerChange = useCallback((event: any) => {
    if (event.value === null) {
      dispatch(setCustomerId(undefined));
      dispatch(resetUiQuoteItems());
      dispatch(resetUiQuoteAdjustments());
    } else {
      dispatch(setCustomerId(event.value.fromPartyId));
    }
  }, []);

  const memoizedOnClose2 = useCallback(() => {
    setShowNewCustomer(false);
  }, []);

  const status = renderSwitchStatus();

  return (
    <>
      {showNewCustomer && (
        <ModalContainer
          show={showNewCustomer}
          onClose={memoizedOnClose2}
          width={500}
        >
          <CreateCustomerModalForm
            onClose={memoizedOnClose2}
            onUpdateCustomerDropDown={updateCustomerDropDown}
          />
        </ModalContainer>
      )}

      <OrderMenu selectedMenuItem="quotes" />
      <RibbonContainer>
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
          <Grid container spacing={2} alignItems={"center"}>
            <Grid item xs={10}>
              <Box display="flex" justifyContent="space-between">
                <Typography
                  sx={{
                    fontWeight: "bold",
                    paddingLeft: 3,
                    fontSize: "18px",
                    color: formEditMode === 1 ? "green" : "black",
                  }}
                  variant="h6"
                >
                  {" "}
                  {quote && quote?.quoteId
                    ? `Quote No: ${quote?.quoteId}`
                    : "New Quote    "}{" "}
                </Typography>
              </Box>
            </Grid>

            <Grid item xs={2}>
              <div>
                <Menu onSelect={handleMenuSelect}>
                  <MenuItem
                    text={getTranslatedLabel("general.actions", "Actions")}
                  >
                    {formEditMode === 1 && <MenuItem text="Create Quote" />}
                    {formEditMode > 1 && <MenuItem text="Update Quote" />}
                    {formEditMode > 1 && (
                      <MenuItem text="Create Order From Quote" />
                    )}
                    {/* <MenuItem text="Duplicate Job Quote"/> */}
                    <MenuItem text="New Quote" />
                  </MenuItem>
                </Menu>
              </div>
            </Grid>
            <Grid item xs={1}>
              {formEditMode > 1 && (
                <Ribbon
                  side={language === "ar" ? "left" : "right"}
                  type="corner"
                  size="large"
                  withStripes
                  backgroundColor={status.backgroundColor}
                  color={status.foreColor}
                  fontFamily="sans-serif"
                >
                  <Typography
                    variant="h4"
                    sx={{ fontSize: language === "ar" ? "1.1rem" : "0.9rem" }}
                  >
                    {status.label}
                  </Typography>
                </Ribbon>
              )}
            </Grid>
          </Grid>

          <Form
            ref={formRef}
            initialValues={quote}
            key={formRef2.current.toString()}
            onSubmitClick={(values) => handleSubmit(values)}
            render={() => (
              <FormElement>
                <fieldset className={"k-form-fieldset"}>
                  <Grid
                    container
                    alignItems={"start"}
                    justifyContent="flex-start"
                    spacing={1}
                  >
                    <Grid
                      container
                      spacing={2}
                      alignItems="center"
                      justifyContent={"start"}
                      xs={12}
                      sx={{ paddingLeft: 3 }}
                    >
                      <Grid item container xs={10} spacing={2}>
                        <Grid
                          container
                          item
                          xs={12}
                          alignItems={"flex-end"}
                          spacing={2}
                        >
                          <Grid
                            item
                            xs={3}
                            className={
                              formEditMode > 2 ? "grid-disabled" : "grid-normal"
                            }
                          >
                            <Field
                              id={"fromPartyId"}
                              name={"fromPartyId"}
                              label={
                                customerId !== undefined &&
                                customerTaxStatus &&
                                customerTaxStatus.isExempt !== "N" ? (
                                  <span style={{ color: "red" }}>
                                    Customer - Tax Exempt
                                  </span>
                                ) : (
                                  "Customer"
                                )
                              }
                              component={FormComboBoxVirtualCustomer}
                              autoComplete={"off"}
                              validator={requiredValidator}
                              onChange={onCustomerChange}
                              disabled={formEditMode > 2}
                            />
                          </Grid>
                          <Grid item xs={2}>
                            <Button
                              color={"secondary"}
                              onClick={() => {
                                setShowNewCustomer(true);
                              }}
                              variant="outlined"
                            >
                              New Customer
                            </Button>
                          </Grid>
                          <Grid item xs={3}>
                            <Field
                              id="currencyUomId"
                              name="currencyUomId"
                              component={MemoizedFormDropDownList2}
                              data={currencies ?? []}
                              label={"Currency"}
                              dataItemKey={"currencyUomId"}
                              disabled={formEditMode > 1}
                              textField={"description"}
                            />
                          </Grid>
                          {agreements &&
                            agreements?.length > 0 &&
                            customerId !== undefined && (
                              <Grid item xs={3}>
                                <Field
                                  id="agreementId"
                                  name="agreementId"
                                  label={"Agreement"}
                                  component={MemoizedFormDropDownList2}
                                  data={agreements ?? []}
                                  dataItemKey={"agreementId"}
                                  textField={"description"}
                                />
                              </Grid>
                            )}
                        </Grid>
                        <Grid container item xs={9} spacing={2}>
                          <Grid item xs={4}>
                            <Field
                              id={"issueDate"}
                              name={"issueDate"}
                              label={"Issue Date"}
                              component={FormDatePicker}
                              autoComplete={"off"}
                              disabled={formEditMode > 2}
                            />
                          </Grid>
                          <Grid item xs={4}>
                            <Field
                              id={"validFromDate"}
                              name={"validFromDate"}
                              label={"Valid From"}
                              component={FormDatePicker}
                              autoComplete={"off"}
                              disabled={formEditMode > 2}
                            />
                          </Grid>
                          <Grid item xs={4}>
                            <Field
                              id={"validThruDate"}
                              name={"validThruDate"}
                              label={"Valid Thru"}
                              component={FormDatePicker}
                              autoComplete={"off"}
                              disabled={formEditMode > 2}
                            />
                          </Grid>
                        </Grid>
                        <Grid container item xs={12} spacing={2}>
                          <Grid
                            item
                            xs={3}
                            className={
                              formEditMode > 2 ? "grid-disabled" : "grid-normal"
                            }
                          >
                            <Field
                              id={"customerRemarks"}
                              name={"customerRemarks"}
                              label={"Customer Remarks"}
                              component={FormTextArea}
                              autoComplete={"off"}
                              disabled={formEditMode > 2}
                            />
                          </Grid>
                          <Grid
                            item
                            xs={3}
                            className={
                              formEditMode > 2 ? "grid-disabled" : "grid-normal"
                            }
                          >
                            <Field
                              id={"internalRemarks"}
                              name={"internalRemarks"}
                              label={"Internal Remarks"}
                              component={FormTextArea}
                              autoComplete={"off"}
                              disabled={formEditMode > 2}
                            />
                          </Grid>
                        </Grid>
                      </Grid>
                      <Grid
                        item
                        container
                        xs={2}
                        spacing={2}
                        alignItems="flex-end"
                      >
                        {memoizedQuoteTotals}
                      </Grid>
                    </Grid>

                    <Grid
                      container
                      justifyContent={"flex-start"}
                      alignItems={"center"}
                      sx={{ ml: 2, mt: 3 }}
                    >
                      <Grid item xs={9}>
                        <QuoteItemsList
                          quoteFormEditMode={formEditMode}
                          quoteId={quote ? quote.quoteId : undefined}
                        />
                      </Grid>
                    </Grid>
                    <Grid container spacing={2} alignItems="flex-end">
                      <Grid item xs={10}>
                        <Button
                          onClick={handleCancelForm}
                          variant="contained"
                          color="error"
                        >
                          Back
                        </Button>
                      </Grid>
                    </Grid>

                    {isLoading && (
                      <LoadingComponent message="Processing Job Quote..." />
                    )}
                  </Grid>
                </fieldset>
              </FormElement>
            )}
          />
        </Paper>
      </RibbonContainer>
    </>
  );
}
