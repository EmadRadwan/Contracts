import React, { useEffect, useState } from "react";
import CatalogMenu from "../../menu/CatalogMenu";
import { useFetchProductPromosQuery } from "../../../../app/store/apis";
import {
  Grid as KendoGrid,
  GRID_COL_INDEX_ATTRIBUTE,
  GridColumn as Column,
  GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import { useTableKeyboardNavigation } from "@progress/kendo-react-data-tools";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import CreateProductPromoForm from "../../form/productPromo/CreateProductPromoForm";
import { ProductPromo } from "../../../../app/models/product/productPromo";
import { handleDatesArray } from "../../../../app/util/utils";
import { Grid, Paper } from "@mui/material";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

export default function PromosList() {
  const [promo, setPromo] = useState<ProductPromo | undefined>(undefined);
  const [promos, setPromos] = useState<ProductPromo[] | undefined>(undefined);
  const [editMode, setEditMode] = useState(0);
  const { data, error, isLoading } = useFetchProductPromosQuery(undefined);
  const {getTranslatedLabel} = useTranslationHelper()

  useEffect(() => {
    if (data) {
      const promosArray = handleDatesArray(data);
      setPromos(promosArray);
    }
  }, [data]);

  const handleSelectPromo = (promoId: string) => {
    const selectedPromo: ProductPromo | undefined = promos?.find(
      (p) => p.productPromoId === promoId
    );

    setPromo(selectedPromo);
    setEditMode(2);
  };

  const cancelEdit = React.useCallback(() => {
    setEditMode(0);
    setPromo(undefined);
  }, [setEditMode, setPromo]);

  const ProductPromoIdCell = (props: any) => {
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
            // todo implement function to handle promo selection and displays form with promo info
            handleSelectPromo(props.dataItem.productPromoId);
          }}
        >
          {props.dataItem.productPromoId}
        </Button>
      </td>
    );
  };

  if (editMode) {
    return (
      <CreateProductPromoForm
        productPromo={promo}
        cancelEdit={cancelEdit}
        editMode={editMode}
      />
    );
  }

  return (
    <>
      <CatalogMenu />
      <Paper elevation={5} className={`div-container-withBorderCurved`}>
        <Grid container columnSpacing={1} alignItems="center">
          <Grid item xs={12}>
            <div className="div-container">
              <KendoGrid
                style={{ height: "65vh", flex: 1 }}
                data={promos}
                resizable={true}
                reorderable={true}
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
                      >
                        {getTranslatedLabel("product.promos.list.create", "Create New Promotion")}
                      </Button>
                    </Grid>
                  </Grid>
                </GridToolbar>

                <Column
                  field="productPromoId"
                  title={getTranslatedLabel("product.promos.list.id", "Promotion ID")}
                  cell={ProductPromoIdCell}
                  width={110}
                  locked={true}
                />
                <Column field="promoName" title={getTranslatedLabel("product.promos.list.name", "Promotion Name")} />
                <Column
                  field="operatorEnumDescription"
                  title={getTranslatedLabel("product.promos.list.condition", "Promotion Condition")}
                />
                <Column
                  field="inputParamEnumDescription"
                  title={getTranslatedLabel("product.promos.list.input", "Input Parameter Description")}
                />
                <Column field="quantity" title={getTranslatedLabel("product.promos.list.quantity", "Quantity")} />
                <Column
                  field="productPromoActionEnumDescription"
                  title={getTranslatedLabel("product.promos.list.description", "Promotion Description")}
                />
                <Column field="amount" title={getTranslatedLabel("product.promos.list.amount", "Discount Amount")} />
              </KendoGrid>
            </div>
            {isLoading && <LoadingComponent message={getTranslatedLabel("product.promos.list.loading", "Loading Promos...")} />}
          </Grid>
        </Grid>
      </Paper>
    </>
  );
}
