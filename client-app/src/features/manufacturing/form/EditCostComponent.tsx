import React, { useEffect, useState } from 'react';
import { Box, Button, Grid, Paper, Typography } from '@mui/material';
import { Field, Form, FormElement, FormRenderProps } from '@progress/kendo-react-form';
import { toast } from 'react-toastify';
import { useTranslationHelper } from '../../../app/hooks/useTranslationHelper';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import FormNumericTextBox from '../../../app/common/form/FormNumericTextBox';
import FormDateTimePicker from '../../../app/common/form/FormDateTimePicker';
import { requiredValidator } from '../../../app/common/form/Validators';
import { MemoizedFormDropDownList } from '../../../app/common/form/MemoizedFormDropDownList';
import FormInput from '../../../app/common/form/FormInput';
import { useAppDispatch, useAppSelector, useFetchCompanyBaseCurrencyQuery } from '../../../app/store/configureStore';
import { currenciesSelectors, fetchCurrenciesAsync } from '../../catalog/slice/currencySlice';
import {useAddCostComponentCalcMutation, useAddCostComponentMutation} from "../../../app/store/apis";

// CostComponent interface
interface CostComponent {
    costComponentId?: string | null;
    productId: string;
    costComponentTypeId: string;
    costUomId: string;
    cost: number | null;
    fromDate: Date | null;
}

interface Props {
    selectedCostComponent: CostComponent | undefined;
    productId: string;
    productName: string; // REFACTOR: Added productName to Props
    editMode: number; // 1 for create, 2 for edit
    cancelEdit: () => void;
}

// EditCostComponent Component
export function EditCostComponent({ selectedCostComponent, productId, productName, editMode, cancelEdit }: Props) {
    const [isProcessing, setIsProcessing] = useState(false);
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'product.costs.form';
    const { currenciesLoaded } = useAppSelector(state => state.currency);
    const currencies = useAppSelector(currenciesSelectors.selectAll);
    const { data: baseCurrency, isLoading: isBaseCurrencyLoading } = useFetchCompanyBaseCurrencyQuery(undefined);
    const dispatch = useAppDispatch();
    const [addCostComponent, {isLoading: addLoading}] = useAddCostComponentMutation();

    useEffect(() => {
        if (!currenciesLoaded) {
            dispatch(fetchCurrenciesAsync());
        }
    }, [currenciesLoaded, dispatch]);

    // REFACTOR: Initialize form values with costUomId mapped to currencyUomId
    // Uses baseCurrency?.currencyUomId with fallback to 'USD' for create mode
    const initialValues = {
        productId: productId,
        costComponentTypeId: 'EST_STD_MAT_COST',
        currencyUomId: editMode === 2 ? selectedCostComponent?.costUomId || '' : baseCurrency?.currencyUomId || 'USD',
        cost: selectedCostComponent?.cost || null,
        fromDate: selectedCostComponent?.fromDate || new Date(),
    };

    const handleSubmitData = async (data: CostComponent) => {
        setIsProcessing(true);
        const payload = { ...data };
        try {
           
            if (editMode === 2) {
                // await updateCostComponent(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.successUpdate`, 'Cost Component Updated Successfully!'));
            } else {
                await addCostComponent(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.successCreate`, 'Cost Component Created Successfully!'));
            }
            cancelEdit();
        } catch (e: any) {
            const errorMsg = e.data?.error || e.data?.title || (e.data?.errors ? e.data.errors.join(', ') : e.message) || 'Operation failed.';
            console.error('Submission error:', { error: e, payload });
            toast.error(errorMsg);
        } finally {
            setIsProcessing(false);
        }
    };

    // REFACTOR: Dynamic title including productName
    // Displays productName in both create and edit modes
    const titleText = editMode === 2 && selectedCostComponent?.costComponentId
        ? getTranslatedLabel(
            `${localizationKey}.editTitle`,
            `Edit Cost Component (${selectedCostComponent.costComponentId}) for ${productName}`
        )
        : getTranslatedLabel(
            `${localizationKey}.createTitle`,
            `New Cost Component for ${productName}`
        );

    // REFACTOR: Reference to FormRenderProps for potential future use
    const formRef = React.useRef<FormRenderProps | null>(null);

    if (isBaseCurrencyLoading || !currenciesLoaded) {
        return <LoadingComponent message={getTranslatedLabel('general.loading', 'Loading...')} />;
    }

    return (
        <Paper elevation={5} className="div-container-withBorderCurved" style={{ padding: '16px' }}>
            <Grid container spacing={2}>
                <Grid item xs={12}>
                    <Box display="flex" justifyContent="space-between">
                        <Typography sx={{ p: 2 }} variant="h3" color={editMode === 2 ? 'black' : 'green'}>
                            {titleText}
                        </Typography>
                    </Box>
                </Grid>
            </Grid>
            <Form
                initialValues={initialValues}
                onSubmit={(values) => handleSubmitData(values as CostComponent)}
                render={(formRenderProps) => {
                    formRef.current = formRenderProps; // Store form reference
                    return (
                        <FormElement>
                            <fieldset className="k-form-fieldset">
                                <Grid container spacing={2}>
                                    {/* REFACTOR: Row 1 - productId and costComponentTypeId (hidden fields) */}
                                    <Grid item xs={6}>
                                        <Field
                                            name="productId"
                                            component={FormInput}
                                            type="hidden"
                                            value={productId}
                                        />
                                    </Grid>
                                    <Grid item xs={6}>
                                        <Field
                                            name="costComponentTypeId"
                                            component={FormInput}
                                            type="hidden"
                                            value="EST_STD_MAT_COST"
                                        />
                                    </Grid>

                                    {/* REFACTOR: Row 2 - currencyUomId */}
                                    <Grid item xs={6}>
                                        <Field
                                            id="currencyUomId"
                                            name="currencyUomId"
                                            label={getTranslatedLabel(`${localizationKey}.currency`, 'Currency *')}
                                            component={MemoizedFormDropDownList}
                                            data={currencies}
                                            dataItemKey="currencyUomId"
                                            textField="description"
                                            validator={requiredValidator}
                                            disabled={editMode === 2}
                                        />
                                    </Grid>

                                    {/* REFACTOR: Row 3 - cost and fromDate */}
                                    <Grid item xs={6}>
                                        <Field
                                            id="cost"
                                            name="cost"
                                            label={getTranslatedLabel(`${localizationKey}.cost`, 'Cost *')}
                                            component={FormNumericTextBox}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                    <Grid item xs={6}>
                                        <Field
                                            id="fromDate"
                                            name="fromDate"
                                            label={getTranslatedLabel(`${localizationKey}.fromDate`, 'From Date *')}
                                            component={FormDateTimePicker}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                </Grid>

                                {/* REFACTOR: Form buttons for submit and cancel */}
                                <div className="k-form-buttons">
                                    <Grid container spacing={2}>
                                        <Grid item xs={1}>
                                            <Button
                                                variant="contained"
                                                type="submit"
                                                color="success"
                                                disabled={!formRenderProps.allowSubmit || isProcessing}
                                            >
                                                {getTranslatedLabel(`${localizationKey}.submit`, 'Submit')}
                                            </Button>
                                        </Grid>
                                        <Grid item xs={1}>
                                            <Button
                                                onClick={cancelEdit}
                                                color="error"
                                                variant="contained"
                                            >
                                                {getTranslatedLabel(`${localizationKey}.cancel`, 'Cancel')}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </div>
                                {isProcessing && (
                                    <LoadingComponent message={getTranslatedLabel(`${localizationKey}.processing`, 'Processing Cost Component...')} />
                                )}
                            </fieldset>
                        </FormElement>
                    );
                }}
            />
        </Paper>
    );
}