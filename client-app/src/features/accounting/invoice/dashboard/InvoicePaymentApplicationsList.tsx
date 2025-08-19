import React, { useCallback, useState } from "react";
import { Button, Grid } from "@mui/material";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridToolbar,
  GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import { State } from "@progress/kendo-data-query";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import InvoicePaymentApplicationForm from "../../payment/form/InvoicePaymentApplicationForm";
import { useAppSelector, useFetchInvoicePaymentApplicationsQuery } from "../../../../app/store/configureStore";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { router } from "../../../../app/router/Routes";

interface Props {
  onClose: () => void;
  canEdit: boolean;
}

const InvoicePaymentApplicationsList = ({ onClose, canEdit }: Props) => {
  const initialDataState = { take: 6, skip: 0 };
  const [dataState, setDataState] = React.useState<State>(initialDataState);
  const dataStateChange = (e: GridDataStateChangeEvent) => {
    console.log("dataStateChange", e.dataState);
    setDataState(e.dataState);
  };
  const {selectedInvoice} = useAppSelector(state => state.accountingSharedUi)
  const {data: paymentApplications} = useFetchInvoicePaymentApplicationsQuery(selectedInvoice?.invoiceId!)
  console.log(paymentApplications)
  const [editMode, setEditMode] = useState(0);
  const memoizedOnClose = useCallback(() => {
    setEditMode(0);
  }, []);

  const handleSelectPayment = (paymentId: string) => {
    router.navigate("/payments", {state: {selectedPaymentId: paymentId}})
  }

  const PaymentDescriptionCell = (props: any) => {
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

  if (editMode === 1) {
    return <InvoicePaymentApplicationForm onClose={memoizedOnClose} />;
  }
  return (
      <Grid container columnSpacing={1} alignItems="center">
        <Grid item xs={12}>
          <div className="div-container">
            <KendoGrid
              style={{  flex: 1 }}
              data={paymentApplications ? paymentApplications : []}
              resizable={true}
              filterable={true}
              sortable={true}
              pageable={true}
              {...dataState}
              onDataStateChange={dataStateChange}
            >
              <GridToolbar>
                <Grid container>
                  <Grid item xs={3}>
                    <Button
                      color={"secondary"}
                      onClick={() => {
                        setEditMode(1);
                      }}
                      variant="outlined"
                      disabled={!canEdit}
                    >
                      Assign Payment to Invoice
                    </Button>
                  </Grid>
                </Grid>
              </GridToolbar>
              <Column
                field="paymentId"
                title="Payment Number"
                cell={PaymentDescriptionCell}
                locked={true}
              />
              <Column
                field="amountApplied"
                title="Applied Amount"
              />
            </KendoGrid>

            {false && <LoadingComponent message="Loading Payments..." />}
          </div>
        </Grid>
        <Grid item xs={3} p={1}>
          <Button variant="contained" color="error" onClick={onClose}>
            Back
          </Button>
        </Grid>
      </Grid>
  );
};

export default InvoicePaymentApplicationsList;
