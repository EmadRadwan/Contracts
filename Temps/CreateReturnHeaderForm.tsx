import React, { useEffect, useState } from "react";
import {
  useFetchInternalAccountingOrganizationsLovQuery,
  useFetchReturnHeaderTypesQuery,
  useFetchReturnStatusItemsQuery,
} from "../../../../app/store/apis";
import Grid from "@mui/material/Grid";
import { Box, Paper, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import Button from "@mui/material/Button";
import { FormComboBoxVirtualCustomer } from "../../../../app/common/form/FormComboBoxVirtualCustomer";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import {
  radioGroupValidator,
  radioGroupValidatorReturnHeader,
  requiredValidator,
} from "../../../../app/common/form/Validators";
import {
  useAppDispatch,
  useFetchCompanyBaseCurrencyQuery,

  useFetchCurrenciesQuery,
} from "../../../../app/store/configureStore";
import useReturn from "../../hook/useReturn";
import {
  resetUiReturn,
  setSelectedReturn,
  setUiReturnItems,
} from "../../slice/returnUiSlice";
import { FormRadioGroup } from "../../../../app/common/form/FormRadioGroup";
import { FormComboBoxVirtualSupplier } from "../../../../app/common/form/FormComboBoxVirtualSupplier";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { RibbonContainer, Ribbon } from "react-ribbons";
import OrderMenu from "../../menu/OrderMenu";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import ReturnsMenu from "../../menu/ReturnsMenu";

interface Props {
  selectedReturn?: any;
  editMode: number;
  cancelEdit: () => void;
}

export default function CreateReturnHeaderForm({
  selectedReturn,
  cancelEdit,
  editMode,
}: Props) {
  const formRef = React.useRef<Form>(null);
  const formRef2 = React.useRef<boolean>(false);
  const [selectedMenuItem, setSelectedMenuItem] = React.useState("");

  const [isLoading, setIsLoading] = useState(false);
  const { data: returnTypesData } = useFetchReturnHeaderTypesQuery(undefined);
  const { data: currencies } = useFetchCurrenciesQuery(undefined);
  const [returnHeaderTypeChecked, setReturnHeaderTypeChecked] = React.useState<
  string | undefined
  >("CUSTOMER_RETURN");
  const { data: returnStatusItems, refetch } = useFetchReturnStatusItemsQuery(returnHeaderTypeChecked === "VENDOR_RETURN" ? "V" : "C");
  const {
    returnHeader,
    setReturnHeader,
    formEditMode,
    setFormEditMode,
    handleCreate,
    productStoreFacilities,
  } = useReturn({
    selectedMenuItem,
    editMode,
    formRef2,
    selectedReturn,
    setIsLoading,
  });
  const { getTranslatedLabel } = useTranslationHelper();

  const { data: companies } =
    useFetchInternalAccountingOrganizationsLovQuery(undefined);

  console.log("returnHeaderTypeChecked", returnHeaderTypeChecked);

  const dispatch = useAppDispatch();

  const returnHeaderTypeIdHandleChange = (e: any) => {
    setReturnHeaderTypeChecked(e.value);
  };

  useEffect(() => {
    console.log(returnHeader);
    setReturnHeaderTypeChecked(returnHeader?.returnHeaderTypeId)
  }, [formEditMode, returnHeader]);

  useEffect(() => {
    refetch()
  }, [returnHeaderTypeChecked])

  useEffect(() => {
    if (formEditMode < 2) {
      console.log("local editMode", editMode);
      console.count("local effect for resetting return");
      setReturnHeader({
        // @ts-ignore
        returnId: undefined,
        currencyUomId: baseCurrency?.currencyUomId,
        returnHeaderTypeId: "CUSTOMER_RETURN",
        entryDate: new Date(),
      });
    }
  }, [editMode, formEditMode, setReturnHeader]);

  const { data: baseCurrency } = useFetchCompanyBaseCurrencyQuery(undefined);

  useEffect(() => {
    if (selectedReturn) {
      dispatch(setSelectedReturn(selectedReturn));
      setReturnHeader({
        ...selectedReturn,
        entryDate: new Date(selectedReturn.entryDate),
      });
    } else {
      setReturnHeader({
        // @ts-ignore
        returnId: undefined,
        currencyUomId: baseCurrency?.currencyUomId,
        returnHeaderTypeId: "CUSTOMER_RETURN",
        entryDate: new Date(),
      });
      formRef2.current = !formRef2.current;
    }
  }, [baseCurrency?.currencyUomId, selectedReturn, setReturnHeader]);

  const renderSwitchReturnStatus = () => {
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
      case 5:
        return {
          label: "Requested",
          backgroundColor: "green",
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
    renderSwitchReturnStatus();
  }, [formEditMode, renderSwitchReturnStatus]);

  // menu select event handler
  async function handleMenuSelect(e: MenuSelectEvent) {
    if (e.item.text === "Create Return") {
      setSelectedMenuItem("Create Return");
      setTimeout(() => {
        // @ts-ignore
        formRef.current!.onSubmit();
      });
    }
    if (e.item.text === "Update Return") {
      setSelectedMenuItem("Update Return");
      setTimeout(() => {
        // @ts-ignore
        formRef.current!.onSubmit();
      });
    }
    if (e.item.text === "Approve Return") {
      setSelectedMenuItem("Approve Return");
      setTimeout(() => {
        // @ts-ignore
        formRef.current!.onSubmit();
      });
    }
    if (e.item.text === "New Return") {
      handleNewReturn();
    }
    if (e.item.text === "Complete Return") {
      setSelectedMenuItem("Complete Return");
      setTimeout(() => {
        // @ts-ignore
        formRef.current!.onSubmit();
      });
    }
  }

  const handleSubmit = (data: any) => {
    if (!data.isValid) {
      return false;
    }
    setIsLoading(true);
    handleCreate(data);
  };

  const handleNewReturn = () => {
    setReturnHeader({
      // @ts-ignore
      returnId: undefined,
      currencyUomId: baseCurrency?.currencyUomId,
      returnHeaderTypeId: "CUSTOMER_RETURN",
      entryDate: new Date(),
    });
    setFormEditMode(1);
    dispatch(setUiReturnItems([]));
    dispatch(resetUiReturn(null));
    formRef2.current = !formRef2.current;
  };

  const handleCancelForm = () => {
    dispatch(setUiReturnItems([]));
    dispatch(resetUiReturn(null));
    dispatch(setSelectedReturn(undefined));
    formRef2.current = !formRef2.current;
    cancelEdit();
  };
  const status = renderSwitchReturnStatus();
  return (
    <>
      <OrderMenu selectedMenuItem={"/returns"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container>
          <Grid container spacing={2}>
            {formEditMode === 1 ? (
              <>
                <Grid item xs={11}>
                  <Box display="flex" justifyContent="space-between" mt={2}>
                    <Typography color="green" sx={{ ml: 3 }} variant="h4">
                      New Return
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={1}>
                  <div className="col-md-6">
                    <Menu onSelect={handleMenuSelect}>
                      <MenuItem
                        text={getTranslatedLabel("general.actions", "Actions")}
                      >
                        {formEditMode === 1 && (
                          <MenuItem text="Create Return" />
                        )}
                        <MenuItem text="New Return" />
                      </MenuItem>
                    </Menu>
                  </div>
                </Grid>
              </>
            ) : (
              <>
                <Grid item xs={10} pb={0}>
                  <ReturnsMenu />
                </Grid>
                <Grid item xs={1}>
                  <div className="col-md-6">
                    <Menu onSelect={handleMenuSelect}>
                      <MenuItem
                        text={getTranslatedLabel("general.actions", "Actions")}
                      >
                        {formEditMode === 2 && (
                          <MenuItem text="Approve Return" />
                        )}
                        {formEditMode === 2 && (
                          <MenuItem text="Update Return" />
                        )}
                        {formEditMode === 3 && (
                          <MenuItem text="Complete Return" />
                        )}
                        <MenuItem text="New Return" />
                      </MenuItem>
                    </Menu>
                  </div>
                </Grid>
                {editMode > 1 && (
                  <Grid item xs={1}>
                    <RibbonContainer>
                      <Ribbon
                        side="right"
                        type="corner"
                        size="large"
                        backgroundColor={status.backgroundColor}
                        color={status.foreColor}
                        fontFamily="sans-serif"
                      >
                        {status.label}
                      </Ribbon>
                    </RibbonContainer>
                  </Grid>
                )}
                <Grid item xs={12}>
                  <Box display="flex" justifyContent="space-between">
                    <Typography color="black" sx={{ ml: 3 }} variant="h4">
                      {`Return Number ${returnHeader?.returnId || ""}`}
                    </Typography>
                  </Box>
                </Grid>
              </>
            )}
          </Grid>
        </Grid>

        <Form
          ref={formRef}
          initialValues={returnHeader}
          key={formRef2.current.toString()}
          onSubmitClick={(values) => handleSubmit(values)}
          render={() => (
            <FormElement>
              <fieldset className={"k-form-fieldset"}>
                <Grid
                  container
                  spacing={2}
                  alignItems="center"
                  sx={{ paddingLeft: 3 }}
                >
                  <Grid
                    item
                    xs={12}
                    className={
                      formEditMode > 1 ? "grid-disabled" : "grid-normal"
                    }
                  >
                    <Field
                      id={"returnHeaderTypeId"}
                      name={"returnHeaderTypeId"}
                      label={"Return Type"}
                      layout={"horizontal"}
                      component={FormRadioGroup}
                      validator={radioGroupValidatorReturnHeader}
                      onChange={returnHeaderTypeIdHandleChange}
                      defaultValue={"CUSTOMER_RETURN"}
                      disabled={formEditMode > 1}
                      data={returnTypesData}
                    />
                  </Grid>

                  <Grid item container xs={12} spacing={2}>
                    {returnHeaderTypeChecked === "CUSTOMER_RETURN" && (
                      <Grid
                        item
                        xs={3}
                        className={
                          formEditMode > 1 ? "grid-disabled" : "grid-normal"
                        }
                      >
                        <Field
                          id={"fromPartyId"}
                          name={"fromPartyId"}
                          label={"Customer *"}
                          component={FormComboBoxVirtualCustomer}
                          autoComplete={"off"}
                          validator={requiredValidator}
                          //disabled={formEditMode > 2}
                        />
                      </Grid>
                    )}

                    {returnHeaderTypeChecked === "VENDOR_RETURN" && (
                      <Grid
                        item
                        xs={3}
                        className={
                          formEditMode > 1 ? "grid-disabled" : "grid-normal"
                        }
                      >
                        <Field
                          id={"toPartyId"}
                          name={"toPartyId"}
                          label={"Supplier *"}
                          component={FormComboBoxVirtualSupplier}
                          autoComplete={"off"}
                          validator={requiredValidator}
                        />
                      </Grid>
                    )}

                    <Grid item xs={3}>
                      <Field
                        id={"company"}
                        name={
                          returnHeaderTypeChecked === "CUSTOMER_RETURN"
                            ? "toPartyId"
                            : "fromPartyId"
                        }
                        label={"Company *"}
                        component={MemoizedFormDropDownList2}
                        dataItemKey={"partyId"}
                        textField={"partyName"}
                        data={companies ?? []}
                        validator={requiredValidator}
                        disabled={editMode > 1}
                      />
                    </Grid>

                    <Grid item xs={3}>
                      <Field
                        id={"destinationFacilityId"}
                        name={"destinationFacilityId"}
                        label={"Destination Facility *"}
                        component={MemoizedFormDropDownList}
                        dataItemKey={"destinationFacilityId"}
                        textField={"facilityName"}
                        data={
                          productStoreFacilities ? productStoreFacilities : []
                        }
                        validator={requiredValidator}
                        disabled={editMode > 1}
                      />
                    </Grid>
                  </Grid>

                  {
                    <Grid item xs={3}>
                      <Field
                        id={"entryDate"}
                        name={"entryDate"}
                        label={"Entry Date"}
                        disabled={editMode > 1}
                        component={FormDatePicker}
                      />
                    </Grid>
                  }

                  <Grid item xs={3}>
                    <Field
                      id={"statusId"}
                      name={"statusId"}
                      label={"Status *"}
                      component={MemoizedFormDropDownList2}
                      dataItemKey={"statusId"}
                      textField={"description"}
                      data={returnStatusItems ?? []}
                      validator={requiredValidator}
                    />
                  </Grid>

                  <Grid item xs={3}>
                    <Field
                      id={"currencyUomId"}
                      name={"currencyUomId"}
                      label={"Currency *"}
                      component={MemoizedFormDropDownList2}
                      dataItemKey={"currencyUomId"}
                      textField={"description"}
                      data={currencies ?? []}
                      validator={requiredValidator}
                    />
                  </Grid>
                </Grid>

                {/* {formEditMode > 1 && (
                  <Grid container justifyContent="flex-start">
                    <Grid item xs={12}>
                      <ReturnItemsList
                        returnFormEditMode={formEditMode}
                        returnId={
                          returnHeader ? returnHeader.returnId : undefined
                        }
                      />
                    </Grid>
                  </Grid>
                )} */}

                <Grid container spacing={1}>
                  <Grid item>
                    <Button
                      sx={{ m: 2 }}
                      onClick={handleCancelForm}
                      color="error"
                      variant="contained"
                    >
                      Cancel
                    </Button>
                  </Grid>
                </Grid>

                {isLoading && (
                  <LoadingComponent message="Processing Return..." />
                )}
              </fieldset>
            </FormElement>
          )}
        />
      </Paper>
    </>
  );
}
