import React, {useEffect, useState} from "react"
import AccountingMenu from "../../invoice/menu/AccountingMenu"
import {useAppDispatch, useAppSelector, useFetchFinancialAccountsQuery} from "../../../../app/store/configureStore"
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
import { FinancialAccount } from "../../../../app/models/accounting/financialAccount";
import { setSelectedFinancialAccount } from "../../slice/accountingSharedUiSlice";
import FinancialAccountForm from "../form/FinancialAccountForm";

const FinancialAccountsList = () => {
    const [dataState, setDataState] = useState<State>({take: 8, skip: 0});
    const dispatch = useAppDispatch()
    const [editMode, setEditMode] = useState(0);
    const [financialAccounts, setFinancialAccounts] = useState<DataResult>({data: [], total: 0})
    const dataStateChange = (e: GridDataStateChangeEvent) => {
        console.log('dataStateChange', e.dataState);
        setDataState(e.dataState);
    };
    const {selectedFinancialAccount} = useAppSelector(state => state.accountingSharedUi)
    const {data, isFetching} = useFetchFinancialAccountsQuery({...dataState})

    useEffect(() => {
            if (data) {
                const adjustedData = handleDatesArray(data.data);
                setFinancialAccounts({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    const handleSelectFinancialAccount = (finAccountId: string) => {
        console.log(finAccountId)
        const selected: FinancialAccount | undefined = financialAccounts?.data?.find((a: FinancialAccount) => a.finAccountId === finAccountId)
        dispatch(setSelectedFinancialAccount({
            ...selected,
            availableBalance: selected?.availableBalance?.toString(),
            actualBalance: selected?.actualBalance?.toString()
        }))
        setEditMode(2)
    }

    const FinancialAccountCell = (props: any) => {
        
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
                onClick={() => handleSelectFinancialAccount(props.dataItem.finAccountId)}
            >
                {props.dataItem.finAccountName}
            </Button>

            </td>
        )
    };

    const dataToExport = data ? handleDatesArray(data.data) : []


    const _export = React.useRef(null);

    const handleClose = () => {
        dispatch(setSelectedFinancialAccount(undefined))
        setEditMode(0)
    }

    if (selectedFinancialAccount || editMode) {
        return <FinancialAccountForm selectedFinancialAccount={selectedFinancialAccount} editMode={selectedFinancialAccount ? 2 : editMode} onClose={handleClose} />
    }
    
    console.log('isFetching', isFetching)
    
    return (
        <>
            <AccountingMenu selectedMenuItem={'/financialAcc'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>

                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={6}>
                        <Grid container>
                            <div className="div-container">
                                <ExcelExport data={dataToExport}
                                             ref={_export}>
                                    <KendoGrid style={{height: "65vh", width: "94vw", flex: 1}}
                                               data={financialAccounts ? financialAccounts : {
                                                   data: [],
                                                   total: data!.total || 0
                                               }}
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
                                                    <Button color={"secondary"} variant="outlined" onClick={() => setEditMode(1)}>
                                                        Create Financial Account
                                                    </Button>
                                                </Grid>
                                            </Grid>
                                        </GridToolbar>
                                        <Column field="finAccountId" title="Fin Account"
                                                cell={FinancialAccountCell} width={220}
                                                locked={true}/>
                                        <Column field="availableBalance" title="Available Balance" />
                                        <Column field="actualBalance" title="Actual Balance" />
                                        <Column field="currencyUomDescription" title="Currency" width={160} />
                                        <Column field="finAccountTypeDescription" title="Financial Account Type"/>
                                        <Column field="organizationPartyName" title="Org. Party" />
                                        <Column field="ownerPartyName" title="Owner Party" />
                                        {/* <Column field="replenishLevel" title="Replenish Level" /> */}
                                        <Column field="isRefundable" title="Is Refundable" />


                                    </KendoGrid>
                                </ExcelExport>
                                {isFetching && <LoadingComponent message='Loading Financial Accounts...'/>}
                            </div>

                        </Grid>

                    </Grid>


                </Grid>
            </Paper>
        </>
    )
}

export default FinancialAccountsList