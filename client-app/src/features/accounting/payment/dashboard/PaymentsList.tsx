import {
  useAppDispatch,
  useAppSelector,
  useFetchPaymentsQuery,
} from "../../../../app/store/configureStore";
import React, { useEffect, useState } from "react";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
} from "@progress/kendo-react-grid";
import { DataResult, State } from "@progress/kendo-data-query";
import { Payment } from "../../../../app/models/accounting/payment";
import Button from "@mui/material/Button";
import { Grid, Paper } from "@mui/material";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import { Menu, MenuItem, MenuSelectEvent } from "@progress/kendo-react-layout";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { handleDatesArray } from "../../../../app/util/utils";
import PaymentForm from "../form/PaymentForm";
import { setPaymentType } from "../slice/paymentsUiSlice";
import {useCalculatePaymentTotalsMutation} from "../../../../app/store/apis";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { useLocation } from "react-router";

export default function PaymentsList() {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.payments.list";
  const location = useLocation()
  const [payments, setPayments] = React.useState<DataResult>({
    data: [],
    total: 0,
  });
  const [dataState, setDataState] = React.useState<State>({
    sort: [
      {
        field: "effectiveDate",
        dir: "desc",
      },
    ],
    skip: 0,
    take: 6,
  });

  const dataStateChange = (e: GridDataStateChangeEvent) => {
    setDataState(e.dataState);
  };
  const selectedPaymentFromUi = useAppSelector(state => state.accountingSharedUi.selectedPayment)
  const { data, error, isFetching, refetch } = useFetchPaymentsQuery({ ...dataState });

  const [show, setShow] = useState(false);

  const dispatch = useAppDispatch();

  const [editMode, setEditMode] = useState(0);
  const [payment, setPayment] = useState<Payment | undefined>(undefined);
  React.useEffect(() => {
    if (data) {
      const adjustedData = handleDatesArray(data.data);
      setPayments({ data: adjustedData, total: data.total });
    }
  }, [data]);

  function handleSelectPayment(paymentId: string) {
    const selectedPayment: Payment | undefined = payments.data.find(
      (payment: any) => payment.paymentId === paymentId
    );
    setPayment(selectedPayment);

    // based on paymentTypeId set paymentType to 1 for incoming payment
    // and 2 for outgoing payment in redux store
    if (
      selectedPayment?.paymentTypeId === "CUSTOMER_DEPOSIT" ||
      selectedPayment?.paymentTypeId === "CUSTOMER_PAYMENT"
    ) {
      dispatch(setPaymentType(1));
    } else {
      dispatch(setPaymentType(2));
    }
    setFormEditModeFromStatus(selectedPayment?.statusDescriptionEnglish)
    
  }


  useEffect(() => {
    if ((selectedPaymentFromUi || location?.state?.selectedPaymentId) && payments.data.length > 0) {
      const id = location?.state?.selectedPaymentId ?? selectedPaymentFromUi?.paymentId
      handleSelectPayment(id)
    }
  }, [selectedPaymentFromUi, payments, location?.state?.selectedPaymentId])

  useEffect(() => {
    console.log(editMode)
  }, [editMode])

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
            handleSelectPayment(
              props.dataItem.paymentId,
              props.dataItem.statusDescription
            );
          }}
        >
          {props.dataItem.paymentId}
        </Button>
      </td>
    );
  };

  function handleMenuSelect(e: MenuSelectEvent) {
    if (e.item.data === "new") return
    if (e.item.data === "incoming") {
      dispatch(setPaymentType(1));
    } else {
      dispatch(setPaymentType(2));
    }
    setPayment(undefined);
    setEditMode(1);
  }

  if (editMode > 0) {
    return (
      <PaymentForm
        selectedPayment={payment}
        cancelEdit={cancelEdit}
        editMode={editMode}
      />
    );
  }

  const updatePayment = (paymentId: string, updates: { amountToApply?: number }) => {
    setPayments((prevState) => {
        const updatedData = prevState.data.map((pay: Payment) => {
            if (pay.paymentId === paymentId) {
                return { ...pay, ...updates };
            }
            return pay;
        });
        return { ...prevState, data: updatedData };
    });
};

const GetSummaryCell = (props: any) => {
    const { dataItem, updatePayment } = props;
    const [trigger, { isLoading }] = useCalculatePaymentTotalsMutation();

    const handleGetSummary = async () => {
        try {
            const result = await trigger([dataItem.paymentId]).unwrap();
            if (result && result.length > 0) {
                const { amountToApply } = result[0];
                updatePayment(dataItem.paymentId, { amountToApply });
            }
        } catch (error) {
            console.error("Error calculating payment totals:", error);
        }
    };

    return (
        <td className="k-command-cell">
            <Button
                className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base"
                onClick={handleGetSummary}
                variant="contained"
                color="primary"
                disabled={isLoading}
            >
                {isLoading ? "Loading..." : "Get Summary"}
            </Button>
        </td>
    );
};

  return (
    <>
      <AccountingMenu selectedMenuItem={"/payments"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item xs={4}>
            <Menu onSelect={handleMenuSelect}>
              <MenuItem
                text={getTranslatedLabel(
                  `${localizationKey}.actions.new`,
                  "New Payment"
                )}
                data="new"
              >
                <MenuItem
                  text={getTranslatedLabel(
                    `${localizationKey}.actions.incoming`,
                    "Incoming Payment"
                  )}
                  data={"incoming"}
                />
                <MenuItem
                  text={getTranslatedLabel(
                    `${localizationKey}.actions.outgoing`,
                    "Outgoing Payment"
                  )}
                  data={"outgoing"}
                />
              </MenuItem>
            </Menu>
          </Grid>
          <Grid item xs={12}>
            <div className="div-container">
              <KendoGrid
                style={{ height: "65vh", flex: 1 }}
                data={payments ? payments : { data: [], total: 0 }}
                resizable={true}
                filterable={true}
                sortable={true}
                pageable={true}
                {...dataState}
                onDataStateChange={dataStateChange}
              >
                <Column
                  field="paymentId"
                  title={getTranslatedLabel(`${localizationKey}.paymentId`,"Payment Number")}
                  cell={PaymentDescriptionCell}
                  width={150}
                  locked={!show}
                />
                <Column
                  field="paymentTypeDescription"
                  title={getTranslatedLabel(`${localizationKey}.paymentType`,"Payment Type")}
                  width={150}
                />
                <Column
                  field="partyIdFromName"
                  title={getTranslatedLabel(`${localizationKey}.from`,"From Party")}
                  width={150}
                />
                <Column field="partyIdToName" title={getTranslatedLabel(`${localizationKey}.to`,"To Party")} width={150} />
                <Column
                  field="effectiveDate"
                  title={getTranslatedLabel(`${localizationKey}.date`,"Payment Date")}
                  width={150}
                  format="{0: dd/MM/yyyy}"
                />
                <Column field="statusDescription" title={getTranslatedLabel(`${localizationKey}.status`,"Status")} width={100} />
                <Column field="amount" title={getTranslatedLabel(`${localizationKey}.amount`,"Amount")} width={130} />
                <Column field="comments" title={getTranslatedLabel(`${localizationKey}.comments`,"Comments")} width={150} />
              </KendoGrid>
              {isFetching && <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`,"Loading Payments...")} />}
            </div>
          </Grid>
        </Grid>
      </Paper>
    </>
  );
}
