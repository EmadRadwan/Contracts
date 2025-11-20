import {orderBy, SortDescriptor, State, process} from '@progress/kendo-data-query';
import React, {useCallback, useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import {useAppDispatch, useAppSelector} from "../../../app/store/configureStore";
import {Button, Grid, Skeleton, Typography} from "@mui/material";
import {CertificateItem} from "../../../app/models/project/certificateItem";
import {
    resetUiCertificateItems,
    setUiCertificateItems,
    setUiCertificateItemsFromApi
} from "../slice/certificateItemsUiSlice";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {
    certificateSubTotal,
    displayCertificateItemsSelector,
    nonDeletedCertificateItemsSelector
} from "../slice/certificateSelectors";
import {useFetchCertificateItemsQuery} from "../../../app/store/apis/certificateItemsApi";
import {CertificateItemFormMemo} from "../form/CertificateItemForm";


interface Props {
    editMode: number; // 0: view, 1: create, 2: edit CREATED, 3: edit APPROVED, 4: edit COMPLETED
    workEffortId?: string;
    isFormCollapsed: boolean;
}

export default function CertificateItemsList({editMode, workEffortId, isFormCollapsed}: Props) {
    const initialSort: Array<SortDescriptor> = [{field: "productName", dir: "asc"}];
    const [sort, setSort] = useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = useState<State>(initialDataState);
    const [show, setShow] = useState(false);
    const [itemEditMode, setItemEditMode] = useState(0);
    const [certificateItem, setCertificateItem] = useState<CertificateItem | undefined>(undefined);
    const dispatch = useAppDispatch();
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "certificate.items.list";
    const subtotal = useAppSelector(certificateSubTotal);
    const {currentCertificateType} = useAppSelector((state) => state.certificateUi);
    const {data: certificateItemsData, isFetching, isLoading} = useFetchCertificateItemsQuery(workEffortId || "", {
        skip: !workEffortId,
    });
    const uiCertificateItems: CertificateItem[] = useAppSelector(displayCertificateItemsSelector);
    const nonDeletedItems = useAppSelector(nonDeletedCertificateItemsSelector);

    console.log('certificateItemsData', certificateItemsData)
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    useEffect(() => {
        if (certificateItemsData && !isFetching && !isLoading) {
            dispatch(setUiCertificateItemsFromApi(certificateItemsData));
        }
    }, [certificateItemsData, isFetching, isLoading, dispatch]);
    

    const handleSelectCertificateItem = useCallback(
        (workEffortId: string) => {
            const selectedCertificateItem = uiCertificateItems.find((item) => item.workEffortId === workEffortId);
            if (!selectedCertificateItem) return;
            const certificateItem: CertificateItem = {
                ...selectedCertificateItem,
                productId: {
                    ProductId: selectedCertificateItem.productId,
                    ProductName: selectedCertificateItem.productName || "",
                    ProductType: "",
                },
                uomId: {
                    UomId: selectedCertificateItem.uomId,
                    Description: selectedCertificateItem.uomName || "",
                },
                gratuities: selectedCertificateItem.gratuities || 0,
                transportationExpenses: selectedCertificateItem.transportationExpenses || 0,
                discount: selectedCertificateItem.discount || 0,
                unitPrice: selectedCertificateItem.unitPrice || 0,
                procurementDate: selectedCertificateItem.procurementDate
                    ? new Date(selectedCertificateItem.procurementDate)
                    : new Date(),
            };
            setCertificateItem(certificateItem);
            setItemEditMode(2);
            setShow(true);
            console.log('selectedCertificateItem', selectedCertificateItem);
            console.log('certificateItem', certificateItem);
        },
        [uiCertificateItems]
    );


    const descriptionCell = (props: GridCellProps) => (
        <td>
            <Button onClick={() => handleSelectCertificateItem(props.dataItem.workEffortId)}>
                {props.dataItem.productName}
            </Button>
        </td>
    );

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

    const remove = useCallback(
        (dataItem: CertificateItem, nonDeletedItems: CertificateItem[]) => {
            const newCertificateItems = nonDeletedItems.map((item) =>
                item.workEffortId === dataItem.workEffortId ? { ...item, isDeleted: true } : item
            );
            dispatch(setUiCertificateItems(newCertificateItems));
        },
        [dispatch]
    );

    const CommandCell = (props: GridCellProps) => (
        <DeleteCertificateItemCell {...props} remove={(dataItem: CertificateItem) => remove(dataItem, nonDeletedItems)} />
    );
    


    const memoizedOnClose = useCallback(() => {
        setShow(false);
    }, []);

    const modalWidth = 700;


    const isSupplyWithDiscount = ["SUPPLY_PROCUREMENT_CERTIFICATE"].includes(currentCertificateType);
    const isSupplyWithoutDiscount = ["COMPANY_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType);

    const dataWithSummaries = uiCertificateItems ? (() => {
        const groupedByProductId: { [key: string]: CertificateItem[] } = uiCertificateItems.reduce(
            (acc, item) => {
                const productId = item.productId || '';
                if (!acc[productId]) acc[productId] = [];
                acc[productId].push(item);
                return acc;
            },
            {} as { [key: string]: CertificateItem[] }
        );

        const sortedData = orderBy(uiCertificateItems, sort);
        let result: any[] = [];
        Object.keys(groupedByProductId).forEach((productId) => {
            const groupItems = groupedByProductId[productId];
            // REFACTOR: Calculate productSubtotal for each group
            // Purpose: Sum net values for items with the same productId to display in code column
            // Improvement: Ensures subtotal is available for last item in group, matching CertificateItemsListGrouped
            const subtotal = groupItems.reduce((sum: number, item: CertificateItem) => sum + (item.displayTotal || 0), 0);
            const sortedGroupItems = sortedData.filter((item) => item.productId === productId);
            const updatedGroupItems = sortedGroupItems.map((item, index) => ({
                ...item,
                isLastInGroup: index === sortedGroupItems.length - 1,
                productSubtotal: subtotal.toFixed(2), // Add productSubtotal to each item
            }));
            result = [...result, ...updatedGroupItems];
        });

        const pagedData = process(result, {skip: page.skip, take: page.take});
        return pagedData.data;
    })() : [];

    const columns = [
        {
            field: 'code',
            title: getTranslatedLabel(`${localizationKey}.code`, 'Code'),
            width: 250,
            // REFACTOR: Enforce LTR rendering with stronger specificity
            // Purpose: Ensure productId/serial displays before subtotal in RTL mode
            // Improvement: Uses span with inline LTR direction and !important to override page-level RTL
            cell: (props: GridCellProps) => (
                <td style={{direction: 'ltr !important', textAlign: 'left !important'}}>
        <span style={{direction: 'ltr !important'}}>
          {props.dataItem.isLastInGroup && props.dataItem.productSubtotal !== undefined
              ? `${props.dataItem.code} (${getTranslatedLabel(`${localizationKey}.productSubtotal`, 'Subtotal')}: ${props.dataItem.productSubtotal})`
              : props.dataItem.code}
        </span>
                </td>
            ),
        },
        {
            field: "productName",
            title: getTranslatedLabel(`${localizationKey}.description`, "Product"),
            cell: descriptionCell,
            width: 280,
        },
        {
            field: "description",
            title: getTranslatedLabel(`${localizationKey}.description`, "Description"),
            width: 280,
        },
        {
            field: "quantity",
            title: getTranslatedLabel(`${localizationKey}.quantity`, "Quantity"),
            width: 100,
        },
        {
            field: "unitPrice",
            title: getTranslatedLabel(`${localizationKey}.unitPrice`, "Unit Price"),
            format: "{0:n2}",
            width: 120,
        },
        {
            field: "displayTotal",
            title: getTranslatedLabel(`${localizationKey}.totalAmount`, "Total Amount"),
            format: "{0:n2}",
            width: 130,
        },
        ...(isSupplyWithDiscount
            ? [
                {
                    field: "discount",
                    title: getTranslatedLabel(`${localizationKey}.discount`, "Discount"),
                    format: "{0:n2}",
                    width: 120,
                },
                {
                    field: "formattedProcurementDate",
                    title: getTranslatedLabel(`${localizationKey}.procurementDate`, "Procurement Date"),
                    width: 180,
                },
                {
                    field: "transportationExpenses",
                    title: getTranslatedLabel(`${localizationKey}.transportationExpenses`, "Transportation Expenses"),
                    format: "{0:n2}",
                    width: 180,
                },
                {
                    field: "gratuities",
                    title: getTranslatedLabel(`${localizationKey}.gratuities`, "Gratuities"),
                    format: "{0:n2}",
                    width: 120,
                },
            ]
            : []),
        ...(isSupplyWithoutDiscount
            ? [
                {
                    field: "formattedProcurementDate",
                    title: getTranslatedLabel(`${localizationKey}.procurementDate`, "Procurement Date"),
                    width: 180,
                },
                {
                    field: "transportationExpenses",
                    title: getTranslatedLabel(`${localizationKey}.transportationExpenses`, "Transportation Expenses"),
                    format: "{0:n2}",
                    width: 180,
                },
                {
                    field: "gratuities",
                    title: getTranslatedLabel(`${localizationKey}.gratuities`, "Gratuities"),
                    format: "{0:n2}",
                    width: 120,
                },
            ]
            : []),
        {
            cell: CommandCell,
            width: 100,
        },
    ];

    const sortedData = orderBy(uiCertificateItems || [], sort);
    const pagedData = sortedData.slice(page.skip, page.skip + page.take);


    return (
        <>
            {show && (
                <ModalContainer show={show} onClose={memoizedOnClose} width={modalWidth}>
                    <CertificateItemFormMemo
                        certificateItem={certificateItem}
                        editMode={itemEditMode}
                        onClose={memoizedOnClose}
                        formEditMode={editMode}
                    />
                </ModalContainer>
            )}
            <Grid container columnSpacing={1} direction="column" alignItems="flex-start" sx={{mt: 1}}>
                <Grid container>
                    {(isFetching || isLoading) ? (
                        <Grid container spacing={2} direction="column">
                            <Grid item>
                                <Skeleton animation="wave" variant="rounded" height={40} sx={{width: "70%"}}/>
                            </Grid>
                            <Grid item>
                                <Skeleton animation="wave" variant="rounded" height={40} sx={{width: "70%"}}/>
                            </Grid>
                            <Grid item>
                                <Skeleton animation="wave" variant="rounded" height={40} sx={{width: "70%"}}/>
                            </Grid>
                        </Grid>
                    ) : (
                        <Grid item xs={12}>
                            <KendoGrid
                                className="main-grid"
                                style={{height: isFormCollapsed ? "60vh" : "30vh"}}
                                data={dataWithSummaries}
                                sortable
                                scrollable="scrollable"
                                resizable={true}
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
                                                {getTranslatedLabel(`${localizationKey}.Total`, "Total")}: {subtotal.toFixed(2)}
                                            </Typography>
                                        </Grid>
                                    </Grid>
                                </GridToolbar>
                                {columns.map((column, index) => (
                                    <Column key={index} {...column} />
                                ))}
                            </KendoGrid>
                        </Grid>
                    )}
                </Grid>
            </Grid>
        </>
    );
}

export const CertificateItemsListMemo = React.memo(CertificateItemsList);