import {useAppSelector, useFetchFacilityInventoriesByInventoryItemQuery} from "../../../app/store/configureStore";
import React, {useEffect, useState} from "react";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridCellProps,
    GridColumn as Column,
    GridDataStateChangeEvent, GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";

import {Grid, Paper, Typography} from "@mui/material";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {handleDatesArray, handleDatesObject} from "../../../app/util/utils";
import {InventoryItem} from "../../../app/models/facility/inventoryItem";
import InventoryItemForm from "../form/InventoryItemForm";
import FacilityMenu from "../menu/FacilityMenu";
import {useDispatch} from "react-redux";
import {selectProductById, setFacilityId, setInventoryItemId, setSelectedProductName} from "../slice/facilityInventoryUiSlice";
import {DataResult, State} from "@progress/kendo-data-query";
import {useNavigate} from "react-router";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { router } from "../../../app/router/Routes";

export default function InventoryItemsList() {
    const {selectedProductId, selectedProductName} =
        useAppSelector((state: any) => state.facilityInventoryUi);
    const dispatch = useDispatch()
    const [inventoryItem, setInventoryItem] = useState<InventoryItem | undefined>(
        undefined,
    );
    const [inventoryItems, setInventoryItems] = React.useState<DataResult>({data: [], total: 0});
    const navigate = useNavigate();

    const [editMode, setEditMode] = useState(0);
    const initialDataState: State = {take: 8, skip: 0}
    const [dataState, setDataState] = React.useState<State>(initialDataState);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };
    const {getTranslatedLabel} = useTranslationHelper();

    useEffect(() => {
        // If productId is not undefined, add productId filter
        if (selectedProductName !== undefined) {
            let newDataState = {
                "filter":
                    {
                        "logic": "and",
                        "filters":
                            [{
                                "field": "productId",
                                "operator": "eq",
                                "value": selectedProductId
                            }]
                    }
                ,
                ...initialDataState
            };
            setDataState(newDataState);
        }

    }, [selectedProductId]);

    const {data, isFetching} =
        useFetchFacilityInventoriesByInventoryItemQuery({...dataState});

    useEffect(() => {
            if (data) {
                const adjustedData = handleDatesArray(data.data);
                setInventoryItems({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    function handleSelectInventoryItem(inventoryItemId: string) {
        const selectedInventoryItem: any = data?.data.find(
            (inventoryItem: any) => inventoryItem.inventoryItemId === inventoryItemId,
        );
        dispatch(setFacilityId(selectedInventoryItem.facilityId))
        setInventoryItem(handleDatesObject(selectedInventoryItem));
        setEditMode(2);
    }

    const TransferInventoryItemCell = (props: any) => {
        const {dataItem, transfer} = props;
        // console.log('dataItem from TransferInventoryItemCell', dataItem);
        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    onClick={() => transfer(dataItem)}
                >
                    {getTranslatedLabel('facility.items.transfer', 'Transfer')}
                </Button>
            </td>
        );
    };

    const transfer = (dataItem: InventoryItem) => {
        // console.log('dataItem from transfer', dataItem);
        dispatch(setInventoryItemId(dataItem.inventoryItemId));
        navigate("/inventoryTransfer", {state: {inventoryItem: dataItem}});

    };

    const TransferCell = (props: GridCellProps) => (
        <TransferInventoryItemCell {...props} transfer={transfer}/>
    );

    const DetailsCell = (props: GridCellProps) => (
        <NavigateToDetailsCell {...props} />
    );

    const NavigateToDetailsCell = (props: any) => {
        // console.log('dataItem from TransferInventoryItemCell', dataItem);
        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    onClick={() => router.navigate("/inventoryItemDetails")}
                >
                    {getTranslatedLabel("facility.items.details", "Details")}
                </Button>
            </td>
        );
    };

    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
        dispatch(setFacilityId(undefined))
    }, [setEditMode]);

    const onClearSelection = () => {
        dispatch(selectProductById(undefined));
        dispatch(setSelectedProductName(undefined));
        setDataState(initialDataState)
    }
    const InventoryItemCell = (props: any) => {
        const field = props.field || "";
        const value = props.dataItem[field];
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: "blue"}}
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
                        handleSelectInventoryItem(props.dataItem.inventoryItemId);
                    }}
                >
                    {props.dataItem.inventoryItemId}
                </Button>
            </td>
        );
    };

    if (editMode > 0) {
        return (
            <InventoryItemForm
                inventoryItem={inventoryItem}
                cancelEdit={cancelEdit}
                editMode={editMode}
            />
        );
    }

    return (
        <>
            <FacilityMenu/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container columnSpacing={1} alignItems="center">
                    {selectedProductId && <Grid item xs={10}>
                        <Typography sx={{p: 2}} variant="h4">
                            {selectedProductName
                                ? ` ${selectedProductName}`
                                : ""}
                        </Typography>
                    </Grid>}
                    {selectedProductId && <Grid item xs={2}>
                        <Button variant="contained" onClick={onClearSelection}>
                            {getTranslatedLabel("facility.items.clear", "Clear selected product")}
                        </Button>
                    </Grid>}
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                style={{height: '65vh'}}
                                resizable={true}
                                filterable={true}
                                sortable={true}
                                pageable={true}
                                {...dataState}
                                data={inventoryItems ? inventoryItems : {data: [], total: 77}}
                                onDataStateChange={dataStateChange}
                            >
                                <GridToolbar>
                                    <Grid container>
                                        <Grid item xs={4}>
                                            <Button
                                                color={"secondary"}
                                                variant="outlined"
                                                onClick={() => setEditMode(1)}
                                            >
                                                Create Inventory Item
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </GridToolbar> 
                                <Column
                                    field="inventoryItemId"
                                    title={getTranslatedLabel('facility.items.item', 'Inventory Item')}
                                    cell={InventoryItemCell}
                                    width={100}
                                    reorderable={false}
                                />
                                {!selectedProductId && <Column
                                    field="productName"
                                    title={getTranslatedLabel('facility.items.product', 'Product')}
                                    width={160}
                                />}
                                <Column
                                    field="facilityName"
                                    title={getTranslatedLabel('facility.items.facility', 'Facility')}
                                    width={160}
                                />
                                <Column
                                    field="quantityOnHandTotal"
                                    title={getTranslatedLabel('facility.items.qoh', 'QOH Total')}
                                    width={100}
                                />
                                <Column
                                    field="availableToPromiseTotal"
                                    title={getTranslatedLabel('facility.items.atp', 'ATP Total')}
                                    width={100}
                                />
                                <Column
                                    field="colorFeatureDescription"
                                    title={getTranslatedLabel('facility.items.color', 'Color')}
                                    width={100}
                                />
                                <Column
                                    field="sizeFeatureDescription"
                                    title={getTranslatedLabel('facility.items.size', 'Size')}
                                    width={100}
                                />
                                <Column field="partyName" title={getTranslatedLabel('facility.items.supplier', 'Supplier')} />
                                <Column
                                    field="datetimeReceived"
                                    title={getTranslatedLabel('facility.items.received', 'Datetime Received')}
                                    width={160}
                                    format="{0: dd/MM/yyyy}"
                                />
                                <Column cell={TransferCell} width={125}/>
                                <Column cell={DetailsCell} width={125}/>
                            </KendoGrid>
                            {isFetching && (
                                <LoadingComponent message={getTranslatedLabel('facility.items.loading', 'Loading Inventory Items...')}/>
                            )}
                        </div>

                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}