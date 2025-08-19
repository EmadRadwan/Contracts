import React, {useEffect, useState} from 'react';
import {Box, Button, Grid, Paper, Typography} from '@mui/material';
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {toast} from 'react-toastify';
import {useAddCostComponentCalcMutation, useUpdateCostComponentCalcMutation} from "../../../app/store/apis";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import {requiredValidator} from "../../../app/common/form/Validators";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import FormInput from "../../../app/common/form/FormInput";
import {useAppDispatch, useAppSelector, useFetchCompanyBaseCurrencyQuery} from "../../../app/store/configureStore";
import {currenciesSelectors, fetchCurrenciesAsync} from "../../catalog/slice/currencySlice";

// CostComponentCalc interface
interface CostComponentCalc {
    costComponentCalcId: string | null;
    costGlAccountTypeId: string;
    offsettingGlAccountTypeId: string | null;
    currencyUomId: string;
    costCustomMethodId: string | null;
    description: string;
    fixedCost: number | null;
    variableCost: number | null;
    perMilliSecond: number | null;
}

interface Props {
    selectedCostComponentCalc: CostComponentCalc | undefined;
    editMode: number; // 1 for create, 2 for edit
    cancelEdit: () => void;
}

// EditCostComponentCalc Component
export function EditCostComponentCalc({selectedCostComponentCalc, editMode, cancelEdit}: Props) {
    const [isProcessing, setIsProcessing] = useState(false);
    const [addCostComponentCalc, {isLoading: addLoading}] = useAddCostComponentCalcMutation();
    const [updateCostComponentCalc, { isLoading: updateLoading }] = useUpdateCostComponentCalcMutation();
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = 'manufacturing.costs.form';
    const {currenciesLoaded} = useAppSelector(state => state.currency);
    const currencies = useAppSelector(currenciesSelectors.selectAll);
    const {data: baseCurrency, isLoading: isBaseCurrencyLoading} = useFetchCompanyBaseCurrencyQuery(undefined);
    const dispatch = useAppDispatch();


    useEffect(() => {
        if (!currenciesLoaded) {
            dispatch(fetchCurrenciesAsync());
        }
    }, [currenciesLoaded, dispatch]);


    const initialValues = {
        costComponentCalcId: selectedCostComponentCalc?.costComponentCalcId || null,
        currencyUomId: editMode === 2 ? selectedCostComponentCalc?.currencyUomId : baseCurrency?.currencyUomId || 'EGY',
        description: selectedCostComponentCalc?.description || '',
        fixedCost: selectedCostComponentCalc?.fixedCost || null,
        variableCost: selectedCostComponentCalc?.variableCost || null,
        perMilliSecond: selectedCostComponentCalc?.perMilliSecond || null
    };

    console.log('baseCurrency?.currencyUomId', baseCurrency?.currencyUomId)
    const handleSubmitData = async (data: CostComponentCalc) => {
        setIsProcessing(true);
        try {
            const payload = {...data};
            if (editMode === 2) {
                await updateCostComponentCalc(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.successUpdate`, 'Cost Component Calc Updated Successfully!'));
            } else {
                await addCostComponentCalc(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.successCreate`, 'Cost Component Calc Created Successfully!'));
            }
            cancelEdit();
        } catch (e: any) {
            const errorMsg = e.data?.error || e.data?.title || (e.data?.errors ? e.data.errors.join(', ') : e.message) || 'Operation failed.';
            console.error('Submission error:', {error: e, payload});
            toast.error(errorMsg);
        } finally {
            setIsProcessing(false);
        }
    };

    const titleText = editMode === 2 && selectedCostComponentCalc?.costComponentCalcId
        ? getTranslatedLabel(`${localizationKey}.editTitle`, `Edit Cost Component Calc (${selectedCostComponentCalc.costComponentCalcId})`)
        : getTranslatedLabel(`${localizationKey}.createTitle`, 'New Cost Component Calc');

    if (isBaseCurrencyLoading || !currenciesLoaded) {
        return <LoadingComponent message={getTranslatedLabel('general.loading', '')}/>;
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
                onSubmit={(values) => handleSubmitData(values as CostComponentCalc)}
                render={(formRenderProps) => (
                    <FormElement>
                        <fieldset className="k-form-fieldset">
                            <Grid container spacing={2}>
                                {/* REFACTOR: Row 1 - costComponentCalcId (editable in add mode) and description */}
                                <Grid item xs={6}>
                                    {editMode === 2 ? (
                                        <Field
                                            name="costComponentCalcId"
                                            component={FormInput}
                                            type="hidden"
                                            value={selectedCostComponentCalc?.costComponentCalcId}
                                        />
                                    ) : (
                                        <Field
                                            id="costComponentCalcId"
                                            name="costComponentCalcId"
                                            label={getTranslatedLabel(`${localizationKey}.costComponentCalcId`, 'Cost Component Calc ID *')}
                                            component={FormInput}
                                            validator={requiredValidator}
                                        />
                                    )}
                                </Grid>
                                <Grid item xs={6}>
                                    <Field
                                        id="description"
                                        name="description"
                                        label={getTranslatedLabel(`${localizationKey}.description`, 'Description')}
                                        component={FormInput}
                                    />
                                </Grid>
                                {/* REFACTOR: Row 2 - fixedCost and variableCost */}
                                <Grid item xs={6}>
                                    <Field
                                        id="fixedCost"
                                        name="fixedCost"
                                        label={getTranslatedLabel(`${localizationKey}.fixedCost`, 'Fixed Cost')}
                                        component={FormNumericTextBox}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    <Field
                                        id="variableCost"
                                        name="variableCost"
                                        label={getTranslatedLabel(`${localizationKey}.variableCost`, 'Variable Cost')}
                                        component={FormNumericTextBox}
                                    />
                                </Grid>
                                {/* REFACTOR: Row 3 - perMilliSecond and currencyUomId (currencyUomId in half row) */}
                                <Grid item xs={6}>
                                    <Field
                                        id="perMilliSecond"
                                        name="perMilliSecond"
                                        label={getTranslatedLabel(`${localizationKey}.perMilliSecond`, 'Per Milli Second')}
                                        component={FormNumericTextBox}
                                    />
                                </Grid>
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
                            </Grid>
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
                                <LoadingComponent
                                    message={getTranslatedLabel(`${localizationKey}.processing`, 'Processing Cost Component Calc...')}
                                />
                            )}
                        </fieldset>
                    </FormElement>
                )}
            />
        </Paper>
    );
}