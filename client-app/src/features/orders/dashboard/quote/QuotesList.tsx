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
import {Grid, Paper} from "@mui/material";
import {ExcelExport} from "@progress/kendo-react-excel-export";
import {DataResult, State} from "@progress/kendo-data-query";
import {Quote} from "../../../../app/models/order/quote";
import {useAppDispatch, useFetchQuotesQuery} from "../../../../app/store/configureStore";
import QuoteForm from "../../form/quote/QuoteForm";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {handleDatesArray, handleDatesObject} from "../../../../app/util/utils";
import {setCustomerId} from "../../slice/sharedOrderUiSlice";
import OrderMenu from "../../menu/OrderMenu";

export default function QuotesList() {

    const [editMode, setEditMode] = useState(0);
    const [quote, setQuote] = useState<Quote | undefined>(undefined);


    const [dataState, setDataState] = React.useState<State>({take: 6, skip: 0});
    const [quotes, setQuotes] = React.useState<DataResult>({data: [], total: 0});

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };
    
    const dispatch = useAppDispatch();

    const {data, error, isFetching} = useFetchQuotesQuery({...dataState});

    useEffect(() => {
            if (data) {
                const adjustedData = handleDatesArray(data.data);
                setQuotes({data: adjustedData, total: data.total})
            }
        }
        , [data]);


    function handleSelectQuote(quoteId: string, quoteStatus: string) {
        // select the order from data array based on orderId
        const selectedQuote: Quote | undefined = data?.data.find(
            (quote: any) => quote.quoteId === quoteId,
        );

        //todo: apply in similar situations
        dispatch(setCustomerId(selectedQuote?.fromPartyId.fromPartyId));
        setQuote(handleDatesObject(selectedQuote));

        if (quoteStatus === "Created") {
            setEditMode(2);
        }

        if (quoteStatus === "Ordered") {
            setEditMode(4);
        }
    }

    console.log("editmode", editMode);

    // convert cancelEdit function to memoized function
    const cancelEdit = React.useCallback(() => {
        setEditMode(0);
        setQuote(undefined);
    }, [setEditMode, setQuote]);

    const QuoteDescriptionCell = (props: any) => {
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
                        handleSelectQuote(
                            props.dataItem.quoteId,
                            props.dataItem.statusDescription,
                        );
                    }}
                >
                    {props.dataItem.quoteId}
                </Button>
            </td>
        );
    };

    // Code for Grid functionality
    const dataToExport = data ? handleDatesArray(data.data) : [];

    const _export = React.useRef(null);
    const excelExport = () => {
        if (_export.current !== null) {
            _export.current!.save();
        }
    };

    if (editMode > 0) {
        return (
            <QuoteForm
                selectedQuote={quote}
                cancelEdit={cancelEdit}
                editMode={editMode}
            />
        );
    }

    return (
        <>

            <OrderMenu selectedMenuItem="quotes"/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={12}>
                        {/* <Grid container alignItems="center">
                            <Grid item xs={4}>
                                <Typography sx={{p: 2}} variant="h4">
                                    Quotes
                                </Typography>
                            </Grid>

                        </Grid> */}

                        <Grid container>
                            <ExcelExport data={dataToExport} ref={_export}>
                                <KendoGrid
                                    style={{height: '65vh', width: "94vw", flex: 1}}
                                    resizable={true}
                                    filterable={true}
                                    sortable={true}
                                    pageable={true}
                                    {...dataState}
                                    data={quotes ? quotes : {data: [], total: 77}}
                                    onDataStateChange={dataStateChange}
                                >
                                    <GridToolbar>
                                        <Grid container>
                                            {/* <Grid item xs={3}>
                                                <button
                                                    title="Export Excel"
                                                    className="k-button k-primary"
                                                    onClick={excelExport}
                                                >
                                                    Export to Excel
                                                </button>
                                            </Grid> */}
                                            <Grid item xs={2}>
                                                <Button
                                                    color={"secondary"}
                                                    onClick={() => {
                                                        setEditMode(1);
                                                    }}
                                                    variant="outlined"
                                                >
                                                    New Quote
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </GridToolbar>
                                    <Column
                                        field="quoteId"
                                        title="Quote Number"
                                        cell={QuoteDescriptionCell}
                                        locked={true}
                                    />
                                    <Column field="fromPartyName" title="Customer" />
                                    <Column field="grandTotal" title="Amount" />
                                    <Column
                                        field="statusDescription"
                                        title="Status"
                                    />
                                    <Column
                                        field="issueDate"
                                        title="Quote Date"
                                        format="{0: dd/MM/yyyy}"
                                    />
                                </KendoGrid>
                            </ExcelExport>
                            {isFetching && <LoadingComponent message="Loading Quotes..."/>}
                        </Grid>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}
