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
import {FacilityInventory, FacilityInventoryParams} from "../../../app/models/facility/facilityInventory";
import {MetaData} from "../../../app/models/pagination";

interface FacilityInventoryState extends EntityState<FacilityInventory> {
    facilityInventoriesLoaded: boolean;
    status: string;
    facilityInventoryParams: FacilityInventoryParams;
    selectedProductId: string | undefined;
    metaData: MetaData | null;

}


const facilityInventorysAdapter = createEntityAdapter<FacilityInventory>({
    selectId: (FacilityInventory) => FacilityInventory.productId,
});


export const fetchFacilityInventorysAsync = createAsyncThunk<FacilityInventory[], void, { state: RootState }>(
    'facilityInventory/fetchFacilityInventorysAsync',
    async (_, thunkAPI) => {
        const params = getAxiosParams(thunkAPI.getState().facilityInventory.facilityInventoryParams);
        try {
            const response = await agent.Facilities.listFacilityInventoriesByProduct(params);
            thunkAPI.dispatch(setMetaData(response.metaData));
            return response.items;
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)


function getAxiosParams(facilityInventoryParams: FacilityInventoryParams) {
    const params = new URLSearchParams();
    params.append('pageNumber', facilityInventoryParams.pageNumber!.toString());
    params.append('pageSize', facilityInventoryParams.pageSize!.toString());
    params.append('facilityId', facilityInventoryParams.facilityId!);
    if (facilityInventoryParams.searchTerm) params.append('searchTerm', facilityInventoryParams.searchTerm);

    return params;
}

function initParams() {
    return {
        pageNumber: 1,
        pageSize: 6,
        facilityId: '',
        productId: '',

    }
}

export const initialState: FacilityInventoryState = facilityInventorysAdapter.getInitialState({
    facilityInventoriesLoaded: false,
    status: 'idle',
    facilityInventoryParams: initParams(),
    selectedProductId: undefined,
    metaData: null,
})

export const facilityInventorySlice = createSlice({
    name: 'facilityInventory',
    initialState: initialState,
    reducers: {
        setFacilityInventoryParams: (state, action) => {
            state.facilityInventoriesLoaded = false;
            state.facilityInventoryParams = {...state.facilityInventoryParams, ...action.payload, pageNumber: 1};
        },
        setPageNumber: (state, action) => {
            state.facilityInventoriesLoaded = false;
            state.facilityInventoryParams = {...state.facilityInventoryParams, ...action.payload};
        },
        setMetaData: (state, action) => {
            state.metaData = action.payload;
        },
        resetFacilityInventoryParams: (state) => {
            state.facilityInventoryParams = initParams();
        },
        setFacilityInventory: (state, action) => {
            facilityInventorysAdapter.upsertOne(state, action.payload);
        },
        removeFacilityInventory: (state, action) => {
            facilityInventorysAdapter.updateOne(state, action.payload[0]);
            state.facilityInventoriesLoaded = false;
        },
        selectProductById(state, action: PayloadAction<string>) {
            state.selectedProductId = action.payload
        }
    },
    extraReducers: (builder => {
        builder.addCase(fetchFacilityInventorysAsync.pending, (state) => {
            state.status = 'pendingFetchFacilityInventorys';
        });
        builder.addCase(fetchFacilityInventorysAsync.fulfilled, (state, action) => {
            facilityInventorysAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.facilityInventoriesLoaded = true;
        });
        builder.addCase(fetchFacilityInventorysAsync.rejected, (state, action) => {
            state.status = 'idle';
        });

    })
})

const facilityInventorySelector = (state: RootState) => state.facilityInventory
export const {
    selectProductById,
    resetFacilityInventoryParams,
    setFacilityInventory,
    removeFacilityInventory,
    setPageNumber,
    setFacilityInventoryParams,
    setMetaData
} = facilityInventorySlice.actions;
export const facilityInventorySelectors = facilityInventorysAdapter.getSelectors((state: RootState) => state.facilityInventory);
export const {selectEntities} = facilityInventorySelectors

const getSelectedFacilityInventoryId = createSelector(
    facilityInventorySelector,
    (facilityInventory) => facilityInventory.selectedProductId
)

export const getSelectedFacilityInventoryIdEntity = createSelector(
    selectEntities,
    getSelectedFacilityInventoryId,
    (entities, id) => id && entities[id]
)

export const getModifiedFacilityInventories = createSelector(
    facilityInventorySelectors.selectAll,
    (entities) => {
        if (entities.length > 0) {
            return entities
        } else {
            return entities
        }
    }
)
