import React, {useCallback, useEffect, useState} from "react";

import Grid from "@mui/material/Grid";
import {Box, Paper, Typography} from "@mui/material";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import Button from "@mui/material/Button";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";

import {useSelector} from "react-redux";
import {useAppDispatch, useAppSelector, useFetchCustomerTaxStatusQuery} from "../../../app/store/configureStore";
import {Vehicle} from "../../../app/models/service/vehicle";
import useQuote from "../../services/hook/useQuote";
import {quoteSubTotal} from "../../orders/slice/quoteSelectors";
import {resetUiQuoteItems, setUiQuoteItems} from "../../orders/slice/quoteItemsUiSlice";
import {resetUiQuoteAdjustments, setUiQuoteAdjustments} from "../../orders/slice/quoteAdjustmentsUiSlice";
import {setCustomerId, setSelectedVehicle, setVehicleId} from "../../orders/slice/sharedOrderUiSlice";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import QuoteAdjustmentsList from "../../orders/dashboard/quote/QuoteAdjustmentsList";
import CreateCustomerModalForm from "../../parties/form/CreateCustomerModalForm";
import VehicleModalForm from "../../services/form/VehicleModalForm";
import VehicleAnnotation from "../../services/form/VehicleAnnotation";
import {FormMultiColumnComboBoxVirtualVehicle} from "../../../app/common/form/FormMultiColumnComboBoxVirtualVehicle";
import {FormComboBoxVirtualCustomer} from "../../../app/common/form/FormComboBoxVirtualCustomer";
import {requiredValidator} from "../../../app/common/form/Validators";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import JobQuoteItemsList from "./JobQuoteItemsList";
import QuoteTotals from "../form/QuoteTotals";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import FormTextArea from "../../../app/common/form/FormTextArea";
import VehicleMenu from "../menu/VehicleMenu";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";


interface Props {
    selectedQuote?: any;
    editMode: number;
    cancelEdit: () => void;
}

export default function JobQuoteForm({
                                      selectedQuote,
                                      cancelEdit,
                                      editMode,
                                  }: Props) {
    const [showList, setShowList] = useState(false);
    const [showNewCustomer, setShowNewCustomer] = useState(false);
    const [showNewVehicle, setShowNewVehicle] = useState(false);
    const [showVehicleAnnotation, setShowVehicleAnnotation] = useState(false);
    const formRef = React.useRef<any>();
    const formRef2 = React.useRef<boolean>(false);
    const [selectedMenuItem, setSelectedMenuItem] = React.useState("");
    const [vehicle, setVehicle] = React.useState<Vehicle | undefined>(undefined);

    const [isLoading, setIsLoading] = useState(false);
    const quoteSTotal: any = useSelector(quoteSubTotal);
    const customerId = useAppSelector(state => state.sharedOrderUi.selectedCustomerId);
    const {data: customerTaxStatus} = useFetchCustomerTaxStatusQuery(customerId,
        {skip: customerId === undefined});
    const {getTranslatedLabel} = useTranslationHelper()


    const {quote, setQuote, formEditMode, setFormEditMode, handleCreate} =
        useQuote({
            selectedMenuItem,
            editMode,
            formRef2,
            selectedQuote,
            setIsLoading,
        });

    const dispatch = useAppDispatch();
    console.log("Quote", quote);

    useEffect(() => {
        if (selectedQuote) {
            console.log("selectedQuote", selectedQuote);
            setQuote(selectedQuote);
            setVehicle(selectedQuote.vehicleId);
        }
    }, [selectedQuote, setQuote]);

    useEffect(() => {
        if (formEditMode < 2) {
            console.log("local editMode", editMode);
            console.count("local effect for resetting quote");
            setQuote(undefined);
        }
    }, [editMode, formEditMode, setQuote]);


    const renderSwitchQuoteStatus = () => {
        switch (formEditMode) {
            case 1:
                return "New";
            case 2:
                return "Created";
            case 3:
                return "Approved";
            case 4:
                return "Ordered";
        }
    };

    // menu select event handler
    async function handleMenuSelect(e: MenuSelectEvent) {
        if (e.item.text === "Create Job Quote") {
            setSelectedMenuItem("Create Job Quote");
            setTimeout(() => {
                formRef.current.onSubmit();
            });
        }
        if (e.item.text === "Update Job Quote") {
            setSelectedMenuItem("Update Job Quote");
            setTimeout(() => {
                formRef.current.onSubmit();
            });
        }
        if (e.item.text === "Create Job Order") {
            setSelectedMenuItem("Create Job Order");
            setTimeout(() => {
                formRef.current.onSubmit();
            });
        }
        if (e.item.text === "New Job Quote") {
            handleNewQuote();
        }
        if (e.item.text === "Quote Adjustments") {
            setShowList(true);
        }
        if (e.item.text === "Annotations") {
            setShowVehicleAnnotation(true);
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
        setQuote(undefined);
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

    const updateVehicleDropDown = (newVehicle: any, customer: any) => {
        formRef?.current.onChange("vehicleId", {value: newVehicle, valid: true});
        formRef?.current.onChange("fromPartyId", {value: customer, valid: true});
        dispatch(setCustomerId(customer.fromPartyId));
        dispatch(dispatch(setVehicleId(newVehicle.vehicleId)));
        dispatch(setSelectedVehicle(newVehicle));
        setVehicle(newVehicle);
    };

    const onCustomerChange = useCallback((event) => {
        if (event.value === null) {
            dispatch(setCustomerId(undefined));
        } else {
            dispatch(setCustomerId(event.value.fromPartyId));
        }
    }, []);

    const onVehicleChange = useCallback((event) => {
        if (event.value === null) {
            formRef?.current.onChange("fromPartyId", {
                value: {fromPartyId: "", fromPartyName: ""},
                valid: true,
            });
            dispatch(setCustomerId(undefined));
            dispatch(dispatch(setVehicleId(undefined)));
            dispatch(dispatch(setSelectedVehicle(undefined)));
            setVehicle(undefined);
        } else {
            formRef?.current.onChange("fromPartyId", {
                value: event.value.fromPartyId,
                valid: true,
            });
            dispatch(setCustomerId(event.value.fromPartyId.fromPartyId));
            dispatch(dispatch(setVehicleId(event.value.vehicleId)));
            const vehicle = {
                ...event.value,
                serviceDate: event.value.serviceDate.toISOString().split('T')[0]
            };
            dispatch(setSelectedVehicle(vehicle));
            setVehicle(event.value);
        }
    }, []);

    const memoizedOnClose = useCallback(
        () => {
            setShowList(false)
        },
        [],
    );

    const memoizedOnClose2 = useCallback(
        () => {
            setShowNewCustomer(false)
        },
        [],
    );

    const memoizedOnClose3 = useCallback(
        () => {
            setShowNewVehicle(false)
        },
        [],
    );

    const memoizedOnClose4 = useCallback(
        () => {
            setShowVehicleAnnotation(false)
        },
        [],
    );


    return (
        <>
            {showList && (<ModalContainer show={showList} onClose={memoizedOnClose} width={900}>
                <QuoteAdjustmentsList
                    onClose={memoizedOnClose}
                />
            </ModalContainer>)}

            {showNewCustomer && (<ModalContainer show={showNewCustomer} onClose={memoizedOnClose2} width={500}>
                <CreateCustomerModalForm
                    onClose={memoizedOnClose2}
                    onUpdateCustomerDropDown={updateCustomerDropDown}
                />
            </ModalContainer>)}

            {showNewVehicle && (<ModalContainer show={showNewVehicle} onClose={memoizedOnClose3} width={800}>
                <VehicleModalForm
                    onClose={() => setShowNewVehicle(false)}
                    onUpdateVehicleDropDown={updateVehicleDropDown}
                />
            </ModalContainer>)}

            {showVehicleAnnotation && (
                <ModalContainer show={showVehicleAnnotation} onClose={memoizedOnClose4} width={500}>
                    <VehicleAnnotation
                        vehicle={vehicle}
                        onClose={() => setShowVehicleAnnotation(false)}
                    />
                </ModalContainer>)}

            <VehicleMenu/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>

                <Grid container spacing={2} alignItems={"center"}>
                    <Grid item xs={3}>
                        <Box display="flex" justifyContent="space-between">
                            <Typography sx={{fontWeight: "bold"}} variant="h6">
                                {" "}
                                Quote No: {quote && quote?.quoteId}{" "}
                            </Typography>
                        </Box>
                    </Grid>
                    <Grid item xs={7}>
                        <Typography color="primary" sx={{p: 2}} variant="h5">
                            {"Status:  " + renderSwitchQuoteStatus()}
                        </Typography>
                    </Grid>

                    <Grid item xs={2}>
                        <div>
                            <Menu onSelect={handleMenuSelect}>
                                <MenuItem text={getTranslatedLabel("general.actions", "Actions")}>
                                    {formEditMode === 1 && <MenuItem text="Create Job Quote"/>}
                                    {formEditMode === 2 && <MenuItem text="Update Job Quote"/>}
                                    {formEditMode === 2 && <MenuItem text="Create Job Order"/>}
                                    {/* <MenuItem text="Duplicate Job Quote"/> */}
                                    <MenuItem text="New Job Quote"/>
                                </MenuItem>
                            </Menu>
                        </div>
                    </Grid>
                </Grid>

                <Form
                    ref={formRef}
                    initialValues={quote}
                    key={formRef2.current.toString()}
                    onSubmitClick={(values) => handleSubmit(values)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className={"k-form-fieldset"}>
                                <Grid container spacing={2} alignItems="flex-end">
                                    <Grid
                                        item
                                        xs={3}
                                        className={
                                            formEditMode > 2 ? "grid-disabled" : "grid-normal"
                                        }
                                    >
                                        <Field
                                            id={"vehicleId"}
                                            name={"vehicleId"}
                                            label={"Chassis No"}
                                            component={FormMultiColumnComboBoxVirtualVehicle}
                                            autoComplete={"off"}
                                            validator={requiredValidator}
                                            onChange={onVehicleChange}
                                            disabled={formEditMode > 2}
                                        />
                                    </Grid>
                                    <Grid item xs={2}>
                                        <Button
                                            color={"secondary"}
                                            onClick={() => {
                                                setShowNewVehicle(true);
                                            }}
                                            variant="outlined"
                                        >
                                            New Chassis
                                        </Button>
                                    </Grid>
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
                                                customerTaxStatus.isExempt !== "N"
                                                    ? <span style={{color: 'red'}}>Customer - Tax Exempt</span>
                                                    : "Customer"
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
                                </Grid>
                                <Grid container spacing={2} alignItems={"flex-end"}>
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
                                    <Grid item xs={2}>
                                        <Field
                                            id={"currentMileage"}
                                            name={"currentMileage"}
                                            label={"Current Mileage"}
                                            component={FormNumericTextBox}
                                            validator={requiredValidator}
                                            disabled={formEditMode > 2}
                                        />
                                    </Grid>


                                </Grid>

                                <Grid container justifyContent={"center"} alignItems={"center"} mt={6}>
                                    <Grid item xs={10} sx={{pr: 20}}>
                                        <JobQuoteItemsList
                                            quoteFormEditMode={formEditMode}
                                            quoteId={quote ? quote.quoteId : undefined}
                                        />
                                    </Grid>
                                    <Grid item xs={2}>

                                        <Menu onSelect={handleMenuSelect} vertical={true}>
                                            <MenuItem
                                                text="Quote Adjustments"
                                                disabled={quoteSTotal === 0}
                                            />
                                            <MenuItem text="Annotations"/>
                                        </Menu>
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
                                    <Grid item xs={2}>
                                        <QuoteTotals/>
                                    </Grid>
                                </Grid>


                                {isLoading && (
                                    <LoadingComponent message="Processing Job Quote..."/>
                                )}
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}

