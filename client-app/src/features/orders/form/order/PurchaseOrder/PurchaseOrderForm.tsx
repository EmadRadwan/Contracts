import React, {useCallback, useEffect, useMemo, useRef, useState} from "react";
import usePurchaseOrder from "../../../hook/usePurchaseOrder";
import {
    useAppDispatch,
    useAppSelector,
    useFetchCompanyBaseCurrencyQuery,
    useFetchCurrenciesQuery
} from "../../../../../app/store/configureStore";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";
import Grid from "@mui/material/Grid";
import {Box, Paper, Typography} from "@mui/material";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import {FormComboBoxVirtualSupplier} from "../../../../../app/common/form/FormComboBoxVirtualSupplier";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import {PurchaseOrderItemsListMemo} from "../../../dashboard/order/PurchaseOrderItemsList";
import OrderTotals from "../OrderTotals";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import {resetUiOrderItems, setUiOrderItems} from "../../../slice/orderItemsUiSlice";
import {resetUiOrderAdjustments, setUiOrderAdjustments} from "../../../slice/orderAdjustmentsUiSlice";
import FormTextArea from "../../../../../app/common/form/FormTextArea";
import OrderMenu from "../../../menu/OrderMenu";
import {useFetchAgreementsByPartyIdQuery} from "../../../../../app/store/apis";
import {MemoizedFormDropDownList2} from "../../../../../app/common/form/MemoizedFormDropDownList2";
import {
    setAddTax,
    setNeedsTaxRecalculation,
    setSelectedApprovedPurchaseOrder,
    setSupplierId
} from "../../../slice/sharedOrderUiSlice";
import {RibbonContainer, Ribbon} from "react-ribbons"
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";
import {setOrderFormEditMode} from "../../../slice/ordersUiSlice";
import {useNavigate} from "react-router";
import {MemoizedFormCheckBox} from "../../../../../app/common/form/FormCheckBox";
import {nonDeletedOrderItemsSelector} from "../../../slice/orderSelectors";
import {useRecalculateTaxesMutation} from "../../../../../app/store/apis/accounting/taxApi";
import {toast} from "react-toastify";
import {debounce} from "lodash";
import LoadingButton from "@mui/lab/LoadingButton"; 

interface Props {
    selectedOrder?: any
    editMode: number;
    cancelEdit: () => void;
}

export default function PurchaseOrderForm({selectedOrder, cancelEdit, editMode}: Props) {
    const formRef = React.useRef<any>(null);
    const formRef2 = React.useRef<boolean>(false);
    const [selectedMenuItem, setSelectedMenuItem] = React.useState('');
    const [isLoading, setIsLoading] = useState(false);
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = 'order.po.form'
    const selectedMenuItemRef = useRef<string>("");
    const [isSubmitting, setIsSubmitting] = useState(false);
    const {language} = useAppSelector(state => state.localization)
    const supplierId = useAppSelector(
        (state) => state.sharedOrderUi.selectedSupplierId
    );
    const {data: agreements} = useFetchAgreementsByPartyIdQuery({partyId: supplierId!, orderType: "PURCHASE_ORDER"}, {
        skip: !supplierId
    })
    const {data: currencies} = useFetchCurrenciesQuery(undefined);
    const {data: baseCurrency} = useFetchCompanyBaseCurrencyQuery(undefined);

    const {order, setOrder, formEditMode, setFormEditMode, handleCreate, isUpdatePurchaseOrderLoading, isAddPurchaseOrderLoading} = usePurchaseOrder({
        selectedMenuItem,
        editMode,
        formRef2,
        selectedOrder, setIsLoading
    });

    const initialFormValues = useMemo(() => ({
        fromPartyId: order?.fromPartyId || null,
        currencyUomId: order?.currencyUomId || baseCurrency?.currencyUomId || '',
        internalRemarks: order?.internalRemarks || '',
        agreementId: order?.agreementId || null,
        addTax: order?.addTax || false,
    }), [order, baseCurrency]);

    const handleError = useCallback((error: any, defaultMessage: string) => {
        const message = error?.message || defaultMessage;
        console.error("Error:", error);
        toast.error(message);
    }, []);

    const dispatch = useAppDispatch();
    const navigate = useNavigate()
    const memoizedOrderTotals = useMemo(() => <OrderTotals/>, [order]);

    const addTax = useAppSelector((state) => state.sharedOrderUi.addTax);
    const needsTaxRecalculation = useAppSelector((state) => state.sharedOrderUi.needsTaxRecalculation);

    const orderItems = useAppSelector(nonDeletedOrderItemsSelector);
    const allAdjustments = useAppSelector(
        (state) => state.orderAdjustmentsUi.orderAdjustments.entities
    );
    const allAvailableAdjustments = Object.values(allAdjustments);
    const [recalculateTaxes, {isLoading: isTaxLoading}] =
        useRecalculateTaxesMutation();

    const prevAddTaxRef = useRef(addTax); // Track previous addTax value


    const debouncedRecalculateTaxes = useMemo(
        () =>
            debounce((orderItems, dispatch) => {
                console.log('Debounced recalculateTaxes called:', orderItems);
                recalculateTaxes({orderItems, forceCalculate: true})
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
        if (prevAddTaxRef.current && !addTax) {
            const nonTaxAdjustments = allAvailableAdjustments.filter(
                (adj: any) => adj.orderAdjustmentTypeId !== "VAT_TAX"
            );
            dispatch(setUiOrderAdjustments(nonTaxAdjustments));
        }
        prevAddTaxRef.current = addTax;
    }, [addTax, allAvailableAdjustments, dispatch]);

    useEffect(() => {
        if (selectedOrder) {
            setOrder(selectedOrder)
        } else {
            setOrder({orderId: undefined, currencyUomId: baseCurrency?.currencyUomId});
            formRef2.current = !formRef2.current
        }

    }, [baseCurrency?.currencyUomId, selectedOrder, setOrder]);

    const handleInternalRemarksChange = useCallback(
        (event: any) => {
            if (formRef.current) {
                const newValue = event.value || '';
                console.log('internalRemarks changed:', {
                    newValue,
                    previous: formRef.current.valueGetter('internalRemarks'),
                    modified: formRef.current.modified
                });
                formRef.current.onChange('internalRemarks', { value: newValue, touched: true });
            }
        },
        []
    );

    useEffect(() => {
        if (formRef.current) {
            console.log('Form state updated:', {
                isValid: formRef.current.isValid,
                modified: formRef.current.modified,
                allowSubmit: formRef.current.allowSubmit,
                values: formRef.current.valueGetter(),
                touched: formRef.current.touched
            });
        }
    }, [formRef.current?.modified, formRef.current?.isValid, formRef.current?.values?.internalRemarks]);

    const formKey = useMemo(() => formRef2.current.toString(), [formRef2.current]);

    const renderSwitchStatus = () => {
        switch (formEditMode) {
            case 1:
                return {label: "New", backgroundColor: "green", foreColor: "#ffffff"};
            case 2:
                return {
                    //   label: "Created",
                    backgroundColor: "green",
                    foreColor: "#ffffff",
                };
            case 3:
                return {
                    //   label: "Approved",
                    backgroundColor: "yellow",
                    foreColor: "#000000",
                }; // Black text on yellow
            case 4:
                return {
                    //   label: "Completed",
                    backgroundColor: "blue",
                    foreColor: "#ffffff",
                };
            default:
                return {
                    //   label: "Unknown",
                    backgroundColor: "gray",
                    foreColor: "#ffffff",
                };
        }
    };

    useEffect(() => {
        renderSwitchStatus();
    }, [formEditMode, renderSwitchStatus]);

    const handleNewOrder = () => {
        setOrder({orderId: undefined, currencyUomId: baseCurrency?.currencyUomId});
        setFormEditMode(1)
        dispatch(setUiOrderItems([]));
        dispatch(resetUiOrderItems());
        dispatch(resetUiOrderAdjustments());
        dispatch(setUiOrderAdjustments([]));
        dispatch(setSupplierId(undefined))
        formRef2.current = !formRef2?.current
    };

    const handleCancelForm = () => {
        dispatch(setUiOrderItems([]));
        dispatch(resetUiOrderItems());
        setFormEditMode(0)
        dispatch(resetUiOrderAdjustments());
        dispatch(setUiOrderAdjustments([]));
        dispatch(setOrderFormEditMode(0))
        formRef2.current = !formRef2.current;
        cancelEdit()
    };

    // menu select event handler
    const handleMenuSelect = useCallback(
        (e: MenuSelectEvent) => {
            const menuItem = e.item.text;
            if (menuItem === "New Order") {
                handleNewOrder();
            } else if (menuItem === "Receive Inventory") {
                dispatch(setSelectedApprovedPurchaseOrder({orderId: selectedOrder ? selectedOrder.orderId! : order?.orderId}));
                navigate("/receiveInventory");
            }
            // No submission actions in menu
        },
        [dispatch, navigate, selectedOrder, order, handleNewOrder]
    );

    const handleSubmit = useCallback(
        async (formProps: any) => {
            if (!formProps.isValid) {
                toast.error("Form is invalid");
                setIsLoading(false);
                return false;
            }
            if (isSubmitting) {
                // Prevent multiple submissions
                return false;
            }
            setIsSubmitting(true); // Lock submission
            const values = formProps.values;
            const actionType = formEditMode === 1 ? "Create Order" : formEditMode === 2 ? "Update Order" : "Approve Order";

            try {
                // Perform the primary action (Create or Update)
                const result = await handleCreate({ values, selectedMenuItem: actionType });

                // Chain Approve Order only after a successful Create Order
                if (formEditMode === 1 && result?.orderId) {
                    await handleCreate({
                        values: { ...values, orderId: result.orderId },
                        selectedMenuItem: "Approve Order"
                    });
                }
            } catch (error) {
                toast.error("Operation failed");
                setIsSubmitting(false);
            }
        },
        [handleCreate, formEditMode, isSubmitting]
    );

    useEffect(() => {
        if (!isLoading) {
            setIsSubmitting(false);
        }
    }, [isLoading]);

    const onSupplierChange = useCallback(
        (event: any) => {
            if (event.value === null) {
                dispatch(resetUiOrderItems());
                dispatch(resetUiOrderAdjustments());
            }
        }, [dispatch]
    );

    const status = renderSwitchStatus();
    const isLoadingCombined = isLoading || isAddPurchaseOrderLoading || isUpdatePurchaseOrderLoading; 

    return (
        <>
            {isLoadingCombined && (
                // Purpose: Display LoadingComponent when either local or RTK Query loading is active
                <LoadingComponent
                    message='Processing Order...'
                    style={{ zIndex: 9999, position: 'fixed', top: 0, left: 0, width: '100%', height: '100%' }}
                />
            )}
            <OrderMenu selectedMenuItem={'/orders'}/>

            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container spacing={2} alignItems={"center"} position={"relative"}>
                    <Grid item xs={10}>
                        <Box display='flex' justifyContent='space-between'>
                            <Typography
                                sx={{
                                    fontWeight: "bold",
                                    paddingLeft: 3,
                                    fontSize: '18px',
                                    color: formEditMode === 1 ? "green" : "black"
                                }}
                                variant="h6"
                            >
                                {" "}
                                {order && order?.orderId ? `Purchase Order No: ${order?.orderId}` : "New Purchase Order"}{" "}
                            </Typography>
                        </Box>
                    </Grid>

                    <Grid item xs={1}>
                        <Menu onSelect={handleMenuSelect}>
                            <MenuItem text={getTranslatedLabel("general.actions", "Actions")}>
                                <MenuItem text="New Order"/>
                                {formEditMode === 3 && <MenuItem text="Receive Inventory"/>}
                            </MenuItem>
                        </Menu>
                    </Grid>
                    <Grid item xs={1}>
                        {formEditMode > 1 && (
                            <RibbonContainer>
                                <Ribbon
                                    side={language === "ar" ? "left" : "right"}
                                    type="corner"
                                    size="large"
                                    backgroundColor={status.backgroundColor}
                                    color={status.foreColor}
                                    fontFamily="sans-serif"
                                >
                                    {order?.statusDescription}
                                </Ribbon>
                            </RibbonContainer>
                        )}
                    </Grid>
                </Grid>

                <Form
                    ref={formRef}
                    initialValues={initialFormValues}
                    key={formKey}
                    onSubmitClick={values => handleSubmit(values)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className={'k-form-fieldset'}>
                                <Grid container alignItems={"start"} justifyContent="start" spacing={1}>

                                    <Grid container spacing={2} alignItems={"center"} justifyContent={"flex-start"}
                                          xs={12} sx={{paddingLeft: 3}}>
                                        <Grid item container xs={9} spacing={2} alignItems={"flex-end"}>
                                            <Grid item xs={3}
                                                  className={formEditMode > 2 ? "grid-disabled" : "grid-normal"}>
                                                <Field
                                                    id={"fromPartyId"}
                                                    name={"fromPartyId"}
                                                    label={"Supplier"}
                                                    component={FormComboBoxVirtualSupplier}
                                                    autoComplete={"off"}
                                                    disabled={formEditMode > 1}
                                                    onChange={onSupplierChange}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Field
                                                    id="currencyUomId"
                                                    name="currencyUomId"
                                                    component={MemoizedFormDropDownList2}
                                                    data={currencies ?? []}
                                                    label={'Currency'}
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
                                            {/*{(agreements && agreements?.length > 0 && supplierId !== undefined) && (
                                                <Grid item xs={4}>
                                                    <Field
                                                        id="agreementId"
                                                        name="agreementId"
                                                        label={'Agreement'}
                                                        component={MemoizedFormDropDownList2}
                                                        data={agreements ?? []}
                                                        disabled={formEditMode > 1}
                                                        dataItemKey={"agreementId"}
                                                        textField={"description"}
                                                    />
                                                </Grid>
                                            )} */}
                                        </Grid>
                                        <Grid item container xs={3} spacing={2} alignItems="flex-end">
                                            {memoizedOrderTotals}
                                        </Grid>
                                        <Grid container item xs={12} spacing={2}>
                                            <Grid item xs={5}
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
                                                    onChange={handleInternalRemarksChange}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>

                                    <Grid item xs={12}>
                                        <Grid container spacing={1} alignItems={"center"} sx={{ml: 1, mt: 3}}>
                                            <Grid item xs={9}>
                                                <PurchaseOrderItemsListMemo orderFormEditMode={formEditMode}
                                                                            orderId={order ? order.orderId : undefined}/>
                                            </Grid>
                                        </Grid>
                                    </Grid>

                                </Grid>

                                <div className="k-form-buttons">
                                    <Grid container spacing={2}>
                                        {/* Purpose: Disable button during submission to prevent double calls */}
                                        {(formEditMode === 1 || formEditMode === 2) && (
                                            <Grid item>
                                                <LoadingButton
                                                    size="large"
                                                    type="submit"
                                                    loading={isLoading}
                                                    variant="contained"
                                                    //disabled={!formRenderProps.isValid || isSubmitting}
                                                    onClick={() => {
                                                        console.log('Submit clicked. Form state:', {
                                                            isValid: formRenderProps.isValid,
                                                            modified: formRenderProps.modified,
                                                            allowSubmit: formRenderProps.allowSubmit,
                                                            values: formRenderProps.valueGetter(),
                                                            touched: formRenderProps.touched
                                                        });
                                                        formRef.current.onSubmit();
                                                    }}
                                                >
                                                    {isLoading
                                                        ? getTranslatedLabel(`${localizationKey}.processing`, "Processing...")
                                                        : getTranslatedLabel(
                                                            formEditMode === 1 ? `${localizationKey}.actions.create` : `${localizationKey}.actions.update`,
                                                            formEditMode === 1 ? "Create Order" : "Update Order"
                                                        )}
                                                </LoadingButton>
                                            </Grid>
                                        )}
                                        <Grid item>
                                            <Button
                                                onClick={handleCancelForm}
                                                size="large"
                                                color="error"
                                                variant="outlined"
                                            >
                                                {getTranslatedLabel("general.cancel", "Cancel")}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </div>
                                
                            </fieldset>

                        </FormElement>

                    )}
                />

            </Paper>


        </>

    );
}