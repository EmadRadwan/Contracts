import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {Fragment, useCallback, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import {Grid} from "@mui/material";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import {useSelector} from "react-redux";
import QuoteAdjustmentForm from "../../form/quote/QuoteAdjustmentForm";
import {quoteAdjustmentsSelector, quoteLevelAdjustments} from "../../slice/quoteSelectors";
import {setUiQuoteAdjustments} from "../../slice/quoteAdjustmentsUiSlice";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import {QuoteAdjustment} from "../../../../app/models/order/quoteAdjustment";

interface Props {
    onClose: () => void;
}


export default function QuoteAdjustmentsList({onClose}: Props) {
    const initialSort: Array<SortDescriptor> = [
        {field: "partyId", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    const [show, setShow] = useState(false);
    const [quoteAdjustment, setQuoteAdjusment] = useState<QuoteAdjustment | undefined>(undefined);
    const uiQuoteLevelAdjustments: any = useSelector(quoteLevelAdjustments);
    const uiQuoteAdjustments: any = useSelector(quoteAdjustmentsSelector);


    const quoteFormEditMode: any = useAppSelector(state => state.quotesUi.quoteFormEditMode);

    const dispatch = useAppDispatch();


    const [editMode, setEditMode] = useState(0);
    const {user} = useAppSelector(state => state.account);
    const roleWithPercentage = user!.roles!.find(role => role.Name === 'AddAdjustments');


    console.count('QuoteAdjustmentsList.tsx Rendered');

    /*console.table({
        'showList - prop': showList,
        'onClose - prop': onClose,
        'quoteId - prop': quoteId,
        'show - state': show,
        'quoteAdjustment - state': quoteAdjustment,
        'quoteAdjustments - state': quoteAdjustments,
        'quoteAdjustmentsFromUi - selector': quoteAdjustmentsFromUi,
        'editMode - state': editMode,
        'allAdjustments - query': allAdjustments,
    });*/

    function handleSelectQuoteAdjustment(quoteAdjustmentId: string) {
        // select quote adjustment from quoteAdjustments list
        const quoteAdjustment = uiQuoteLevelAdjustments!.find((adjustment: QuoteAdjustment) => adjustment.quoteAdjustmentId === quoteAdjustmentId)


        setQuoteAdjusment(quoteAdjustment)


        setEditMode(2);
        setShow(true);
    }


    const DeleteQuoteItemAdjustmentCell = (props: any) => {
        const {dataItem} = props;

        return (
            <td className="k-command-cell">
                <Button
                    className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                    onClick={() =>
                        props.remove(dataItem)
                    }
                    disabled={dataItem.isManual === 'N'}
                    color="error"
                >
                    Remove
                </Button>
            </td>
        )
    };
    const remove = (dataItem: QuoteAdjustment) => {
        const newNonDeletedQuoteAdjustments = uiQuoteAdjustments?.map((item: QuoteAdjustment) => {
            if (item.quoteAdjustmentId === dataItem?.quoteAdjustmentId) {
                return {...item, isAdjustmentDeleted: true};
            } else {
                return item;
            }
        });
        dispatch(setUiQuoteAdjustments(newNonDeletedQuoteAdjustments));
    };

    const CommandCell = (props: GridCellProps) => (
        <DeleteQuoteItemAdjustmentCell
            {...props}
            remove={remove}
        />
    )

    const quoteAdjustmentCell = (props: any) => {

        return (props.dataItem.isManual === 'Y' ?
                <td>
                    <Button
                        onClick={() => handleSelectQuoteAdjustment(props.dataItem.quoteAdjustmentId)}
                    >
                        {props.dataItem.quoteAdjustmentTypeDescription}
                    </Button>
                </td>
                :
                <td>
                    {props.dataItem.quoteAdjustmentTypeDescription}
                </td>
        )
    };

    const memoizedOnClose = useCallback(
        () => {
            setShow(false)
        },
        [],
    );


    ////console.log('quoteItems', quoteAdjustments);
    ////console.log('nonDeletedQuoteAdjustments', nonDeletedQuoteAdjustments);

    return <Fragment>
        {show && (<ModalContainer show={show} onClose={memoizedOnClose} width={500}>
            <QuoteAdjustmentForm quoteAdjustment={quoteAdjustment} editMode={editMode}
                                 onClose={memoizedOnClose}/>
        </ModalContainer>)}
        <Grid container padding={2} columnSpacing={1}>
            <Grid container alignItems="center">
                <Grid item xs={6}>
                    {roleWithPercentage && <Button disabled={quoteFormEditMode > 2} color={"secondary"} onClick={() => {
                        setEditMode(1);
                        setShow(true);
                    }} variant="outlined">
                        Add Quote Adjustment
                    </Button>}
                </Grid>
            </Grid>
            <Grid container>
                <div className="div-container">
                    <KendoGrid className="main-grid" style={{height: "300px"}}
                               data={orderBy(uiQuoteLevelAdjustments ? uiQuoteLevelAdjustments : [], sort).slice(page.skip, page.take + page.skip)}
                               sortable={true}
                               sort={sort}
                               onSortChange={(e: GridSortChangeEvent) => {
                                   setSort(e.sort);
                               }}
                               skip={page.skip}
                               take={page.take}
                               total={uiQuoteLevelAdjustments ? uiQuoteLevelAdjustments.length : 0}
                               pageable={true}
                               onPageChange={pageChange}

                    >

                        <Column field="quoteAdjustmentTypeDescription" title="Adjustment Type"
                                cell={quoteAdjustmentCell} width={140}/>
                        <Column field="quoteId" title="quoteId" width={0}/>
                        <Column field="quoteItemSeqId" title="quoteItemSeqId" width={0}/>
                        <Column field="amount" title="Amount" width={130}/>
                        <Column field="description" title="Description" width={140}/>
                        <Column field="sourcePercentage" title="Percentage" width={150}/>
                        <Column field="isManual" title="User Entered" width={110}/>
                        <Column cell={CommandCell} width="100px"/>

                    </KendoGrid>
                </div>
            </Grid>


        </Grid>
        <Grid item xs={2}>
            <Button onClick={() => onClose()} color="error" variant="contained">
                Close
            </Button>
        </Grid>
    </Fragment>

}
