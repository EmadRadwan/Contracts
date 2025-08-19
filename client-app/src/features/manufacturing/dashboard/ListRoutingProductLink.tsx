import React, { useState, useEffect } from 'react';
import { Grid as KendoGrid, GridColumn as Column, GridDataStateChangeEvent, GRID_COL_INDEX_ATTRIBUTE, GridToolbar } from '@progress/kendo-react-grid';
import { useTableKeyboardNavigation } from '@progress/kendo-react-data-tools';
import { Grid, Paper, Button, Typography } from '@mui/material';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { DataResult, State } from '@progress/kendo-data-query';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import RoutingMenu from "../menu/RoutingMenu";
import { handleDatesArray } from "../../../app/util/utils";
import {useDeleteRoutingProductLinkMutation, useGetRoutingProductLinksQuery} from "../../../app/store/apis";
import {EditRoutingProductLink} from "../form/EditRoutingProductLink";

export default function ListRoutingProductLink() {
    const { workEffortId } = useParams<{ workEffortId: string }>();
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'manufacturing.routingProductLink.grid';
    const [editMode, setEditMode] = useState(0);
    const [selectedProductLink, setSelectedProductLink] = useState<RoutingProductLink | undefined>(undefined);
    const [dataState, setDataState] = useState<State>({
        take: 6,
        skip: 0,
        sort: [{ field: 'productId', dir: 'asc' }],
    });
    const [routingProductLinks, setRoutingProductLinks] = useState<DataResult>({ data: [], total: 0 });
    const { data, isFetching, error } = useGetRoutingProductLinksQuery(workEffortId!);
    const [removeRoutingProductLink, { isLoading: isRemoving }] = useDeleteRoutingProductLinkMutation();

    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data);
            setRoutingProductLinks(adjustedData);
        }
    }, [data]);

    const dataStateChange = (e: GridDataStateChangeEvent) => {
        setDataState(e.dataState);
    };

    const handleSelectProductLink = (productLink?: RoutingProductLink) => {
        if (productLink) {
            setSelectedProductLink(productLink);
            setEditMode(2); // Edit mode
        } else {
            setSelectedProductLink(null);
            setEditMode(1); // New mode
        }
    };

    const cancelEdit = () => {
        setEditMode(0);
        setSelectedProductLink(null);
    };

    const handleDeleteProductLink = async (productLink: RoutingProductLink) => {
        try {
            // Purpose: Aligns with OFBiz deleteLink specification, using workEffortId, productId, fromDate, and workEffortGoodStdTypeId
            // Benefit: Ensures accurate identification of the record to delete
            await removeRoutingProductLink({
                workEffortId: workEffortId!,
                productId: productLink.productId.productId,
                fromDate: productLink.fromDate,
                workEffortGoodStdTypeId: 'ROU_PROD_TEMPLATE',
            }).unwrap();
            toast.success(getTranslatedLabel(`${localizationKey}.deleteSuccess`, 'Routing Product Link Deleted Successfully!'));
        } catch (e: any) {
            const errorMsg = e.data?.error || e.data?.errors?.join(', ') || 'Delete operation failed.';
            toast.error(errorMsg);
        }
    };

    const ProductIdCell = (props: any) => {
        const { productId } = props.dataItem;
        const navigationAttributes = useTableKeyboardNavigation(props.id);

        // Purpose: Replaced navigation with edit mode trigger, passing full record to EditRoutingProductLink form
        // Benefit: Aligns with user requirement to edit the entire record instead of navigating
        return (
            <td
                className={props.className}
                style={{ ...props.style, color: 'blue', cursor: 'pointer' }}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
                onClick={() => handleSelectProductLink(props.dataItem)}
            >
                {productId.productId}
            </td>
        );
    };

    const DeleteCell = (props: any) => {
        const navigationAttributes = useTableKeyboardNavigation(props.id);
        return (
            <td
                className={props.className}
                colSpan={props.colSpan}
                role="gridcell"
                aria-colindex={props.ariaColumnIndex}
                aria-selected={props.isSelected}
                {...{ [GRID_COL_INDEX_ATTRIBUTE]: props.columnIndex }}
                {...navigationAttributes}
            >
                <Button
                    color="error"
                    onClick={() => handleDeleteProductLink(props.dataItem)}
                    disabled={isRemoving}
                >
                    {getTranslatedLabel('common.delete', 'Delete')}
                </Button>
            </td>
        );
    };

    return (
        <>
            {editMode ? (
                <EditRoutingProductLink
                    productLink={selectedProductLink}
                    editMode={editMode}
                    cancelEdit={cancelEdit}
                />
            ) : (
                <>
                    <ManufacturingMenu />
                    <Paper elevation={5} className="div-container-withBorderCurved">
                        <Grid container columnSpacing={1}>
                            <Grid container alignItems="center">
                                <Grid item xs={8}>
                                    <Typography sx={{ p: 2 }} variant="h4">
                                        {getTranslatedLabel(`${localizationKey}.title`, 'Routing Product Links')}
                                    </Typography>
                                </Grid>
                            </Grid>
                            <RoutingMenu workEffortId={workEffortId!} selectedMenuItem="routingProductLink" />
                            <Grid container p={2}>
                                <div className="div-container">
                                    <KendoGrid
                                        className="main-grid"
                                        data={routingProductLinks}
                                        resizable
                                        sortable
                                        pageable
                                        {...dataState}
                                        onDataStateChange={dataStateChange}
                                    >
                                        <GridToolbar>
                                            <Grid item xs={3}>
                                                <Button
                                                    color="secondary"
                                                    onClick={() => handleSelectProductLink()}
                                                    variant="outlined"
                                                    disabled={isFetching}
                                                >
                                                    {getTranslatedLabel(`${localizationKey}.create`, 'Create Product Link')}
                                                </Button>
                                            </Grid>
                                        </GridToolbar>
                                        <Column
                                            field="productId.productId"
                                            title={getTranslatedLabel('manufacturing.productId', 'Product ID')}
                                            cell={ProductIdCell}
                                            width={150}
                                        />
                                        <Column
                                            field="productId.productName"
                                            title={getTranslatedLabel('manufacturing.productName', 'Product Name')}
                                            width={250}
                                        />
                                        <Column
                                            field="fromDate"
                                            title={getTranslatedLabel('common.fromDate', 'From Date')}
                                            format="{0:yyyy-MM-dd}"
                                            width={180}
                                        />
                                        <Column
                                            field="thruDate"
                                            title={getTranslatedLabel('common.thruDate', 'Thru Date')}
                                            format="{0:yyyy-MM-dd}"
                                            width={180}
                                        />
                                        <Column
                                            field="estimatedQuantity"
                                            title={getTranslatedLabel('manufacturing.quantity', 'Quantity')}
                                            width={120}
                                        />
                                        <Column title=" " cell={DeleteCell} width={100} />
                                    </KendoGrid>
                                    {isFetching && (
                                        <LoadingComponent
                                            message={getTranslatedLabel(`${localizationKey}.loading`, 'Loading Routing Product Links...')}
                                        />
                                    )}
                                </div>
                            </Grid>
                        </Grid>
                    </Paper>
                </>
            )}
        </>
    );
}