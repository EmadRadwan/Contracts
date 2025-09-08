import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import React, { useCallback, useState } from "react";
import { Grid as KendoGrid, GridCellProps, GridColumn as Column, GridPageChangeEvent, GridSortChangeEvent, GridToolbar } from "@progress/kendo-react-grid";
import { useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import { Button, Grid, Skeleton, Typography } from "@mui/material";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import { setUiCertificateItems } from "../slice/certificateItemsUiSlice";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { certificateSubTotal, displayCertificateItemsSelector, nonDeletedCertificateItemsSelector } from "../slice/certificateSelectors";
import { useFetchCertificateItemsQuery } from "../../../app/store/apis/certificateItemsApi";
import { CertificateItemFormMemo } from "../form/CertificateItemForm";
import agent from "../../../app/api/agent";

interface ProductItem {
    ProductId: string;
    ProductName: string;
    ProductType: string;
}

interface UOMItem {
    UomId: string;
    Description: string;
}

interface Props {
    editMode: number; // 0: view, 1: create, 2: edit CREATED, 3: edit APPROVED, 4: edit COMPLETED
    workEffortId?: string;
}

export default function CertificateItemsList({ editMode, workEffortId }: Props) {
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
    const subtotal = useAppSelector(certificateSubTotal);
    const { currentCertificateType } = useAppSelector((state) => state.certificateUi);
    const { data: certificateItemsData, isFetching, isLoading } = useFetchCertificateItemsQuery(workEffortId || "", {
        skip: !workEffortId,
    });
    const uiCertificateItems: CertificateItem[] = useAppSelector(displayCertificateItemsSelector);

    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    const handleSelectCertificateItem = useCallback(
        (workEffortId: string) => {
            const selectedCertificateItem = uiCertificateItems.find((item) => item.workEffortId === workEffortId);
            if (!selectedCertificateItem) return;
            const productItem: ProductItem = {
                ProductId: selectedCertificateItem.productId,
                ProductName: selectedCertificateItem.productName || "",
                ProductType: "",
            };
            const uomItem: UOMItem = {
                UomId: selectedCertificateItem.uomId,
                Description: selectedCertificateItem.uomName || "",
            };
            setCertificateItem({
                ...selectedCertificateItem,
                productId: productItem,
                uomId: uomItem,
            });
            setItemEditMode(2);
            setShow(true);
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
        (dataItem: CertificateItem) => {
            const originalItems = useAppSelector(nonDeletedCertificateItemsSelector);
            const newCertificateItems = originalItems.map((item) =>
                item.workEffortId === dataItem.workEffortId ? { ...item, isDeleted: true } : item
            );
            dispatch(setUiCertificateItems(newCertificateItems));
        },
        [dispatch]
    );

    const CommandCell = (props: GridCellProps) => <DeleteCertificateItemCell {...props} remove={remove} />;

    const updateCertificateItems = useCallback(
        (certificateItem: CertificateItem, editMode: number) => {
            const originalItems = useAppSelector(nonDeletedCertificateItemsSelector);
            let newCertificateItems: CertificateItem[];
            try {
                if (editMode === 1) {
                    newCertificateItems = originalItems ? [...originalItems, certificateItem] : [certificateItem];
                } else {
                    newCertificateItems = originalItems.map((item) =>
                        item.workEffortId === certificateItem.workEffortId ? certificateItem : item
                    );
                }
                dispatch(setUiCertificateItems(newCertificateItems));
            } catch (e) {
                console.error("Error updating certificate items:", e);
            }
        },
        [dispatch]
    );

    const memoizedOnClose = useCallback(() => {
        setShow(false);
    }, []);

    // REFACTOR: Updated column rendering logic to support the five new certificate types.
    // Purpose: Replaces PROCUREMENTS and CONTRACTING checks with logic for supply-related (SUPPLY_PROCUREMENT_CERTIFICATE, EXTERNAL_SUPPLY_SALE_CERTIFICATE) and contracting-related (WORKMANSHIP_CONTRACTING_CERTIFICATE, CONTRACTOR_PURCHASE_CERTIFICATE, COMPANY_SUPPLY_SALE_CERTIFICATE) types.
    // Context: Ensures relevant fields (e.g., discount for supply types, deductions for contracting types) are displayed based on the certificate type, aligning with the sheet’s context.
    const isSupplyType = ["SUPPLY_PROCUREMENT_CERTIFICATE", "EXTERNAL_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType);
    const isContractingType = [
        "WORKMANSHIP_CONTRACTING_CERTIFICATE",
        "CONTRACTOR_PURCHASE_CERTIFICATE",
        "COMPANY_SUPPLY_SALE_CERTIFICATE",
    ].includes(currentCertificateType);

    const columns = [
        {
            field: "productName",
            title: getTranslatedLabel(`${localizationKey}.description`, "Description"),
            cell: descriptionCell,
            width: 280,
        },
        { field: "quantity", title: getTranslatedLabel(`${localizationKey}.quantity`, "Quantity") },
        { field: "unitPrice", title: getTranslatedLabel(`${localizationKey}.unitPrice`, "Unit Price"), format: "{0:n2}" },
        { field: "displayTotal", title: getTranslatedLabel(`${localizationKey}.totalAmount`, "Total Amount"), format: "{0:n2}" },
        ...(isSupplyType
            ? [
                { field: "discount", title: getTranslatedLabel(`${localizationKey}.discount`, "Discount"), format: "{0:n2}" },
                {
                    field: "formattedProcurementDate",
                    title: getTranslatedLabel(`${localizationKey}.procurementDate`, "Procurement Date"),
                },
                { field: "facilityName", title: getTranslatedLabel(`${localizationKey}.facilityName`, "Facility") },
                {
                    field: "transportationExpenses",
                    title: getTranslatedLabel(`${localizationKey}.transportationExpenses`, "Transportation Expenses"),
                    format: "{0:n2}",
                },
                {
                    field: "gratuities",
                    title: getTranslatedLabel(`${localizationKey}.gratuities`, "Gratuities"),
                    format: "{0:n2}",
                },
            ]
            : []),
        ...(isContractingType
            ? [
                { field: "deductions", title: getTranslatedLabel(`${localizationKey}.deductions`, "Deductions"), format: "{0:n2}" },
                { field: "deserved", title: getTranslatedLabel(`${localizationKey}.deserved`, "Deserved"), format: "{0:n2}" },
                { field: "insurance", title: getTranslatedLabel(`${localizationKey}.insurance`, "Insurance"), format: "{0:n2}" },
                { field: "net", title: getTranslatedLabel(`${localizationKey}.net`, "Net"), format: "{0:n2}" },
                {
                    field: "isContractorPurchased",
                    title: getTranslatedLabel(`${localizationKey}.isContractorPurchased`, "Contractor Purchased"),
                },
                {
                    field: "achievementPercentage",
                    title: getTranslatedLabel(`${localizationKey}.achievementPercentage`, "Achievement %"),
                    format: "{0:n0}",
                },
            ]
            : []),
        { cell: CommandCell },
    ];

    return (
        <>
            {show && (
                <ModalContainer show={show} onClose={memoizedOnClose} width={700}>
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