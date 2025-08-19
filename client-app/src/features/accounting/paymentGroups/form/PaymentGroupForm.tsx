import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {
    useAddPaymentGroupMutation,
    useAppDispatch,
    useAppSelector, useCancelCheckRunMutation, useFetchPaymentGroupMembersQuery, useFetchPaymentGroupTypesQuery,
    useLazyFetchPaymentGroupMembersQuery,

} from "../../../../app/store/configureStore";
import { useCallback, useMemo, useRef, useState, useEffect } from "react";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { PaymentGroup } from "../../../../app/models/accounting/paymentGroup";
import { requiredValidator } from "../../../../app/common/form/Validators";
import {
    Field,
    Form,
    FormElement,
    FormRenderProps,
} from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { Button, Grid, Paper, Typography } from "@mui/material";
import FormInput from "../../../../app/common/form/FormInput";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { toast } from "react-toastify";
import PaymentGroupMenu from "../menu/PaymentGroupMenu";
import { router } from "../../../../app/router/Routes";
import { setPaymentGroupMemberFormEditMode, setSelectedPaymentGroupMember } from "../../slice/accountingSharedUiSlice";
import { handleDatesArray } from "../../../../app/util/utils";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridToolbar,
} from "@progress/kendo-react-grid";

// Props interface for the PaymentForm component
interface Props {
    editMode: number;
    cancelEdit: () => void;
}

export default function PaymentGroupForm({
    cancelEdit,
    editMode,
}: Props) {
    const { selectedPaymentGroup } = useAppSelector(state => state.accountingSharedUi)
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.pay-group.form";
    const localizationKeyList = "accounting.pay-group-payments.list";
    const [getMembers, { data: paymentGroupMembers, isLoading: isLoadingMembers }] = useLazyFetchPaymentGroupMembersQuery()
    const [data, setData] = useState<any[]>([]);
    const dispatch = useAppDispatch();
    const formRef = useRef<any>(null);
    const { data: paymentGroupTypes } = useFetchPaymentGroupTypesQuery(undefined)
    const [createGroup] = useAddPaymentGroupMutation();
    const [cancelCheckRun] = useCancelCheckRunMutation();

    const [buttonFlag, setButtonFlag] = useState(false);
    

    useEffect(() => {
        if (paymentGroupMembers) {
            setData(handleDatesArray(paymentGroupMembers));
        }
    }, [paymentGroupMembers]);

    const handleCancelForm = useCallback(() => {
        cancelEdit();
    }, [dispatch, cancelEdit]);

    useEffect(() => {
        if (editMode === 2 && selectedPaymentGroup) {
            getMembers(selectedPaymentGroup?.paymentGroupId)
        }
    }, [editMode, selectedPaymentGroup])


    const handleSubmit = async (data: any) => {
        console.log(data)
        try {
            const newGroup = await createGroup(data).unwrap()
            toast.success("Payment group created successfully")
            handleCancelForm()
        } catch (e) {
            console.error(e)
            toast.error("Something went wrong")
        }
    }

    const onPaymentGroupTypeChange = (e) => {
        console.log(e)
        const description = paymentGroupTypes.find(p => p.paymentGroupTypeId === e.value).description
        const date = new Date();
        const formattedDate = date.toISOString().slice(0, 19).replace('T', ' ') + '.' + date.getMilliseconds().toString().padEnd(2, '0');
        formRef.current.onChange("paymentGroupName", { value: `${description} for ${formattedDate}` })
    }

    function handleSelectPaymentGroup(paymentId: string) {
          const selectedPayment = data.find(
            (paymentGroup: any) => paymentGroup.paymentId === paymentId
          );
          dispatch(setSelectedPaymentGroupMember(selectedPayment));
          dispatch(setPaymentGroupMemberFormEditMode(2))
          router.navigate("/paymentGroups/payments")
        }

    const PaymentDescriptionCell = (props: any) => {
        const field = props.field || "";
        const value = props.dataItem[field];
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{ ...props.style, color: "blue" }}
                colSpan={props.colSpan}
                role={"gridcell"}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{
                    [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex,
                }}
                {...navigationAttributes}
            >
                <Button
                    onClick={() => {
                        handleSelectPaymentGroup(
                            props.dataItem.paymentId
                        );
                    }}
                >
                    {props.dataItem.paymentId}
                </Button>
            </td>
        );
    }


    return (
        <>
            <AccountingMenu selectedMenuItem="/paymentGroups" />
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                {editMode > 1 && <PaymentGroupMenu selectedMenuItem="/paymentGroups/overview" />}
                <Typography sx={{ p: 2 }} color={editMode > 1 ? "black" : "green"} variant="h4">
                    {editMode === 1
                        ? "New Payment Group"
                        : `Payment Group: ${selectedPaymentGroup?.paymentGroupId}`}
                </Typography>
                <Form
                    ref={formRef}
                    onSubmit={handleSubmit}
                    initialValues={selectedPaymentGroup ? { ...selectedPaymentGroup } : null}
                    render={(formRenderProps: FormRenderProps) => {
                        return (
                            <FormElement>
                                <fieldset className="k-form-fieldset">
                                    <Grid container spacing={2} padding={2}>
                                        <Grid item xs={12}>
                                            <Grid container spacing={2} alignItems="flex-end">
                                                <Grid item xs={4}>
                                                    <Field
                                                        id="paymentGroupTypeId"
                                                        name="paymentGroupTypeId"
                                                        label={getTranslatedLabel(
                                                            `${localizationKey}.paymentGroupType`,
                                                            "Payment Group Type *"
                                                        )}
                                                        component={MemoizedFormDropDownList}
                                                        data={paymentGroupTypes ?? []}
                                                        dataItemKey="paymentGroupTypeId"
                                                        textField="description"
                                                        onChange={onPaymentGroupTypeChange}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>
                                                <Grid item xs={4}>
                                                    <Field
                                                        id="paymentGroupName"
                                                        name="paymentGroupName"
                                                        label={getTranslatedLabel(
                                                            `${localizationKey}.paymentGroupName`,
                                                            "Payment Group Name *"
                                                        )}
                                                        component={FormInput}
                                                        validator={requiredValidator}
                                                    />

                                                </Grid>
                                                {/* Added FinAccountName as text-only display when available */}
                                                {selectedPaymentGroup?.finAccountName && (
                                                    <Grid item xs={4}>
                                                        <Typography variant="body2" color="textSecondary">
                                                            {getTranslatedLabel(
                                                                `${localizationKey}.finAccountName`,
                                                                "Financial Account Name"
                                                            )}
                                                        </Typography>
                                                        <Typography variant="body1">
                                                            {selectedPaymentGroup.finAccountName}
                                                        </Typography>
                                                    </Grid>
                                                )}
                                                {/* REFACTORED: Added OwnerPartyId field as text-only display when available */}
                                                {selectedPaymentGroup?.ownerPartyId && (
                                                    <Grid item xs={4}>
                                                        <Typography variant="body2" color="textSecondary">
                                                            {getTranslatedLabel(
                                                                `${localizationKey}.ownerPartyId`,
                                                                "Owner Party ID"
                                                            )}
                                                        </Typography>
                                                        <Typography variant="body1">
                                                            {selectedPaymentGroup.ownerPartyId}
                                                        </Typography>
                                                    </Grid>
                                                )}
                                            </Grid>
                                        </Grid>

                                        <Grid container spacing={2} justifyContent="space-between">
                                            <Grid item container xs={10} spacing={2}>
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

                                            <Grid item container xs={2} justifyContent="flex-end">
                                                <Grid item>
                                                    <Button
                                                        sx={{ mt: 2 }}
                                                        onClick={() => {
                                                            cancelCheckRun(selectedPaymentGroup?.paymentGroupId)
                                                                .unwrap()
                                                                .then(() => {
                                                                    toast.success("Check run cancelled successfully")
                                                                })
                                                                .catch((e) => {
                                                                    console.error(e)
                                                                    toast.error("Something went wrong")
                                                                })
                                                        }}
                                                        color="error"
                                                        variant="contained"
                                                    >
                                                        {getTranslatedLabel(`${localizationKey}.cancel`, "Cancel Check Run")}
                                                    </Button>
                                                </Grid>
                                            </Grid>
                                        </Grid>

                                    </Grid>
                                </fieldset>
                            </FormElement>
                        );
                    }}
                />

                

                {data && data.length > 0 && (
                    <KendoGrid
                        style={{ height: "35vh", flex: 1 }}
                        data={data ?? []}
                        resizable={true}
                        filterable={true}
                        sortable={true}
                        pageable={true}
                    >

                        <Column
                            field="paymentId"
                            title={getTranslatedLabel(`${localizationKeyList}.paymentId`, "Payment Number")}
                            cell={PaymentDescriptionCell}
                            locked={true}
                        />
                        <Column
                            field="fromDate"
                            format="{0: dd/MM/yyyy}"
                            title={getTranslatedLabel(`${localizationKeyList}.fromDate`, "From Date")}
                        />
                        <Column
                            field="thruDate"
                            format="{0: dd/MM/yyyy}"
                            title={getTranslatedLabel(`${localizationKeyList}.thruDate`, "Thru Date")}
                        />

                    </KendoGrid>
                )}
            </Paper>
        </>
    );
}