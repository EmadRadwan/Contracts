import React, { useCallback, useMemo, useRef, useState } from "react";
import { Button, Grid, Paper, Typography, Box } from "@mui/material";
import {
    Field,
    Form,
    FormElement,
    FormRenderProps,
} from "@progress/kendo-react-form";
import { toast } from "react-toastify";

import { requiredValidator } from "../../../../../app/common/form/Validators";

import FormDatePicker from "../../../../../app/common/form/FormDatePicker";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import FormTextArea from "../../../../../app/common/form/FormTextArea";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import ModalContainer from "../../../../../app/common/modals/ModalContainer";
import CreateCustomerModalForm from "../../../../parties/form/CreateCustomerModalForm";

import { FormSimpleComboBoxVirtualApartment } from "../../../../../app/common/form/FormSimpleComboBoxVirtualApartment";
import { FormComboBoxVirtualCustomer } from "../../../../../app/common/form/FormComboBoxVirtualCustomer";
import { FormComboBoxVirtualPartyEmployee } from "../../../../../app/common/form/FormComboBoxVirtualPartyEmployee";

import { ReserveRequest } from "../../../../../app/models/order/ReserveRequest";
import {
    useAddReserveRequestMutation,
    useUpdateReserveRequestMutation,
} from "../../../../../app/store/apis/salesRequestApi";
import { FormComboBox } from "../../../../../app/common/form/FormComboBox";
import SalesRequestMenu from "../menu/SalesRequestMenu";

interface Props {
    reserveRequest?: ReserveRequest;
    editMode: number; // 1 = create, 2 = edit
    cancelEdit: () => void;
    onReserveRequestCreated?: (created: ReserveRequest) => void;
    onReserveRequestUpdated?: (updated: ReserveRequest) => void;
}

function ReserveRequestForm({
                                reserveRequest,
                                editMode,
                                cancelEdit,
                                onReserveRequestCreated,
                                onReserveRequestUpdated,
                            }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();

    const [createRR, { isLoading: isCreating }] = useAddReserveRequestMutation();
    const [updateRR, { isLoading: isUpdating }] = useUpdateReserveRequestMutation();

    const [showNewCustomer, setShowNewCustomer] = useState(false);
    const formRef = useRef<FormRenderProps | null>(null);

    // -----------------------------------------------------------------
    // Helper to turn combo-box objects into simple IDs for backend
    // -----------------------------------------------------------------
    const flattenComboValues = (data: any) => {
        const copy = { ...data };

        if (copy.productId && typeof copy.productId === "object") {
            copy.productId = copy.productId.apartmentId ?? copy.productId.ProductId;
        }
        if (copy.fromPartyId && typeof copy.fromPartyId === "object") {
            copy.fromPartyId = copy.fromPartyId.fromPartyId ?? copy.fromPartyId.partyId;
        }
        if (copy.employeePartyId && typeof copy.employeePartyId === "object") {
            copy.employeePartyId = copy.employeePartyId.fromPartyId;
        }

        return copy;
    };

    const paymentMethods = [
        { payMethod: "CASH", payMethodName: getTranslatedLabel("reserveRequest.payMethod.cash", "Cash") },
        { payMethod: "BANK_TRANSFER", payMethodName: getTranslatedLabel("reserveRequest.payMethod.bankTransfer", "Bank Transfer") },
        { payMethod: "CHEQUE", payMethodName: getTranslatedLabel("reserveRequest.payMethod.cheque", "Cheque") },
    ];

    const chequeStatusOptions = [
        { chequeStatus: "COLLECT", label: getTranslatedLabel("reserveRequest.chequeStatus.collect", "Collect") },
        { chequeStatus: "REGISTER", label: getTranslatedLabel("reserveRequest.chequeStatus.register", "Register") },
    ];

    // -----------------------------------------------------------------
    // Conditional validator for chequeStatus
    // -----------------------------------------------------------------
    const chequeStatusValidator = (value: any, valueGetter: any) => {
        const payMethod = valueGetter("payMethod");
        if (payMethod === "CHEQUE" && (!value || value.trim() === "")) {
            return "Cheque status is required when payment method is Cheque";
        }
        return undefined;
    };

    // -----------------------------------------------------------------
    // Submit handler
    // -----------------------------------------------------------------
    const handleSubmitData = async (data: any) => {
        try {
            const flattened = flattenComboValues(data);

            const payload = {
                reserveRequestDto: {
                    ...(editMode === 2 && { reserveRequestId: reserveRequest?.reserveRequestId }),
                    ...flattened
                }
            };
            if (editMode === 2) {
                const updated = await updateRR(payload).unwrap();
                toast.success(getTranslatedLabel("reserveRequest.updated", "Reserve request updated"));
                onReserveRequestUpdated?.(updated as ReserveRequest);
            } else {
                const created = await createRR(payload).unwrap();
                toast.success(getTranslatedLabel("reserveRequest.created", "Reserve request created"));
                onReserveRequestCreated?.(created as ReserveRequest);
            }
        } catch (error: any) {
            toast.error(
                error?.data?.errors
                    ? Object.values(error.data.errors).flat().join(" ")
                    : getTranslatedLabel("reserveRequest.error", "Failed to save reserve request")
            );
        }
    };

    // -----------------------------------------------------------------
    // Update customer combo after creating a new customer
    // -----------------------------------------------------------------
    const updateCustomerDropDown = useCallback(
        (newCustomer: { partyId: string; description: string }) => {
            formRef.current?.onChange("fromPartyId", {
                value: { fromPartyId: newCustomer.partyId, fromPartyName: newCustomer.description },
            });
        },
        []
    );

    // -----------------------------------------------------------------
    // Initial values – SAME PATTERN as SalesRequestForm
    // -----------------------------------------------------------------
    const today = new Date();
    const formInitialValues = useMemo(() => {
        if (editMode === 1) {
            // Create mode
            return {
                reserveDate: today,
                reserveAmount: null,
                payMethod: null,
                chequeStatus: null,
                comments: null,
                fromPartyId: null,
                employeePartyId: null,
                productId: null,
            };
        }

        // Edit mode – guaranteed reserveRequest
        const rr = reserveRequest!;

        // Apartment object for virtual ComboBox
        const apartmentObj = rr.apartmentId
            ? {
                apartmentId: rr.apartmentId,
                apartmentName: rr.apartmentName ?? "",
                projectName: rr.projectName ?? "",
                apartmentSpaceM2: rr.apartmentSpaceM2,
                gardenSpaceM2: rr.gardenSpaceM2,
                apartmentStatusDescription: rr.apartmentStatusDescription ?? "",
                floorNumber: rr.floorNumber ?? "",
            }
            : null;

        // Customer object
        const customerObj = rr.fromPartyId
            ? {
                fromPartyId: rr.fromPartyId,
                fromPartyName: rr.fromPartyName ?? "",
            }
            : null;

        // Employee object
        const employeeObj = rr.employeePartyId
            ? {
                fromPartyId: rr.employeePartyId,
                fromPartyName: rr.employeeName ?? "",
            }
            : null;

        return {
            reserveRequestId: rr.reserveRequestId,
            productId: apartmentObj,
            fromPartyId: customerObj,
            employeePartyId: employeeObj,
            reserveDate: rr.reserveDate ? new Date(rr.reserveDate) : null,
            reserveAmount: rr.reserveAmount,
            payMethod: rr.payMethod,
            chequeStatus: rr.chequeStatus ?? null,
            comments: rr.comments,
        };
    }, [editMode, reserveRequest]);

    // -----------------------------------------------------------------
    // Render
    // -----------------------------------------------------------------
    return (
        <>
            <SalesRequestMenu
                onMenuSelect={(key) => {
                    if (key === "salesRequest.menu.reserveRequests") {
                        cancelEdit();
                    }
                }}
            />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Box sx={{ p: 2 }}>
                    <Typography variant="h4" gutterBottom>
                        {reserveRequest?.reserveRequestId
                            ? `${getTranslatedLabel("reserveRequest.titleEdit", "Reserve Request")}: ${reserveRequest.reserveRequestId}`
                            : getTranslatedLabel("reserveRequest.titleNew", "New Reserve Request")}
                    </Typography>
                </Box>

                <Form
                    key={editMode}
                    initialValues={formInitialValues}
                    onSubmit={handleSubmitData}
                    render={(formRenderProps: FormRenderProps) => {
                        formRef.current = formRenderProps;
                        const { valueGetter } = formRenderProps;
                        const apt = valueGetter("productId");
                        const payMethod = valueGetter("payMethod");

                        return (
                            <FormElement>
                                <fieldset className="k-form-fieldset">
                                    {/* Same layout as before */}
                                    <Grid container spacing={2} alignItems="flex-end">
                                        <Grid item xs={4}>
                                            <Field
                                                name="productId"
                                                label={getTranslatedLabel("reserveRequest.apartment", "Apartment *")}
                                                component={FormSimpleComboBoxVirtualApartment}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={2}>
                                            <Field
                                                name="reserveDate"
                                                label={getTranslatedLabel("reserveRequest.reserveDate", "Reserve Date *")}
                                                component={FormDatePicker}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={2.5}>
                                            <Field
                                                name="fromPartyId"
                                                label={getTranslatedLabel("reserveRequest.customer", "Customer *")}
                                                component={FormComboBoxVirtualCustomer}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={0.5}>
                                            <Button
                                                size="small"
                                                color="secondary"
                                                variant="outlined"
                                                onClick={() => setShowNewCustomer(true)}
                                                sx={{ height: "100%", minWidth: 32, p: 0 }}
                                            >
                                                +
                                            </Button>
                                        </Grid>
                                        <Grid item xs={2}>
                                            <Field
                                                name="employeePartyId"
                                                label={getTranslatedLabel("reserveRequest.employee", "Employee")}
                                                component={FormComboBoxVirtualPartyEmployee}
                                            />
                                        </Grid>
                                    </Grid>

                                    {/* Apartment details */}
                                    <Grid container spacing={2} mt={1}>
                                        {apt && (
                                            <>
                                                <Grid item xs={3}><Typography variant="caption" color="textSecondary">{getTranslatedLabel("reserveRequest.project", "Project")}</Typography><Typography>{apt.projectName ?? "-"}</Typography></Grid>
                                                <Grid item xs={2}><Typography variant="caption" color="textSecondary">Apt m²</Typography><Typography>{apt.apartmentSpaceM2 ?? "-"}</Typography></Grid>
                                                <Grid item xs={2}><Typography variant="caption" color="textSecondary">Garden m²</Typography><Typography>{apt.gardenSpaceM2 ?? "-"}</Typography></Grid>
                                                <Grid item xs={3}><Typography variant="caption" color="textSecondary">{getTranslatedLabel("reserveRequest.status", "Status")}</Typography><Typography>{apt.apartmentStatusDescription ?? "-"}</Typography></Grid>
                                            </>
                                        )}
                                    </Grid>

                                    <Grid container spacing={2} mt={2}>
                                        <Grid item xs={3}>
                                            <Field
                                                name="reserveAmount"
                                                label={getTranslatedLabel("reserveRequest.amount", "Reserve Amount *")}
                                                component={FormNumericTextBox}
                                                format="n2"
                                                min={0}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="payMethod"
                                                name="payMethod"
                                                label={getTranslatedLabel("reserveRequest.payMethod", "Payment Method *")}
                                                component={FormComboBox}
                                                data={paymentMethods}
                                                dataItemKey="payMethod"
                                                textField="payMethodName"
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        {payMethod === "CHEQUE" && (
                                            <Grid item xs={3}>
                                                <Field
                                                    id="chequeStatus"
                                                    name="chequeStatus"
                                                    label={getTranslatedLabel("reserveRequest.chequeStatus", "Cheque Status *")}
                                                    component={FormComboBox}
                                                    data={chequeStatusOptions}
                                                    dataItemKey="chequeStatus"
                                                    textField="label"
                                                    validator={chequeStatusValidator}
                                                />
                                            </Grid>
                                        )}
                                    </Grid>

                                    <Grid container spacing={2} mt={2}>
                                        <Grid item xs={6}>
                                            <Field
                                                name="comments"
                                                label={getTranslatedLabel("reserveRequest.comments", "Comments")}
                                                component={FormTextArea}
                                                rows={3}
                                            />
                                        </Grid>
                                    </Grid>

                                    <div className="k-form-buttons" style={{ marginTop: 16 }}>
                                        <Grid container spacing={2}>
                                            <Grid item>
                                                <Button
                                                    type="submit"
                                                    variant="contained"
                                                    color="success"
                                                    disabled={isCreating || isUpdating || !formRenderProps.allowSubmit}
                                                >
                                                    {editMode === 1 ? getTranslatedLabel("general.create", "Create") : getTranslatedLabel("general.update", "Update")}
                                                </Button>
                                            </Grid>
                                            <Grid item>
                                                <Button variant="contained" color="error" onClick={cancelEdit}>
                                                    {getTranslatedLabel("general.cancel", "Cancel")}
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </div>

                                    {(isCreating || isUpdating) && <Box mt={2}><Typography>Processing...</Typography></Box>}
                                </fieldset>
                            </FormElement>
                        );
                    }}
                />
            </Paper>

            {showNewCustomer && (
                <ModalContainer show={showNewCustomer} onClose={() => setShowNewCustomer(false)} width={500}>
                    <CreateCustomerModalForm
                        onClose={() => setShowNewCustomer(false)}
                        onUpdateCustomerDropDown={updateCustomerDropDown}
                    />
                </ModalContainer>
            )}
        </>
    );
}

export default React.memo(ReserveRequestForm);