import {
    createAsyncThunk,
    createEntityAdapter,
    createSelector,
    createSlice,
    EntityState,
    PayloadAction
} from "@reduxjs/toolkit";
import agent from "../../../app/api/agent";
import {RootState} from "../../../app/store/configureStore";
import {ProductCategory} from "../../../app/models/product/productCategory";
import {ProductCategoryMember} from "../../../app/models/product/productCategoryMember";
import {handleDatesArray, handleDatesObject} from "../../../app/util/utils";

interface ProductCategoryState extends EntityState<ProductCategoryMember> {
    productCategoryMembersLoaded: boolean;
    productCategoriesLoaded: boolean;
    productCategories: ProductCategory[];
    status: string;
    selectedProductCategoryMemberId: string | undefined;

}


const productCategoriesAdapter = createEntityAdapter<ProductCategoryMember>({
    selectId: (productCategory) => productCategory.productCategoryId.concat(productCategory.productId
        , productCategory.fromDate.toString()
    )
});


export const fetchProductCategoryMembersAsync = createAsyncThunk<ProductCategoryMember[], string, { state: RootState }>(
    'productCategoryMembers/fetchProductCategoryMembersAsync',
    async (productId, thunkAPI) => {
        try {
            return await agent.ProductCategories.getProductCategories(productId);
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)
export const fetchProductCategoriesAsync = createAsyncThunk<ProductCategory[]>(
    'productCategory/fetchProductCategoriesAsync',
    async (_, thunkAPI) => {
        try {
            return await agent.ProductCategories.list();
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)


export const initialState: ProductCategoryState = productCategoriesAdapter.getInitialState({
    productCategoryMembersLoaded: false,
    productCategoriesLoaded: false,
    status: 'idle',
    productCategories: [],
    selectedProductCategoryMemberId: undefined

})

export const productCategorySlice = createSlice({
    name: 'productCategory',
    initialState: initialState,
    reducers: {
        addProductCategoryMember: (state, action) => {
            productCategoriesAdapter.upsertOne(state, action.payload);
        },
        updateProductCategoryMember: (state, action) => {
            productCategoriesAdapter.upsertOne(state, action.payload);

        },
        removeProductCategoryMember: (state, action) => {
            productCategoriesAdapter.removeOne(state, action.payload);
            state.productCategoryMembersLoaded = false;
        },
        selectProductCategoryMemberId(state, action: PayloadAction<string>) {
            state.selectedProductCategoryMemberId = action.payload
        }
    },
    extraReducers: (builder => {
        builder.addCase(fetchProductCategoryMembersAsync.pending, (state) => {
            state.status = 'pendingFetchProductCategoryMembers';
        });
        builder.addCase(fetchProductCategoryMembersAsync.fulfilled, (state, action) => {
            productCategoriesAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.productCategoryMembersLoaded = true;
        });
        builder.addCase(fetchProductCategoryMembersAsync.rejected, (state, action) => {
            //console.log(action.payload);
            state.status = 'idle';
        });
        builder.addCase(fetchProductCategoriesAsync.pending, (state) => {
            state.status = 'pendingFetchProductCategories';
        });
        builder.addCase(fetchProductCategoriesAsync.fulfilled, (state, action) => {
            state.productCategories = action.payload
            state.status = 'idle';
            state.productCategoriesLoaded = true;
        });
        builder.addCase(fetchProductCategoriesAsync.rejected, (state, action) => {
            state.status = 'idle';
        });
    })
})
const productCategoryMemberSelector = (state: RootState) => state.productCategory

export const productCategoriesSelectors = productCategoriesAdapter.getSelectors((state: RootState) => state.productCategory);
export const {
    addProductCategoryMember,
    updateProductCategoryMember,
    removeProductCategoryMember,
    selectProductCategoryMemberId
} = productCategorySlice.actions;

export const {selectEntities} = productCategoriesSelectors


const getSelectedProductCategoryMemberId = createSelector(
    productCategoryMemberSelector,
    (ProductCategoryMember) => ProductCategoryMember.selectedProductCategoryMemberId
)

export const getSelectedProductCategoryMemberEntity = createSelector(
    selectEntities,
    getSelectedProductCategoryMemberId,
    (entities, id) => id && handleDatesObject(entities[id])
)


export const getModifiedProductCategories = createSelector(
    productCategoriesSelectors.selectAll,
    (entities) => {
        if (entities.length > 0) {
            return handleDatesArray(entities)
        } else {
            return entities
        }
    }
)