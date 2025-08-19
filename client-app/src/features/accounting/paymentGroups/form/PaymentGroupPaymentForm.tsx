import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {
    useAddPaymentGroupMemberMutation,
    useAppDispatch,
    useAppSelector, useFetchPaymentGroupMembersQuery,
    useUpdatePaymentGroupMemberMutation,

} from "../../../../app/store/configureStore";
import { useCallback, useRef, useState, useEffect } from "react";
import { requiredValidator } from "../../../../app/common/form/Validators";
import {
    Field,
    Form,
    FormElement,
    FormRenderProps,
} from "@progress/kendo-react-form";
import { Button, Grid, Paper, Typography } from "@mui/material";
import FormInput from "../../../../app/common/form/FormInput";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { toast } from "react-toastify";
import PaymentGroupMenu from "../menu/PaymentGroupMenu";
import { router } from "../../../../app/router/Routes";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { setSelectedPaymentGroupMember } from "../../slice/accountingSharedUiSlice";
import { useFetchIncomingPaymentsQuery, useFetchOutgoingPaymentsQuery } from "../../../../app/store/apis";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { FormMultiComboBoxVirtualIncomingPayments } from "../../../../app/common/form/FormMultiComboBoxVirtualIncomingPayments";
import { FormMultiComboBoxVirtualOutgoingPayments } from "../../../../app/common/form/FormMultiComboBoxVirtualOutgoingPayments";


// Props interface for the PaymentForm component
interface Props {
    editMode: number;
    cancelEdit: () => void;
}

export default function PaymentGroupPaymentForm({
    cancelEdit,
    editMode,
}: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const { selectedPaymentGroupMember, selectedPaymentGroup } = useAppSelector(state => state.accountingSharedUi)
    const localizationKey = "accounting.pay-group.form";
    const dispatch = useAppDispatch();
    const formRef = useRef<any>(null);
    const [updateGroupMember, { isLoading: isUpdating }] = useUpdatePaymentGroupMemberMutation()
    const [createGroupMember, { isLoading: isAdding }] = useAddPaymentGroupMemberMutation()
    const [payments, setPayments] = useState<any[]>([])
    // const {data: incomingPayments, isFetching: isFetchingIncomingPayments} = useFetchIncomingPaymentsQuery()
    // const {data: outgoingPayments, isFetching: isFetchingOutgoingPayments} = useFetchOutgoingPaymentsQuery()
    const isCheckRun = selectedPaymentGroup?.paymentGroupTypeId === "CHECK_RUN"
    

    const [buttonFlag, setButtonFlag] = useState(false);
   
    if (!selectedPaymentGroup) {
        router.navigate("/paymentGroups")
    }

    const handleCancelForm = useCallback(() => {
        cancelEdit();
        dispatch(setSelectedPaymentGroupMember(undefined));
    }, [dispatch, cancelEdit]);


    const handleSubmit = async (data: any) => {
        console.log(data)
        setButtonFlag(true)
        try {
            if (editMode === 1) {
                data.paymentGroupId = selectedPaymentGroup?.paymentGroupId
                await createGroupMember({...data, paymentId: data.paymentId.paymentId}).unwrap()
                toast.success("Payment group member created successfully")
            } else {
                data.paymentGroupId = selectedPaymentGroup?.paymentGroupId
                await updateGroupMember(data).unwrap()
                toast.success("Payment group member updated successfully")
            }            
            handleCancelForm()
        } catch (e) {
            console.error(e)
            toast.error("Something went wrong")
        } finally {
            setButtonFlag(false)
        }
    }

    return (
        <>
            <AccountingMenu selectedMenuItem="/paymentGroups" />
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                {editMode > 1 && <PaymentGroupMenu selectedMenuItem="/paymentGroups/payments" />}
                <Typography sx={{ p: 2 }} color={editMode > 1 ? "black" : "green"} variant="h4">
                    {editMode === 1
                        ? `New Payment Group Member for Payment Group: ${selectedPaymentGroup?.paymentGroupId}`
                        : `Payment: ${selectedPaymentGroupMember?.paymentId}`}
                </Typography>
                <Form
                    ref={formRef}
                    onSubmit={handleSubmit}
                    initialValues={selectedPaymentGroupMember ? { ...selectedPaymentGroupMember } : null}
                    render={(formRenderProps: FormRenderProps) => {
                        return (
                            <FormElement>
                                <fieldset className="k-form-fieldset">
                                    <Grid container spacing={2} padding={2}>
                                        <Grid item xs={12}>
                                            <Grid container spacing={2} alignItems="flex-end">
                                                <Grid item container xs={12}>
                                                    <Grid item xs={4}>
                                                        {isCheckRun ? <Field
                                                            id="paymentId"
                                                            name="paymentId"
                                                            label={getTranslatedLabel(
                                                                `${localizationKey}.paymentId`,
                                                                "Payment *"
                                                            )}
                                                            component={FormMultiComboBoxVirtualOutgoingPayments}
                                                            // data={payments ?? []}
                                                            // dataItemKey="paymentId"
                                                            // textField="text"
                                                            validator={requiredValidator}
                                                            disabled={editMode > 1}
                                                        /> : <Field
                                                            id="paymentId"
                                                            name="paymentId"
                                                            label={getTranslatedLabel(
                                                                `${localizationKey}.paymentId`,
                                                                "Payment *"
                                                            )}
                                                            component={ FormMultiComboBoxVirtualIncomingPayments}
                                                            // data={payments ?? []}
                                                            // dataItemKey="paymentId"
                                                            // textField="text"
                                                            validator={requiredValidator}
                                                            disabled={editMode > 1}
                                                        />}
                                                    </Grid>
                                                </Grid>
                                                <Grid item xs={4}>
                                                    <Field
                                                        id="fromDate"
                                                        name="fromDate"
                                                        label={getTranslatedLabel(
                                                            `${localizationKey}.fromDate`,
                                                            "From Date"
                                                        )}
                                                        component={FormDatePicker}
                                                        disabled={editMode > 1}
                                                    />

                                                </Grid>

                                                <Grid item xs={4}>
                                                    <Field
                                                        id="thruDate"
                                                        name="thruDate"
                                                        label={getTranslatedLabel(
                                                            `${localizationKey}.thruDate`,
                                                            "Thru Date"
                                                        )}
                                                        component={FormDatePicker}
                                                    />

                                                </Grid>
                                            </Grid>
                                        </Grid>

                                        <Grid container spacing={2}>
                                            <Grid item >
                                                <Button
                                                    type="submit"
                                                    variant="contained"
                                                    sx={{ mt: 2, ml: 2 }}
                                                    disabled={!formRenderProps.allowSubmit || buttonFlag}
                                                >
                                                    {editMode === 1 ? getTranslatedLabel(`general.create`, "Create") : getTranslatedLabel(`general.update`, "Update")}
                                                </Button>
                                            </Grid>
                                            <Grid item xs={1}>
                                                <Button
                                                    sx={{ mt: 2 }}
                                                    onClick={handleCancelForm}
                                                    color="error"
                                                    variant="contained"
                                                >
                                                    {getTranslatedLabel("general.cancel", "Cancel")}
                                                </Button>
                                            </Grid>
                                        </Grid>

                                    </Grid>
                                </fieldset>
                            </FormElement>
                        );
                    }}
                />
            </Paper>
        </>
    );
}