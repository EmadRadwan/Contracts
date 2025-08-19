import React, {useEffect, useRef, useState} from "react"
import {DataResult, State} from '@progress/kendo-data-query';
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import AccountingMenu from "../../../invoice/menu/AccountingMenu";
import GlSettingsMenu from "../../menu/GlSettingsMenu";
import {handleDatesArray} from "../../../../../app/util/utils";
import {Button, Grid, Paper, Typography} from "@mui/material";
import {ExcelExport} from "@progress/kendo-react-excel-export";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {useFetchInvoiceItemTypesQuery} from "../../../../../app/store/configureStore";
import {InvoiceItemType} from "../../../../../app/models/accounting/invoiceItemType";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";

const InvoiceItemTypeList = () => {
    const [dataState, setDataState] = useState<State>({take: 6, skip: 0});
    const [invoiceItemTypes, setInvoiceItemTypes] = useState<DataResult>({data: [], total: 0})
    const [selectedType, setSelectedType] = useState()
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        console.log('dataStateChange', e.dataState);
        setDataState(e.dataState);
    }
    const {data, isLoading, error} = useFetchInvoiceItemTypesQuery({...dataState})
    console.log(data)

    const handleSelectInvItemType = (selectedInvItemType: InvoiceItemType) => {
        setSelectedType(selectedInvItemType)
        console.log(selectedType)
    }

    const _export = useRef(null)
    const excelExport = () => {
        if (_export.current !== null) {
            // @ts-ignore
            _export.current!.save();
        }
    };
    useEffect(() => {
            if (data) {
                let adjustedData = handleDatesArray(data?.data)
                setInvoiceItemTypes({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    const InvoiceItemTypeCell = (props: any) => {
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
                <Button onClick={() => {
                    handleSelectInvItemType(props.dataItem)
                }}
                >
                    {props.dataItem.invoiceItemTypeId}
                </Button>

            </td>
        )
    }

    let dataToExport = invoiceItemTypes ? invoiceItemTypes : []
    return (
        <>
            <AccountingMenu selectedMenuItem={'/globalGL'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <GlSettingsMenu/>
                <Grid container spacing={1} alignItems={"center"}>
                    {/* <Grid item xs={12}>
                        <Typography sx={{p: 2}} variant='h5'>Invoice Item Types</Typography>
                    </Grid> */}
                    <Grid container p={1}>
                        <div className="div-container">
                            <ExcelExport data={dataToExport} ref={_export}>
                                <KendoGrid className="main-grid" style={{height: "65vh", width: "93vw", flex: 1}}
                                           data={invoiceItemTypes ? invoiceItemTypes : {data: [], total: 0}}
                                           sortable={true}
                                           pageable={true}
                                           filterable={true}
                                           resizable={true}
                                           {...dataState}
                                           onDataStateChange={dataStateChange}
                                >
                                    <GridToolbar>
                                        {/* <Grid item xs={3}>
                                            <Button color={"primary"} variant={"contained"}
                                                    onClick={excelExport}>
                                                Export to Excel
                                            </Button>
                                        </Grid> */}
                                    </GridToolbar>
                                    <Column
                                        field="invoiceItemTypeId"
                                        title="Invoice Item Type"
                                        cell={InvoiceItemTypeCell}
                                        // width={200}
                                    />
                                    <Column
                                        field="description"
                                        title="Description"
                                        // width={250}
                                        className="grid-col"
                                    />
                                    <Column
                                        field="parentTypeId"
                                        title="Parent Type"
                                        // width={160}
                                    />
                                    <Column
                                        field="defaultGlAccountId"
                                        title="Default GL Account"
                                        // width={160}
                                    />
                                    <Column
                                        field="hasTable"
                                        title="Has Table"
                                        // width={160}
                                    />

                                </KendoGrid>
                                {isLoading && <LoadingComponent message='Loading Invoice Item Types...'/>}
                            </ExcelExport>
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    )
}

export default InvoiceItemTypeList