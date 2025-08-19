import {useFetchInventoryTransferQuery} from "../../../app/store/configureStore";
import React, {useEffect, useState} from "react";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";

import {Grid, Paper} from "@mui/material";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {handleDatesArray, handleDatesObject} from "../../../app/util/utils";
import FacilityMenu from "../menu/FacilityMenu";
import {useDispatch} from "react-redux";
import {DataResult, State} from "@progress/kendo-data-query";
import {InventoryTransfer} from "../../../app/models/facility/inventoryTransfer";
import InventoryTransferForm from "../form/InventoryTransferForm";
import {useLocation} from "react-router-dom";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

export default function InventoryTransferList() {
    const dispatch = useDispatch();
    const location = useLocation();
    const {getTranslatedLabel} = useTranslationHelper()

    const [inventoryTransfer, setInventoryTransfer] = useState<InventoryTransfer | undefined>(
        undefined,
    );
    const [inventoryTransfers, setInventoryTransfers] = React.useState<DataResult>({data: [], total: 0});
    const [shouldShowTransferForm, setShouldShowTransferForm] = useState(false);

    const [editMode, setEditMode] = useState(0);
    const [dataState, setDataState] = React.useState<State>({take: 6, skip: 0});

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const {data, isFetching} =
        useFetchInventoryTransferQuery({...dataState});

    useEffect(() => {
        if (location.state?.inventoryItem && !inventoryTransfer) {
            setShouldShowTransferForm(true);
        }
    }, [location.state?.inventoryItem]);

    useEffect(() => {
            if (data) {
                const adjustedData = handleDatesArray(data.data);
                setInventoryTransfers({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    function handleSelectInventoryTransfer(inventoryTransferId: string) {
        const selectedInventoryTransfer: any = data?.data.find(
            (inventoryTransfer: any) => inventoryTransfer.inventoryTransferId === inventoryTransferId,
        );
        setInventoryTransfer(handleDatesObject(selectedInventoryTransfer));
        setEditMode(2);
    }


    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
    }, [setEditMode]);

    const InventoryTransferCell = (props: any) => {
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
                        handleSelectInventoryTransfer(props.dataItem.inventoryTransferId);
                    }}
                >
                    {props.dataItem.inventoryItemId}
                </Button>
            </td>
        );
    };

    console.log('shouldShowTransferForm: ', shouldShowTransferForm);
    if (shouldShowTransferForm) {
        return (
            <InventoryTransferForm
                inventoryItem={location.state?.inventoryItem}
                cancelEdit={cancelEdit}
                editMode={1}
                setShouldShowTransferForm={setShouldShowTransferForm}
            />
        );
    }

    if (editMode > 0) {
        return (
            <InventoryTransferForm
                cancelEdit={cancelEdit}
                editMode={editMode}
                selectedInventoryTransfer={inventoryTransfer}
                setShouldShowTransferForm={setShouldShowTransferForm}
            />
        );
    }

    return (
        <>
            <FacilityMenu/>
            <Paper elevation={5} className={`div-container-withBorderCurved`} >
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                style={{height: '65vh'}}
                                resizable={true}
                                filterable={true}
                                sortable={true}
                                pageable={true}
                                {...dataState}
                                data={inventoryTransfers ? inventoryTransfers : {data: [], total: 77}}
                                onDataStateChange={dataStateChange}
                            >
                                <Column
                                    field="inventoryTransferId"
                                    title={getTranslatedLabel("facility.transfer.list.invTransfer", "Inventory Transfer")}
                                    cell={InventoryTransferCell}
                                    width={110}
                                    reorderable={false}
                                />
                                <Column
                                    field="inventoryItemId"
                                    title={getTranslatedLabel("facility.transfer.list.item", "Inventory Item")}
                                    reorderable={false}
                                />
                                <Column
                                    field="facilityName"
                                    title={getTranslatedLabel("facility.transfer.list.fromFacility", "From Facility")}
                                    width={160}
                                />
                                <Column
                                    field="facilityToName"
                                    title={getTranslatedLabel("facility.transfer.list.toFacility", "To Facility")}
                                    width={160}
                                />
                                <Column
                                    field="productName"
                                    title={getTranslatedLabel("facility.transfer.list.product", "Product")}
                                    width={160}
                                />

                                <Column
                                    field="atpQoh"
                                    title={getTranslatedLabel("facility.transfer.list.atp_qoh", "ATP/QOH")}
                                />
                                <Column
                                    field="sendDate"
                                    title={getTranslatedLabel("facility.transfer.list.sendDate", "Send Date")}
                                    width={160}
                                    format="{0: dd/MM/yyyy}"
                                />
                                <Column
                                    field="receiveDate"
                                    title={getTranslatedLabel("facility.transfer.list.receiveDate", "Receive Date")}
                                    width={160}
                                    format="{0: dd/MM/yyyy}"
                                />
                            </KendoGrid>
                            {isFetching && (
                                <LoadingComponent message="Loading Inventory Transfers..."/>
                            )}
                        </div>

                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}