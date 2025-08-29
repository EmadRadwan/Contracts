import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { useCallback, useState } from "react";
import { Grid as KendoGrid, GridCellProps, GridColumn as Column, GridPageChangeEvent, GridSortChangeEvent, GridToolbar } from "@progress/kendo-react-grid";
import { useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import Button from "@mui/material/Button";
import { Grid, Skeleton, Typography } from "@mui/material";
import {CertificateItem} from "../../../app/models/project/certificateItem";
import {setUiCertificateItems} from "../slice/certificateItemsUiSlice";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {certificateSubTotal, nonDeletedCertificateItemsSelector} from "../slice/certificateSelectors";
import {useFetchCertificateItemsQuery} from "../../../app/store/apis/certificateItemsApi";
import {CertificateItemFormMemo} from "../form/CertificateItemForm";


interface Props {
  editMode: number; // 0: view, 1: create, 2: edit CREATED, 3: edit APPROVED, 4: edit COMPLETED
  workEffortId?: string;
}


export default function CertificateItemsList({ editMode, workEffortId }: Props) {
    const initialSort: Array<SortDescriptor> = [{ field: "description", dir: "asc" }];
    const [sort, setSort] = useState(initialSort);
    const initialDataState: State = { skip: 0, take: 4 };
    const [page, setPage] = useState<State>(initialDataState);
    const [show, setShow] = useState(false);
    const [itemEditMode, setItemEditMode] = useState(0);
    const [certificateItem, setCertificateItem] = useState<CertificateItem | undefined>(undefined);
    const dispatch = useAppDispatch();
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "certificate.items.list";
    const subtotal = useAppSelector(certificateSubTotal);

    // REFACTOR: Fetch certificate items
    // Purpose: Load items for the certificate
    // Context: Uses useFetchCertificateItemsQuery from certificateItemsApi
    const { data: certificateItemsData, isFetching, isLoading } = useFetchCertificateItemsQuery(workEffortId || "", {
        skip: !workEffortId,
    });
    const uiCertificateItems: CertificateItem[] = useAppSelector(nonDeletedCertificateItemsSelector);

    // REFACTOR: Handle pagination
    // Purpose: Update page state for grid
    // Context: Matches PurchaseOrderItemsList pageChange
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    // REFACTOR: Handle item selection
    // Purpose: Open form for editing an item using workEffortId
    // Context: Updated to use workEffortId instead of itemId
    const handleSelectCertificateItem = useCallback(
        (workEffortId: string) => {
            const selectedCertificateItem = uiCertificateItems.find((item) => item.workEffortId === workEffortId);
            setCertificateItem(selectedCertificateItem);
            setItemEditMode(2);
            setShow(true);
        },
        [uiCertificateItems]
    );

    // REFACTOR: Custom cell for description
    // Purpose: Make description clickable to edit item
    // Context: Updated to pass workEffortId
    const descriptionCell = (props: GridCellProps) => (
        <td>
            <Button onClick={() => handleSelectCertificateItem(props.dataItem.workEffortId)}>
                {props.dataItem.description}
            </Button>
        </td>
    );

    // REFACTOR: Custom cell for delete button
    // Purpose: Allow soft deletion of items
    // Context: Uses editMode for disabling
    const DeleteCertificateItemCell = (props: GridCellProps) => (
        <td className="k-command-cell">
            <Button
                className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                disabled={editMode > 3}
                onClick={() => props.remove(props.dataItem)}
                color="error"
            >
                {getTranslatedLabel(`${localizationKey}.remove`, "Remove")}
            </Button>
        </td>
    );

    // REFACTOR: Delete logic
    // Purpose: Mark item as deleted using workEffortId
    // Context: Updated to use workEffortId
    const remove = useCallback(
        (dataItem: CertificateItem) => {
            const newCertificateItems = uiCertificateItems.map((item) =>
                item.workEffortId === dataItem.workEffortId ? { ...item, isDeleted: true } : item
            );
            dispatch(setUiCertificateItems(newCertificateItems));
        },
        [uiCertificateItems, dispatch]
    );

    // REFACTOR: Command cell
    // Purpose: Render delete button
    // Context: Mirrors CommandCell
    const CommandCell = (props: GridCellProps) => <DeleteCertificateItemCell {...props} remove={remove} />;

    // REFACTOR: Update items
    // Purpose: Add or update items in Redux store using workEffortId
    // Context: Updated to use workEffortId for updates
    const updateCertificateItems = useCallback(
        (certificateItem: CertificateItem, editMode: number) => {
            let newCertificateItems: CertificateItem[];
            try {
                if (editMode === 1) {
                    // Add new item
                    newCertificateItems = uiCertificateItems ? [...uiCertificateItems, certificateItem] : [certificateItem];
                } else {
                    // Update existing item
                    newCertificateItems = uiCertificateItems.map((item) =>
                        item.workEffortId === certificateItem.workEffortId ? certificateItem : item
                    );
                }
                dispatch(setUiCertificateItems(newCertificateItems));
            } catch (e) {
                console.error("Error updating certificate items:", e);
            }
        },
        [uiCertificateItems, dispatch]
    );

    // REFACTOR: Close modal
    // Purpose: Hide form modal
    // Context: Mirrors memoizedOnClose
    const memoizedOnClose = useCallback(() => {
        setShow(false);
    }, []);

    return (
        <>
            {show && (
                <ModalContainer show={show} onClose={memoizedOnClose} width={700}>
                    <CertificateItemFormMemo
                        certificateItem={certificateItem}
                        editMode={itemEditMode}
                        onClose={memoizedOnClose}
                        formEditMode={editMode}
                        updateCertificateItems={updateCertificateItems}
                    />
                </ModalContainer>
            )}
            <Grid container columnSpacing={1} direction="column" alignItems="flex-start" sx={{ mt: 1 }}>
                <Grid container>
                    {(isFetching || isLoading) ? (
                        <Grid container spacing={2} direction="column">
                            <Grid item>
                                <Skeleton animation="wave" variant="rounded" height={40} sx={{ width: "70%" }} />
                            </Grid>
                            <Grid item>
                                <Skeleton animation="wave" variant="rounded" height={40} sx={{ width: "70%" }} />
                            </Grid>
                            <Grid item>
                                <Skeleton animation="wave" variant="rounded" height={40} sx={{ width: "70%" }} />
                            </Grid>
                        </Grid>
                    ) : (
                        <Grid item xs={12}>
                            <KendoGrid
                                className="main-grid"
                                style={{ height: "30vh" }}
                                data={orderBy(uiCertificateItems || [], sort).slice(page.skip, page.take + page.skip)}
                                sortable
                                sort={sort}
                                onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                                skip={page.skip}
                                take={page.take}
                                total={uiCertificateItems?.length || 0}
                                pageable
                                onPageChange={pageChange}
                            >
                                <GridToolbar>
                                    <Grid container justifyContent="space-between">
                                        <Grid item>
                                            <Button
                                                color="secondary"
                                                onClick={() => {
                                                    setCertificateItem(undefined);
                                                    setItemEditMode(1);
                                                    setShow(true);
                                                }}
                                                variant="outlined"
                                                disabled={editMode > 3}
                                            >
                                                {getTranslatedLabel(`${localizationKey}.addItem`, "Add Item")}
                                            </Button>
                                        </Grid>
                                        <Grid item>
                                            <Typography>
                                                {getTranslatedLabel(`${localizationKey}.subtotal`, "Subtotal")}: {subtotal.toFixed(2)}
                                            </Typography>
                                        </Grid>
                                    </Grid>
                                </GridToolbar>
                                <Column field="description" title={getTranslatedLabel(`${localizationKey}.description`, "Description")} cell={descriptionCell} width={280} />
                                <Column field="quantity" title={getTranslatedLabel(`${localizationKey}.quantity`, "Quantity")} />
                                <Column field="unitPrice" title={getTranslatedLabel(`${localizationKey}.unitPrice`, "Unit Price")} format="{0:n2}" />
                                <Column field="totalAmount" title={getTranslatedLabel(`${localizationKey}.totalAmount`, "Total Amount")} format="{0:n2}" />
                                <Column field="completionPercentage" title={getTranslatedLabel(`${localizationKey}.completionPercentage`, "Completion %")} format="{0:n0}" />
                                <Column cell={CommandCell} />
                            </KendoGrid>
                        </Grid>
                    )}
                </Grid>
            </Grid>
        </>
    );
}

// REFACTOR: Memoize component
// Purpose: Optimize performance by preventing unnecessary re-renders
// Context: Matches PurchaseOrderItemsListMemo
export const CertificateItemsListMemo = React.memo(CertificateItemsList);