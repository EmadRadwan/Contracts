import {createSlice, PayloadAction} from "@reduxjs/toolkit";
import {WorkEffort} from "../../../app/models/manufacturing/workEffort";


interface ManufacturingSharedState {
    productionRunStatusDescription: string | undefined;
    jobRunUnderProcessing : WorkEffort | undefined;
    inventoryProduced: boolean
}

export const manufacturingSharedInitialState: ManufacturingSharedState = {
    productionRunStatusDescription: undefined,
    jobRunUnderProcessing : undefined,
    inventoryProduced: false
};


export const manufacturingSharedSlice = createSlice({
    name: "manufacturingSharedUi",
    initialState: manufacturingSharedInitialState,
    reducers: {
       setProductionRunStatusDescription(state, action: PayloadAction<string | undefined>) {
            state.productionRunStatusDescription = action.payload;
        },
        setJobRunUnderProcessing(state, action: PayloadAction<WorkEffort | undefined>) {
            const updatedJobRunUnderProcessing = { ...state.jobRunUnderProcessing, ...action.payload };

            state.jobRunUnderProcessing = updatedJobRunUnderProcessing
        },
        clearJobRunUnderProcessing(state, action: PayloadAction<WorkEffort | undefined>) {
            state.jobRunUnderProcessing = undefined;
        },
        setInventoryProduced(state, action: PayloadAction<boolean>) {
            state.inventoryProduced = action.payload;
        }
    }
});

export const {
    setProductionRunStatusDescription,
    setJobRunUnderProcessing, clearJobRunUnderProcessing, setInventoryProduced
} = manufacturingSharedSlice.actions;


