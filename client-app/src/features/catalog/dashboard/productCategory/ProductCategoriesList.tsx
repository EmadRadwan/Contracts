import React, {useEffect, useState} from "react";
import {
    Grid as KendoGrid,
    GridColumn as Column,
    GridPageChangeEvent,
    GridSortChangeEvent,
    GridToolbar,
} from "@progress/kendo-react-grid";
import {orderBy, SortDescriptor, State} from "@progress/kendo-data-query";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import {Navigate, useNavigate} from "react-router";
import {Grid, Paper, Typography} from "@mui/material";
import Button from "@mui/material/Button";

import {
    fetchProductCategoryMembersAsync,
    getModifiedProductCategories,
    getSelectedProductCategoryMemberEntity,
    selectProductCategoryMemberId
} from "../../slice/productCategorySlice";
import ProductCategoryForm from "../../form/productCategory/ProductCategoryForm";
import CatalogMenu from "../../menu/CatalogMenu";

export default function ProductCategoriesList() {
    const initialSort: Array<SortDescriptor> = [
        {field: "categoryName", dir: "desc"},
    ];
    const [sort, setSort] = React.useState(initialSort);
    const initialDataState: State = {skip: 0, take: 4};
    const [page, setPage] = React.useState<any>(initialDataState);
    const pageChange = (event: GridPageChangeEvent) => {
        setPage(event.page);
    };
    const navigate = useNavigate();


    const {productCategoryMembersLoaded} = useAppSelector(state => state.productCategory);
    const dispatch = useAppDispatch();
    const [editMode, setEditMode] = useState(0);
    const selectedProductCategoryMember = useAppSelector(getSelectedProductCategoryMemberEntity)

    const selectProductCategoryMember = (productCategoryId: string) => dispatch(selectProductCategoryMemberId(productCategoryId))

    const selectedProduct = useAppSelector(state => state.productUi.selectedProduct);
    const productCategoryMembers2 = useAppSelector(getModifiedProductCategories)


    useEffect(() => {
        if (!productCategoryMembersLoaded) if (selectedProduct instanceof Object) {
            dispatch(fetchProductCategoryMembersAsync(selectedProduct.productId));
        }
    }, [productCategoryMembersLoaded, dispatch, selectedProduct])

    function handleBackClick() {
        navigate('/products', {state: {myStateProp: 'bar'}});
    }

    if (!selectedProduct) {
        return <Navigate to="/products"/>
    }

    function handleSelectProductCategoryMember(productCategoryId: string) {
        //console.log('productCategoryId', productCategoryId)

        selectProductCategoryMember(productCategoryId)
        setEditMode(2);
    }

    function cancelEdit() {
        setEditMode(0);
    }


    const productCategoryCell = (props: any) => {
        return (
            <td>
                <Button
                    onClick={() => handleSelectProductCategoryMember(props.dataItem.productCategoryId.concat(props.dataItem.productId
                        , props.dataItem.fromDate.toISOString().split('.')[0] + "Z"))}
                >
                    {props.dataItem.description}
                </Button>


            </td>
        )
    }


    if (editMode) {
        return <ProductCategoryForm productCategoryMember
                                        ={selectedProductCategoryMember} cancelEdit={cancelEdit} editMode={editMode}/>
    }

    return (
        <>
            <CatalogMenu selectedMenuItem={'products'}/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container columnSpacing={1}>
                    <Grid container alignItems="center">
                        <Grid item xs={12}>
                            <Typography sx={{p: 2}} variant='h4'>Categories
                                for {selectedProduct.productName}</Typography>
                        </Grid>


                    </Grid>

                    <Grid container p={2}>
                        <div className="div-container">
                            <KendoGrid className="main-grid" style={{height: "300px", width: "700px"}}
                                       data={orderBy(productCategoryMembers2, sort).slice(page.skip, page.take + page.skip)}
                                       sortable={true}
                                       sort={sort}
                                       onSortChange={(e: GridSortChangeEvent) => {
                                           setSort(e.sort);
                                       }}
                                       skip={page.skip}
                                       take={page.take}
                                       total={productCategoryMembers2.length}
                                       pageable={true}
                                       onPageChange={pageChange}

                            >
                                <GridToolbar>
                                    <Grid item xs={3}>
                                        <Button color={"secondary"} onClick={() => setEditMode(1)} variant="outlined">
                                            Create Product Category
                                        </Button>
                                    </Grid>
                                </GridToolbar>

                                <Column field="description" title="Product Category" cell={productCategoryCell}
                                        width={300}/>
                                <Column field="fromDate" title="From" width={100} format="{0: dd/MM/yyyy}"/>
                                <Column field="thruDate" title="To" width={100} format="{0: dd/MM/yyyy}"/>
                                <Column field="productId" title="productId" width={0}/>
                                <Column field="productCategoryId" title="productCategoryId" width={0}/>

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
    )
}


