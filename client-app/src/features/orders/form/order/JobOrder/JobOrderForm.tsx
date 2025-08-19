import {Field, Form, FormElement} from "@progress/kendo-react-form";
import React, {useEffect, useState} from "react";
import {useSelector} from "react-redux";
import {jobOrderSubTotal, resetUiJobOrder, setUiJobOrderItems,} from "../../../slice/jobOrderUiSlice";
import {useAppDispatch, useAppSelector, useFetchCustomerTaxStatusQuery,} from "../../../../../app/store/configureStore";
import useJobOrder from "../../../hook/useJobOrder";
import JobOrderAdjustmentsList from "../../../dashboard/jobOrder/JobOrderAdjustmentsList";
import {Grid, Paper, Typography} from "@mui/material";
import {Box} from "@mui/system";
import {FormComboBoxVirtualCustomer} from "../../../../../app/common/form/FormComboBoxVirtualCustomer";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import {
    FormMultiColumnComboBoxVirtualVehicle
} from "../../../../../app/common/form/FormMultiColumnComboBoxVirtualVehicle";
import JobOrderPaymentsList from "../../../dashboard/jobOrder/JobOrderPaymentsList";
import FormTextArea from "../../../../../app/common/form/FormTextArea";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import JobOrderTotals from "./JobOrderTotals";
import JobOrderItemsList from "../../../dashboard/jobOrder/JobOrderItemsList";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";

interface Props {
    selectedOrder?: any;
    editMode: number;
    cancelEdit: () => void;
}

export default function JobOrderForm({
                                         selectedOrder,
                                         cancelEdit,
                                         editMode,
                                     }: Props) {
    const [showList, setShowList] = useState(false);
    const [showPaymentList, setShowPaymentList] = useState(false);
    const formRef = React.useRef<Form>(null);
    const formRef2 = React.useRef<boolean>(false);
    const [selectedMenuItem, setSelectedMenuItem] = React.useState("");
    const {getTranslatedLabel} = useTranslationHelper()

    const [isLoading, setIsLoading] = useState(false);
    const jobOrderSTotal: any = useSelector(jobOrderSubTotal);
    const customerId = useAppSelector(
        (state) => state.jobOrderUi.selectedCustomerId,
    );
    const {data: customerTaxStatus} = useFetchCustomerTaxStatusQuery(
        customerId,
        {skip: customerId === undefined},
    );

    const language = useAppSelector((state) => state.localization.language);

    const {order, setOrder, formEditMode, setFormEditMode, handleCreate} =
        useJobOrder({
            selectedMenuItem,
            editMode,
            formRef2,
            selectedOrder,
            setIsLoading,
        });

    console.log("formEditMode from form", formEditMode);
    console.log("editMode from form", editMode);

    const dispatch = useAppDispatch();

    console.log("selectedOrder", selectedOrder);

    useEffect(() => {
        if (selectedOrder) {
            setOrder(selectedOrder);
        }
    }, [selectedOrder, setOrder]);

    useEffect(() => {
        if (formEditMode < 2) {
            setOrder(undefined);
        }
    }, [editMode, formEditMode, setOrder]);

    const renderSwitchOrderStatus = () => {
        switch (formEditMode) {
            case 1:
                return "New";
            case 2:
                return "Created";
            case 3:
                return "Approved";
            case 4:
                return "Completed";
        }
    };

    // menu select event handler
    async function handleMenuSelect(e: MenuSelectEvent) {
        if (e.item.text === "Update Job Order") {
            setSelectedMenuItem("Update Job Order");
            setTimeout(() => {
                // @ts-ignore
                formRef.current.onSubmit();
            });
        }
        if (e.item.text === "Approve Job Order") {
            setSelectedMenuItem("Approve Job Order");
            setTimeout(() => {
                // @ts-ignore
                formRef.current.onSubmit();
            });
        }

        if (e.item.text === "Complete Order") {
            setSelectedMenuItem("Complete Job Order");
            setTimeout(() => {
                // @ts-ignore
                formRef.current.onSubmit();
            });
        }
        if (e.item.text === "Order Adjustments") {
            setShowList(true);
        }
        if (e.item.text === "Payments") {
            setShowPaymentList(true);
        }
    }

    const handleSubmit = (data: any) => {
        if (!data.isValid) {
            return false;
        }
        setIsLoading(true);
        handleCreate(data);
    };

    const handleCancelForm = () => {
        dispatch(setUiJobOrderItems([]));
        dispatch(resetUiJobOrder(null));
        formRef2.current = !formRef2.current;
        cancelEdit();
    };

    return (
        <>
            <JobOrderAdjustmentsList
                onClose={() => setShowList(false)}
                showList={showList}
                orderId={formEditMode === 1 ? undefined : order?.orderId}
                width={700}
            />
            <JobOrderPaymentsList
                onClose={() => setShowPaymentList(false)}
                showPaymentList={showPaymentList}
                orderId={formEditMode === 1 ? undefined : order?.orderId}
                width={630}
            />

            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container spacing={2} alignItems={"center"}>
                    <Grid item xs={9}>
                        <Box display="flex" justifyContent="space-between">
                            <Typography sx={{p: 2}} variant="h4" color={"green"}>
                                Job Order
                            </Typography>
                        </Box>
                    </Grid>

                    <Grid item xs={3}>
                        <div className="col-md-6">
                            <Menu onSelect={handleMenuSelect}>
                                <MenuItem text={getTranslatedLabel("general.actions", "Actions")}>
                                    {formEditMode === 2 && <MenuItem text="Update Job Order"/>}
                                    {formEditMode === 2 && <MenuItem text="Approve Job Order"/>}
                                    {formEditMode === 3 && <MenuItem text="Complete Job Order"/>}
                                </MenuItem>
                            </Menu>
                        </div>
                    </Grid>
                </Grid>

                <Grid container spacing={2} alignItems={"center"}>
                    <Grid item xs={3}>
                        <Box display="flex" justifyContent="space-between">
                            <Typography sx={{fontWeight: "bold"}} variant="h6">
                                {" "}
                                {order && order?.orderId}{" "}
                            </Typography>
                        </Box>
                    </Grid>
                    <Grid item xs={4}>
                        <Typography color="primary" sx={{p: 2}} variant="h5">
                            {"Status:  " + renderSwitchOrderStatus()}
                        </Typography>
                    </Grid>
                </Grid>

                <Form
                    ref={formRef}
                    initialValues={order}
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
                                            disabled={editMode > 1}
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
                                            id={"fromPartyId"}
                                            name={"fromPartyId"}
                                            label={"Customer"}
                                            component={FormComboBoxVirtualCustomer}
                                            autoComplete={"off"}
                                            validator={requiredValidator}
                                            disabled={editMode > 1}
                                        />
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
                                            disabled={formEditMode > 2}
                                        />
                                    </Grid>

                                    {customerId !== undefined &&
                                    customerTaxStatus &&
                                    customerTaxStatus.isExempt !== "N" ? (
                                        <Grid
                                            item
                                            xs={3}
                                            style={{
                                                display: "flex",
                                                justifyContent: "center",
                                                alignItems: "center",
                                            }}
                                        >
                                            <Typography color="error" variant="h6">
                                                Customer is Tax Exempt
                                            </Typography>
                                        </Grid>
                                    ) : (
                                        <Grid item xs={3} style={{minHeight: "56px"}}/> // Adjust the minHeight as needed
                                    )}
                                </Grid>
                                <Grid container justifyContent="center">
                                    <Grid item xs={10}>
                                        <JobOrderItemsList
                                            orderFormEditMode={formEditMode}
                                            orderId={order ? order.orderId : undefined}
                                        />
                                    </Grid>
                                    <Grid item xs={2}>
                                        <div className="col-md-6">
                                            <Menu onSelect={handleMenuSelect}>
                                                <MenuItem
                                                    text="Order Adjustments"
                                                    disabled={jobOrderSTotal === 0}
                                                />
                                                <MenuItem text="Payments"/>
                                            </Menu>
                                        </div>
                                    </Grid>
                                </Grid>
                                <Grid container spacing={2} alignItems="flex-end">
                                    <JobOrderTotals/>
                                </Grid>

                                <Grid container spacing={1}>
                                    <Grid item xs={1}>
                                        <Button
                                            sx={{m: 1}}
                                            onClick={handleCancelForm}
                                            variant="contained"
                                            color="error"
                                        >
                                            Cancel
                                        </Button>
                                    </Grid>
                                </Grid>

                                {isLoading && (
                                    <LoadingComponent message="Processing Job Order..."/>
                                )}
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}
