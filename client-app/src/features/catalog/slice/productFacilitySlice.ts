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
import {handleDatesArray, handleDatesObject} from "../../../app/util/utils";
import {ProductFacility} from "../../../app/models/product/productFacility";

interface ProductFacilityState extends EntityState<ProductFacility> {
    productFacilitiesLoaded: boolean;
    status: string;
    selectedProductFacilityId: string | undefined;
}


const productFacilitiesAdapter = createEntityAdapter<ProductFacility>({
    selectId: (ProductFacility) => ProductFacility.productId.concat(ProductFacility.facilityId)
});


export const fetchProductFacilitiesAsync = createAsyncThunk<ProductFacility[], string, { state: RootState }>(
    'productFacility/fetchProductFacilitiesAsync',
    async (productId, thunkAPI) => {
        try {
            return await agent.Facilities.getProductFacilities(productId);
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)


export const initialState: ProductFacilityState = productFacilitiesAdapter.getInitialState({
    productFacilitiesLoaded: false,
    status: 'idle',
    selectedProductFacilityId: undefined
})

export const productFacilitySlice = createSlice({
    name: 'productFacility',
    initialState: initialState,
    reducers: {
        addProductFacility: (state, action) => {
            productFacilitiesAdapter.upsertOne(state, action.payload);
        },
        updateProductFacility: (state, action) => {
            //console.log('state', state.ids[0])
            productFacilitiesAdapter.upsertOne(state, action.payload);

        },
        removeProductFacility: (state, action) => {
            productFacilitiesAdapter.removeOne(state, action.payload);
            state.productFacilitiesLoaded = false;
        },
        selectProductFacilityId(state, action: PayloadAction<string>) {
            state.selectedProductFacilityId = action.payload
        }
    },
    extraReducers: (builder => {
        builder.addCase(fetchProductFacilitiesAsync.pending, (state) => {
            state.status = 'pendingFetchProductFacilities';
        });
        builder.addCase(fetchProductFacilitiesAsync.fulfilled, (state, action) => {
            productFacilitiesAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.productFacilitiesLoaded = true;
        });
        builder.addCase(fetchProductFacilitiesAsync.rejected, (state, action) => {
            //console.log(action.payload);
            state.status = 'idle';
        });
    })
})

const productFacilitySelector = (state: RootState) => state.productFacility
export const {
    addProductFacility,
    updateProductFacility,
    removeProductFacility,
    selectProductFacilityId
} = productFacilitySlice.actions;
export const productFacilitiesSelectors = productFacilitiesAdapter.getSelectors((state: RootState) => state.productFacility);
export const {selectEntities} = productFacilitiesSelectors

const getSelectedProductFacilityId = createSelector(
    productFacilitySelector,
    (productFacility) => productFacility.selectedProductFacilityId
)

export const getSelectedProductFacilityIdEntity = createSelector(
    selectEntities,
    getSelectedProductFacilityId,
    (entities, id) => id && handleDatesObject(entities[id])
)


export const getModifiedProductFacilities = createSelector(
    productFacilitiesSelectors.selectAll,
    (entities) => {
        if (entities.length > 0) {
            return handleDatesArray(entities)
        } else {
            return entities
        }
    }
)