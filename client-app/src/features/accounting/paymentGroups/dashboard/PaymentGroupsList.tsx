import {
  useAppDispatch,
  useAppSelector,
  useFetchPaymentGroupsQuery
} from "../../../../app/store/configureStore";
import React, { useEffect, useState } from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import { DataResult, State } from "@progress/kendo-data-query";
import { Payment } from "../../../../app/models/accounting/payment";
import Button from "@mui/material/Button";
import { Grid, Paper } from "@mui/material";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { handleDatesArray } from "../../../../app/util/utils";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useLocation } from "react-router";
import PaymentGroupForm from "../form/PaymentGroupForm";
import { setSelectedPaymentGroup } from "../../slice/accountingSharedUiSlice";
import { PaymentGroup } from "../../../../app/models/accounting/paymentGroup";

const PaymentGroupsList = () => {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.pay-group.list";
  const location = useLocation()
  const [paymentGroups, setPayments] = React.useState<DataResult>({
    data: [],
    total: 0,
  });
  const [dataState, setDataState] = React.useState<State>({
    skip: 0,
    take: 6,
  });

  const dataStateChange = (e: GridDataStateChangeEvent) => {
    setDataState(e.dataState);
  };
  const { data, isFetching } = useFetchPaymentGroupsQuery({ ...dataState });


  const dispatch = useAppDispatch();

  const [editMode, setEditMode] = useState(0);
  const {selectedPaymentGroup} = useAppSelector(state => state.accountingSharedUi)
  React.useEffect(() => {
    if (data) {
      const adjustedData = handleDatesArray(data.data);
      console.log(adjustedData)
      setPayments({ data: adjustedData, total: data.total });
    }
  }, [data]);

  function handleSelectPaymentGroup(paymentGroupId: string) {
    const selectedPaymentGroup: PaymentGroup | undefined = paymentGroups.data.find(
      (paymentGroup: any) => paymentGroup.paymentGroupId === paymentGroupId
    );
    dispatch(setSelectedPaymentGroup(selectedPaymentGroup));
    setEditMode(2)
  }
  useEffect(() => {
    if (selectedPaymentGroup) {
      setEditMode(2)
    }
  }, [])

  const setFormEditModeFromStatus = (paymentStatus: string) => {
    if (paymentStatus === "Not Paid") {
      setEditMode(2);
    }

    if (paymentStatus === "Received") {
      setEditMode(3);
    }

    if (paymentStatus === "Sent") {
      setEditMode(4);
    }

    if (paymentStatus === "Confirmed") {
      setEditMode(5);
    }

    if (paymentStatus === "Cancelled") {
      setEditMode(6);
    }
  }

  function cancelEdit() {
    setEditMode(0);
    dispatch(setSelectedPaymentGroup(undefined))
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
              props.dataItem.paymentGroupId
            );
          }}
        >
          {props.dataItem.paymentGroupId}
        </Button>
      </td>
    );
  };

  if (editMode > 0) {
    return (
      <PaymentGroupForm
        cancelEdit={cancelEdit}
        editMode={editMode}
      />
    );
  }
  return (
    <>
      <AccountingMenu selectedMenuItem={"/paymentGroups"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          {/* <Grid item xs={4}>
            <Menu onSelect={handleMenuSelect}>
              <MenuItem
                text={getTranslatedLabel(
                  `${localizationKey}.actions.new`,
                  "New Payment"
                )}
              />
            </Menu>
          </Grid> */}
          <Grid item xs={12}>
            <div className="div-container">
              <KendoGrid
                style={{ height: "65vh", flex: 1 }}
                data={paymentGroups ? paymentGroups : { data: [], total: 0 }}
                resizable={true}
                filterable={true}
                sortable={true}
                pageable={true}
                {...dataState}
                onDataStateChange={dataStateChange}
              >
                <GridToolbar>
                  <Grid container>
                    <Grid item xs={4}>
                      <Button
                        color={"secondary"}
                        variant="outlined"
                        onClick={() => setEditMode(1)}
                      >
                        {getTranslatedLabel(`${localizationKey}.actions.new`, "New Payment Group")}
                      </Button>
                    </Grid>
                  </Grid>
                </GridToolbar>
                <Column
                  title={getTranslatedLabel(`${localizationKey}.paymentGroupId`,"Payment Group Number")}
                  cell={PaymentDescriptionCell}
                  locked={true}
                />
                <Column
                  field="paymentGroupName"
                  title={getTranslatedLabel(`${localizationKey}.paymentGroupName`,"Payment Group Name")}
                />
                <Column
                  field="paymentGroupTypeDescription"
                  title={getTranslatedLabel(`${localizationKey}.paymentGroupType`,"Payment Group Type")}
                />
                
              </KendoGrid>
              {isFetching && <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`,"Loading Payment Groups...")} />}
            </div>
          </Grid>
        </Grid>
      </Paper>
    </>
  );
}


export default PaymentGroupsList