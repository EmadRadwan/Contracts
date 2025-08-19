import React, { useEffect, useState, useRef } from "react";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { Box, Grid, Paper, Typography } from "@mui/material";
import Button from "@mui/material/Button";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import { RibbonContainer, Ribbon } from "react-ribbons";
import {
    useFetchInternalAccountingOrganizationsLovQuery,
    useFetchReturnStatusItemsQuery,
    useFetchPaymentMethodsQuery,
    useFetchBillingAccountsForPartyQuery,
} from "../../../../app/store/apis";
import {
    useAppDispatch,
    useFetchCompanyBaseCurrencyQuery,
    useFetchCurrenciesQuery,
} from "../../../../app/store/configureStore";
import {
    resetUiReturn,
    setSelectedReturn,
    setUiReturnItems,
} from "../../slice/returnUiSlice";
import useReturn from "../../hook/useReturn";
import {
    radioGroupValidatorReturnHeader,
    requiredValidator,
} from "../../../../app/common/form/Validators";
import { FormRadioGroup } from "../../../../app/common/form/FormRadioGroup";
import { FormComboBoxVirtualCustomer } from "../../../../app/common/form/FormComboBoxVirtualCustomer";
import { FormComboBoxVirtualSupplier } from "../../../../app/common/form/FormComboBoxVirtualSupplier";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import OrderMenu from "../../menu/OrderMenu";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import ReturnsMenu from "../../menu/ReturnsMenu";
import { Return } from "../../../../app/models/order/return";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import { useFetchFinAccountsForPartyQuery } from "../../../../app/store/apis/accounting/financialAccountsApi";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";

interface Props {
    selectedReturn?: Return;
    editMode: number;
    cancelEdit: () => void;
    handleNewReturn: () => void; // Added prop for new return handler
}

export default function EditReturn({ selectedReturn, editMode, cancelEdit, handleNewReturn }: Props) {
    const formRef = useRef<Form>(null);
    const formRef2 = useRef<boolean>(false);
    const [selectedMenuItem, setSelectedMenuItem] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();

    const returnTypesData = [
        { value: "CUSTOMER_RETURN", label: "Return from Customer" },
        { value: "VENDOR_RETURN", label: "Return to Vendor" }
    ];
    const { data: currencies, isFetching: isFetchingCurrencies } = useFetchCurrenciesQuery(undefined);
    const { data: baseCurrency, isFetching: isFetchingBaseCurrency } = useFetchCompanyBaseCurrencyQuery(undefined);
    const { data: rawCompanies, isFetching: isFetchingCompanies } = useFetchInternalAccountingOrganizationsLovQuery(undefined);

    const [formInitialValues, setFormInitialValues] = useState({
        ...selectedReturn,
        currencyUomId: selectedReturn?.currencyUomId || "",
        statusId: editMode === 1 ? "RETURN_REQUESTED" : selectedReturn?.statusId || "",
        entryDate: editMode === 1 ? new Date() : selectedReturn?.entryDate || null,
        needsInventoryReceive: editMode === 1 ? "Y" : selectedReturn?.needsInventoryReceive || "Y",
        returnHeaderTypeId: editMode === 1 ? "CUSTOMER_RETURN" : selectedReturn?.returnHeaderTypeId || "CUSTOMER_RETURN",
    });
    
    const { data: returnStatusItems, isFetching: isFetchingReturnStatusItems, refetch: refetchStatuses } = useFetchReturnStatusItemsQuery(
        {
            returnId: editMode === 1 ? "" : selectedReturn?.returnId,
            returnHeaderType: editMode === 1 ? (formInitialValues.returnHeaderTypeId === "VENDOR_RETURN" ? "V" : "C") : selectedReturn?.returnHeaderTypeId === "VENDOR_RETURN" ? "V" : "C",
        },
        { skip: !selectedReturn && editMode !== 1 }
    );
    // Map company data to rename partyId to companyId
    const companies = rawCompanies?.map(company => ({
        ...company,
        companyId: company.partyId,
        partyId: undefined
    }));
    
    const [partyId, setPartyId] = useState<string | undefined>(undefined);

    
    

    
    console.log('returnStatusItems', returnStatusItems)

    const {
        data: billingAccounts,
        isFetching: isFetchingBillingAccounts,
    } = useFetchBillingAccountsForPartyQuery(partyId, {
        skip: !partyId,
        refetchOnMountOrArgChange: true,
    });
    const { data: finAccounts, isFetching: isFetchingFinAccounts } = useFetchFinAccountsForPartyQuery(partyId, {
        skip: !partyId,
        refetchOnMountOrArgChange: true,
    });
    const { data: paymentMethods, isFetching: isFetchingPaymentMethods } = useFetchPaymentMethodsQuery(partyId, {
        skip: !partyId,
        refetchOnMountOrArgChange: true,
    });

    const {
        returnHeader,
        setReturnHeader,
        formEditMode,
        handleCreate,
        productStoreFacilities,
    } = useReturn({
        selectedMenuItem,
        formRef2,
        editMode,
        selectedReturn,
        setIsLoading,
    });
    

    useEffect(() => {
        if (!isFetchingBaseCurrency && baseCurrency?.currencyUomId) {
            setFormInitialValues(prev => ({
                ...prev,
                currencyUomId: prev.currencyUomId || baseCurrency.currencyUomId,
            }));
        }
    }, [isFetchingBaseCurrency, baseCurrency]);




    const renderSwitchReturnStatus = () => {
        // Why: Ensures correct status display for both customer (ORDER_RETURN_STTS) and vendor (PORDER_RETURN_STTS) returns
        const isVendorReturn = formInitialValues.returnHeaderTypeId === "VENDOR_RETURN";
        const statusId = returnHeader?.statusId;

        switch (statusId) {
            // Customer return statuses (ORDER_RETURN_STTS)
            case "RETURN_REQUESTED":
                return { label: "Requested", backgroundColor: "green", foreColor: "#ffffff" };
            case "RETURN_ACCEPTED":
                return { label: "Accepted", backgroundColor: "yellow", foreColor: "#000000" };
            case "RETURN_RECEIVED":
                return { label: "Received", backgroundColor: "orange", foreColor: "#ffffff" };
            case "RETURN_COMPLETED":
                return { label: "Completed", backgroundColor: "blue", foreColor: "#ffffff" };
            case "RETURN_MAN_REFUND":
                return { label: "Manual Refund Required", backgroundColor: "purple", foreColor: "#ffffff" };
            case "RETURN_CANCELLED":
                return { label: "Cancelled", backgroundColor: "red", foreColor: "#ffffff" };

            // Vendor return statuses (PORDER_RETURN_STTS)
            case "SUP_RETURN_REQUESTED":
                return { label: "Requested", backgroundColor: "green", foreColor: "#ffffff" };
            case "SUP_RETURN_ACCEPTED":
                return { label: "Accepted", backgroundColor: "yellow", foreColor: "#000000" };
            case "SUP_RETURN_SHIPPED":
                return { label: "Shipped", backgroundColor: "orange", foreColor: "#ffffff" };
            case "SUP_RETURN_COMPLETED":
                return { label: "Completed", backgroundColor: "blue", foreColor: "#ffffff" };
            case "SUP_RETURN_CANCELLED":
                return { label: "Cancelled", backgroundColor: "red", foreColor: "#ffffff" };

            // Default case
            default:
                return { label: formEditMode === 1 ? "New" : "Unknown", backgroundColor: "gray", foreColor: "#ffffff" };
        }
    };


    const returnHeaderTypeIdHandleChange = (e: any) => {
        const newType = e.value;
        setFormInitialValues(prev => ({
            ...prev,
            returnHeaderTypeId: newType,
            needsInventoryReceive: newType === "VENDOR_RETURN" ? "N" : "Y",
            fromPartyId: undefined,
            toPartyId: undefined,
            company: undefined,
        }));
        setReturnHeader((prev: any) => ({
            ...prev,
            returnHeaderTypeId: newType,
            needsInventoryReceive: newType === "VENDOR_RETURN" ? "N" : "Y",
            fromPartyId: undefined,
            toPartyId: undefined,
            company: undefined,
        }));
        setPartyId(undefined);
        refetchStatuses();
    };

    const handleMenuSelect = async (e: MenuSelectEvent) => {
        setSelectedMenuItem(e.item.text);
        if (e.item.text === "New Return") {
            handleNewReturn();
        }
    };

    const handleSubmit = (data: any) => {
        if (!data.isValid) return false;
        setIsLoading(true);
        const action = selectedMenuItem || (formEditMode === 1 ? "Create Return" : "Update Return");
        handleCreate(data.values, action);
    };

    const handleCancelForm = () => {
        dispatch(setUiReturnItems([]));
        dispatch(resetUiReturn(null));
        dispatch(setSelectedReturn(undefined));
        setPartyId(undefined);
        formRef2.current = !formRef2.current;
        setFormInitialValues({
            currencyUomId: baseCurrency?.currencyUomId || "",
            statusId: "RETURN_REQUESTED",
            entryDate: new Date(),
            needsInventoryReceive: "Y",
            returnHeaderTypeId: "CUSTOMER_RETURN", // Default for new form, but won't override existing selection
        });
        cancelEdit();
    };

    const status = renderSwitchReturnStatus();

    const isDataLoading = isFetchingCurrencies ||
        isFetchingBaseCurrency ||
        isFetchingCompanies ||
        isFetchingReturnStatusItems ||
        !currencies ||
        !baseCurrency ||
        !rawCompanies ||
        !returnStatusItems ||
        !productStoreFacilities;

    if (isDataLoading) {
        return <LoadingComponent message="Loading Return Form..." />;
    }
    
    return (
        <>
            <OrderMenu selectedMenuItem="/returns" />
            <Paper elevation={5} className="div-container-withBorderCurved">
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
                                    <Menu onSelect={handleMenuSelect}>
                                        <MenuItem text={getTranslatedLabel("general.actions", "Actions")}>
                                            <MenuItem text="New Return" />
                                        </MenuItem>
                                    </Menu>
                                </Grid>
                            </>
                        ) : (
                            <>
                                <Grid item xs={10} pb={0}>
                                    <ReturnsMenu selectedMenuItem={selectedMenuItem} returnId={returnHeader?.returnId} />
                                </Grid>
                                <Grid item xs={1}>
                                    <Menu onSelect={handleMenuSelect}>
                                        <MenuItem text={getTranslatedLabel("general.actions", "Actions")}>
                                            {returnHeader?.statusId === "RETURN_REQUESTED" && (
                                                <>
                                                    <MenuItem text="Approve Return" />
                                                    <MenuItem text="Update Return" />
                                                </>
                                            )}
                                            {returnHeader?.statusId === "RETURN_ACCEPTED" && (
                                                <MenuItem text="Complete Return" />
                                            )}
                                            <MenuItem text="New Return" />
                                        </MenuItem>
                                    </Menu>
                                </Grid>
                                {formEditMode > 1 && (
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
                    initialValues={formInitialValues}
                    key={JSON.stringify(formInitialValues)}
                    onSubmitClick={handleSubmit}
                    render={() => (
                        <FormElement>
                            <fieldset className="k-form-fieldset">
                                <Grid container spacing={2} alignItems="center" sx={{ paddingLeft: 3 }}>
                                    <Grid item xs={12} className={formEditMode > 1 ? "grid-disabled" : "grid-normal"}>
                                        <Field
                                            id="returnHeaderTypeId"
                                            name="returnHeaderTypeId"
                                            label="Return Type"
                                            layout="horizontal"
                                            component={FormRadioGroup}
                                            validator={radioGroupValidatorReturnHeader}
                                            onChange={returnHeaderTypeIdHandleChange}
                                            value={formInitialValues.returnHeaderTypeId} // REFACTOR: Use formInitialValues for value
                                            disabled={formEditMode > 1}
                                            data={returnTypesData}
                                        />
                                    </Grid>

                                    <Grid item container xs={12} spacing={2}>
                                        {formInitialValues.returnHeaderTypeId === "CUSTOMER_RETURN" && (
                                            <Grid item xs={3}>
                                                <Field
                                                    id={"fromPartyId"}
                                                    name={"fromPartyId"}
                                                    label={"Customer *"}
                                                    component={FormComboBoxVirtualCustomer}
                                                    autoComplete={"off"}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                        )}

                                        {formInitialValues.returnHeaderTypeId === "VENDOR_RETURN" && (
                                            <Grid item xs={3} className={formEditMode > 1 ? "grid-disabled" : "grid-normal"}>
                                                <Field
                                                    id="toPartyId"
                                                    name="toPartyId"
                                                    label="Supplier *"
                                                    component={FormComboBoxVirtualSupplier}
                                                    autoComplete="off"
                                                    validator={requiredValidator}
                                                    valueField="partyId"
                                                    textField="partyName"
                                                />
                                            </Grid>
                                        )}

                                        <Grid item xs={3}>
                                            <Field
                                                id={"companyId"}
                                                name={"companyId"}
                                                label="Company *"
                                                component={MemoizedFormDropDownList}
                                                dataItemKey="partyId"
                                                textField="partyName"
                                                data={companies ?? []}
                                                validator={requiredValidator}
                                            />
                                        </Grid>

                                        <Grid item xs={3}>
                                            <Field
                                                id="destinationFacilityId"
                                                name="destinationFacilityId"
                                                label="Destination Facility *"
                                                component={MemoizedFormDropDownList}
                                                dataItemKey="destinationFacilityId"
                                                textField="facilityName"
                                                data={productStoreFacilities ?? []}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                    </Grid>

                                    <Grid item xs={3}>
                                        <Field
                                            id="entryDate"
                                            name="entryDate"
                                            label="Entry Date"
                                            disabled={formEditMode > 1}
                                            component={FormDatePicker}
                                            validator={requiredValidator}
                                        />
                                    </Grid>

                                    <Grid item xs={3}>
                                        <Field
                                            id="statusId"
                                            name="statusId"
                                            label="Status *"
                                            component={MemoizedFormDropDownList2}
                                            dataItemKey={"statusId"}
                                            textField={"description"}
                                            data={returnStatusItems || []}
                                            validator={requiredValidator}
                                        />
                                    </Grid>

                                    <Grid item xs={3}>
                                        <Field
                                            id="currencyUomId"
                                            name="currencyUomId"
                                            label="Currency *"
                                            component={MemoizedFormDropDownList2}
                                            dataItemKey={"currencyUomId"}
                                            textField={"description"}
                                            data={currencies ?? []}
                                            validator={requiredValidator}
                                        />
                                    </Grid>

                                    {billingAccounts?.length > 0 && (
                                        <Grid item xs={3}>
                                            <Field
                                                id="billingAccountId"
                                                name="billingAccountId"
                                                label="Billing Account"
                                                component={MemoizedFormDropDownList2}
                                                dataItemKey="billingAccountId"
                                                textField="description"
                                                data={billingAccounts}
                                                disabled={formEditMode > 2}
                                            />
                                        </Grid>
                                    )}

                                    {finAccounts?.length > 0 && (
                                        <Grid item xs={3}>
                                            <Field
                                                id="finAccountId"
                                                name="finAccountId"
                                                label="Financial Account"
                                                component={MemoizedFormDropDownList2}
                                                dataItemKey="finAccountId"
                                                textField="finAccountName"
                                                data={finAccounts}
                                                disabled={formEditMode > 2}
                                            />
                                        </Grid>
                                    )}

                                    {paymentMethods && (paymentMethods.creditCards?.length > 0 || paymentMethods.eftAccounts?.length > 0) && (
                                        <Grid item xs={3}>
                                            <Field
                                                id="paymentMethodId"
                                                name="paymentMethodId"
                                                label="Payment Method"
                                                component={MemoizedFormDropDownList2}
                                                dataItemKey="paymentMethodId"
                                                textField="description"
                                                data={[...(paymentMethods.creditCards || []), ...(paymentMethods.eftAccounts || [])]}
                                                disabled={formEditMode > 2}
                                            />
                                        </Grid>
                                    )}
                                    
                                </Grid>

                                <Grid container spacing={1}>
                                    <Grid item>
                                        <Button sx={{ m: 2 }} onClick={handleCancelForm} color="error" variant="contained">
                                            Cancel
                                        </Button>
                                    </Grid>
                                    {/* Refactored: Added Submit button to trigger form submission */}
                                    <Grid item>
                                        <Button
                                            sx={{ m: 2 }}
                                            onClick={() => formRef.current?.onSubmit()}
                                            color="primary"
                                            variant="contained"
                                            disabled={isLoading}
                                        >
                                            Submit
                                        </Button>
                                    </Grid>
                                </Grid>

                                {(isLoading || isFetchingBillingAccounts || isFetchingFinAccounts || isFetchingPaymentMethods) && (
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