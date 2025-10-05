import { orderBy, SortDescriptor, State, process} from "@progress/kendo-data-query";
import React, {useCallback, useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar
} from "@progress/kendo-react-grid";
import { useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import { Button, Grid, Skeleton, Typography } from "@mui/material";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import {
    resetUiCertificateItems,
    setUiCertificateItems,
    setUiCertificateItemsFromApi
} from "../slice/certificateItemsUiSlice";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { certificateSubTotal, displayCertificateItemsSelector, nonDeletedCertificateItemsSelector } from "../slice/certificateSelectors";
import { useFetchCertificateItemsQuery } from "../../../app/store/apis/certificateItemsApi";
import { CertificateItemFormMemo } from "../form/CertificateItemForm";



interface Props {
    editMode: number; // 0: view, 1: create, 2: edit CREATED, 3: edit APPROVED, 4: edit COMPLETED
    workEffortId?: string;
    isFormCollapsed: boolean;
}

export default function CertificateItemsListGrouped({ editMode, workEffortId, isFormCollapsed }: Props) {
    const initialSort: Array<SortDescriptor> = [{ field: "productName", dir: "asc" }];
    const [sort, setSort] = useState(initialSort);
    const initialDataState: State = { skip: 0, take: 4 };
    const [page, setPage] = useState<State>(initialDataState);
    const [show, setShow] = useState(false);
    const [itemEditMode, setItemEditMode] = useState(0);
    const [certificateItem, setCertificateItem] = useState<CertificateItem | undefined>(undefined);
    const dispatch = useAppDispatch();
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "certificate.items.list";
    const total = useAppSelector(certificateSubTotal);
    const certificateItemsEntities = useAppSelector(state => state.certificateItemsUi.certificateItemsEntities);
    const { data: certificateItemsData, isFetching, isLoading, isError, error } = useFetchCertificateItemsQuery(workEffortId || "", {
        skip: !workEffortId,
    });
    const uiCertificateItems: CertificateItem[] = useAppSelector(displayCertificateItemsSelector);
    const nonDeletedItems = useAppSelector(nonDeletedCertificateItemsSelector);

    useEffect(() => {
        if (certificateItemsData && !isFetching && !isLoading) {
            dispatch(setUiCertificateItemsFromApi(certificateItemsData));
        }
    }, [certificateItemsData, isFetching, isLoading, dispatch]);

    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };



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
                materialPrice: selectedCertificateItem.materialPrice || 0, // Use materialPrice directly
                laborPrice: selectedCertificateItem.laborPrice || 0, // Use laborPrice directly
                procurementDate: selectedCertificateItem.procurementDate
                    ? new Date(selectedCertificateItem.procurementDate)
                    : new Date(),
                additionalInsurance: selectedCertificateItem.additionalInsurance || 0,
                deductionDescription: selectedCertificateItem.deductionDescription || "", // Added deductionDescription
            };
            setCertificateItem(certificateItem);
            setItemEditMode(2);
            setShow(true);
            // console.log('selectedCertificateItem', selectedCertificateItem);
            // console.log('certificateItem', certificateItem);
        },
        [uiCertificateItems]
    );

    const deductionDescriptionCell = (props: GridCellProps) => (
        <td>
            {props.dataItem.deductionDescription ? (
                <Button onClick={() => handleSelectCertificateItem(props.dataItem.workEffortId)}>
                    {props.dataItem.deductionDescription.length > 50
                        ? `${props.dataItem.deductionDescription.substring(0, 50)}...`
                        : props.dataItem.deductionDescription}
                </Button>
            ) : (
                "-"
            )}
        </td>
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

    const modalWidth = 1200;

    const columns = [
        {
            field: "code",
            title: getTranslatedLabel(`${localizationKey}.code`, "Code"),
            width: 250,
            cell: (props: GridCellProps) =>
                props.dataItem.isLastInGroup ? (
                    <td>{`${props.dataItem.code} (${getTranslatedLabel(`${localizationKey}.productSubtotal`, "Subtotal")}: ${props.dataItem.productSubtotal})`}</td>
                ) : (
                    <td>{props.dataItem.code}</td>
                ),
        },
        { field: "productName", title: getTranslatedLabel(`${localizationKey}.description`, "Product"), cell: descriptionCell, width: 280 },
        { field: "description", title: getTranslatedLabel(`${localizationKey}.description`, "Description"), width: 280 },
        { field: "quantity", title: getTranslatedLabel(`${localizationKey}.quantity`, "Quantity"), width: 100 },
        { field: "materialPrice", title: getTranslatedLabel(`${localizationKey}.materialPrice`, "Material Price"), format: "{0:n2}", width: 150 },
        { field: "laborPrice", title: getTranslatedLabel(`${localizationKey}.laborPrice`, "Labor Price"), format: "{0:n2}", width: 150 },
        { field: "displayTotal", title: getTranslatedLabel(`${localizationKey}.totalAmount`, "Total Amount"), format: "{0:n2}", width: 130 },
        { field: "deductions", title: getTranslatedLabel(`${localizationKey}.deductions`, "Deductions"), format: "{0:n2}", width: 120 },
        {
            field: "deductionDescription",
            title: getTranslatedLabel(`${localizationKey}.deductionDescription`, "Deduction Description"),
            width: 200, // Set width to accommodate text, adjustable based on UI needs
        },
        { field: "deserved", title: getTranslatedLabel(`${localizationKey}.deserved`, "Deserved"), format: "{0:n2}", width: 120 },
        { field: "insurance", title: getTranslatedLabel(`${localizationKey}.insurance`, "Insurance"), format: "{0:n2}", width: 120 },
        { field: "additionalInsurance", title: getTranslatedLabel(`${localizationKey}.additionalInsurance`, "Additional Insurance"), format: "{0:n2}", width: 140 },
        { field: "net", title: getTranslatedLabel(`${localizationKey}.net`, "Net"), format: "{0:n2}", width: 120 },
        { field: "achievementPercentage", title: getTranslatedLabel(`${localizationKey}.achievementPercentage`, "Achievement %"), format: "{0:n0}", width: 140 },
        { cell: CommandCell, width: 100 },
    ];


    const dataWithSummaries = uiCertificateItems
        ? (() => {
            const groupedByProductId: { [key: string]: CertificateItem[] } = uiCertificateItems.reduce(
                (acc, item) => {
                    const productId = item.productId || "";
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
                const subtotal = groupItems.reduce((sum: number, item: CertificateItem) => sum + (item.net || 0), 0);
                const sortedGroupItems = sortedData.filter((item) => item.productId === productId);
                const updatedGroupItems = sortedGroupItems.map((item, index) => ({
                    ...item,
                    isLastInGroup: index === sortedGroupItems.length - 1,
                    productSubtotal: subtotal.toFixed(2),
                }));
                result = [...result, ...updatedGroupItems];
            });

            const pagedData = process(result, { skip: page.skip, take: page.take });
            return pagedData.data;
        })()
        : [];



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
                                style={{ height: isFormCollapsed ? "60vh" : "30vh" }}
                                scrollable="scrollable"
                                resizable={true}
                                sortable
                                sort={sort}
                                onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                                skip={page.skip}
                                take={page.take}
                                total={dataWithSummaries.length}
                                pageable
                                onPageChange={pageChange}
                                data={dataWithSummaries}
                                rowSpannable
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
                                                {getTranslatedLabel(`${localizationKey}.subtotal`, "Total")}: {total.toFixed(2)}
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

export const CertificateItemsListGroupedMemo = React.memo(CertificateItemsListGrouped);