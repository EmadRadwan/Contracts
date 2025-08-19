import {
    useAppDispatch,
    useAppSelector,
    useFetchProductionRunsQuery,
} from "../../../app/store/configureStore";
import React, {useCallback, useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GRID_COL_INDEX_ATTRIBUTE,
} from "@progress/kendo-react-grid";
import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import {Button, Grid, Paper} from "@mui/material";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {useTableKeyboardNavigation} from "@progress/kendo-react-data-tools";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {toast} from "react-toastify";
import {
    useFetchProductionRunComponentsForReturnQuery,
    useReceiveProductionRunComponentsMutation,
} from "../../../app/store/apis";
import {handleDatesArray} from "../../../app/util/utils";
import ReturnInventoryItemForm from "../form/ReturnInventoryItemForm";

interface ProductionRunComponentDto {
    productId: string;
    productName: string;
    estimatedQuantity: number;
    workEffortId: string;
    workEffortName: string;
    fromDate: string | null;
    lotId: string | null;
    issuedQuantity: number;
    returnedQuantity: number;
    quantityToReturn?: number;
    includeThisItem?: boolean;
}

interface Props {
    productionRunId?: string | undefined;
}

const ProductionRunReturnedInventoryList = ({productionRunId}: Props) => {
    const dispatch = useAppDispatch();
    const {getTranslatedLabel} = useTranslationHelper();
    const [show, setShow] = useState(false);
    const [selectedItem, setSelectedItem] = useState<ProductionRunComponentDto | null>(null);
    const [returnItemsData, setReturnItemsData] = useState<ProductionRunComponentDto[]>([]);
    const initialSort: Array<SortDescriptor> = [{field: "productName", dir: "asc"}];
    const [sort, setSort] = useState(initialSort);
    const initialDataState: State = {skip: 0, take: 10};
    const [page, setPage] = useState<State>(initialDataState);

    const {jobRunUnderProcessing: selectedProductionRun} = useAppSelector((state) => state.manufacturingSharedUi);
    const {data: productionRuns, isLoading: isProductionRunsLoading} = useFetchProductionRunsQuery(undefined);
    const {data, isLoading} = useFetchProductionRunComponentsForReturnQuery(
        selectedProductionRun?.productionRunId || productionRunId,
        {
            skip: !selectedProductionRun?.productionRunId && !productionRunId,
        }
    );
    const [receiveProductionRunComponents, {isLoading: isMutationLoading}] =
        useReceiveProductionRunComponentsMutation();

    useEffect(() => {
        if (data) {
            const modifiedItems = handleDatesArray(data).map((item: ProductionRunComponentDto) => ({
                ...item,
                quantityToReturn: 0,
                includeThisItem: false,
            }));
            setReturnItemsData(modifiedItems);
        }
    }, [data]);

    
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    const handleSelectItemToReturn = (item: ProductionRunComponentDto) => {
        const selected = returnItemsData.find(
            (i) => i.workEffortId === item.workEffortId && i.productId === item.productId
        );
        if (selected) {
            setSelectedItem(selected);
            setShow(true);
        }
    };

    const memoizedOnClose = useCallback(() => {
        setShow(false);
    }, []);

    const handleSubmit = (data: ProductionRunComponentDto) => {
        setReturnItemsData((prev) =>
            prev.map((item) =>
                item.workEffortId === data.workEffortId && item.productId === data.productId
                    ? {...item, ...data}
                    : item
            )
        );
    };

    const IncludeCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: props.dataItem.includeThisItem ? "green" : "red"}}
                colSpan={props.colSpan}
                role={"gridcell"}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{[GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex}}
                {...navigationAttributes}
            >
                {props.dataItem.includeThisItem ? "Yes" : "No"}
            </td>
        );
    };

    const EditCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                style={{...props.style, color: "blue"}}
                colSpan={props.colSpan}
                role={"gridcell"}
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{[GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex}}
                {...navigationAttributes}
            >
                <Button onClick={() => handleSelectItemToReturn(props.dataItem)}>
                    {getTranslatedLabel("manufacturing.return.edit", "Edit")}
                </Button>
            </td>
        );
    };

    async function handleMenuSelect(e: MenuSelectEvent) {
        if (e.item.text === "Return Materials") {
            handleReturnMaterials();
        }
    }

    async function handleReturnMaterials() {
        const itemsToReturn = returnItemsData.filter(
            (item) => item.includeThisItem && item.quantityToReturn > 0
        );
        const payload = {
            productionRunId: selectedProductionRun?.productionRunId || productionRunId,
            items: itemsToReturn.map((item) => ({
                productId: item.productId,
                workEffortId: item.workEffortId,
                fromDate: item.fromDate,
                lotId: item.lotId, // Can be null
                quantity: item.quantityToReturn,
            })),
        };

        if (payload.items.length > 0) {
            try {
                const res = await receiveProductionRunComponents(payload).unwrap();
                toast.success("Materials returned successfully.");
                setReturnItemsData((prev) =>
                    prev.map((item) => ({...item, includeThisItem: false, quantityToReturn: 0}))
                );
            } catch (e) {
                console.error(e);
                toast.error("Something went wrong while returning materials.");
            }
        } else {
            toast.error("No items were included to be returned, select at least one.");
        }
    }

    return (
        <>
            {show && (
                <ModalContainer show={show} onClose={memoizedOnClose} width={950}>
                    <ReturnInventoryItemForm
                        component={selectedItem!}
                        onClose={memoizedOnClose}
                        handleSubmit={handleSubmit}
                    />
                </ModalContainer>
            )}
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1} alignItems="center">
                    <Grid item xs={1}>
                        <Menu onSelect={handleMenuSelect}>
                            <MenuItem
                                disabled={!returnItemsData || returnItemsData.length === 0}
                                text={getTranslatedLabel("general.actions", "Actions")}
                            >
                                <MenuItem text="Return Materials"/>
                            </MenuItem>
                        </Menu>
                    </Grid>
                    <Grid item xs={12}>
                        <KendoGrid
                            data={orderBy(returnItemsData, sort).slice(page.skip, page.take + page.skip)}
                            sortable={true}
                            sort={sort}
                            onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                            skip={page.skip}
                            take={page.take}
                            total={returnItemsData.length}
                            pageable={true}
                            onPageChange={pageChange}
                            resizable={true}
                            reorderable={true}
                        >
                            
                            <Column
                                field="productName"
                                title={getTranslatedLabel("manufacturing.return.product", "Product")}
                                width={200}
                                cell={(props) => (
                                    <td>{`${props.dataItem.productName}`}</td>
                                )}
                            />
                            <Column
                                field="estimatedQuantity"
                                title={getTranslatedLabel("manufacturing.return.estimatedQuantity", "Estimated Quantity")}
                                width={100}
                            />
                            <Column
                                field="issuedQuantity"
                                title={getTranslatedLabel("manufacturing.return.issuedQuantity", "Issued Quantity")}
                                width={100}
                            />
                            <Column
                                field="returnedQuantity"
                                title={getTranslatedLabel("manufacturing.return.returnedQuantity", "Returned")}
                                width={100}
                            />
                            <Column
                                field="lotId"
                                title={getTranslatedLabel("manufacturing.return.lotId", "Lot ID")}
                                width={100}
                            />
                            <Column
                                field="quantityToReturn"
                                title={getTranslatedLabel("manufacturing.return.quantityToReturn", "Quantity to Return")}
                                width={120}
                            />
                            <Column
                                field="includeThisItem"
                                title={getTranslatedLabel("manufacturing.return.include", "Include")}
                                width={100}
                                cell={IncludeCell}
                            />
                            <Column width={100} cell={EditCell}/>
                        </KendoGrid>
                    </Grid>
                </Grid>
                {(isLoading || isProductionRunsLoading) && (
                    <LoadingComponent message="Loading Components..."/>
                )}
                {isMutationLoading && (
                    <LoadingComponent message="Processing Returns..."/>
                )}
            </Paper>
        </>
    );
};

export default ProductionRunReturnedInventoryList;