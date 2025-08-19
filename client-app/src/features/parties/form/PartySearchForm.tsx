import React, {useEffect, useState} from "react";
import {Form, FormElement} from "@progress/kendo-react-form";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {debounce, Paper, TextField} from "@mui/material";
import {PartyParams} from "../../../app/models/party/party";
import RadioButtonGroup from "../../../app/components/RadioButtonGroup";
import CheckboxButtons from "../../../app/components/CheckboxButtons";
import {useFetchRoleTypesQuery} from "../../../app/store/apis";
import {RoleType} from "../../../app/models/common/roleType";
import {setPartyParams} from "../slice/partySlice";


interface Props {
    params: PartyParams;
    show: boolean;
    onClose: () => void;
    onSubmit: (partyParam: PartyParams) => void;
}

export default function PartySearchForm({params, onSubmit, show, onClose}: Props) {
    const [partyTypeDesc, setPartyTypeDesc] = useState<string[]>()
    const {data: roleTypes} = useFetchRoleTypesQuery(undefined);

    const [orderBy, setOderBy] = useState(params.orderBy)
    const [roleTypeArray, setRoleTypeArray] = useState<string[]>()
    const [searchTerm, setSearchTerm] = useState(params.searchTerm);

    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };

    const debouncedSearch = debounce((event: any) => {
        setPartyParams({searchTerm: event.target.value})
    }, 1000)
    // console.log('PartySearchForm.tsx Rendered')


    ////console.log("params", params);
    ////console.log("partyBy", partyBy);
    ////console.log("partyTypeArray", partyTypeArray);

    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, [closeOnEscapeKeyDown]);


    const getPartyTypes = (): string[] => {
        return roleTypes!.map(type => type.description)
    }


    const handleChangedCheckBoxButtons = (items: string[]) => {
        const filteredPartyTypes = roleTypes!.filter((type: RoleType) => items.includes(type.description))
        const values: any[] = filteredPartyTypes.map((type: RoleType) => type.roleTypeId)

        const descArray = filteredPartyTypes.map((type: RoleType) => type.description)
        setPartyTypeDesc(descArray.length > 0 ? descArray : [''])
        setRoleTypeArray(values)
    }


    const sortOptions = [
        {value: 'partyIdAsc', label: 'Party ID Asc'},
        {value: 'partyIdDesc', label: 'Party ID Desc'},
        {value: 'createdStampAsc', label: 'Party Date Asc'},
        {value: 'createdStampDesc', label: 'Party Date Desc'},
    ]

    async function handleSubmitData(data: any) {
        const partyParam: PartyParams = {orderBy: orderBy, roleTypes: roleTypeArray!}
        onSubmit(partyParam)
        onClose();
    }


    return ReactDOM.createPortal(
        <CSSTransition
            in={show}
            unmountOnExit
            timeout={{enter: 0, exit: 300}}
        >
            <div className="modal">
                <div className="modal-content" onClick={e => e.stopPropagation()}>
                    <div className="div-container-withBpartyBox">
                        <Form
                            initialValues={params}
                            onSubmitClick={values => handleSubmitData(values)}
                            render={(formRenderProps) => (

                                <FormElement>
                                    <fieldset className={'k-form-fieldset'}>


                                        <div className="k-form-buttons">
                                            <Grid container rowSpacing={1}>
                                                <Grid item xs={8}>
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
                                                </Grid>
                                                <Grid item xs={8}>
                                                    <Paper sx={{mb: 2, p: 2}}>
                                                        <RadioButtonGroup
                                                            selectedValue={orderBy}
                                                            options={sortOptions}
                                                            onChange={(e) => setOderBy(e.target.value)}
                                                        />
                                                    </Paper>

                                                </Grid><Grid item xs={8}>
                                                <Paper sx={{mb: 2, p: 2}}>
                                                    <CheckboxButtons
                                                        items={getPartyTypes()}
                                                        checked={partyTypeDesc}
                                                        onChange={handleChangedCheckBoxButtons}

                                                    />
                                                </Paper>

                                            </Grid>


                                            </Grid>
                                            <Grid item xs={3}>
                                                <Button
                                                    variant="contained"
                                                    type={'submit'}
                                                >
                                                    Find
                                                </Button>
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Button onClick={() => onClose()} variant="contained">
                                                    Cancel
                                                </Button>
                                            </Grid>

                                        </div>

                                    </fieldset>

                                </FormElement>

                            )}
                        />
                    </div>

                </div>
            </div>
        </CSSTransition>,
        document.getElementById("root")!
    );
}
export const PartySearchFormMemo = React.memo(PartySearchForm)
