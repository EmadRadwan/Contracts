import {debounce, TextField} from "@mui/material";
import {useState} from "react";
import {useAppDispatch, useAppSelector} from "../../../app/store/configureStore";
import {setPartyParams} from "../slice/partySlice";

export default function PartySearch() {
    const {partyParams} = useAppSelector(state => state.party);
    const [searchTerm, setSearchTerm] = useState(partyParams.searchTerm);
    const dispatch = useAppDispatch();

    const debouncedSearch = debounce((event: any) => {
        dispatch(setPartyParams({searchTerm: event.target.value}))
    }, 1000)

    return (
        <TextField
            label='Search parties'
            variant='outlined'
            fullWidth
            value={searchTerm || ''}
            onChange={(event: any) => {
                setSearchTerm(event.target.value);
                debouncedSearch(event);
            }}
        />
    )
}