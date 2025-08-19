import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";

import {ExcelExport} from "@progress/kendo-react-excel-export";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {Grid, Paper, Typography} from "@mui/material";
import {useAppDispatch, useAppSelector, useFetchFixedAssetsQuery,} from "../../../../app/store/configureStore";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {handleDatesArray} from "../../../../app/util/utils";
import {useLocation} from 'react-router-dom';
import {DataResult, State} from "@progress/kendo-data-query";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { FixedAsset } from "../../../../app/models/accounting/fixedAsset";
import { setSelectedFixedAsset } from "../../slice/accountingSharedUiSlice";
import FixedAssetForm from "../form/FixedAssetForm";


function FixedAssetsList() {
    const [editMode, setEditMode] = useState(0);
    const location = useLocation();
    const dispatch = useAppDispatch();
    const [show, setShow] = useState(false);
    const [dataState, setDataState] = React.useState<State>({take: 9, skip: 0});
    const [fixedAssets, setFixedAssets] = React.useState<DataResult>({data: [], total: 0});
    const selectedFixedAsset = useAppSelector(state => state.accountingSharedUi.selectedFixedAsset)

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        console.log('dataStateChange', e.dataState);
        setDataState(e.dataState);
    };
    const {data, error, isFetching} = useFetchFixedAssetsQuery({...dataState});

    useEffect(() => {
            if (data) {
                const adjustedData = handleDatesArray(data.data);
                setFixedAssets({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    function handleSelectFixedAssets(fixedAssetId: string) {
        const selectedFixedAsset: FixedAsset | undefined = data?.data?.find((fixedAsset: FixedAsset) => fixedAsset.fixedAssetId === fixedAssetId);
        console.log(selectedFixedAsset)
        dispatch(setSelectedFixedAsset(selectedFixedAsset!))
        setEditMode(2);
    }

    function cancelEdit() {
        dispatch(setSelectedFixedAsset(undefined))
        setEditMode(0);
    }


    const FixedAssetCell = (props: any) => {
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
                onClick={() => handleSelectFixedAssets(props.dataItem.fixedAssetId)}
            >
                {props.dataItem.fixedAssetName}
            </Button>

            </td>
        )
    };

    if (location.state?.myStateProp === 'bar' && selectedFixedAsset) {
        return <FixedAssetForm fixedAsset={selectedFixedAsset} cancelEdit={cancelEdit} editMode={2} />
    }

    if (editMode > 0) {
        return <FixedAssetForm fixedAsset={selectedFixedAsset} cancelEdit={cancelEdit} editMode={editMode} />
    }
    

    return (
        <>
            <AccountingMenu selectedMenuItem={'fixedAssets'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>

                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid style={{height: '65vh'}}
                                       data={fixedAssets ? fixedAssets : {data: [], total: data!.total}}
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
                                                Create Fixed Asset
                                            </Button>
                                        </Grid>

                                    </Grid>


                                </GridToolbar>
                                <Column field="fixedAssetName" title="Fixed Asset Name" cell={FixedAssetCell}
                                        width={250}
                                        locked={true}/>
                                <Column field="fixedAssetTypeDescription" title="Asset Type" width={180}/>
                                <Column field="dateAcquired" title="Date Acquired"
                                        width={250} format="{0: dd/MM/yyyy}"/>
                                <Column field="expectedEndOfLife" title="Expected End Of Life" width={150}
                                        format="{0: dd/MM/yyyy}"/>
                                <Column field="purchaseCost" title="Purchase Cost" width={170}/>
                                <Column field="salvageValue" title="Salvage Value" width={200}/>
                                <Column field="depreciation" title="Depreciation" width={200}/>


                            </KendoGrid>
                            {isFetching && <LoadingComponent message='Loading Fixed Assets...'/>}
                        </div>

                    </Grid>
                </Grid>
            </Paper>
        </>
    )
}

export default FixedAssetsList