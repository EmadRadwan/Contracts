import React, { useEffect, useState } from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import {useAppSelector, useGetProductFacilitiesQuery} from "../../../../app/store/configureStore";
import { Navigate, useNavigate } from "react-router";
import { Grid, Paper, Typography } from "@mui/material";
import Button from "@mui/material/Button";
import ProductFacilityForm from "../../form/productFacility/ProductFacilityForm";
import CatalogMenu from "../../menu/CatalogMenu";

export default function ProductFacilitiesList() {
    const initialSort: Array<SortDescriptor> = [{ field: "facilityName", dir: "desc" }];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = { skip: 0, take: 4 };
    const [page, setPage] = React.useState<any>(initialDataState);
    const [editMode, setEditMode] = useState(0);
    const [selectedProductFacilityId, setSelectedProductFacilityId] = useState<string | null>(null);

    const navigate = useNavigate();
    const selectedProduct = useAppSelector(state => state.productUi.selectedProduct);

    // REFACTOR: Replaced Redux slice action (fetchProductFacilitiesAsync) with RTK Query hook.
    // This uses useGetProductFacilitiesQuery to fetch product facilities, leveraging RTK Query's caching and loading states.
    const { data: productFacilities = [], isLoading } = useGetProductFacilitiesQuery(
        selectedProduct?.productId || "",
        { skip: !selectedProduct }
    );

    // REFACTOR: Replaced Redux selector (getSelectedProductFacilityIdEntity) with local state.
    // Since RTK Query manages data fetching, we use local state to track the selected product facility.
    const selectedProductFacility = productFacilities.find(
        pf => pf.productId + pf.facilityId === selectedProductFacilityId
    );

    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    function handleBackClick() {
        navigate('/products', { state: { myStateProp: 'bar' } });
    }

    if (!selectedProduct) {
        return <Navigate to="/products" />;
    }

    function handleSelectProductFacility(productFacilityId: string) {
        setSelectedProductFacilityId(productFacilityId);
        setEditMode(2);
    }

    function cancelEdit() {
        setEditMode(0);
        setSelectedProductFacilityId(null);
    }

    const productFacilityCell = (props: any) => {
        return (
            <td>
                <Button
                    onClick={() => handleSelectProductFacility(props.dataItem.productId.concat(props.dataItem.facilityId))}
                >
                    {props.dataItem.facilityName}
                </Button>
            </td>
        );
    };

    if (editMode) {
        return (
            <ProductFacilityForm
                productFacility={selectedProductFacility}
                cancelEdit={cancelEdit}
                editMode={editMode}
            />
        );
    }

    return (
        <>
            <CatalogMenu selectedMenuItem='/products' />
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container columnSpacing={1}>
                    <Grid container alignItems="center">
                        <Grid item xs={12}>
                            <Typography sx={{ p: 2 }} variant='h4'>
                                Facilities for {selectedProduct.productName}
                            </Typography>
                        </Grid>
                    </Grid>

                    <Grid container p={2}>
                        <div className="div-container">
                            <KendoGrid
                                className="main-grid"
                                style={{ height: "300px", width: "700px" }}
                                data={orderBy(productFacilities, sort).slice(page.skip, page.take + page.skip)}
                                sortable={true}
                                sort={sort}
                                onSortChange={(e: GridSortChangeEvent) => {
                                    setSort(e.sort);
                                }}
                                skip={page.skip}
                                take={page.take}
                                total={productFacilities.length}
                                pageable={true}
                                onPageChange={pageChange}
                            >
                                <GridToolbar>
                                    <Grid item xs={3}>
                                        <Button color={"secondary"} onClick={() => setEditMode(1)} variant="outlined">
                                            Create Product Facility
                                        </Button>
                                    </Grid>
                                </GridToolbar>

                                <Column field="facilityName" title="Product Facility" cell={productFacilityCell} width={220} />
                                <Column field="minimumStock" title="Minimum Stock" width={140} />
                                <Column field="reorderQuantity" title="Reorder Quantity" width={140} />
                                <Column field="lastInventoryCount" title="Last Inventory Count" width={0} />
                                <Column field="productId" title="productId" width={0} />
                                <Column field="facilityId" title="facilityId" width={0} />
                            </KendoGrid>
                            <Grid item xs={3} paddingTop={1}>
                                <Button variant="contained" color="error" onClick={handleBackClick}>
                                    Back
                                </Button>
                            </Grid>
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}