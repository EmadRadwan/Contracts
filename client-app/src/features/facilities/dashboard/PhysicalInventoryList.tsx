import { useCallback, useEffect, useState } from "react";
import FacilityMenu from "../menu/FacilityMenu";
import { Button, Grid, Paper } from "@mui/material";
import PhysicalInventoryForm from "../form/PhysicalInventoryForm";
import {
  useAppDispatch,
  useAppSelector
} from "../../../app/store/configureStore";
import {
  setSelectedPhysicalInventoryProductId,
  selectFacilityId,
} from "../slice/FacilitySlice";
import { ProductInventoryItem } from "../../../app/models/facility/productInventoryItem";
import {
  Grid as KendoGrid,
  GridColumn as Column,
  GridCellProps,
} from "@progress/kendo-react-grid";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import TransferPhysicalInventoryForm from "../form/TransferPhysicalInventoryForm";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {useFetchInventoryAvailableByFacilityQuery} from "../../../app/store/apis";

const PhysicalInventoryList = () => {
  const {getTranslatedLabel} = useTranslationHelper()
  const { selectedPhysicalInventoryProductId, selectedFacilityId } =
    useAppSelector((state) => state.facility);

  const [show, setShow] = useState(false);
  const [showGrid, setShowGrid] = useState(false);
  const [selectedInventoryItem, setSelectedInventoryItem] = useState<
    ProductInventoryItem | undefined
  >(undefined);

  const [queryParams, setQueryParams] = useState<{
    facilityId: string | undefined;
    productId: string | undefined;
  }>({
    facilityId: selectedFacilityId,
    productId: selectedPhysicalInventoryProductId?.productId,
  });

  const { data, isFetching, isSuccess } = useFetchInventoryAvailableByFacilityQuery(
      {
        facilityId: queryParams.facilityId!,
        productId: queryParams.productId!,
      },
      { skip: !queryParams.facilityId || !queryParams.productId }
  );
  
  const dispatch = useAppDispatch();

  const handleSubmit = useCallback(
      (values: any) => {
        setQueryParams({
          facilityId: values.facilityId,
          productId: values.productId.productId,
        });
        dispatch(setSelectedPhysicalInventoryProductId(values.productId));
        dispatch(selectFacilityId(values.facilityId));
      },
      [dispatch]
  );
  
  const memoizedOnClose = useCallback(() => {
    setSelectedInventoryItem(undefined);
    setShow(false);
  }, []);

  const handleTransferInventory = (values: any) => {
    console.log(values);
    memoizedOnClose();
  };

  useEffect(() => {
    if (isSuccess) {
      setShowGrid(true);
    }
  }, [isSuccess]);


  const TransferInventoryItemCell = (props: any) => {
    const { dataItem, transfer } = props;
    return (
      <td className="k-command-cell">
        <Button
          className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
          onClick={() => transfer(dataItem)}
        >
          {getTranslatedLabel("facility.physical.adjust", "Adjust Inventory")}
        </Button>
      </td>
    );
  };

  const transfer = (dataItem: ProductInventoryItem) => {
    const selectedItem = data?.find(
      (item: ProductInventoryItem) =>
        item.inventoryItemId === dataItem.inventoryItemId
    );
    if (selectedItem) {
      setSelectedInventoryItem({
        ...selectedItem,
        productName: selectedPhysicalInventoryProductId?.productName!
      });
    }
    setShow(true);
  };

  const CommandCell = (props: GridCellProps) => (
    <TransferInventoryItemCell {...props} transfer={transfer} />
  );

  return (
    <>
      {show && (
        <ModalContainer show={show} onClose={memoizedOnClose} width={800}>
          <TransferPhysicalInventoryForm
            inventoryItem={selectedInventoryItem!}
            onClose={memoizedOnClose}
            onSubmit={(values) => handleTransferInventory(values)}
          />
        </ModalContainer>
      )}

      <FacilityMenu />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item xs={12}>
            <PhysicalInventoryForm
              onSubmit={(values: any) => handleSubmit(values)}
            />
          </Grid>
        </Grid>
        <Grid container columnSpacing={1} alignItems="center">
          {showGrid && (
            <Grid item xs={12}>
              <div className="div-container">
                <KendoGrid
                  style={{ height: "40vh" }}
                  data={data ?? []}
                  resizable={true}
                  pageable={true}
                  reorderable={true}
                >
                  <Column field="inventoryItemId" title={getTranslatedLabel("facility.physical.invItem", "Inventory Item")} />
                  <Column field="itemATP" title={getTranslatedLabel("facility.physical.itemATP", "Item ATP")} />
                  <Column field="itemQOH" title={getTranslatedLabel("facility.physical.itemQOH", "Item QOH")} />
                  <Column field="productATP" title={getTranslatedLabel("facility.physical.productATP", "Product ATP")} />
                  <Column field="productQOH" title={getTranslatedLabel("facility.physical.productQOH", "Product QOH")} />
                  <Column cell={CommandCell} />
                </KendoGrid>
                {isFetching && (
                  <LoadingComponent message={getTranslatedLabel("facility.physical.loading", "Loading Physical Inventory...")} />
                )}
              </div>
            </Grid>
          )}
        </Grid>
      </Paper>
    </>
  );
};

export default PhysicalInventoryList;
