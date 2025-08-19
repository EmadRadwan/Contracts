import React, {useEffect, useRef, useState} from "react";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {useAppDispatch, useAppSelector, useFetchFacilitiesQuery,} from "../../../app/store/configureStore";
import {
    FormMultiColumnComboBoxVirtualFacilityProduct
} from "../../../app/common/form/FormMultiColumnComboBoxVirtualProduct";
import {FacilityInventoryItemParams} from "../../../app/models/facility/facilityInventory";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import {Paper} from "@mui/material";
import {Facility} from "../../../app/models/facility/facility";

import {selectProductById, setSelectedProductName} from "../slice/facilityInventoryUiSlice";

interface Props {
    show: boolean;
    params: FacilityInventoryItemParams;
    onSubmit: (params: any) => void;
    onClose: () => void;
    width?: number;
    mode?: string
    productName?: string
}

export default function FacilityInventoryItemsSearchForm({
                                                             onSubmit,
                                                             params,
                                                             width,
                                                             show,
                                                             mode,
                                                             onClose,
                                                             productName
                                                         }: Props) {
    const {data: facilities} = useFetchFacilitiesQuery(undefined);
    const {
        selectedFacilityId,
        selectedProductId,
        selectedProductName
    } = useAppSelector((state: any) => state.facilityInventoryUi);
    const formRef = useRef<any>()
    const [formKey, setFormKey] = useState(0)
    const dispatch = useAppDispatch()
    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };

    let facilitiesList: (Facility | { facilityId: string; facilityName: string; })[];
    if (facilities) {
        facilitiesList = [...facilities, {facilityId: "", facilityName: "All"}]
    }

    // console.log(facilities!)
    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, []);

    async function handleSubmitData(data: any) {

        data.values.productId = undefined
        if (data.values.filteredProduct) {
            const {productId, productName} = data.values.filteredProduct;
            data.values.productId = productId;
            dispatch(selectProductById(productId))
            dispatch(setSelectedProductName(productName))

        }

        const {
            pageSize,
            orderBy,
            pageNumber,
            filterFlag,
            filteredProduct,
            ...rest
        } = data.values

        if (Object.values(rest).some((value: any) => value)) {
            data.values.filterFlag = true
        }

        onSubmit(data.values);
        onClose();
    }

    const onClearForm = () => {
        const {
            pageNumber,
            pageSize,
            orderBy,
            ...rest
        } = formRef.current.values

        const keys = Object.keys(rest)
        keys.forEach((key: string) => {
            formRef.current.onChange(key, {value: undefined})
        })
        dispatch(selectProductById(undefined))
        dispatch(setSelectedProductName(""))
        formRef.current.onSubmit()
    }

    return ReactDOM.createPortal(
        <CSSTransition in={show} unmountOnExit timeout={{enter: 0, exit: 300}}>
            <div className="modal">
                <div
                    className="modal-content"
                    style={{width}}
                    onClick={(e) => e.stopPropagation()}
                >
                    <Paper elevation={5} className={`div-container-withBorderCurved`}>
                        <Form
                            initialValues={params}
                            ref={formRef}
                            onSubmitClick={(values) => handleSubmitData(values)}
                            render={(formRenderProps) => (
                                <FormElement>
                                    <fieldset className={"k-form-fieldset"}>
                                        <Grid container spacing={2}>
                                            <Grid item container columnSpacing={2}>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"facilityId"}
                                                        name={"facilityId"}
                                                        label={"Facility"}
                                                        component={MemoizedFormDropDownList}
                                                        autoComplete={"off"}
                                                        data={facilitiesList ? facilitiesList : []}
                                                        dataItemKey={"facilityId"}
                                                        textField={"facilityName"}

                                                    />
                                                </Grid>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"filteredProduct"}
                                                        name={"filteredProduct"}
                                                        label={"Product"}
                                                        component={
                                                            FormMultiColumnComboBoxVirtualFacilityProduct
                                                        }
                                                        autoComplete={"off"}
                                                    />
                                                </Grid>

                                            </Grid>
                                            <Grid item container columnSpacing={2}>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"dateReceivedFrom"}
                                                        name={"dateReceivedFrom"}
                                                        label={"DateTime Received From"}
                                                        component={FormDatePicker}
                                                        format="{dd/MM/yyyy}"
                                                    />
                                                </Grid>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"dateReceivedTo"}
                                                        name={"dateReceivedTo"}
                                                        label={"DateTime Received To"}
                                                        component={FormDatePicker}
                                                        format="{dd/MM/yyyy}"
                                                    />
                                                </Grid>
                                            </Grid>
                                        </Grid>

                                        <div className="k-form-buttons">
                                            <Grid container p={1}>
                                                <Grid item xs={3} paddingLeft={8}>
                                                    <Button
                                                        variant="contained"
                                                        type={"submit"}
                                                        color="success"
                                                    >
                                                        Find
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={4}>
                                                    <Button
                                                        onClick={() => onClose()}
                                                        color="error"
                                                        variant="contained"
                                                    >
                                                        Cancel
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={4}>
                                                    <Button
                                                        onClick={() => onClearForm()}
                                                        color="warning"
                                                        variant="contained"
                                                    >
                                                        Clear
                                                    </Button>
                                                </Grid>
                                            </Grid>
                                        </div>
                                    </fieldset>
                                </FormElement>
                            )}
                        />
                    </Paper>
                </div>
            </div>
        </CSSTransition>,
        document.getElementById("root")!,
    );
}

export const FacilityInventoryItemsSearchFormMemo = React.memo(
    FacilityInventoryItemsSearchForm
);
