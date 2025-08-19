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
import {Facility} from "../../../app/models/facility/facility";
import {FacilityType} from "../../../app/models/facility/facilityType";
import {handleDatesObject} from "../../../app/util/utils";

type ProductLov = {
    productId: string | undefined,
    productName: string | undefined
}

interface FacilityState extends EntityState<Facility> {
    facilitiesLoaded: boolean;
    facilityTypesLoaded: boolean;
    status: string;
    facilityTypes: FacilityType[];
    selectedFacilityId: string | undefined;
    selectedPhysicalInventoryProductId: ProductLov | undefined;
}


const facilitiesAdapter = createEntityAdapter<Facility>({
    selectId: (Facility) => Facility.facilityId
});


const facilityTypesAdapter = createEntityAdapter<FacilityType>({
    selectId: (FacilityType) => FacilityType.facilityTypeId
});


export const fetchFacilitiesAsync = createAsyncThunk<Facility[], void, { state: RootState }>(
    'facility/fetchFacilitiesAsync',
    async (_, thunkAPI) => {
        try {
            return await agent.Facilities.list();
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)


export const fetchFacilityTypesAsync = createAsyncThunk<FacilityType[]>(
    'facility/fetchFacilityTypesAsync',
    async (_, thunkAPI) => {
        try {
            return await agent.FacilityTypes.list();
        } catch (error: any) {
            return thunkAPI.rejectWithValue({error: error.data})
        }
    }
)


export const initialState: FacilityState = facilitiesAdapter.getInitialState({
    facilitiesLoaded: false,
    status: 'idle',
    facilityTypes: [],
    facilityTypesLoaded: false,
    selectedFacilityId: undefined,
    selectedPhysicalInventoryProductId: undefined
})

export const facilitySlice = createSlice({
    name: 'facilityPrice',
    initialState: initialState,
    reducers: {
        setFacility: (state, action) => {
            facilitiesAdapter.upsertOne(state, action.payload);
        },
        removeFacility: (state, action) => {
            facilitiesAdapter.removeOne(state, action.payload);
            state.facilitiesLoaded = false;
        },
        selectFacilityId(state, action: PayloadAction<string | undefined>) {
            state.selectedFacilityId = action.payload
        },
        setSelectedPhysicalInventoryProductId(state, action: PayloadAction<ProductLov | undefined>) {
            state.selectedPhysicalInventoryProductId = action.payload
        }
    },
    extraReducers: (builder => {
        builder.addCase(fetchFacilitiesAsync.pending, (state) => {
            state.status = 'pendingFetchFacilities';
        });
        builder.addCase(fetchFacilitiesAsync.fulfilled, (state, action) => {
            facilitiesAdapter.setAll(state, action.payload);
            state.status = 'idle';
            state.facilitiesLoaded = true;
        });
        builder.addCase(fetchFacilitiesAsync.rejected, (state, action) => {
            //console.log(action.payload);
            state.status = 'idle';
        });
        builder.addCase(fetchFacilityTypesAsync.pending, (state) => {
            state.status = 'pendingFetchFacilityTypes';
        });
        builder.addCase(fetchFacilityTypesAsync.fulfilled, (state, action) => {
            state.facilityTypes = action.payload;
            state.status = 'idle';
            state.facilityTypesLoaded = true;
        });
        builder.addCase(fetchFacilityTypesAsync.rejected, (state, action) => {
            state.status = 'idle';
        });
    })
})

export const {setFacility, removeFacility, selectFacilityId, setSelectedPhysicalInventoryProductId} = facilitySlice.actions;
export const facilitiesSelectors = facilitiesAdapter.getSelectors((state: RootState) => state.facility);
export const {selectEntities} = facilitiesSelectors

const facilitySelector = (state: RootState) => state.facility

const getSelectedFacilityId = createSelector(
    facilitySelector,
    (facility) => facility.selectedFacilityId
)

export const getSelectedFacilityIdEntity = createSelector(
    selectEntities,
    getSelectedFacilityId,
    (entities, id) => id && handleDatesObject(entities[id])
)

