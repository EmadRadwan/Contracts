import CatalogMenu from "../../menu/CatalogMenu";
import React, {useEffect, useState} from "react"
import {useAppDispatch, useFetchProductStoresQuery} from "../../../../app/store/configureStore"
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {ExcelExport} from "@progress/kendo-react-excel-export";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {Button, Grid, Paper, Typography} from "@mui/material";
import {DataResult, State} from "@progress/kendo-data-query"
import {handleDatesArray} from "../../../../app/util/utils"
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import { setSelectedProductStore } from "../../slice/productStoreUiSlice";
import ProductStoreForm from "../../form/productStore/ProductStoreForm";

export default function StoresList() {
    const [dataState, setDataState] = useState<State>({take: 8, skip: 0});
    const [editMode, setEditMode] = useState(0);
    const dispatch = useAppDispatch()
    const [stores, setStores] = useState<DataResult>({data: [], total: 0})
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        console.log('dataStateChange', e.dataState);
        setDataState(e.dataState);
    };
    const {getTranslatedLabel} = useTranslationHelper();
    const {data, isFetching} = useFetchProductStoresQuery({...dataState})
    useEffect(() => {
        console.log(stores)
    }, [stores])

    useEffect(() => {
            if (data) {
                // the .map is just for test purposes to remove  all the null values
                const adjustedData = handleDatesArray(data.data)
                setStores({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    const handleSelectStore = (storeId: string) => {
        console.log(storeId)
        const selectedProductStore = stores.data.find(store => store.productStoreId === storeId)
        if (selectedProductStore) {
            dispatch(setSelectedProductStore(selectedProductStore))
        }
        setEditMode(2)
    }

    const FinancialAccountCell = (props: any) => {
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
                onClick={() => handleSelectStore(props.dataItem.productStoreId)}
            >
                {props.dataItem.storeName}
            </Button>

            </td>
        )
    };

    const dataToExport = data ? handleDatesArray(data.data) : []


    const _export = React.useRef(null);
    const excelExport = () => {
        if (_export.current !== null) {
            _export.current!.save();
        }
    };

    const cancelEdit = () => {
        dispatch(setSelectedProductStore(undefined));
        setEditMode(0);
    }

    if (editMode > 0) {
        return <ProductStoreForm editMode={editMode} cancelEdit={cancelEdit} />
    }

    return (
        <>
            <CatalogMenu/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>

                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={6}>
                        {/* <Grid container alignItems="center">
                            <Grid item xs={6}>
                                <Typography sx={{p: 2}} variant='h4'>Product Stores</Typography>
                            </Grid>

                        </Grid> */}


                        <Grid container>
                            <div className="div-container">
                                <ExcelExport data={dataToExport}
                                             ref={_export}>
                                    <KendoGrid style={{height: "70vh", width: "94vw", flex: 1}}
                                               data={stores ? stores : {data: [], total: data!.total || 0}}
                                               resizable={true}
                                               filterable={true}
                                               sortable={true}
                                               pageable={true}
                                               {...dataState}
                                               onDataStateChange={dataStateChange}
                                    >
                                        <GridToolbar>
                                            <Grid container>
                                                {/* <Grid item xs={4}>
                                                    <Button className="k-button k-primary" color="primary"
                                                            variant="contained"
                                                            onClick={excelExport}>
                                                        Export to Excel
                                                    </Button>
                                                </Grid> */}
                                                <Grid item xs={4}>
                                                    <Button color={"secondary"} onClick={() => setEditMode(1)}
                                                            variant="outlined">
                                                        {getTranslatedLabel("product.stores.list.new", "Create Store")}
                                                    </Button>
                                                </Grid>

                                            </Grid>


                                        </GridToolbar>
                                        <Column field="fixedAssetName" title={getTranslatedLabel("product.stores.list.name", "Fixed Asset Name")}
                                                cell={FinancialAccountCell}
                                                locked={true}/>
                                        <Column field="payToPartyName" title={getTranslatedLabel("product.stores.list.party", "Pay to Party")} />
                                        <Column field="inventoryFacilityName" title={getTranslatedLabel("product.stores.list.facility", "Inventory Facility")} />
                                        <Column field="defaultCurrencyUomId" title={getTranslatedLabel("product.stores.list.currency", "Currency")}/>
                                        {/* <Column field="finAccountTypeDescription" title="Financial Account Type" width={150}/>
                                        <Column field="organizationPartyName" title="Org. Party" width={160}/>
                                        <Column field="ownerPartyName" title="Owner Party" width={160}/>
                                        <Column field="replenishLevel" title="Replenish Level" width={140}/>
                                        <Column field="isRefundable" title="Is Refundable" width={150}/> */}


                                    </KendoGrid>
                                </ExcelExport>
                                {isFetching && <LoadingComponent message='Loading Stores...'/>}
                            </div>

                        </Grid>

                    </Grid>


                </Grid>
            </Paper>
        </>

    )
}