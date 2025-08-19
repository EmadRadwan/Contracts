import {Box, Button, CircularProgress, Grid, Paper, Typography} from "@mui/material";
import {Field, Form, FormElement, FormRenderProps} from "@progress/kendo-react-form";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import FormInput from "../../../app/common/form/FormInput";
import {requiredValidator} from "../../../app/common/form/Validators";
import {
    useFetchFinishedProductFacilitiesQuery,
    useFetchFinishedProductsForWIPQuery,
    useProductionRunDeclareAndProduceMutation
} from "../../../app/store/apis";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {RootState, useAppDispatch, useAppSelector} from "../../../app/store/configureStore";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import React, {useRef, useState} from "react";
import {toast} from "react-toastify";
import {setInventoryProduced} from "../slice/manufacturingSharedUiSlice";

interface Props {
    productId: string; // WIP template, e.g., "001a2b3c-4d5e-6f7a-8b9c-d0e1f2a3b4c1"
    mainProductionRunId: string; // Main WorkEffort ID
    closeModal: () => void;
}

export const maxValidator = (maxValue: number) => (value: number) => {
    if (value && value > maxValue) {
        return `Quantity cannot exceed ${maxValue}`;
    }
    return undefined;
};

export const minValidator = (minValue: number) => (value: number) => {
    if (value === undefined || value === null || value < minValue) {
        return `Quantity must be at least ${minValue}`;
    }
    return undefined;
};

export default function ProductionRunDeclareAndProduceTopDiscreate({
                                                                       productId,
                                                                       mainProductionRunId,
                                                                       closeModal
                                                                   }: Props) {
    const [declareAndProduce, {isLoading}] = useProductionRunDeclareAndProduceMutation();
    const {getTranslatedLabel} = useTranslationHelper();
    const {language} = useAppSelector((state: RootState) => state.localization);
    const dispatch = useAppDispatch();

    const {data: products} = useFetchFinishedProductsForWIPQuery(productId);
    const {data: facilities} = useFetchFinishedProductFacilitiesQuery(undefined);

    // Use ref to store form state
    const formRef = useRef<FormRenderProps | null>(null);
    // Track selected productId
    const [selectedProductId, setSelectedProductId] = useState<string>("");

    /*// Debounced productId change handler
    const handleProductIdChange = useDebouncedCallback((event: FormFieldChangeEvent) => {
        const newProductId = event.value as string;
        if (newProductId && newProductId !== selectedProductId) {
            setSelectedProductId(newProductId);
        }
    }, 300);
*/
    // Fetch WIP status only when productId is selected
    /*const { data: wipStatus } = useFetchProductionRunWipStatusQuery(
        {
            mainProductionRunId,
            finishedProductId: selectedProductId
        },
        { skip: !selectedProductId }
    );*/


    // Calculate available WIP and max finished product units
    /*const availableWip = useMemo(() => {
        const totalWipCapacity = wipStatus?.totalWipCapacity || 0;
        const consumedWip = wipStatus?.consumedWip || 0;
        return totalWipCapacity - consumedWip;
    }, [wipStatus]);*/

    // Debug products and form values
    /*useEffect(() => {
        console.log("Products:", products);
        console.log("WipStatus:", wipStatus);
        console.log("AvailableWip:", availableWip);
        if (formRef.current) {
            console.log("Form Values:", {
                productId: formRef.current.valueGetter("productId"),
                finishedProductQuantity: formRef.current.valueGetter("finishedProductQuantity"),
                selectedProductId,
                productsAvailable: !!products && products.length > 0
            });
        }
    }, [products, wipStatus, availableWip, selectedProductId]);
*/

    /*const getMaxFinishedProductUnits = (selectedProductId: string) => {
        if (!products || !selectedProductId || availableWip <= 0) return 0;
        const product = products.find(p => p.productId === selectedProductId);
        console.log("Selected Product:", product);
        if (!product || !product.wipPerUnit) {
            console.warn("WIP per unit missing for product:", product?.productId);
            return 0;
        }
        return Math.floor(availableWip / product.wipPerUnit); // Max units
    };

    const getWipConsumed = (finishedProductQuantity: number | undefined, selectedProductId: string) => {
        if (!products || !selectedProductId || finishedProductQuantity === undefined || finishedProductQuantity <= 0) return 0;
        const product = products.find(p => p.productId === selectedProductId);
        if (!product || !product.wipPerUnit) {
            console.warn("WIP per unit missing for product:", product?.productId);
            return 0;
        }
        return finishedProductQuantity * product.wipPerUnit; // WIP consumed in kg
    };
*/
    const handleSubmit = async (data: { values: any }) => {
        // check if there's no validation error
        if (!data.isValid) {
            return false
        }
        const submitData = {
            workEffortId: mainProductionRunId,
            facilityId: data.values.facilityId,
            productId: data.values.productId,
            quantity: data.values.finishedProductQuantity,
            lotId: data.values.lotId,
            inventoryItemTypeId: "NON_SERIAL_INV_ITEM"
        };

        try {
            await declareAndProduce(submitData).unwrap();
            dispatch(setInventoryProduced(true));
            toast.success("Added to inventory Successfully");
            closeModal();
        } catch (err: any) {
            const errorMessage = err?.data?.Error || "Failed to declare and produce";
            toast.error(errorMessage);
            console.error("Failed to declare and produce:", err);
        }
    };

    console.log('productId', productId);

    return (
        <Paper elevation={5} className="div-container-withBorderCurved"
               sx={{maxWidth: 400, margin: "auto", padding: 3}}>
            <Typography variant="h6" align="center" gutterBottom>
                {getTranslatedLabel("manufacturing.jobshop.prodruntasks.declareandproduce.title", "Declare and Produce")}
            </Typography>
            <Form
                initialValues={{
                    workEffortId: mainProductionRunId,
                    finishedProductQuantity: 0,
                    inventoryItemTypeId: "NON_SERIAL_INV_ITEM"
                }}
                onSubmitClick={handleSubmit}
                ref={formRef}
                render={(props) => (
                    <FormElement>
                        <fieldset className="k-form-fieldset">
                            <Grid container spacing={2} alignItems="start" dir={language === "en" ? "ltr" : "rtl"}>
                                <Grid item xs={12}>
                                    <Grid container spacing={2} alignItems="center">

                                        <Grid item xs={12}>
                                            <Field
                                                id="facilityId"
                                                name="facilityId"
                                                label={getTranslatedLabel("manufacturing.jobshop.prodruntasks.declareandproduce.facilityId", "Finished Products Facility *")}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey="facilityId"
                                                textField="facilityName"
                                                data={facilities || []}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={12} display="flex" alignItems="flex-start" direction="column">
                                            <Field
                                                name="finishedProductQuantity"
                                                min={0}
                                                component={FormNumericTextBox}
                                                label={getTranslatedLabel("manufacturing.jobshop.prodruntasks.declareandproduce.quantity", "Finished Product Quantity (Units) *")}
                                                /*validator={[
                                                    requiredValidator,
                                                    minValidator(1),
                                                    maxValidator(getMaxFinishedProductUnits(selectedProductId))
                                                ]}*/
                                                //disabled={!products || products.length === 0 || !wipStatus}
                                            />

                                        </Grid>
                                        <Grid item xs={12}>
                                            <Field
                                                name="lotId"
                                                component={FormInput}
                                                label={getTranslatedLabel("manufacturing.jobshop.prodruntasks.declareandproduce.lotId", "Lot ID")}
                                            />
                                        </Grid>
                                    </Grid>
                                </Grid>
                            </Grid>
                            <Box mt={4}>
                                <Grid container spacing={2}>
                                    <Grid item>
                                        <Button
                                            color="success"
                                            type="submit"
                                            variant="contained"
                                            disabled={!props.allowSubmit || isLoading}
                                        >
                                            {isLoading ? <CircularProgress size={24}/> : "Submit"}
                                        </Button>
                                    </Grid>
                                    <Grid item>
                                        <Button
                                            onClick={closeModal}
                                            color="error"
                                            variant="contained"
                                            disabled={isLoading}
                                        >
                                            Cancel
                                        </Button>
                                    </Grid>
                                </Grid>
                            </Box>
                        </fieldset>
                    </FormElement>
                )}
            />
        </Paper>
    );
}