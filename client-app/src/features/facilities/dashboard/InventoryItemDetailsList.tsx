import React, {useEffect, useState} from "react";
import {Grid as KendoGrid, GridColumn as Column, GridDataStateChangeEvent} from "@progress/kendo-react-grid";
import {Button, Grid, Paper, Typography} from "@mui/material";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import FacilityMenu from "../menu/FacilityMenu";
import {
    useAppDispatch,
    useAppSelector,
    useFetchFacilityInventoriesByInventoryItemDetailsQuery,
} from "../../../app/store/configureStore";
import {DataResult, State} from "@progress/kendo-data-query";
import {handleDatesArray} from "../../../app/util/utils";
import { selectProductById, setSelectedProductName } from "../slice/facilityInventoryUiSlice";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';

export default function InventoryItemDetailsList() {
    const {getTranslatedLabel} = useTranslationHelper()
    const {selectedProductName, selectedProductId} =
        useAppSelector((state) => state.facilityInventoryUi);
    const [inventoryItemDetails, setInventoryDetailsItems] = React.useState<DataResult>({data: [], total: 0});
    const dispatch = useAppDispatch()

    const [productName, setProductName] = useState<string | undefined>(selectedProductName! || undefined);
    const initialDataState: State = {take: 8, skip: 0}
    const [dataState, setDataState] = React.useState<State>(initialDataState);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        console.log('dataStateChange', e.dataState);
        setDataState(e.dataState);
    };
    const {data, isFetching} =
        useFetchFacilityInventoriesByInventoryItemDetailsQuery({...dataState});
    console.log(data)
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
    
            // Update data state
        }, [selectedProductId]);

    useEffect(() => {
            if (data) {
                const adjustedData = handleDatesArray(data.data);
                setInventoryDetailsItems({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    useEffect(() => {
        setProductName(selectedProductName!)
    }, [selectedProductName])

    const CustomCell = (props) => {
        const actionable = props.dataItem[props.field] < 0
        return (
          <td
            {...props.tdProps}
            colSpan={1}
            style={{
              backgroundColor: actionable  && "#f55d65"
            }}
          >
            {props.field === "quantityOnHandDiff" ? props.dataItem.quantityOnHandDiff : props.dataItem.availableToPromiseDiff}
          </td>
        );
      };

    const CustomCell1 = (props) => {
        const value = props.dataItem[props.field];
        const isPositive = value > 0;
        const isNegative = value < 0;

        // Check if the record is a "starting inventory"
        const isStartingInventory =
            props.dataItem.quantityOnHandDiff === props.dataItem.availableToPromiseDiff &&
            props.dataItem.quantityOnHandDiff === props.dataItem.accountingQuantityDiff;

        const isReservation = props.field === "availableToPromiseDiff" && !isStartingInventory; // Differentiate only if not a starting inventory

        return (
            <td {...props.tdProps} colSpan={1}>
                <div style={{ display: "flex", alignItems: "center", gap: "5px" }}>
                    {isPositive && (
                        <ArrowUpwardIcon
                            style={{
                                color: isReservation ? "blue" : "green",
                            }}
                        />
                    )}
                    {isNegative && (
                        <ArrowDownwardIcon
                            style={{
                                color: isReservation ? "blue" : "red",
                            }}
                        />
                    )}
                    <span>{value}</span>
                </div>
            </td>
        );
    };
      const onClearSelection = () => {
        dispatch(selectProductById(undefined));
        dispatch(setSelectedProductName(undefined));
        setDataState(initialDataState)
    }

    return (
        <>
            <FacilityMenu/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>

                <Grid container columnSpacing={1} alignItems="center">
                    {selectedProductId && <Grid item xs={10}>
                        <Typography sx={{p: 2}} variant="h4">
                            Inventory Items Details{" "}
                            {productName && <span>for {productName}</span>}
                        </Typography>
                    </Grid>}
                    {selectedProductId && <Grid item xs={2} >
                        <Button variant="contained" onClick={onClearSelection}>
                            Clear selected product
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
                                data={inventoryItemDetails ? inventoryItemDetails : {
                                    inventoryItemDetails: [],
                                    total: 0
                                }}
                                onDataStateChange={dataStateChange}
                                // rowRender={rowRender}
                            >
                                <Column
                                    field="inventoryItemId"
                                    title={getTranslatedLabel("facility.details.item", "Inventory Item")}
                                    width={130}
                                    locked
                                />
                                <Column field="orderId" title={getTranslatedLabel("facility.details.order", "Order Id")} />
                                <Column field="productName" title={getTranslatedLabel("facility.details.product", "Product")} />
                                <Column field="facilityName" title={getTranslatedLabel("facility.details.facility", "Facility")} />
                                <Column
                                    field="quantityOnHandTotal"
                                    title={getTranslatedLabel("facility.details.qohTotal", "QOH Total")}
                                />
                                <Column
                                    field="availableToPromiseTotal"
                                    title={getTranslatedLabel("facility.details.atpTotal", "ATP Total")}
                                />
                                <Column
                                    field="quantityOnHandDiff"
                                    title={getTranslatedLabel("facility.details.qohDiff", "QOH DiFF")}
                                    cell={CustomCell1}
                                />
                                <Column
                                    field="availableToPromiseDiff"
                                    title={getTranslatedLabel("facility.details.atpDiff", "ATP DiFF")}
                                    cell={CustomCell1}
                                />
                                <Column
                                    field="effectiveDate"
                                    title={getTranslatedLabel("facility.details.date", "Effective Date")}
                                    format="{0: dd/MM/yyyy}"
                                /> 
                                <Column
                                    field="workEffortId"
                                    title={getTranslatedLabel("facility.details.workEffortId", "Work Effort Id")}
                                />
                            </KendoGrid>
                            {isFetching && (
                                <LoadingComponent message="Loading Inventory Details..."/>
                            )}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}
