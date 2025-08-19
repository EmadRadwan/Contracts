import React, {useCallback, useEffect, useMemo, useRef, useState} from "react";
import {
    useFetchAgreementsByPartyIdQuery, useFetchBillingAccountsByPartyIdQuery, useGetBackOrderedQuantityQuery
} from "../../../../../app/store/apis";
import Grid from "@mui/material/Grid";
import {Box, Paper, Typography} from "@mui/material";
import {
    Field,
    Form,
    FormElement,
    FormRenderProps,
} from "@progress/kendo-react-form";
import Button from "@mui/material/Button";
import {FormComboBoxVirtualCustomer} from "../../../../../app/common/form/FormComboBoxVirtualCustomer";
import SalesOrderItemsList from "../../../dashboard/order/SalesOrderItemsList";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import OrderTotals from "../OrderTotals";
import {
    useAppDispatch,
    useAppSelector,
    useFetchCompanyBaseCurrencyQuery,
    useFetchCurrenciesQuery,
    useFetchOrderPaymentMethodsQuery,
} from "../../../../../app/store/configureStore";
import useSalesOrder from "../../../hook/useSalesOrder";
import CreateCustomerModalForm from "../../../../parties/form/CreateCustomerModalForm";
import {
    resetUiOrderItems,
    setUiOrderItems,
} from "../../../slice/orderItemsUiSlice";
import {setAddTax, setCustomerId, setNeedsTaxRecalculation} from "../../../slice/sharedOrderUiSlice";
import ModalContainer from "../../../../../app/common/modals/ModalContainer";
import {
    resetUiOrderAdjustments,
    setUiOrderAdjustments,
} from "../../../slice/orderAdjustmentsUiSlice";
import OrderMenu from "../../../menu/OrderMenu";
import FormTextArea from "../../../../../app/common/form/FormTextArea";
import {MemoizedFormDropDownList2} from "../../../../../app/common/form/MemoizedFormDropDownList2";
import {FormRadioGroup} from "../../../../../app/common/form/FormRadioGroup";
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";
import {RibbonContainer, Ribbon} from "react-ribbons";
import {setOrderFormEditMode} from "../../../slice/ordersUiSlice";
import "../../../../../app/layout/styles.css";
import {setSelectedOrder} from "../../../../accounting/slice/accountingSharedUiSlice";
import {Link, NavLink} from "react-router-dom";
import {resetUiOrderTerms} from "../../../slice/orderTermsUiSlice";

import {Popover} from '@progress/kendo-react-tooltip';
import {MemoizedFormCheckBox} from "../../../../../app/common/form/FormCheckBox";
import {useSelector} from "react-redux";
import {nonDeletedOrderItemsSelector} from "../../../slice/orderSelectors";
import {toast} from "react-toastify";
import { useRecalculateTaxesMutation} from "../../../../../app/store/apis/accounting/taxApi";
import {debounce} from "lodash";


interface Props {
    selectedOrder?: any;
    editMode: number;
    cancelEdit: () => void;
}

export default function SalesOrderForm({
                                           selectedOrder,
                                           cancelEdit,
                                           editMode,
                                       }: Props) {
    const [showNewCustomer, setShowNewCustomer] = useState(false);
    const formRef = React.useRef<any>();
    const formRef2 = React.useRef<boolean>(false);
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = 'order.so.form'

    const [isLoading, setIsLoading] = useState(false);
    const customerId = useAppSelector(
        (state) => state.sharedOrderUi.selectedCustomerId
    );
    const {data: billingAccount} =
        useFetchBillingAccountsByPartyIdQuery(customerId);



    

    const {data: agreements} = useFetchAgreementsByPartyIdQuery(
        {partyId: customerId!, orderType: "SALES_ORDER"},
        {
            skip: !customerId,
        }
    );
    const {data: paymentMethodTypes} =
        useFetchOrderPaymentMethodsQuery(undefined);

    const language = useAppSelector((state) => state.localization.language);

    const {order, setOrder, formEditMode, setFormEditMode, handleCreate} =
        useSalesOrder({
            editMode,
            formRef2,
            selectedOrder,
            setIsLoading,
        });

    const { data: backOrderedData, isLoading: isBackOrderedLoading } = useGetBackOrderedQuantityQuery(
        order?.orderId || '',
        { skip: !order?.orderId }
    );

    const showQuickShip = useMemo(() => {
        if (isBackOrderedLoading || !backOrderedData) {
            return false; // Hide while loading or no data
        }
        return backOrderedData.success && backOrderedData.data === 0;
    }, [backOrderedData, isBackOrderedLoading]);

    console.log("showQuickShip:", showQuickShip); // Debugging line

    const selectedMenuItemRef = useRef<string>("");
    const {data: baseCurrency, isLoading: isBaseCurrencyLoading} = useFetchCompanyBaseCurrencyQuery(undefined);
    const {data: currencies, isLoading: isCurrenciesLoading} = useFetchCurrenciesQuery(undefined);


    const addTax = useAppSelector((state) => state.sharedOrderUi.addTax);
    const needsTaxRecalculation = useAppSelector((state) => state.sharedOrderUi.needsTaxRecalculation);

    const orderItems = useSelector(nonDeletedOrderItemsSelector);
    const allAdjustments = useAppSelector(
        (state) => state.orderAdjustmentsUi.orderAdjustments.entities
    );
    const allAvailableAdjustments = Object.values(allAdjustments);

    const [recalculateTaxes, { isLoading: isTaxLoading }] = useRecalculateTaxesMutation();


    const invoiceId = selectedOrder?.invoiceId || order?.invoiceId;
    const paymentId = selectedOrder?.paymentId || order?.paymentId;


    const [showPopover, setShowPopover] = useState(false);
    const popoverAnchor = useRef<HTMLButtonElement | null>(null);

    const memoizedOrderTotals = useMemo(() => <OrderTotals/>, []);
    const dispatch = useAppDispatch();
    const prevAddTaxRef = useRef(addTax); // Track previous addTax value

    // Special Remark: Add standardized error handling utility
    const handleError = useCallback((error: any, defaultMessage: string) => {
        const message = error?.message || defaultMessage;
        console.error("Error:", error);
        toast.error(message);
    }, []);


    const debouncedRecalculateTaxes = useMemo(
        () =>
            debounce((orderItems, dispatch) => {
                console.log('Debounced recalculateTaxes called:', orderItems);
                recalculateTaxes({ orderItems, forceCalculate: true })
                    .unwrap()
                    .then(() => {
                        dispatch(setNeedsTaxRecalculation(false));
                    })
                    .catch((error) => handleError(error, "Failed to recalculate tax"));
            }, 300),
        [recalculateTaxes, handleError, dispatch]
    );

    useEffect(() => {
        if (needsTaxRecalculation && addTax && orderItems.length > 0) {
            debouncedRecalculateTaxes(orderItems, dispatch);
        }
    }, [needsTaxRecalculation, addTax, orderItems, dispatch, debouncedRecalculateTaxes]);

    useEffect(() => {
        return () => {
            debouncedRecalculateTaxes.cancel();
        };
    }, [debouncedRecalculateTaxes]);

    useEffect(() => {
        if (prevAddTaxRef.current && !addTax) { // Only run when addTax changes from true to false
            const nonTaxAdjustments = allAvailableAdjustments.filter(
                adj => adj.orderAdjustmentTypeId !== "VAT_TAX"
            );
            dispatch(setUiOrderAdjustments(nonTaxAdjustments));
        }
        prevAddTaxRef.current = addTax; // Update the ref with the current value
    }, [addTax, allAvailableAdjustments, dispatch]); 


    const memoizedSalesOrderItemsList = useMemo(
        () => (
            <SalesOrderItemsList orderFormEditMode={formEditMode} orderId={order ? order.orderId : undefined}/>
        ),
        [formEditMode, order]
    );


    useEffect(() => {
        if (selectedOrder) {
            setOrder(selectedOrder);
        } else {
            setOrder({
                orderId: undefined,
                currencyUomId: baseCurrency?.currencyUomId,
            });
            formRef2.current = !formRef2.current;
        }
        return () => {
            dispatch(setSelectedOrder(undefined))
        }
    }, [baseCurrency?.currencyUomId, selectedOrder, setOrder, dispatch]);

    useEffect(() => {
        dispatch(setOrderFormEditMode(formEditMode));
        if (formEditMode < 2) {
            setOrder({
                orderId: undefined,
                currencyUomId: baseCurrency?.currencyUomId,
            });
            formRef2.current = !formRef2.current;
            dispatch(setUiOrderItems([]));
            dispatch(setUiOrderAdjustments([]));
        }
    }, [editMode, formEditMode, setOrder, dispatch, baseCurrency?.currencyUomId]);

    const renderSwitchStatus = useCallback(() => {
        switch (formEditMode) {
            case 1:
                return {label: "New", backgroundColor: "green", foreColor: "#ffffff"};
            case 2:
                return {
                    // label: "Created",
                    backgroundColor: "green",
                    foreColor: "#ffffff",
                };
            case 3:
                return {
                    // label: "Approved",
                    backgroundColor: "yellow",
                    foreColor: "#000000",
                }; // Black text on yellow
            case 4:
                return {
                    // label: "Completed",
                    backgroundColor: "blue",
                    foreColor: "#ffffff",
                };
            default:
                return {
                    // label: "Unknown",
                    backgroundColor: "gray",
                    foreColor: "#ffffff",
                };
        }
    }, [formEditMode]);

    useEffect(() => {
        renderSwitchStatus();
    }, [formEditMode, renderSwitchStatus]);

    async function handleMenuSelect(e: MenuSelectEvent) {
        if (e.item.data === "new") {
            handleNewOrder();
        } else if (e.item.data === "ship") {
            if (!formRef.current) {
                toast.error("Form reference is not available");
                return;
            }
            selectedMenuItemRef.current = "Quick Ship Order";
            console.log("Submitting with menuItem: Quick Ship Order"); // Debug
            formRef.current.onSubmit();
        }
    }

    const handleSubmit = (formProps: any) => {
        console.log("handleSubmit formProps:", formProps); // Debug
        if (!formProps.isValid) {
            toast.error("Form is invalid");
            return false;
        }

        const values = formProps.values;
        if (
            values?.paymentMethodTypeId === "EXT_BILLACT" &&
            billingAccount?.length
        ) {
            values.billingAccountId = billingAccount[0].billingAccountId;
        }

        const selectedMenuItem = selectedMenuItemRef.current;
        if (!selectedMenuItem) {
            toast.error("No action selected");
            return false;
        }

        setIsLoading(true);
        handleCreate({ values, selectedMenuItem });
    };
    
    const handleNewOrder = () => {
        setOrder({
            orderId: undefined,
            currencyUomId: baseCurrency?.currencyUomId,
        });
        setFormEditMode(1);
        dispatch(setUiOrderItems([]));
        dispatch(resetUiOrderItems());
        dispatch(resetUiOrderAdjustments());
        dispatch(resetUiOrderTerms())
        dispatch(setUiOrderAdjustments([]));
        dispatch(setCustomerId(undefined));
        formRef2.current = !formRef2.current;
    };

    const handleCancelForm = () => {
        setOrder({
            orderId: undefined,
            currencyUomId: baseCurrency?.currencyUomId,
        });
        dispatch(setUiOrderItems([]));
        dispatch(resetUiOrderItems());
        dispatch(resetUiOrderTerms())
        dispatch(resetUiOrderAdjustments());
        dispatch(setUiOrderAdjustments([]));
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

    const memoizedOnClose2 = useCallback(() => {
        setShowNewCustomer(false);
    }, []);


    const onCustomerChange = useCallback(
        (event: any) => {
            if (event.value === null) {
                dispatch(resetUiOrderItems());
                dispatch(resetUiOrderAdjustments());
            }
        },
        [dispatch]
    );

    const status = renderSwitchStatus();

    const finalPaymentMethodTypes = useMemo(() => {
        if (!paymentMethodTypes) return [];
        // If there's a billing account, we add a new item to represent 'Billing Account'
        if (billingAccount && billingAccount.length > 0) {
            return [
                ...paymentMethodTypes,
                {
                    value: "EXT_BILLACT",
                    label: getTranslatedLabel(
                        `${localizationKey}.billingAccount`,
                        "Billing Account"
                    ),
                },
            ];
        }
        return paymentMethodTypes;
    }, [paymentMethodTypes, billingAccount]);

    const paymentMethodLabel = useMemo(() => {
        if (!order?.paymentMethodTypeId) return "";
        if (order.paymentMethodTypeId === "EXT_BILLACT") {
          return getTranslatedLabel(
            `${localizationKey}.billingAccount`,
            "Billing Account"
          );
        }
        const found = finalPaymentMethodTypes.find(
            (pm) => pm.value === order.paymentMethodTypeId
        );
        return found?.label || order.paymentMethodTypeId;
    }, [order?.paymentMethodTypeId, finalPaymentMethodTypes, getTranslatedLabel, localizationKey]);

    // Payment method popover button is only relevant if the method is 'Billing Account'
    const isBillingAccountUsed = order?.paymentMethodTypeId === "EXT_BILLACT";
    const isOrderApprovedOrBillingAccountPresent = isBillingAccountUsed || (billingAccount && billingAccount.length > 0);



    if (isBaseCurrencyLoading || isCurrenciesLoading) {
        return <LoadingComponent message="Loading initial data..."/>;
    }

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
            <OrderMenu selectedMenuItem={"/orders"}/>
            <RibbonContainer>
                <Paper
                    dir={language === "ar" ? "rtl" : "ltr"}
                    elevation={5}
                    className={`div-container-withBorderCurved ${
                        language === "ar" ? "k-rtl" : ""
                    }`}
                >
                    <Grid
                        container
                        // spacing={2}
                        alignItems={"center"}
                        sx={{position: "relative"}}
                    >
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
                                    {order && order?.orderId
                                        ? `${getTranslatedLabel(`${localizationKey}.orderNo`, 'Sales Order No: ')} ${order?.orderId}`
                                        : `${getTranslatedLabel(`${localizationKey}.new`, "New Sales Order")}`}
                                </Typography>
                            </Box>
                        </Grid>

                        <Grid item xs={1}>
                            <Menu onSelect={handleMenuSelect}>
                                <MenuItem
                                    text={getTranslatedLabel("general.actions", "Actions")}
                                >
                                    <MenuItem text={getTranslatedLabel(`${localizationKey}.actions.new`, "New Order")}
                                              data='new'/>
                                    {formEditMode === 3 && showQuickShip && (
                                        <MenuItem
                                        text={getTranslatedLabel(`${localizationKey}.actions.ship`, "Quick Ship Order")}
                                        data='ship'/>
                                    )}
                                </MenuItem>
                            </Menu>
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
                                    <Typography variant="h4" sx={{fontSize: language === "ar" ? "1.1rem" : "0.9rem"}}>
                                        {order.statusDescription}
                                    </Typography>
                                </Ribbon>
                            )}
                        </Grid>
                    </Grid>

                    <Form
                        ref={formRef}
                        initialValues={
                            formEditMode > 1
                                ? order
                                : {currencyUomId: baseCurrency?.currencyUomId}
                        }
                        key={formRef2.current.toString()}
                        onSubmitClick={(values) => handleSubmit(values)}
                        render={(formRenderProps: FormRenderProps) => (
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
                                            spacing={1}
                                            alignItems="center"
                                            justifyContent={"flex-start"}
                                            xs={12}
                                            item
                                            sx={{paddingLeft: 3}}
                                        >
                                            <Grid item container xs={9} spacing={2}>
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
                                                        sx={{paddingTop: "0 !important"}}
                                                        className={
                                                            formEditMode > 2 ? "grid-disabled" : "grid-normal"
                                                        }
                                                    >
                                                        <Field
                                                            id={"fromPartyId"}
                                                            name={"fromPartyId"}
                                                            label={
                                                                getTranslatedLabel(`${localizationKey}.customer`, "Customer")
                                                            }
                                                            component={FormComboBoxVirtualCustomer}
                                                            autoComplete={"off"}
                                                            disabled={formEditMode > 1}
                                                            onChange={onCustomerChange}
                                                            validator={requiredValidator}
                                                        />
                                                    </Grid>
                                                    <Grid item xs={2}>
                                                        <Button
                                                            color={"secondary"}
                                                            onClick={() => {
                                                                setShowNewCustomer(true);
                                                            }}
                                                            variant="outlined"
                                                            disabled={formEditMode > 1}
                                                        >
                                                            {getTranslatedLabel(`${localizationKey}.new-customer`, "New Customer")}
                                                        </Button>
                                                    </Grid>
                                                    <Grid item xs={2}>
                                                        <Field
                                                            id="currencyUomId"
                                                            name="currencyUomId"
                                                            component={MemoizedFormDropDownList2}
                                                            data={currencies ?? []}
                                                            label={getTranslatedLabel(`${localizationKey}.currency`, "Currency")}
                                                            dataItemKey={"currencyUomId"}
                                                            disabled={formEditMode > 1}
                                                            textField={"description"}
                                                        />
                                                    </Grid>
                                                    {formEditMode === 1 && (
                                                        <Grid item xs={3}>
                                                            <Field
                                                                id={"addTax"}
                                                                name={"addTax"}
                                                                label={getTranslatedLabel(`${localizationKey}.addTax`, "Add Tax")}
                                                                component={MemoizedFormCheckBox}
                                                                onChange={(e: any) => {
                                                                    dispatch(setAddTax(e.value));
                                                                }}
                                                                disabled={isTaxLoading}
                                                            />
                                                            {isTaxLoading && (
                                                                <Typography variant="caption" color="textSecondary">
                                                                    Calculating Tax...
                                                                </Typography>
                                                            )}
                                                        </Grid>
                                                    )}

                                                    {/*{agreements &&
                                                        agreements?.length > 0 &&
                                                        customerId !== undefined && (
                                                            <Grid item xs={3}>
                                                                <Field
                                                                    id="agreementId"
                                                                    name="agreementId"
                                                                    label={getTranslatedLabel(`${localizationKey}.agreement`, "Agreement")}
                                                                    component={MemoizedFormDropDownList2}
                                                                    data={agreements ?? []}
                                                                    disabled={formEditMode > 1}
                                                                    dataItemKey={"agreementId"}
                                                                    textField={"description"}
                                                                />
                                                            </Grid>
                                                        )} */}
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
                                                            label={getTranslatedLabel(`${localizationKey}.customer-remarks`, "Customer Remarks")}
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
                                                            label={getTranslatedLabel(`${localizationKey}.internal-remarks`, "Internal Remarks")}
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
                                                xs={3}
                                                spacing={2}
                                                alignItems="flex-end"
                                            >
                                                {memoizedOrderTotals}
                                            </Grid>
                                        </Grid>

                                        <Grid
                                            container
                                            item
                                            alignItems={"center"}
                                            sx={{ml: 2, mt: 3}}
                                        >
                                            <Grid item xs={10}>
                                                {memoizedSalesOrderItemsList}
                                            </Grid>


                                            <Grid
                                                item
                                                container
                                                xs={2}
                                                justifyContent={"flex-start"}
                                                direction={"column"}
                                            >
                                               
                                                {formEditMode < 3 ? (
                                                    // Render the Payment Method Type radio group (editable)
                                                    <Grid item xs={3}>
                                                        <Field
                                                            id={"paymentMethodTypeId"}
                                                            name={"paymentMethodTypeId"}
                                                            label={getTranslatedLabel(`${localizationKey}.pmt`, "Payment Method Type")}
                                                            component={FormRadioGroup}
                                                            disabled={formEditMode > 2}
                                                            layout={"vertical"}
                                                            data={finalPaymentMethodTypes || []}
                                                            onChange={() => {
                                                                // Clear other payment-related fields
                                                                formRenderProps.onChange("billingAccountId", {
                                                                    value: null,
                                                                });
                                                            }}
                                                        />
                                                    </Grid>
                                                ) : (
                                                    // Render a simple text display of the Payment Method
                                                    <Grid item xs={12} sx={{mb: 1}}>
                                                        <Typography variant="subtitle1" sx={{fontWeight: "bold"}}>
                                                            {getTranslatedLabel(
                                                                `${localizationKey}.pmt`,
                                                                "Payment Method"
                                                            )}
                                                        </Typography>
                                                        <Typography variant="body1">
                                                            {paymentMethodLabel || "N/A"}
                                                        </Typography>
                                                    </Grid>
                                                )}

                                                {invoiceId && formEditMode != 1 && (
                                                    <Grid item xs={12} md={6}>
                                                        <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>
                                                            {getTranslatedLabel("general.invoiceNo", "Invoice No:")}
                                                        </Typography>
                                                        <Typography variant="body1">
                                                            <Link to={`/invoices/${invoiceId}`} state={{}}>
                                                                {invoiceId}
                                                            </Link>
                                                        </Typography>
                                                    </Grid>
                                                )}

                                                {/* Payment No. */}
                                                {paymentId && formEditMode != 1 && (
                                                    <Grid item xs={12} md={6}>
                                                        <Typography variant="subtitle1" sx={{fontWeight: "bold"}}>
                                                            {getTranslatedLabel("general.paymentNo", "Payment No:")}
                                                        </Typography>
                                                        <Typography variant="body1">
                                                            <NavLink to="/payments"
                                                                     state={{selectedPaymentId: paymentId}}>
                                                                {paymentId}
                                                            </NavLink>
                                                        </Typography>
                                                    </Grid>
                                                )}


                                                {isOrderApprovedOrBillingAccountPresent && (
                                                    <Grid item xs={12}>
                                                        <Button
                                                            ref={popoverAnchor}
                                                            variant="contained"
                                                            color="primary"
                                                            onClick={() => setShowPopover(!showPopover)}
                                                            sx={{mt: 2}}
                                                        >
                                                            Show Billing Account Balance
                                                        </Button>
                                                        <Popover show={showPopover} anchor={popoverAnchor.current}
                                                                 position="bottom">
                                                            <Box p={2} width={250}>
                                                                {billingAccount?.map((ba: any) => (
                                                                    <Box key={ba.billingAccountId} sx={{mb: 1}}>
                                                                        <Typography variant="subtitle1"
                                                                                    sx={{fontWeight: "bold"}}>
                                                                            Account Limit:
                                                                        </Typography>
                                                                        <Typography variant="body1">
                                                                            {ba.accountLimit.toLocaleString()} {ba.accountCurrencyUomId}
                                                                        </Typography>

                                                                        <Typography variant="subtitle1"
                                                                                    sx={{fontWeight: "bold", mt: 1}}>
                                                                            Account Balance:
                                                                        </Typography>
                                                                        <Typography variant="body1">
                                                                            {ba.accountBalance.toLocaleString()} {ba.accountCurrencyUomId}
                                                                        </Typography>
                                                                    </Box>
                                                                ))}
                                                            </Box>
                                                        </Popover>
                                                    </Grid>
                                                )}
                                            </Grid>
                                        </Grid>

                                        <Grid container spacing={1}>
                                            <Grid item xs={1}>
                                                <Button
                                                    sx={{m: 1}}
                                                    onClick={handleCancelForm}
                                                    color="error"
                                                    variant="contained"
                                                >
                                                    {getTranslatedLabel("general.cancel", "Cancel")}
                                                </Button>
                                            </Grid>
                                            {formEditMode <= 3 && (
                                                <Grid item xs={2}>
                                                    <Button
                                                        sx={{ m: 1, fontSize: "1rem", padding: "8px 16px" }}
                                                        onClick={() => {
                                                            const menuItem = formEditMode === 1 ? "Create Order" : "Update Order";
                                                            selectedMenuItemRef.current = menuItem;
                                                            setTimeout(() => {
                                                                formRef.current.onSubmit();
                                                            });
                                                        }}
                                                        color="primary"
                                                        variant="contained"
                                                        disabled={!formRenderProps.allowSubmit}
                                                        aria-label={formEditMode === 1 ? "Create Order" : "Update Order"}
                                                    >
                                                        {isLoading
                                                            ? getTranslatedLabel(`${localizationKey}.processing`, "Processing...")
                                                            : getTranslatedLabel(
                                                                formEditMode === 1
                                                                    ? `${localizationKey}.actions.create`
                                                                    : `${localizationKey}.actions.update`,
                                                                formEditMode === 1 ? "Create Order" : "Update Order"
                                                            )}
                                                    </Button>
                                                </Grid>
                                            )}
                                        </Grid>

                                        {isLoading && (
                                            <LoadingComponent
                                                message={getTranslatedLabel(`${localizationKey}.processing`, "Processing Order...")}/>
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
