import React, {useEffect, useState} from "react";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
} from "@progress/kendo-react-grid";
import {DataResult, State} from '@progress/kendo-data-query';
import Button from "@mui/material/Button";
import {Grid, Paper} from "@mui/material";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {handleDatesArray} from "../../../../app/util/utils";

import {RootState, useAppDispatch, useFetchAcctTransEntriesQuery,} from "../../../../app/store/configureStore";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import {useLocation} from "react-router-dom";
import {AcctgTrans} from "../../../../app/models/accounting/acctgTrans";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import SetupAccountingMenu from "../menu/SetupAccountingMenu";
import AccountingSummaryMenu from "../menu/AccountingSummaryMenu";
import {useSelector} from "react-redux";
import {router} from "../../../../app/router/Routes";


export default function AccountingTransactionEntriesList() {

    const [accountingTransEntries, setAccountingTransEntries] = React.useState<DataResult>({data: [], total: 0});
    const [dataState, setDataState] = React.useState<State>({take: 6, skip: 0});

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const dispatch = useAppDispatch();
    const companyName = useSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);

    // if company name is not set, then redirect to the orgGl
    useEffect(() => {
        if (!companyName) {
            router.navigate("/orgGl");
        }
    }, [companyName]);

    const location = useLocation()

    useEffect(() => {
            if (location?.state?.glAccountId) {
                setDataState({
                    "filter": {
                            "logic": "and",
                            "filters":
                             [{
                                "field": "glAccountId",
                                "operator": "eq",
                                "value": location?.state?.glAccountId
                            }]
                        },
                    take: 6,
                    skip: 0
                })
            }
        }, [location?.state?.glAccountId])


    const {getTranslatedLabel} = useTranslationHelper();

    const [editMode, setEditMode] = useState(0);
    const [acctTrans, setAcctTrans] = useState<AcctgTrans | undefined>(undefined);

    const [show, setShow] = useState(false);


    const {data, error, isFetching} = useFetchAcctTransEntriesQuery({...dataState});

    useEffect(() => {
            if (data) {
                const adjustedData = handleDatesArray(data.data);
                setAccountingTransEntries({data: adjustedData, total: data.total})
            }
        }
        , [data]);


    function handleSelectAcctTrans(acctTransId: string) {
        // select the acctTrans from data array based on acctTransId
        const selectedAcctTrans: AcctgTrans | undefined = data?.data.find((acctTrans: any) => acctTrans.acctTransId === acctTransId);


        // set component selected acctTrans
        setAcctTrans(selectedAcctTrans);
    }


    const AcctTransDescriptionCell = (props: any) => {
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
                onClick={() => {
                    handleSelectAcctTrans(
                        props.dataItem.acctgTransId,
                    )
                }}
            >
                {props.dataItem.acctgTransId}
            </Button>

            </td>
        )
    };


    // convert cancelEdit function to memoized function
    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
        setAcctTrans(undefined);
    }, [setEditMode, setAcctTrans]);

    /*if (shouldShowTransactionForm) {
        return <AcctTransForm selectedAcctTrans={location.state?.acctTrans} cancelEdit={cancelEdit} editMode={2}/>
    }
*/


    /*if (editMode > 0 && whatWasClicked === 'acctTransId') {
        return <AcctTransForm selectedAcctTrans={acctTrans} cancelEdit={cancelEdit} editMode={editMode}/>
    }
*/

    return <>

        <AccountingMenu selectedMenuItem={'acctTrans'}/>
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <Grid container columnSpacing={1} alignItems="center">
                <SetupAccountingMenu/>
                <AccountingSummaryMenu/>

                <Grid item xs={12}>
                    <div className="div-container">
                        <KendoGrid
                            resizable={true}
                            filterable={true}
                            sortable={true}
                            pageable={true}
                            {...dataState}
                            data={accountingTransEntries ? accountingTransEntries : {data: [], total: 77}}
                            onDataStateChange={dataStateChange}
                        >

                            <Column field="acctTransId"
                                    cell={AcctTransDescriptionCell} width={110}
                                    locked={!show} title={"Acctg Trans Id"}/>
                            <Column field="acctgTransEntrySeqId" title="SEQ Id" width={130}/>

                            <Column field="transactionDate" title="Transaction Date" width={130}
                                    format="{0: dd/MM/yyyy}"/>
                            <Column field="acctgTransactionTypeDescription" title="Acctg Trans Type" width={150}/>

                            <Column field="glFiscalTypeId" title="Fiscal Gl Type" width={100}/>
                            <Column field="glAccountId" title="Gl Account Id" width={100}/>
                            <Column field="glAccountTypeDescription" title="Gl Account" width={150}/>
                            <Column field="invoiceId" title="Invoice Id" width={100}/>
                            <Column field="paymentId" title="Payment Id" width={100}/>
                            <Column field="workEffortId" title="WorkEffort Id" width={100}/>
                            <Column field="shipmentId" title="Shipment Id" width={100}/>
                            <Column field="isPosted" title="Is Posted" width={100}/>
                            <Column field="postedDate" title="Posted Date" width={150}
                                    format="{0: dd/MM/yyyy}"/>
                            <Column field="description" title="Description" width={130}/>

                        </KendoGrid>
                        {isFetching && <LoadingComponent message='Loading Accounting Trans Entries...'/>}
                    </div>

                </Grid>
            </Grid>
        </Paper>

    </>
}

