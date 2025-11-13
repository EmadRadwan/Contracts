import { orderBy, SortDescriptor, State, process } from "@progress/kendo-data-query";
import React, { useCallback, useEffect, useMemo, useState } from "react";
import {
    Grid as KendoGrid,
    GridCellProps,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import { Button, Grid, Skeleton, Typography } from "@mui/material";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import {
    resetUiCertificateItems,
    setUiCertificateItems,
    setUiCertificateItemsFromApi,
} from "../slice/certificateItemsUiSlice";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {
    certificateSubTotal,
    displayCertificateItemsSelector,
    nonDeletedCertificateItemsSelector,
} from "../slice/certificateSelectors";
import { useFetchCertificateItemsQuery } from "../../../app/store/apis/certificateItemsApi";
import { CertificateItemFormMemo } from "../form/CertificateItemForm";

interface Props {
    editMode: number; // 0: view, 1: create, 2: edit CREATED, 3: edit APPROVED, 4: edit COMPLETED
    workEffortId?: string;
    isFormCollapsed: boolean;
}

/* -------------------------------------------------------------------------- */
/*  Main Component                                                            */
/* -------------------------------------------------------------------------- */
export default function CertificateItemsListGrouped({
                                                        editMode,
                                                        workEffortId,
                                                        isFormCollapsed,
                                                    }: Props) {
    /* --------------------------- State ------------------------------------- */
    const initialSort: SortDescriptor[] = [{ field: "productName", dir: "asc" }];
    const [sort, setSort] = useState<SortDescriptor[]>(initialSort);
    const [page, setPage] = useState<State>({ skip: 0, take: 4 });
    const [show, setShow] = useState(false);
    const [itemEditMode, setItemEditMode] = useState(0);
    const [certificateItem, setCertificateItem] = useState<CertificateItem | undefined>(undefined);

    const dispatch = useAppDispatch();
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "certificate.items.list";

    /* --------------------------- Selectors --------------------------------- */
    const total = useAppSelector(certificateSubTotal);
    const uiItems = useAppSelector(displayCertificateItemsSelector); // already has code, productSubtotal, isLastInGroup
    const nonDeletedItems = useAppSelector(nonDeletedCertificateItemsSelector);

    const { data: apiItems, isFetching, isLoading } = useFetchCertificateItemsQuery(
        workEffortId || "",
        { skip: !workEffortId }
    );

    /* --------------------------- Effects ----------------------------------- */
    useEffect(() => {
        if (apiItems && !isFetching && !isLoading) {
            dispatch(setUiCertificateItemsFromApi(apiItems));
        }
    }, [apiItems, isFetching, isLoading, dispatch]);

    /* --------------------------- Paging / Sorting -------------------------- */
    // Let kendo-data-query handle paging + sorting on the *already-enriched* data
    // Purpose: Remove manual grouping/subtotal logic – selector does it once
    const processedData = useMemo(() => {
        const sorted = orderBy(uiItems, sort);
        return process(sorted, { skip: page.skip, take: page.take });
    }, [uiItems, sort, page]);

    const pageChange = (e: GridPageChangeEvent) => setPage(e.page);
    const sortChange = (e: GridSortChangeEvent) => setSort(e.sort);

    /* --------------------------- Row Actions ------------------------------- */
    const handleSelect = useCallback(
        (workEffortId: string) => {
            const src = uiItems.find((i) => i.workEffortId === workEffortId);
            if (!src) return;

            const item: CertificateItem = {
                ...src,
                productId: {
                    ProductId: src.productId,
                    ProductName: src.productName ?? "",
                    ProductType: "",
                },
                uomId: {
                    UomId: src.uomId,
                    Description: src.uomName ?? "",
                },
                gratuities: src.gratuities ?? 0,
                transportationExpenses: src.transportationExpenses ?? 0,
                discount: src.discount ?? 0,
                materialPrice: src.materialPrice ?? 0,
                laborPrice: src.laborPrice ?? 0,
                procurementDate: src.procurementDate ? new Date(src.procurementDate) : new Date(),
                additionalInsurance: src.additionalInsurance ?? 0,
                deductionDescription: src.deductionDescription ?? "",
                insuranceMode: src.insuranceMode ?? "value",
                additionalInsuranceMode: src.additionalInsuranceMode ?? "value",
            };

            setCertificateItem(item);
            setItemEditMode(2);
            setShow(true);
        },
        [uiItems]
    );

    const remove = useCallback(
        (dataItem: CertificateItem) => {
            const updated = nonDeletedItems.map((i) =>
                i.workEffortId === dataItem.workEffortId ? { ...i, isDeleted: true } : i
            );
            dispatch(setUiCertificateItems(updated));
        },
        [nonDeletedItems, dispatch]
    );

    /* --------------------------- Custom Cells ------------------------------ */
    const deductionDescriptionCell = (props: GridCellProps) => (
        <td>
            {props.dataItem.deductionDescription ? (
                <Button onClick={() => handleSelect(props.dataItem.workEffortId)}>
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
            <Button onClick={() => handleSelect(props.dataItem.workEffortId)}>
                {props.dataItem.productName}
            </Button>
        </td>
    );

    const DeleteCell = (props: GridCellProps) => (
        <td className="k-command-cell">
            <Button
                className="k-button k-button-md k-rounded-md k-button-solid k-button-solid-base k-grid-remove-command"
                disabled={editMode > 3}
                onClick={() => props.remove?.(props.dataItem)}
                color="error"
            >
                {getTranslatedLabel(`${localizationKey}.remove`, "Remove")}
            </Button>
        </td>
    );

    const CommandCell = (props: GridCellProps) => (
        <DeleteCell {...props} remove={remove} />
    );

    /* --------------------------- Toolbar ----------------------------------- */
    const openAddModal = () => {
        setCertificateItem(undefined);
        setItemEditMode(1);
        setShow(true);
    };

    const closeModal = useCallback(() => setShow(false), []);

    /* --------------------------- Columns ----------------------------------- */
    const columns = useMemo(
        () => [
            {
                field: "code",
                title: getTranslatedLabel(`${localizationKey}.code`, "Code"),
                width: 280,
                // REFACTOR: Show subtotal in the first column on the last row of each product group
                // Purpose: Match legacy UI behavior exactly
                cell: (props: GridCellProps) => (
                    <td>
                        {props.dataItem.isLastInGroup
                            ? `${props.dataItem.code} (${getTranslatedLabel(
                                `${localizationKey}.productSubtotal`,
                                "Subtotal"
                            )}: ${props.dataItem.productSubtotal})`
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
            { field: "description", title: getTranslatedLabel(`${localizationKey}.description`, "Description"), width: 280 },
            { field: "quantity", title: getTranslatedLabel(`${localizationKey}.quantity`, "Quantity"), width: 100 },
            { field: "materialPrice", title: getTranslatedLabel(`${localizationKey}.materialPrice`, "Material Price"), format: "{0:n2}", width: 150 },
            { field: "laborPrice", title: getTranslatedLabel(`${localizationKey}.laborPrice`, "Labor Price"), format: "{0:n2}", width: 150 },
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
            { field: "achievementPercentage", title: getTranslatedLabel(`${localizationKey}.achievementPercentage`, "Achievement %"), format: "{0:n0}", width: 140 },
            { cell: CommandCell, width: 100 },
        ],
        [editMode, getTranslatedLabel, localizationKey, handleSelect, remove]
    );
    /* --------------------------- Render ------------------------------------ */
    return (
        <>
            {/* ---------- Modal ---------- */}
            {show && (
                <ModalContainer show={show} onClose={closeModal} width={1200}>
                    <CertificateItemFormMemo
                        certificateItem={certificateItem}
                        editMode={itemEditMode}
                        onClose={closeModal}
                        formEditMode={editMode}
                    />
                </ModalContainer>
            )}

            {/* ---------- Grid ---------- */}
            <Grid container columnSpacing={1} direction="column" alignItems="flex-start" sx={{ mt: 1 }}>
                <Grid container>
                    {(isFetching || isLoading) ? (
                        <Grid container spacing={2} direction="column">
                            {[...Array(3)].map((_, i) => (
                                <Grid item key={i}>
                                    <Skeleton animation="wave" variant="rounded" height={40} sx={{ width: "70%" }} />
                                </Grid>
                            ))}
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
                                onSortChange={sortChange}
                                skip={page.skip}
                                take={page.take}
                                total={uiItems.length}
                                pageable
                                onPageChange={pageChange}
                                data={processedData.data}
                                rowSpannable
                            >
                                <GridToolbar>
                                    <Grid container justifyContent="space-between">
                                        <Grid item>
                                            <Button
                                                color="secondary"
                                                onClick={openAddModal}
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

                                {columns.map((col, i) => (
                                    <Column key={i} {...col} />
                                ))}
                            </KendoGrid>
                        </Grid>
                    )}
                </Grid>
            </Grid>
        </>
    );
}

/* -------------------------------------------------------------------------- */
/*  Memoised export (unchanged)                                               */
/* -------------------------------------------------------------------------- */
export const CertificateItemsListGroupedMemo = React.memo(CertificateItemsListGrouped);