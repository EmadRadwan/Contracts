import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { useState } from "react";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import {
  useAppDispatch,
  useAppSelector,
} from "../../../../app/store/configureStore";
import Button from "@mui/material/Button";
import { Grid, Paper, Typography } from "@mui/material";
import CreatePaymentApplicationForm from "../form/CreatePaymentApplicationForm";
import { useFetchPaymentApplicationsForPaymentQuery } from "../../../../app/store/apis";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import AssignPaymentApplicationForm from "../form/AssignPaymentApplicationForm";

interface Props {
  paymentId: string;
  onClose: () => void;
}

export default function PaymentApplicationsList({ paymentId, onClose }: Props) {
  const initialSort: Array<SortDescriptor> = [
    { field: "paymentId", dir: "desc" },
  ];
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 4 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };
  const { data: paymentApplications } =
    useFetchPaymentApplicationsForPaymentQuery(paymentId!);
  const [show, setShow] = useState(false);

  const [editMode, setEditMode] = useState(0);

  function cancelEdit() {
    setEditMode(0);
  }

  

  return (
    <>
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid
          container
          columnSpacing={1}
          direction={"column"}
          justifyContent={"center"}
        >
          <Grid item xs={8}>
            <Typography variant="h4" ml={2} mb={4}>
              {`Payment applications for payment: ${paymentId}`}
            </Typography>
          </Grid>
          {paymentApplications && paymentApplications!.length > 0 ? (
            <Grid item>
              <div className="div-container">
                <KendoGrid
                  // className="main-grid"
                  style={{ height: "45vh", width: "850px" }}
                  data={paymentApplications ?? []}
                  sortable={true}
                  sort={sort}
                  onSortChange={(e: GridSortChangeEvent) => {
                    setSort(e.sort);
                  }}
                  skip={page.skip}
                  take={page.take}
                  total={paymentApplications ? paymentApplications!.length : 0}
                  pageable={true}
                  onPageChange={pageChange}
                >
                  <Column field="paymentId" title="Payment Id" locked />
                  <Column field="invoiceId" title="Invoice Id" />
                  <Column field="invoiceItemSeqId" title="Invoice Item Id" />
                  <Column field="amountApplied" title="Amount" />
                </KendoGrid>
              </div>
            </Grid>
          ) : (
            <AssignPaymentApplicationForm paymentId={paymentId} />
          )}
          <Grid item xs={2} alignSelf={"end"}>
            <Button onClick={() => onClose()} color="error" variant="contained">
              Close
            </Button>
          </Grid>
        </Grid>
      </Paper>
    </>
  );
}
