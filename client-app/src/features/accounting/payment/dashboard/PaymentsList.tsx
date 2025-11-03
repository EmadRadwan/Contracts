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
  GridDataStateChangeEvent, GridToolbar,
} from "@progress/kendo-react-grid";
import { DataResult, State } from "@progress/kendo-data-query";
import Button from "@mui/material/Button";
import { Grid, Paper } from "@mui/material";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { handleDatesArray } from "../../../../app/util/utils";
import PaymentForm from "../form/PaymentForm";
import {resetForm, setFormEditMode, setPaymentType} from "../slice/paymentsUiSlice";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {useLocation, useNavigate} from "react-router";
import {setSelectedPayment} from "../../slice/accountingSharedUiSlice";

interface PaymentsListProps {
  paymentType: 'incoming' | 'outgoing';
}

export default function PaymentsList({ paymentType }: PaymentsListProps) {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.payments.list";
  const location = useLocation()
  const navigate = useNavigate();
  const dispatch = useAppDispatch();

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

  const formEditMode = useAppSelector((s) => s.paymentsUi.formEditMode);



  useEffect(() => {
    if (location.state?.resetPaymentForm) {
      dispatch(resetForm());
      // clean location state
      navigate(location.pathname, { replace: true, state: {} });
    }
  }, [location.state, dispatch, navigate]);


  const dataStateChange = (e: GridDataStateChangeEvent) => {
    setDataState(e.dataState);
  };

  const { data, error, isFetching, refetch } = useFetchPaymentsQuery({
    ...dataState,
    paymentType
  });
  
  const [show, setShow] = useState(false);


  React.useEffect(() => {
    if (data) {
      const adjustedData = handleDatesArray(data.data);
      setPayments({ data: adjustedData, total: data.total });
    }
  }, [data]);

  const handleSelectPayment = (paymentId: string) => {
    const pay = payments.data.find((p: any) => p.paymentId === paymentId);
    if (!pay) return;

    dispatch(setSelectedPayment(pay));
    dispatch(setPaymentType(paymentType === "incoming" ? 1 : 2));

    // Fix: Use statusId instead of statusDescriptionEnglish for mapping
    const statusMap: Record<string, number> = {
      "PMNT_NOT_PAID": 2,
      "PMNT_RECEIVED": 3,
      "PMNT_SENT": 4,
      "PMNT_CONFIRMED": 5,
      "PMNT_CANCELLED": 6,
    };
    const mode = statusMap[pay.statusId] ?? 2; // Use statusId, not statusDescriptionEnglish
    dispatch(setFormEditMode(mode));
  };

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
                    props.dataItem.paymentId
                );
              }}
          >
            {props.dataItem.paymentId}
          </Button>
        </td>
    );
  };

  
  const handleNewPayment = () => {
    dispatch(setPaymentType(paymentType === "incoming" ? 1 : 2));
    dispatch(setSelectedPayment(undefined));
    dispatch(setFormEditMode(1)); // new form
  };


  if (formEditMode > 0) {
    return (
        <PaymentForm
            editMode={formEditMode}
            cancelEdit={() => dispatch(resetForm())}
        />
    );
  }




  return (
    <>
      <AccountingMenu selectedMenuItem={"/payments"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          
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
                <GridToolbar>
                  <Button
                      variant="contained"
                      color="primary"
                      onClick={handleNewPayment}
                  >
                    {getTranslatedLabel(
                        `${localizationKey}.actions.${paymentType}`, // Dynamically use 'incoming' or 'outgoing'
                        `New ${paymentType === 'incoming' ? 'Incoming' : 'Outgoing'} Payment` // Fallback
                    )}
                  </Button>
                </GridToolbar>
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
                  field="orderId"
                  title={getTranslatedLabel(`${localizationKey}.orderId`,"orderId")}
                  width={150}
                />
                <Column
                  field="certificateNumber"
                  title={getTranslatedLabel(`${localizationKey}.certificateNumber`,"certificateNumber")}
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
