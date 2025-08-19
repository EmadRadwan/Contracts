import {useAppDispatch,} from "../../../../app/store/configureStore";
import React, {useEffect, useState} from "react";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import {
    Grid as KendoGrid,
    GRID_COL_INDEX_ATTRIBUTE,
    GridColumn as Column,
    GridDataStateChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import {DataResult, State} from "@progress/kendo-data-query";
import {Grid, Paper} from "@mui/material";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import AccountingMenu from "../menu/AccountingMenu";
import {useFetchInvoicesQuery,} from "../../../../app/store/apis/invoice/invoicesApi";
import {Invoice} from "../../../../app/models/accounting/invoice";
import {handleDatesArray} from "../../../../app/util/utils";
import {useNavigate} from "react-router";
import {setSelectedInvoice} from "../../slice/accountingSharedUiSlice";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";

export default function InvoicesList() {
    const [invoices, setInvoices] = useState<DataResult>({data: [], total: 0});
    const navigate = useNavigate();

    const initialDataState = {take: 6, skip: 0};
    const [dataState, setDataState] = React.useState<State>(initialDataState);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };
    const {data, isLoading, isFetching} = useFetchInvoicesQuery({...dataState});

    const dispatch = useAppDispatch();

    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.invoices.list";

    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data.data);
            setInvoices({data: adjustedData, total: data!.totalCount});
        }
    }, [data]);

    useEffect(() => {
        return () => {
            dispatch(setSelectedInvoice(undefined));
        };
    }, [dispatch]);

    const handleSelectInvoice = (invoice: Invoice) => {
        navigate(`/invoices/${invoice.invoiceId}`);
    };

    const InvoiceDescriptionCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: "blue"}}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{[GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex}}
                {...navigationAttributes}
            >
                <Button onClick={() => handleSelectInvoice(props.dataItem)}>
                    {props.dataItem.invoiceId}
                </Button>
            </td>
        );
    };


    return (
        <>
            <AccountingMenu selectedMenuItem={"/invoices"}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        <div className="div-container">
                            <KendoGrid
                                style={{height: "65vh", flex: 1}}
                                data={invoices ? invoices : {data: [], total: 0}}
                                resizable={true}
                                filterable={true}
                                sortable={true}
                                pageable={true}
                                {...dataState}
                                onDataStateChange={dataStateChange}
                            >
                                <GridToolbar>
                                    <Grid container>
                                        <Grid item xs={2}>
                                            <Button
                                                color="secondary"
                                                onClick={() => navigate("/invoices/new")}
                                                variant="outlined"
                                            >
                                                {getTranslatedLabel(`${localizationKey}.actions.create`, "Create Invoice")}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </GridToolbar>
                                <Column
                                    field="invoiceId"
                                    title={getTranslatedLabel(
                                        `${localizationKey}.invoiceId`,
                                        "Invoice Number"
                                    )}
                                    cell={InvoiceDescriptionCell}
                                    width={150}
                                    locked={true}
                                />
                                <Column
                                    field="invoiceTypeDescription"
                                    title={getTranslatedLabel(
                                        `${localizationKey}.invoiceType`,
                                        "Invoice Type"
                                    )}
                                    width={150}
                                />
                                <Column
                                    field="invoiceDate"
                                    title={getTranslatedLabel(
                                        `${localizationKey}.invoiceDate`,
                                        "Invoice Date"
                                    )}
                                    width={200}
                                    format="{0: dd/MM/yyyy}"
                                />
                                <Column
                                    field="statusDescription"
                                    title={getTranslatedLabel(
                                        `${localizationKey}.status`,
                                        "Status"
                                    )}
                                    width={100}
                                />
                                <Column
                                    field="description"
                                    title={getTranslatedLabel(
                                        `${localizationKey}.description`,
                                        "Description"
                                    )}
                                    width={100}
                                />
                                <Column
                                    field="fromPartyName"
                                    title={getTranslatedLabel(`${localizationKey}.from`, "From")}
                                    width={150}
                                />
                                <Column
                                    field="toPartyName"
                                    title={getTranslatedLabel(`${localizationKey}.to`, "To")}
                                    width={150}
                                />

                            </KendoGrid>

                            {(isLoading) && (
                                <LoadingComponent
                                    message={getTranslatedLabel(
                                        `${localizationKey}.loading`,
                                        "Loading Invoices..."
                                    )}
                                />
                            )}
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}
