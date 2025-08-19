import {createSlice, PayloadAction} from "@reduxjs/toolkit";


interface AppState {
    headerSelectedMenu: string;
}

export const appInitialState: AppState = {
    headerSelectedMenu: '',
};


export const appUiSlice = createSlice({
    name: "appUiSlice",
    initialState: appInitialState,
    reducers: {
        setHeaderSelectedMenu(state, action: PayloadAction<string>) {
            state.headerSelectedMenu = action.payload;
        },
    },
});

export const {
    setHeaderSelectedMenu,
} = appUiSlice.actions;

