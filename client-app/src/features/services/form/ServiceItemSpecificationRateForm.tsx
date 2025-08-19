import {Field, Form, FormElement} from "@progress/kendo-react-form";
import React from 'react';

import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {QuoteItem} from "../../../app/models/order/quoteItem";
import {requiredValidator} from "../../../app/common/form/Validators";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import {
    useAppDispatch,
    useAppSelector,
    useCreateServiceRateMutation,
    useCreateServiceSpecificationMutation,
    useFetchVehicleMakesQuery,
} from "../../../app/store/configureStore";
import {toast} from "react-toastify";
import {ServiceSpecification} from "../../../app/models/service/serviceSpecification";
import {ServiceRate} from "../../../app/models/service/serviceRate";
import {useFetchVehicleModelsQuery} from "../../../app/store/apis";
import {setIsNewServiceRateAndSpecificationAdded, setIsSelectedPriceZero} from "../../orders/slice/quoteItemsUiSlice";

interface Props {
    onClose: () => void;
}

export default function ServiceItemSpecificationRateForm({
                                                             onClose,
                                                         }: Props) {
    const [
        addServiceSpecification,
        {
            data: addServiceSpecificationResult,
            error: addServiceSpecificationError,
            isLoading: isAddServiceSpecificationLoading,
        },
    ] = useCreateServiceSpecificationMutation();

    const [
        addServiceRate,
        {
            data: addServiceRateResult,
            error: addServiceRateError,
            isLoading: isAddServiceRateLoading,
        },
    ] = useCreateServiceRateMutation();

    const {data: serviceRateMakes} = useFetchVehicleMakesQuery(undefined);

    const {data: serviceRateModels} = useFetchVehicleModelsQuery(undefined);

    const MyForm = React.useRef<any>();
    const vehicle = useAppSelector((state) => state.sharedOrderUi.selectedVehicle);
    const product = useAppSelector((state) => state.sharedOrderUi.selectProductOrService);
    const dispatch = useAppDispatch();

    async function handleSubmitData(data: any) {
        try {
            if (product && vehicle) {
                // get vehicle make and model from serviceRateMakes and serviceRateModels
                // based on vehicle.makeId and vehicle.modelId as vehicle makeId and modelId aare the description not the Id
                // so we need to get the Id from the description
                // then create serviceSpecification and serviceRate
                const makeId = serviceRateMakes?.find(
                    (make) => make.makeDescription === vehicle?.makeDescription,
                )?.makeId;
                const modelId = serviceRateModels?.find(
                    (model) => model.modelDescription === vehicle?.modelDescription,
                )?.modelId;
                const serviceSpecification: ServiceSpecification = {
                    productId: product,
                    makeId: makeId,
                    modelId: modelId,
                    standardTimeInMinutes: data.standardTimeInMinutes,
                    fromDate: new Date(),
                };
                const serviceRate: ServiceRate = {
                    makeId: makeId,
                    modelId: modelId,
                    rate: data.rate,
                    fromDate: new Date(),
                };
                const updatedServiceSpecification =
                    await addServiceSpecification(serviceSpecification).unwrap();
                const createdServiceRate = await addServiceRate(serviceRate).unwrap();
                dispatch(setIsSelectedPriceZero(false));
                dispatch(setIsNewServiceRateAndSpecificationAdded(true));
                toast.success("Service Specification Updated Successfully");
                onClose();
            }
        } catch (error: any) {
            toast.error(error.data.title);
        }
    }

    return (
        <React.Fragment>
            <Form
                ref={MyForm}
                onSubmit={(values) => handleSubmitData(values as QuoteItem)}
                render={(formRenderProps) => (
                    <FormElement>
                        {formRenderProps.visited &&
                            formRenderProps.errors &&
                            formRenderProps.errors.VALIDATION_SUMMARY && (
                                <div className={"k-messagebox k-messagebox-error"}>
                                    {formRenderProps.errors.VALIDATION_SUMMARY}
                                </div>
                            )}
                        <fieldset className={"k-form-fieldset"}>
                            <Grid container spacing={2}>
                                <Grid item xs={12}>
                                    <Field
                                        id={"standardTimeInMinutes"}
                                        format="n0"
                                        min={1}
                                        name={"standardTimeInMinutes"}
                                        label={"Standard Time In Minutes *"}
                                        component={FormNumericTextBox}
                                        validator={requiredValidator}
                                    />
                                </Grid>

                                <Grid item xs={12}>
                                    <Field
                                        id={"rate"}
                                        format="n0"
                                        min={1}
                                        name={"rate"}
                                        label={
                                            "Service Rate - per hour - in Workshop Currency *"
                                        }
                                        component={FormNumericTextBox}
                                        validator={requiredValidator}
                                    />
                                </Grid>
                            </Grid>

                            <div className="k-form-buttons">
                                <Grid container rowSpacing={2}>
                                    <Grid item xs={3}>
                                        <Button
                                            variant="contained"
                                            type={"submit"}
                                            color="success"
                                            disabled={!formRenderProps.allowSubmit}
                                        >
                                            Submit
                                        </Button>
                                    </Grid>
                                    <Grid item xs={2}>
                                        <Button
                                            onClick={() => {
                                                onClose();
                                            }}
                                            variant="contained"
                                            color="error"
                                        >
                                            Cancel
                                        </Button>
                                    </Grid>
                                </Grid>
                            </div>
                        </fieldset>
                    </FormElement>
                )}
            />
        </React.Fragment>
    );
}
