import {orderBy, SortDescriptor, State, process} from '@progress/kendo-data-query';
import React, {useCallback, useEffect, useState, useMemo} from "react";
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

import ModalContainer from "../../../app/common/modals/ModalContainer";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {
    certificateSubTotal,
    displayCertificateItemsSelector,
    nonDeletedCertificateItemsSelector
} from "../slice/certificateSelectors";
import {useFetchCertificateItemsQuery} from "../../../app/store/apis/certificateItemsApi";
import {CertificateItemFormMemo} from "../form/CertificateItemForm";
import CertificateItemKendoBulkAdd from "../form/CertificateItemKendoBulkAddV2";
import {
    setProcessedCertificateItems,
    setUiCertificateItems,
    setUiCertificateItemsFromApi
} from "../slice/certificateItemsUiSlice";


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
    const [showBulkAdd, setShowBulkAdd] = useState(false);
    const dispatch = useAppDispatch();
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "projects.certificate.items.list";
    const subtotal = useAppSelector(certificateSubTotal);
    const {currentCertificateType} = useAppSelector((state) => state.certificateUi);

    const {data: certificateItemsData, isFetching, isLoading} = useFetchCertificateItemsQuery(workEffortId || "", {
        skip: !workEffortId,
    });
    const uiCertificateItems: CertificateItem[] = useAppSelector(displayCertificateItemsSelector);
    const nonDeletedItems = useAppSelector(nonDeletedCertificateItemsSelector);

    const isSupplyWithDiscount = ["SUPPLY_PROCUREMENT_CERTIFICATE"].includes(currentCertificateType);
    const isSupplyWithoutDiscount = ["COMPANY_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType);
    const isWorkmanship = ["WORKMANSHIP_CONTRACTING_CERTIFICATE"].includes(currentCertificateType);

    useEffect(() => {
        if (certificateItemsData && !isFetching && !isLoading) {
            dispatch(setUiCertificateItemsFromApi(certificateItemsData));
        }
    }, [certificateItemsData, isFetching, isLoading, dispatch]);

    const memoizedOnClose = useCallback(() => {
        setShow(false);
    }, []);

    const memoizedOnBulkClose = useCallback(() => {
        setShowBulkAdd(false);
    }, []);

    const modalWidth = isWorkmanship ? 1200 : 700;
    const bulkModalWidth = "95%";

    const addItem = useCallback((item: CertificateItem) => {
        dispatch(setProcessedCertificateItems([item]));
    }, [dispatch]);

    const updateItem = useCallback((item: CertificateItem) => {
        dispatch(setProcessedCertificateItems([item]));
    }, [dispatch]);

    const deleteItem = useCallback((itemId: string) => {
        dispatch(setProcessedCertificateItems([{ workEffortId: itemId, isDeleted: true } as CertificateItem]));
    }, [dispatch]);

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
                materialPrice: selectedCertificateItem.materialPrice || 0,
                laborPrice: selectedCertificateItem.laborPrice || 0,
                procurementDate: selectedCertificateItem.procurementDate
                    ? new Date(selectedCertificateItem.procurementDate)
                    : new Date(),
                additionalInsurance: selectedCertificateItem.additionalInsurance || 0,
                deductionDescription: selectedCertificateItem.deductionDescription || "",
                insuranceMode: selectedCertificateItem.insuranceMode || "value",
                additionalInsuranceMode: selectedCertificateItem.additionalInsuranceMode || "value",
            };
            setCertificateItem(certificateItem);
            setItemEditMode(2);
            setShow(true);
        },
        [uiCertificateItems]
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

    const CommandCell = (props: GridCellProps) => (
        <DeleteCertificateItemCell {...props} remove={(dataItem: CertificateItem) => remove(dataItem, nonDeletedItems)} />
    );

    const dataWithSummaries = useMemo(() => {
        const sorted = orderBy(uiCertificateItems, sort);
        return process(sorted, { skip: page.skip, take: page.take });
    }, [uiCertificateItems, sort, page]);

    const columns = [
        {
            field: 'code',
            title: getTranslatedLabel(`${localizationKey}.code`, 'Code'),
            width: 250,
            cell: (props: GridCellProps) => (
                <td>
                    {props.dataItem.isLastInGroup && props.dataItem.productSubtotal !== undefined
                        ? `${props.dataItem.code} (${getTranslatedLabel(`${localizationKey}.productSubtotal`, 'Subtotal')}: ${props.dataItem.productSubtotal})`
                        : props.dataItem.code}
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
            format: "{0:n3}",
        },
        ...(isWorkmanship
            ? [
                { field: "materialPrice", title: getTranslatedLabel(`${localizationKey}.materialPrice`, "Material Price"), format: "{0:n3}", width: 150 },
                { field: "laborPrice", title: getTranslatedLabel(`${localizationKey}.laborPrice`, "Labor Price"), format: "{0:n3}", width: 150 },
                { field: "displayTotal", title: getTranslatedLabel(`${localizationKey}.totalAmount`, "Total Amount"), format: "{0:n2}", width: 130 },
                { field: "deductions", title: getTranslatedLabel(`${localizationKey}.deductions`, "Deductions"), format: "{0:n2}", width: 120 },
                {
                    field: "deductionDescription",
                    title: getTranslatedLabel(`${localizationKey}.deductionDescription`, "Deduction Description"),
                    width: 200,
                    cell: deductionDescriptionCell,
                },
                { field: "deserved", title: getTranslatedLabel(`${localizationKey}.deserved`, "Deserved"), format: "{0:n2}", width: 120 },
                { field: "insurance", title: getTranslatedLabel(`${localizationKey}.insurance`, "Insurance"), format: "{0:n2}", width: 120 },
                { field: "additionalInsurance", title: getTranslatedLabel(`${localizationKey}.additionalInsurance`, "Additional Insurance"), format: "{0:n2}", width: 140 },
                { field: "net", title: getTranslatedLabel(`${localizationKey}.net`, "Net"), format: "{0:n2}", width: 120 },
                { field: "achievementPercentage", title: getTranslatedLabel(`${localizationKey}.achievementPercentage`, "Achievement %"), format: "{0:n9}", width: 140 },
            ]
            : [
                {
                    field: "unitPrice",
                    title: getTranslatedLabel(`${localizationKey}.unitPrice`, "Unit Price"),
                    format: "{0:n3}",
                    width: 120,
                },
                {
                    field: "displayTotal",
                    title: getTranslatedLabel(`${localizationKey}.totalAmount`, "Total Amount"),
                    format: "{0:n3}",
                    width: 130,
                },
            ]),
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

    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

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
            {showBulkAdd && (
                <ModalContainer show={showBulkAdd} onClose={memoizedOnBulkClose} width={bulkModalWidth}>
                    <CertificateItemKendoBulkAdd
                        onClose={memoizedOnBulkClose}
                        addItem={addItem}
                        updateItem={updateItem}
                        deleteItem={deleteItem}
                        initialItems={uiCertificateItems}
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
                                data={dataWithSummaries.data}
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
                                            <Button
                                                color="primary"
                                                onClick={() => {
                                                    setShowBulkAdd(true);
                                                }}
                                                variant="outlined"
                                                disabled={editMode > 3}
                                                sx={{ml: 1}}
                                            >
                                                {getTranslatedLabel(`${localizationKey}.bulkAdd`, "Bulk Add")}
                                            </Button>
                                        </Grid>
                                        {(isSupplyWithDiscount || isSupplyWithoutDiscount || isWorkmanship) && (
                                            <Grid item>
                                                <Typography>
                                                    {getTranslatedLabel(`${localizationKey}.Total`, "Total")}: {subtotal.toFixed(2)}
                                                </Typography>
                                            </Grid>
                                        )}
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
