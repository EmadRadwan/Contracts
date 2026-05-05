import React, {useCallback, useEffect, useState} from "react";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent, GridRowProps,
} from "@progress/kendo-react-grid";
import {DataResult, State} from '@progress/kendo-data-query';
import Button from "@mui/material/Button";
import {Grid, Paper} from "@mui/material";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {handleDatesArray, normalizeNumeric} from "../../../../app/util/utils";

import {
    RootState,
    useAppDispatch,
    useAppSelector,
    useFetchAcctTransEntriesQuery
} from "../../../../app/store/configureStore";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import {useLocation} from "react-router-dom";
import {AcctgTrans} from "../../../../app/models/accounting/acctgTrans";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import SetupAccountingMenu from "../menu/SetupAccountingMenu";
import AccountingSummaryMenu from "../menu/AccountingSummaryMenu";
import {useSelector} from "react-redux";
import {useNavigate} from "react-router";
import { AccountingTransactionEntriesDateRangeExcel } from "./AccountingTransactionEntriesDateRangeExcel";


export default function AccountingTransactionEntriesList() {

    const [accountingTransEntries, setAccountingTransEntries] = React.useState<DataResult>({data: [], total: 0});
    const [dataState, setDataState] = React.useState<State>({
        take: 25,
        skip: 0,
        sort: [{ field: "createdStamp", dir: "desc" }], // Default sort: newest first
    });
    const {user} = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const dispatch = useAppDispatch();
    const companyName = useSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);
    const navigate = useNavigate();               // <-- add this hook




    const handleSelectAcctTrans = useCallback((acctgTransId: string) => {
        navigate(`/editAcctgTrans/${acctgTransId}`, {
            state: { acctgTransId }, // optional: for immediate use without URL parsing
        });
    }, [navigate]);
    
    const location = useLocation()

    useEffect(() => {
        if (location?.state?.glAccountId) {
            setDataState(prev => ({
                ...prev,
                filter: {
                    logic: "and",
                    filters: [{ field: "glAccountId", operator: "eq", value: location?.state?.glAccountId }]
                },
                skip: 0,
                // Preserve the default sort when applying filter
                sort: [{ field: "acctgTransId", dir: "desc" }]
            }));
        }
    }, [location?.state?.glAccountId]);


    const {getTranslatedLabel} = useTranslationHelper();

    const [editMode, setEditMode] = useState(0);
    const [acctTrans, setAcctTrans] = useState<AcctgTrans | undefined>(undefined);

    const [show, setShow] = useState(false);


    const queryArgs = {...dataState, companyId};
    const {data, error, isFetching} = companyId
        ? useFetchAcctTransEntriesQuery(queryArgs)
        : {data: null, error: new Error("CompanyId is missing"), isFetching: false};

    useEffect(() => {
            if (data) {
                const adjustedData = handleDatesArray(data.data).map((item: any) => ({
                    ...item,
                    amount: normalizeNumeric(item.amount),
                }));
                setAccountingTransEntries({data: adjustedData, total: data.total})
            }
        }
        , [data]);

    


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
            >
                <Button onClick={() => handleSelectAcctTrans(props.dataItem.acctgTransId)}>
                    {props.dataItem.acctgTransId}
                </Button>

            </td>
        )
    };

    const rowRender = useCallback(
        (trElement: React.ReactElement<HTMLTableRowElement>, props: GridRowProps) => {
            const isDebit = props.dataItem.debitCreditFlag === "D";
            const style = {backgroundColor: isDebit ? "rgba(55, 180, 0, 0.32)" : "#ffffff"};
            return React.cloneElement(trElement, {style}, trElement.props.children);
        },
        []
    );


    // convert cancelEdit function to memoized function
    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
        setAcctTrans(undefined);
    }, [setEditMode, setAcctTrans]);


    return <>

        <AccountingMenu selectedMenuItem={'acctTrans'}/>
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <Grid container columnSpacing={1} alignItems="center">
                <SetupAccountingMenu/>
                <AccountingSummaryMenu/>
                <Grid item>
                    <AccountingTransactionEntriesDateRangeExcel
                        companyId={companyId}
                        getTranslatedLabel={getTranslatedLabel}
                    />
                </Grid>

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
                            rowRender={rowRender}
                        >

                            <Column field="acctgTransId"
                                    cell={AcctTransDescriptionCell} width={110}
                                    locked={!show} title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txns.acctgTransId",
                                "Acctg Trans Id"
                            )}/>
                            <Column
                                field="amount"
                                title={getTranslatedLabel(
                                    "accounting.payments.list.amount",
                                    "Amount"
                                )}
                                width={100}
                                format="{0:n}"
                                filter={"numeric"}
                            />

                            <Column field="acctgTransEntrySeqId" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txnEntries.acctgTransEntrySeqId",
                                "SEQ Id"
                            )} width={70}/>

                            <Column field="debitCreditFlag" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txnEntries.acctgTransEntrySeqId",
                                "SEQ Id"
                            )} width={80}/>

                            <Column field="transactionDate" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txns.transactionDate",
                                "Transaction Date"
                            )} width={130}
                                    format="{0: dd/MM/yyyy}"/>

                            <Column field="glAccountId" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txnEntries.glAccountId",
                                "Gl Account Id"
                            )} width={100}/>

                            <Column field="glAccountName" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txnEntries.glAccountName",
                                "Gl Account Id"
                            )} width={250}/>
                            
                            <Column field="partyName" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txnEntries.partyName",
                                "Party Name"
                            )} width={200}/>

                            <Column field="description" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txns.description",
                                "Description"
                            )} width={230}/> 
                            
                            <Column field="salesRequestId" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txns.salesRequestId",
                                "Sales Request Id"
                            )} width={100}/>

                            <Column field="invoiceId" title={getTranslatedLabel(
                                "accounting.invoices.list.invoiceId",
                                "Invoice Id"
                            )} width={100}/>

                            <Column field="paymentId" title={getTranslatedLabel(
                                "accounting.payments.list.paymentId",
                                "Payment Id"
                            )} width={100}/>

                            <Column field="certificateNumber" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txnEntries.workEffortId",
                                "WorkEffort Id"
                            )} width={100}/>

                            <Column field="productName" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txns.productName",
                                "Product Name"
                            )} width={100}/>

                            <Column field="isPosted" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txns.isPosted",
                                "Is Posted"
                            )} width={100}/>

                            <Column field="postedDate" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txns.postedDate",
                                "Posted Date"
                            )} width={150}
                                    format="{0: dd/MM/yyyy}"/>

                            <Column field="acctgTransactionTypeDescription" title={getTranslatedLabel(
                                "accounting.orgGL.accounting.summary.txns.acctgTransType",
                                "Acctg Trans Type"
                            )} width={150}/>

                        </KendoGrid>
                        {isFetching && <LoadingComponent message={getTranslatedLabel(
                            "accounting.orgGL.accounting.summary.loading",
                            "Loading Accounting Trans Entries..."
                        )}/>}
                    </div>

                </Grid>
            </Grid>
        </Paper>

    </>
}

