import {
  RootState,
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
import {Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Grid, Paper} from "@mui/material";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { handleDatesArray } from "../../../../app/util/utils";
import PaymentForm from "../form/PaymentForm";
import {resetForm, setFormEditMode, setPaymentType} from "../slice/paymentsUiSlice";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import {useLocation, useNavigate} from "react-router";
import {setSelectedPayment} from "../../slice/accountingSharedUiSlice";
import {useSelector} from "react-redux";
import {PaymentsDailyExcel} from "../report/PaymentsDailyExcel";
import { skipToken } from '@reduxjs/toolkit/query/react';
import {PaymentsDateRangeExcel} from "../report/PaymentsDateRangeExcel";
import {useDeletePaymentMutation} from "../../../../app/store/apis";
import {Can} from "../../../account/Can";
import {toast} from "react-toastify";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import PaymentTransactionsList from "../../transaction/dashboard/PaymentTransactionsList";   // <-- add this import


interface PaymentsListProps {
  paymentType: 'incoming' | 'outgoing';
}

export default function PaymentsList({ paymentType }: PaymentsListProps) {
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "accounting.payments.list";
  const location = useLocation()
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const companyName = useSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);
  const [dailyQueryArg, setDailyQueryArg] = React.useState<typeof skipToken | { paymentType: 'incoming' | 'outgoing' }>(skipToken);
// near other useState declarations
  const [selectedPaymentIdForTransactions, setSelectedPaymentIdForTransactions] = useState<string | null>(null);
  const [payments, setPayments] = React.useState<DataResult>({
    data: [],
    total: 0,
  });
  const isIncoming = paymentType === "incoming";
  const isOutgoing = paymentType === "outgoing";

  const [dataState, setDataState] = React.useState<State>({
    sort: [
      {
        field: "createdStamp",
        dir: "desc",
      },
    ],
    skip: 0,
    take: 25,
  });

  const formEditMode = useAppSelector((s) => s.paymentsUi.formEditMode);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [paymentToDelete, setPaymentToDelete] = useState<string | null>(null);

  const [deletePayment, { isLoading: isDeleting }] = useDeletePaymentMutation();

  const handleDeleteClick = (paymentId: string) => {
    setPaymentToDelete(paymentId);
    setDeleteDialogOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!paymentToDelete) return;

    try {
      await deletePayment(paymentToDelete).unwrap();

      // Success toast
      toast.success(
          getTranslatedLabel("accounting.payments.list.deleteSuccess", "تم حذف الدفعة بنجاح")
      );
    } catch (err: any) {
      // Extract error message from API response
      let errorMessage = getTranslatedLabel("accounting.payments.list.deleteFailed", "فشل حذف الدفعة");

      if (err?.data?.error) {
        errorMessage = err.data.error; // e.g., "لا يمكن حذف الدفعة لأنها مرتبطة بشهادة مشروع رقم: 98-0001"
      } else if (typeof err?.data === "string") {
        errorMessage = err.data;
      }

      // Error toast with Arabic message
      toast.error(errorMessage);

      console.error('Delete payment failed:', err);
    } finally {
      // Always close the dialog, whether success or failure
      setDeleteDialogOpen(false);
      setPaymentToDelete(null);
    }
  };

  const handleCancelDelete = () => {
    setDeleteDialogOpen(false);
    setPaymentToDelete(null);
  };



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

  const handleShowTransactions = (paymentId: string) => {
    // Optional: select the payment in global state (if you want consistency with edit flow)
    const payment = payments.data.find((p: any) => p.paymentId === paymentId);
    if (payment) {
      dispatch(setSelectedPayment(payment));
      dispatch(setPaymentType(paymentType === "incoming" ? 1 : 2));
    }

    setSelectedPaymentIdForTransactions(paymentId);
  };

  const ActionsCell = (props: any) => {
    const navigationAttributes = useTableKeyboardNavigation(props.id);

    return (
        <td
            className={props.className}
            style={{ ...props.style, textAlign: "center" }}
            colSpan={props.colSpan}
            role="gridcell"
            aria-colindex={props.ariaColumnIndex}
            aria-selected={props.isSelected}
            {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
            {...navigationAttributes}
        >
          <div style={{ display: "flex", gap: 8, justifyContent: "center" }}>
            {/* View Transactions Button */}
            <Button
                size="small"
                color="info"
                variant="outlined"
                onClick={() => handleShowTransactions(props.dataItem.paymentId)}
            >
              {getTranslatedLabel("accounting.payments.list.transactions", "Transactions")}
            </Button>
            {/* Delete Button - already existing */}
            <Can perform="deletePayment">
              <Button
                  size="small"
                  color="error"
                  variant="outlined"
                  onClick={() => handleDeleteClick(props.dataItem.paymentId)}
                  disabled={isDeleting}
              >
                {getTranslatedLabel("accounting.payments.list.deleteButton", "Delete")}
              </Button>
            </Can>
          </div>
        </td>
    );
  };

  const gridColumns = [
    // ── Always visible (common) ──
    {
      field: "paymentId",
      title: getTranslatedLabel(`${localizationKey}.paymentId`, "Payment Number"),
      cell: PaymentDescriptionCell,
      width: 150,
      locked: !show,
    },
    {
      field: "paymentTypeDescription",
      title: getTranslatedLabel(`${localizationKey}.paymentType`, "Payment Type"),
      width: 150,
    },
    {
      field: "amount",
      title: getTranslatedLabel(`${localizationKey}.amount`, "Amount"),
      width: 130,
      filter: "numeric",
    },
    {
      field: "paymentMethodTypeDescription",
      title: getTranslatedLabel(`${localizationKey}.paymentMethodTypeDescription`, "Payment Method Type"),
      width: 150,
    },

    // ── Incoming-only columns ──
    ...(isIncoming
        ? [
          {
            field: "buildingNumber",
            title: getTranslatedLabel(`${localizationKey}.buildingNumber`, "Building Number"),
            width: 140,
          },
          {
            field: "productId",
            title: getTranslatedLabel(`${localizationKey}.productId`, "Product ID"),
            width: 130,
          },
          {
            field: "partyIdFromName",
            title: getTranslatedLabel(`${localizationKey}.from`, "From Party"),
            width: 180,
          },
          {
            field: "effectiveDate",
            title: getTranslatedLabel(`${localizationKey}.date`, "Payment Date"),
            width: 150,
            format: "{0: dd/MM/yyyy}",
            filter: "date",
          },
          {
            field: "statusDescription",
            title: getTranslatedLabel(`${localizationKey}.status`, "Status"),
            width: 120,
          },
          {
            field: "comments",
            title: getTranslatedLabel(`${localizationKey}.comments`, "Comments"),
            width: 280,
          },
          {
            field: "projectName",
            title: getTranslatedLabel(`${localizationKey}.projectName`, "Project / Comments"),
            width: 180,
          },
          {
            field: "costCenterDescription",
            title: getTranslatedLabel(`${localizationKey}.costCenterDescription`, "Cost Center"),
            width: 160,
          },
          {
            field: "paymentRefNum",
            title: getTranslatedLabel(`${localizationKey}.paymentRefNum`, "Ref Number"),
            width: 140,
          },
        ]
        : []),

    // ── Outgoing-only columns ──
    ...(isIncoming
        ? [] // nothing extra for incoming beyond what's above
        : [
          {
            field: "orderId",
            title: getTranslatedLabel(`${localizationKey}.orderId`, "Order ID"),
            width: 140,
          },
          {
            field: "certificateNumber",
            title: getTranslatedLabel(`${localizationKey}.certificateNumber`, "Certificate No"),
            width: 150,
          },
          {
            field: "partyIdFromName",
            title: getTranslatedLabel(`${localizationKey}.from`, "From Party"),
            width: 180,
          },
          {
            field: "partyIdToName",
            title: getTranslatedLabel(`${localizationKey}.to`, "To Party"),
            width: 180,
          },
          {
            field: "effectiveDate",
            title: getTranslatedLabel(`${localizationKey}.date`, "Payment Date"),
            width: 150,
            format: "{0: dd/MM/yyyy}",
            filter: "date",
          },
          {
            field: "statusDescription",
            title: getTranslatedLabel(`${localizationKey}.status`, "Status"),
            width: 120,
          },
          {
            field: "paymentRefNum",
            title: getTranslatedLabel(`${localizationKey}.paymentRefNum`, "Ref Number"),
            width: 140,
          },
          {
            field: "projectName",
            title: getTranslatedLabel(`${localizationKey}.projectName`, "Project"),
            width: 180,
          },
          {
            field: "costCenterDescription",
            title: getTranslatedLabel(`${localizationKey}.costCenterDescription`, "Cost Center"),
            width: 160,
          },
          {
            field: "salesRequestId",
            title: getTranslatedLabel(`${localizationKey}.salesRequestId`, "Sales Request ID"),
            width: 150,
          },
          {
            field: "comments",
            title: getTranslatedLabel(`${localizationKey}.comments`, "Comments"),
            width: 280,
          },
        ]),

    // ── Always last ──
    {
      title: getTranslatedLabel(`${localizationKey}.actions`, "Actions"),
      width: 220,
      cell: ActionsCell,
    },
  ];


  return (
    <>
      <AccountingMenu selectedMenuItem={"/payments"} />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          
          <Grid item xs={12}>
            <div className="div-container">
              <KendoGrid
                style={{ flex: 1 }}
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

                  <PaymentsDailyExcel
                      companyName={companyName}
                      paymentType={paymentType}
                      getTranslatedLabel={getTranslatedLabel}
                  />
                  <PaymentsDateRangeExcel
                      companyName={companyName}
                      paymentType={paymentType}
                      getTranslatedLabel={getTranslatedLabel}
                  />
                </GridToolbar>
                {gridColumns.map((col, index) => (
                    <Column key={index} {...col} />
                ))}
              </KendoGrid>
              <Dialog
                  open={deleteDialogOpen}
                  onClose={handleCancelDelete}
                  aria-labelledby="delete-payment-dialog-title"
                  aria-describedby="delete-payment-dialog-description"
              >
                <DialogTitle id="delete-payment-dialog-title">
                  {getTranslatedLabel(
                      "accounting.payments.list.deleteDialogTitle",
                      "Confirm Deletion"
                  )}
                </DialogTitle>
                <DialogContent>
                  <DialogContentText id="delete-payment-dialog-description">
                    {getTranslatedLabel(
                        "accounting.payments.list.deleteDialogMessage",
                        "Are you sure you want to delete payment {0}? This action cannot be undone."
                    ).replace("{0}", paymentToDelete || "")}
                  </DialogContentText>
                </DialogContent>
                <DialogActions>
                  <Button onClick={handleCancelDelete} disabled={isDeleting}>
                    {getTranslatedLabel("global.cancel", "Cancel")}
                  </Button>
                  <Button
                      onClick={handleConfirmDelete}
                      color="error"
                      variant="contained"
                      disabled={isDeleting}
                      autoFocus
                  >
                    {isDeleting
                        ? getTranslatedLabel("global.deleting", "Deleting...")
                        : getTranslatedLabel("accounting.payments.list.deleteConfirm", "Delete")}
                  </Button>
                </DialogActions>
              </Dialog>
              {selectedPaymentIdForTransactions && (
                  <ModalContainer
                      show={!!selectedPaymentIdForTransactions}
                      onClose={() => setSelectedPaymentIdForTransactions(null)}
                      width={950}
                  >
                    <PaymentTransactionsList
                        onClose={() => setSelectedPaymentIdForTransactions(null)}
                        paymentId={selectedPaymentIdForTransactions}
                    />
                  </ModalContainer>
              )}
              {isFetching && <LoadingComponent message={getTranslatedLabel(`${localizationKey}.loading`,"Loading Payments...")} />}
            </div>
          </Grid>
        </Grid>
      </Paper>
    </>
  );
}
