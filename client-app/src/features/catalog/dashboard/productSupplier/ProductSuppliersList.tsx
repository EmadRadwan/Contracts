import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import { orderBy, SortDescriptor, State } from "@progress/kendo-data-query";
import { useAppSelector } from "../../../../app/store/configureStore";
import { Navigate, useNavigate } from "react-router";
import { Grid, Paper, Typography } from "@mui/material";
import Button from "@mui/material/Button";
import ProductSupplierForm from "../../form/productSupplier/ProductSupplierForm";
import CatalogMenu from "../../menu/CatalogMenu";
import {useGetProductSuppliersQuery} from "../../../../app/store/apis";
import {handleDatesArray} from "../../../../app/util/utils";

export default function ProductSuppliersList() {
    const initialSort: Array<SortDescriptor> = [{ field: "partyName", dir: "desc" }];
    const [sort, setSort] = useState(initialSort);
    const initialDataState: State = { skip: 0, take: 4 };
    const [page, setPage] = useState<State>(initialDataState);
    const [editMode, setEditMode] = useState(0);
    const [suppliers, setSuppliers] = useState<{ data: any[], total: number }>({ data: [], total: 0 });
    const [selectedSupplierProduct, setSelectedSupplierProduct] = useState<any>(null);
    const selectedProduct = useAppSelector(state => state.productUi.selectedProduct);
    const navigate = useNavigate();

    // REFACTOR: Replaced Redux slice with RTK Query for data fetching
    // Improves data management with automatic caching and reduces boilerplate
    const { data = [], isLoading, error } = useGetProductSuppliersQuery(
        selectedProduct?.productId || "",
        { skip: !selectedProduct } // Skip query if no product is selected
    );

    useEffect(() => {
        if (data) {
            const adjustedData = handleDatesArray(data);

            setSuppliers(adjustedData);
        }
    }, [data]);


    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    function handleBackClick() {
        navigate('/products', { state: { myStateProp: 'bar' } });
    }

    if (!selectedProduct) {
        return <Navigate to="/products" />;
    }

    function handleSelectSupplierProduct(supplierProduct: any) {
        // REFACTOR: Updated to use SupplierProductDto structure for selection
        // Ensures compatibility with the new API data structure
        setSelectedSupplierProduct(supplierProduct);
        setEditMode(2);
    }

    function cancelEdit() {
        setEditMode(0);
        setSelectedSupplierProduct(null);
    }

    const supplierProductCell = (props: any) => {
        return (
            <td>
                <Button
                    onClick={() => handleSelectSupplierProduct(props.dataItem)}
                >
                    {props.dataItem.partyName}
                </Button>
            </td>
        );
    };

    if (editMode) {
        return (
            <ProductSupplierForm
                supplierProduct={selectedSupplierProduct}
                cancelEdit={cancelEdit}
                editMode={editMode}
            />
        );
    }

    if (isLoading) {
        return <Typography>Loading...</Typography>;
    }
    

    return (
        <>
            <CatalogMenu selectedMenuItem="/products" />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1}>
                    <Grid container alignItems="center">
                        <Grid item xs={8}>
                            <Typography sx={{ p: 2 }} variant="h4">
                                Suppliers for {selectedProduct.productName}
                            </Typography>
                        </Grid>
                    </Grid>

                    <Grid container p={2}>
                        <div className="div-container">
                            <KendoGrid
                                className="main-grid"
                                style={{ height: "400px", width: "1000px" }}
                                data={orderBy(suppliers, sort).slice(page.skip, page.take + page.skip)}
                                sortable={true}
                                sort={sort}
                                onSortChange={(e: GridSortChangeEvent) => {
                                    setSort(e.sort);
                                }}
                                skip={page.skip}
                                take={page.take}
                                total={suppliers.total}
                                pageable={true}
                                onPageChange={pageChange}
                            >
                                <GridToolbar>
                                    <Grid item xs={3}>
                                        <Button
                                            color="secondary"
                                            onClick={() => setEditMode(1)}
                                            variant="outlined"
                                        >
                                            Create Product Supplier
                                        </Button>
                                    </Grid>
                                </GridToolbar>

                                <Column field="partyName" title="Supplier" cell={supplierProductCell} width={300} />
                                <Column field="currencyUomDescription" title="Currency" width={150} />
                                <Column field="quantityUomDescription" title="Quantity Uom" width={200} />
                                <Column field="lastPrice" title="Last Price" width={150} />
                                <Column field="availableFromDate" title="From" width={100} format="{0: dd/MM/yyyy}" />
                                <Column field="availableThruDate" title="To" width={100} format="{0: dd/MM/yyyy}" />
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