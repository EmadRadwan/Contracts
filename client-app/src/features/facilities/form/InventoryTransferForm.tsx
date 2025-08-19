import React, {useState} from 'react';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import {Field, Form, FormElement} from '@progress/kendo-react-form';

import {Box, Paper, Typography} from "@mui/material";
import {useFetchFacilitiesQuery} from "../../../app/store/configureStore";
import {requiredValidator} from "../../../app/common/form/Validators";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import {InventoryTransfer} from "../../../app/models/facility/inventoryTransfer";
import FacilityMenu from "../menu/FacilityMenu";
import useInventoryTransfer from "../hook/useInventoryTransfer";
import {InventoryItem} from "../../../app/models/facility/inventoryItem";
import {Facility} from "../../../app/models/facility/facility";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import FormTextArea from "../../../app/common/form/FormTextArea";

interface Props {
    selectedInventoryTransfer?: InventoryTransfer
    inventoryItem?: InventoryItem;
    editMode: number;
    cancelEdit: () => void;
    setShouldShowTransferForm: (value: boolean) => void;
}


export default function InventoryTransferForm({
                                                  selectedInventoryTransfer,
                                                  inventoryItem,
                                                  cancelEdit,
                                                  editMode,
                                                  setShouldShowTransferForm
                                              }: Props) {
    const [isLoading, setIsLoading] = useState(false);
    const [selectedMenuItem, setSelectedMenuItem] = React.useState('');
    const [formKey, setFormKey] = React.useState(Math.random());
    const [allowedFacilities, setAllowedFacilities] = React.useState<any[]>([]);

    const {data: facilities, error, isFetching} = useFetchFacilitiesQuery(undefined);
    const {
        inventoryTransfer,
        setInventoryTransfer,
        formEditMode,
        setFormEditMode,
        handleCreate
    } = useInventoryTransfer({
        selectedMenuItem,
        editMode,
        selectedInventoryTransfer,
        setIsLoading, inventoryItem
    });

    console.log('facilities', facilities);
    console.log('inventoryItem', inventoryItem);
    console.log('allowedFacilities', allowedFacilities);

    // if facilities are fetched, set allowedFacilities where the facilityId is not 
    // equal to the inventoryItem facilityId, and map facilityId to be facilityIdTo
    React.useEffect(() => {
        if (facilities) {
            const allowedFacilities = facilities.filter((facility: Facility) => facility.facilityId !== inventoryItem?.facilityId)
                .map((facility: Facility) => ({
                    facilityIdTo: facility.facilityId,
                    facilitytName: facility.facilityName
                }));
            setAllowedFacilities(allowedFacilities);
        }
    }, [facilities]);

    async function handleSubmit(data: any) {
        setIsLoading(true);
        handleCreate(data);
    }

    const handleCancelForm = () => {
        setShouldShowTransferForm(false);
        setInventoryTransfer(undefined);
        setFormKey(Math.random());
        cancelEdit()
    };

    return (
        <>
            <FacilityMenu/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Form
                    initialValues={inventoryTransfer}
                    onSubmit={values => handleSubmit(values)}
                    render={(formRenderProps) => (

                        <FormElement>
                            <fieldset className={'k-form-fieldset'}>
                                <Box display="flex" flexDirection="column">
                                    <Typography sx={{pl: 1, mb: 2}} color="black" variant="h6">
                                        <span style={{display: 'inline-block', width: '150px'}}>Inventory Item:</span>
                                        <span style={{fontWeight: "bold"}}>
                                            {inventoryItem?.inventoryItemId}
                                        </span>
                                    </Typography>

                                    <Typography sx={{pl: 1, mb: 2}} color="black" variant="h6">
                                        <span style={{display: 'inline-block', width: '150px'}}>Product:</span>
                                        <span style={{fontWeight: "bold"}}>
                                            {inventoryItem?.productName}
                                        </span>
                                    </Typography>

                                    <Typography sx={{pl: 1, mb: 2}} color="black" variant="h6">
                                        <span style={{display: 'inline-block', width: '150px'}}>From Facility:</span>
                                        <span style={{fontWeight: "bold"}}>
                                            {inventoryItem?.facilityName}
                                        </span>
                                    </Typography>

                                    <Typography sx={{pl: 1, mb: 2}} color="black" variant="h6">
                                        <span style={{display: 'inline-block', width: '150px'}}>ATP/QOH:</span>
                                        <span style={{fontWeight: "bold"}}>
                                             {inventoryItem?.availableToPromiseTotal} / {inventoryItem?.quantityOnHandTotal}
                                        </span>
                                    </Typography>

                                    <Typography sx={{pl: 1, mb: 2}} color="black" variant="h6">
                                        <span style={{display: 'inline-block', width: '150px'}}>Transfer Status:</span>
                                        <span style={{fontWeight: "bold"}}>
                                             Requested
                                        </span>
                                    </Typography>
                                </Box>
                                <Grid container spacing={2}>
                                    <Grid item xs={2}>
                                        <Grid container spacing={2} direction={"column"}>
                                            <Grid item xs={2}>
                                                <Field
                                                    id={'sendDate *'}
                                                    name={'sendDate'}
                                                    label={'Send Date'}
                                                    component={FormDatePicker}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>

                                            <Grid item xs={2}>
                                                <Field
                                                    id={"facilityIdTo"}
                                                    name={"facilityIdTo"}
                                                    label={"To Facility*"}
                                                    component={MemoizedFormDropDownList}
                                                    dataItemKey={"facilityId"}
                                                    textField={"facilitytName"}
                                                    data={allowedFacilities ? allowedFacilities : []}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>

                                            <Grid item xs={2}>
                                                <Field
                                                    id={"transferQuantity"}
                                                    format="n2"
                                                    min={0}
                                                    name={"transferQuantity"}
                                                    label={"Quantity to Transfer *"}
                                                    component={FormNumericTextBox}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>

                                            <Grid item xs={2}>
                                                <Field
                                                    id={"comments"}
                                                    name={"comments"}
                                                    label={"Comments"}
                                                    component={FormTextArea}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                        </Grid>

                                    </Grid>
                                </Grid>


                                <div className="k-form-buttons">
                                    <Grid container rowSpacing={2}>
                                        <Grid item xs={1}>
                                            <Button
                                                variant="contained"
                                                type={'submit'}
                                                color='success'
                                                disabled={!formRenderProps.allowSubmit}
                                            >
                                                Submit
                                            </Button>
                                        </Grid>
                                        <Grid item xs={1}>
                                            <Button onClick={handleCancelForm} color='error' variant="contained">
                                                Cancel
                                            </Button>
                                        </Grid>

                                    </Grid>
                                </div>


                            </fieldset>

                        </FormElement>

                    )}
                />

            </Paper>

        </>
    );
}

// fields required for the form:
// inventoryItemId
// productId, and name
// inventory comments ?! needs clarification, is it coming from the inventoryItem form?
// ATP/QOH
// transfer status
// transfer send date
// facilityIdTo
// comments
// transfer quantity

