import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { Fragment, useCallback, useState } from "react";
import {
  Grid as KendoGrid,
  GridCellProps,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import { Grid } from "@mui/material";
import { OrderAdjustment } from "../../../../app/models/order/orderAdjustment";
import {
  useAppDispatch,
  useAppSelector,
} from "../../../../app/store/configureStore";
import { useSelector } from "react-redux";
import OrderAdjustmentForm from "../../form/order/OrderAdjustmentForm";
import {
  orderLevelAdjustments,
} from "../../slice/orderSelectors";
import { setUiOrderAdjustments } from "../../slice/orderAdjustmentsUiSlice";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

interface Props {
  onClose: () => void;
}

export default function OrderAdjustmentsList({ onClose }: Props) {
  const initialSort: Array<SortDescriptor> = [
    { field: "partyId", dir: "desc" },
  ];
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 4 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };

  const [show, setShow] = useState(false);
  const [orderAdjustment, setOrderAdjusment] = useState<
    OrderAdjustment | undefined
  >(undefined);
  const uiOrderAdjustments: any = useSelector(orderLevelAdjustments);

  const orderFormEditMode: any = useAppSelector(
    (state) => state.ordersUi.orderFormEditMode
  );

  const dispatch = useAppDispatch();
  const { getTranslatedLabel } = useTranslationHelper();
  const localizationKey = "order.so.adj.list";

  const [editMode, setEditMode] = useState(0);
  const { user } = useAppSelector((state) => state.account);
/*  const roleWithPercentage = (user!.roles || []).find(
    (role) => role.Name === "AddAdjustments"
  );*/

 


  function handleSelectOrderAdjustment(orderAdjustmentId: string) {
    // select order adjustment from orderAdjustments list
    const orderAdjustment = uiOrderAdjustments!.find(
      (adjustment: OrderAdjustment) =>
        adjustment.orderAdjustmentId === orderAdjustmentId
    );

    setOrderAdjusment(orderAdjustment);

    setEditMode(2);
    setShow(true);
  }

  const DeleteOrderItemAdjustmentCell = (props: any) => {
    const { dataItem } = props;

    return (
      <td className="k-command-cell">
        <Button
          className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
          onClick={() => props.remove(dataItem)}
          disabled={dataItem.isManual === "N" || orderFormEditMode > 1}
          color="error"
        >
          {getTranslatedLabel("general.remove", "Remove")}
        </Button>
      </td>
    );
  };
  const remove = (dataItem: OrderAdjustment) => {
    const newNonDeletedOrderAdjustments = uiOrderAdjustments?.map(
      (item: OrderAdjustment) => {
        if (item.orderAdjustmentId === dataItem?.orderAdjustmentId) {
          return { ...item, isAdjustmentDeleted: true };
        } else {
          return item;
        }
      }
    );
    dispatch(setUiOrderAdjustments(newNonDeletedOrderAdjustments));
  };

  const CommandCell = (props: GridCellProps) => (
    <DeleteOrderItemAdjustmentCell {...props} remove={remove} />
  );

  const orderAdjustmentCell = (props: any) => {
    return props.dataItem.isManual === "Y" ? (
      <td>
        <Button
          onClick={() =>
            handleSelectOrderAdjustment(props.dataItem.orderAdjustmentId)
          }
        >
          {props.dataItem.orderAdjustmentTypeDescription}
        </Button>
      </td>
    ) : (
      <td>{props.dataItem.orderAdjustmentTypeDescription}</td>
    );
  };

  const memoizedOnClose = useCallback(() => {
    setShow(false);
  }, []);

  ////console.log('orderItems', orderAdjustments);
  ////console.log('nonDeletedOrderAdjustments', nonDeletedOrderAdjustments);

  return (
    <Fragment>
      {show && (
        <ModalContainer show={show} onClose={memoizedOnClose} width={500}>
          <OrderAdjustmentForm
            orderAdjustment={orderAdjustment}
            editMode={editMode}
            onClose={memoizedOnClose}
          />
        </ModalContainer>
      )}
      <Grid container padding={2} columnSpacing={1}>
        <Grid container alignItems="center">
          <Grid item xs={6}>
              <Button
                disabled={orderFormEditMode > 2}
                color={"secondary"}
                onClick={() => {
                  setEditMode(1);
                  setShow(true);
                }}
                variant="outlined"
              >
                {getTranslatedLabel(
                  `${localizationKey}.add`,
                  "Add Order Adjustment"
                )}
              </Button>
            
          </Grid>
        </Grid>
        <Grid container>
          <div className="div-container">
            <KendoGrid
              className="main-grid"
              style={{ height: "300px" }}
              data={orderBy(
                uiOrderAdjustments ? uiOrderAdjustments : [],
                sort
              ).slice(page.skip, page.take + page.skip)}
              sortable={true}
              sort={sort}
              onSortChange={(e: GridSortChangeEvent) => {
                setSort(e.sort);
              }}
              skip={page.skip}
              take={page.take}
              total={
                uiOrderAdjustments ? uiOrderAdjustments.length : 0
              }
              pageable={true}
              onPageChange={pageChange}
            >
              <Column
                field="orderAdjustmentTypeDescription"
                title={getTranslatedLabel(`${localizationKey}.type`, "Adjustment Type")}
                cell={orderAdjustmentCell}
                width={140}
              />
              <Column field="amount" title={getTranslatedLabel(`${localizationKey}.amount`, "Amount")} width={130} />
              <Column field="description" title={getTranslatedLabel(`${localizationKey}.description`, "Description")} width={140} />
              <Column field="sourcePercentage" title={getTranslatedLabel(`${localizationKey}.percentage`, "Percentage")} width={150} />
              <Column field="isManual" title={getTranslatedLabel(`${localizationKey}.userEntered`, "User Entered")} width={110} />
              <Column cell={CommandCell} width="100px" />
            </KendoGrid>
          </div>
        </Grid>
      </Grid>
      <Grid item xs={2}>
        <Button onClick={() => onClose()} color="error" variant="contained">
          {getTranslatedLabel("general.close", "Close")}
        </Button>
      </Grid>
    </Fragment>
  );
}
