import React, { useState, useMemo } from 'react';
import { useAppSelector } from '../../../../app/store/configureStore';
import { Button, Grid, Paper, Typography } from '@mui/material';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar,
} from '@progress/kendo-react-grid';
import { SortDescriptor } from '@progress/kendo-data-query';
import { Navigate, useNavigate } from 'react-router-dom';
import ProductAssociationsForm from '../../form/productAssociations/ProductAssociationsForm';
import {  useFetchProductAssociationsQuery } from '../../../../app/store/apis/productAssociationsApi';
import { ProductAssociation } from '../../../../app/models/product/productAssociation';
import { handleDatesArray } from '../../../../app/util/utils';
import CatalogMenu from '../../menu/CatalogMenu';

interface PageState {
    skip: number;
    take: number;
}


export default function ProductAssociationsList() {
    const selectedProduct = useAppSelector(state => state.productUi.selectedProduct);
    const [selectedAssoc, setSelectedAssoc] = useState<ProductAssociation | undefined>();
    const [editMode, setEditMode] = useState(0);

    
    const initialDataState: PageState = { skip: 0, take: 4 };
    const [page, setPage] = useState<PageState>(initialDataState);

    const initialSort: SortDescriptor[] = [{ field: 'productId', dir: 'desc' }];
    const [sort, setSort] = useState<SortDescriptor[]>(initialSort);

    const navigate = useNavigate();

    const { data: productAssociations, isLoading, error } = useFetchProductAssociationsQuery(selectedProduct?.productId);

    const processedData = useMemo(
        () => handleDatesArray(productAssociations || []),
        [productAssociations]
    );

    const handleBackClick = () => {
        navigate('/products', { state: { myStateProp: 'bar' } });
    };

    const handleSelectProductAssociation = (selectedProductAssociation: ProductAssociation) => {
        const updatedAssociation = {
            ...selectedProductAssociation,
            productIdTo: {
                productId: selectedProductAssociation.productIdTo,
                productName: selectedProductAssociation.productNameTo
            }
        };
        setSelectedAssoc(updatedAssociation);
        setEditMode(2);
    };
    

    if (!selectedProduct) {
        return <Navigate to="/products" />;
    }

    if (editMode > 0) {
        return (
            <ProductAssociationsForm
                selectedProduct={selectedProduct}
                selectedProductAssociation={selectedAssoc}
                editMode={editMode}
                cancelEdit={() => setEditMode(0)}
            />
        );
    }

    if (isLoading) {
        return <Typography>Loading...</Typography>;
    }

    if (error) {
        return <Typography color="error">Error loading associations</Typography>;
    }

    const productIdToCell = (props: any) => {
        return (
            <td>
                <Button
                    onClick={() => handleSelectProductAssociation(props.dataItem)}
                >
                    {props.dataItem.productNameTo}
                </Button>


            </td>
        )
    }

    return (
        <>
            <CatalogMenu selectedMenuItem="products" />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1}>
                    <Grid container alignItems="center">
                        <Grid item xs={12}>
                            <Typography sx={{ p: 2 }} variant="h4">
                                Associations for {selectedProduct?.productName}
                            </Typography>
                        </Grid>
                    </Grid>

                    <Grid container p={2}>
                        <div className="div-container">
                            <KendoGrid
                                className="main-grid"
                                style={{ height: '300px' }}
                                data={processedData}
                                sortable
                                sort={sort}
                                onSortChange={(e: GridSortChangeEvent) => setSort(e.sort)}
                                skip={page.skip}
                                take={page.take}
                                pageable
                                onPageChange={(event: GridPageChangeEvent) => setPage(event.page)}
                            >
                                <GridToolbar>
                                    <Grid item xs={3}>
                                        {/* REFACTOR: Align with OFBiz create button */}
                                        <Button
                                            color="secondary"
                                            onClick={() => setEditMode(1)}
                                            variant="outlined"
                                        >
                                            Create Product Association
                                        </Button>
                                    </Grid>
                                </GridToolbar>

                                {/* REFACTOR: Hide IDs to match OFBiz grid hidden fields */}
                                <Column field="productAssocTypeId" title="productAssocTypeId" width={0} />
                                <Column field="productId" title="productId" width={0} />
                                <Column
                                    cell={productIdToCell}
                                    title="Associated Product"
                                    width={300}
                                />
                                {/* REFACTOR: Standardize date format to match OFBiz date-time display */}
                                <Column field="fromDate" title="From Date" width={150} format="{0: yyyy/MM/dd}" />
                                <Column field="thruDate" title="To Date" width={150} format="{0: yyyy/MM/dd}" />
                                <Column field="quantity" title="Quantity" width={100} />
                                <Column field="reason" title="Reason" width={100} />
                                <Column field="sequenceNum" title="Sequence" width={100} />
                            </KendoGrid>

                            <Grid item xs={3} p={1}>
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