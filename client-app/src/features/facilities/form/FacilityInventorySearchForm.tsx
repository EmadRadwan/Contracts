import React, {useEffect, useRef} from "react";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {
    useAppSelector,
    useFetchFacilitiesQuery,
    useFetchProductCategoriesQuery,
} from "../../../app/store/configureStore";
import {
    FormMultiColumnComboBoxVirtualFacilityProduct
} from "../../../app/common/form/FormMultiColumnComboBoxVirtualProduct";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import {FacilityInventoryParams} from "../../../app/models/facility/facilityInventory";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import {FormComboBoxVirtualSupplier} from "../../../app/common/form/FormComboBoxVirtualSupplier";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import {Paper} from "@mui/material";
import {Facility} from "../../../app/models/facility/facility";
import {FormDropDownTreeProductCategory} from "../../../app/common/form/FormDropDownTreeProductCategory";

interface Props {
    show: boolean;
    params: FacilityInventoryParams;
    onSubmit: (params: any) => void;
    onClose: () => void;
    width?: number;
    productName?: string
}

export default function FacilityInventorySearchForm({
                                                        onSubmit,
                                                        params,
                                                        width,
                                                        show,
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
    const {data: productCategories} = useFetchProductCategoriesQuery(undefined);
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
        data.values.partyId = undefined
        if (data.values.filteredSupplier) {
            const {fromPartyId} = data.values.filteredSupplier;
            data.values.partyId = fromPartyId;
        }
        data.values.productId = undefined
        if (data.values.filteredProduct) {
            const {productId} = data.values.filteredProduct;
            data.values.productId = productId;
        }

        const {
            pageSize,
            orderBy,
            pageNumber,
            filterFlag,
            filteredProduct,
            filteredSupplier,
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
            pageSize,
            pageNumber,
            orderBy,
            ...rest
        } = formRef.current.values

        const keys = Object.keys(rest)
        keys.forEach((key: string) => {
            formRef.current.onChange(key, {value: undefined})
        })
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
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"qohMinStockDiff"}
                                                        name={"qohMinStockDiff"}
                                                        label={"QOH Minus Minimum Stock less than"}
                                                        min={0}
                                                        component={FormNumericTextBox}
                                                        autoComplete={"off"}

                                                    />
                                                </Grid>
                                                <Grid item xs={6}>
                                                    <Field
                                                        width={"auto"}
                                                        id={"atpMinStockDiff"}
                                                        name={"atpMinStockDiff"}
                                                        label={"ATP Minus Minimum Stock less than"}
                                                        min={0}
                                                        component={FormNumericTextBox}
                                                        autoComplete={"off"}
                                                    />
                                                </Grid>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"filteredSupplier"}
                                                        name={"filteredSupplier"}
                                                        label={"Supplier"}
                                                        component={FormComboBoxVirtualSupplier}
                                                        autoComplete={"off"}

                                                    />
                                                </Grid>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"productCategory"}
                                                        name={"productCategory"}
                                                        label={"Product Category"}
                                                        textField={"text"}
                                                        dataItemKey={"productCategoryId"}
                                                        selectField={"selected"}
                                                        expandField={"expanded"}
                                                        component={FormDropDownTreeProductCategory}
                                                        data={productCategories ? productCategories : []}
                                                        autoComplete={"off"}

                                                    />
                                                </Grid>
                                            </Grid>
                                            <Grid item container columnSpacing={2}>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"soldThrough"}
                                                        name={"soldThrough"}
                                                        label={"Show Products Sold Through"}
                                                        component={FormDatePicker}
                                                        format="{dd/MM/yyyy}"
                                                        position={"relative"}
                                                    />
                                                </Grid>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"fromDateSellThrough"}
                                                        name={"fromDateSellThrough"}
                                                        label={"From Date Sell Through"}
                                                        component={FormDatePicker}
                                                        format="{dd/MM/yyyy}"
                                                    />
                                                </Grid>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"throughDateSellThrough"}
                                                        name={"throughDateSellThrough"}
                                                        label={"Through Date Sell Through"}
                                                        component={FormDatePicker}
                                                        format="{dd/MM/yyyy}"
                                                    />
                                                </Grid>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"monthsInPastLimit"}
                                                        name={"monthsInPastLimit"}
                                                        label={"Months in Past Limit"}
                                                        component={FormNumericTextBox}
                                                        min={0}

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

export const FacilityInventorySearchFormMemo = React.memo(
    FacilityInventorySearchForm,
);
