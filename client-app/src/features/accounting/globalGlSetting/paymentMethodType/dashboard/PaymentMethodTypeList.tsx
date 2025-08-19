import React, {useRef, useState} from "react"
import {useFetchPaymentMethodTypesQuery} from "../../../../../app/store/configureStore"
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import AccountingMenu from "../../../invoice/menu/AccountingMenu";
import GlSettingsMenu from "../../menu/GlSettingsMenu";
import {DataResult, State} from '@progress/kendo-data-query';
import {Button, Grid, Paper, Typography} from "@mui/material";
import {ExcelExport} from "@progress/kendo-react-excel-export";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import PaymentMethodTypeForm from "../form/PaymentMethodTypeForm";
import {toast} from "react-toastify";
import {handleDatesArray} from "../../../../../app/util/utils";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";


const PaymentMethodTypeList = () => {

    const [show, setShow] = useState(false)
    const [paymentMethod, setPaymentMethod] = useState(undefined)
    const [dataState, setDataState] = useState<State>({take: 6, skip: 0});
    const [paymentMethodTypes, setPaymentMethodTypes] = useState<DataResult>({data: [], total: 0})
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        console.log('dataStateChange', e.dataState);
        setDataState(e.dataState);
    };
    const _export = React.useRef(null);
    const {data, error, isFetching} = useFetchPaymentMethodTypesQuery({...dataState})
    console.log(data)

    


    const PaymentMethodUpdateCell = (props: any) => {
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
                    handleSelectPaymentMethod(props.dataItem)
                }}
                >
                    {props.dataItem.defaultGlAccountId}
                </Button>

            </td>
        )
    }

    const handleSelectPaymentMethod = (selectedPaymentMethod: any) => {
        setPaymentMethod(selectedPaymentMethod)
        setShow(true)

    }

    React.useEffect(() => {
            if (data) {
                let adjustedData = handleDatesArray(data?.data)
                setPaymentMethodTypes({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    const dataToExport: never[] = paymentMethodTypes ? paymentMethodTypes : {data: [], total: 0}

    const handleUpdate = (data: any) => {
        console.log(data.values)
        toast.success("Updated payment method in console")
        setShow(false)
    }

    return (
        <>
            <PaymentMethodTypeForm show={show} onClose={() => setShow(false)} onSubmit={handleUpdate}
                                   selectedPaymentMethod={paymentMethod ? paymentMethod : undefined} width={500}/>
            <AccountingMenu selectedMenuItem={'/globalGL'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <GlSettingsMenu />
                <Grid container spacing={1} alignItems={"center"}>
                    {/* <Grid item xs={12}>
                        <Typography sx={{p: 2}} variant='h5'>Payment Method Type List</Typography>
                    </Grid> */}
                    <Grid container p={1}>
                        <div className="div-container">
                            <ExcelExport data={dataToExport} ref={_export}>
                                <KendoGrid className="main-grid" style={{height: "65vh", width: "93vw", flex: 1}}
                                           data={paymentMethodTypes ? paymentMethodTypes : {data: [], total: 0}}
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
                                        field="paymentMethodTypeId"
                                        title="Payment Method Type"
                                        // width={200}
                                    />
                                    <Column
                                        field="description"
                                        title="Description"
                                        // width={200}
                                    />
                                    <Column
                                        field="defaultGlAccountId"
                                        title="Default Gl Account Id"
                                        cell={PaymentMethodUpdateCell}
                                        // width={200}
                                    />
                                </KendoGrid>
                                {isFetching && <LoadingComponent message='Loading Peyment Methods...'/>}
                            </ExcelExport>
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    )
}

export default PaymentMethodTypeList
