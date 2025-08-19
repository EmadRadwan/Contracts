import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import React, {useCallback, useState} from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import Button from "@mui/material/Button";
import {Grid} from "@mui/material";
import {useSelector} from "react-redux";
import { useFetchInvoiceItemsQuery,} from "../../../../app/store/configureStore";
import {InvoiceItem} from "../../../../app/models/accounting/invoiceItem";
import {nonDeletedInvoiceItemsSelector} from "../slice/invoiceSelectors";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import EditInvoiceItem from "../form/EditInvoiceItem";

interface Props {
    invoiceId: string | undefined
    canEdit: boolean
}

export default function InvoiceItemsList({invoiceId, canEdit}: Props) {
    const initialSort: Array<SortDescriptor> = [
        {field: "invoiceItemSeqId", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    const [invoiceItem, setInvoiceItem] = useState<InvoiceItem | undefined>(undefined);
    const uiInvoiceItems: any = useSelector(nonDeletedInvoiceItemsSelector);
    const [editMode, setEditMode] = useState(0)
    const [show, setShow] = useState(false)
    
    const {data: invoiceItemsData} = useFetchInvoiceItemsQuery(invoiceId,
        {skip: invoiceId === undefined});

    const handleSelectInvoiceItem = useCallback(
        (dataItem: InvoiceItem) => {
            // Purpose: Disables edit functionality when canEdit is false
            // Improvement: Aligns with Add Invoice Item button permissions
            if (!canEdit) return;

            console.log('Selected Invoice Item:', dataItem);
            const selectedItem = uiInvoiceItems.find(
                (item: InvoiceItem) =>
                    item.invoiceId === dataItem.invoiceId && item.invoiceItemSeqId === dataItem.invoiceItemSeqId
            );
            console.log('Found Selected Item:', selectedItem);

            const invoiceItemToDisplay: InvoiceItem = {
                ...selectedItem,
                productId: selectedItem?.invoiceItemProduct || selectedItem?.productId,
            };
            setInvoiceItem(invoiceItemToDisplay);
            setEditMode(2);
            setShow(true);
        },
        [uiInvoiceItems, canEdit]
    );

    const handleCloseModal = useCallback(() => {
        setShow(false);
        setInvoiceItem(undefined);
        setEditMode(0);
    }, []);

    const memoizedOnClose = useCallback(
        () => {
            setShow(false)
        },
        [],
    );

    const invoiceItemCell = (props: any) => {
        // Purpose: Prevents editing of invoice items when not allowed
        // Improvement: Provides consistent UX with Add Invoice Item button
        return (
            <td>
                <Button
                    onClick={() => handleSelectInvoiceItem(props.dataItem)}
                    disabled={!canEdit}
                >
                    {props.dataItem.productName}
                </Button>
            </td>
        );
    };

    if (!invoiceId) {
        return <div>No invoice selected</div>;
    }
    
    return (
        <>
        {show && (<ModalContainer show={show} onClose={memoizedOnClose} width={800}>
            <EditInvoiceItem
                invoiceItem={invoiceItem}
                editMode={editMode}
                onClose={handleCloseModal}
                invoiceId={invoiceId}
            />
            </ModalContainer>)}
            <Grid container columnSpacing={1} direction={"column"} alignItems="flex-start">

                <KendoGrid className="main-grid" style={{height: "40vh"}}
                           data={orderBy(uiInvoiceItems ? uiInvoiceItems : [], sort).slice(page.skip, page.take + page.skip)}
                           sortable={true}
                           sort={sort}
                           onSortChange={(e: GridSortChangeEvent) => {
                               setSort(e.sort);
                           }}
                           skip={page.skip}
                           resizable={true}
                           take={page.take}
                           total={uiInvoiceItems ? uiInvoiceItems.length : 0}
                           pageable={true}
                           onPageChange={pageChange}
                >
                    <GridToolbar>
                        <Grid item xs={4}>
                            <Button
                                onClick={() => {
                                    // Purpose: Ensures modal only opens when editing is allowed
                                    // Improvement: Consistent with edit button behavior
                                    if (!canEdit) return;
                                    setEditMode(1);
                                    setShow(true);
                                    setInvoiceItem(undefined);
                                }}
                                variant="outlined"
                                disabled={!canEdit}
                                color="secondary"
                            >
                                Add Invoice Item
                            </Button>
                        </Grid>
                    </GridToolbar>

                    <Column field="productName" title="Product" cell={invoiceItemCell} width={250}/>
                    <Column field="invoiceId" title="invoiceId" width={0}/>
                    <Column field="invoiceItemSeqId" title="invoiceItemSeqId" width={0}/>
                    <Column field="invoiceItemTypeDescription" title="Item Type Description" width={230}/>
                    <Column field="amount" title="Amount" width={100}/>
                    <Column field="quantity" title="Quantity" width={100}/>
                    <Column field="description" title="Description" width={200}/>

                </KendoGrid>
            </Grid>
        </>

    )
}