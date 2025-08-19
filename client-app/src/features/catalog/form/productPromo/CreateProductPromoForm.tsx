import Grid from "@mui/material/Grid";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import Button from "@mui/material/Button";
import FormInput from "../../../../app/common/form/FormInput";
import {requiredValidator} from "../../../../app/common/form/Validators";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import {
    inputParameters,
    ProductPromo,
    productPromoActions,
    productPromoConditions
} from "../../../../app/models/product/productPromo";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import {
    FormMultiColumnComboBoxVirtualSalesProduct
} from "../../../../app/common/form/FormMultiColumnComboBoxVirtualSalesProduct";
import {Box, Paper, Typography} from "@mui/material";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";
import CatalogMenu from "../../menu/CatalogMenu";

interface Props {
    productPromo: ProductPromo | undefined
    editMode: number
    cancelEdit: () => void
}


export default function CreateProductPromoForm({productPromo, editMode, cancelEdit}: Props) {

    const {getTranslatedLabel} = useTranslationHelper()

    console.log(productPromo)
    // productPromo.condValue = parseInt(productPromo.condValue)

    const modifiedInputParameters = inputParameters.map(i => {
        return {
            inputParamEnumId: i.ENUM_ID,
            inputParamEnumDescription: i.DESCRIPTION
        }
    })

    const modifiedProductPromoConditions = productPromoConditions.map(c => {
        return {
            operatorEnumId: c.ENUM_ID,
            operatorEnumDescription: c.DESCRIPTION
        }
    })

    const modifiedProductPromoActions = productPromoActions.map(a => {
        return {
            productPromoActionEnumId: a.ENUM_ID,
            productPromoActionEnumDescription: a.DESCRIPTION
        }
    })

    const handleCancelForm = () => {
        cancelEdit()
    }
    const handleSubmit = (data: any) => {
        if (!data.isValid) {
            return false
        }
        console.log(data);
    };

    return (
        <>
      <CatalogMenu />
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container spacing={2}>
                    <Grid item xs={8}>
                        {<Box display='flex' justifyContent='space-between'>
                            <Typography color={productPromo?.productPromoId ? "black" : "green"} sx={{p: 2}}
                                        variant='h4'>
                                {productPromo ? `${getTranslatedLabel("product.promos.form.title", "Promotion")} ${productPromo?.productPromoId}: ${productPromo.promoText}` : getTranslatedLabel("product.promos.form.new", "New Product Promo")}
                            </Typography>
                        </Box>}
                    </Grid>
                </Grid>
                <Form
                    initialValues={productPromo}
                    onSubmitClick={values => handleSubmit(values)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className={'k-form-fieldset'}>
                                <Grid container spacing={4}>
                                    <Grid item container spacing={2}>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'productPromoId'}
                                                name={'productPromoId'}
                                                label={getTranslatedLabel("product.promos.form.id", "Promo ID")}
                                                component={FormInput}
                                                disabled={true}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'promoName'}
                                                name={'promoName'}
                                                label={getTranslatedLabel("product.promos.form.name", "Promo Name")}
                                                component={FormInput}
                                                disabled={true}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'promoText'}
                                                name={'promoText'}
                                                label={getTranslatedLabel("product.promos.form.description", "Promo Description")}
                                                component={FormInput}
                                                validator={requiredValidator}
                                            />
                                        </Grid>


                                        <Grid item xs={4}>
                                            <Field
                                                id={'fromDate'}
                                                name={'fromDate'}
                                                label={getTranslatedLabel("product.promos.form.from", "From Date")}
                                                component={FormDatePicker}
                                                validator={requiredValidator}
                                                disabled={editMode > 1}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'thruDate'}
                                                name={'thruDate'}
                                                label={getTranslatedLabel("product.promos.form.to", "To Date")}
                                                component={FormDatePicker}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'productId'}
                                                name={'productId'}
                                                label={getTranslatedLabel("product.promos.form.product", "Product")}
                                                component={FormMultiColumnComboBoxVirtualSalesProduct}
                                                validator={requiredValidator}
                                                autocomplete={"off"}
                                                disabled={editMode > 1}
                                            />
                                        </Grid>

                                    </Grid>
                                    <Grid item container spacing={2}>

                                        <Grid item xs={4}>
                                            <Field
                                                id={"inputParamEnumId"}
                                                name={"inputParamEnumId"}
                                                label={getTranslatedLabel("product.promos.form.input", "Input Parameter Description")}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey={"inputParamEnumId"}
                                                textField={"inputParamEnumDescription"}
                                                data={modifiedInputParameters}
                                                validator={requiredValidator}
                                                disabled={editMode > 1}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'operatorEnumId'}
                                                name={'operatorEnumId'}
                                                label={getTranslatedLabel("product.promos.form.condition", "Promotion Condition")}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey={"operatorEnumId"}
                                                textField={"operatorEnumDescription"}
                                                data={modifiedProductPromoConditions}
                                                validator={requiredValidator}
                                                disabled={editMode > 1}
                                            />
                                        </Grid>


                                        <Grid item xs={4}>
                                            <Field
                                                id={'productPromoActionEnumId'}
                                                name={'productPromoActionEnumId'}
                                                label={getTranslatedLabel("product.promos.form.action", "Product Promotion Action")}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey={"productPromoActionEnumId"}
                                                textField={"productPromoActionEnumDescription"}
                                                data={modifiedProductPromoActions}
                                                validator={requiredValidator}
                                                disabled={editMode > 1}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'quantity'}
                                                format="n2"
                                                name={'quantity'}
                                                label={getTranslatedLabel("product.promos.form.quantity", "Quantity")}
                                                component={FormNumericTextBox}
                                                validator={requiredValidator}
                                                disabled={editMode > 1}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'amount'}
                                                format="n"
                                                name={'amount'}
                                                label={getTranslatedLabel("product.promos.form.amount", "Amount")}
                                                component={FormNumericTextBox}
                                                validator={requiredValidator}
                                                disabled={editMode > 1}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'condValue'}
                                                format="n"
                                                name={'condValue'}
                                                label={getTranslatedLabel("product.promos.form.value", "Condition value")}
                                                component={FormInput}
                                                validator={requiredValidator}
                                                disabled={editMode > 1}
                                            />
                                        </Grid>

                                    </Grid>


                                </Grid>

                                <div className="k-form-buttons">
                                    <Grid justifyContent={"start"} container spacing={1}>
                                        <Grid item xs={1}>
                                            <Button type={'submit'}
                                                    disabled={!formRenderProps.allowSubmit}
                                                    color="success"
                                                    variant="contained">
                                                {getTranslatedLabel("general.submit", "Submit")}
                                            </Button>
                                        </Grid>
                                        <Grid item xs={1}>
                                            <Button onClick={handleCancelForm} color="error" variant="contained">
                                                {getTranslatedLabel("general.cancel", "Cancel")}
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
    )
}