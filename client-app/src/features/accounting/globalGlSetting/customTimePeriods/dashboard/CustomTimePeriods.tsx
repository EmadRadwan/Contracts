import React, {useEffect, useRef, useState} from "react"
import {DataResult, State} from '@progress/kendo-data-query';
import {useFetchCustomTimePeriodsQuery} from '../../../../../app/store/configureStore'
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
import LoadingComponent from "../../../../../app/layout/LoadingComponent";

const CustomTimePeriods = () => {
    const [dataState, setDataState] = useState<State>({take: 6, skip: 0});
    const [timePeriods, setTimePeriods] = useState<DataResult>({data: [], total: 0})
    const [selectedPeriod, setSelectedPeriod] = useState()
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        console.log('dataStateChange', e.dataState);
        setDataState(e.dataState);
    }
    const _export = useRef(null)
    const excelExport = () => {
        if (_export.current !== null) {
            // @ts-ignore
            _export.current!.save();
        }
    };
    const {data, isFetching, error} = useFetchCustomTimePeriodsQuery({...dataState})
    console.log(data)

    useEffect(() => {
            if (data) {
                let adjustedData = handleDatesArray(data?.data)
                setTimePeriods({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    let dataToExport = timePeriods ? timePeriods : []

    const handleSelectTimePeriod = (selectedTimePeriod: any) => {
        setSelectedPeriod(selectedTimePeriod)
        console.log(selectedTimePeriod)
    }

    const CustomTimePeriodCell = (props: any) => {
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
                    handleSelectTimePeriod(props.dataItem)
                }}
                >
                    {props.dataItem.customTimePeriodId}
                </Button>

            </td>
        )
    }

    return (
        <>
            <AccountingMenu selectedMenuItem={'/globalGL'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <GlSettingsMenu />
                <Grid container spacing={1} alignItems={"center"}>
                    {/* <Grid item xs={12}>
                        <Typography sx={{p: 2}} variant='h5'>Edit Custom Time Periods</Typography>
                    </Grid> */}
                    <Grid container p={1}>
                        <div className="div-container">
                            <ExcelExport data={dataToExport} ref={_export}>
                                <KendoGrid className="main-grid" style={{height: "65vh", width: "93vw", flex: 1}}
                                           data={timePeriods ? timePeriods : {data: [], total: 0}}
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
                                    {/* <Column
                                        field="periodNum"
                                        title="Period Number"
                                        width={130}
                                    /> */}
                                    <Column
                                        field="customTimePeriodId"
                                        title="Custom Time Period Id"
                                        cell={CustomTimePeriodCell}
                                        width={160}
                                    />
                                    <Column
                                        field="periodTypeDescription"
                                        title="Period Type"
                                        // width={150}
                                    />
                                    <Column
                                        field="periodName"
                                        title="Period"
                                        // width={130}
                                    />
                                    <Column
                                        field="fromDate"
                                        title="From"
                                        // width={140}
                                        format="{0: dd/MM/yyyy}"
                                    />
                                    <Column
                                        field="thruDate"
                                        title="To"
                                        // width={140}
                                        format="{0: dd/MM/yyyy}"
                                    />
                                    <Column
                                        field="isClosed"
                                        title="Is Closed"
                                        // width={130}
                                    />

                                </KendoGrid>
                                {isFetching && <LoadingComponent message='Loading Time Periods...'/>}
                            </ExcelExport>
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    )
}

export default CustomTimePeriods