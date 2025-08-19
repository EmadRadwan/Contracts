import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {Grid, Paper} from "@mui/material";
import {useAppDispatch, useAppSelector, useFetchBillingAccountsQuery,} from "../../../../app/store/configureStore";
import Button from "@mui/material/Button";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {handleDatesArray} from "../../../../app/util/utils";
import {useLocation} from 'react-router-dom';
import {DataResult, State} from "@progress/kendo-data-query";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import { BillingAccount } from "../../../../app/models/accounting/billingAccount";
import BillingAccountForm from "../form/BillingAccountForm";
import { setSelectedBillingAccount } from "../../slice/accountingSharedUiSlice";


function BillingAccountsList() {
    const [editMode, setEditMode] = useState(0);
    const location = useLocation();
    const dispatch = useAppDispatch();
    const {selectedBillingAccount} = useAppSelector(state => state.accountingSharedUi)
    // const [show, setShow] = useState(false);
    const [dataState, setDataState] = React.useState<State>({take: 9, skip: 0});
    const [billingAccounts, setBillingAccounts] = React.useState<DataResult>({data: [], total: 0});

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        console.log('dataStateChange', e.dataState);
        setDataState(e.dataState);
    };
    const {data, isFetching} = useFetchBillingAccountsQuery({...dataState});

    useEffect(() => {
            if (data) {
                const adjustedData = handleDatesArray(data.data);
                setBillingAccounts({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    function handleSelectBillingAccounts(billingAccountId: string) {
        const selectedAccount: BillingAccount | undefined = billingAccounts?.data.find((billingAccount: any) => billingAccount.billingAccountId === billingAccountId);
        if (selectedAccount) {
            dispatch(setSelectedBillingAccount({
                ...selectedAccount!,
                partyId: {
                    fromPartyId: selectedAccount?.partyId,
                    fromPartyName: selectedAccount?.partyName
                }
            }))
            setEditMode(2);
        }
    }

    useEffect(() => {
        if (location.state?.selectedBillingAccountId && billingAccounts.data.length > 0) {
            handleSelectBillingAccounts(location.state?.selectedBillingAccountId)
        }
    }, [location.state?.selectedBillingAccountId, billingAccounts.data])

    function cancelEdit() {
        dispatch(setSelectedBillingAccount(undefined))
        setEditMode(0);
    }


    const BillingAccountCell = (props: any) => {
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
                onClick={() => handleSelectBillingAccounts(props.dataItem.billingAccountId)}
            >
                {props.dataItem.billingAccountId}
            </Button>

            </td>
        )
    };
    if (selectedBillingAccount) {
        return <BillingAccountForm selectedBillingAccount={selectedBillingAccount} editMode={2} onClose={cancelEdit} />
    }
    if (editMode > 0) {
        return <BillingAccountForm selectedBillingAccount={selectedBillingAccount} editMode={editMode} onClose={cancelEdit} />
    }
    return (
        <>
            <AccountingMenu selectedMenuItem={'/billingAccounts'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>

                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={6}>
                       
                        <Grid container>
                            <div className="div-container">
                                    <KendoGrid style={{height: "65vh", width: "94vw", flex: 1}}
                                               data={billingAccounts ? billingAccounts : {data: [], total: data!.total}}
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
                                                        Create Billing Account
                                                    </Button>
                                                </Grid>

                                            </Grid>
                                        </GridToolbar>
                                        <Column field="billingAccountId" title="Billing Account" cell={BillingAccountCell} />
                                        <Column field="partyName" title="Customer Name" width={250}/>
                                        <Column field="accountLimit" title="Account Limit" />
                                        <Column field="accountCurrencyUomDescription" title="Account Currency"
                                                />
                                        <Column field="fromDate" title="From"  format="{0: dd/MM/yyyy}"/>
                                        <Column field="thruDate" title="To"  format="{0: dd/MM/yyyy}"/>


                                    </KendoGrid>
                                {isFetching && <LoadingComponent message='Loading Billing Accounts...'/>}
                            </div>

                        </Grid>

                    </Grid>


                </Grid>
            </Paper>
        </>
    )
}

export default BillingAccountsList