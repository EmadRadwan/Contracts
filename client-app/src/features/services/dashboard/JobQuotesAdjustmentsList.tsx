import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import {Grid, Paper} from "@mui/material";
import {useSelector} from "react-redux";
import {QuoteAdjustment} from "../../../app/models/order/quoteAdjustment";
import {quoteAdjustmentsSelector, quoteLevelAdjustments, setUiQuoteAdjustments} from "../slice/quoteUiSlice";
import {useAppDispatch, useAppSelector} from "../../../app/store/configureStore";
import JobQuoteAdjustmentForm from "../form/JobQuoteAdjustmentForm";

interface Props {
    showList: boolean;
    onClose: () => void;
    quoteId?: string;
    width?: number;
}

export default function JobQuoteAdjustmentsList({showList, onClose, quoteId, width}: Props) {
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
    const [jobQuoteAdjustment, setJobQuoteAdjusment] = useState<QuoteAdjustment | undefined>(undefined);
    const uiJobQuoteLevelAdjustments: any = useSelector(quoteLevelAdjustments)
    const uiJobQuoteAdjustments: any = useSelector(quoteAdjustmentsSelector)

    const dispatch = useAppDispatch();


    const [editMode, setEditMode] = useState(0);
    const {user} = useAppSelector(state => state.account);
    const roleWithPercentage = user!.roles!.find(role => role.Name === 'AddAdjustments');


    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, []);


    function handleSelectJobQuoteAdjustment(quoteAdjustmentId: string) {
        // select jobQuote adjustment from jobQuoteAdjustments list
        const jobQuoteAdjustment = uiJobQuoteLevelAdjustments!.find((adjustment: QuoteAdjustment) => adjustment.quoteAdjustmentId === quoteAdjustmentId)


        setJobQuoteAdjusment(jobQuoteAdjustment)


        setEditMode(2);
        setShow(true);
    }


    const DeleteJobQuoteItemAdjustmentCell = (props: any) => {
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
        const newNonDeletedJobQuoteAdjustments = uiJobQuoteAdjustments?.map((item: QuoteAdjustment) => {
            if (item.quoteAdjustmentId === dataItem?.quoteAdjustmentId) {
                return {...item, isAdjustmentDeleted: true};
            } else {
                return item;
            }
        });
        dispatch(setUiQuoteAdjustments(newNonDeletedJobQuoteAdjustments));
    };

    const CommandCell = (props: GridCellProps) => (
        <DeleteJobQuoteItemAdjustmentCell
            {...props}
            remove={remove}
        />
    )

    const JobQuoteAdjustmentCell = (props: any) => {

        return (props.dataItem.isManual === 'Y' ?
                <td>
                    <Button
                        onClick={() => handleSelectJobQuoteAdjustment(props.dataItem.jobQuoteAdjustmentId)}
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

    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };


    return ReactDOM.createPortal(
        <CSSTransition
            in={showList}
            unmountOnExit
            timeout={{enter: 0, exit: 300}}
        >
            <div className="modal">
                <div className="modal-content" style={{width: width}} onClick={e => e.stopPropagation()}>
                    <Paper elevation={5} className={`div-container-withBorderCurved`}>
                        <JobQuoteAdjustmentForm jobQuoteAdjustment={jobQuoteAdjustment} editMode={editMode}
                                                onClose={() => setShow(false)}
                                                show={show} width={400}/>
                        <Grid container columnSpacing={1} p={2}>
                            <Grid container alignItems="center">
                                <Grid item xs={6}>
                                    {roleWithPercentage && <Button color={"secondary"} onClick={() => {
                                        setEditMode(1);
                                        setShow(true);
                                    }} variant="outlined">
                                        Add Adjustment
                                    </Button>}
                                </Grid>
                            </Grid>
                            <Grid container>
                                <div className="div-container">
                                    <KendoGrid className="main-grid" style={{height: "300px"}}
                                               data={orderBy(uiJobQuoteLevelAdjustments ? uiJobQuoteLevelAdjustments : [], sort).slice(page.skip, page.take + page.skip)}
                                               sortable={true}
                                               sort={sort}
                                               onSortChange={(e: GridSortChangeEvent) => {
                                                   setSort(e.sort);
                                               }}
                                               skip={page.skip}
                                               take={page.take}
                                               total={uiJobQuoteLevelAdjustments ? uiJobQuoteLevelAdjustments.length : 0}
                                               pageable={true}
                                               onPageChange={pageChange}

                                    >

                                        <Column field="quoteAdjustmentTypeDescription" title="Adjustment Type"
                                                cell={JobQuoteAdjustmentCell} width={150}/>
                                        <Column field="quoteId" title="jobQuoteId" width={0}/>
                                        <Column field="quoteItemSeqId" title="jobQuoteItemSeqId" width={0}/>
                                        <Column field="amount" title="Amount" width={130}/>
                                        <Column field="sourcePercentage" title="Percentage" width={140}/>
                                        <Column field="description" title="Description" width={140}/>
                                        <Column cell={CommandCell} width="60px"/>

                                    </KendoGrid>
                                </div>
                            </Grid>


                        </Grid>
                        <Grid item xs={2}>
                            <Button onClick={() => onClose()} color="error" variant="contained">
                                Close
                            </Button>
                        </Grid>
                    </Paper>

                </div>
            </div>
        </CSSTransition>,
        document.getElementById("root")!
    );
}
