import React, {useState, useEffect} from 'react'
import PaymentGroupMenu from '../menu/PaymentGroupMenu'
import AccountingMenu from '../../invoice/menu/AccountingMenu'
import { Button, Grid, Paper, Typography } from "@mui/material";
import { useAppDispatch, useAppSelector, useExpirePaymentGroupMemberMutation, useFetchPaymentGroupMembersQuery } from '../../../../app/store/configureStore';
import { router } from '../../../../app/router/Routes';
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import PaymentGroupPaymentForm from '../form/PaymentGroupPaymentForm';
import LoadingComponent from '../../../../app/layout/LoadingComponent';
import { handleDatesArray } from '../../../../app/util/utils';
import {toast} from 'react-toastify';
import { setPaymentGroupMemberFormEditMode, setSelectedPaymentGroupMember } from '../../slice/accountingSharedUiSlice';

const PaymentGroupPaymentsList = () => {
  const {selectedPaymentGroup, paymentGroupMemberFormEditMode} = useAppSelector(state => state.accountingSharedUi)
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.pay-group-payments.list";
  if (!selectedPaymentGroup) {
    router.navigate("/paymentGroups")
  }
  const {data: paymentGroupMembers, isLoading} = useFetchPaymentGroupMembersQuery(selectedPaymentGroup?.paymentGroupId)
  const [data, setData] = useState<any[]>([]);
  const dispatch = useAppDispatch();
  const [cancelMember] = useExpirePaymentGroupMemberMutation()
  

  useEffect(() => {
    if (paymentGroupMembers) {
      setData(handleDatesArray(paymentGroupMembers));
    }
    }, [paymentGroupMembers]);

  function handleSelectPaymentGroup(paymentId: string) {
      const selectedPayment = data.find(
        (paymentGroup: any) => paymentGroup.paymentId === paymentId
      );
      dispatch(setSelectedPaymentGroupMember(selectedPayment));
      dispatch(setPaymentGroupMemberFormEditMode(2))
    }

    const handleCancelForm = () => {
      dispatch(setPaymentGroupMemberFormEditMode(0))
      dispatch(setSelectedPaymentGroupMember(undefined));
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

  const CancelPaymentGroupMemberCell = (props: any) => {
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
          color={"error"}
          disabled={!!props.dataItem.thruDate}
          onClick={async () => {
            await cancelMember({
              paymentId: props.dataItem.paymentId,
              paymentGroupId: selectedPaymentGroup?.paymentGroupId,
              fromDate: props.dataItem.fromDate,
            }).unwrap().then(() => {
              toast.success(getTranslatedLabel(`${localizationKey}.cancel.success`,"Payment Group Member cancelled successfully"));
            }).catch((error) => {
              console.log("Error cancelling payment group member", error);
              toast.error(getTranslatedLabel(`${localizationKey}.cancel.error`,"Error cancelling payment group member"));
            });
          }}
        >
          Cancel
        </Button>
      </td>
    );
  }
  if (paymentGroupMemberFormEditMode > 0) {
    return <PaymentGroupPaymentForm cancelEdit={handleCancelForm} editMode={paymentGroupMemberFormEditMode} />

  }
  return (
    <>
    {isLoading && <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`,"Loading Payment Group Payments")}/>}
      <AccountingMenu selectedMenuItem="/paymentGroups" />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <PaymentGroupMenu selectedMenuItem="/paymentGroup/payments" />
        <Typography sx={{ p: 2 }} color={"black"} variant="h4">
          {`Payments for Payment Group: ${selectedPaymentGroup.paymentGroupId}`}
        </Typography>

        <KendoGrid
          style={{ height: "65vh", flex: 1 }}
          data={data ?? []}
          resizable={true}
          filterable={true}
          sortable={true}
          pageable={true}
        >
          <GridToolbar>
            <Grid container>
              <Grid item xs={4}>
                <Button
                  color={"secondary"}
                  variant="outlined"
                  onClick={() => dispatch(setPaymentGroupMemberFormEditMode(1))}
                >
                  {getTranslatedLabel(`${localizationKey}.actions.new`, "New Payment Group Member")}
                </Button>
              </Grid>
            </Grid>
          </GridToolbar>
          
          <Column
            field="paymentId"
            title={getTranslatedLabel(`${localizationKey}.paymentId`,"Payment Number")}
            cell={PaymentDescriptionCell}
            locked={true}
          />
          <Column
            field="fromDate"
            format="{0: dd/MM/yyyy}"
            title={getTranslatedLabel(`${localizationKey}.fromDate`,"From Date")}
          />
          <Column
            field="thruDate"
            format="{0: dd/MM/yyyy}"
            title={getTranslatedLabel(`${localizationKey}.thruDate`,"Thru Date")}
          />
          <Column
            cell={CancelPaymentGroupMemberCell}
          />
        
        </KendoGrid>
      </Paper>
      
    </>
   
  )
}

export default PaymentGroupPaymentsList