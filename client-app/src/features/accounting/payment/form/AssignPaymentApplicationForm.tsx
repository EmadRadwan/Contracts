import { Button, Grid, Typography } from "@mui/material";
import React, { useEffect, useState } from "react";
import {
  Grid as KendoGrid,
  GridColumn as Column,
} from "@progress/kendo-react-grid";
import AssignPaymentForm from "./AssignPaymentForm";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import ApplyPaymentForm from "./ApplyPaymentForm";
import { useFetchNotListedInvoicesQuery } from "../../../../app/store/configureStore";
import { handleDatesArray } from "../../../../app/util/utils";

interface Props {
  paymentId: string;
}

const AssignPaymentApplicationForm = ({ paymentId }: Props) => {
  const [selectedPossibleApplication, setSelectedPossibleApplication] =
    useState(null);
  const [possibleInvoices, setPossibleInvoices] = useState([])
  const [showApplyForm, setShowApplyForm] = useState(false)

  const {data: unlistedInvoices} = useFetchNotListedInvoicesQuery(paymentId)
  console.log(unlistedInvoices)

  useEffect(() => {
    if (unlistedInvoices) {
      let updatedInvoices = handleDatesArray(unlistedInvoices)!
      setPossibleInvoices(updatedInvoices)
    }
  }, [unlistedInvoices]);

  const handleSelectPaymentapplication = (dataItem: any) => {
    // find the payment application selected to be applied and open up a modal with the payment id and invoice id disabled and a field to enter the amount
    // and maybe another button to apply all (apply the total amount of the payment)
    let selectedInvoice = possibleInvoices?.find((invoice: any) => invoice.invoiceId === dataItem.invoiceId)!
    setSelectedPossibleApplication(selectedInvoice);
    setShowApplyForm(true)
  };

  const handleSelectApplyAll = (dataItem: any) => {};

  const handleAssignPayment = (values: any) => {
    //TODO: send mutation with applied amount and other needed data
    console.log(values);
  };

  const PaymentApplicationCell = (props: any) => {
    return (
      <td>
        <Button onClick={() => handleSelectPaymentapplication(props.dataItem)}>
          {props.dataItem.invoiceId}
        </Button>
      </td>
    );
  };
  const ApplyAllCell = (props: any) => {
    return (
      <td>
        <Button onClick={() => handleSelectApplyAll(props.dataItem)}>
          Apply All
        </Button>
      </td>
    );
  };
  return (
    <>
    {showApplyForm && (
      <ModalContainer show={showApplyForm} onClose={() => setShowApplyForm(false)}>
        <ApplyPaymentForm paymentId={paymentId} selectedApplication={selectedPossibleApplication} onClose={() => setShowApplyForm(false)}/>
      </ModalContainer>
    )}
      <Typography variant="h5" m={3}>
        No applications found...
      </Typography>
      <Grid item>
        <div className="div-container">
          <Typography variant="h6" m={1}>
            Possible invoices to apply
          </Typography>
          <KendoGrid
            className="main-grid"
            style={{ height: "auto", width: "65vw" }}
            data={possibleInvoices ?? []}
            skip={0}
            take={4}
            total={possibleInvoices?.length ?? 0}
          >
            <Column
              field="invoiceId"
              title="Invoice Id"
              cell={PaymentApplicationCell}
            />
            <Column
              field="invoiceDate"
              title="Invoice Date"
              format="{0: dd/MM/yyyy}"
            />
            <Column field="amount" title="Invoice Amount" />
            <Column field="currencyUomId" title="Currency" />
            <Column field="amountApplied" title="Amount Applied" />
            <Column cell={ApplyAllCell} />
          </KendoGrid>
        </div>
        <AssignPaymentForm
          onSubmit={(values: any) => handleAssignPayment(values)}
        />
      </Grid>
    </>
  );
};

export default AssignPaymentApplicationForm;
