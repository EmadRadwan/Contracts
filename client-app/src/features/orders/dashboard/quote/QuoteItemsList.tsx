import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { useCallback, useState } from "react";
import {
  Grid as KendoGrid,
  GridCellProps,
  GridColumn as Column,
  GridPageChangeEvent,
  GridSortChangeEvent,
  GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import { Grid } from "@mui/material";
import { useSelector } from "react-redux";
import {
  useAppDispatch,
  useAppSelector,
  useFetchCustomerTaxStatusQuery,
} from "../../../../app/store/configureStore";
import { useFetchQuoteItemsQuery } from "../../../../app/store/apis/quote/quoteItemsApi";
import { useFetchQuoteAdjustmentsQuery } from "../../../../app/store/apis/quote/quoteAdjustmentsApi";

import { QuoteItemAdjustmentsListMemo } from "../../../services/dashboard/QuoteItemAdjustmentsList";
// import JobQuoteMarketingPkgItemsList from "./JobQuoteMarketingPkgItemsList";
// import ServiceItemSpecificationRateForm from "../form/ServiceItemSpecificationRateForm";

import {
  nonDeletedQuoteItemsSelector,
  quoteItemAdjustments,
  quoteSubTotal,
  selectAdjustedQuoteItems,
} from "../../../orders/slice/quoteSelectors";
import {
  setSelectedQuoteItem,
  setUiQuoteItems,
} from "../../../orders/slice/quoteItemsUiSlice";
import { setUiQuoteAdjustments } from "../../../orders/slice/quoteAdjustmentsUiSlice";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import { QuoteItem } from "../../../../app/models/order/quoteItem";
import { QuoteAdjustment } from "../../../../app/models/order/quoteAdjustment";
import QuoteItemForm from "../../form/quote/QuoteItemForm";
import { setSelectProductOrService } from "../../../orders/slice/sharedOrderUiSlice";
import QuoteAdjustmentsList from "./QuoteAdjustmentsList";

interface Props {
  quoteFormEditMode: number;
  quoteId?: string;
}

export default function QuoteItemsList({ quoteFormEditMode, quoteId }: Props) {
  // state for the grid
  const initialSort: Array<SortDescriptor> = [
    { field: "quoteItemSeqId", dir: "asc" },
  ];
  const [sort, setSort] = React.useState(initialSort);
  const initialDataState: State = { skip: 0, take: 4 };
  const [page, setPage] = React.useState<any>(initialDataState);
  const pageChange = (event: GridPageChangeEvent) => {
    setPage(event.page);
  };

  // state for the quote items component
  const [show, setShow] = useState(false);
  const [showList, setShowList] = useState(false);

  // state for the quote item adjustments component
  const [showItemAdjustmentList, setShowItemAdjustmentList] = useState(false);
  // const [showPackageProductList, setShowPackageProductList] = useState(false);

  // state for the quote items component edit mode
  const [editMode, setEditMode] = useState(0);
  const quoteSTotal: any = useSelector(quoteSubTotal);

  // state for selected quote item for editing
  const [quoteItem, setQuoteItem] = useState<QuoteItem | undefined>(undefined);

  const uiQuoteItems: any = useSelector(nonDeletedQuoteItemsSelector);

  const uiQuoteAdjustments: any = useSelector(quoteItemAdjustments);
  const customerId = useAppSelector(
    (state) => state.sharedOrderUi.selectedCustomerId
  );
  // const vehicleId = useAppSelector((state) => state.sharedOrderUi.selectedVehicleId);

  // get the quote items from the store
  const { data: quoteItemsData } = useFetchQuoteItemsQuery(quoteId, {
    skip: quoteId === undefined,
  });
  // get the quote adjustments from the store
  const { data: quoteAdjustmentsData } = useFetchQuoteAdjustmentsQuery(
    quoteId,
    { skip: quoteId === undefined }
  );

  const { data: customerTaxStatus } = useFetchCustomerTaxStatusQuery(
    customerId,
    { skip: customerId === undefined }
  );

  const dispatch = useAppDispatch();
  const adjustedQuoteItems = useSelector(selectAdjustedQuoteItems);

  // handle quote item selection from grid for editing
  function handleSelectQuoteItem(quoteItemId: string) {
    // get the quote item from the quoteItemsAndAdjustments state

    const selectedQuoteItem: QuoteItem | undefined = uiQuoteItems!.find(
      (quoteItem: any) =>
        quoteItem.quoteId.concat(quoteItem.quoteItemSeqId) === quoteItemId
    );

    // change string based productId with the actual product object
    // if this quoteItem is a saved one then we'll use the one from quoteItemProduct, else we'll use the one from productLov
    let quoteItemToDisplay;
    if (selectedQuoteItem?.quoteItemProduct) {
      quoteItemToDisplay = {
        ...selectedQuoteItem,
        productId: selectedQuoteItem?.quoteItemProduct,
      };
    } else {
      quoteItemToDisplay = {
        ...selectedQuoteItem,
        productId: selectedQuoteItem?.productLov,
      };
    }

    // set the selected quote item in state and start edit mode
    setQuoteItem(quoteItemToDisplay);
    setEditMode(2);
    setShow(true);
  }

  // custom grid cell to show total adjustments for an quote item and enable opening the items adjustments component
  const ItemDiscountCommandCell = (props: any) => {
    return (
      <td>
        <Button
          onClick={() => {
            // get the quote item from the quoteItemsAndAdjustments state
            const selectedQuoteItem: QuoteItem | undefined = uiQuoteItems!.find(
              (quoteItem: any) =>
                quoteItem.quoteId.concat(quoteItem.quoteItemSeqId) ===
                props.dataItem.quoteId.concat(props.dataItem.quoteItemSeqId)
            );

            // set the selected quote item in state and start edit mode
            dispatch(setSelectedQuoteItem(selectedQuoteItem));
            // save it in local state and open the adjustments component
            setQuoteItem(selectedQuoteItem);
            setShowItemAdjustmentList(true);
          }}
        >
          {props.dataItem.discountAndPromotionAdjustments
            ? props.dataItem.discountAndPromotionAdjustments.toFixed(2)
            : 0}
        </Button>
      </td>
    );
  };

  const ItemQuantityCommandCell = (props: any) => {
    const { productTypeId, quantity } = props.dataItem;
    const isMarketingPackage = productTypeId === "MARKETING_PKG";

    return <td>{quantity}</td>;
  };

  // custom grid cell for quote item selection
  const quoteItemCell = (props: any) => {
    return (
      <td>
        <Button
          onClick={() =>
            handleSelectQuoteItem(
              props.dataItem.quoteId.concat(props.dataItem.quoteItemSeqId)
            )
          }
          disabled={props.dataItem.isPromo === "Y"}
        >
          {props.dataItem.productName}
        </Button>
      </td>
    );
  };
  // close the quote item form
  const memoizedOnClose = useCallback(() => {
    setShow(false);
  }, []);
  // close the quote item Adjustment list
  const OnCloseItemAdjustmentList = useCallback(() => {
    setShowItemAdjustmentList(false);
  }, []);

  // const OnCloseMarketingItemsList = useCallback(() => {
  //     setShowPackageProductList(false);
  // }, []);

  // custom grid button function to delete an quote item
  const DeleteQuoteItemCell = (props: any) => {
    const { dataItem } = props;

    return (
      <td className="k-command-cell">
        <Button
          className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
          onClick={() => props.remove(dataItem)}
          disabled={props.dataItem.isPromo === "Y"}
          color="error"
        >
          Remove
        </Button>
      </td>
    );
  };

  // the actual deletion logic for an quote item
  const remove = (dataItem: QuoteItem) => {
    // get a local copy of the quote items
    let localQuoteItems: QuoteItem[] | undefined = uiQuoteItems;

    // check if the quote item has a related promo item(s) and delete them as well
    // start by checking if the other quote item is a promo item and by using
    // parentQuoteItemSeqId to find the related quote items

    const relatedQuoteItems = uiQuoteItems?.filter((item: QuoteItem) => {
      return item.parentQuoteItemSeqId === dataItem?.quoteItemSeqId;
    });
    // mark related quote items as deleted
    const newRelatedQuoteItems = relatedQuoteItems?.map((item: QuoteItem) => {
      return { ...item, isProductDeleted: true };
    });

    // update local quote items with the deleted quote items in newRelatedQuoteItems
    localQuoteItems = localQuoteItems?.map((item: QuoteItem) => {
      if (
        newRelatedQuoteItems?.some(
          (quoteItem: QuoteItem) =>
            quoteItem.quoteItemSeqId === item.quoteItemSeqId
        )
      ) {
        return newRelatedQuoteItems?.find(
          (quoteItem: QuoteItem) =>
            quoteItem.quoteItemSeqId === item.quoteItemSeqId
        );
      } else {
        return item;
      }
    });

    // mark the original quote item as deleted in local quote items
    localQuoteItems = localQuoteItems?.map((item: QuoteItem) => {
      if (item.quoteItemSeqId === dataItem?.quoteItemSeqId) {
        return { ...item, isProductDeleted: true };
      } else {
        return item;
      }
    });

    // set the quote items in state
    dispatch(setUiQuoteItems(localQuoteItems!));

    // also mark adjustments that are related to all the quote items
    // we are deleting that are defined in relatedQuoteItems
    const relatedQuoteAdjustments = uiQuoteAdjustments?.filter(
      (item: QuoteAdjustment) => {
        return localQuoteItems?.some(
          (quoteItem: QuoteItem) =>
            quoteItem.quoteItemSeqId === item.quoteItemSeqId &&
            quoteItem.isProductDeleted
        );
      }
    );
    // mark related quote adjustments as deleted
    const newRelatedQuoteAdjustments = relatedQuoteAdjustments?.map(
      (item: QuoteAdjustment) => {
        return { ...item, isAdjustmentDeleted: true };
      }
    );
    // set the quote adjustments in state
    dispatch(setUiQuoteAdjustments(newRelatedQuoteAdjustments!));
  };

  // support for quote item deletion cell
  const CommandCell = (props: GridCellProps) => (
    <DeleteQuoteItemCell {...props} remove={remove} />
  );

  // const memoizedOnClose2 = useCallback(() => {
  //     setShowNewRateSpecification(false);
  // }, []);

  const containerStyles = {
    opacity: 0.5,
    pointerEvents: "none",
  };
  console.log("quoteFormEditMode", quoteFormEditMode);

  return (
    <>
      {show && (
        <ModalContainer show={show} onClose={memoizedOnClose} width={700}>
          <QuoteItemForm
            quoteItem={quoteItem}
            editMode={editMode}
            onClose={memoizedOnClose}
            quoteFormEditMode={quoteFormEditMode}
          />
        </ModalContainer>
      )}
      {showList && (<ModalContainer show={showList} onClose={memoizedOnClose} width={900}>
                <QuoteAdjustmentsList
                    onClose={memoizedOnClose}
                />
            </ModalContainer>)}

      {showItemAdjustmentList && (
        <ModalContainer
          show={showItemAdjustmentList}
          onClose={memoizedOnClose}
          width={500}
        >
          <QuoteItemAdjustmentsListMemo
            quoteItem={quoteItem}
            onClose={OnCloseItemAdjustmentList}
            quoteFormEditMode={quoteFormEditMode}
          />
        </ModalContainer>
      )}

      {/* {showPackageProductList && (
                <ModalContainer show={showPackageProductList} onClose={memoizedOnClose} width={700}>
                    <JobQuoteMarketingPkgItemsList
                        onClose={OnCloseMarketingItemsList}
                    />
                </ModalContainer>)}

            {showNewRateSpecification && (
                <ModalContainer show={showNewRateSpecification} onClose={memoizedOnClose} width={500}>
                    <ServiceItemSpecificationRateForm
                        onClose={memoizedOnClose2}
                    />
                </ModalContainer>)} */}

      <Grid
        container
        columnSpacing={1}
        direction={"column"}
        alignItems="flex-start" sx={{mt: 1}}
      >
        <Grid item sx={quoteFormEditMode > 2 ? containerStyles : {}}>
          <KendoGrid
            className="main-grid"
            style={{ height: "40vh" }}
            data={orderBy(
              adjustedQuoteItems ? adjustedQuoteItems : [],
              sort
            ).slice(page.skip, page.take + page.skip)}
            sortable={true}
            sort={sort}
            onSortChange={(e: GridSortChangeEvent) => {
              setSort(e.sort);
            }}
            skip={page.skip}
            take={page.take}
            total={adjustedQuoteItems ? adjustedQuoteItems.length : 0}
            pageable={true}
            onPageChange={pageChange}
          >
            <GridToolbar>
              <Grid container justifyContent={"space-between"}>
                <Grid item>
                  <Button
                    color={"success"}
                    onClick={() => {
                      setQuoteItem(undefined);
                      setEditMode(1);
                      setShow(true);
                      dispatch(setSelectProductOrService("PRODUCT"));
                    }}
                    variant="contained"
                    disabled={customerId === undefined || quoteFormEditMode > 2}
                  >
                    Add Product
                  </Button>
                </Grid>
                <Grid item>
                  <Button
                    color="secondary"
                    onClick={() => {
                      setShowList(true);
                    }}
                    variant="outlined"
                    disabled={quoteSTotal === 0}
                  >
                    Quote Adjustments
                  </Button>
                </Grid>
              </Grid>
            </GridToolbar>
            <Column
              field="productName"
              title="Product"
              cell={quoteItemCell}
              width={300}
            />
            <Column field="quoteItemSeqId" title="quoteItemSeqId" width={0} />
            <Column field="unitPrice" title="Price" width={110} />
            <Column
              cell={ItemQuantityCommandCell}
              title="Quantity"
              width={120}
            />
            <Column
              cell={ItemDiscountCommandCell}
              title="Adjustments"
              width={150}
            />
            <Column
              field="subTotal"
              title="Sub Total"
              width={120}
              format="{0:c}"
            />
            <Column cell={CommandCell} width="100px" />
          </KendoGrid>
        </Grid>
      </Grid>
    </>
  );
}
