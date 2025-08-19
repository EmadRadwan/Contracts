import React, {useEffect, useState} from "react";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {Paper} from "@mui/material";
import {VehicleParams} from "../../../app/models/service/vehicle";
import RadioButtonGroup from "../../../app/components/RadioButtonGroup";
import CheckboxButtons from "../../../app/components/CheckboxButtons";
import {
    useFetchVehicleMakesQuery,
    useFetchVehicleModelsByMakeIdQuery,
    useFetchVehicleTypesQuery
} from "../../../app/store/apis";
import {FormComboBoxVirtualCustomer} from "../../../app/common/form/FormComboBoxVirtualCustomer";
import FormInput from "../../../app/common/form/FormInput";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";

//todo: search needs to add search text for customer name and phone number

interface Props {
    params: VehicleParams;
    show: boolean;
    onClose: () => void;
    width?: number
    onSubmit: (vehicleParam: VehicleParams) => void;
}

export default function VehicleSearchForm({params, onSubmit, width, show, onClose}: Props) {
    const [orderBy, setOrderBy] = useState(params.orderBy)

    const [vehicleTypeDesc, setVehicleTypeDesc] = useState<string[]>()
    const [vehicleTypeArray, setVehicleTypeArray] = useState<string[]>()

    const [partyId, setPartyId] = useState<string>("")
    const [customerPhone, setCustomerPhone] = useState("")
    const [selectedVehicleMake, setSelectedVehicleMake] = useState("")
    const [selectedVehicleModel, setSelectedVehicleModel] = useState("")
    const [chassisNumber, setChassisNumber] = useState("")
    const [plateNumber, setPlateNumber] = useState("")
    const [selectedVehicleTypes, setSelectedVehicleTypes] = useState<string[]>()

    // let formRef = useRef<boolean>(false)

    const {data: vehicleTypes} = useFetchVehicleTypesQuery(undefined)
    const {data: vehicleMakes} = useFetchVehicleMakesQuery(undefined)
    const {data: vehicleModels} = useFetchVehicleModelsByMakeIdQuery(selectedVehicleMake,
        {skip: true})

    // eslint-disable-next-line react-hooks/exhaustive-deps
    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };


    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, [closeOnEscapeKeyDown]);

    useEffect(() => {
        setVehicleTypeArray(vehicleTypes?.map(type => type.vehicleTypeDescription))
    }, [vehicleTypes])

    const onVehicleMakeDropdownChange = React.useCallback(
        (event) => {
            const make = event.value;
            setSelectedVehicleMake(make);
        },
        [setSelectedVehicleMake]
    );

    const onVehicleModelDropdownChange = React.useCallback(
        (event) => {
            const model = event.value;
            setSelectedVehicleModel(model);
        },
        [setSelectedVehicleModel]
    );


    // const getVehicleTypes = (): string[] => {
    //     return (vehicleTypes?.length ? vehicleTypes?.map(type => type.vehicleTypeDescription) : [])
    // }

    const handleChangedCheckBoxButtons = (items: string[]) => {
        const filteredVehicleTypes = vehicleTypes ? vehicleTypes.filter(type => items.includes(type.vehicleTypeDescription)) : []
        const values: any[] = filteredVehicleTypes ? filteredVehicleTypes.map(type => type.vehicleTypeId) : []

        const descArray = filteredVehicleTypes.map(type => type.vehicleTypeDescription)
        setVehicleTypeDesc(descArray.length > 0 ? descArray : [''])
        setSelectedVehicleTypes(values)
    }

    const handleChangeCustomer = (customer: any) => {
        setPartyId(customer.value.fromPartyId)
    }


    const sortOptions = [
        {value: 'createdStampAsc', label: 'Vehicle Date Asc'},
        {value: 'createdStampDesc', label: 'Vehicle Date Desc'},
    ]

    async function handleSubmitData(data: any) {
        const vehicleParam: VehicleParams = {
            orderBy: orderBy,
            vehicleTypes: selectedVehicleTypes,
            customerPhone,
            ownerPartyId: partyId,
            makeId: selectedVehicleMake,
            modelId: selectedVehicleModel,
            chassisNumber,
            plateNumber
        }
        onSubmit(vehicleParam)
        // clearing all fields of the form
        setVehicleTypeDesc([])
        setSelectedVehicleTypes([])
        setPartyId("")
        setPlateNumber("")
        setChassisNumber("")
        setCustomerPhone("")
        setSelectedVehicleMake("")
        setSelectedVehicleModel("")
        //
        onClose();
    }


    return ReactDOM.createPortal(
        <CSSTransition
            in={show}
            unmountOnExit
            timeout={{enter: 0, exit: 300}}
        >
            <div className="modal">
                <div className="modal-content" style={{width}} onClick={e => e.stopPropagation()}>
                    <Paper elevation={5} className={`div-container-withBorderCurved`}>
                        <Form
                            initialValues={{}}
                            // key={formRef.current.toString()}
                            onSubmitClick={values => handleSubmitData(values)}
                            render={(formRenderProps) => (

                                <FormElement>
                                    <fieldset className={'k-form-fieldset'}>
                                        <Grid container spacing={2} paddingLeft={2} paddingRight={2}>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"fromPartyId"}
                                                    name={"fromPartyId"}
                                                    label={"Customer"}
                                                    component={FormComboBoxVirtualCustomer}
                                                    autoComplete={"off"}
                                                    onChange={handleChangeCustomer}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={'vehicleSearchByCustomerPhoneNum'}
                                                    name={'vehicleSearchByCustomerPhoneNum'}
                                                    label={'Customer Phone Number'}
                                                    component={FormInput}
                                                    value={customerPhone}
                                                    onChange={(e) => setCustomerPhone(e.target.value)}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                        </Grid>
                                        <Grid container spacing={2} paddingLeft={2} paddingRight={2}>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={'chassisNumber'}
                                                    name={'chassisNumber'}
                                                    label={'Chassis Number'}
                                                    component={FormInput}
                                                    value={chassisNumber}
                                                    onChange={e => setChassisNumber(e.target.value)}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>

                                            <Grid item xs={6}>
                                                <Field
                                                    id={'plateNumber'}
                                                    name={'plateNumber'}
                                                    label={'Plate Number'}
                                                    value={plateNumber}
                                                    onChange={e => setPlateNumber(e.target.value)}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                        </Grid>
                                        <Grid container spacing={2} paddingLeft={2} paddingRight={2} paddingBottom={1}>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"makeId"}
                                                    name={"makeId"}
                                                    label={"Make"}
                                                    component={MemoizedFormDropDownList}
                                                    dataItemKey={"makeId"}
                                                    textField={"makeDescription"}
                                                    data={vehicleMakes ? vehicleMakes : []}
                                                    value={selectedVehicleMake}
                                                    onChange={onVehicleMakeDropdownChange}
                                                />
                                            </Grid>

                                            <Grid item xs={6}>
                                                <Field
                                                    id={"modelId"}
                                                    name={"modelId"}
                                                    label={"Model"}
                                                    component={MemoizedFormDropDownList}
                                                    dataItemKey={"modelId"}
                                                    textField={"modelDescription"}
                                                    data={vehicleModels ? vehicleModels : []}
                                                    disabled={!selectedVehicleMake}
                                                    value={selectedVehicleModel}
                                                    onChange={onVehicleModelDropdownChange}
                                                />
                                            </Grid>
                                        </Grid>

                                        <Grid container spacing={2} paddingLeft={2} paddingRight={2}>
                                            <Grid item xs={6}>
                                                <Paper sx={{mb: 1, p: 2}}>
                                                    <RadioButtonGroup
                                                        selectedValue={orderBy}
                                                        options={sortOptions}
                                                        onChange={(e) => setOrderBy(e.target.value)}
                                                    />
                                                </Paper>

                                            </Grid>
                                            <Grid item xs={6}>
                                                <Paper sx={{mb: 1, p: 2}}>
                                                    <CheckboxButtons
                                                        items={vehicleTypeArray ? vehicleTypeArray : []}
                                                        checked={vehicleTypeDesc}
                                                        onChange={handleChangedCheckBoxButtons}

                                                    />
                                                </Paper>

                                            </Grid>


                                        </Grid>

                                    </fieldset>

                                    <div className="k-form-buttons">
                                        <Grid container paddingLeft={2}>
                                            <Grid item xs={2}>
                                                <Button
                                                    variant="contained"
                                                    type={'submit'}
                                                    color="success"
                                                >
                                                    Find
                                                </Button>
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Button onClick={() => onClose()} color="error" variant="contained">
                                                    Cancel
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </div>

                                </FormElement>

                            )}
                        />
                    </Paper>

                </div>
            </div>
        </CSSTransition>,
        document.getElementById("root")!
    );
}
export const VehicleSearchFormMemo = React.memo(VehicleSearchForm)
