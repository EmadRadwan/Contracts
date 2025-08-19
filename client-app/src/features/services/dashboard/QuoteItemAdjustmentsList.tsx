import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
} from "@progress/kendo-react-grid";

import Button from "@mui/material/Button";
import {Grid, Typography} from "@mui/material";
import {useSelector} from "react-redux";
import {useAppDispatch, useAppSelector,} from "../../../app/store/configureStore";
import {quoteAdjustmentsSelector, quoteItemAdjustmentsSelector, setUiQuoteAdjustments,} from "../slice/quoteUiSlice";

import {QuoteAdjustment} from "../../../app/models/order/quoteAdjustment";
import QuoteItemAdjustmentForm from "../../orders/form/quote/QuoteItemAdjustmentForm";


interface Props {
    quoteItem?: any;
    onClose: () => void;
    quoteFormEditMode: number;
}

function QuoteItemAdjustmentsList({
                                      quoteItem,
                                      onClose,
                                      quoteFormEditMode,
                                  }: Props) {
    // state for the grid
    const initialSort: Array<SortDescriptor> = [
        {field: "partyId", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    // state for showing adjustment item form
    const [show, setShow] = useState(false);

    const [quoteAdjustment, setQuoteAdjustment] = useState<QuoteAdjustment | undefined>(undefined);
    const [editMode, setEditMode] = useState(0);
    const dispatch = useAppDispatch();
    const uiQuoteItemAdjustments: any = useSelector(quoteItemAdjustmentsSelector);
    const uiQuoteAdjustments: any = useSelector(quoteAdjustmentsSelector);
    const {user} = useAppSelector((state) => state.account);
    const roleWithPercentage = user!.roles!.find(
        (role) => role.Name === "AddAdjustments",
    );


    function handleSelectQuoteAdjustment(quoteAdjustmentId: string) {
        // select quote adjustment from quoteAdjustments list
        const quoteAdjustment = uiQuoteItemAdjustments!.find(
            (adjustment: QuoteAdjustment) =>
                adjustment.quoteAdjustmentId === quoteAdjustmentId,
        );
        setQuoteAdjustment(quoteAdjustment);

        setEditMode(2);
        setShow(true);
    }

    const DeleteQuoteItemAdjustmentCell = (props: any) => {
        const {dataItem} = props;

        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    onClick={() => props.remove(dataItem)}
                    disabled={dataItem.isManual === "N"}
                    color="error"
                >
                    Remove
                </Button>
            </td>
        );
    };
    const remove = (dataItem: QuoteAdjustment) => {
        const newQuoteAdjustments: QuoteAdjustment[] | undefined =
            uiQuoteAdjustments?.map((item: QuoteAdjustment) => {
                if (item.quoteAdjustmentId === dataItem?.quoteAdjustmentId) {
                    return {...item, isAdjustmentDeleted: true};
                } else {
                    return item;
                }
            });
        // update quote items in the slice
        dispatch(setUiQuoteAdjustments(newQuoteAdjustments));
    };

    const CommandCell = (props: GridCellProps) => (
        <DeleteQuoteItemAdjustmentCell {...props} remove={remove}/>
    );

    const quoteAdjustmentCell = (props: any) => {
        return props.dataItem.isManual === "Y" ? (
            <td>
                <Button
                    onClick={() =>
                        handleSelectQuoteAdjustment(props.dataItem.quoteAdjustmentId)
                    }
                >
                    {props.dataItem.quoteAdjustmentTypeDescription}
                </Button>
            </td>
        ) : (
            <td>{props.dataItem.quoteAdjustmentTypeDescription}</td>
        );
    };

    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };

    return (
        <React.Fragment>
            <QuoteItemAdjustmentForm
                quoteItem={quoteItem}
                quoteAdjustment={quoteAdjustment}
                editMode={editMode}
                onClose={() => setShow(false)}
                show={show}
                width={1000}
            />
            <Grid container columnSpacing={1}>
                <Grid container alignItems="center">
                    <Grid item xs={4}>
                        <Typography sx={{p: 2}} variant="h5">
                            Adjustments for
                        </Typography>
                    </Grid>
                    <Grid item xs={4}>
                        <Typography color="primary" sx={{p: 2}} variant="h5">
                            {quoteItem?.productName}
                        </Typography>
                    </Grid>

                    <Grid item xs={4}>
                        {roleWithPercentage && (
                            <Button
                                color={"secondary"}
                                onClick={() => {
                                    setEditMode(1);
                                    setShow(true);
                                }}
                                variant="contained"
                                disabled={quoteFormEditMode > 2}
                            >
                                Add Adjustment
                            </Button>
                        )}
                    </Grid>
                </Grid>
                <Grid item xs={12}>
                    <div className="div-container">
                        <KendoGrid
                            className="main-grid"
                            style={{height: "400px"}}
                            data={orderBy(
                                uiQuoteItemAdjustments ? uiQuoteItemAdjustments : [],
                                sort,
                            ).slice(page.skip, page.take + page.skip)}
                            sortable={true}
                            sort={sort}
                            onSortChange={(e: GridSortChangeEvent) => {
                                setSort(e.sort);
                            }}
                            skip={page.skip}
                            take={page.take}
                            total={
                                uiQuoteItemAdjustments ? uiQuoteItemAdjustments.length : 0
                            }
                            pageable={true}
                            onPageChange={pageChange}
                        >
                            <Column
                                field="quoteAdjustmentTypeDescription"
                                title="Adjustment Type"
                                cell={quoteAdjustmentCell}
                                width={150}
                            />
                            <Column field="quoteId" title="quoteId" width={0}/>
                            <Column
                                field="quoteItemSeqId"
                                title="quoteItemSeqId"
                                width={0}
                            />
                            <Column field="amount" title="Amount" width={130}/>
                            <Column
                                field="sourcePercentage"
                                title="Percentage"
                                width={140}
                            />
                            <Column cell={CommandCell} width="100px"/>
                        </KendoGrid>
                    </div>
                </Grid>
            </Grid>
            <Grid item xs={12}></Grid>
            <Grid item xs={2}>
                <Button onClick={() => onClose()} color="error" variant="contained">
                    Close
                </Button>
            </Grid>
        </React.Fragment>
    );
}

export const QuoteItemAdjustmentsListMemo = React.memo(
    QuoteItemAdjustmentsList,
);
