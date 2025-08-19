import {createSlice} from '@reduxjs/toolkit';
import {loadMessages} from '@progress/kendo-react-intl';
// import {messages} from "../messages/messages";
import enDefaults from "../messages/shared-en-defaults.json";
import arDefaults from "../messages/shared-ar-defaults.json";
import trDefaults from "../messages/shared-tr-defaults.json";
import jsonMessages from "../messages/en.json";
import jsonArMessages from "../messages/ar.json";
import jsonTrMessages from "../messages/tr.json";
import jsonTrCustomMessages from "../messages/tr-with-pronunciation.json";

interface InitialState {
    language: string
    languages: string[]
}

const initialState: InitialState = {
    language: localStorage?.getItem("selectedLang") ?? 'en',
    languages: ['en', 'ar', 'tr'],
};
if (initialState.language === 'en') {
    loadMessages(enDefaults, 'en');
    loadMessages(jsonMessages, 'en');
} else if (initialState.language === 'ar') {
    loadMessages(arDefaults, 'ar');
    loadMessages(jsonArMessages, 'ar');
    loadMessages(jsonTrCustomMessages, 'tr')
} else if (initialState.language === "tr") {
    loadMessages(trDefaults, 'tr')
    loadMessages(jsonTrMessages, 'tr')
    loadMessages(jsonArMessages, 'ar');
}
export const localizationSlice = createSlice({
    name: 'localization',
    initialState,
    reducers: {
        setLanguage: (state, action) => {
            state.language = action.payload;
        }
    }
});

export const {setLanguage} = localizationSlice.actions;
