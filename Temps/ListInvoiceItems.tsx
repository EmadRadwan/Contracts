import React, {useCallback, useState} from "react";
import {useAppSelector, useAppDispatch, useFetchInvoiceItemsQuery} from "../../../../app/store/configureStore";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import {Grid, Button, Box} from "@mui/material";
import {nonDeletedInvoiceItemsSelector} from "../slice/invoiceSelectors";
import {InvoiceItem} from "../../../../app/models/accounting/invoiceItem";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import AddInvoiceItem from "../form/AddInvoiceItem";

interface Props {
    invoiceId: string | undefined;
    canEdit: boolean;
}

const ListInvoiceItems: React.FC<Props> = ({invoiceId, canEdit}) => {
    const initialSort: Array<SortDescriptor> = [{field: "invoiceItemSeqId", dir: "desc"}];
    const [sort, setSort] = useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = useState<State>(initialDataState);
    const [showModal, setShowModal] = useState(false);
    const [selectedInvoiceItem, setSelectedInvoiceItem] = useState<InvoiceItem | undefined>(undefined);
    const [editMode, setEditMode] = useState(0); // 0: none, 1: create, 2: edit
    const uiInvoiceItems = useAppSelector(nonDeletedInvoiceItemsSelector);
    const dispatch = useAppDispatch();

    const {data: invoiceItemsData} = useFetchInvoiceItemsQuery(invoiceId, {
        skip: invoiceId === undefined,
    });

    // REFACTOR: Added sorting and pagination to match OFBiz ListInvoiceItems
    // Purpose: Replicates OFBiz entity-condition with order-by invoiceItemSeqId
    // Improvement: Ensures consistent grid behavior with existing InvoiceItemsList
    const handlePageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    const handleSortChange = (event: GridSortChangeEvent) => {
        setSort(event.sort);
    };

    const handleSelectInvoiceItem = useCallback(
        (item: InvoiceItem) => {
            const selectedItem = uiInvoiceItems.find(
                (i: InvoiceItem) =>
                    i.invoiceId.concat(i.invoiceItemSeqId) === item.invoiceId.concat(item.invoiceItemSeqId)
            );
            const invoiceItemToDisplay = selectedItem?.invoiceItemProduct
                ? {...selectedItem, productId: selectedItem.invoiceItemProduct}
                : {...selectedItem};
            setSelectedInvoiceItem(invoiceItemToDisplay);
            setEditMode(2);
            setShowModal(true);
        },
        [uiInvoiceItems]
    );

    const handleAddInvoiceItem = useCallback(() => {
        setSelectedInvoiceItem(undefined);
        setEditMode(1);
        setShowModal(true);
    }, []);

    const handleCloseModal = useCallback(() => {
        setShowModal(false);
        setEditMode(0);
        setSelectedInvoiceItem(undefined);
    }, []);

    const invoiceItemCell = (props: any) => (
        <td>
            <Button onClick={() => handleSelectInvoiceItem(props.dataItem)}>
                {props.dataItem.productName || "Select"}
            </Button>
        </td>
    );

    return (
        <Box p={2}>
            {showModal && (
                <ModalContainer show={showModal} onClose={handleCloseModal} width={800}>
                    <AddInvoiceItem
                        invoiceItem={selectedInvoiceItem}
                        editMode={editMode}
                        onClose={handleCloseModal}
                        invoiceId={invoiceId}
                    />
                </ModalContainer>
            )}
            <Grid container direction="column" alignItems="flex-start">
                <KendoGrid
                    className="main-grid"
                    style={{height: "40vh"}}
                    data={orderBy(uiInvoiceItems || [], sort).slice(page.skip, page.take + page.skip)}
                    sortable
                    sort={sort}
                    onSortChange={handleSortChange}
                    skip={page.skip}
                    take={page.take}
                    total={uiInvoiceItems?.length || 0}
                    pageable
                    onPageChange={handlePageChange}
                    resizable
                >
                    <GridToolbar>
                        <Grid item xs={4}>
                            <Button
                                onClick={handleAddInvoiceItem}
                                variant="outlined"
                                disabled={!canEdit}
                                color="secondary"
                            >
                                Add Invoice Item
                            </Button>
                        </Grid>
                    </GridToolbar>
                    <Column field="productName" title="Product" cell={invoiceItemCell} width={250}/>
                    <Column field="invoiceId" title="Invoice ID" width={0}/>
                    <Column field="invoiceItemSeqId" title="Item Seq ID" width={0}/>
                    <Column field="invoiceItemTypeDescription" title="Item Type Description" width={230}/>
                    <Column field="amount" title="Amount" width={100}/>
                    <Column field="quantity" title="Quantity" width={100}/>
                    <Column field="description" title="Description" width={200}/>
                </KendoGrid>
            </Grid>
        </Box>
    );
};

export default ListInvoiceItems;
