import React, {useEffect, useState} from 'react';
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar,
} from '@progress/kendo-react-grid';
import { orderBy, SortDescriptor, State } from '@progress/kendo-data-query';
import {useAppSelector, useGetProductPricesQuery} from '../../../../app/store/configureStore';
import { Navigate, useNavigate } from 'react-router';
import ProductPricesForm from '../../form/productPrice/ProductPriceForm';
import { Grid, Paper, Typography } from '@mui/material';
import Button from '@mui/material/Button';
import CatalogMenu from '../../menu/CatalogMenu';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import { ProductPrice } from '../../models/product/productPrice';
import {handleDatesArray} from "../../../../app/util/utils";

export default function ProductPricesList() {
    const localizationKey = 'product.prod-price.list';
    const { getTranslatedLabel } = useTranslationHelper();
    const initialSort: Array<SortDescriptor> = [{ field: 'productPriceTypeDescription', dir: 'desc' }];
    const [sort, setSort] = useState(initialSort);
    const initialDataState: State = { skip: 0, take: 4 };
    const [page, setPage] = useState<State>(initialDataState);
    const navigate = useNavigate();
    const [editMode, setEditMode] = useState(0);
    // REFACTOR: Replaced Redux selector for selected product price with local state
    const [selectedProductPrice, setSelectedProductPrice] = useState<ProductPrice | null>(null);
    const [productPrices, setProductPrices] = useState<[]>(null);
    const selectedProduct = useAppSelector(state => state.productUi.selectedProduct);

    // REFACTOR: Replaced fetchProductPricesAsync and Redux selector with RTK Query hook
    const { data: productPricesData = [], isLoading } = useGetProductPricesQuery(
        { productId: selectedProduct?.productId || '' },
        { skip: !selectedProduct } // Skip query if no selected product
    );

    useEffect(() => {
        if (productPricesData) {
            const adjustedData = handleDatesArray(productPricesData);
            setProductPrices(adjustedData);
        }
    }, [productPricesData]);
    
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };

    function handleBackClick() {
        navigate('/products', { state: { myStateProp: 'bar' } });
    }

    if (!selectedProduct) {
        return <Navigate to="/products" />;
    }

    function handleSelectProductPrice(productPrice: ProductPrice) {
        // REFACTOR: Replaced Redux action with local state update for selected product price
        setSelectedProductPrice(productPrice);
        setEditMode(2);
    }

    function cancelEdit() {
        setEditMode(0);
        // REFACTOR: Clear local state when cancelling edit
        setSelectedProductPrice(null);
    }

    const productPriceCell = (props: any) => {
        return (
            <td>
                <Button
                    onClick={() => handleSelectProductPrice(props.dataItem)}
                >
                    {props.dataItem.productPriceTypeDescription}
                </Button>
            </td>
        );
    };

    if (editMode) {
        return <ProductPricesForm productPrice={selectedProductPrice} cancelEdit={cancelEdit} editMode={editMode} />;
    }
    
    console.log('selectedProduct:', selectedProduct);

    return (
        <>
            <CatalogMenu selectedMenuItem="/products" />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container columnSpacing={1}>
                    <Grid container alignItems="center">
                        <Grid item xs={8}>
                            <Typography sx={{ p: 2 }} variant="h4">
                                {getTranslatedLabel(`${localizationKey}.title`, 'Prices for')} {selectedProduct.productName}
                            </Typography>
                        </Grid>
                    </Grid>

                    <Grid container p={2}>
                        <div className="div-container">
                            <KendoGrid
                                className="main-grid"
                                style={{ height: '300px' }}
                                data={orderBy(productPrices ? productPrices : [], sort).slice(page.skip, page.take + page.skip)}
                                sortable
                                sort={sort}
                                onSortChange={(e: GridSortChangeEvent) => {
                                    setSort(e.sort);
                                }}
                                skip={page.skip}
                                take={page.take}
                                total={productPrices ? productPrices.length : 0}
                                pageable
                                onPageChange={pageChange}
                            >
                                <GridToolbar>
                                    <Grid item xs={3}>
                                        <Button
                                            color="secondary"
                                            onClick={() => setEditMode(1)}
                                            variant="outlined"
                                            disabled={isLoading}
                                        >
                                            {getTranslatedLabel(`${localizationKey}.create`, 'Create Product Price')}
                                        </Button>
                                    </Grid>
                                </GridToolbar>

                                <Column
                                    field="productPriceTypeDescription"
                                    title={getTranslatedLabel(`${localizationKey}.type`, 'Price Type')}
                                    cell={productPriceCell}
                                    width={300}
                                />
                                <Column
                                    field="currencyUomDescription"
                                    title={getTranslatedLabel(`${localizationKey}.currency`, 'Currency')}
                                    width={300}
                                />
                                <Column
                                    field="price"
                                    title={getTranslatedLabel(`${localizationKey}.price`, 'Price')}
                                    width={150}
                                />
                                <Column field="rowVersion" title="rowVersion" width={0} />
                                <Column
                                    field="fromDate"
                                    title={getTranslatedLabel(`${localizationKey}.from`, 'From')}
                                    width={100}
                                    format="{0: dd/MM/yyyy}"
                                />
                                <Column
                                    field="thruDate"
                                    title={getTranslatedLabel(`${localizationKey}.thru`, 'To')}
                                    width={100}
                                    format="{0: dd/MM/yyyy}"
                                />
                            </KendoGrid>
                            <Grid item xs={3} p={1}>
                                <Button variant="contained" color="error" onClick={handleBackClick}>
                                    {getTranslatedLabel('general.back', 'Back')}
                                </Button>
                            </Grid>
                        </div>
                    </Grid>
                </Grid>
            </Paper>
        </>
    );
}