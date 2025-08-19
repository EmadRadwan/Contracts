import React, { useCallback, useState } from "react";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridDataStateChangeEvent,
  GridCellProps,
} from "@progress/kendo-react-grid";

import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import { Grid, Paper } from "@mui/material";
import Button from "@mui/material/Button";
import { State } from "@progress/kendo-data-query";
import {
  useAppDispatch,
//   useAppDispatch,
  useFetchProductsWithBOMQuery,
} from "../../../app/store/configureStore";
import { BillOfMaterial } from "../../../app/models/manufacturing/billOfMaterial";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { router } from "../../../app/router/Routes";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import BOMSimulationForm from "../form/BOMSimulationForm";
import {setProductId} from "../../orders/slice/sharedOrderUiSlice";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

//todo: Add new product button

function BOMProductsList() {
//   const [editMode, setEditMode] = useState(0);
//   const location = useLocation();
//   const dispatch = useAppDispatch();
const { getTranslatedLabel } = useTranslationHelper()
  const [selectedProduct, setSelectedProduct] = useState<BillOfMaterial | undefined>(undefined)
  const [showBOMSimulation, setShowBOMSimulation] = useState(false);
  const [dataState, setDataState] = React.useState<State>({ take: 9, skip: 0 });
  const dispatch = useAppDispatch();
  const dataStateChange = (e: GridDataStateChangeEvent) => {
    console.log("dataStateChange", e.dataState);
    setDataState(e.dataState);
  };
  const { data, isFetching } = useFetchProductsWithBOMQuery({
    ...dataState,
  });

  function handleSelectProduct(productId: string) {
     const selectedProduct: BillOfMaterial | undefined = data?.data?.find(
       (product: any) => product.productId === productId
     );
     
     console.log('selectedProduct', selectedProduct);
     
     // save selected product to redux store in sharedOrderUI
    dispatch(setProductId(selectedProduct?.productId!));
    

    // setEditMode(2);
    router.navigate("/bomProductComponents");
  }

//   function cancelEdit() {
//     setEditMode(0);
//   }

  const BOMSimulationCellButton = (props: any) => {
    const { dataItem } = props;

    return (
      <td className="k-command-cell">
        <Button
          className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
          onClick={() => props.open(dataItem)}
          // disabled={dataItem.isManual === "N"}
          color="error"
        >
          {getTranslatedLabel("manufacturing.bom.list.bomSimulation", "BOM Simulation")}
        </Button>
      </td>
    );
  };
  const open = (dataItem: any) => {
    setSelectedProduct(dataItem)
    setShowBOMSimulation(true)
  }
  const CommandCell = (props: GridCellProps) => (
    <BOMSimulationCellButton
      {...props}
      open={open}
    />
  );

  const ProductNameCell = (props: any) => {
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
        <Button onClick={() => handleSelectProduct(props.dataItem.productId)}>
          {props.dataItem.productName}
        </Button>
      </td>
    );
  };

  const onCloseBOMSImulation = useCallback(() => {
    setShowBOMSimulation(false);
  }, []);

  return (
    <>
      <ManufacturingMenu selectedMenuItem="billOfMaterials"/>
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item xs={12}>
            <Grid item xs={12}>
              <div className="div-container">
                <KendoGrid
                  style={{ height: "65vh", flex: 1 }}
                  data={
                    data
                      ? { data: data.data, total: data.total }
                      : { data: [], total: 0 }
                  }
                  resizable={true}
                  filterable={true}
                  sortable={true}
                  pageable={true}
                  {...dataState}
                  onDataStateChange={dataStateChange}
                >
                  <Column
                    field="productName"
                    title={getTranslatedLabel("manufacturing.bom.list.product", "Product")}
                    //cell={ProductNameCell}
                    locked={true}
                  />
                  <Column
                    field="uomDescription"
                    title={getTranslatedLabel("manufacturing.bom.list.uomDescription", "UOM Description")}
                  />
                  <Column
                    field="productDescription"
                    title={getTranslatedLabel("manufacturing.bom.list.productDescription", "Product Description")}
                  />
                  <Column
                    field="productAssocTypeDescription"
                    title={getTranslatedLabel("manufacturing.bom.list.bomType", "BOM Type")}
                  />
                  <Column cell={CommandCell} width={250} filterable={false} />
                </KendoGrid>
                {isFetching && (
                  <LoadingComponent
                    message={getTranslatedLabel("manufacturing.bom.list.loading", "Loading BOM Products...")}
                  />
                )}
              </div>
            </Grid>
          </Grid>
        </Grid>
      </Paper>
      {(showBOMSimulation && selectedProduct) && (
        <ModalContainer
          show={showBOMSimulation}
          onClose={onCloseBOMSImulation}
          width={1200}
        >
          <BOMSimulationForm selectedProduct={selectedProduct} onClose={onCloseBOMSImulation}/>
        </ModalContainer>
      )}
    </>
  );
}

export default BOMProductsList;