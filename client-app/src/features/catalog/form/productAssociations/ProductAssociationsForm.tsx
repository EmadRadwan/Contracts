import React, {useState} from 'react';
import {ProductAssociation} from '../../../../app/models/product/productAssociation';
import {Box, Button, Grid, Paper, Typography} from '@mui/material';
import {
    useAddProductAssociationMutation, useFetchProductAssociationTypesQuery,
    useUpdateProductAssociationMutation
} from '../../../../app/store/configureStore';
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {MemoizedFormDropDownList} from '../../../../app/common/form/MemoizedFormDropDownList';
import {requiredValidator} from '../../../../app/common/form/Validators';
import FormDatePicker from '../../../../app/common/form/FormDatePicker';
import LoadingComponent from '../../../../app/layout/LoadingComponent';
import FormInput from '../../../../app/common/form/FormInput';
import {Product} from '../../../../app/models/product/product';
import FormNumericTextBox from '../../../../app/common/form/FormNumericTextBox';
import {toast} from 'react-toastify';
import CatalogMenu from '../../menu/CatalogMenu';
import {useDeleteProductAssociationMutation} from "../../../../app/store/apis/productAssociationsApi";
import {
    FormMultiColumnComboBoxVirtualAssocsProduct
} from "../../../../app/common/form/FormMultiColumnComboBoxVirtualAssocsProduct";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";

interface Props {
    selectedProduct: Product;
    selectedProductAssociation: ProductAssociation | undefined;
    editMode: number;
    cancelEdit: () => void;
}

// REFACTOR: Explicitly define ProductAssociation interface to match backend DTO and OFBiz ProductAssoc
interface ProductAssociation {
    productId: string;
    productIdTo: string;
    productAssocTypeId: string;
    fromDate: string | null;
    thruDate: string | null;
    reason: string | null;
    quantity: number | null;
    sequenceNum: number | null;
}

export default function ProductAssociationsForm({
                                                    selectedProduct,
                                                    selectedProductAssociation,
                                                    editMode,
                                                    cancelEdit
                                                }: Props) {

    // REFACTOR: Fetch products and association types, handling loading and error states
    const {
        data: assocTypesRaw,
        isFetching: assocTypesFetching,
        error: assocTypesError
    } = useFetchProductAssociationTypesQuery();
    const [isProcessing, setIsProcessing] = useState(false);

    const [addAssociation, {isLoading: addLoading}] = useAddProductAssociationMutation();
    const [updateAssociation, {isLoading: updateLoading}] = useUpdateProductAssociationMutation();
    const [deleteAssociation, {isLoading: deleteLoading}] = useDeleteProductAssociationMutation();
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "order.so.items.adj.form";
    const isFieldDisabled = editMode === 2;


    const assocTypes = assocTypesRaw?.map((type: any) => ({
        productAssocTypeId: type.productAssociationTypeId,
        description: type.description
    })) || [];
    
    const handleSubmitData = async (data: ProductAssociation) => {
        setIsProcessing(true);
        try {
            const payload = {
                ...data,
                productId: selectedProduct.productId,
                productIdTo: data.productIdTo.productId
            };
            if (editMode === 2) {
                await updateAssociation(payload).unwrap();
                toast.success("Association Updated Successfully!");
            } else {
                await addAssociation(payload).unwrap();
                toast.success("Association Created Successfully!");
            }
            cancelEdit();
        } catch (e: any) {
            const errorMsg = e.data?.title || e.data?.errors?.join(', ') || "Operation failed.";
            toast.error(errorMsg);
        } finally {
            setIsProcessing(false);
        }
    };

    const handleDelete = async () => {
        if (!selectedProductAssociation) return;
        setIsProcessing(true);
        try {
            await deleteAssociation({
                productId: selectedProduct.productId,
                productIdTo: selectedProductAssociation.productIdTo,
                productAssocTypeId: selectedProductAssociation.productAssocTypeId,
                fromDate: selectedProductAssociation.fromDate
            }).unwrap();
            toast.success("Association Deleted Successfully!");
            cancelEdit();
        } catch (e: any) {
            const errorMsg = e.data?.title || "Deletion failed.";
            toast.error(errorMsg);
        } finally {
            setIsProcessing(false);
        }
    };

    // REFACTOR: Show loading state for initial data fetching
    if (assocTypesFetching) {
        return <LoadingComponent message="Loading form data..."/>;
    }

    // REFACTOR: Handle fetch errors
    if (assocTypesError) {
        return <Typography color="error">Error loading form data</Typography>;
    }

    return (
        <>
            <CatalogMenu selectedMenuItem="products"/>
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2}>
                    <Grid item xs={8}>
                        <Box display="flex" justifyContent="space-between">
                            <Typography sx={{p: 2}} variant="h3" color={editMode === 2 ? "black" : "green"}>
                                {`${editMode === 2 ? "Edit Association for" : "New Association for"} ${selectedProduct.productName}`}
                            </Typography>
                        </Box>
                    </Grid>
                </Grid>
                <Form
                    initialValues={{
                        productId: selectedProduct.productId,
                        productIdTo: selectedProductAssociation?.productIdTo,
                        productAssocTypeId: editMode === 2 ? selectedProductAssociation?.productAssocTypeId : 'MANUF_COMPONENT',
                        fromDate: editMode === 2 ? selectedProductAssociation?.fromDate : new Date(),
                        thruDate: selectedProductAssociation?.thruDate || null,
                        reason: selectedProductAssociation?.reason || '',
                        quantity: selectedProductAssociation?.quantity || 1,
                        sequenceNum: selectedProductAssociation?.sequenceNum || 0,
                    }}
                    onSubmit={values => handleSubmitData(values as ProductAssociation)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className="k-form-fieldset">
                                <Grid container spacing={2}>
                                    <Grid item container xs={6}>
                                        <Grid item xs={8}>
                                            {/* REFACTOR: Replace FormInput with dropdown for productIdTo, using filtered products */}
                                            <Field
                                                id={"productIdTo"}
                                                name={"productIdTo"}
                                                label={getTranslatedLabel(`${localizationKey}.product`, "Product")}
                                                component={FormMultiColumnComboBoxVirtualAssocsProduct}
                                                autoComplete={"off"}
                                                validator={requiredValidator}
                                                disabled={isFieldDisabled}
                                            />
                                            {/* REFACTOR: Update productAssocTypeId dropdown to show description */}
                                            <Field
                                                id="productAssocTypeId"
                                                name="productAssocTypeId"
                                                label="Product Association Type *"
                                                component={MemoizedFormDropDownList}
                                                data={assocTypes || []}
                                                dataItemKey="productAssocTypeId"
                                                textField="description" // Use description for user-friendly display
                                                validator={requiredValidator}
                                                disabled={isFieldDisabled}
                                            />
                                            <Field
                                                id="quantity"
                                                name="quantity"
                                                label="Quantity *"
                                                component={FormNumericTextBox}
                                                validator={requiredValidator}
                                            />
                                            <Field
                                                id="reason"
                                                name="reason"
                                                label="Reason"
                                                component={FormInput}
                                            />
                                            {/* REFACTOR: Add sequenceNum field to align with OFBiz ProductAssoc */}
                                            <Field
                                                id="sequenceNum"
                                                name="sequenceNum"
                                                label="Sequence Number"
                                                component={FormNumericTextBox}
                                            />
                                        </Grid>
                                    </Grid>
                                    <Grid item container xs={6}>
                                        <Grid item xs={8}>
                                            <Field
                                                id="fromDate"
                                                name="fromDate"
                                                label="From Date *"
                                                component={FormDatePicker}
                                                validator={requiredValidator}
                                                disabled={isFieldDisabled}
                                            />
                                            <Field
                                                id="thruDate"
                                                name="thruDate"
                                                label="To Date"
                                                component={FormDatePicker} // REFACTOR: Use FormDatePicker for consistency
                                            />
                                        </Grid>
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
                                                Submit
                                            </Button>
                                        </Grid>
                                        {/* REFACTOR: Add delete button for edit mode, aligning with OFBiz deleteProductAssoc */}
                                        {editMode === 2 && (
                                            <Grid item xs={1}>
                                                <Button
                                                    variant="contained"
                                                    color="error"
                                                    onClick={handleDelete}
                                                    disabled={isProcessing}
                                                >
                                                    Delete
                                                </Button>
                                            </Grid>
                                        )}
                                        <Grid item xs={1}>
                                            <Button onClick={cancelEdit} color="error" variant="contained">
                                                Cancel
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </div>
                                {isProcessing && (
                                    <LoadingComponent message="Processing Product Association..." />
                                )}
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}