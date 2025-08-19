import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";

import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {Grid, Paper, Typography} from "@mui/material";
import Button from "@mui/material/Button";
import {State} from "@progress/kendo-data-query";
import {RootState, useFetchBomProductComponentsApiQuery} from "../../../app/store/configureStore";
import {BillOfMaterial} from "../../../app/models/manufacturing/billOfMaterial";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {router} from "../../../app/router/Routes";
import {useSelector} from "react-redux";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

//todo: Add new product button

function BOMProductComponentsList() {
    const [editMode, setEditMode] = useState(0);
    const [dataState, setDataState] = React.useState<State>({take: 9, skip: 0});
    const { getTranslatedLabel } = useTranslationHelper();

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        console.log('dataStateChange', e.dataState);
        setDataState(e.dataState);
    };
    const {data, isFetching} = useFetchBomProductComponentsApiQuery({...dataState});

    // get the selected productId from the redux store
    const selectedProductId = useSelector((state: RootState) => state.sharedOrderUi.selectedProductId);
    console.log('selectedProductId', selectedProductId);

    useEffect(() => {
        // If productId is not undefined, add productId filter
        if (selectedProductId !== undefined) {
            let newDataState = {
                "filter":
                    {
                        "logic":
                            "and",
                        "filters":
                            [
                                {
                                    "field": "productId",
                                    "operator": "eq",
                                    "value": selectedProductId
                                }
                            ]
                    }
                ,
                "skip":
                    0,
                "take":
                    6

            };
            setDataState(newDataState);
        }

        // Update data state
    }, [selectedProductId]); // Run this effect whenever productId changes

    function handleSelectProduct(productId: string) {
        const selectedProduct: BillOfMaterial | undefined = data?.data?.find((product: any) => product.productId === productId);

        setEditMode(2);
        router.navigate("/bomProductComponents");

    }

    function cancelEdit() {
        setEditMode(0);
    }


    const ProductNameCell = (props: any) => {
        const field = props.field || '';
        const value = props.dataItem[field];
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: 'blue'}}
                colSpan={props.colSpan}
                role={'gridcell'}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{
                    [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex
                }}
                {...navigationAttributes}
            ><Button
                onClick={() => handleSelectProduct(props.dataItem.productId)}
            >
                {props.dataItem.productNameTo}
            </Button>

            </td>
        )
    };



    return (
        <>
            <ManufacturingMenu selectedMenuItem={'billOfMaterials'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>

                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <Grid container alignItems="center">
                            <Grid item xs={12}>
                            <Typography sx={{p: 2}} variant='h4'>
                                    {getTranslatedLabel("manufacturing.bom.components.billOfMaterialsFor", "Bill of Materials (Recipe) for")} {data && data.data[0].productName}
                                </Typography>
                            </Grid>

                        </Grid>


                        <Grid item xs={12}>
                            <div className="div-container">
                                <KendoGrid
                                    style={{height: '65vh', flex: 1}}
                                    data={data ? {data: data.data, total: data.total} : {data: [], total: 0}}
                                    resizable={true}
                                    filterable={true}
                                    sortable={true}
                                    pageable={true}
                                    {...dataState}
                                    onDataStateChange={dataStateChange}
                                >
                                    <GridToolbar>
                                        <Grid container>
                                            <Grid item xs={4}>
                                                <Button color={"secondary"} onClick={() => setEditMode(1)}
                                                        variant="outlined">
                                                    {getTranslatedLabel("manufacturing.bom.components.createBOMComponent", "Create BOM Component")}
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </GridToolbar>
                                    <Column field="productIdTo" title={getTranslatedLabel("manufacturing.bom.components.product", "Product")} cell={ProductNameCell} width={300} locked={true}/>
                                    <Column field="productDescriptionTo" title={getTranslatedLabel("manufacturing.bom.components.productDescription", "Product Description")} width={400}/>
                                    <Column field="quantity" title={getTranslatedLabel("manufacturing.bom.components.quantity", "Quantity")} width={130}/>
                                    <Column field="quantityUOMDescription" title={getTranslatedLabel("manufacturing.bom.components.quantityUOM", "Quantity UOM")} width={150}/>
                                    <Column field="sequenceNum" title={getTranslatedLabel("manufacturing.bom.components.sequenceNum", "Sequence Num")} width={150}/>
                                    <Column field="scrapFactor" title={getTranslatedLabel("manufacturing.bom.components.scrapFactor", "Scrap Factor")} width={150}/>
                                </KendoGrid>
                                {isFetching && <LoadingComponent message={getTranslatedLabel("manufacturing.bom.components.loadingBOMProducts", "Loading BOM Products...")}/>}
                           </div>

                        </Grid>

                    </Grid>


                </Grid>
            </Paper>
        </>
    )
}

export default BOMProductComponentsList;

